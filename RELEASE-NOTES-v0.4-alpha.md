# REvoat v0.4-alpha

Two things for people running REvoat as a website: the User Agreement and
Privacy Policy can now be edited from the web UI without touching source code,
and upgrading to a new release is one command that cannot silently lose your
data.

## New: owner-editable User Agreement & Privacy Policy

Previously, changing either legal page meant editing Razor source and running
`docker compose up -d --build`. They now live in the database and are edited
in place.

**How to use it**

1. Set `VOAT_SITE_OWNERS` in `.env` to the account(s) allowed to edit.
   Several can be comma-separated: `VOAT_SITE_OWNERS=admin,otheruser`.
   Unset, it defaults to `admin`.
2. Log in as one of those accounts and open `/help/useragreement` or
   `/help/privacy`.
3. Click **✎ Edit** at the top of the page. The body becomes a markdown
   editor pre-filled with the current text.
4. Click **Save**. The page reloads with the new text.

**Details worth knowing**

- **Markdown** is the format. Raw HTML is escaped — the same `SafeMode` the
  comment renderer uses. Use `#` / `##` for headings; those are the levels the
  page styling expects.
- Use the literal text **`{siteName}`** anywhere you want the site's name. It
  is substituted on render, so renaming `siteName` in config updates both pages.
- The owner list is **usernames, not roles**. A GlobalAdmin who is not listed
  still cannot edit.
- Edits are stored in the database and survive rebuilds and restarts. A fresh
  install with no stored edits renders the built-in defaults, identical to
  before.
- Only these two pages are editable this way. The footer copyright line is the
  `footerText` setting and still needs a rebuild.

**Security**

- Access is a config list of usernames, checked **again server-side** on save.
  The Edit button is display only; a hand-crafted POST from a non-owner gets a
  403. Unknown page slugs also get 403.
- Saves require a valid login, a valid anti-forgery token, and pass a spam
  guard. Markdown is capped at 30 000 characters.
- Markdown goes through the existing `SafeMode` renderer, so `<script>` in a
  saved page renders as escaped text and does not execute. `javascript:` links
  are neutralized.
- **Default owner is `admin`**, and that account's default password is printed
  in INSTALL.md. Change it before the site is reachable by anyone else.

## Other changes

- **User agreement rules text** updated: new explicit bullets under "Do Not
  Incite Harm" and "Protect Kids", and revised contact addresses and dates.
- **Footer copyright line** rewritten to disclaim affiliation with the original
  project, across the site footer and the four standalone offline pages.
- **Roles and polls** (carried from v0.3-alpha's tail): `enableRoles` is on and
  `enableVotes` is on, so poll submissions are no longer admin-only.

## New: upgrade to a new release without losing data

If you run REvoat as a website, this is the part that matters. Previously,
upgrading meant `git pull && docker compose up -d --build` with nothing backed
up and no way back.

```bash
./scripts/upgrade.sh
```

It takes a full backup first, brings in the newest release, rebuilds, applies
any new database schema, checks the site answers, and prints how to roll back
if it did not go well. `--ref v0.5-alpha` targets a specific release.

Also included:

- `./scripts/backup.sh` — database, uploaded files, cookie keys, TLS
  certificates and your `.env`, into one timestamped folder with checksums.
  Safe to run as often as you like.
- `./scripts/restore.sh backups/<timestamp> --yes` — puts everything back
  exactly as it was. Verifies the checksums before touching anything and
  refuses a backup that never finished writing.
- `UPGRADING.md` — the full guide, including moving to a new machine.

**The one thing that can still lose your data:** do not unpack a new release
into a *new, separate* folder. Docker Compose names its data volumes after the
folder you run it from, so a fresh folder is a brand-new, empty install — your
data is untouched but invisible, which looks exactly like a wipe. Unpack over
the existing directory, or just run `scripts/upgrade.sh`, which finds your
existing data and uses it regardless of what the folder is called. Never run
`docker compose down -v`; that deletes the volumes outright.

There are no manual database migration steps. The schema scripts are additive
and idempotent, so re-running them never overwrites content you have changed.

## Upgrading

```bash
./scripts/upgrade.sh
```

See [UPGRADING.md](UPGRADING.md) — particularly the first two sections — before
your first upgrade.

No manual database step. The `SitePage` table is created automatically on
startup if it does not already exist (idempotent, PostgreSQL and SQL Server).
Existing databases are left untouched otherwise.

To add or change who can edit:

```bash
# .env
VOAT_SITE_OWNERS=admin,otheruser
```

then `docker compose up -d app`.

To confirm the table landed:

```bash
docker compose exec db psql -U voat -d voat -c '\d "SitePage"'
```

## Notes

- There is **no preview button** yet. Save/Cancel is what you get; the existing
  preview endpoint renders with different settings and is duplicated in the
  minified JS bundle.
- The DMCA contact in the User Agreement still reads `copyright@voat.co`, left
  verbatim deliberately — change it from the new editor if you want a different
  address.

See `CHANGELOG.md` for the full technical record of this change.
