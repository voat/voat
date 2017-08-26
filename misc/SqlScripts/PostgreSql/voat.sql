BEGIN;
CREATE EXTENSION IF NOT EXISTS citext;
CREATE SCHEMA IF NOT EXISTS "dbo";
alter database {dbName} set search_path to 'dbo';
alter database {dbName} set TimeZone to 'UTC';

CREATE TABLE "dbo"."Ad"(
	"ID" int NOT NULL,
	"IsActive" boolean NOT NULL,
	"GraphicUrl" citext NOT NULL CHECK (char_length("GraphicUrl") <= 100),
	"DestinationUrl" citext CHECK (char_length("DestinationUrl") <= 1000),
	"Name" citext NOT NULL CHECK (char_length("Name") <= 100),
	"Description" citext NOT NULL CHECK (char_length("Description") <= 2000),
	"StartDate" timestamptz,
	"EndDate" timestamptz,
	"Subverse" citext CHECK (char_length("Subverse") <= 50),
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."AdminLog"(
	"ID" int NOT NULL,
	"UserName" citext NOT NULL CHECK (char_length("UserName") <= 20),
	"RefUserName" citext CHECK (char_length("RefUserName") <= 20),
	"RefSubverse" citext CHECK (char_length("RefSubverse") <= 50),
	"RefUrl" citext CHECK (char_length("RefUrl") <= 200),
	"RefCommentID" int,
	"RefSubmissionID" int,
	"Type" citext CHECK (char_length("Type") <= 100),
	"Action" citext NOT NULL CHECK (char_length("Action") <= 100),
	"Details" citext NOT NULL CHECK (char_length("Details") <= 1000),
	"InternalDetails" citext,
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."ApiClient"(
	"ID" int NOT NULL,
	"IsActive" boolean NOT NULL,
	"UserName" citext CHECK (char_length("UserName") <= 100),
	"AppName" citext NOT NULL CHECK (char_length("AppName") <= 50),
	"AppDescription" citext CHECK (char_length("AppDescription") <= 2000),
	"AppAboutUrl" citext CHECK (char_length("AppAboutUrl") <= 200),
	"RedirectUrl" citext CHECK (char_length("RedirectUrl") <= 200),
	"PublicKey" citext NOT NULL CHECK (char_length("PublicKey") <= 100),
	"PrivateKey" citext NOT NULL CHECK (char_length("PrivateKey") <= 100),
	"LastAccessDate" timestamptz,
	"CreationDate" timestamptz NOT NULL,
	"ApiThrottlePolicyID" int,
	"ApiPermissionPolicyID" int);

CREATE TABLE "dbo"."ApiCorsPolicy"(
	"ID" int NOT NULL,
	"IsActive" boolean NOT NULL,
	"AllowOrigin" citext NOT NULL CHECK (char_length("AllowOrigin") <= 100),
	"AllowMethods" citext NOT NULL CHECK (char_length("AllowMethods") <= 100),
	"AllowHeaders" citext NOT NULL CHECK (char_length("AllowHeaders") <= 100),
	"AllowCredentials" boolean,
	"MaxAge" int,
	"UserName" citext CHECK (char_length("UserName") <= 100),
	"Description" citext CHECK (char_length("Description") <= 500),
	"CreatedBy" citext NOT NULL CHECK (char_length("CreatedBy") <= 100),
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."ApiLog"(
	"ID" int NOT NULL,
	"ApiClientID" int NOT NULL,
	"Method" citext NOT NULL CHECK (char_length("Method") <= 10),
	"Url" citext NOT NULL CHECK (char_length("Url") <= 500),
	"Headers" citext,
	"Body" citext,
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."ApiPermissionPolicy"(
	"ID" int NOT NULL,
	"Name" citext NOT NULL CHECK (char_length("Name") <= 100),
	"Policy" citext NOT NULL CHECK (char_length("Policy") <= 2000));

CREATE TABLE "dbo"."ApiThrottlePolicy"(
	"ID" int NOT NULL,
	"Name" citext NOT NULL CHECK (char_length("Name") <= 100),
	"Policy" citext NOT NULL CHECK (char_length("Policy") <= 2000));

CREATE TABLE "dbo"."Badge"(
	"ID" citext NOT NULL CHECK (char_length("ID") <= 50),
	"Graphic" citext NOT NULL CHECK (char_length("Graphic") <= 50),
	"Title" citext NOT NULL CHECK (char_length("Title") <= 300),
	"Name" citext NOT NULL CHECK (char_length("Name") <= 50));

CREATE TABLE "dbo"."BannedDomain"(
	"ID" int NOT NULL,
	"Domain" citext NOT NULL CHECK (char_length("Domain") <= 100),
	"CreatedBy" citext NOT NULL CHECK (char_length("CreatedBy") <= 50),
	"CreationDate" timestamptz NOT NULL,
	"Reason" citext NOT NULL CHECK (char_length("Reason") <= 500));

CREATE TABLE "dbo"."BannedUser"(
	"ID" int NOT NULL,
	"UserName" citext NOT NULL CHECK (char_length("UserName") <= 50),
	"CreationDate" timestamptz NOT NULL,
	"Reason" citext NOT NULL CHECK (char_length("Reason") <= 500),
	"CreatedBy" citext NOT NULL CHECK (char_length("CreatedBy") <= 50));

CREATE TABLE "dbo"."Comment"(
	"ID" int NOT NULL,
	"UserName" citext NOT NULL CHECK (char_length("UserName") <= 50),
	"Content" citext NOT NULL,
	"CreationDate" timestamptz NOT NULL,
	"LastEditDate" timestamptz,
	"SubmissionID" int,
	"UpCount" int NOT NULL,
	"DownCount" int NOT NULL,
	"ParentID" int,
	"IsAnonymized" boolean NOT NULL,
	"IsDistinguished" boolean NOT NULL,
	"FormattedContent" citext,
	"IsDeleted" boolean NOT NULL);

CREATE TABLE "dbo"."CommentRemovalLog"(
	"CommentID" int NOT NULL,
	"Moderator" citext NOT NULL CHECK (char_length("Moderator") <= 50),
	"CreationDate" timestamptz NOT NULL,
	"Reason" citext NOT NULL CHECK (char_length("Reason") <= 500));

CREATE TABLE "dbo"."CommentSaveTracker"(
	"ID" int NOT NULL,
	"CommentID" int NOT NULL,
	"UserName" citext NOT NULL CHECK (char_length("UserName") <= 50),
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."CommentVoteTracker"(
	"ID" int NOT NULL,
	"CommentID" int NOT NULL,
	"UserName" citext NOT NULL CHECK (char_length("UserName") <= 50),
	"VoteStatus" int NOT NULL,
	"CreationDate" timestamptz NOT NULL,
	"IPAddress" citext CHECK (char_length("IPAddress") <= 90),
	"VoteValue" double precision NOT NULL);

CREATE TABLE "dbo"."DefaultSubverse"(
	"Subverse" citext NOT NULL CHECK (char_length("Subverse") <= 20),
	"Order" int NOT NULL);

CREATE TABLE "dbo"."EventLog"(
	"ID" int NOT NULL,
	"ParentID" int,
	"ActivityID" citext CHECK (char_length("ActivityID") <= 50),
	"UserName" citext CHECK (char_length("UserName") <= 100),
	"Origin" citext CHECK (char_length("Origin") <= 100),
	"Type" citext NOT NULL CHECK (char_length("Type") <= 300),
	"Message" citext NOT NULL CHECK (char_length("Message") <= 1500),
	"Category" citext NOT NULL CHECK (char_length("Category") <= 1000),
	"Exception" citext,
	"Data" citext,
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."Featured"(
	"ID" int NOT NULL,
	"DomainType" int NOT NULL,
	"DomainID" int NOT NULL,
	"Title" citext CHECK (char_length("Title") <= 100),
	"Description" citext CHECK (char_length("Description") <= 500),
	"StartDate" timestamptz NOT NULL,
	"EndDate" timestamptz,
	"CreatedBy" citext NOT NULL CHECK (char_length("CreatedBy") <= 50));

CREATE TABLE "dbo"."FeaturedSubverse"(
	"ID" int NOT NULL,
	"Subverse" citext NOT NULL CHECK (char_length("Subverse") <= 20),
	"CreatedBy" citext NOT NULL CHECK (char_length("CreatedBy") <= 50),
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."Filter"(
	"ID" int NOT NULL,
	"IsActive" boolean NOT NULL,
	"Name" citext NOT NULL CHECK (char_length("Name") <= 100),
	"Description" citext CHECK (char_length("Description") <= 1000),
	"Pattern" citext NOT NULL CHECK (char_length("Pattern") <= 100),
	"Replacement" citext CHECK (char_length("Replacement") <= 1000),
	"AppliesTo" int,
	"Action" int,
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."Message"(
	"ID" int NOT NULL,
	"CorrelationID" citext NOT NULL CHECK (char_length("CorrelationID") <= 36),
	"ParentID" int,
	"Type" int NOT NULL,
	"Sender" citext NOT NULL CHECK (char_length("Sender") <= 50),
	"SenderType" int NOT NULL,
	"Recipient" citext NOT NULL CHECK (char_length("Recipient") <= 50),
	"RecipientType" int NOT NULL,
	"Title" citext CHECK (char_length("Title") <= 500),
	"Content" citext,
	"FormattedContent" citext,
	"Subverse" citext CHECK (char_length("Subverse") <= 20),
	"SubmissionID" int,
	"CommentID" int,
	"IsAnonymized" boolean NOT NULL,
	"ReadDate" timestamptz,
	"CreatedBy" citext CHECK (char_length("CreatedBy") <= 50),
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."ModeratorInvitation"(
	"ID" int NOT NULL,
	"CreatedBy" citext NOT NULL CHECK (char_length("CreatedBy") <= 50),
	"CreationDate" timestamptz NOT NULL,
	"Recipient" citext NOT NULL CHECK (char_length("Recipient") <= 50),
	"Subverse" citext NOT NULL CHECK (char_length("Subverse") <= 50),
	"Power" int NOT NULL);

CREATE TABLE "dbo"."RuleReport"(
	"ID" bigint NOT NULL,
	"Subverse" citext CHECK (char_length("Subverse") <= 50),
	"UserName" citext CHECK (char_length("UserName") <= 100),
	"SubmissionID" int,
	"CommentID" int,
	"RuleSetID" int NOT NULL,
	"ReviewedBy" citext CHECK (char_length("ReviewedBy") <= 100),
	"ReviewedDate" timestamptz,
	"CreatedBy" citext NOT NULL CHECK (char_length("CreatedBy") <= 100),
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."RuleSet"(
	"ID" int NOT NULL,
	"IsActive" boolean NOT NULL,
	"Subverse" citext CHECK (char_length("Subverse") <= 50),
	"ContentType" smallint,
	"SortOrder" int,
	"Name" citext NOT NULL CHECK (char_length("Name") <= 200),
	"Description" citext CHECK (char_length("Description") <= 1000),
	"CreatedBy" citext NOT NULL CHECK (char_length("CreatedBy") <= 50),
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."SessionTracker"(
	"SessionID" citext NOT NULL CHECK (char_length("SessionID") <= 90),
	"Subverse" citext NOT NULL CHECK (char_length("Subverse") <= 20),
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."StickiedSubmission"(
	"SubmissionID" int NOT NULL,
	"Subverse" citext NOT NULL CHECK (char_length("Subverse") <= 20),
	"CreatedBy" citext NOT NULL CHECK (char_length("CreatedBy") <= 50),
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."Submission"(
	"ID" int NOT NULL,
	"UserName" citext NOT NULL CHECK (char_length("UserName") <= 50),
	"Content" citext,
	"CreationDate" timestamptz NOT NULL,
	"Type" int NOT NULL,
	"Title" citext CHECK (char_length("Title") <= 200),
	"Rank" double precision NOT NULL,
	"Subverse" citext CHECK (char_length("Subverse") <= 20),
	"UpCount" int NOT NULL,
	"DownCount" int NOT NULL,
	"Thumbnail" citext CHECK (char_length("Thumbnail") <= 150),
	"LastEditDate" timestamptz,
	"FlairLabel" citext CHECK (char_length("FlairLabel") <= 50),
	"FlairCss" citext CHECK (char_length("FlairCss") <= 50),
	"IsAnonymized" boolean NOT NULL,
	"Views" double precision NOT NULL,
	"IsDeleted" boolean NOT NULL,
	"RelativeRank" double precision NOT NULL,
	"Url" citext CHECK (char_length("Url") <= 3000),
	"FormattedContent" citext,
	"IsAdult" boolean NOT NULL,
	"ArchiveDate" timestamptz);

CREATE TABLE "dbo"."SubmissionRemovalLog"(
	"SubmissionID" int NOT NULL,
	"Moderator" citext NOT NULL CHECK (char_length("Moderator") <= 50),
	"CreationDate" timestamptz NOT NULL,
	"Reason" citext NOT NULL CHECK (char_length("Reason") <= 500));

CREATE TABLE "dbo"."SubmissionSaveTracker"(
	"ID" int NOT NULL,
	"SubmissionID" int NOT NULL,
	"UserName" citext NOT NULL CHECK (char_length("UserName") <= 50),
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."SubmissionVoteTracker"(
	"ID" int NOT NULL,
	"SubmissionID" int NOT NULL,
	"UserName" citext NOT NULL CHECK (char_length("UserName") <= 50),
	"VoteStatus" int NOT NULL,
	"CreationDate" timestamptz NOT NULL,
	"IPAddress" citext CHECK (char_length("IPAddress") <= 90),
	"VoteValue" double precision NOT NULL);

CREATE TABLE "dbo"."Subverse"(
	"ID" int NOT NULL,
	"Name" citext NOT NULL CHECK (char_length("Name") <= 20),
	"Title" citext NOT NULL CHECK (char_length("Title") <= 100),
	"Description" citext CHECK (char_length("Description") <= 500),
	"SideBar" citext CHECK (char_length("SideBar") <= 4000),
	"IsAdult" boolean NOT NULL,
	"IsThumbnailEnabled" boolean NOT NULL,
	"ExcludeSitewideBans" boolean NOT NULL,
	"CreationDate" timestamptz NOT NULL,
	"Stylesheet" citext,
	"SubscriberCount" int,
	"IsPrivate" boolean NOT NULL,
	"IsAuthorizedOnly" boolean NOT NULL,
	"IsAnonymized" boolean,
	"LastSubmissionDate" timestamptz,
	"MinCCPForDownvote" int NOT NULL,
	"IsAdminPrivate" boolean NOT NULL,
	"IsAdminDisabled" boolean,
	"CreatedBy" citext CHECK (char_length("CreatedBy") <= 50),
	"LastUpdateDate" timestamptz);

CREATE TABLE "dbo"."SubverseBan"(
	"ID" int NOT NULL,
	"Subverse" citext NOT NULL CHECK (char_length("Subverse") <= 20),
	"UserName" citext NOT NULL CHECK (char_length("UserName") <= 50),
	"CreatedBy" citext NOT NULL CHECK (char_length("CreatedBy") <= 50),
	"CreationDate" timestamptz NOT NULL,
	"Reason" citext NOT NULL CHECK (char_length("Reason") <= 500));

CREATE TABLE "dbo"."SubverseFlair"(
	"ID" int NOT NULL,
	"Subverse" citext NOT NULL CHECK (char_length("Subverse") <= 20),
	"Label" citext CHECK (char_length("Label") <= 50),
	"CssClass" citext CHECK (char_length("CssClass") <= 50));

CREATE TABLE "dbo"."SubverseModerator"(
	"ID" int NOT NULL,
	"Subverse" citext NOT NULL CHECK (char_length("Subverse") <= 20),
	"UserName" citext NOT NULL CHECK (char_length("UserName") <= 50),
	"Power" int NOT NULL,
	"CreatedBy" citext CHECK (char_length("CreatedBy") <= 50),
	"CreationDate" timestamptz);

CREATE TABLE "dbo"."SubverseSet"(
	"ID" int NOT NULL,
	"Name" citext NOT NULL CHECK (char_length("Name") <= 20),
	"Title" citext CHECK (char_length("Title") <= 100),
	"Description" citext CHECK (char_length("Description") <= 500),
	"UserName" citext CHECK (char_length("UserName") <= 50),
	"Type" int NOT NULL,
	"IsPublic" boolean NOT NULL,
	"SubscriberCount" int NOT NULL,
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."SubverseSetList"(
	"ID" int NOT NULL,
	"SubverseSetID" int NOT NULL,
	"SubverseID" int NOT NULL,
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."SubverseSetSubscription"(
	"ID" int NOT NULL,
	"SubverseSetID" int NOT NULL,
	"UserName" citext NOT NULL CHECK (char_length("UserName") <= 50),
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."UserBadge"(
	"ID" int NOT NULL,
	"UserName" citext NOT NULL CHECK (char_length("UserName") <= 50),
	"BadgeID" citext NOT NULL CHECK (char_length("BadgeID") <= 50),
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."UserBlockedUser"(
	"ID" int NOT NULL,
	"BlockUser" citext NOT NULL CHECK (char_length("BlockUser") <= 50),
	"UserName" citext NOT NULL CHECK (char_length("UserName") <= 50),
	"CreationDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."UserContribution"(
	"ID" int NOT NULL,
	"UserName" citext NOT NULL CHECK (char_length("UserName") <= 50),
	"IsReceived" boolean NOT NULL,
	"ContentType" int NOT NULL,
	"VoteStatus" int NOT NULL,
	"VoteCount" int NOT NULL,
	"VoteValue" double precision NOT NULL,
	"ValidThroughDate" timestamptz NOT NULL,
	"LastUpdateDate" timestamptz NOT NULL);

CREATE TABLE "dbo"."UserPreference"(
	"UserName" citext NOT NULL CHECK (char_length("UserName") <= 50),
	"DisableCSS" boolean NOT NULL,
	"NightMode" boolean NOT NULL,
	"Language" citext NOT NULL CHECK (char_length("Language") <= 50),
	"OpenInNewWindow" boolean NOT NULL,
	"EnableAdultContent" boolean NOT NULL,
	"DisplayVotes" boolean NOT NULL,
	"DisplaySubscriptions" boolean NOT NULL,
	"UseSubscriptionsMenu" boolean NOT NULL,
	"Bio" citext CHECK (char_length("Bio") <= 100),
	"Avatar" citext CHECK (char_length("Avatar") <= 50),
	"DisplayAds" boolean NOT NULL,
	"DisplayCommentCount" int,
	"HighlightMinutes" int,
	"VanityTitle" citext CHECK (char_length("VanityTitle") <= 50),
	"CollapseCommentLimit" int,
	"BlockAnonymized" boolean NOT NULL,
	"CommentSort" int);

CREATE TABLE "dbo"."ViewStatistic"(
	"SubmissionID" int NOT NULL,
	"ViewerID" citext NOT NULL CHECK (char_length("ViewerID") <= 90));

CREATE SEQUENCE "dbo"."ad_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."Ad"."ID";
CREATE SEQUENCE "dbo"."adminlog_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."AdminLog"."ID";
CREATE SEQUENCE "dbo"."apiclient_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."ApiClient"."ID";
CREATE SEQUENCE "dbo"."apicorspolicy_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."ApiCorsPolicy"."ID";
CREATE SEQUENCE "dbo"."apilog_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."ApiLog"."ID";
CREATE SEQUENCE "dbo"."apipermissionpolicy_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."ApiPermissionPolicy"."ID";
CREATE SEQUENCE "dbo"."apithrottlepolicy_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."ApiThrottlePolicy"."ID";
CREATE SEQUENCE "dbo"."banneddomain_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."BannedDomain"."ID";
CREATE SEQUENCE "dbo"."banneduser_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."BannedUser"."ID";
CREATE SEQUENCE "dbo"."comment_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."Comment"."ID";
CREATE SEQUENCE "dbo"."commentsavetracker_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."CommentSaveTracker"."ID";
CREATE SEQUENCE "dbo"."commentvotetracker_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."CommentVoteTracker"."ID";
CREATE SEQUENCE "dbo"."eventlog_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."EventLog"."ID";
CREATE SEQUENCE "dbo"."featured_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."Featured"."ID";
CREATE SEQUENCE "dbo"."featuredsubverse_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."FeaturedSubverse"."ID";
CREATE SEQUENCE "dbo"."filter_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."Filter"."ID";
CREATE SEQUENCE "dbo"."message_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."Message"."ID";
CREATE SEQUENCE "dbo"."moderatorinvitation_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."ModeratorInvitation"."ID";
CREATE SEQUENCE "dbo"."rulereport_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."RuleReport"."ID";
CREATE SEQUENCE "dbo"."ruleset_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."RuleSet"."ID";
CREATE SEQUENCE "dbo"."submission_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."Submission"."ID";
CREATE SEQUENCE "dbo"."submissionsavetracker_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."SubmissionSaveTracker"."ID";
CREATE SEQUENCE "dbo"."submissionvotetracker_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."SubmissionVoteTracker"."ID";
CREATE SEQUENCE "dbo"."subverse_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."Subverse"."ID";
CREATE SEQUENCE "dbo"."subverseban_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."SubverseBan"."ID";
CREATE SEQUENCE "dbo"."subverseflair_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."SubverseFlair"."ID";
CREATE SEQUENCE "dbo"."subversemoderator_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."SubverseModerator"."ID";
CREATE SEQUENCE "dbo"."subverseset_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."SubverseSet"."ID";
CREATE SEQUENCE "dbo"."subversesetlist_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."SubverseSetList"."ID";
CREATE SEQUENCE "dbo"."subversesetsubscription_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."SubverseSetSubscription"."ID";
CREATE SEQUENCE "dbo"."userbadge_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."UserBadge"."ID";
CREATE SEQUENCE "dbo"."userblockeduser_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."UserBlockedUser"."ID";
CREATE SEQUENCE "dbo"."usercontribution_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "dbo"."UserContribution"."ID";
ALTER TABLE "dbo"."Ad" ADD CONSTRAINT "PK_Ad" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."AdminLog" ADD CONSTRAINT "PK_AdminLog" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."ApiClient" ADD CONSTRAINT "PK_ApiClient" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."ApiCorsPolicy" ADD CONSTRAINT "PK_ApiCorsPolicy" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."ApiLog" ADD CONSTRAINT "PK_ApiLog" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."ApiPermissionPolicy" ADD CONSTRAINT "PK_ApiPermissionPolicy" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."ApiThrottlePolicy" ADD CONSTRAINT "PK_ApiThrottlePolicy" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."Badge" ADD CONSTRAINT "PK_Badge" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."BannedDomain" ADD CONSTRAINT "PK_BannedDomain" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."BannedUser" ADD CONSTRAINT "PK_BannedUser" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."Comment" ADD CONSTRAINT "PK_Comment" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."CommentRemovalLog" ADD CONSTRAINT "PK_CommentRemovalLog" PRIMARY KEY ("CommentID");
ALTER TABLE "dbo"."CommentSaveTracker" ADD CONSTRAINT "PK_CommentSaveTracker" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."CommentVoteTracker" ADD CONSTRAINT "PK_CommentVoteTracker" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."DefaultSubverse" ADD CONSTRAINT "PK_DefaultSubverse" PRIMARY KEY ("Subverse");
ALTER TABLE "dbo"."EventLog" ADD CONSTRAINT "PK_EventLog" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."Featured" ADD CONSTRAINT "PK_Featured" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."FeaturedSubverse" ADD CONSTRAINT "PK_FeaturedSubverse" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."Filter" ADD CONSTRAINT "PK_Filter" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."Message" ADD CONSTRAINT "PK_Message" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."ModeratorInvitation" ADD CONSTRAINT "PK_ModeratorInvitation" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."RuleReport" ADD CONSTRAINT "PK_RuleReport" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."RuleSet" ADD CONSTRAINT "PK_RuleSet" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."SessionTracker" ADD CONSTRAINT "PK_SessionTracker" PRIMARY KEY ("SessionID","Subverse");
ALTER TABLE "dbo"."StickiedSubmission" ADD CONSTRAINT "PK_StickiedSubmission" PRIMARY KEY ("SubmissionID","Subverse");
ALTER TABLE "dbo"."Submission" ADD CONSTRAINT "PK_Submission" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."SubmissionRemovalLog" ADD CONSTRAINT "PK_SubmissionRemovalLog" PRIMARY KEY ("SubmissionID");
ALTER TABLE "dbo"."SubmissionSaveTracker" ADD CONSTRAINT "PK_SubmissionSaveTracker" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."SubmissionVoteTracker" ADD CONSTRAINT "PK_SubmissionVoteTracker" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."Subverse" ADD CONSTRAINT "PK_Subverse" PRIMARY KEY ("Name");
ALTER TABLE "dbo"."SubverseBan" ADD CONSTRAINT "PK_SubverseBan" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."SubverseFlair" ADD CONSTRAINT "PK_SubverseFlair" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."SubverseModerator" ADD CONSTRAINT "PK_SubverseModerator" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."SubverseSet" ADD CONSTRAINT "PK_SubverseSet" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."SubverseSetList" ADD CONSTRAINT "PK_SubverseSetList" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."SubverseSetSubscription" ADD CONSTRAINT "PK_SubverseSetSubscription" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."UserBadge" ADD CONSTRAINT "PK_UserBadge" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."UserBlockedUser" ADD CONSTRAINT "PK_UserBlockedUser" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."UserContribution" ADD CONSTRAINT "PK_UserContribution" PRIMARY KEY ("ID");
ALTER TABLE "dbo"."UserPreference" ADD CONSTRAINT "PK_UserPreference" PRIMARY KEY ("UserName");
ALTER TABLE "dbo"."ViewStatistic" ADD CONSTRAINT "PK_ViewStatistic" PRIMARY KEY ("SubmissionID","ViewerID");
ALTER TABLE "dbo"."Subverse" ADD CONSTRAINT "IX_Subverse" UNIQUE ("ID");
ALTER TABLE "dbo"."ApiClient" ADD CONSTRAINT "FK_ApiClient_ApiPermissionPolicy" FOREIGN KEY ("ApiPermissionPolicyID") REFERENCES "dbo"."ApiPermissionPolicy" ( "ID");
ALTER TABLE "dbo"."ApiClient" ADD CONSTRAINT "FK_ApiClient_ApiThrottlePolicy" FOREIGN KEY ("ApiThrottlePolicyID") REFERENCES "dbo"."ApiThrottlePolicy" ( "ID");
ALTER TABLE "dbo"."Comment" ADD CONSTRAINT "FK_Comment_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "dbo"."Submission" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "dbo"."CommentRemovalLog" ADD CONSTRAINT "FK_CommentRemovalLog_Comment" FOREIGN KEY ("CommentID") REFERENCES "dbo"."Comment" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "dbo"."CommentSaveTracker" ADD CONSTRAINT "FK_CommentSaveTracker_Comment" FOREIGN KEY ("CommentID") REFERENCES "dbo"."Comment" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "dbo"."CommentVoteTracker" ADD CONSTRAINT "FK_CommentVoteTracker_Comment" FOREIGN KEY ("CommentID") REFERENCES "dbo"."Comment" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "dbo"."DefaultSubverse" ADD CONSTRAINT "FK_DefaultSubverse_Subverse" FOREIGN KEY ("Subverse") REFERENCES "dbo"."Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "dbo"."FeaturedSubverse" ADD CONSTRAINT "FK_FeaturedSubverse_Subverse" FOREIGN KEY ("Subverse") REFERENCES "dbo"."Subverse" ( "Name");
ALTER TABLE "dbo"."RuleReport" ADD CONSTRAINT "FK_RuleReport_RuleSet" FOREIGN KEY ("RuleSetID") REFERENCES "dbo"."RuleSet" ( "ID");
ALTER TABLE "dbo"."StickiedSubmission" ADD CONSTRAINT "FK_StickiedSubmission_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "dbo"."Submission" ( "ID");
ALTER TABLE "dbo"."StickiedSubmission" ADD CONSTRAINT "FK_StickiedSubmission_Subverse" FOREIGN KEY ("Subverse") REFERENCES "dbo"."Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "dbo"."Submission" ADD CONSTRAINT "FK_Submission_Subverse" FOREIGN KEY ("Subverse") REFERENCES "dbo"."Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "dbo"."SubmissionRemovalLog" ADD CONSTRAINT "FK_SubmissionRemovalLog_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "dbo"."Submission" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "dbo"."SubmissionSaveTracker" ADD CONSTRAINT "FK_SubmissionSaveTracker_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "dbo"."Submission" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "dbo"."SubmissionVoteTracker" ADD CONSTRAINT "FK_SubmissionVoteTracker_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "dbo"."Submission" ( "ID");
ALTER TABLE "dbo"."SubverseBan" ADD CONSTRAINT "FK_SubverseBan_Subverse" FOREIGN KEY ("Subverse") REFERENCES "dbo"."Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "dbo"."SubverseFlair" ADD CONSTRAINT "FK_SubverseFlair_Subverse" FOREIGN KEY ("Subverse") REFERENCES "dbo"."Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "dbo"."SubverseModerator" ADD CONSTRAINT "FK_SubverseModerator_Subverse" FOREIGN KEY ("Subverse") REFERENCES "dbo"."Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "dbo"."SubverseSetList" ADD CONSTRAINT "FK_SubverseSetList_Subverse" FOREIGN KEY ("SubverseID") REFERENCES "dbo"."Subverse" ( "ID");
ALTER TABLE "dbo"."SubverseSetList" ADD CONSTRAINT "FK_SubverseSetList_SubverseSet" FOREIGN KEY ("SubverseSetID") REFERENCES "dbo"."SubverseSet" ( "ID");
ALTER TABLE "dbo"."SubverseSetSubscription" ADD CONSTRAINT "FK_SubverseSetSubscription_Set" FOREIGN KEY ("SubverseSetID") REFERENCES "dbo"."SubverseSet" ( "ID");
ALTER TABLE "dbo"."UserBadge" ADD CONSTRAINT "FK_UserBadge_Badge" FOREIGN KEY ("BadgeID") REFERENCES "dbo"."Badge" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "dbo"."ViewStatistic" ADD CONSTRAINT "FK_ViewStatistic_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "dbo"."Submission" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "dbo"."Ad" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."ad_id_seq"');
ALTER TABLE "dbo"."Ad" ALTER COLUMN "IsActive" SET DEFAULT true;
ALTER TABLE "dbo"."AdminLog" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."adminlog_id_seq"');
ALTER TABLE "dbo"."ApiClient" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."apiclient_id_seq"');
ALTER TABLE "dbo"."ApiClient" ALTER COLUMN "IsActive" SET DEFAULT true;
ALTER TABLE "dbo"."ApiCorsPolicy" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."apicorspolicy_id_seq"');
ALTER TABLE "dbo"."ApiLog" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."apilog_id_seq"');
ALTER TABLE "dbo"."ApiPermissionPolicy" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."apipermissionpolicy_id_seq"');
ALTER TABLE "dbo"."ApiThrottlePolicy" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."apithrottlepolicy_id_seq"');
ALTER TABLE "dbo"."BannedDomain" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."banneddomain_id_seq"');
ALTER TABLE "dbo"."BannedUser" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."banneduser_id_seq"');
ALTER TABLE "dbo"."Comment" ALTER COLUMN "DownCount" SET DEFAULT 0;
ALTER TABLE "dbo"."Comment" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."comment_id_seq"');
ALTER TABLE "dbo"."Comment" ALTER COLUMN "IsAnonymized" SET DEFAULT false;
ALTER TABLE "dbo"."Comment" ALTER COLUMN "IsDeleted" SET DEFAULT false;
ALTER TABLE "dbo"."Comment" ALTER COLUMN "IsDistinguished" SET DEFAULT false;
ALTER TABLE "dbo"."Comment" ALTER COLUMN "UpCount" SET DEFAULT 1;
ALTER TABLE "dbo"."CommentSaveTracker" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."commentsavetracker_id_seq"');
ALTER TABLE "dbo"."CommentVoteTracker" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."commentvotetracker_id_seq"');
ALTER TABLE "dbo"."CommentVoteTracker" ALTER COLUMN "VoteValue" SET DEFAULT 0;
ALTER TABLE "dbo"."EventLog" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."eventlog_id_seq"');
ALTER TABLE "dbo"."Featured" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."featured_id_seq"');
ALTER TABLE "dbo"."FeaturedSubverse" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."featuredsubverse_id_seq"');
ALTER TABLE "dbo"."Filter" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."filter_id_seq"');
ALTER TABLE "dbo"."Filter" ALTER COLUMN "IsActive" SET DEFAULT true;
ALTER TABLE "dbo"."Message" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."message_id_seq"');
ALTER TABLE "dbo"."ModeratorInvitation" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."moderatorinvitation_id_seq"');
ALTER TABLE "dbo"."RuleReport" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."rulereport_id_seq"');
ALTER TABLE "dbo"."RuleSet" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."ruleset_id_seq"');
ALTER TABLE "dbo"."RuleSet" ALTER COLUMN "IsActive" SET DEFAULT true;
ALTER TABLE "dbo"."Submission" ALTER COLUMN "DownCount" SET DEFAULT 0;
ALTER TABLE "dbo"."Submission" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."submission_id_seq"');
ALTER TABLE "dbo"."Submission" ALTER COLUMN "IsAdult" SET DEFAULT false;
ALTER TABLE "dbo"."Submission" ALTER COLUMN "IsAnonymized" SET DEFAULT false;
ALTER TABLE "dbo"."Submission" ALTER COLUMN "IsDeleted" SET DEFAULT false;
ALTER TABLE "dbo"."Submission" ALTER COLUMN "Rank" SET DEFAULT 0;
ALTER TABLE "dbo"."Submission" ALTER COLUMN "RelativeRank" SET DEFAULT 0;
ALTER TABLE "dbo"."Submission" ALTER COLUMN "UpCount" SET DEFAULT 1;
ALTER TABLE "dbo"."Submission" ALTER COLUMN "Views" SET DEFAULT 1;
ALTER TABLE "dbo"."SubmissionSaveTracker" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."submissionsavetracker_id_seq"');
ALTER TABLE "dbo"."SubmissionVoteTracker" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."submissionvotetracker_id_seq"');
ALTER TABLE "dbo"."SubmissionVoteTracker" ALTER COLUMN "VoteValue" SET DEFAULT 0;
ALTER TABLE "dbo"."Subverse" ALTER COLUMN "ExcludeSitewideBans" SET DEFAULT false;
ALTER TABLE "dbo"."Subverse" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."subverse_id_seq"');
ALTER TABLE "dbo"."Subverse" ALTER COLUMN "IsAdminPrivate" SET DEFAULT false;
ALTER TABLE "dbo"."Subverse" ALTER COLUMN "IsAdult" SET DEFAULT false;
ALTER TABLE "dbo"."Subverse" ALTER COLUMN "IsAnonymized" SET DEFAULT false;
ALTER TABLE "dbo"."Subverse" ALTER COLUMN "IsAuthorizedOnly" SET DEFAULT false;
ALTER TABLE "dbo"."Subverse" ALTER COLUMN "IsPrivate" SET DEFAULT false;
ALTER TABLE "dbo"."Subverse" ALTER COLUMN "IsThumbnailEnabled" SET DEFAULT true;
ALTER TABLE "dbo"."Subverse" ALTER COLUMN "MinCCPForDownvote" SET DEFAULT 0;
ALTER TABLE "dbo"."SubverseBan" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."subverseban_id_seq"');
ALTER TABLE "dbo"."SubverseFlair" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."subverseflair_id_seq"');
ALTER TABLE "dbo"."SubverseModerator" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."subversemoderator_id_seq"');
ALTER TABLE "dbo"."SubverseSet" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."subverseset_id_seq"');
ALTER TABLE "dbo"."SubverseSet" ALTER COLUMN "IsPublic" SET DEFAULT true;
ALTER TABLE "dbo"."SubverseSet" ALTER COLUMN "SubscriberCount" SET DEFAULT 0;
ALTER TABLE "dbo"."SubverseSetList" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."subversesetlist_id_seq"');
ALTER TABLE "dbo"."SubverseSetSubscription" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."subversesetsubscription_id_seq"');
ALTER TABLE "dbo"."UserBadge" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."userbadge_id_seq"');
ALTER TABLE "dbo"."UserBlockedUser" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."userblockeduser_id_seq"');
ALTER TABLE "dbo"."UserContribution" ALTER COLUMN "ID" SET DEFAULT nextval('"dbo"."usercontribution_id_seq"');
ALTER TABLE "dbo"."UserContribution" ALTER COLUMN "IsReceived" SET DEFAULT true;
ALTER TABLE "dbo"."UserContribution" ALTER COLUMN "VoteCount" SET DEFAULT 0;
ALTER TABLE "dbo"."UserPreference" ALTER COLUMN "BlockAnonymized" SET DEFAULT false;
ALTER TABLE "dbo"."UserPreference" ALTER COLUMN "DisplayAds" SET DEFAULT false;
ALTER TABLE "dbo"."UserPreference" ALTER COLUMN "DisplaySubscriptions" SET DEFAULT false;
ALTER TABLE "dbo"."UserPreference" ALTER COLUMN "UseSubscriptionsMenu" SET DEFAULT true;
ALTER TABLE "dbo"."Ad" ALTER COLUMN "CreationDate" SET DEFAULT now();
ALTER TABLE "dbo"."AdminLog" ALTER COLUMN "CreationDate" SET DEFAULT now();
ALTER TABLE "dbo"."ApiClient" ALTER COLUMN "CreationDate" SET DEFAULT now();
ALTER TABLE "dbo"."ApiCorsPolicy" ALTER COLUMN "CreationDate" SET DEFAULT now();
ALTER TABLE "dbo"."ApiLog" ALTER COLUMN "CreationDate" SET DEFAULT now();
ALTER TABLE "dbo"."EventLog" ALTER COLUMN "CreationDate" SET DEFAULT now();
ALTER TABLE "dbo"."Filter" ALTER COLUMN "CreationDate" SET DEFAULT now();
ALTER TABLE "dbo"."RuleReport" ALTER COLUMN "CreationDate" SET DEFAULT now();
ALTER TABLE "dbo"."RuleSet" ALTER COLUMN "CreationDate" SET DEFAULT now();
ALTER TABLE "dbo"."SessionTracker" ALTER COLUMN "CreationDate" SET DEFAULT now();
ALTER TABLE "dbo"."SubverseSet" ALTER COLUMN "CreationDate" SET DEFAULT now();
ALTER TABLE "dbo"."SubverseSetList" ALTER COLUMN "CreationDate" SET DEFAULT now();
ALTER TABLE "dbo"."SubverseSetSubscription" ALTER COLUMN "CreationDate" SET DEFAULT now();
ALTER TABLE "dbo"."UserBlockedUser" ALTER COLUMN "CreationDate" SET DEFAULT now();
ALTER TABLE "dbo"."UserContribution" ALTER COLUMN "LastUpdateDate" SET DEFAULT now();
select setval('"dbo"."ad_id_seq"',(select max("ID") from "dbo"."Ad")::bigint);
select setval('"dbo"."adminlog_id_seq"',(select max("ID") from "dbo"."AdminLog")::bigint);
select setval('"dbo"."apiclient_id_seq"',(select max("ID") from "dbo"."ApiClient")::bigint);
select setval('"dbo"."apicorspolicy_id_seq"',(select max("ID") from "dbo"."ApiCorsPolicy")::bigint);
select setval('"dbo"."apilog_id_seq"',(select max("ID") from "dbo"."ApiLog")::bigint);
select setval('"dbo"."apipermissionpolicy_id_seq"',(select max("ID") from "dbo"."ApiPermissionPolicy")::bigint);
select setval('"dbo"."apithrottlepolicy_id_seq"',(select max("ID") from "dbo"."ApiThrottlePolicy")::bigint);
select setval('"dbo"."banneddomain_id_seq"',(select max("ID") from "dbo"."BannedDomain")::bigint);
select setval('"dbo"."banneduser_id_seq"',(select max("ID") from "dbo"."BannedUser")::bigint);
select setval('"dbo"."comment_id_seq"',(select max("ID") from "dbo"."Comment")::bigint);
select setval('"dbo"."commentsavetracker_id_seq"',(select max("ID") from "dbo"."CommentSaveTracker")::bigint);
select setval('"dbo"."commentvotetracker_id_seq"',(select max("ID") from "dbo"."CommentVoteTracker")::bigint);
select setval('"dbo"."eventlog_id_seq"',(select max("ID") from "dbo"."EventLog")::bigint);
select setval('"dbo"."featured_id_seq"',(select max("ID") from "dbo"."Featured")::bigint);
select setval('"dbo"."featuredsubverse_id_seq"',(select max("ID") from "dbo"."FeaturedSubverse")::bigint);
select setval('"dbo"."filter_id_seq"',(select max("ID") from "dbo"."Filter")::bigint);
select setval('"dbo"."message_id_seq"',(select max("ID") from "dbo"."Message")::bigint);
select setval('"dbo"."moderatorinvitation_id_seq"',(select max("ID") from "dbo"."ModeratorInvitation")::bigint);
select setval('"dbo"."rulereport_id_seq"',(select max("ID") from "dbo"."RuleReport")::bigint);
select setval('"dbo"."ruleset_id_seq"',(select max("ID") from "dbo"."RuleSet")::bigint);
select setval('"dbo"."submission_id_seq"',(select max("ID") from "dbo"."Submission")::bigint);
select setval('"dbo"."submissionsavetracker_id_seq"',(select max("ID") from "dbo"."SubmissionSaveTracker")::bigint);
select setval('"dbo"."submissionvotetracker_id_seq"',(select max("ID") from "dbo"."SubmissionVoteTracker")::bigint);
select setval('"dbo"."subverse_id_seq"',(select max("ID") from "dbo"."Subverse")::bigint);
select setval('"dbo"."subverseban_id_seq"',(select max("ID") from "dbo"."SubverseBan")::bigint);
select setval('"dbo"."subverseflair_id_seq"',(select max("ID") from "dbo"."SubverseFlair")::bigint);
select setval('"dbo"."subversemoderator_id_seq"',(select max("ID") from "dbo"."SubverseModerator")::bigint);
select setval('"dbo"."subverseset_id_seq"',(select max("ID") from "dbo"."SubverseSet")::bigint);
select setval('"dbo"."subversesetlist_id_seq"',(select max("ID") from "dbo"."SubverseSetList")::bigint);
select setval('"dbo"."subversesetsubscription_id_seq"',(select max("ID") from "dbo"."SubverseSetSubscription")::bigint);
select setval('"dbo"."userbadge_id_seq"',(select max("ID") from "dbo"."UserBadge")::bigint);
select setval('"dbo"."userblockeduser_id_seq"',(select max("ID") from "dbo"."UserBlockedUser")::bigint);
select setval('"dbo"."usercontribution_id_seq"',(select max("ID") from "dbo"."UserContribution")::bigint);

COMMIT;
