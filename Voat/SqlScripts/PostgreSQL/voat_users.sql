BEGIN;
CREATE SCHEMA IF NOT EXISTS "dbo";
ALTER DATABASE voatusers SET search_path TO dbo;

CREATE TABLE "dbo"."AspNetRoles"(
	"Id" varchar(128) NOT NULL,
	"Name" varchar NOT NULL);

CREATE TABLE "dbo"."AspNetUserClaims"(
	"Id" int NOT NULL,
	"ClaimType" varchar,
	"ClaimValue" varchar,
	"UserId" varchar(128) NOT NULL);

CREATE TABLE "dbo"."AspNetUserLogins"(
	"UserId" varchar(128) NOT NULL,
	"LoginProvider" varchar(128) NOT NULL,
	"ProviderKey" varchar(128) NOT NULL);

CREATE TABLE "dbo"."AspNetUserRoles"(
	"UserId" varchar(128) NOT NULL,
	"RoleId" varchar(128) NOT NULL);

CREATE TABLE "dbo"."AspNetUsers"(
	"Id" varchar(128) NOT NULL,
	"UserName" varchar(800) NOT NULL,
	"PasswordHash" varchar,
	"SecurityStamp" varchar,
	"Email" varchar,
	"IsConfirmed" boolean,
	"EmailConfirmed" boolean,
	"PhoneNumber" char(10),
	"PhoneNumberConfirmed" boolean,
	"TwoFactorEnabled" boolean,
	"LockoutEndDateUtc" timestamp,
	"LockoutEnabled" boolean,
	"AccessFailedCount" int,
	"RegistrationDateTime" timestamp NOT NULL,
	"RecoveryQuestion" varchar(50),
	"Answer" varchar(50),
	"Partner" boolean NOT NULL,
	"LastLoginFromIp" varchar(50),
	"LastLoginDateTime" timestamp NOT NULL);

CREATE TABLE "dbo"."Sessions"(
	"SessionId" varchar(88) NOT NULL,
	"Created" timestamp NOT NULL,
	"Expires" timestamp NOT NULL,
	"LockDate" timestamp NOT NULL,
	"LockCookie" int NOT NULL,
	"Locked" boolean NOT NULL,
	"SessionItem" bytea,
	"Flags" int NOT NULL,
	"Timeout" int NOT NULL);

CREATE TABLE "dbo"."aspnet_Applications"(
	"ApplicationName" varchar(256) NOT NULL,
	"LoweredApplicationName" varchar(256) NOT NULL,
	"ApplicationId" uuid NOT NULL,
	"Description" varchar(256));

CREATE TABLE "dbo"."aspnet_Membership"(
	"ApplicationId" uuid NOT NULL,
	"UserId" uuid NOT NULL,
	"Password" varchar(128) NOT NULL,
	"PasswordFormat" int NOT NULL,
	"PasswordSalt" varchar(128) NOT NULL,
	"MobilePIN" varchar(16),
	"Email" varchar(256),
	"LoweredEmail" varchar(256),
	"PasswordQuestion" varchar(256),
	"PasswordAnswer" varchar(128),
	"IsApproved" boolean NOT NULL,
	"IsLockedOut" boolean NOT NULL,
	"CreateDate" timestamp NOT NULL,
	"LastLoginDate" timestamp NOT NULL,
	"LastPasswordChangedDate" timestamp NOT NULL,
	"LastLockoutDate" timestamp NOT NULL,
	"FailedPasswordAttemptCount" int NOT NULL,
	"FailedPasswordAttemptWindowStart" timestamp NOT NULL,
	"FailedPasswordAnswerAttemptCount" int NOT NULL,
	"FailedPasswordAnswerAttemptWindowStart" timestamp NOT NULL,
	"Comment" text);

CREATE TABLE "dbo"."aspnet_Paths"(
	"ApplicationId" uuid NOT NULL,
	"PathId" uuid NOT NULL,
	"Path" varchar(256) NOT NULL,
	"LoweredPath" varchar(256) NOT NULL);

CREATE TABLE "dbo"."aspnet_PersonalizationAllUsers"(
	"PathId" uuid NOT NULL,
	"PageSettings" bytea NOT NULL,
	"LastUpdatedDate" timestamp NOT NULL);

CREATE TABLE "dbo"."aspnet_PersonalizationPerUser"(
	"Id" uuid NOT NULL,
	"PathId" uuid,
	"UserId" uuid,
	"PageSettings" bytea NOT NULL,
	"LastUpdatedDate" timestamp NOT NULL);

CREATE TABLE "dbo"."aspnet_Profile"(
	"UserId" uuid NOT NULL,
	"PropertyNames" text NOT NULL,
	"PropertyValuesString" text NOT NULL,
	"PropertyValuesBinary" bytea NOT NULL,
	"LastUpdatedDate" timestamp NOT NULL);

CREATE TABLE "dbo"."aspnet_Roles"(
	"ApplicationId" uuid NOT NULL,
	"RoleId" uuid NOT NULL,
	"RoleName" varchar(256) NOT NULL,
	"LoweredRoleName" varchar(256) NOT NULL,
	"Description" varchar(256));

CREATE TABLE "dbo"."aspnet_SchemaVersions"(
	"Feature" varchar(128) NOT NULL,
	"CompatibleSchemaVersion" varchar(128) NOT NULL,
	"IsCurrentVersion" boolean NOT NULL);

CREATE TABLE "dbo"."aspnet_Users"(
	"ApplicationId" uuid NOT NULL,
	"UserId" uuid NOT NULL,
	"UserName" varchar(256) NOT NULL,
	"LoweredUserName" varchar(256) NOT NULL,
	"MobileAlias" varchar(16),
	"IsAnonymous" boolean NOT NULL,
	"LastActivityDate" timestamp NOT NULL);

CREATE TABLE "dbo"."aspnet_UsersInRoles"(
	"UserId" uuid NOT NULL,
	"RoleId" uuid NOT NULL);

CREATE TABLE "dbo"."aspnet_WebEvent_Events"(
	"EventId" char(32) NOT NULL,
	"EventTimeUtc" timestamp NOT NULL,
	"EventTime" timestamp NOT NULL,
	"EventType" varchar(256) NOT NULL,
	"EventSequence" numeric(19, 0) NOT NULL,
	"EventOccurrence" numeric(19, 0) NOT NULL,
	"EventCode" int NOT NULL,
	"EventDetailCode" int NOT NULL,
	"Message" varchar(1024),
	"ApplicationPath" varchar(256),
	"ApplicationVirtualPath" varchar(256),
	"MachineName" varchar(256) NOT NULL,
	"RequestUrl" varchar(1024),
	"ExceptionType" varchar(256),
	"Details" text);

CREATE TABLE "dbo"."vw_aspnet_Applications"(
	"ApplicationName" varchar(256) NOT NULL,
	"LoweredApplicationName" varchar(256) NOT NULL,
	"ApplicationId" uuid NOT NULL,
	"Description" varchar(256));

CREATE TABLE "dbo"."vw_aspnet_MembershipUsers"(
	"UserId" uuid NOT NULL,
	"PasswordFormat" int NOT NULL,
	"MobilePIN" varchar(16),
	"Email" varchar(256),
	"LoweredEmail" varchar(256),
	"PasswordQuestion" varchar(256),
	"PasswordAnswer" varchar(128),
	"IsApproved" boolean NOT NULL,
	"IsLockedOut" boolean NOT NULL,
	"CreateDate" timestamp NOT NULL,
	"LastLoginDate" timestamp NOT NULL,
	"LastPasswordChangedDate" timestamp NOT NULL,
	"LastLockoutDate" timestamp NOT NULL,
	"FailedPasswordAttemptCount" int NOT NULL,
	"FailedPasswordAttemptWindowStart" timestamp NOT NULL,
	"FailedPasswordAnswerAttemptCount" int NOT NULL,
	"FailedPasswordAnswerAttemptWindowStart" timestamp NOT NULL,
	"Comment" text,
	"ApplicationId" uuid NOT NULL,
	"UserName" varchar(256) NOT NULL,
	"MobileAlias" varchar(16),
	"IsAnonymous" boolean NOT NULL,
	"LastActivityDate" timestamp NOT NULL);

CREATE TABLE "dbo"."vw_aspnet_Profiles"(
	"UserId" uuid NOT NULL,
	"LastUpdatedDate" timestamp NOT NULL,
	"DataSize" int);

CREATE TABLE "dbo"."vw_aspnet_Roles"(
	"ApplicationId" uuid NOT NULL,
	"RoleId" uuid NOT NULL,
	"RoleName" varchar(256) NOT NULL,
	"LoweredRoleName" varchar(256) NOT NULL,
	"Description" varchar(256));

CREATE TABLE "dbo"."vw_aspnet_Users"(
	"ApplicationId" uuid NOT NULL,
	"UserId" uuid NOT NULL,
	"UserName" varchar(256) NOT NULL,
	"LoweredUserName" varchar(256) NOT NULL,
	"MobileAlias" varchar(16),
	"IsAnonymous" boolean NOT NULL,
	"LastActivityDate" timestamp NOT NULL);

CREATE TABLE "dbo"."vw_aspnet_UsersInRoles"(
	"UserId" uuid NOT NULL,
	"RoleId" uuid NOT NULL);

CREATE TABLE "dbo"."vw_aspnet_WebPartState_Paths"(
	"ApplicationId" uuid NOT NULL,
	"PathId" uuid NOT NULL,
	"Path" varchar(256) NOT NULL,
	"LoweredPath" varchar(256) NOT NULL);

CREATE TABLE "dbo"."vw_aspnet_WebPartState_Shared"(
	"PathId" uuid NOT NULL,
	"DataSize" int,
	"LastUpdatedDate" timestamp NOT NULL);

CREATE TABLE "dbo"."vw_aspnet_WebPartState_User"(
	"PathId" uuid,
	"UserId" uuid NOT NULL,
	"DataSize" int,
	"LastUpdatedDate" timestamp NOT NULL);

ALTER TABLE "dbo"."AspNetRoles" ADD CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id");
ALTER TABLE "dbo"."AspNetUserClaims" ADD CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id");
ALTER TABLE "dbo"."AspNetUserLogins" ADD CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("UserId","LoginProvider","ProviderKey");
ALTER TABLE "dbo"."AspNetUserRoles" ADD CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId","RoleId");
ALTER TABLE "dbo"."aspnet_Applications" ADD CONSTRAINT "PK_aspnet_Applications" PRIMARY KEY ("ApplicationId");
ALTER TABLE "dbo"."aspnet_Membership" ADD CONSTRAINT "PK_aspnet_Membership" PRIMARY KEY ("ApplicationId");
ALTER TABLE "dbo"."aspnet_Paths" ADD CONSTRAINT "PK_aspnet_Paths" PRIMARY KEY ("PathId");
ALTER TABLE "dbo"."aspnet_PersonalizationAllUsers" ADD CONSTRAINT "PK_aspnet_PersonalizationAllUsers" PRIMARY KEY ("PathId");
ALTER TABLE "dbo"."aspnet_PersonalizationPerUser" ADD CONSTRAINT "PK_aspnet_PersonalizationPerUser" PRIMARY KEY ("Id");
ALTER TABLE "dbo"."aspnet_Profile" ADD CONSTRAINT "PK_aspnet_Profile" PRIMARY KEY ("UserId");
ALTER TABLE "dbo"."aspnet_Roles" ADD CONSTRAINT "PK_aspnet_Roles" PRIMARY KEY ("RoleId");
ALTER TABLE "dbo"."aspnet_SchemaVersions" ADD CONSTRAINT "PK_aspnet_SchemaVersions" PRIMARY KEY ("Feature","CompatibleSchemaVersion");
ALTER TABLE "dbo"."aspnet_Users" ADD CONSTRAINT "PK_aspnet_Users" PRIMARY KEY ("UserId");
ALTER TABLE "dbo"."aspnet_UsersInRoles" ADD CONSTRAINT "PK_aspnet_UsersInRoles" PRIMARY KEY ("RoleId");
ALTER TABLE "dbo"."aspnet_WebEvent_Events" ADD CONSTRAINT "PK_aspnet_WebEvent_Events" PRIMARY KEY ("EventId");
ALTER TABLE "dbo"."vw_aspnet_Applications" ADD CONSTRAINT "PK_vw_aspnet_Applications" PRIMARY KEY ("ApplicationId");
ALTER TABLE "dbo"."vw_aspnet_MembershipUsers" ADD CONSTRAINT "PK_vw_aspnet_MembershipUsers" PRIMARY KEY ("UserId");
ALTER TABLE "dbo"."vw_aspnet_Profiles" ADD CONSTRAINT "PK_vw_aspnet_Profiles" PRIMARY KEY ("UserId");
ALTER TABLE "dbo"."vw_aspnet_Roles" ADD CONSTRAINT "PK_vw_aspnet_Roles" PRIMARY KEY ("ApplicationId","RoleId");
ALTER TABLE "dbo"."vw_aspnet_UsersInRoles" ADD CONSTRAINT "PK_vw_aspnet_UsersInRoles" PRIMARY KEY ("UserId","RoleId");
ALTER TABLE "dbo"."vw_aspnet_WebPartState_Paths" ADD CONSTRAINT "PK_vw_aspnet_WebPartState_Paths" PRIMARY KEY ("ApplicationId","PathId");
ALTER TABLE "dbo"."vw_aspnet_WebPartState_Shared" ADD CONSTRAINT "PK_vw_aspnet_WebPartState_Shared" PRIMARY KEY ("PathId");
ALTER TABLE "dbo"."vw_aspnet_WebPartState_User" ADD CONSTRAINT "PK_vw_aspnet_WebPartState_User" PRIMARY KEY ("UserId");
CREATE INDEX "ClusteredIndex-20141217-230754" ON "dbo"."AspNetRoles" ("Id" ASC);
CREATE INDEX "ClusteredIndex-20141217-230805" ON "dbo"."AspNetUserClaims" ("Id" ASC);
CREATE INDEX "ClusteredIndex-20141217-230815" ON "dbo"."AspNetUserLogins" ("UserId" ASC);
CREATE INDEX "ClusteredIndex-20141217-230826" ON "dbo"."AspNetUserRoles" ("RoleId" ASC);
CREATE INDEX "ClusteredIndex-20141217-230838" ON "dbo"."AspNetUsers" ("Id" ASC);
CREATE INDEX "IX_UserName" ON "dbo"."AspNetUsers" ("UserName" ASC);
CREATE INDEX "ClusteredIndex-20141217-230903" ON "dbo"."Sessions" ("SessionId" ASC);
CREATE INDEX "ClusteredIndex-20141217-230529" ON "dbo"."aspnet_Applications" ("ApplicationId" ASC);
CREATE INDEX "ClusteredIndex-20141217-230542" ON "dbo"."aspnet_Membership" ("ApplicationId" ASC);
CREATE INDEX "ClusteredIndex-20141217-230607" ON "dbo"."aspnet_Paths" ("PathId" ASC);
CREATE INDEX "ClusteredIndex-20141217-230616" ON "dbo"."aspnet_PersonalizationAllUsers" ("PathId" ASC);
CREATE INDEX "ClusteredIndex-20141217-230628" ON "dbo"."aspnet_PersonalizationPerUser" ("Id" ASC);
CREATE INDEX "ClusteredIndex-20141217-230638" ON "dbo"."aspnet_Profile" ("UserId" ASC);
CREATE INDEX "ClusteredIndex-20141217-230647" ON "dbo"."aspnet_Roles" ("RoleId" ASC);
CREATE INDEX "ClusteredIndex-20141217-230658" ON "dbo"."aspnet_SchemaVersions" ("IsCurrentVersion" ASC);
CREATE INDEX "ClusteredIndex-20141217-230719" ON "dbo"."aspnet_Users" ("UserId" ASC);
CREATE INDEX "ClusteredIndex-20141217-230730" ON "dbo"."aspnet_UsersInRoles" ("RoleId" ASC);
CREATE INDEX "ClusteredIndex-20141217-230743" ON "dbo"."aspnet_WebEvent_Events" ("EventId" ASC);
CREATE INDEX "ClusteredIndex-20141217-230915" ON "dbo"."vw_aspnet_Applications" ("ApplicationId" ASC);
CREATE INDEX "ClusteredIndex-20141217-230926" ON "dbo"."vw_aspnet_MembershipUsers" ("UserId" ASC);
CREATE INDEX "ClusteredIndex-20141217-230937" ON "dbo"."vw_aspnet_Profiles" ("UserId" ASC);
CREATE INDEX "ClusteredIndex-20141217-230946" ON "dbo"."vw_aspnet_Roles" ("RoleId" ASC);
CREATE INDEX "ClusteredIndex-20141217-230955" ON "dbo"."vw_aspnet_Users" ("UserId" ASC);
CREATE INDEX "ClusteredIndex-20141217-231004" ON "dbo"."vw_aspnet_UsersInRoles" ("RoleId" ASC);
CREATE INDEX "ClusteredIndex-20141217-231014" ON "dbo"."vw_aspnet_WebPartState_Paths" ("PathId" ASC);
CREATE INDEX "ClusteredIndex-20141217-231023" ON "dbo"."vw_aspnet_WebPartState_Shared" ("PathId" ASC);
CREATE INDEX "ClusteredIndex-20141217-231032" ON "dbo"."vw_aspnet_WebPartState_User" ("UserId" ASC);
ALTER TABLE "dbo"."AspNetUsers" ALTER COLUMN "IsConfirmed" SET DEFAULT false;
ALTER TABLE "dbo"."AspNetUsers" ALTER COLUMN "LastLoginDateTime" SET DEFAULT '1900-01-01T00:00:00.000';
ALTER TABLE "dbo"."AspNetUsers" ALTER COLUMN "Partner" SET DEFAULT false;
ALTER TABLE "dbo"."AspNetUsers" ALTER COLUMN "RegistrationDateTime" SET DEFAULT '1900-01-01T00:00:00.000';

COMMIT;
