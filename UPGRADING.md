# Upgrading REvoat

How to move a running site to a new release **without losing anything**, and
how to put it back if an upgrade goes wrong.

You need shell access to the machine running the site and permission to run
`docker compose` on it.

---

## Read this first: the one thing that loses your data

**Do not extract a new release into a new, separate folder and run it from
there.**

Docker Compose names its data volumes after the folder you run it from. If you
unpack the new release next to the old one and start it, Compose creates a
brand-new set of empty volumes and you get what looks like a brand-new, empty
site. Your data is not deleted — it is simply invisible to the new folder —
but the effect is the same until you notice.

Do this instead:

- **If you installed with git:** run `./scripts/upgrade.sh`. It handles
  everything.
- **If you installed from a release archive:** unpack the new release **over
  the existing directory**, then run `./scripts/upgrade.sh`. Your `.env` is not
  in the release archive and is not overwritten by extracting over it.

Also: **never run `docker compose down -v`**. The `-v` flag deletes the data
volumes — the database, the uploads and the TLS certificates — and there is no
undo.

---

## Where your data lives

Everything worth keeping is in Docker's named volumes and in one file:

| What | Where it lives | If you lose it |
|---|---|---|
| Users, posts, comments, votes, edited page content | the `db-data` volume | **Everything.** This is the site. |
| Avatars and link thumbnails | the `storage` volume | Uploads gone; accounts and posts stay |
| Login cookies | the `dataprotection` volume | Everyone is logged out. They just log in again — nothing is lost |
| HTTPS certificates | the `caddy-data` volume | Certificates are re-issued automatically; may hit Let's Encrypt rate limits |
| Your configuration and secrets | `.env` in the install directory | You must re-enter the passwords and signing key |

The database is backed up as a SQL dump rather than as a copy of the volume,
because copying a live database's files is not safe. A dump also restores onto
a different machine or a different PostgreSQL version.

---

## Upgrading

```bash
./scripts/upgrade.sh
```

Run it from the directory your site is installed in. In order it:

1. refuses to run if it cannot find your existing data, rather than starting a
   fresh site
2. takes a full backup into `backups/<timestamp>/`
3. fetches and checks out the newest release
4. rebuilds and restarts the containers
5. applies any new database schema
6. checks the site answers
7. prints how to roll back if it did not go well

Useful options:

```bash
./scripts/upgrade.sh --ref v0.5-alpha   # a specific release instead of the newest
./scripts/upgrade.sh --no-backup        # skip the backup (not recommended)
./scripts/upgrade.sh --yes              # do not prompt; for unattended use
```

If you run it twice, nothing breaks — the second run just backs up again and
rebuilds.

### Upgrading from a version that has no `scripts/` folder

If you are on a release from before these scripts existed, the directory you
are upgrading *from* has no `scripts/` in it. Either way works:

**Option A — copy the scripts into your install.** Unpack the new release into
a temporary folder, copy its `scripts/` directory into your existing install,
then run `./scripts/upgrade.sh` from your install as usual.

**Option B — work from the new folder.** Unpack the new release into a new
folder and run `bash scripts/upgrade.sh` from there. This is safe *for the
scripts themselves* — they find your existing data and use it — but you must
copy `.env` from the old folder into the new one first. It is never included
in a release archive.

---

## Backing up

```bash
./scripts/backup.sh
```

Creates `backups/<timestamp>/` containing the database dump, the uploads, the
cookie keys, the TLS certificates and a copy of `.env`, with checksums recorded
in `manifest.txt`. Run it before any upgrade (the upgrade script does this for
you) and on a schedule:

```bash
# crontab -e   — 03:17 every night
17 3 * * * cd /path/to/your/install && ./scripts/backup.sh >> /var/log/revoat-backup.log 2>&1
```

> **Backups contain your secrets.** The `.env` copy holds the database
> password and the signing key. Keep the `backups/` directory private
> (`chmod 700`) and encrypt it before copying it anywhere off this machine.

---

## Rolling back

If an upgrade did not work out:

```bash
./scripts/restore.sh backups/<timestamp> --yes
```

This puts the database, the uploads, the cookie keys and the certificates back
exactly as they were. It verifies the backup's checksums before it touches
anything, and it will not restore from an incomplete backup.

Notes:

- **You may need to roll the code back too.** If the new version has a bug, the
  data restore alone will not fix it. Git installs can also do:
  ```bash
  git checkout v0.4-alpha && ./scripts/upgrade.sh --no-backup
  ```
- **Everyone may need to log in again** if the cookie keys changed.
- **Sometimes the data is fine and only the app is broken.** If the upgrade
  failed during the build, nothing changed — the old site is still running and
  there is nothing to restore.

To restore just the database (for example when moving machines):

```bash
./scripts/restore.sh backups/<timestamp> --db-only --yes
```

---

## Moving to a new machine

A backup directory is self-contained, so the move is straightforward.

1. On the old machine: `./scripts/backup.sh`
2. Copy the whole `backups/<timestamp>/` directory to the new machine
   (it contains secrets — use `scp`, not email or a shared drive).
3. On the new machine, get the code and follow [INSTALL.md](INSTALL.md) up to
   and including `cp .env.example .env`. **Then overwrite `.env` with the copy
   from your backup** — that is what carries your passwords and signing key.
4. Start once so the volumes exist: `docker compose up -d`
5. `./scripts/restore.sh backups/<timestamp> --yes`
6. Point DNS at the new machine.

The folder name on the new machine does not have to match the old one. The
backup records which install it came from and the scripts use that.

---

## When a release needs database changes

You do not have to do anything by hand.

There are no manual migration steps. The database schema scripts are
**additive and idempotent** — they only ever add what is missing and never
overwrite or delete content you have created or edited. The upgrade script runs
them for you on every upgrade, and the application applies its own small
schema fixes at startup. Re-running them is always safe.

---

## Troubleshooting

**`$'\r': command not found`**
The scripts have Windows line endings. This happens on a Windows checkout made
without the repository's `.gitattributes` being honoured. Fix it with
`sed -i 's/\r$//' scripts/*.sh`, or re-checkout on the Linux machine.

**"no REvoat data volumes exist on this machine"**
There is genuinely nothing to upgrade — this is a first install. Follow
[INSTALL.md](INSTALL.md) first.

**"found several REvoat installs and cannot tell which is which"**
More than one install's data is on this machine and the scripts will not guess.
Re-run from the directory belonging to the one you mean, or check what exists
with `docker volume ls` and `docker compose ls -a`.

**The site does not answer after an upgrade**
Do not panic and do not run `down -v`. Check the app's log:

```bash
docker compose logs --tail=50 app
```

If it is not fixable from there, roll back to the backup the upgrade took —
its path is printed in the upgrade output and in `backups/`.

**Disk full during a backup**
Delete old folders under `backups/`. Keep at least one, ideally two.

---

## See also

- [INSTALL.md](INSTALL.md) — first-time installation
- `CHANGELOG.md` — what changed in each release
