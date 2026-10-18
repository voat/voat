BEGIN;

CREATE TABLE IF NOT EXISTS "AspNetRoleClaims"( 
	"Id" int NOT NULL,
	"ClaimType" varchar(256),
	"ClaimValue" varchar(256),
	"RoleId" varchar(128) NOT NULL);

CREATE TABLE IF NOT EXISTS "AspNetRoles"( 
	"Id" varchar(128) NOT NULL,
	"ConcurrencyStamp" varchar(50),
	"Name" varchar(256) NOT NULL,
	"NormalizedName" varchar(256) NOT NULL);

CREATE TABLE IF NOT EXISTS "AspNetUserClaims"( 
	"Id" int NOT NULL,
	"ClaimType" varchar,
	"ClaimValue" varchar,
	"UserId" varchar(128) NOT NULL);

CREATE TABLE IF NOT EXISTS "AspNetUserLogins"( 
	"UserId" varchar(128) NOT NULL,
	"LoginProvider" varchar(128) NOT NULL,
	"ProviderKey" varchar(128) NOT NULL,
	"ProviderDisplayName" varchar(500) NOT NULL);

CREATE TABLE IF NOT EXISTS "AspNetUserRoles"( 
	"UserId" varchar(128) NOT NULL,
	"RoleId" varchar(128) NOT NULL);

CREATE TABLE IF NOT EXISTS "AspNetUserTokens"( 
	"UserId" varchar(128) NOT NULL,
	"LoginProvider" varchar(128) NOT NULL,
	"Name" varchar(128) NOT NULL,
	"Value" varchar);

CREATE TABLE IF NOT EXISTS "AspNetUsers"( 
	"Id" varchar(128) NOT NULL,
	"UserName" varchar(256) NOT NULL,
	"PasswordHash" varchar,
	"SecurityStamp" varchar,
	"Email" varchar(256),
	"EmailConfirmed" boolean NOT NULL,
	"PhoneNumber" varchar(20),
	"PhoneNumberConfirmed" boolean NOT NULL,
	"TwoFactorEnabled" boolean NOT NULL,
	"LockoutEnd" timestamp with time zone,
	"LockoutEnabled" boolean NOT NULL,
	"AccessFailedCount" int,
	"RegistrationDateTime" timestamp NOT NULL,
	"RecoveryQuestion" varchar(50),
	"Answer" varchar(50),
	"LastLoginFromIp" varchar(50),
	"LastLoginDateTime" timestamp NOT NULL,
	"ConcurrencyStamp" varchar(50),
	"NormalizedEmail" varchar(256),
	"NormalizedUserName" varchar(256));


CREATE SEQUENCE IF NOT EXISTS "aspnetroleclaims_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "AspNetRoleClaims"."Id";
CREATE SEQUENCE IF NOT EXISTS "aspnetuserclaims_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "AspNetUserClaims"."Id";
-- Constraints are only added when missing so this script is safe to re-run.
DO $$
BEGIN
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_AspNetRoleClaims') THEN ALTER TABLE "AspNetRoleClaims" ADD CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_AspNetRoles') THEN ALTER TABLE "AspNetRoles" ADD CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_AspNetUserClaims') THEN ALTER TABLE "AspNetUserClaims" ADD CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_AspNetUserLogins') THEN ALTER TABLE "AspNetUserLogins" ADD CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("UserId","LoginProvider","ProviderKey"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_AspNetUserRoles') THEN ALTER TABLE "AspNetUserRoles" ADD CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId","RoleId"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_AspNetUserTokens') THEN ALTER TABLE "AspNetUserTokens" ADD CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId","LoginProvider","Name"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_AspNetUsers') THEN ALTER TABLE "AspNetUsers" ADD CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_AspNetRoleClaims_AspNetRoles') THEN ALTER TABLE "AspNetRoleClaims" ADD CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ( "Id") ON DELETE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_AspNetUserClaims_AspNetUsers') THEN ALTER TABLE "AspNetUserClaims" ADD CONSTRAINT "FK_AspNetUserClaims_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ( "Id") ON DELETE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_AspNetUserLogins_AspNetUsers') THEN ALTER TABLE "AspNetUserLogins" ADD CONSTRAINT "FK_AspNetUserLogins_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ( "Id") ON DELETE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_AspNetUserRoles_AspNetRoles') THEN ALTER TABLE "AspNetUserRoles" ADD CONSTRAINT "FK_AspNetUserRoles_AspNetRoles" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ( "Id") ON DELETE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_AspNetUserRoles_AspNetUsers') THEN ALTER TABLE "AspNetUserRoles" ADD CONSTRAINT "FK_AspNetUserRoles_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ( "Id") ON DELETE CASCADE; END IF;
END
$$;
CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId" ASC);
CREATE INDEX IF NOT EXISTS "IX_AspNetRoles_NormalizedName" ON "AspNetRoles" ("NormalizedName" ASC);
CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId" ASC);
CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId" ASC);
CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId" ASC);
CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_UserId" ON "AspNetUserRoles" ("UserId" ASC);
CREATE INDEX IF NOT EXISTS "IX_AspNetUsers_NormalizedEmail" ON "AspNetUsers" ("NormalizedEmail" ASC);
CREATE INDEX IF NOT EXISTS "IX_AspNetUsers_NormalizedUserName" ON "AspNetUsers" ("NormalizedUserName" ASC);
ALTER TABLE "AspNetRoleClaims" ALTER COLUMN "Id" SET DEFAULT nextval('"aspnetroleclaims_id_seq"');
ALTER TABLE "AspNetUserClaims" ALTER COLUMN "Id" SET DEFAULT nextval('"aspnetuserclaims_id_seq"');
ALTER TABLE "AspNetUsers" ALTER COLUMN "RegistrationDateTime" SET DEFAULT (now() at time zone 'utc');
-- Resynchronize sequences with table contents (safe on empty tables).
select setval('"aspnetroleclaims_id_seq"', COALESCE((select max("Id") from "AspNetRoleClaims"), 0) + 1, false);
select setval('"aspnetuserclaims_id_seq"', COALESCE((select max("Id") from "AspNetUserClaims"), 0) + 1, false);

COMMIT;
