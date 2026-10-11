# Installing Voat

Voat is a Reddit-style community platform (ASP.NET Core on .NET 10, PostgreSQL).
This guide assumes basic command-line skills and nothing more.

There are three ways to run it:

1. **[Local with Docker](#1-local-with-docker)** — easiest; one command starts everything.
2. **[Local without Docker](#2-local-without-docker)** — for development on a machine with the .NET 10 SDK.
3. **[VPS / public server](#3-vps--public-server)** — production install with automatic HTTPS.

Jump to [Configuration reference](#configuration-reference), [Default accounts](#default-accounts),
or [Troubleshooting](#troubleshooting) if you hit a snag.

---

## What you need

| Way | Requirements |
|---|---|
| Local with Docker | [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows/Mac) or Docker Engine + Compose (Linux) |
| Local without Docker | [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), PostgreSQL 13+ (18 recommended) |
| VPS | A Linux VPS (Ubuntu 24.04 works well), Docker + Docker Compose, a domain name pointing at the server |

Everything else (Caddy web server, PostgreSQL in Docker) is set up for you.

---

## 1. Local with Docker

Open a terminal (Windows: **Git Bash** or PowerShell) in the project folder:

```bash
# 1. Create your local configuration
cp .env.example .env
```

Now edit `.env` in any text editor and set three values:

```ini
POSTGRES_PASSWORD=pick-a-long-random-password
VOAT_SIGNING_KEY=pick-a-long-random-key
VOAT_DOMAIN=localhost
```

Quick way to generate random values (Git Bash):

```bash
openssl rand -hex 24    # use as POSTGRES_PASSWORD
openssl rand -hex 32    # use as VOAT_SIGNING_KEY
```

PowerShell alternative:

```powershell
[guid]::NewGuid().ToString("N") + [guid]::NewGuid().ToString("N")
```

Start everything (first run builds the app image and creates the database — allow a few minutes):

```bash
docker compose up -d --build
```

That is the one command. It starts:

* **db** — PostgreSQL, schema and default content created automatically on first boot
* **app** — the Voat web application
* **caddy** — web server in front of the app, with automatic HTTPS

Open **<http://localhost:8080>** — the app's own port, no certificate warnings.
(The site is also reachable via Caddy at <https://localhost> using a local
self-signed certificate; your browser will warn about it locally, which is
normal.)

Log in with the default admin account (see [Default accounts](#default-accounts))
and **change the password immediately**.

Useful commands:

```bash
docker compose logs -f app     # watch the app's log
docker compose down            # stop everything (data is kept)
docker compose down -v         # stop and DELETE all data (fresh start)
```

---

## 2. Local without Docker

Use this for day-to-day development.

### 2a. Database

Install PostgreSQL, then create a database and user (as an admin user, e.g. `postgres`):

```sql
CREATE USER voat WITH PASSWORD 'your-local-password';
CREATE DATABASE voat OWNER voat;
```

Load the schema + default content (schema → identity → functions → base data → seed):

```bash
psql -h localhost -U voat -d voat -f misc/SqlScripts/PostgreSql/setup.sql
```

(`setup.sql` includes every other script in the right order and is safe to re-run.)

### 2b. Configuration

Copy the local template over the settings file:

```bash
cp src/Voat.UI/~appsettings.json src/Voat.UI/appsettings.json
```

Then edit `src/Voat.UI/appsettings.json` and replace the three `{server}` / `{database}` /
`{userid}` / `{password}` placeholders in the connection strings with your values
(all three can point at the same database).

**Secrets belong in the environment, not in files.** Alternatively (or additionally)
set them as environment variables before starting the app — environment values
always win over `appsettings.json`:

```bash
# Git Bash / Linux
export voat__settings__signingKey="your-signing-key"
export voat__data__connections__0__value="Server=localhost;port=5432;Database=voat;User ID=voat;Password=your-local-password;Persist Security Info=True;Keepalive=1;"
export voat__data__connections__1__value="$voat__data__connections__0__value"
export voat__data__connections__2__value="$voat__data__connections__0__value"
```

```powershell
# PowerShell
$env:voat__settings__signingKey = "your-signing-key"
$env:voat__data__connections__0__value = "Server=localhost;port=5432;Database=voat;User ID=voat;Password=your-local-password;Persist Security Info=True;Keepalive=1;"
```

(The `__` double underscore is the .NET configuration separator: `voat__settings__signingKey`
maps to `voat:settings:signingKey`.)

### 2c. Run

```bash
dotnet run --project src/Voat.UI
```

Open **<http://localhost:5000>** (or the URL the console prints).

### 2d. Tests (optional)

```bash
dotnet test
```

The test suite provisions its own database (`voat_test`) from the same SQL
scripts. Copy `test/Voat.Tests/~appsettings.json` to
`test/Voat.Tests/appsettings.json` and point it at your PostgreSQL first
(that file is gitignored because it carries credentials).

---

## 3. VPS / public server

Example assumes Ubuntu 24.04 and a domain `example.com` whose **DNS A record
already points at the server**.

### 3a. Install Docker

```bash
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker $USER   # then log out and back in
```

### 3b. Get the code and configure

```bash
git clone https://github.com/YOUR/voat.git voat
cd voat
cp .env.example .env
nano .env                     # or any editor
```

In `.env` set at minimum:

```ini
POSTGRES_PASSWORD=<long random password>
VOAT_SIGNING_KEY=<long random key>
VOAT_DOMAIN=example.com
VOAT_ACME_EMAIL=you@example.com
VOAT_FORCE_HTTPS=true
```

Generate the random values with `openssl rand -hex 24` / `openssl rand -hex 32`.

### 3c. Start

```bash
docker compose up -d --build
```

Caddy obtains an HTTPS certificate automatically (Let's Encrypt) and renews it.
Visit **https://example.com**, log in as admin, **change the admin password**,
and create your own account.

### 3d. Firewall

Allow only web traffic and SSH:

```bash
sudo ufw allow OpenSSH
sudo ufw allow 80
sudo ufw allow 443
sudo ufw enable
```

### 3e. Backups

Two things matter:

```bash
# 1. Database (run nightly via cron, for example)
docker compose exec -T db pg_dump -U voat voat | gzip > backup-$(date +%F).sql.gz

# 2. Uploaded files + cookie keys (copy the named volumes)
docker run --rm -v voat-core_storage:/s -v $PWD:/backup alpine tar czf /backup/storage-$(date +%F).tar.gz -C /s .
```

### 3f. Updates

```bash
git pull
docker compose up -d --build
docker compose exec db psql -U voat -d voat -f /voat-sql/setup.sql   # apply any new SQL
```

(`setup.sql` is idempotent — safe to run repeatedly.)

---

## Configuration reference

All secrets and environment-specific values come from the environment
(`.env` for Docker; exported variables or `appsettings.json` for bare-metal).
`appsettings.json` in the repository contains **no secrets**.

| `.env` key | Meaning | Default |
|---|---|---|
| `POSTGRES_USER` / `POSTGRES_DB` | Database user / name | `voat` |
| `POSTGRES_PASSWORD` | Database password (**required**) | — |
| `VOAT_SIGNING_KEY` | Key for internal signatures/IP hashing (**required**) | — |
| `VOAT_DOMAIN` | Public hostname (`localhost` for local) | `localhost` |
| `VOAT_ACME_EMAIL` | Let's Encrypt contact email | empty |
| `VOAT_FORCE_HTTPS` | HSTS + secure cookies | `false` |
| `VOAT_COOKIE_DOMAIN` | Cookie scope (`.example.com` for subdomains) | empty |
| `VOAT_SMTP_HOST` / `VOAT_SMTP_PORT` | Outgoing mail relay | `127.0.0.1` / `0` |
| `VOAT_USE_SENDGRID` / `VOAT_SENDGRID_API_KEY` | Use SendGrid for mail | `false` |
| `VOAT_CAPTCHA_ENABLED` + `VOAT_RECAPTCHA_*` | reCAPTCHA on registration | disabled |
| `VOAT_API_PUBLIC_KEY` / `VOAT_API_PRIVATE_KEY` | Legacy API keys (API not ported) | empty |
| `VOAT_STORAGE_CONNECTION_STRING` | Azure blob storage (optional) | empty |

Every setting in `src/Voat.UI/appsettings.json` can be overridden by an
environment variable using `__` for nesting, e.g.
`voat__settings__siteName` or `voat__data__connections__0__value`.

---

## Default accounts

The database seed creates:

| Username | Password | Role |
|---|---|---|
| `admin` | `ChangeMe123!` | GlobalAdmin |

**Change this password immediately** after first login (Settings → Change password).
Anyone who finds your site can otherwise take it over.

---

## Troubleshooting

**`docker compose up` fails with "set POSTGRES_PASSWORD in .env"**
You started without a `.env` file (or left the value empty). Run
`cp .env.example .env`, fill in the required values, then start again.

**The site shows a database error on first run**
The database initializes only when the data volume is empty and healthy.
Check `docker compose logs db` and `docker compose logs app`. If the volume was
created before the scripts existed, apply them manually:
`docker compose exec db psql -U voat -d voat -f /voat-sql/setup.sql`

**Port already in use (`8080`, `80`, `443`, `5432`)**
Another program (often IIS, Skype, or a local PostgreSQL) owns the port. Either
stop that program or edit the port mapping in `docker-compose.yml`
(e.g. change `"127.0.0.1:8080:8080"` to `"127.0.0.1:8090:8080"`).

**Browser certificate warning on https://localhost**
Normal locally: Caddy uses a self-signed certificate for `localhost`. Use
<http://localhost:8080> instead, or add Caddy's local root CA to your trust store.
On a real domain this never happens (Let's Encrypt certificates are trusted).

**Caddy cannot get a certificate (VPS)**
The domain's DNS A record must point at the server *before* the first start,
ports 80/443 must be open in the firewall and reachable from the internet.
Check `docker compose logs caddy`.

**Login cookies don't stick / you are logged out on restart**
The cookie-signing keys live in the `dataprotection` Docker volume. If you
removed volumes (`docker compose down -v`), old cookies simply become invalid —
log in again. If it happens without `down -v`, check that the volume exists
(`docker volume ls`).

**Email never arrives**
Out of the box the SMTP handler points at `127.0.0.1` inside the app container,
where nothing listens. Configure a real relay (`VOAT_SMTP_HOST`) or SendGrid in
`.env`. Registration works without mail, but verification emails will not send.

**I forgot the admin password**
Create a fresh account (registration is open), then grant it admin from the
database:

```bash
docker compose exec db psql -U voat -d voat
```

```sql
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT u."Id", r."Id" FROM "AspNetUsers" u, "AspNetRoles" r
WHERE u."UserName" = 'yourname' AND r."NormalizedName" = 'GLOBALADMIN'
  AND NOT EXISTS (SELECT 1 FROM "AspNetUserRoles" x WHERE x."UserId" = u."Id" AND x."RoleId" = r."Id");
```

**Where are uploads stored?**
Avatars and thumbnails go to the `storage` Docker volume mounted at
`/app/wwwroot/storage` (local non-Docker: `src/Voat.UI/wwwroot/storage/`).

**Running the tests**
`dotnet test` — see [2d](#2d-tests-optional). Some tests contact external web
services and will fail offline; that is expected and documented in CHANGELOG.md.
