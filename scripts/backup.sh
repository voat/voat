#!/usr/bin/env bash
#
# Back up a running REvoat install — everything that would hurt to lose.
#
#     ./scripts/backup.sh [output-dir]      # default: ./backups
#
# Produces backups/<YYYYmmdd-HHMMSS>/ containing:
#
#   database.sql.gz       portable pg_dump (restores on another box / PG version)
#   storage.tar.gz        exact copy of the avatars & thumbnails volume
#   dataprotection.tar.gz exact copy of the cookie/antiforgery key ring
#   caddy-data.tar.gz     exact copy of the Let's Encrypt certs
#   .env                  your configuration, copied byte-for-byte
#   manifest.txt          what this backup is of, and when
#
# Why BOTH a dump and volume tarballs: the dump is portable, the tarballs are a
# faithful "put it all back exactly as it was" on this machine. Backups are
# cheap; not being able to roll back is not.
#
# The .env copy contains your passwords and signing key. Treat the backup
# directory as a secret — chmod 700 it and do not leave it world-readable.

source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/lib.sh"

usage() {
    cat <<'EOF'
Usage: scripts/backup.sh [output-dir]

Backs up the database, uploaded files, cookie keys, TLS certs and .env.
Default output directory is ./backups.
EOF
}

case "${1:-}" in
    -h|--help) usage; exit 0 ;;
esac

OUT_BASE="${1:-$REPO_ROOT/backups}"

need_docker
detect_project

[ -f "$REPO_ROOT/.env" ] \
    || warn "no .env in $REPO_ROOT — the backup will not include your configuration."

DEST="$OUT_BASE/$(timestamp)"
mkdir -p "$DEST"
chmod 700 "$OUT_BASE" 2>/dev/null || true

step "Backing up Compose project '$PROJECT' to $DEST"

# ---- database ----------------------------------------------------------
step "Database (pg_dump)"
PGUSER="$(pg_user)"
PGDB="$(pg_db)"
dc exec -T db pg_dump -U "$PGUSER" -d "$PGDB" --clean --if-exists \
    | gzip > "$DEST/database.sql.gz" \
    || die "pg_dump failed. Is the database container running? (docker compose -p $PROJECT ps)"

[ -s "$DEST/database.sql.gz" ] || die "database.sql.gz came out empty — refusing to call that a backup."
info "database.sql.gz ($(du -h "$DEST/database.sql.gz" | cut -f1))"

# ---- volumes -----------------------------------------------------------
for vol in $VOLUMES_WITH_DATA; do
    step "Volume ${PROJECT}_${vol}"
    if docker volume inspect "${PROJECT}_${vol}" >/dev/null 2>&1; then
        copy_volume_to "$vol" "$DEST/${vol}.tar.gz"
        info "${vol}.tar.gz ($(du -h "$DEST/${vol}.tar.gz" | cut -f1))"
    else
        warn "volume ${PROJECT}_${vol} does not exist — skipped."
    fi
done

# ---- configuration -----------------------------------------------------
step "Configuration"
if [ -f "$REPO_ROOT/.env" ]; then
    cp "$REPO_ROOT/.env" "$DEST/.env"
    chmod 600 "$DEST/.env" 2>/dev/null || true
    info ".env copied (contains secrets — keep this directory private)"
else
    info ".env not present — skipped"
fi

# ---- manifest ----------------------------------------------------------
# Written LAST: its presence is what marks the backup complete, so restore.sh
# refuses a directory without one (i.e. a run that was killed part-way).
{
    echo "backup_format=1"
    echo "created=$(date -u '+%Y-%m-%dT%H:%M:%SZ')"
    echo "host=$(hostname 2>/dev/null || echo unknown)"
    echo "project_name=$PROJECT"
    echo "compose_file=$COMPOSE_FILE"
    echo "code_version=$(git_describe)"
    echo "docker=$(docker --version 2>/dev/null | head -1)"
    echo "compose=$(docker compose version 2>/dev/null | head -1)"
    echo "database=$PGDB"
    echo
    echo "# checksums (sha256  filename) — restore.sh verifies these before touching anything"
    for f in "$DEST"/*.gz "$DEST"/.env; do
        [ -f "$f" ] && sha256_of "$f"
    done
    echo
    echo "# contents"
    echo "#   database.sql.gz       full pg_dump --clean --if-exists of database '$PGDB'"
    echo "#   storage.tar.gz        user avatars and link thumbnails"
    echo "#   dataprotection.tar.gz cookie / antiforgery keys (losing these logs everyone out)"
    echo "#   caddy-data.tar.gz     Let's Encrypt account and certificates"
    echo "#   .env                  site configuration and secrets"
    echo "#"
    echo "# The database is a logical dump rather than a tar of the db-data volume:"
    echo "# a tar of a running PostgreSQL is not crash-consistent. The dump is also"
    echo "# portable across machines and PostgreSQL major versions."
    echo
    echo "# restore with:"
    echo "#   ./scripts/restore.sh $DEST"
} > "$DEST/manifest.txt"

echo
info "Backup complete: ${C_BLD}$DEST${C_OFF}"
echo
echo "  Roll back to this point with:"
echo "      ./scripts/restore.sh $DEST"
echo
echo "  This directory contains your passwords and signing key."
echo "  Keep it private (chmod 700) or encrypt it before copying it off this machine."
