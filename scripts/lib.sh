#!/usr/bin/env bash
#
# Shared helpers for the REvoat operator scripts:
#   scripts/backup.sh, scripts/upgrade.sh, scripts/restore.sh
#
# Sourced, never executed on its own. Everything here exists to make one thing
# true: every docker compose call targets the project that ALREADY HOLDS YOUR
# DATA, never a fresh one this directory would otherwise invent.

# Git Bash on Windows rewrites container paths ("/src", "/backup") into
# "C:/Program Files/Git/src" before docker ever sees them. No-op on Linux.
export MSYS_NO_PATHCONV=1

set -euo pipefail

# ---- output ------------------------------------------------------------
if [ -t 1 ]; then
    C_RED=$'\033[31m'; C_YEL=$'\033[33m'; C_GRN=$'\033[32m'
    C_BLD=$'\033[1m'; C_OFF=$'\033[0m'
else
    C_RED=''; C_YEL=''; C_GRN=''; C_BLD=''; C_OFF=''
fi

info() { printf '%s\n' "${C_GRN}==>${C_OFF} $*"; }
step() { printf '%s\n' "${C_BLD}---${C_OFF} $*"; }
warn() { printf '%s\n' "${C_YEL}warning:${C_OFF} $*" >&2; }
die()  { printf '%s\n' "${C_RED}error:${C_OFF} $*" >&2; exit 1; }

# ---- layout ------------------------------------------------------------
# Resolve the repo root from this file's location so the scripts work when
# invoked from anywhere (cron, a subshell, a different cwd).
LIB_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$LIB_DIR/.." && pwd)"
COMPOSE_FILE="$REPO_ROOT/docker-compose.yml"

# ---- prerequisites -----------------------------------------------------
need_docker() {
    command -v docker >/dev/null 2>&1 \
        || die "docker is not installed or not on PATH."
    docker info >/dev/null 2>&1 \
        || die "docker is installed but the daemon is not reachable (try: sudo usermod -aG docker \$USER)."
    docker compose version >/dev/null 2>&1 \
        || die "docker compose v2 is required ('docker compose version' failed)."
    [ -f "$COMPOSE_FILE" ] \
        || die "no docker-compose.yml at $COMPOSE_FILE — run this from a REvoat checkout."
}

# The name this directory would give a Compose project if nothing existed yet.
# Compose derives it from the checkout folder name, lowercased.
natural_project() {
    basename "$REPO_ROOT" | tr '[:upper:]' '[:lower:]'
}

# ---- project detection -------------------------------------------------
# Find the Compose project that already owns this install's data.
#
# Detection is by DATA, not by folder name, because that is what must not be
# lost. A project is considered "ours" when it has both a *_db-data volume
# (the database) and a *_storage volume (avatars/thumbnails) — the two the
# compose file always creates together.
#
# Sets: PROJECT
detect_project() {
    local candidates=() v prefix
    while read -r v; do
        [ -z "$v" ] && continue
        prefix="${v%_db-data}"
        if docker volume inspect "${prefix}_storage" >/dev/null 2>&1; then
            candidates+=("$prefix")
        fi
    done < <(docker volume ls -q 2>/dev/null | grep -E '_db-data$' || true)

    local natural
    natural="$(natural_project)"

    if [ "${#candidates[@]}" -eq 0 ]; then
        PROJECT="$natural"
        return 0
    fi

    if [ "${#candidates[@]}" -eq 1 ]; then
        PROJECT="${candidates[0]}"
    else
        # Several installs on this box. Prefer the one matching this folder.
        PROJECT=""
        for v in "${candidates[@]}"; do
            [ "$v" = "$natural" ] && PROJECT="$v"
        done
        if [ -z "$PROJECT" ]; then
            die "found several REvoat installs and cannot tell which is which:
$(printf '  %s\n' "${candidates[@]}")
Re-run from the checkout that owns the one you mean."
        fi
    fi

    if [ "$PROJECT" != "$natural" ]; then
        warn "This folder would normally start Compose project '$natural', but your
       data lives in project '$PROJECT'. Preserving '$PROJECT'.
       (Running it as '$natural' would start a NEW EMPTY site and your
       existing data would be untouched but invisible — it would look
       like everything had been lost.)"
    fi
}

# ---- compose wrapper ---------------------------------------------------
# Every compose call goes through this so the project name is never accidental.
dc() {
    [ -n "${PROJECT:-}" ] || die "internal error: detect_project must run first."
    docker compose -p "$PROJECT" -f "$COMPOSE_FILE" "$@"
}

# ---- database identity -------------------------------------------------
# Read these from the running container rather than parsing .env, so no script
# ever has to handle a secret it does not need.
pg_user() {
    local u
    u="$(dc exec -T db printenv POSTGRES_USER 2>/dev/null | tr -d '\r' || true)"
    printf '%s' "${u:-voat}"
}

pg_db() {
    local d
    d="$(dc exec -T db printenv POSTGRES_DB 2>/dev/null | tr -d '\r' || true)"
    printf '%s' "${d:-voat}"
}

# ---- volumes -----------------------------------------------------------
# Volumes that hold state worth copying. caddy-config is excluded: it is
# Caddy's runtime scratch and nothing in it is worth archiving.
VOLUMES_WITH_DATA="storage dataprotection caddy-data"

# Volume tarballs travel over stdin/stdout, never through a host bind mount.
# The only path Docker ever sees is a volume name, which cannot be mangled —
# and on Git Bash, MSYS rewrites anything that looks like a POSIX path into
# "C:/Program Files/Git/..." before docker reads it. A .partial name plus mv
# also means a killed run cannot leave a truncated tar that looks complete.

copy_volume_to() {  # copy_volume_to <volume-name> <dest.tar.gz>
    local vol="$1" dest="$2"

    docker run --rm -v "${PROJECT}_${vol}:/src:ro" alpine tar czf - -C /src . \
        > "${dest}.partial" \
        || { rm -f "${dest}.partial"; die "failed to archive the '${vol}' volume."; }
    mv "${dest}.partial" "$dest"
}

copy_volume_from() {  # copy_volume_from <volume-name> <src.tar.gz>
    local vol="$1" src="$2"
    [ -f "$src" ] || die "missing $src"

    docker volume inspect "${PROJECT}_${vol}" >/dev/null 2>&1 \
        || docker volume create "${PROJECT}_${vol}" >/dev/null

    # Clearing the target first matters: a plain tar x leaves stale files
    # behind, which a rollback would silently keep.
    docker run --rm -i -v "${PROJECT}_${vol}:/dst" alpine \
        sh -c 'find /dst -mindepth 1 -delete 2>/dev/null; tar xzf - -C /dst' \
        < "$src" \
        || die "failed to restore the '${vol}' volume."
}

# ---- integrity ---------------------------------------------------------
sha256_of() {  # sha256_of <file>  ->  "hash  basename"
    local f="$1"
    if command -v sha256sum >/dev/null 2>&1; then
        sha256sum "$f" | awk '{print $1"  "$2}' | sed "s|  .*/|  |"
    elif command -v shasum >/dev/null 2>&1; then
        shasum -a 256 "$f" | awk '{print $1"  "$2}' | sed "s|  .*/|  |"
    else
        echo "  $(basename "$f")"
    fi
}

# ---- misc --------------------------------------------------------------
timestamp() { date +%Y%m%d-%H%M%S; }

# git -C <msys-path> fails on Git Bash (Windows git wants "C:\..."), so run git
# from inside the directory instead. Works identically on Linux.
git_at() { ( cd "$REPO_ROOT" && git "$@" ); }

has_git() {
    [ -d "$REPO_ROOT/.git" ] && command -v git >/dev/null 2>&1
}

# --match 'v*' keeps feature tags (avatar-upload-fix, ...) out of the version
# string; this repo carries several.
git_describe() {
    if has_git; then
        git_at describe --tags --match 'v*' --always --dirty 2>/dev/null || echo "unknown"
    else
        echo "no git (release archive install)"
    fi
}
