DROP SCHEMA IF EXISTS dbo CASCADE;
CREATE SCHEMA dbo;
SET SCHEMA 'dbo';

/* Make dbo schema the default one */
ALTER DATABASE voatusers SET search_path TO dbo;


CREATE TABLE "__MigrationHistory"(
	"MigrationId" Varchar(150) NOT NULL,
	"ContextKey" Varchar(300) NOT NULL,
	"Model" Bytea NOT NULL,
	"ProductVersion" Varchar(32) NOT NULL,
 CONSTRAINT PK___MigrationHistory PRIMARY KEY
(
	"MigrationId"
)
);

CREATE TABLE "aspnet_Applications"(
	"ApplicationName" Varchar(256) NOT NULL,
	"LoweredApplicationName" Varchar(256) NOT NULL,
	"ApplicationId" Char(36) NOT NULL,
	"Description" Varchar(256) NULL,
 CONSTRAINT PK_aspnet_Applications PRIMARY KEY
(
	"ApplicationId"
)
);

CREATE TABLE "aspnet_Membership"(
	"ApplicationId" Char(36) NOT NULL,
	"UserId" Char(36) NOT NULL,
	"Password" Varchar(128) NOT NULL,
	"PasswordFormat" int NOT NULL,
	"PasswordSalt" Varchar(128) NOT NULL,
	"MobilePIN" Varchar(16) NULL,
	"Email" Varchar(256) NULL,
	"LoweredEmail" Varchar(256) NULL,
	"PasswordQuestion" Varchar(256) NULL,
	"PasswordAnswer" Varchar(128) NULL,
	"IsApproved" Boolean NOT NULL,
	"IsLockedOut" Boolean NOT NULL,
	"CreateDate" Timestamp(3) NOT NULL,
	"LastLoginDate" Timestamp(3) NOT NULL,
	"LastPasswordChangedDate" Timestamp(3) NOT NULL,
	"LastLockoutDate" Timestamp(3) NOT NULL,
	"FailedPasswordAttemptCount" int NOT NULL,
	"FailedPasswordAttemptWindowStart" Timestamp(3) NOT NULL,
	"FailedPasswordAnswerAttemptCount" int NOT NULL,
	"FailedPasswordAnswerAttemptWindowStart" Timestamp(3) NOT NULL,
	"Comment" Text NULL,
 CONSTRAINT PK_aspnet_Membership PRIMARY KEY
(
	"ApplicationId"
)
);

CREATE TABLE "aspnet_Paths"(
	"ApplicationId" Char(36) NOT NULL,
	"PathId" Char(36) NOT NULL,
	"Path" Varchar(256) NOT NULL,
	"LoweredPath" Varchar(256) NOT NULL,
 CONSTRAINT PK_aspnet_Paths PRIMARY KEY
(
	"PathId"
)
);

CREATE TABLE "aspnet_PersonalizationAllUsers"(
	"PathId" Char(36) NOT NULL,
	"PageSettings" Bytea NOT NULL,
	"LastUpdatedDate" Timestamp(3) NOT NULL,
 CONSTRAINT PK_aspnet_PersonalizationAllUsers PRIMARY KEY
(
	"PathId"
)
);

CREATE TABLE "aspnet_PersonalizationPerUser"(
	"Id" Char(36) NOT NULL,
	"PathId" Char(36) NULL,
	"UserId" Char(36) NULL,
	"PageSettings" Bytea NOT NULL,
	"LastUpdatedDate" Timestamp(3) NOT NULL,
 CONSTRAINT PK_aspnet_PersonalizationPerUser PRIMARY KEY
(
	"Id"
)
);

CREATE TABLE "aspnet_Profile"(
	"UserId" Char(36) NOT NULL,
	"PropertyNames" Text NOT NULL,
	"PropertyValuesString" Text NOT NULL,
	"PropertyValuesBinary" Bytea NOT NULL,
	"LastUpdatedDate" Timestamp(3) NOT NULL,
 CONSTRAINT PK_aspnet_Profile PRIMARY KEY
(
	"UserId"
)
);

CREATE TABLE "aspnet_Roles"(
	"ApplicationId" Char(36) NOT NULL,
	"RoleId" Char(36) NOT NULL,
	"RoleName" Varchar(256) NOT NULL,
	"LoweredRoleName" Varchar(256) NOT NULL,
	"Description" Varchar(256) NULL,
 CONSTRAINT PK_aspnet_Roles PRIMARY KEY
(
	"RoleId"
)
);

CREATE TABLE "aspnet_SchemaVersions"(
	"Feature" Varchar(128) NOT NULL,
	"CompatibleSchemaVersion" Varchar(128) NOT NULL,
	"IsCurrentVersion" Boolean NOT NULL,
 CONSTRAINT PK_aspnet_SchemaVersions PRIMARY KEY
(
	"Feature" ,
	"CompatibleSchemaVersion"
)
);

CREATE TABLE "aspnet_Users"(
	"ApplicationId" Char(36) NOT NULL,
	"UserId" Char(36) NOT NULL,
	"UserName" Varchar(256) NOT NULL,
	"LoweredUserName" Varchar(256) NOT NULL,
	"MobileAlias" Varchar(16) NULL,
	"IsAnonymous" Boolean NOT NULL,
	"LastActivityDate" Timestamp(3) NOT NULL,
 CONSTRAINT PK_aspnet_Users PRIMARY KEY
(
	"UserId"
)
);

CREATE TABLE "aspnet_UsersInRoles"(
	"UserId" Char(36) NOT NULL,
	"RoleId" Char(36) NOT NULL,
 CONSTRAINT PK_aspnet_UsersInRoles PRIMARY KEY
(
	"RoleId"
)
);

CREATE TABLE "aspnet_WebEvent_Events"(
	"EventId" char(32) NOT NULL,
	"EventTimeUtc" Timestamp(3) NOT NULL,
	"EventTime" Timestamp(3) NOT NULL,
	"EventType" Varchar(256) NOT NULL,
	"EventSequence" decimal(19, 0) NOT NULL,
	"EventOccurrence" decimal(19, 0) NOT NULL,
	"EventCode" int NOT NULL,
	"EventDetailCode" int NOT NULL,
	"Message" Varchar(1024) NULL,
	"ApplicationPath" Varchar(256) NULL,
	"ApplicationVirtualPath" Varchar(256) NULL,
	"MachineName" Varchar(256) NOT NULL,
	"RequestUrl" Varchar(1024) NULL,
	"ExceptionType" Varchar(256) NULL,
	"Details" Text NULL,
 CONSTRAINT PK_aspnet_WebEvent_Events PRIMARY KEY
(
	"EventId"
)
);

CREATE TABLE "AspNetRoles"(
	"Id" Varchar(128) NOT NULL,
	"Name" Text NOT NULL,
 CONSTRAINT PK_AspNetRoles PRIMARY KEY
(
	"Id"
)
);

CREATE TABLE "AspNetUserClaims"(
	"Id" int NOT NULL,
	"ClaimType" Text NULL,
	"ClaimValue" Text NULL,
	"UserId" Varchar(128) NOT NULL,
 CONSTRAINT PK_AspNetUserClaims PRIMARY KEY
(
	"Id"
)
);

CREATE TABLE "AspNetUserLogins"(
	"UserId" Varchar(128) NOT NULL,
	"LoginProvider" Varchar(128) NOT NULL,
	"ProviderKey" Varchar(128) NOT NULL,
 CONSTRAINT PK_AspNetUserLogins PRIMARY KEY
(
	"UserId" ,
	"LoginProvider" ,
	"ProviderKey"
)
);

CREATE TABLE "AspNetUserRoles"(
	"UserId" Varchar(128) NOT NULL,
	"RoleId" Varchar(128) NOT NULL,
 CONSTRAINT PK_AspNetUserRoles PRIMARY KEY
(
	"UserId" ,
	"RoleId"
)
);

CREATE TABLE "AspNetUsers"(
	"Id" Varchar(128) NOT NULL,
	"UserName" Varchar(800) NOT NULL,
	"PasswordHash" Text NULL,
	"SecurityStamp" Text NULL,
	"Email" Text NULL,
	"IsConfirmed" Boolean NULL DEFAULT ((FALSE)),
	"EmailConfirmed" Boolean NULL,
	"PhoneNumber" Char(10) NULL,
	"PhoneNumberConfirmed" Boolean NULL,
	"TwoFactorEnabled" Boolean NULL,
	"LockoutEndDateUtc" Timestamp(3) NULL,
	"LockoutEnabled" Boolean NULL,
	"AccessFailedCount" int NULL,
	"RegistrationDateTime" Timestamp(3) NOT NULL DEFAULT ('1900-01-01T00:00:00.000'),
	"RecoveryQuestion" Varchar(50) NULL,
	"Answer" Varchar(50) NULL,
	"Partner" Boolean NOT NULL DEFAULT ((FALSE)),
	"LastLoginFromIp" Varchar(50) NULL,
	"LastLoginDateTime" Timestamp(3) NOT NULL DEFAULT ('1900-01-01T00:00:00.000')
);

CREATE TABLE "Sessions"(
	"SessionId" Varchar(88) NOT NULL,
	"Created" Timestamp(3) NOT NULL,
	"Expires" Timestamp(3) NOT NULL,
	"LockDate" Timestamp(3) NOT NULL,
	"LockCookie" int NOT NULL,
	"Locked" Boolean NOT NULL,
	"SessionItem" Bytea NULL,
	"Flags" int NOT NULL,
	"Timeout" int NOT NULL
);

CREATE TABLE "vw_aspnet_Applications"(
	"ApplicationName" Varchar(256) NOT NULL,
	"LoweredApplicationName" Varchar(256) NOT NULL,
	"ApplicationId" Char(36) NOT NULL,
	"Description" Varchar(256) NULL,
 CONSTRAINT PK_vw_aspnet_Applications PRIMARY KEY
(
	"ApplicationId"
)
);

CREATE TABLE "vw_aspnet_MembershipUsers"(
	"UserId" Char(36) NOT NULL,
	"PasswordFormat" int NOT NULL,
	"MobilePIN" Varchar(16) NULL,
	"Email" Varchar(256) NULL,
	"LoweredEmail" Varchar(256) NULL,
	"PasswordQuestion" Varchar(256) NULL,
	"PasswordAnswer" Varchar(128) NULL,
	"IsApproved" Boolean NOT NULL,
	"IsLockedOut" Boolean NOT NULL,
	"CreateDate" Timestamp(3) NOT NULL,
	"LastLoginDate" Timestamp(3) NOT NULL,
	"LastPasswordChangedDate" Timestamp(3) NOT NULL,
	"LastLockoutDate" Timestamp(3) NOT NULL,
	"FailedPasswordAttemptCount" int NOT NULL,
	"FailedPasswordAttemptWindowStart" Timestamp(3) NOT NULL,
	"FailedPasswordAnswerAttemptCount" int NOT NULL,
	"FailedPasswordAnswerAttemptWindowStart" Timestamp(3) NOT NULL,
	"Comment" Text NULL,
	"ApplicationId" Char(36) NOT NULL,
	"UserName" Varchar(256) NOT NULL,
	"MobileAlias" Varchar(16) NULL,
	"IsAnonymous" Boolean NOT NULL,
	"LastActivityDate" Timestamp(3) NOT NULL,
 CONSTRAINT PK_vw_aspnet_MembershipUsers PRIMARY KEY
(
	"UserId"
)
);

CREATE TABLE "vw_aspnet_Profiles"(
	"UserId" Char(36) NOT NULL,
	"LastUpdatedDate" Timestamp(3) NOT NULL,
	"DataSize" int NULL,
 CONSTRAINT PK_vw_aspnet_Profiles PRIMARY KEY
(
	"UserId"
)
);

CREATE TABLE "vw_aspnet_Roles"(
	"ApplicationId" Char(36) NOT NULL,
	"RoleId" Char(36) NOT NULL,
	"RoleName" Varchar(256) NOT NULL,
	"LoweredRoleName" Varchar(256) NOT NULL,
	"Description" Varchar(256) NULL,
 CONSTRAINT PK_vw_aspnet_Roles PRIMARY KEY
(
	"ApplicationId" ,
	"RoleId"
)
);

CREATE TABLE "vw_aspnet_Users"(
	"ApplicationId" Char(36) NOT NULL,
	"UserId" Char(36) NOT NULL,
	"UserName" Varchar(256) NOT NULL,
	"LoweredUserName" Varchar(256) NOT NULL,
	"MobileAlias" Varchar(16) NULL,
	"IsAnonymous" Boolean NOT NULL,
	"LastActivityDate" Timestamp(3) NOT NULL
);

CREATE TABLE "vw_aspnet_UsersInRoles"(
	"UserId" Char(36) NOT NULL,
	"RoleId" Char(36) NOT NULL,
 CONSTRAINT PK_vw_aspnet_UsersInRoles PRIMARY KEY
(
	"UserId" ,
	"RoleId"
)
);

CREATE TABLE "vw_aspnet_WebPartState_Paths"(
	"ApplicationId" Char(36) NOT NULL,
	"PathId" Char(36) NOT NULL,
	"Path" Varchar(256) NOT NULL,
	"LoweredPath" Varchar(256) NOT NULL,
 CONSTRAINT PK_vw_aspnet_WebPartState_Paths PRIMARY KEY
(
	"ApplicationId" ,
	"PathId"
)
);

CREATE TABLE "vw_aspnet_WebPartState_Shared"(
	"PathId" Char(36) NOT NULL,
	"DataSize" int NULL,
	"LastUpdatedDate" Timestamp(3) NOT NULL,
 CONSTRAINT PK_vw_aspnet_WebPartState_Shared PRIMARY KEY
(
	"PathId"
)
);

CREATE TABLE "vw_aspnet_WebPartState_User"(
	"PathId" Char(36) NULL,
	"UserId" Char(36) NOT NULL,
	"DataSize" int NULL,
	"LastUpdatedDate" Timestamp(3) NOT NULL,
 CONSTRAINT PK_vw_aspnet_WebPartState_User PRIMARY KEY
(
	"UserId"
)
);

