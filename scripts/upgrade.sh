#!/usr/bin/env bash
#
# Upgrade a running REvoat install to a new release — without losing data.
#
#     ./scripts/upgrade.sh                      # to the newest release tag
#     ./scripts/upgrade.sh --ref v0.5-alpha     # to a specific release
#
# What it does, in order:
#
#   1. refuses to run against a fresh or absent install
#   2. takes a full backup (skip with --no-backup, at your own risk)
#   3. brings in the new code
#   4. rebuilds and restarts the containers
#   5. applies any new database schema (additive and idempotent)
#   6. checks the site actually answers
#   7. tells you how to roll back if it did not go well
#
# It never creates a second, empty install. If this folder does not own the
# data, it stops and says so instead of quietly starting a blank site.

source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/lib.sh"

BACKUP_DIR=""
BACKUP_DONE=0

on_error() {
    printf '\n'
    warn "The upgrade did not complete."
    if [ "$BACKUP_DONE" = 1 ] && [ -n "$BACKUP_DIR" ]; then
        echo "  Nothing has been rolled back automatically."
        echo "  Your pre-upgrade backup is at: $BACKUP_DIR"
        echo
        echo "  To put things back the way they were:"
        echo "      ./scripts/restore.sh $BACKUP_DIR --yes"
    else
        echo "  No backup was taken (it was skipped or never reached)."
    fi
    echo
    echo "  If the containers are in a bad state:"
    echo "      docker compose -p ${PROJECT:-<project>} ps"
    echo "      docker compose -p ${PROJECT:-<project>} logs --tail=50 app"
}
trap on_error ERR

usage() {
    cat <<'EOF'
Usage: scripts/upgrade.sh [options]

  --ref <tag>   Upgrade to a specific git tag, branch or commit.
                Default: the newest release tag.
  --no-backup   Skip the automatic backup. Not recommended.
  --yes         Do not prompt; assume yes (for cron / unattended use).
  -h, --help    This text.
EOF
}

TARGET_REF=""
DO_BACKUP=1
ASSUME_YES=0

while [ $# -gt 0 ]; do
    case "$1" in
        --ref)        TARGET_REF="${2:-}"; shift 2 || die "--ref needs a value" ;;
        --no-backup)  DO_BACKUP=0; shift ;;
        --yes|-y)     ASSUME_YES=1; shift ;;
        -h|--help)    usage; exit 0 ;;
        *)            usage >&2; die "unknown option: $1" ;;
    esac
done

need_docker

# ---- guards: is there anything to upgrade? ------------------------------
[ -f "$REPO_ROOT/.env" ] || die "no .env in $REPO_ROOT.
That means this is not a configured install yet. Follow INSTALL.md first;
this script only upgrades an existing site."

detect_project

[ -n "$(docker volume ls -q | grep -E "_db-data$" || true)" ] || die "no REvoat data volumes exist on this machine.
There is nothing to upgrade — this would be a first install. See INSTALL.md."

dc ps >/dev/null 2>&1 || die "cannot talk to Compose project '$PROJECT'."

BACKUP_DIR="$REPO_ROOT/backups/$(timestamp)"

# ---- 1. backup ---------------------------------------------------------
if [ "$DO_BACKUP" = 1 ]; then
    step "Taking a backup before touching anything"
    "$LIB_DIR/backup.sh" "$REPO_ROOT/backups"
    # backup.sh writes to backups/<timestamp>; pick up the one it just made
    BACKUP_DIR="$(ls -1dt "$REPO_ROOT"/backups/*/ 2>/dev/null | head -1)"
    BACKUP_DIR="${BACKUP_DIR%/}"
    BACKUP_DONE=1
    [ -n "$BACKUP_DIR" ] && info "Backup at: $BACKUP_DIR"
else
    warn "Skipping the backup (--no-backup). If this goes wrong there is nothing to roll back to."
fi

# ---- 2. new code -------------------------------------------------------
step "Fetching the new code"

if has_git; then

    if [ -n "$(git_at status --porcelain 2>/dev/null)" ] && [ "$ASSUME_YES" != 1 ]; then
        die "the working tree has uncommitted changes.
Commit, stash or remove them first — upgrading would overwrite them.
Re-run with --yes to upgrade anyway."
    fi

    git_at fetch --tags --quiet origin || die "git fetch failed (is 'origin' reachable?)."

    if [ -z "$TARGET_REF" ]; then
        TARGET_REF="$(git_at tag -l 'v*' --sort=-v:refname | head -1)"
        [ -n "$TARGET_REF" ] || die "found no v* release tags to upgrade to.
Pass one explicitly: ./scripts/upgrade.sh --ref <tag>"
        info "Newest release tag is $TARGET_REF"
    fi

    info "Checking out $TARGET_REF"
    git_at checkout --quiet --detach "$TARGET_REF" \
        || die "could not check out '$TARGET_REF' — does that tag/branch exist?"

    info "Now at: $(git_describe)"

else
    # Release-archive install: no git to fetch with. The operator unpacks.
    cat <<EOF

${C_YEL}This install has no git repository (a release archive rather than a clone).${C_OFF}
Unpack the new release OVER THIS DIRECTORY, never beside it:

    cd $(dirname "$REPO_ROOT")
    tar --strip-components=1 -xzf /path/to/REvoat-<version>.tar.gz -C $(basename "$REPO_ROOT")

${C_BLD}Unpacking into a new, separate folder is the one thing that loses your data.${C_OFF}
Docker Compose names its volumes after the folder, so a new folder is a brand
new empty install — your site would come up blank even though nothing was
deleted. Overwrite this directory instead; your .env and your volumes survive.

Then re-run this script with --no-backup (the backup was already taken).
EOF
    [ "$ASSUME_YES" = 1 ] && die "cannot continue without the new code in place."
    exit 1
fi

# ---- 3. rebuild --------------------------------------------------------
step "Rebuilding and restarting (this can take a few minutes)"
dc up -d --build || die "docker compose up failed."

# ---- 4. schema ---------------------------------------------------------
# setup.sql is additive and idempotent: CREATE ... IF NOT EXISTS everywhere,
# seed rows guarded with WHERE NOT EXISTS. Re-running it never overwrites
# operator data. docker/db-init only runs on a FIRST boot, so this step is
# what applies schema changes on an existing database.
step "Applying database schema"
PGUSER="$(pg_user)"
PGDB="$(pg_db)"
dc exec -T db psql -U "$PGUSER" -d "$PGDB" -v ON_ERROR_STOP=1 -f /voat-sql/setup.sql \
    || die "schema step failed."
info "Schema is up to date."

# ---- 5. smoke check ----------------------------------------------------
step "Checking the site answers"
URL="http://127.0.0.1:8080/"
ok=0
for _ in $(seq 1 60); do
    code="$(curl -s -o /dev/null -w '%{http_code}' --max-time 3 "$URL" 2>/dev/null || echo 000)"
    case "$code" in
        2*|3*) ok=1; break ;;
    esac
    sleep 2
done

if [ "$ok" = 1 ]; then
    info "Site is answering ($code) at $URL"
else
    warn "The site did not answer at $URL within 120 seconds."
    echo "  Container state:"
    dc ps
    echo
    echo "  Recent app logs:"
    dc logs --tail=30 app
    die "the upgrade finished but the app does not look healthy.
Your backup is at: $BACKUP_DIR"
fi

# ---- done --------------------------------------------------------------
printf '\n%s\n' "${C_GRN}${C_BLD}Upgrade complete.${C_OFF}"
echo
echo "  Version now running: $(git_describe)"
echo "  Backup taken at:     ${BACKUP_DIR:-none}"
echo
if [ -n "$BACKUP_DIR" ]; then
    echo "  If something is wrong, roll back with:"
    echo "      ./scripts/restore.sh $BACKUP_DIR --yes"
    echo
fi
echo "  See UPGRADING.md for what to check and how to move to a new machine."
