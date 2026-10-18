#!/usr/bin/env bash
#
# Restore a REvoat install from a backup made by scripts/backup.sh.
#
#     ./scripts/restore.sh backups/20261016-120000 --yes
#     ./scripts/restore.sh backups/20261016-120000 --db-only --yes
#
# Default restores EVERYTHING: the database plus the uploaded files, the
# cookie keys and the TLS certificates. That is what you want after a bad
# upgrade — put it all back exactly as it was.
#
# --db-only restores just the database. Use that when moving to a new machine,
# or when you only kept the .sql.gz and not the volume tarballs.
#
# This DESTROYS the current data and replaces it. That is the point, and it is
# why --yes is required.

source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/lib.sh"

usage() {
    cat <<'EOF'
Usage: scripts/restore.sh <backup-dir> [--db-only] [--yes]

  --db-only   Restore only the database (skip uploads, cookie keys, certs).
  --yes       Required. Confirms you mean to overwrite the current data.
  -h, --help  This text.
EOF
}

DB_ONLY=0
ASSUME_YES=0
BACKUP=""

while [ $# -gt 0 ]; do
    case "$1" in
        --db-only)  DB_ONLY=1; shift ;;
        --yes|-y)   ASSUME_YES=1; shift ;;
        -h|--help)  usage; exit 0 ;;
        -*)         usage >&2; die "unknown option: $1" ;;
        *)          BACKUP="$1"; shift ;;
    esac
done

[ -n "$BACKUP" ] || { usage >&2; die "a backup directory is required."; }
BACKUP="${BACKUP%/}"
[ -d "$BACKUP" ] || die "no such backup directory: $BACKUP"
[ -f "$BACKUP/manifest.txt" ] || die "$BACKUP has no manifest.txt.
The manifest is written last, so its absence means the backup never finished.
Do not restore from it — the files may be truncated."
[ -f "$BACKUP/database.sql.gz" ] || die "$BACKUP has no database.sql.gz — that is not a backup made by scripts/backup.sh."

# ---- verify integrity before touching anything -------------------------
step "Verifying the backup"
if grep -qE '^[0-9a-f]{64}  ' "$BACKUP/manifest.txt"; then
    while read -r hash name; do
        [ -z "${name:-}" ] && continue
        [ -f "$BACKUP/$name" ] || die "manifest lists '$name' but it is missing from $BACKUP.
The backup is incomplete — do not restore from it."
        actual="$(sha256_of "$BACKUP/$name" | awk '{print $1}')"
        [ "$actual" = "$hash" ] || die "checksum mismatch for '$name'.
The backup is damaged — do not restore from it."
    done < <(grep -E '^[0-9a-f]{64}  ' "$BACKUP/manifest.txt")
    info "All checksums match."
else
    warn "no checksums in this backup (older format) — restoring without verification."
fi

need_docker
detect_project

# ---- confirm -----------------------------------------------------------
echo
echo "${C_BLD}About to overwrite the current REvoat install.${C_OFF}"
echo
echo "  Compose project:   $PROJECT"
echo "  Restoring from:    $BACKUP"
if [ -f "$BACKUP/manifest.txt" ]; then
    echo "  Backup taken at:   $(sed -n 's/^created=//p' "$BACKUP/manifest.txt" | head -1)"
    echo "  Backup version:    $(sed -n 's/^code_version=//p' "$BACKUP/manifest.txt" | head -1)"
fi
echo
echo "  Will be replaced:"
echo "      database        (from database.sql.gz)"
if [ "$DB_ONLY" = 0 ]; then
    for vol in $VOLUMES_WITH_DATA; do
        [ -f "$BACKUP/${vol}.tar.gz" ] && echo "      volume ${PROJECT}_${vol}   (from ${vol}.tar.gz)"
    done
else
    echo "      (uploads, cookie keys and TLS certs left alone --db-only)"
fi
echo
echo "  This cannot be undone from here. Keep $BACKUP until you are sure."
echo

if [ "$ASSUME_YES" != 1 ]; then
    printf 'Type %srestore%s to continue: ' "$C_BLD" "$C_OFF"
    read -r answer
    [ "$answer" = "restore" ] || die "aborted. Nothing was changed."
fi

# ---- stop writers ------------------------------------------------------
step "Stopping the site so nothing writes during the restore"
dc stop app caddy >/dev/null 2>&1 || true

# ---- database ----------------------------------------------------------
step "Restoring the database"
PGUSER="$(pg_user)"
PGDB="$(pg_db)"

# Drop and recreate so nothing from the current (possibly newer) schema
# survives the rollback. The dump is right beside us, so a failed attempt can
# simply be re-run.
dc exec -T db psql -U "$PGUSER" -d postgres -v ON_ERROR_STOP=1 \
    -c "DROP DATABASE IF EXISTS \"${PGDB}\" WITH (FORCE)" >/dev/null \
    || die "could not drop database '$PGDB'."
dc exec -T db psql -U "$PGUSER" -d postgres -v ON_ERROR_STOP=1 \
    -c "CREATE DATABASE \"${PGDB}\" OWNER \"${PGUSER}\"" >/dev/null \
    || die "could not recreate database '$PGDB'."

gunzip -c "$BACKUP/database.sql.gz" \
    | dc exec -T db psql -U "$PGUSER" -d "$PGDB" -v ON_ERROR_STOP=1 --quiet \
    || die "restoring the database dump failed.
The backup at $BACKUP is still intact — fix the problem and re-run."

info "Database '$PGDB' restored."

# ---- volumes -----------------------------------------------------------
if [ "$DB_ONLY" = 0 ]; then
    for vol in $VOLUMES_WITH_DATA; do
        tar_file="$BACKUP/${vol}.tar.gz"
        [ -f "$tar_file" ] || { warn "no ${vol}.tar.gz in this backup — skipped."; continue; }
        step "Restoring volume ${PROJECT}_${vol}"
        copy_volume_from "$vol" "$tar_file"
        info "${vol} restored."
    done
else
    info "Skipping file volumes (--db-only)."
fi

# ---- configuration -----------------------------------------------------
# .env is never overwritten: it holds the live signing key and passwords, and
# silently reverting it would log everyone out or break the database login.
if [ -f "$REPO_ROOT/.env" ]; then
    info "Left the existing .env alone (never overwritten)."
elif [ -f "$BACKUP/.env" ]; then
    step "No .env present — restoring it from the backup"
    cp "$BACKUP/.env" "$REPO_ROOT/.env"
    chmod 600 "$REPO_ROOT/.env" 2>/dev/null || true
    info ".env restored."
else
    warn "no .env found in the backup and none in $REPO_ROOT."
fi

# ---- restart -----------------------------------------------------------
step "Starting the site"
dc up -d || die "failed to start the containers after restore."

echo
printf '%s\n' "${C_GRN}${C_BLD}Restore complete.${C_OFF}"
echo
echo "  Check the site at your domain, or locally on http://127.0.0.1:8080/"
echo "  Everyone may need to log in again if the cookie keys were replaced."
echo
echo "  Keep $BACKUP until you have confirmed the restore looks right."
