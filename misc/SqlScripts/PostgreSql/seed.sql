-- Voat seed data (PostgreSQL).
--
-- Creates the initial content a running site needs after schema setup:
--   * the Identity roles referenced by [Authorize(Roles = ...)] in the code
--   * an administrator account "admin" (role GlobalAdmin)
--   * the default subverses and their front-page ordering
--
-- ####################################################################################################
-- #  DEFAULT ADMIN CREDENTIALS:  user "admin" / password "ChangeMe123!"                              #
-- #  The password hash below was generated with ASP.NET Identity V3 (the mode Voat's                 #
-- #  VoatUserManager uses: PBKDF2-HMAC-SHA256, 100000 iterations).                                   #
-- #  CHANGE THE PASSWORD IMMEDIATELY AFTER FIRST LOGIN (Settings -> Change password).                #
-- ####################################################################################################
--
-- Idempotent: every statement is guarded and safe to re-run.

BEGIN;

-- Identity roles used by admin area authorization attributes.
INSERT INTO "AspNetRoles" ("Id", "ConcurrencyStamp", "Name", "NormalizedName")
SELECT 'e5e75a28-2138-4c1e-8e21-2b9390df6cd1', '2c1f0a44-9f4c-4b1f-8d1e-4a2b6c8d0e11', 'GlobalAdmin', 'GLOBALADMIN'
WHERE NOT EXISTS (SELECT 1 FROM "AspNetRoles" WHERE "NormalizedName" = 'GLOBALADMIN');

INSERT INTO "AspNetRoles" ("Id", "ConcurrencyStamp", "Name", "NormalizedName")
SELECT 'b6a43c67-60b5-4f1e-b365-74d8ce78a7f4', '5d2e1b55-8e3d-4c2e-9f2f-5b3c7d9e1f22', 'Admin', 'ADMIN'
WHERE NOT EXISTS (SELECT 1 FROM "AspNetRoles" WHERE "NormalizedName" = 'ADMIN');

INSERT INTO "AspNetRoles" ("Id", "ConcurrencyStamp", "Name", "NormalizedName")
SELECT '7c6d1d7c-1e0d-4a1c-9b45-4b0e5b21a883', '6e3f2c66-7d2c-4d3f-8e3e-6c4d8e0f2a33', 'DelegateAdmin', 'DELEGATEADMIN'
WHERE NOT EXISTS (SELECT 1 FROM "AspNetRoles" WHERE "NormalizedName" = 'DELEGATEADMIN');

INSERT INTO "AspNetRoles" ("Id", "ConcurrencyStamp", "Name", "NormalizedName")
SELECT '9a6a5f42-8a53-43fe-98ee-b38a44cb1f2b', '7f4a3d77-6c1b-4e4a-9d4d-7d5e9f1a3b44', 'GlobalBans', 'GLOBALBANS'
WHERE NOT EXISTS (SELECT 1 FROM "AspNetRoles" WHERE "NormalizedName" = 'GLOBALBANS');

INSERT INTO "AspNetRoles" ("Id", "ConcurrencyStamp", "Name", "NormalizedName")
SELECT '2f6f03d6-6b59-4e8a-b601-2c7d9e388cf5', '8a5b4e88-5b0a-4f5b-8c5c-8e6fa02b4c55', 'GlobalJanitor', 'GLOBALJANITOR'
WHERE NOT EXISTS (SELECT 1 FROM "AspNetRoles" WHERE "NormalizedName" = 'GLOBALJANITOR');

-- Administrator account. Password hash = Identity V3 hash of "ChangeMe123!".
INSERT INTO "AspNetUsers" ("Id", "UserName", "NormalizedUserName", "PasswordHash", "SecurityStamp",
    "Email", "EmailConfirmed", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
    "RegistrationDateTime", "LastLoginDateTime", "LastLoginFromIp", "ConcurrencyStamp")
SELECT 'c1e8b7d2-3f4a-4c5b-8e9f-0a1b2c3d4e5f', 'admin', 'ADMIN',
    'AQAAAAIAAYagAAAAEEFfgI5XBR3d8cQ7uPVnREfGgyVHM81gFonshSS7tuaq1EZoBZqTkWet/QLS55nt3g==',
    '3b7d9f11-2c44-4a5e-9c66-1d2e3f4a5b6c',
    NULL, FALSE, FALSE, FALSE, TRUE, 0,
    now() at time zone 'utc', timestamp '1900-01-01 00:00:00', '0.0.0.0',
    '4c8ea022-1d55-4b6f-8d77-2e3f4a5b6c7d'
WHERE NOT EXISTS (SELECT 1 FROM "AspNetUsers" WHERE "NormalizedUserName" = 'ADMIN');

-- admin -> GlobalAdmin
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT 'c1e8b7d2-3f4a-4c5b-8e9f-0a1b2c3d4e5f', 'e5e75a28-2138-4c1e-8e21-2b9390df6cd1'
WHERE NOT EXISTS (SELECT 1 FROM "AspNetUserRoles" WHERE "UserId" = 'c1e8b7d2-3f4a-4c5b-8e9f-0a1b2c3d4e5f' AND "RoleId" = 'e5e75a28-2138-4c1e-8e21-2b9390df6cd1');

-- Default subverses (same set the unit tests treat as the site defaults).
INSERT INTO "Subverse" ("Name", "Title", "Description", "SideBar", "CreationDate", "CreatedBy")
SELECT 'askvoat', 'v/AskVoat', 'Ask Voat.', 'Ask me anything.', now() at time zone 'utc', 'admin'
WHERE NOT EXISTS (SELECT 1 FROM "Subverse" WHERE "Name" = 'askvoat');

INSERT INTO "Subverse" ("Name", "Title", "Description", "SideBar", "CreationDate", "CreatedBy")
SELECT 'whatever', 'v/whatever', 'Whatever', 'Whatever goes here.', now() at time zone 'utc', 'admin'
WHERE NOT EXISTS (SELECT 1 FROM "Subverse" WHERE "Name" = 'whatever');

INSERT INTO "Subverse" ("Name", "Title", "Description", "SideBar", "CreationDate", "CreatedBy")
SELECT 'news', 'v/news', 'News', 'News.', now() at time zone 'utc', 'admin'
WHERE NOT EXISTS (SELECT 1 FROM "Subverse" WHERE "Name" = 'news');

-- Front page ordering.
INSERT INTO "DefaultSubverse" ("Subverse", "Order")
SELECT 'askvoat', 1 WHERE NOT EXISTS (SELECT 1 FROM "DefaultSubverse" WHERE "Subverse" = 'askvoat');

INSERT INTO "DefaultSubverse" ("Subverse", "Order")
SELECT 'whatever', 2 WHERE NOT EXISTS (SELECT 1 FROM "DefaultSubverse" WHERE "Subverse" = 'whatever');

INSERT INTO "DefaultSubverse" ("Subverse", "Order")
SELECT 'news', 3 WHERE NOT EXISTS (SELECT 1 FROM "DefaultSubverse" WHERE "Subverse" = 'news');

-- admin moderates the seeded subverses.
INSERT INTO "SubverseModerator" ("Subverse", "UserName", "Power", "CreatedBy", "CreationDate")
SELECT 'askvoat', 'admin', 1, 'admin', now() at time zone 'utc'
WHERE NOT EXISTS (SELECT 1 FROM "SubverseModerator" WHERE "Subverse" = 'askvoat' AND "UserName" = 'admin');

INSERT INTO "SubverseModerator" ("Subverse", "UserName", "Power", "CreatedBy", "CreationDate")
SELECT 'whatever', 'admin', 1, 'admin', now() at time zone 'utc'
WHERE NOT EXISTS (SELECT 1 FROM "SubverseModerator" WHERE "Subverse" = 'whatever' AND "UserName" = 'admin');

INSERT INTO "SubverseModerator" ("Subverse", "UserName", "Power", "CreatedBy", "CreationDate")
SELECT 'news', 'admin', 1, 'admin', now() at time zone 'utc'
WHERE NOT EXISTS (SELECT 1 FROM "SubverseModerator" WHERE "Subverse" = 'news' AND "UserName" = 'admin');

COMMIT;
