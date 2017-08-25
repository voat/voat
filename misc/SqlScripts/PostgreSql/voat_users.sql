BEGIN;
CREATE EXTENSION IF NOT EXISTS citext;
CREATE SCHEMA IF NOT EXISTS "dbo";
ALTER DATABASE {dbName} SET search_path TO dbo;

CREATE TABLE "dbo"."AspNetRoleClaims"(
	"Id" int NOT NULL,
	"ClaimType" citext CHECK (char_length("ClaimType") <= 256),
	"ClaimValue" citext CHECK (char_length("ClaimValue") <= 256),
	"RoleId" citext NOT NULL CHECK (char_length("RoleId") <= 128));

CREATE TABLE "dbo"."AspNetRoles"(
	"Id" citext NOT NULL CHECK (char_length("Id") <= 128),
	"ConcurrencyStamp" citext CHECK (char_length("ConcurrencyStamp") <= 50),
	"Name" citext CHECK (char_length("Name") <= 256),
	"NormalizedName" citext CHECK (char_length("NormalizedName") <= 256));

CREATE TABLE "dbo"."AspNetUserClaims"(
	"Id" int NOT NULL,
	"ClaimType" citext,
	"ClaimValue" citext,
	"UserId" citext NOT NULL CHECK (char_length("UserId") <= 128));

CREATE TABLE "dbo"."AspNetUserLogins"(
	"UserId" citext NOT NULL CHECK (char_length("UserId") <= 128),
	"LoginProvider" citext NOT NULL CHECK (char_length("LoginProvider") <= 128),
	"ProviderKey" citext NOT NULL CHECK (char_length("ProviderKey") <= 128),
	"ProviderDisplayName" citext NOT NULL CHECK (char_length("ProviderDisplayName") <= 500));

CREATE TABLE "dbo"."AspNetUserRoles"(
	"UserId" citext NOT NULL CHECK (char_length("UserId") <= 128),
	"RoleId" citext NOT NULL CHECK (char_length("RoleId") <= 128));

CREATE TABLE "dbo"."AspNetUserTokens"(
	"UserId" citext NOT NULL CHECK (char_length("UserId") <= 128),
	"LoginProvider" citext NOT NULL CHECK (char_length("LoginProvider") <= 128),
	"Name" citext NOT NULL CHECK (char_length("Name") <= 128),
	"Value" citext);

CREATE TABLE "dbo"."AspNetUsers"(
	"Id" citext NOT NULL CHECK (char_length("Id") <= 128),
	"UserName" citext NOT NULL CHECK (char_length("UserName") <= 256),
	"PasswordHash" citext,
	"SecurityStamp" citext,
	"Email" citext CHECK (char_length("Email") <= 256),
	"EmailConfirmed" boolean NOT NULL,
	"PhoneNumber" citext CHECK (char_length("PhoneNumber") <= 20),
	"PhoneNumberConfirmed" boolean NOT NULL,
	"TwoFactorEnabled" boolean NOT NULL,
	"LockoutEnd" timestamp with time zone,
	"LockoutEnabled" boolean NOT NULL,
	"AccessFailedCount" int,
	"RegistrationDateTime" timestamp NOT NULL,
	"RecoveryQuestion" citext CHECK (char_length("RecoveryQuestion") <= 50),
	"Answer" citext CHECK (char_length("Answer") <= 50),
	"LastLoginFromIp" citext CHECK (char_length("LastLoginFromIp") <= 50),
	"LastLoginDateTime" timestamp NOT NULL,
	"ConcurrencyStamp" citext CHECK (char_length("ConcurrencyStamp") <= 50),
	"NormalizedEmail" citext CHECK (char_length("NormalizedEmail") <= 256),
	"NormalizedUserName" citext CHECK (char_length("NormalizedUserName") <= 256));


CREATE SEQUENCE "dbo"."aspnetroleclaims_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."AspNetRoleClaims"."Id";
CREATE SEQUENCE "dbo"."aspnetuserclaims_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."AspNetUserClaims"."Id";
ALTER TABLE "dbo"."AspNetRoleClaims" ADD CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id");
ALTER TABLE "dbo"."AspNetRoles" ADD CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id");
ALTER TABLE "dbo"."AspNetUserClaims" ADD CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id");
ALTER TABLE "dbo"."AspNetUserLogins" ADD CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("UserId","LoginProvider","ProviderKey");
ALTER TABLE "dbo"."AspNetUserRoles" ADD CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId","RoleId");
ALTER TABLE "dbo"."AspNetUserTokens" ADD CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId","LoginProvider","Name");
ALTER TABLE "dbo"."AspNetUsers" ADD CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id");
CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "dbo"."AspNetRoleClaims" ("RoleId" ASC);
CREATE INDEX "RoleNameIndex" ON "dbo"."AspNetRoles" ("NormalizedName" ASC);
CREATE INDEX "IX_AspNetUserClaims_UserId" ON "dbo"."AspNetUserClaims" ("UserId" ASC);
CREATE INDEX "IX_AspNetUserLogins_UserId" ON "dbo"."AspNetUserLogins" ("UserId" ASC);
CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "dbo"."AspNetUserRoles" ("RoleId" ASC);
CREATE INDEX "IX_AspNetUserRoles_UserId" ON "dbo"."AspNetUserRoles" ("UserId" ASC);
CREATE INDEX "IX_AspNetUsers_NormalizedEmail" ON "dbo"."AspNetUsers" ("NormalizedEmail" ASC);
CREATE INDEX "IX_AspNetUsers_NormalizedUserName" ON "dbo"."AspNetUsers" ("NormalizedUserName" ASC);
ALTER TABLE "dbo"."AspNetRoleClaims" ADD CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles" FOREIGN KEY ("RoleId") REFERENCES "dbo"."AspNetRoles" ( "Id") ON DELETE CASCADE;
ALTER TABLE "dbo"."AspNetUserClaims" ADD CONSTRAINT "FK_AspNetUserClaims_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES "dbo"."AspNetUsers" ( "Id") ON DELETE CASCADE;
ALTER TABLE "dbo"."AspNetUserLogins" ADD CONSTRAINT "FK_AspNetUserLogins_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES "dbo"."AspNetUsers" ( "Id") ON DELETE CASCADE;
ALTER TABLE "dbo"."AspNetUserRoles" ADD CONSTRAINT "FK_AspNetUserRoles_AspNetRoles" FOREIGN KEY ("RoleId") REFERENCES "dbo"."AspNetRoles" ( "Id") ON DELETE CASCADE;
ALTER TABLE "dbo"."AspNetUserRoles" ADD CONSTRAINT "FK_AspNetUserRoles_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES "dbo"."AspNetUsers" ( "Id") ON DELETE CASCADE;
ALTER TABLE "dbo"."AspNetRoleClaims" ALTER COLUMN "Id" SET DEFAULT nextval('"dbo"."aspnetroleclaims_id_seq"');
ALTER TABLE "dbo"."AspNetUserClaims" ALTER COLUMN "Id" SET DEFAULT nextval('"dbo"."aspnetuserclaims_id_seq"');
ALTER TABLE "dbo"."AspNetUsers" ALTER COLUMN "RegistrationDateTime" SET DEFAULT (now() at time zone 'utc');
select setval('"dbo"."aspnetroleclaims_id_seq"',(select max("Id") from "dbo"."AspNetRoleClaims")::bigint);
select setval('"dbo"."aspnetuserclaims_id_seq"',(select max("Id") from "dbo"."AspNetUserClaims")::bigint);

COMMIT;
