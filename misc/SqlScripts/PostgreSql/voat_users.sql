BEGIN;

CREATE TABLE "AspNetRoleClaims"( 
	"Id" int NOT NULL,
	"ClaimType" varchar(256),
	"ClaimValue" varchar(256),
	"RoleId" varchar(128) NOT NULL);

CREATE TABLE "AspNetRoles"( 
	"Id" varchar(128) NOT NULL,
	"ConcurrencyStamp" varchar(50),
	"Name" varchar(256) NOT NULL,
	"NormalizedName" varchar(256) NOT NULL);

CREATE TABLE "AspNetUserClaims"( 
	"Id" int NOT NULL,
	"ClaimType" varchar,
	"ClaimValue" varchar,
	"UserId" varchar(128) NOT NULL);

CREATE TABLE "AspNetUserLogins"( 
	"UserId" varchar(128) NOT NULL,
	"LoginProvider" varchar(128) NOT NULL,
	"ProviderKey" varchar(128) NOT NULL,
	"ProviderDisplayName" varchar(500) NOT NULL);

CREATE TABLE "AspNetUserRoles"( 
	"UserId" varchar(128) NOT NULL,
	"RoleId" varchar(128) NOT NULL);

CREATE TABLE "AspNetUserTokens"( 
	"UserId" varchar(128) NOT NULL,
	"LoginProvider" varchar(128) NOT NULL,
	"Name" varchar(128) NOT NULL,
	"Value" varchar);

CREATE TABLE "AspNetUsers"( 
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


CREATE SEQUENCE "aspnetroleclaims_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "AspNetRoleClaims"."Id";
CREATE SEQUENCE "aspnetuserclaims_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "AspNetUserClaims"."Id";
ALTER TABLE "AspNetRoleClaims" ADD CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id");
ALTER TABLE "AspNetRoles" ADD CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id");
ALTER TABLE "AspNetUserClaims" ADD CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id");
ALTER TABLE "AspNetUserLogins" ADD CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("UserId","LoginProvider","ProviderKey");
ALTER TABLE "AspNetUserRoles" ADD CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId","RoleId");
ALTER TABLE "AspNetUserTokens" ADD CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId","LoginProvider","Name");
ALTER TABLE "AspNetUsers" ADD CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id");
CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId" ASC);
CREATE INDEX "IX_AspNetRoles_NormalizedName" ON "AspNetRoles" ("NormalizedName" ASC);
CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId" ASC);
CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId" ASC);
CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId" ASC);
CREATE INDEX "IX_AspNetUserRoles_UserId" ON "AspNetUserRoles" ("UserId" ASC);
CREATE INDEX "IX_AspNetUsers_NormalizedEmail" ON "AspNetUsers" ("NormalizedEmail" ASC);
CREATE INDEX "IX_AspNetUsers_NormalizedUserName" ON "AspNetUsers" ("NormalizedUserName" ASC);
ALTER TABLE "AspNetRoleClaims" ADD CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ( "Id") ON DELETE CASCADE;
ALTER TABLE "AspNetUserClaims" ADD CONSTRAINT "FK_AspNetUserClaims_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ( "Id") ON DELETE CASCADE;
ALTER TABLE "AspNetUserLogins" ADD CONSTRAINT "FK_AspNetUserLogins_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ( "Id") ON DELETE CASCADE;
ALTER TABLE "AspNetUserRoles" ADD CONSTRAINT "FK_AspNetUserRoles_AspNetRoles" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ( "Id") ON DELETE CASCADE;
ALTER TABLE "AspNetUserRoles" ADD CONSTRAINT "FK_AspNetUserRoles_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ( "Id") ON DELETE CASCADE;
ALTER TABLE "AspNetRoleClaims" ALTER COLUMN "Id" SET DEFAULT nextval('"aspnetroleclaims_id_seq"');
ALTER TABLE "AspNetUserClaims" ALTER COLUMN "Id" SET DEFAULT nextval('"aspnetuserclaims_id_seq"');
ALTER TABLE "AspNetUsers" ALTER COLUMN "RegistrationDateTime" SET DEFAULT (now() at time zone 'utc');
select setval('"aspnetroleclaims_id_seq"',(select max("Id") from "AspNetRoleClaims")::bigint);
select setval('"aspnetuserclaims_id_seq"',(select max("Id") from "AspNetUserClaims")::bigint);

COMMIT;
