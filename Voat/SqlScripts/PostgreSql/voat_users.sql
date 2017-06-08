BEGIN;
CREATE SCHEMA IF NOT EXISTS "dbo";
ALTER DATABASE {dbName} SET search_path TO dbo;

CREATE TABLE "dbo"."AspNetRoleClaims"( 
	"Id" int NOT NULL,
	"ClaimType" varchar(256),
	"ClaimValue" varchar(256),
	"RoleId" varchar(128) NOT NULL);

CREATE TABLE "dbo"."AspNetRoles"( 
	"Id" varchar(128) NOT NULL,
	"ConcurrencyStamp" varchar(50),
	"Name" varchar(256),
	"NormalizedName" varchar(256));

CREATE TABLE "dbo"."AspNetUserClaims"( 
	"Id" int NOT NULL,
	"ClaimType" varchar,
	"ClaimValue" varchar,
	"UserId" varchar(128) NOT NULL);

CREATE TABLE "dbo"."AspNetUserLogins"( 
	"UserId" varchar(128) NOT NULL,
	"LoginProvider" varchar(128) NOT NULL,
	"ProviderKey" varchar(128) NOT NULL,
	"ProviderDisplayName" varchar(500) NOT NULL);

CREATE TABLE "dbo"."AspNetUserRoles"( 
	"UserId" varchar(128) NOT NULL,
	"RoleId" varchar(128) NOT NULL);

CREATE TABLE "dbo"."AspNetUserTokens"( 
	"UserId" varchar(128) NOT NULL,
	"LoginProvider" varchar(128) NOT NULL,
	"Name" varchar(128) NOT NULL,
	"Value" varchar);

CREATE TABLE "dbo"."AspNetUsers"( 
	"Id" varchar(128) NOT NULL,
	"UserName" varchar(256) NOT NULL,
	"PasswordHash" varchar,
	"SecurityStamp" varchar,
	"Email" varchar(256),
	"EmailConfirmed" boolean NOT NULL,
	"PhoneNumber" varchar(20),
	"PhoneNumberConfirmed" boolean NOT NULL,
	"TwoFactorEnabled" boolean NOT NULL,
	"LockoutEnd" timestamp,
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
