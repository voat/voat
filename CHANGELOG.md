# Changelog

Running log of modernization changes to this Voat fork, and why each was made.
Newest entries at the top. "Phase" refers to the modernization plan (Phase 1 audit,
Phase 2 foundation, Phase 3 data, Phase 4 web/security, Phase 5 tests, Phase 6 packaging).

## [Unreleased]

### Post-Phase 6 — user-requested changes
- **Operator upgrade / backup / restore, without losing data** (user request:
  "include a easy way to upgrade to a new release of the REvoat without losing
  data"). New `scripts/lib.sh`, `scripts/backup.sh`, `scripts/upgrade.sh`,
  `scripts/restore.sh` + a new `UPGRADING.md`, bash only, written for the Linux
  VPS case (and clean under Git Bash). **The actual data-loss hazard is the
  Compose project name:** `docker-compose.yml` deliberately has no top-level
  `name:`, so Compose names the volumes after the checkout folder
  (`voat-core_db-data`, `voat-core_storage`, ...). Extracting a new release into
  a fresh folder therefore stands up a *second, empty* site — the old data is
  untouched but invisible, which reads exactly like a wipe. `lib.sh` resolves
  the project from the volumes that exist on disk (`*_db-data` that also has
  `*_storage`) rather than from the folder name, always passes `-p <project>`,
  and adopts an existing project when the folder disagrees — that adoption is
  the feature, not a failure. It falls back to the folder name only when no
  install exists at all. **Backup is deliberately hybrid:** `pg_dump --clean
  --if-exists` for the database (portable across machines and PostgreSQL
  majors, and *crash-consistent* — a `tar` of a running `db-data` is not, which
  is why the database is never tarred here) plus tarballs of `storage`,
  `dataprotection` and `caddy-data`, a `chmod 600` copy of `.env`, and a
  `manifest.txt` **written last**, so its absence marks an incomplete run and
  `restore.sh` refuses to touch such a directory. The manifest carries sha256
  per artifact and `restore.sh` verifies them before mutating anything.
  **Secrets are never parsed:** `POSTGRES_USER` / `POSTGRES_DB` come from
  `docker compose exec db printenv`, and `.env` is only ever copied. **Two
  platform bugs found and designed around rather than patched:** tars travel
  over stdin/stdout with no host bind mounts at all, because MSYS rewrites
  POSIX-looking paths (`/backup` → `C:/Program Files/Git/backup`) before docker
  reads them; and `git -C <path>` is avoided for the same reason (Windows git
  wants a Windows path) in favour of a `git_at` helper that `cd`s first.
  `upgrade.sh` backs up first, handles both git checkouts (`--ref` for a
  specific tag, otherwise the newest `v*` — filtered, because this repo carries
  non-release tags like `avatar-upload-fix`) and release-archive installs (no
  git at all; prints the unpack-over-the-directory instructions instead of
  failing), then rebuilds, runs the idempotent `/voat-sql/setup.sql` and probes
  `http://127.0.0.1:8080/` (there is no `/health` endpoint). **Schema, zero
  manual steps:** `setup.sql` is additive-only — `CREATE ... IF NOT EXISTS`
  throughout, seed rows behind `WHERE NOT EXISTS`, sequences resynced to
  `max("ID")+1` — so re-running it on a populated database cannot overwrite
  operator edits; `docker/db-init` only ever runs on an empty volume, which is
  why the upgrade has to invoke it explicitly. `UPGRADING.md` also covers the
  bootstrap gap (upgrading *from* a release that predates these scripts) and
  moving to a new machine. **Gotcha worth keeping:** `INSTALL.md`'s
  `docker compose down -v` suggestion was the single most likely way to nuke
  the state this work protects — the line is kept (it is legitimate for a
  throwaway local install) but is now annotated as deleting everything with no
  undo. **Not included, by decision:** no PowerShell twins (bash matches
  `INSTALL.md` and the audience); `docker-compose.yml` left untouched — pinning
  `name:` would silently orphan every existing install whose folder is not
  named `voat-Core`, so detection preserves instead; no migration framework
  (the idempotent `setup.sql` + `SitePageSchema.EnsureSitePageTable()` at
  `Startup.cs:166` *is* the schema story). Also: `.gitattributes` pins `*.sh`
  to LF (a Windows `autocrlf` checkout would otherwise break every script on
  Linux with `$'\r': command not found`), and `backups/` is gitignored because
  it embeds `.env`.
- **Owner-editable User Agreement & Privacy Policy** (user request: edit both
  pages "without having to go into the code"). An account listed in
  `voat:settings:siteOwners` sees a pencil glyph + "Edit" on `/help/useragreement`
  and `/help/privacy`; clicking swaps the body for an inline markdown editor
  (Save/Cancel, no navigation). New `SitePage` table stores the markdown keyed by
  route slug (`useragreement`, `privacy`) with `UpdatedBy` / `LastUpdateDate`, so
  edits survive `docker compose up -d --build` — which replaces the container
  filesystem and would destroy anything held in the view files. Nothing is seeded
  into the table: a missing row falls back to the compiled-in defaults in
  `SitePageDefaults`, so a fresh install renders the same pages as before.
  **Schema, zero manual steps:** `docker/db-init` never re-runs on an existing
  volume, so `SitePageSchema.EnsureSitePageTable()` runs from `Startup.Configure`
  and creates the table only if absent (idempotent single guard per dialect, PG +
  SQL Server). `misc/SqlScripts/*/voat.sql` carry the full definition for fresh
  databases. **Rendering:** `Formatting.FormatSitePageMessage` calls the existing
  `FormatMessage` with `processContent: false` (disables the social autolinker —
  `@mentions`, `s/set`, `/r/`, bare-URL wrapping must not fire on legal text —
  while keeping MarkdownDeep `SafeMode` and its scheme allow-list intact) and
  then substitutes the `{siteName}` placeholder with the HtmlEncoded `siteName`.
  Both page bodies were flattened out of Razor into markdown for this: `<h1>`/`<h2>`
  → `#`/`##` (levels kept 1:1 so `.about-page` CSS still matches), `<ul><li>` →
  `- `, `@siteName` → `{siteName}`. Two Razor-only constructs could not survive
  as markdown and were flattened deliberately: the `IsVoatBranded`/`SiteDomain`
  ternary in Privacy is now plain wording (there is no `{siteDomain}` placeholder
  in scope), and the `@if (IsVoatBranded)` gate around the Updates block is now
  unconditional — otherwise renaming `siteName` would silently hide it. The raw
  `<a href>` to Google's privacy policy became a markdown link, because SafeMode's
  raw-HTML whitelist is only `s`/`sub`/`sup` and a raw `<a>` renders as visible
  escaped text. The DMCA contact `copyright@voat.co` is preserved verbatim rather
  than being run through `{siteName}`. **Access control:** `VoatSettings.SiteOwners`
  (`string[]`, comma-separated entries also accepted so one env var can name
  several accounts) + `IsSiteOwner(userName)` — deliberately a config list, not a
  role, so a GlobalAdmin off the list still cannot edit. Fails closed: null or
  empty list, or a null/empty user name, is denied. Default ships as `["admin"]`
  so the feature works immediately on a stock install; **note that `admin`'s
  default password is published in INSTALL.md, so change it.** Wiring:
  `voat__settings__siteOwners__0: ${VOAT_SITE_OWNERS:-admin}` in `docker-compose.yml`
  — the `:-admin` default matters, because an empty environment variable overrides
  `appsettings.json` and would silently leave the feature with no owner. **Save
  path:** `HomeController.UpdateHelpPage` at `POST /help/edit/{pagetoshow}`
  (route registered before the `help/{*pagetoshow}` catch-all, which would
  otherwise match it as `pagetoshow = "edit/..."`), guarded by
  `[Authorize]` + `[PreventSpam(30)]` + `[VoatValidateAntiForgeryToken]`, writes
  through `Repository` (`SaveSitePage`) rather than the controller's read-only
  `_db`, and redirects back (Post/Redirect/Get). The hidden form is UX only —
  the owner check is enforced again server-side and returns **403** on deny, and
  an unrecognized slug is also 403. Markdown is capped at 30000 characters.
  **Tests:** `SiteOwnerTests` (failing closed, case-insensitivity, comma-separated
  entries, blank-entry handling), `SitePageDefaultsTests` (both defaults non-empty,
  `{siteName}` present, **no** surviving Razor tokens, unknown slug → `null`),
  `SitePageRepositoryTests` (round-trip, unknown name → `null`, second save updates
  in place, author/timestamp restamped), and `SitePageFormattingTests`
  (`FormatSitePageMessage` against the real default bodies: `{siteName}`
  substitution + encoding, SafeMode escaping of `<script>`, no social autolinking,
  markdown links still work, `javascript:` hrefs neutralized). **Gotcha worth
  keeping:** both page views are rendered by `HomeController`, so
  `@Html.Partial("_HelpPage", Model)` with a short name resolved against
  `/Views/Home/` and `/Views/Shared/` and **never** `/Views/Help/` — the first
  cut 500'd on both pages with "The partial view '_HelpPage' was not found".
  Fixed with the explicit `~/Views/Help/_HelpPage.cshtml` path, which is the
  existing convention (`Views/Home/Comments.cshtml` does the same for
  `~/Views/Welcome/_FirstTimeVisitorWelcome.cshtml`). **Second gotcha:** the Edit
  button rendered but did nothing on click. The toggle was written with jQuery
  (`$(function () { ... })`) inline in the partial, but jQuery is loaded in
  `_LayoutContainer.cshtml` *after* `@RenderBody()` (jQuery at line 119, body at
  114), so the script ran as `ReferenceError: $ is not defined` and the handler
  never bound. Rewritten with plain `document.getElementById` /
  `addEventListener` — the elements are already parsed above the script, so it
  binds immediately and has no load-order dependency. The markdown-editor
  toolbar buttons are unaffected: they use inline `onclick="... $(...) ..."`,
  which resolves at click time when jQuery has long since loaded. **Not
  included, by decision:** no preview button in v1 — the existing preview
  endpoint hardcodes `processContent: true` and is duplicated in the minified
  `voat.min.js` bundle.
- `src/Voat.UI/Views/Help/UserAgreement.cshtml` — user agreement updated with
  new rules (user-supplied text). **Rules additions:** "Do Not Incite Harm" now
  carries three explicit bullets (no supporting/idolizing terrorist groups, no
  animal abuse, no crush or animal-abuse videos), and "Protect Kids" now
  carries an explicit bullet against sexualized drawings of children
  (Lolicon/Shotacon) instead of a trailing sentence fragment. **Contact
  info:** the Security and Children sections now publish
  `rayman@disroot.org` in place of `security@voat.co` / `hello@voat.co`. The
  DMCA Copyright Agent remains `copyright@voat.co` as the supplied text
  specifies. **Wording:** "agree to not" → "agree not" (Keep Everyone Safe),
  "real world" → "real-world", "providing they do not" → "provided they do
  not", "third-parties" / "third party service providers" → "third parties" /
  "third-party service providers" (Moderators, Indemnity, Disclaimer),
  "disclaim any and all warranties" → "disclaim any warranties", and an
  Oxford comma in "claims, demands, and damages". **Dates:** Updates section
  moved from 2017-01-11 / May 9, 2014 to 10/15/2026 / October, 2026.
  **Behavior change worth noting:** the two contact-email sentences and the
  Updates section were wrapped in `@if (VoatSettings.Instance.IsVoatBranded)`,
  which is true only while `siteName` is literally "Voat". Those gates were
  dropped so the contact addresses and the Updates block render for every
  deployment; otherwise renaming `siteName` to REvoat would silently blank
  the contact emails and hide the Updates section. `@siteName` substitutions
  throughout the page are preserved. Incidental fix: the Security line
  rendered as "send an emailto security@voat.co" (missing space in the
  conditional string); it now reads "send an email to rayman@disroot.org".
- Footer copyright line rewritten site-wide (user request) to disclaim
  affiliation with the original project: `© REVOAT IS NOT AFFILIATED WITH
  VOAT, Inc. All rights reserved. The original Voat and the Voat Mascot are
  trademarks of Voat, Inc.` Applied to the live `footerText` setting in
  `src/Voat.UI/appsettings.json` (rendered by `Views/Shared/_Footer.cshtml`),
  the two template copies (`src/Voat.UI/~appsettings.json`,
  `test/Voat.Tests/~appsettings.json`), and the four standalone offline
  pages that hard-code the same sentence
  (`wwwroot/static/inactive.htm`/`inactive.min.htm`, `ddos.htm`/`ddos.min.htm`
  /`ddos.min.2.htm`). Each file keeps its existing copyright-symbol encoding:
  `&amp;copy;` in the JSON settings, `&copy;` in the unminified pages, and a
  literal `©` in the minified ones. Note `ddos.min.2.htm` previously read
  "2014-2016" where the others said "2014-2017"; both collapse to the new
  text. The link row above it (`advertise`/`donate`/`API`/`about`/`faq`/
  `markdown`/`source code`) is unchanged and still lives in `_Footer.cshtml`.
- `src/Voat.UI/appsettings.json` — enabled `enableVotes` (user request).
  Scope clarification: this key gates the **"Vote" (poll) submission
  feature** (`VoteController.Create/Save`, the "create a poll" sidebar
  button) — it never gated ordinary upvote/downvote arrows, which run
  through `UserController.Vote` and work for all logged-in users. With the
  key off, only admin-role holders could create polls.
- `src/Voat.UI/appsettings.json` — enabled role checks (`enableRoles: true`,
  `enabledRoles: [ "*" ]`) in `voat:settings`. The admin command pipeline
  (`GlobalBanCommand`) authorizes via `SecurityExtension.IsInAnyRole`, which
  drops every requested role not listed in `enabledRoles` and returns false
  outright when `enableRoles` is false; both settings default to off/empty —
  same as upstream, where production config was never checked into the repo —
  so the Admin Ban Utility answered "Permissions not granted" even to a
  GlobalAdmin. `"*"` simply honors all role names in checks; it grants nothing
  by itself (the user must still hold the role). This also revives the other
  grant gates that were dead for everyone: moderator removal
  (`Repository.cs`), `ModeratorPermission`'s global-admin bypass, and admin
  voting when `EnableVotes` is off (`VoteController.VotesEnabled`).
- `/ModLog` — investigated, **no code change**. Bare `/ModLog` has never been a
  route (identical upstream). Voat's moderation log is per-subverse by design:
  `/v/{subverse}/modlog/submission|comment|banned`. The site-wide ban lists are
  `/log/banned/user` and `/log/banned/domain`. The data layer requires a
  subverse (a null one throws in `Repository_ModLog`), so a global `/ModLog`
  landing page would be new functionality — pending user decision.
- `src/Voat.UI/Views/Shared/Components/Sidebar/Default.cshtml` — removed the
  "Unofficial Voat Mobile Apps" sidebar box (user request). The apps it
  promoted (Boats for Voat, Appvoat, Voatify) are discontinued and every
  link was dead. The surrounding sidebar boxes (Most viewed, Advertisement)
  are unchanged.

### Phase 6 — Packaging, deployment & configuration

Status: **complete**. One-command Docker deployment (app + PostgreSQL + Caddy
with automatic HTTPS), all secrets moved to environment/.env, full install guide.

#### New files
- `Dockerfile` — multi-stage build (sdk:10.0 → aspnet:10.0, runs as the non-root
  `app` user). No Node build needed: minified wwwroot assets are committed.
  Writable locations: `/app/wwwroot/storage` (uploads) and
  `/home/app/.aspnet/DataProtection-Keys`.
- `docker-compose.yml` — `db` (postgres:18, healthcheck, schema+seed on first
  boot), `app` (settings mapped from `.env`), `caddy` (auto-HTTPS + HTTP/3).
  One command: `docker compose up -d --build`.
- `docker/db-init/00-voat-setup.sql` — first-boot wrapper that pulls in the
  ordered, idempotent migration set (`misc/SqlScripts/PostgreSql/setup.sql`).
- `Caddyfile` — reverse proxy to `app:8080`; Let's Encrypt automatically for a
  real `VOAT_DOMAIN`, local self-signed certificate for `localhost`.
- `.env.example` — documented template of every deployment-specific value
  (database password, signing key, domain, SMTP/SendGrid, reCAPTCHA, legacy
  API keys) with generation instructions.
- `.dockerignore` — keeps `bin/obj/test/misc/.env` out of the build context.
- `INSTALL.md` — Local Docker / Local no Docker / VPS guides, configuration
  reference, default accounts, and troubleshooting.

#### Secrets out of source control
- `src/Voat.UI/appsettings.json` — scrubbed: the three connection strings,
  reCAPTCHA keys, legacy API keys, Azure storage connection string and email
  service key are now empty and annotated `SET VIA ENV`. Non-secret site
  settings stay in the file. No code change needed: `Startup` builds
  configuration with `AddEnvironmentVariables()` after the JSON files, so
  environment values (`voat__settings__…`, `voat__data__connections__N__value`)
  always win.
- `.gitignore` — removed the blanket `appsettings.json` ignore (it broke fresh
  clones: `Startup` requires the file at startup and Docker publishes it);
  added `test/Voat.Tests/appsettings.json` (local DB credentials),
  `appsettings.Development.json`, `.env`, `.env.*` (`!.env.example`).
- `test/Voat.Tests/appsettings.json` — still on disk (tests need it), now
  untracked; its `~appsettings.json` template is the committed copy.

#### Production fix applied during packaging
- `src/Voat.Business/Data/Repository/SqlFormatter.cs` — new
  `TableFunctionCall(name, arguments, out commandType)` helper: returns
  `SELECT * FROM <fn>(...)` + `CommandType.Text` for PostgreSQL (the
  `usp_Reports_*` objects are `RETURNS TABLE` functions), and keeps
  `CommandType.StoredProcedure` semantics for the SqlServer store option.
- `src/Voat.Business/Data/Repository/Repository_Stats.cs` — the three report
  call sites now use the helper with `CAST(@… AS timestamp/int)` arguments
  (casts keep `NULL` parameters resolvable). Root cause of the three failing
  `Query_Stats_*` tests: modern Npgsql renders `CommandType.StoredProcedure` as
  `CALL`, which only matches `CREATE PROCEDURE` objects. Verified: all 3
  `Query_Stats` tests pass.

#### Dependency swap during packaging — ImageSharp → SkiaSharp
- Why: the Docker image builds `Release`, and ImageSharp 4.x (the only line with
  the security fixes) injects `SixLabors.ImageSharp.targets`, which hard-fails
  Release builds without a Six Labors license key (Debug builds only warn).
  Per user decision, swapped to SkiaSharp 4.153.1 (Apache-2.0, no key needed)
  instead of buying a license or downgrading to a vulnerable 3.1.x.
- `src/Voat.Business/Voat.Business.csproj` — replaced
  `SixLabors.ImageSharp` with `SkiaSharp` +
  `SkiaSharp.NativeAssets.Linux.NoDependencies` (Linux natives for the Docker
  image; fontconfig dropped — only bitmap ops are used).
- `src/Voat.Business/Imaging/ImageHandler.cs` — reimplemented `Resize` on
  SkiaSharp. Behavior preserved: same geometry (Scale/Center), same
  black-bar thumbnails when `forceJpegFormat` (JPEG quality 100), same
  source-format preservation otherwise (PNG fallback), same
  `InvalidDataException("Stream does not contain a supported image.")` for
  undecodable input (SkiaSharp throws `ArgumentNullException` internally; the
  port normalizes it). Only the imaging library changed — no call-site,
  signature, or output-contract changes.
- Verified: Release build 0 errors (the license blocker is gone); Docker image
  builds end-to-end; runtime harness on real images confirms JPEG w/ black bars
  at 50x50 q100, PNG format preserved, garbage input → `InvalidDataException`;
  full test suite unchanged vs. pre-swap baseline (454 passed / 12 failed /
  30 skipped, identical failure set).

#### Fixes found during smoke test
- `Dockerfile` — runtime stage installs `libgssapi-krb5-2`. Npgsql probes for
  the GSSAPI (Kerberos) library on first DB connection and logs
  `Cannot load library libgssapi_krb5.so.2` when it is missing. Harmless
  (password auth works regardless) but alarming in production logs for the
  beginner audience INSTALL.md targets; installing it also enables Kerberos
  auth for deployments that use it.
- postgres:18 data mount (below).
- `docker-compose.yml` — the `db` volume now mounts at `/var/lib/postgresql`
  (not `/var/lib/postgresql/data`). postgres:18+ images store data in a
  major-version subdirectory and refuse to boot at all when the legacy
  `/var/lib/postgresql/data` path is mounted (even empty) —
  docker-library/postgres#1259. Without this the db container stays
  "unhealthy" and `app` never starts. No data-layout consequences: the named
  volume content and backup commands are unchanged.

#### Operational notes
- DataProtection keys (auth cookies / antiforgery) persist in the
  `dataprotection` volume. `Startup` does not configure key persistence, so
  without this volume every container restart would log every user out.
- Uploaded avatars/thumbnails persist in the `storage` volume.
- The app listens on `http://+:8080` inside the container and is published only
  on `127.0.0.1:8080` (local convenience; unreachable from the internet on a
  VPS). Caddy fronts public traffic on 80/443.
- Seeded admin account `admin` / `ChangeMe123!` (from `seed.sql`) is documented
  in INSTALL.md with a "change immediately" warning — final default-password
  policy still pending user decision.

### Phase 5 — Tests

Status: **complete** (port + triage + report). The suites run on .NET 10 /
MSTest against PostgreSQL 18; failures are classified below.

- `test/Voat.Validation.Tests` (F#): **70 / 70 pass**.
- `test/Voat.Tests` (C#, MSTest 4.5.1 / Microsoft.NET.Test.Sdk 18.10.1):
  **451 passed / 15 failed / 30 skipped** of 496 (2.1 min).

#### Fixed during Phase 5
- `test/Voat.Tests/appsettings.json` created from `~appsettings.json`
  (storeType PostgreSql, local Docker PostgreSQL on 5433).
- `TestDataInitializer.CreateUser` failure path now includes Identity's error
  list in the exception (seed failures were undebuggable).
- Password-policy regression caught by the suite and fixed: `new PasswordOptions()
  { RequiredLength = 8 }` silently inherits Identity's composition-rule defaults
  (RequireDigit/Uppercase/NonAlphanumeric/Lowercase = true) although the legacy
  validator never enforced them. `VoatSettings.PasswordOptions` now sets all four
  flags explicitly to false — length-only policy (NIST), as Phase 4 promised.

#### The 15 failures, classified

**External-service dependent (6) — cannot pass as written; dead or drifted
live web resources. Recommendation: rewrite as offline fixtures or retire.**
- `Local_FileLifeCycle`, `ContentDelivery_FileLifeCycle` — fetch
  `https://voat.co/Graphics/voat-goat.png`; voat.co no longer exists.
- `TestGetOpenGraphImageFromUri` — bbc.com now answers 403 to the fetch.
- `TestEscapedQuotesInTitle` — asserts the *live* LWN.net headline text; the
  article's title has changed since 2016.
- `TestTagInTitle` — live fetch to an external site returns Forbidden.
- `TestRedirect` — asserts live third-party redirect behavior.

**Diagnosed production bug behind 3 failures (FIXED — see Phase 6):**
- `Query_Stats_HighestVotedContent`, `Query_Stats_UserVoteGivenStats`,
  `Query_Stats_UserVoteReceivedStats` — `Repository_Stats` invoked the
  `usp_Reports_*` `RETURNS TABLE` **functions** with
  `CommandType.StoredProcedure`, which Npgsql 10 renders as `CALL` (valid only
  for `CREATE PROCEDURE` objects). Verified via psql: `CALL` fails with 42883,
  `SELECT * FROM func(...)` succeeds. Fixed in Phase 6 (see below); the three
  tests now pass.

**Behavioral differences / test-side issues (6) — not security regressions,
logged with root cause:**
- `TestActivitySerialization`, `TestCommandActivitySerialization` — round-trip
  of `ClaimsPrincipal` through `DataSerializationSettings` returns
  `AuthenticationType` `null` instead of `""` (Json.NET constructor selection on
  modern `ClaimsIdentity`; `Name` and `IsAuthenticated` survive, so the
  SafeTypeNameBinder allow-list is not implicated).
- `Add_Remove_Moderator_Outcome_Test` — the test itself throws
  `NotImplementedException` (`OutcomeTests.cs:25`).
- `TestGeorgesBreak` — MarkdownDeep quote-run rendering differs on the
  `"~~~~~"` edge case.
- `QuerySubmissions_By_Domain` — returns no rows (domain query/paging behavior).
- `PreventUserFromPostingCompromisedTitle1` — the hourly global posting quota
  rule fires before the compromised-title rule for the test's data volume
  (rule ordering), so the quota message wins.

The 30 skipped tests are pre-existing skips (feature disabled by default or
legacy-API scope).

### Phase 4 — Web layer + security

Status: **complete**. All 16 audit findings addressed (13 fixed, 3 fixed with
documented deviations, 2 residuals reported to the user with proposed options).
Solution builds with 0 errors; every fix verified against the live app (Docker
PostgreSQL 18) or a direct API probe (48/48 checks pass).

#### Finding 1 (High) — XSS in message content
- `src/Voat.UI/Views/Messages/Index.cshtml` — line 130 rendered
  `@Html.Raw(message.Content)` inside the reply textarea: raw user markdown/HTML
  with `</textarea><script>…` broke out of the element. Now `@message.Content`,
  matching the established pattern in `_SubmissionComment.cshtml` /
  `_SubmissionDetail.cshtml` (Razor encodes). `@Html.Raw(message.FormattedContent)`
  on line 129 is unchanged — that is the sanitize-pipeline HTML output, by design.

#### Finding 2 (High) — Newtonsoft `$type` gadget deserialization
- **New** `src/Voat.Common/Configuration/SafeTypeNameBinder.cs` —
  `DefaultSerializationBinder` subclass restricting `$type` resolution to an
  allow-list: framework primitives/enums/value types, `Nullable`/arrays
  (recursive), generic collections whose args are themselves allowed,
  `Newtonsoft.Json.Linq.*` / `System.Text.Json.*` containers, and any type from a
  `Voat*`-named assembly. Everything else throws `JsonSerializationException`.
- `src/Voat.Common/Configuration/JsonSettings.cs` — `SerializationBinder = new
  SafeTypeNameBinder()` on `DataSerializationSettings` **and**
  `DataInputSerializationSettings` (the user-supplied vote-option JSON path).
  `TypeNameHandling` is deliberately kept: polymorphic round-trips (vote
  restriction/outcome options, Redis cache entries, command tests) depend on it;
  the allow-list preserves that behavior while closing the gadget vector.

#### Finding 3 (High) — obsolete password hashing + weak policy
- `src/Voat.Business/Auth/VoatUserManager.cs` — hashing switched from the
  hard-coded IdentityV2 mode (PBKDF2-HMAC-SHA1, 1000 iterations) to
  `PasswordHasherCompatibilityMode.IdentityV3` (PBKDF2-HMAC-SHA256) with
  `IterationCount = 100000`. Existing V2 hashes still verify on login and are
  transparently upgraded to V3 on next successful login (Identity behavior).
  `VoatPasswordValidator` now calls `base.ValidateAsync` (was a no-op returning
  Success for any input).
- `src/Voat.Common/Configuration/VoatSettings.cs` — `PasswordOptions.RequiredLength`
  6 → 8 (NIST baseline: length over composition rules; no complexity rules added
  deliberately).
- `test/Voat.Tests/_Infrastructure/Extensions.cs` — `GetUnitTestUserPassword`
  pads to ≥ 8 chars so generated test users satisfy the new policy.
- `misc/SqlScripts/PostgreSql/seed.sql` — admin seed hash regenerated as
  IdentityV3 (100k iterations) for password `ChangeMe123!` and verified against
  the running hasher. Banner comment updated to match.

#### Finding 4 (High) — CSRF on state-changing endpoints
- `src/Voat.UI/Controllers/ApiKeysController.cs` — `[VoatValidateAntiForgeryToken]`
  on `Delete(int id)` and `Create(ApiKeyCreateRequest)` POSTs (`Edit` already had
  it).
- `src/Voat.UI/Views/ApiKeys/Index.cshtml` — `@Html.AntiForgeryToken()` added to
  the Delete form (the form existed without a token).
- `src/Voat.UI/Controllers/SetController.cs` — `[ValidateAntiForgeryToken]` on
  `Update(Set set)` (its form `_Update.cshtml` already emits the token; the
  `submitForm` JS posts `$form.serialize()`, which carries it).
- `src/Voat.UI/Areas/Admin/Controllers/SpamController.cs` —
  `[ValidateAntiForgeryToken]` on `Ban(BanViewModel)` (view already emits token).

#### Finding 5 (High) — markdown link/image scheme allow-list
- `src/Voat.Business/Utilities/UrlUtility.cs` — new
  `IsUrlSchemeAllowed(string)`: HTML-decodes, permits relative/protocol-relative/
  fragment URLs, and allow-lists schemes `http`, `https`, `ftp`, `mailto`.
  `InjectableJavascriptDetected` untouched (tests assert its semantics).
- `src/Voat.Business/Utilities/Formatting.cs` — `PrepareLink` gains a
  scheme-allow-list branch (`href="#"` + `data-ScriptStrip` marker, mirroring the
  legacy javascript-strip pattern); new `PrepareImage` hook strips non-allowed
  `src` schemes on images (previously unchecked entirely).

#### Finding 6 (Medium) — no HSTS / security headers / weak auth-cookie flags
- `src/Voat.UI/Startup.cs` —
  - inline response-header middleware: `X-Content-Type-Options: nosniff`,
    `X-Frame-Options: SAMEORIGIN`, `Referrer-Policy: strict-origin-when-cross-origin`;
  - `app.UseHsts()` when not in Development **and** `ForceHTTPS` is on;
  - `ConfigureApplicationCookie` sets `HttpOnly = true`, `SameSite = Lax`,
    `SecurePolicy = SameAsRequest` explicitly.

#### Finding 7 (Medium) — custom cookies missing flags
- `src/Voat.Http/Extensions/HttpExtensions.cs` — `Add(this IResponseCookies, …)`
  now sets `HttpOnly = true`, `SameSite = SameSiteMode.Lax`, and
  `Secure = VoatSettings.Instance.ForceHTTPS` (plain-http local dev keeps working;
  HTTPS deployments mark cookies Secure).

#### Finding 8 (Medium) — spoofable client IP via forwarded headers
- `src/Voat.Http/Extensions/HttpExtensions.cs` — `RemoteAddress()` treats the
  socket peer as authoritative unless it is a trusted proxy (loopback, 10/8,
  172.16/12, 192.168/16, IPv6 fc00::/7); forwarding headers
  (CF-Connecting-IP / X-Forwarded-For / X-Original-For) are consulted only then.
- `src/Voat.UI/Startup.cs` — `ForwardedHeadersOptions.KnownNetworks` gets the
  same RFC1918 ranges, and `app.UseForwardedHeaders()` is actually invoked
  (previously configured but never added to the pipeline).

#### Finding 9 (Medium) — unsalted IP hashing
- `src/Voat.Business/Utilities/IpHashUtility.cs` — `IpHash.CreateHash` is now
  HMAC-SHA256 keyed with `VoatSettings.Instance.SigningKey` (Base64 output,
  as before). Empty key degrades to unkeyed HMAC — **SigningKey must be set in
  production** (will be in `.env.example`, Phase 6).

#### Finding 10 (Medium) — chat token = unsalted MD5 in redirect URLs
- `src/Voat.Business/Utilities/Chat/ChatRoom.cs` — `GetAccessHash` MD5 →
  HMAC-SHA256 keyed with `SigningKey`. Residual: the token still travels in the
  URL, and the SignalR hub behind chat is dead code (see Residuals).

#### Finding 11 (Medium) — SSRF in thumbnail/OG fetcher
- `src/Voat.Business/Utilities/UrlUtility.cs` —
  `IsUriSafeForServerFetch(Uri)`: http/https only; resolves DNS and rejects the
  URL unless **all** resolved addresses are public — blocks loopback, RFC1918,
  169.254/16 (incl. cloud metadata 169.254.169.254), CGNAT 100.64/10, 0/8,
  multicast/reserved, IPv6 ULA/link-local/multicast, and IPv4-mapped-to-IPv6
  forms of the above.
- `src/Voat.Business/Utilities/HttpResource.cs` — `AllowAutoRedirect` forced
  off at the handler; redirects followed manually (max 5 hops) with every hop
  re-validated against the deny-list (a public URL must not redirect us into the
  internal network). New `HttpResourceOptions.BlockNonPublicAddresses`
  (default on) is the escape hatch for trusted hard-coded endpoints. Covers both
  `ThumbGenerator` and `AjaxGatewayController.TitleFromUri` (single choke point).

#### Finding 12 (Medium) — captcha fails open
- `src/Voat.UI/Utils/AntispamUtility.cs` — exception path adds a ModelState
  error ("Captcha verification failed." + log) instead of silently passing; a
  null/unparseable verify response counts as failure. Disabling
  `CaptchaEnabled` remains the explicit opt-out.

#### Finding 13 (Low) — predictable, plaintext API keys
- `src/Voat.Business/Data/Repository/Repository_Api.cs` — key generation moved
  from `Guid.NewGuid()` to CSPRNG (`RandomNumberGenerator.Create()` +
  `Convert.ToHexString`); public `VO{32 hex}AT` / private `{64 hex}` formats
  preserved exactly. Hashing keys at rest is **deferred** (see Residuals).

#### Finding 14 (Low) — per-process rate limiting
- `src/Voat.Http/Extensions/HttpExtensions.cs` — throttle signature hash MD5 →
  SHA256 (it is only an in-memory cache key). Rate-limit semantics (per-process
  `IMemoryCache` over MD5(IP+UA+path) inputs) intentionally unchanged — see
  Residuals.

#### Finding 15 (Low) — file-manager path traversal
- `src/Voat.Common/Components/FilePather.cs` — `ProcessRelative` drops `.` and
  `..` segments; `LocalPath` additionally verifies the `Path.GetFullPath` result
  stays under the configured root and throws `ArgumentException` otherwise.

#### Finding 16 (Info) — ForwardedHeaders defaults
- Same-network trust set explicitly (Finding 8) and documented for the Phase 6
  install guide (reverse-proxy deployment notes).

#### Dependency vulnerability found during verification
- `src/Voat.Business/Voat.Business.csproj` — `SixLabors.ImageSharp` 3.1.12 →
  **4.1.3**. NuGet audit flagged 5 advisories against 3.1.12 (High:
  GHSA-j3p4-wp97-rph4, GHSA-j9gm-c75j-xc9q, GHSA-jjfr-hcj7-qf5w; Moderate:
  GHSA-gwg2-r3hj-4w44, GHSA-wmxv-xphr-5c9g). All affect `>= 2.0.0, <= 4.1.1` and
  are fixed only in 4.1.2+ — no patched 3.1.x exists, so the major-version bump
  is required. The only consuming code, `src/Voat.Business/Imaging/ImageHandler.cs`
  (Phase 2 reimplementation on ImageSharp), compiles unchanged against 4.1.3 —
  the API surface it uses is stable across 3→4. `dotnet list Voat.sln package
  --vulnerable --include-transitive` is now clean for all 14 projects.

#### Residuals / deliberate deviations (reported to the user, decisions pending)
- **Chat transport is dead**: `MessagingHub` is fully commented out and no
  `AddSignalR`/`MapHub` registration exists — chat cannot work on any deployment
  regardless of this fix. Proposed options: (a) port hub to modern SignalR,
  (b) retire the chat routes. Not silently deleted; awaiting user decision.
- **Chat access token still appears in URLs** (only the hash algorithm was fixed).
  Meaningless while the hub is dead; revisit if chat is ported.
- **API keys are still stored plaintext** — hashing them at rest would remove the
  ability to re-display a lost secret, i.e. a functionality change. Awaiting
  user decision.
- **No Content-Security-Policy header**: ~250 legacy Razor views rely on inline
  script/handlers; a strict CSP would break the UI (style preservation rule).
  The three headers added above are compatible with the existing pages.
- **`mailto:` kept in the markdown scheme allow-list** — inert (opens a mail
  client) and rendered by the legacy site; dropping it would remove a feature.
- **Rate limiting stays per-process** — a multi-instance deployment would need
  Redis-backed counters; semantics documented instead.
- **DNS-rebinding TOCTOU** on SSRF checks: DNS is validated at request start; a
  resolver flipping answers between check and connect is a residual risk
  (mitigated in depth by the per-hop redirect validation).

#### Verification
- `dotnet build Voat.sln`: **0 errors** (legacy warnings unchanged).
- `dotnet list Voat.sln package --vulnerable --include-transitive`:
  **0 vulnerable packages** across all 14 projects.
- Live app (Docker PostgreSQL 18, `voat-pg-test` :5433), full pipeline checks:
  - homepage 200 with new security headers present;
  - `NotFirstTime` cookie now `samesite=lax; httponly`;
  - admin login (`ChangeMe123!`, IdentityV3 seed hash) → 302 +
    `.AspNetCore.Identity.Application` cookie with `samesite=lax; httponly`;
  - all four newly CSRF-protected endpoints (ApiKeys/Delete, ApiKeys/Create,
    Set/Update, Admin/Spam/Ban) return **400** without a valid token.
- Direct API probe (console harness against the built assemblies), **48/48**:
  - markdown: `javascript:` / `data:` / `vbscript:` / HTML-encoded-scheme links
    **and** images all neutered to `#`; `https:`, `mailto:`, relative and
    protocol-relative URLs preserved;
  - `SafeTypeNameBinder`: `System.Diagnostics.Process` and `System.IO.FileInfo`
    rejected *by policy*; `ObjectDataProvider` not instantiable; Voat types,
    `List<int>`, `string` accepted; `List<int>` and `LogInformation` still
    round-trip with `$type` (compat kept); gadget payloads never instantiate on
    either settings profile;
  - SSRF deny-list: localhost, 127.0.0.1, 169.254.169.254, 10/8, 172.16/12,
    192.168/16, CGNAT, IPv6 loopback/ULA, non-http schemes all blocked;
    8.8.8.8 and example.com allowed;
  - `IpHash` output equals keyed HMAC-SHA256, differs from plain SHA-256/512,
    and changes with `SigningKey`.

### Phase 3 — Data layer

Status: **complete** (verification pending one local-machine item, see below).
The SQL migration set is now idempotent, seeded (default subverses + admin
account), and verified against a fresh PostgreSQL 18.

#### Migration-set approach
- Chose a **single idempotent SQL setup set** over EF Core migrations: the schema
  is hand-written SQL (sequences, custom column defaults, plpgsql functions),
  and the unit-test harness executes these exact files by name (splitting on
  `GO`, replacing `{dbName}`). An EF migration history would duplicate and drift
  from the scripts the tests run. The requirement allowed either option.
- Execution order: `voat.sql` (schema) → `voat_users.sql` (identity schema) →
  `procedures.sql` (functions) → `data.sql` (base data) → `seed.sql`
  (production seed). Kept the four original filenames/locations untouched so
  `TestDataInitializer` needs no changes.

#### Files changed
- `misc/SqlScripts/PostgreSql/voat.sql` — idempotency: `CREATE TABLE IF NOT
  EXISTS` / `CREATE SEQUENCE IF NOT EXISTS` on all 40 tables and 37 sequences;
  the 73 `ADD CONSTRAINT` statements wrapped in one `DO` block with per-
  constraint existence guards (PostgreSQL has no `ADD CONSTRAINT IF NOT
  EXISTS`); the 37 `setval(seq, (SELECT MAX(id))::bigint)` calls replaced with
  `setval(seq, COALESCE((SELECT MAX(id)), 0) + 1, false)` — the old form raised
  `NULL` cast errors on empty tables (fresh installs) and skipped the next id.
  The `COALESCE ... + 1, false` form is safe on empty tables and makes the next
  generated id exactly `max + 1`.
- `misc/SqlScripts/PostgreSql/voat_users.sql` — same treatment (7 PKs guarded,
  5 FKs guarded, 8 indexes `CREATE INDEX IF NOT EXISTS`, 2 broken `setval`s
  fixed).
- `misc/SqlScripts/PostgreSql/data.sql` — closing `setval` fixed the same way.
  Its `RuleSet` inserts were already idempotent (`WHERE NOT EXISTS`).
- `misc/SqlScripts/PostgreSql/procedures.sql` — unchanged; already idempotent
  (`DROP FUNCTION IF EXISTS` + `CREATE OR REPLACE`).
- **New** `misc/SqlScripts/PostgreSql/seed.sql` — production seed, idempotent:
  the five Identity roles the code authorizes against (`GlobalAdmin`, `Admin`,
  `DelegateAdmin`, `GlobalBans`, `GlobalJanitor` — from
  `BaseAdminController`/`SpamController` `[Authorize(Roles=...)]`); an admin
  account **`admin` / `ChangeMe123!`** mapped to `GlobalAdmin` (password hash
  generated and round-trip-verified with ASP.NET Identity's `PasswordHasher` in
  IdentityV2 compatibility mode — the exact configuration `VoatUserManager`
  uses, PBKDF2-HMAC-SHA1/1000); default subverses `v/AskVoat`, `v/whatever`,
  `v/news` with front-page ordering and `admin` as moderator. The default
  password is documented inside the file with a must-change warning.
- **New** `misc/SqlScripts/PostgreSql/setup.sql` — one-command entry point
  (`psql -f setup.sql`) that `\ir`-includes the five scripts in order.
- `src/Voat.Business/Data/Extensions.cs` — added a static constructor setting
  `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)`. The
  schema uses `timestamp` (without time zone) columns and all code writes
  `DateTime.UtcNow`; Npgsql 6+ throws `Cannot write DateTime with Kind=Utc` on
  that combination. The switch restores the pre-6.0 behavior this codebase was
  built against (values written as-is, read back `Kind=Unspecified`) with zero
  schema churn. Verified the switch exists in our exact Npgsql 10.0.3 assembly
  and, empirically, that EF writes of UTC DateTimes succeed against the seeded
  database. Alternative rejected: migrating columns to `timestamptz` would
  break the many `Kind=Unspecified` writes (e.g. tests' `new DateTime(1900,1,1)`).

#### Verification (fresh PostgreSQL 18, Docker container, scripts run via psql)
- `psql -f setup.sql` on an empty database: **success** (all sequence resyncs
  return 1 on empty tables; RuleSet resync returns 11 after the 10 seeded rules).
- Second `psql -f setup.sql` run: **success, exit 0** — every object reports
  "already exists, skipping"; no errors. Set is idempotent.
- Seed contents after two runs, exact counts: 5 roles, 1 admin user, 1
  user→role mapping (admin → GlobalAdmin), 3 subverses, 3 front-page defaults,
  3 subverse moderators, 10 rulesets. No duplicates.
- Admin hash re-extracted **from the database** and verified against
  `ChangeMe123!` with the IdentityV2 `PasswordHasher`: `Success`.
- App smoke test against the seeded database: homepage **HTTP 200** with the
  seeded default subverses rendered; request logger wrote 9 `EventLog` rows
  (`DateTime.UtcNow` → `timestamp` column — the legacy-timestamp switch working);
  **admin login through the real ASP.NET Identity pipeline succeeded** (302 +
  identity cookie, logged-in header renders `admin`).

#### Notes / not done here
- The machine's local PostgreSQL 18 Windows service is still unprovisioned
  (SCRAM auth, superuser password unknown). Verification used an isolated
  Docker PostgreSQL instead — nothing on the local service was modified (an
  attempt to temporarily change `pg_hba.conf` was blocked and abandoned before
  touching the file). To provision the local service yourself: as `postgres`,
  `CREATE ROLE voat LOGIN PASSWORD 'voat' CREATEDB;` and `CREATE DATABASE voat
  OWNER voat;`, then run `psql -f misc/SqlScripts/PostgreSql/setup.sql`.
- SqlServer script variants in `misc/SqlScripts/SqlServer` are untouched legacy
  (PostgreSQL is the primary target per requirements).
- Test-harness compatibility preserved: the four script filenames, the `GO`
  batch split (verified the files contain no `GO` substring), and `{dbName}`
  replacement conventions are unchanged.

### Phase 2 — Foundation

Status: **complete.** `dotnet build Voat.sln` succeeds (0 errors), and the app
boots on .NET 10, serves requests, and reaches the database layer.

#### Tooling
- Installed .NET SDK 10.0.401 on the build machine (no .NET SDK was previously
  installed; only runtimes). Target framework for the whole solution is now the
  current LTS, .NET 10 (supported until Nov 2028). .NET 8/9 both reach end of
  support on 2026-11-10, so they were rejected.

#### Project files
- Retargeted all 11 `netcoreapp2.0` projects to `net10.0`; kept
  `netstandard2.0` for the two F# libraries and vendored MarkdownDeep.
- Removed all `.NET Core 2` / `Visual Studio 2017 Preview` references from
  README.md and project descriptions.
- Removed dead packages: `Microsoft.AspNetCore.All` (metapackage gone in 3.0;
  replaced with `<FrameworkReference Include="Microsoft.AspNetCore.App" />` in
  Voat.Business/Voat.Http, nothing needed in Voat.UI which uses the Web SDK),
  `dotnet-test-nunit`, `Microsoft.VisualStudio.Web.CodeGeneration.Tools`,
  `Microsoft.WindowsAzure.ConfigurationManager`, `NUnit` (only used under an
  never-defined `#if NUNIT`), `CoreCompat.System.Drawing.v2` (abandoned),
  `WindowsAzure.Storage`, `StackExchange.Redis.StrongName`,
  `Microsoft.AspNetCore.Identity/Authorization/Mvc/Razor` + `Microsoft.Extensions.Logging.Console/Debug`
  package refs (all in the shared framework now), `Microsoft.AspNetCore.Mvc.Core`
  (unused), `Npgsql.EntityFrameworkCore.PostgreSQL.Design` (obsolete),
  `Microsoft.Extensions.Caching.Memory` (NU1510: satisfied by shared framework),
  `System.ComponentModel.Annotations` (in-box).
- Upgraded vulnerable packages: `Newtonsoft.Json` 10.0.3 → 13.0.4 everywhere
  including the F# Voat.Common.Fs (CVE-2024-21907 / GHSA-5crp-9r3c-p9vr,
  StackOverflow DoS), `log4net` 2.0.8 → 3.5.0 (CVE-2018-1285, XXE).
- Replaced/upgraded deprecated packages: `StackExchange.Redis` 3.4.0,
  `Azure.Storage.Blobs` 12.30.1, `Dapper` 2.1.89, `Sendgrid` 9.29.3,
  `HtmlAgilityPack` 1.13.0, `SixLabors.ImageSharp` 3.1.12, `Microsoft.NET.Test.Sdk`
  18.10.1, MSTest 4.5.1, EF Core 10.0.12, Npgsql.EntityFrameworkCore.PostgreSQL
  10.0.3, `System.ComponentModel.Annotations` 5.0.0 (Voat.Validation.fsproj),
  `FSharp.Core` 4.2.3 pin removed (SDK-managed).
- `SixLabors.ImageSharp` pinned to **3.1.12 (Apache-2.0)** on purpose: 3.2+/4.x
  switched to the Six Labors Split License, which fails Release builds without a
  paid/community license key. Voat is GPL-3; Apache-2.0 avoids the issue.

#### Build fixes (file-by-file)
- `src/external/Source/toptensoftware/MarkdownDeep/NuGet.Config` — removed the
  defunct `dotnet.myget.org` feed (NU1301 broke restore of every project).
- `src/frameworks/Voat.Common.Fs/LoaderAttribute.fs`,
  `src/frameworks/Voat.Validation/ValidationHandler.fs` — re-indented `match`
  arms rejected by modern F# strict indentation. No behavior change.
- `src/external/Source/ghorsey/OpenGraph-Net/HttpDownloader.cs` — rewritten from
  obsolete `WebRequest` (SYSLIB0014 error) to `HttpClient` with automatic
  gzip/deflate; charset sniffing and meta-charset re-encode preserved. Public
  `Headers` property type changed `WebHeaderCollection` → `HttpResponseHeaders`
  (unused outside the vendored library).
- `src/Voat.Business/Imaging/ImageHandler.cs` — `Resize` ported from
  System.Drawing (Windows-only from .NET 7) to SixLabors.ImageSharp; resize
  geometry and JPEG-quality-100 output preserved. **Removed** the unused
  "Port From Gen Libs" helper region (`GetImageFormatFromString`, `GetMimeType`
  x2, `GetEncoder` x2) — nothing referenced it and its `ImageCodecInfo` type
  only exists in Windows-only `System.Drawing.Common`. `Center`/`Scale` and the
  `System.Drawing.Size` signature unchanged (System.Drawing.Primitives is
  in-box and cross-platform).
- `src/Voat.Business/IO/FileManager/AzureBlobFileManager.cs` — rewritten from
  `WindowsAzure.Storage` to `Azure.Storage.Blobs` (`BlobServiceClient`/
  `BlobClient`); same public surface, uploads overwrite as before. Removed the
  trailing dead "Original CDN Interaction code" comment block that only
  documented the retired API.
- `src/Voat.Business/Domain/UserData.cs` — qualified `Voat.Common.ActivityContext`
  (collides with new `System.Diagnostics.ActivityContext` since .NET 5).
- **New** `src/Voat.UI/Utils/ExpressionHelper.cs` — faithful port of
  `Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ExpressionHelper.GetExpressionText`
  (aspnet/Mvc rel/2.0.0, Apache-2.0, .NET Foundation) plus the value-evaluation
  half of `ExpressionMetadataProvider.FromLambdaExpression`; both types are
  internal in ASP.NET Core 3.0+ and were used by `UIExtensions.PartialFor` and
  `VoteController.TryValidateModel2`. Generated html field names stay identical.
- `src/Voat.UI/Utils/Extensions.cs`, `src/Voat.UI/Controllers/VoteController.cs` —
  swapped the removed `ViewFeatures.Internal` using for the local helper.
- `src/Voat.UI/Startup.cs` — ASP.NET Core 2.0 → 10 migration:
  `IHostingEnvironment` → `IWebHostEnvironment`; `loggerFactory.AddConsole/AddDebug`
  (obsolete) → `AddLogging`-scoped `loggingBuilder.AddConsole()/AddDebug()`;
  dropped `app.UseDatabaseErrorPage()` (removed in .NET 5+, EF-migrations
  diagnostics; Voat runs on hand-written SQL, `UseDeveloperExceptionPage`
  remains); `app.UseIdentity()` → `app.UseAuthentication()` +
  `app.UseAuthorization()`; `AddMvc` now sets `MvcOptions.EnableEndpointRouting
  = false` so the legacy `UseMvc` + `IRouteBuilder` RouteConfig (600+ lines of
  routes) keeps working unchanged.
- `src/Voat.UI/Views/Home/submit.cshtml` — modern Razor rejects `@class` in
  anonymous objects inside code blocks (RZ1007); replaced with
  `Dictionary<string, object>` which renders identical html attributes.
- `src/Voat.UI/Views/Shared/DisplayTemplates/IPaginatedList.cshtml` — literal
  `< prev` link text broke the Razor parser (RZ1024); HTML-encoded as
  `&lt; prev` (renders the same).
- `test/Voat.Tests/AssemblyInit.cs` — NUnit-only attributes
  (`SetUpFixture`/`OneTimeSetUp`/`OneTimeTearDown`) replaced with MSTest
  `[AssemblyInitialize]`/`[AssemblyCleanup]` (teardown body is empty).
- `test/Voat.Tests/_Infrastructure/Base/NUnit/Shims.cs` — MSTest 4 removed
  `Assert.ThrowsException(Async)`; `VoatAssert` now forwards to
  `Assert.ThrowsExactly(Async)` (same exact-type semantics).
- `test/Voat.Tests/Command/CommentCommandTests.cs` — two `Assert.IsNotNull`
  messages converted to interpolated strings (MSTest 4 requirement); message
  content unchanged.
- `test/Voat.Validation.Tests/FunctionalTests.cs`,
  `test/Voat.Validation.Tests/ValidationLoaderTests.cs` — `[ExpectedException]`
  (removed in MSTest 4) converted to `Assert.ThrowsExactly` around the test body.

#### Configuration
- Created `src/Voat.UI/appsettings.json` from the `~appsettings.json` template
  for local runs: localhost PostgreSQL connection strings, **Memory cache
  instead of Redis** (the Redis handler connects eagerly and would prevent
  startup without a server), Smtp email handler instead of SendGrid placeholder,
  empty `cookieDomain`. This file is local-only; Phase 6 moves all
  secrets/environment-specific values to environment variables and adds
  `.env.example` + gitignore rules.

#### Verification
- `dotnet build Voat.sln`: **0 errors**. ~300 warnings remain, all pre-existing
  legacy-code warnings (CS4014 unawaited calls, CA2200 rethrows, CS0162
  unreachable code, etc.) — deliberately not churned in this phase.
- Smoke test: app starts under `dotnet run`, Kestrel listens, and a page request
  travels the full pipeline (routing → controller → Razor → EF Core/Npgsql).
  With placeholder DB credentials the request fails with
  `Npgsql.PostgresException 28P01: password authentication failed for user
  "voat"` — i.e. the only remaining gap is database provisioning (Phase 3).

### Phase 1 — Audit
- No code changes. Full read-only audit produced (structure, dependencies,
  database schema, external services, breakage inventory, 16 security findings,
  risk-ranked plan). Findings drive Phases 2–6.
