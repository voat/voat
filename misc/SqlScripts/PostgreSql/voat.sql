BEGIN;
CREATE EXTENSION IF NOT EXISTS citext;
SET TIME ZONE 'UTC';

CREATE TABLE IF NOT EXISTS "Ad"( 
	"ID" int NOT NULL,
	"IsActive" boolean NOT NULL,
	"GraphicUrl" varchar(100) NOT NULL,
	"DestinationUrl" varchar(1000),
	"Name" varchar(100) NOT NULL,
	"Description" varchar(2000) NOT NULL,
	"StartDate" timestamp,
	"EndDate" timestamp,
	"Subverse" varchar(50),
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "AdminLog"( 
	"ID" int NOT NULL,
	"UserName" varchar(20) NOT NULL,
	"RefUserName" varchar(20),
	"RefSubverse" varchar(50),
	"RefUrl" varchar(200),
	"RefCommentID" int,
	"RefSubmissionID" int,
	"Type" varchar(100),
	"Action" varchar(100) NOT NULL,
	"Details" varchar(1000) NOT NULL,
	"InternalDetails" varchar,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "ApiClient"( 
	"ID" int NOT NULL,
	"IsActive" boolean NOT NULL,
	"UserName" varchar(100),
	"AppName" varchar(50) NOT NULL,
	"AppDescription" varchar(2000),
	"AppAboutUrl" varchar(200),
	"RedirectUrl" varchar(200),
	"PublicKey" varchar(100) NOT NULL,
	"PrivateKey" varchar(100) NOT NULL,
	"LastAccessDate" timestamp,
	"CreationDate" timestamp NOT NULL,
	"ApiThrottlePolicyID" int,
	"ApiPermissionPolicyID" int);

CREATE TABLE IF NOT EXISTS "ApiCorsPolicy"( 
	"ID" int NOT NULL,
	"IsActive" boolean NOT NULL,
	"AllowOrigin" varchar(100) NOT NULL,
	"AllowMethods" varchar(100) NOT NULL,
	"AllowHeaders" varchar(100) NOT NULL,
	"AllowCredentials" boolean,
	"MaxAge" int,
	"UserName" varchar(100),
	"Description" varchar(500),
	"CreatedBy" varchar(100) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "ApiLog"( 
	"ID" int NOT NULL,
	"ApiClientID" int NOT NULL,
	"Method" varchar(10) NOT NULL,
	"Url" varchar(500) NOT NULL,
	"Headers" varchar,
	"Body" varchar,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "ApiPermissionPolicy"( 
	"ID" int NOT NULL,
	"Name" varchar(100) NOT NULL,
	"Policy" varchar(2000) NOT NULL);

CREATE TABLE IF NOT EXISTS "ApiThrottlePolicy"( 
	"ID" int NOT NULL,
	"Name" varchar(100) NOT NULL,
	"Policy" varchar(2000) NOT NULL);

CREATE TABLE IF NOT EXISTS "Badge"( 
	"ID" varchar(50) NOT NULL,
	"Graphic" varchar(50) NOT NULL,
	"Title" varchar(300) NOT NULL,
	"Name" varchar(50) NOT NULL);

CREATE TABLE IF NOT EXISTS "BannedDomain"( 
	"ID" int NOT NULL,
	"Domain" varchar(100) NOT NULL,
	"CreatedBy" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"Reason" varchar(500) NOT NULL);

CREATE TABLE IF NOT EXISTS "BannedUser"( 
	"ID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"Reason" varchar(500) NOT NULL,
	"CreatedBy" varchar(50) NOT NULL);

CREATE TABLE IF NOT EXISTS "Comment"( 
	"ID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"Content" varchar NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"LastEditDate" timestamp,
	"SubmissionID" int,
	"UpCount" int NOT NULL,
	"DownCount" int NOT NULL,
	"ParentID" int,
	"IsAnonymized" boolean NOT NULL,
	"IsDistinguished" boolean NOT NULL,
	"FormattedContent" varchar,
	"IsDeleted" boolean NOT NULL);

CREATE TABLE IF NOT EXISTS "CommentRemovalLog"( 
	"CommentID" int NOT NULL,
	"Moderator" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"Reason" varchar(500) NOT NULL);

CREATE TABLE IF NOT EXISTS "CommentSaveTracker"( 
	"ID" int NOT NULL,
	"CommentID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "CommentVoteTracker"( 
	"ID" int NOT NULL,
	"CommentID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"VoteStatus" int NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"IPAddress" varchar(90),
	"VoteValue" double precision NOT NULL);

CREATE TABLE IF NOT EXISTS "DefaultSubverse"( 
	"Subverse" varchar(20) NOT NULL,
	"Order" int NOT NULL);

CREATE TABLE IF NOT EXISTS "EventLog"( 
	"ID" int NOT NULL,
	"ParentID" int,
	"ActivityID" varchar(50),
	"UserName" varchar(100),
	"Origin" varchar(100),
	"Type" varchar(300) NOT NULL,
	"Message" varchar(1500) NOT NULL,
	"Category" varchar(1000) NOT NULL,
	"Exception" varchar,
	"Data" varchar,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "Featured"( 
	"ID" int NOT NULL,
	"DomainType" int NOT NULL,
	"DomainID" int NOT NULL,
	"Title" varchar(100),
	"Description" varchar(500),
	"StartDate" timestamp NOT NULL,
	"EndDate" timestamp,
	"CreatedBy" varchar(50) NOT NULL);

CREATE TABLE IF NOT EXISTS "Filter"( 
	"ID" int NOT NULL,
	"IsActive" boolean NOT NULL,
	"Name" varchar(100) NOT NULL,
	"Description" varchar(1000),
	"Pattern" varchar(100) NOT NULL,
	"Replacement" varchar(1000),
	"AppliesTo" int,
	"Action" int,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "Message"( 
	"ID" int NOT NULL,
	"CorrelationID" varchar(36) NOT NULL,
	"ParentID" int,
	"Type" int NOT NULL,
	"Sender" varchar(50) NOT NULL,
	"SenderType" int NOT NULL,
	"Recipient" varchar(50) NOT NULL,
	"RecipientType" int NOT NULL,
	"Title" varchar(500),
	"Content" varchar,
	"FormattedContent" varchar,
	"Subverse" varchar(20),
	"SubmissionID" int,
	"CommentID" int,
	"IsAnonymized" boolean NOT NULL,
	"ReadDate" timestamp,
	"CreatedBy" varchar(50),
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "ModeratorInvitation"( 
	"ID" int NOT NULL,
	"CreatedBy" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"Recipient" varchar(50) NOT NULL,
	"Subverse" varchar(50) NOT NULL,
	"Power" int NOT NULL);

CREATE TABLE IF NOT EXISTS "RuleReport"( 
	"ID" bigint NOT NULL,
	"Subverse" varchar(50),
	"UserName" varchar(100),
	"SubmissionID" int,
	"CommentID" int,
	"RuleSetID" int NOT NULL,
	"ReviewedBy" varchar(100),
	"ReviewedDate" timestamp,
	"CreatedBy" varchar(100) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "RuleSet"( 
	"ID" int NOT NULL,
	"IsActive" boolean NOT NULL,
	"Subverse" varchar(50),
	"ContentType" smallint,
	"SortOrder" int,
	"Name" varchar(200) NOT NULL,
	"Description" varchar(1000),
	"CreatedBy" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "SessionTracker"( 
	"SessionID" varchar(90) NOT NULL,
	"Subverse" varchar(20) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "SitePage"( 
	"ID" int NOT NULL,
	"Name" varchar(50) NOT NULL,
	"Content" varchar,
	"UpdatedBy" varchar(50),
	"LastUpdateDate" timestamp);

CREATE TABLE IF NOT EXISTS "StickiedSubmission"( 
	"SubmissionID" int NOT NULL,
	"Subverse" varchar(20) NOT NULL,
	"CreatedBy" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "Submission"( 
	"ID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"Content" varchar,
	"CreationDate" timestamp NOT NULL,
	"Type" int NOT NULL,
	"Title" varchar(200),
	"Rank" double precision NOT NULL,
	"Subverse" varchar(20),
	"UpCount" int NOT NULL,
	"DownCount" int NOT NULL,
	"Thumbnail" varchar(150),
	"LastEditDate" timestamp,
	"FlairLabel" varchar(50),
	"FlairCss" varchar(50),
	"IsAnonymized" boolean NOT NULL,
	"Views" double precision NOT NULL,
	"IsDeleted" boolean NOT NULL,
	"RelativeRank" double precision NOT NULL,
	"Url" varchar(3000),
	"DomainReversed" varchar(3000) NULL,
	"FormattedContent" varchar,
	"IsAdult" boolean NOT NULL,
	"ArchiveDate" timestamp);

CREATE TABLE IF NOT EXISTS "SubmissionRemovalLog"( 
	"SubmissionID" int NOT NULL,
	"Moderator" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"Reason" varchar(500) NOT NULL);

CREATE TABLE IF NOT EXISTS "SubmissionSaveTracker"( 
	"ID" int NOT NULL,
	"SubmissionID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "SubmissionVoteTracker"( 
	"ID" int NOT NULL,
	"SubmissionID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"VoteStatus" int NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"IPAddress" varchar(90),
	"VoteValue" double precision NOT NULL);

CREATE TABLE IF NOT EXISTS "Subverse"( 
	"ID" int NOT NULL,
	"Name" varchar(20) NOT NULL,
	"Title" varchar(100) NOT NULL,
	"Description" varchar(500),
	"SideBar" varchar(4000),
	"IsAdult" boolean NOT NULL,
	"IsThumbnailEnabled" boolean NOT NULL,
	"ExcludeSitewideBans" boolean NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"Stylesheet" varchar,
	"SubscriberCount" int,
	"IsPrivate" boolean NOT NULL,
	"IsAuthorizedOnly" boolean NOT NULL,
	"IsAnonymized" boolean,
	"LastSubmissionDate" timestamp,
	"MinCCPForDownvote" int NOT NULL,
	"IsAdminPrivate" boolean NOT NULL,
	"IsAdminDisabled" boolean,
	"CreatedBy" varchar(50),
	"LastUpdateDate" timestamp);

CREATE TABLE IF NOT EXISTS "SubverseBan"( 
	"ID" int NOT NULL,
	"Subverse" varchar(20) NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"CreatedBy" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"Reason" varchar(500) NOT NULL);

CREATE TABLE IF NOT EXISTS "SubverseFlair"( 
	"ID" int NOT NULL,
	"Subverse" varchar(20) NOT NULL,
	"Label" varchar(50),
	"CssClass" varchar(50));

CREATE TABLE IF NOT EXISTS "SubverseModerator"( 
	"ID" int NOT NULL,
	"Subverse" varchar(20) NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"Power" int NOT NULL,
	"CreatedBy" varchar(50),
	"CreationDate" timestamp);

CREATE TABLE IF NOT EXISTS "SubverseSet"( 
	"ID" int NOT NULL,
	"Name" varchar(20) NOT NULL,
	"Title" varchar(100),
	"Description" varchar(500),
	"UserName" varchar(50),
	"Type" int NOT NULL,
	"IsPublic" boolean NOT NULL,
	"SubscriberCount" int NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "SubverseSetList"( 
	"ID" int NOT NULL,
	"SubverseSetID" int NOT NULL,
	"SubverseID" int NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "SubverseSetSubscription"( 
	"ID" int NOT NULL,
	"SubverseSetID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "UserBadge"( 
	"ID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"BadgeID" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "UserBlockedUser"( 
	"ID" int NOT NULL,
	"BlockUser" varchar(50) NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "UserContribution"( 
	"ID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"IsReceived" boolean NOT NULL,
	"ContentType" int NOT NULL,
	"VoteStatus" int NOT NULL,
	"VoteCount" int NOT NULL,
	"VoteValue" double precision NOT NULL,
	"ValidThroughDate" timestamp NOT NULL,
	"LastUpdateDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "UserPreference"( 
	"UserName" varchar(50) NOT NULL,
	"DisableCSS" boolean NOT NULL,
	"NightMode" boolean NOT NULL,
	"Language" varchar(50) NOT NULL,
	"OpenInNewWindow" boolean NOT NULL,
	"EnableAdultContent" boolean NOT NULL,
	"DisplayVotes" boolean NOT NULL,
	"DisplaySubscriptions" boolean NOT NULL,
	"UseSubscriptionsMenu" boolean NOT NULL,
	"Bio" varchar(100),
	"Avatar" varchar(50),
	"DisplayAds" boolean NOT NULL,
	"DisplayCommentCount" int,
	"HighlightMinutes" int,
	"VanityTitle" varchar(50),
	"CollapseCommentLimit" int,
	"BlockAnonymized" boolean NOT NULL,
	"CommentSort" int,
	"DisplayThumbnails" boolean NOT NULL);

CREATE TABLE IF NOT EXISTS "ViewStatistic"( 
	"SubmissionID" int NOT NULL,
	"ViewerID" varchar(90) NOT NULL);

CREATE TABLE IF NOT EXISTS "Vote"( 
	"ID" int NOT NULL,
	"Title" varchar(200) NOT NULL,
	"Content" varchar,
	"FormattedContent" varchar,
	"Subverse" varchar(50),
	"SubmissionID" int,
	"DisplayStatistics" boolean NOT NULL,
	"Status" int,
	"StartDate" timestamp NOT NULL,
	"EndDate" timestamp NOT NULL,
	"LastEditDate" timestamp,
	"ProcessedDate" timestamp,
	"CreatedBy" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE IF NOT EXISTS "VoteOption"( 
	"ID" int NOT NULL,
	"VoteID" int NOT NULL,
	"Title" varchar(200) NOT NULL,
	"Content" varchar,
	"FormattedContent" varchar,
	"SortOrder" int NOT NULL);

CREATE TABLE IF NOT EXISTS "VoteOutcome"( 
	"ID" int NOT NULL,
	"VoteOptionID" int NOT NULL,
	"Type" varchar(1000) NOT NULL,
	"Data" varchar NOT NULL);

CREATE TABLE IF NOT EXISTS "VoteRestriction"( 
	"ID" int NOT NULL,
	"VoteID" int NOT NULL,
	"Type" varchar(1000) NOT NULL,
	"Data" varchar NOT NULL);

CREATE TABLE IF NOT EXISTS "VoteTracker"( 
	"ID" int NOT NULL,
	"VoteID" int NOT NULL,
	"VoteOptionID" int NOT NULL,
	"RestrictionsPassed" boolean NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE SEQUENCE IF NOT EXISTS "ad_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "Ad"."ID";
CREATE SEQUENCE IF NOT EXISTS "adminlog_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "AdminLog"."ID";
CREATE SEQUENCE IF NOT EXISTS "apiclient_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "ApiClient"."ID";
CREATE SEQUENCE IF NOT EXISTS "apicorspolicy_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "ApiCorsPolicy"."ID";
CREATE SEQUENCE IF NOT EXISTS "apilog_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "ApiLog"."ID";
CREATE SEQUENCE IF NOT EXISTS "apipermissionpolicy_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "ApiPermissionPolicy"."ID";
CREATE SEQUENCE IF NOT EXISTS "apithrottlepolicy_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "ApiThrottlePolicy"."ID";
CREATE SEQUENCE IF NOT EXISTS "banneddomain_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "BannedDomain"."ID";
CREATE SEQUENCE IF NOT EXISTS "banneduser_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "BannedUser"."ID";
CREATE SEQUENCE IF NOT EXISTS "comment_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "Comment"."ID";
CREATE SEQUENCE IF NOT EXISTS "commentsavetracker_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "CommentSaveTracker"."ID";
CREATE SEQUENCE IF NOT EXISTS "commentvotetracker_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "CommentVoteTracker"."ID";
CREATE SEQUENCE IF NOT EXISTS "eventlog_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "EventLog"."ID";
CREATE SEQUENCE IF NOT EXISTS "featured_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "Featured"."ID";
CREATE SEQUENCE IF NOT EXISTS "filter_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "Filter"."ID";
CREATE SEQUENCE IF NOT EXISTS "message_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "Message"."ID";
CREATE SEQUENCE IF NOT EXISTS "moderatorinvitation_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "ModeratorInvitation"."ID";
CREATE SEQUENCE IF NOT EXISTS "rulereport_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "RuleReport"."ID";
CREATE SEQUENCE IF NOT EXISTS "ruleset_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "RuleSet"."ID";
CREATE SEQUENCE IF NOT EXISTS "sitepage_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SitePage"."ID";
CREATE SEQUENCE IF NOT EXISTS "submission_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "Submission"."ID";
CREATE SEQUENCE IF NOT EXISTS "submissionsavetracker_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SubmissionSaveTracker"."ID";
CREATE SEQUENCE IF NOT EXISTS "submissionvotetracker_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SubmissionVoteTracker"."ID";
CREATE SEQUENCE IF NOT EXISTS "subverse_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "Subverse"."ID";
CREATE SEQUENCE IF NOT EXISTS "subverseban_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SubverseBan"."ID";
CREATE SEQUENCE IF NOT EXISTS "subverseflair_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SubverseFlair"."ID";
CREATE SEQUENCE IF NOT EXISTS "subversemoderator_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SubverseModerator"."ID";
CREATE SEQUENCE IF NOT EXISTS "subverseset_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SubverseSet"."ID";
CREATE SEQUENCE IF NOT EXISTS "subversesetlist_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SubverseSetList"."ID";
CREATE SEQUENCE IF NOT EXISTS "subversesetsubscription_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SubverseSetSubscription"."ID";
CREATE SEQUENCE IF NOT EXISTS "userbadge_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "UserBadge"."ID";
CREATE SEQUENCE IF NOT EXISTS "userblockeduser_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "UserBlockedUser"."ID";
CREATE SEQUENCE IF NOT EXISTS "usercontribution_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "UserContribution"."ID";
CREATE SEQUENCE IF NOT EXISTS "vote_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "Vote"."ID";
CREATE SEQUENCE IF NOT EXISTS "voteoption_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "VoteOption"."ID";
CREATE SEQUENCE IF NOT EXISTS "voteoutcome_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "VoteOutcome"."ID";
CREATE SEQUENCE IF NOT EXISTS "voterestriction_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "VoteRestriction"."ID";
CREATE SEQUENCE IF NOT EXISTS "votetracker_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "VoteTracker"."ID";
-- Constraints are only added when missing so this script is safe to re-run.
DO $$
BEGIN
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_Ad') THEN ALTER TABLE "Ad" ADD CONSTRAINT "PK_Ad" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_AdminLog') THEN ALTER TABLE "AdminLog" ADD CONSTRAINT "PK_AdminLog" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_ApiClient') THEN ALTER TABLE "ApiClient" ADD CONSTRAINT "PK_ApiClient" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_ApiCorsPolicy') THEN ALTER TABLE "ApiCorsPolicy" ADD CONSTRAINT "PK_ApiCorsPolicy" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_ApiLog') THEN ALTER TABLE "ApiLog" ADD CONSTRAINT "PK_ApiLog" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_ApiPermissionPolicy') THEN ALTER TABLE "ApiPermissionPolicy" ADD CONSTRAINT "PK_ApiPermissionPolicy" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_ApiThrottlePolicy') THEN ALTER TABLE "ApiThrottlePolicy" ADD CONSTRAINT "PK_ApiThrottlePolicy" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_Badge') THEN ALTER TABLE "Badge" ADD CONSTRAINT "PK_Badge" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_BannedDomain') THEN ALTER TABLE "BannedDomain" ADD CONSTRAINT "PK_BannedDomain" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_BannedUser') THEN ALTER TABLE "BannedUser" ADD CONSTRAINT "PK_BannedUser" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_Comment') THEN ALTER TABLE "Comment" ADD CONSTRAINT "PK_Comment" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_CommentRemovalLog') THEN ALTER TABLE "CommentRemovalLog" ADD CONSTRAINT "PK_CommentRemovalLog" PRIMARY KEY ("CommentID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_CommentSaveTracker') THEN ALTER TABLE "CommentSaveTracker" ADD CONSTRAINT "PK_CommentSaveTracker" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_CommentVoteTracker') THEN ALTER TABLE "CommentVoteTracker" ADD CONSTRAINT "PK_CommentVoteTracker" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_DefaultSubverse') THEN ALTER TABLE "DefaultSubverse" ADD CONSTRAINT "PK_DefaultSubverse" PRIMARY KEY ("Subverse"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_EventLog') THEN ALTER TABLE "EventLog" ADD CONSTRAINT "PK_EventLog" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_Featured') THEN ALTER TABLE "Featured" ADD CONSTRAINT "PK_Featured" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_Filter') THEN ALTER TABLE "Filter" ADD CONSTRAINT "PK_Filter" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_Message') THEN ALTER TABLE "Message" ADD CONSTRAINT "PK_Message" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_ModeratorInvitation') THEN ALTER TABLE "ModeratorInvitation" ADD CONSTRAINT "PK_ModeratorInvitation" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_RuleReport') THEN ALTER TABLE "RuleReport" ADD CONSTRAINT "PK_RuleReport" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_RuleSet') THEN ALTER TABLE "RuleSet" ADD CONSTRAINT "PK_RuleSet" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_SessionTracker') THEN ALTER TABLE "SessionTracker" ADD CONSTRAINT "PK_SessionTracker" PRIMARY KEY ("SessionID","Subverse"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_SitePage') THEN ALTER TABLE "SitePage" ADD CONSTRAINT "PK_SitePage" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_StickiedSubmission') THEN ALTER TABLE "StickiedSubmission" ADD CONSTRAINT "PK_StickiedSubmission" PRIMARY KEY ("SubmissionID","Subverse"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_Submission') THEN ALTER TABLE "Submission" ADD CONSTRAINT "PK_Submission" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_SubmissionRemovalLog') THEN ALTER TABLE "SubmissionRemovalLog" ADD CONSTRAINT "PK_SubmissionRemovalLog" PRIMARY KEY ("SubmissionID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_SubmissionSaveTracker') THEN ALTER TABLE "SubmissionSaveTracker" ADD CONSTRAINT "PK_SubmissionSaveTracker" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_SubmissionVoteTracker') THEN ALTER TABLE "SubmissionVoteTracker" ADD CONSTRAINT "PK_SubmissionVoteTracker" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_Subverse') THEN ALTER TABLE "Subverse" ADD CONSTRAINT "PK_Subverse" PRIMARY KEY ("Name"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_SubverseBan') THEN ALTER TABLE "SubverseBan" ADD CONSTRAINT "PK_SubverseBan" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_SubverseFlair') THEN ALTER TABLE "SubverseFlair" ADD CONSTRAINT "PK_SubverseFlair" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_SubverseModerator') THEN ALTER TABLE "SubverseModerator" ADD CONSTRAINT "PK_SubverseModerator" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_SubverseSet') THEN ALTER TABLE "SubverseSet" ADD CONSTRAINT "PK_SubverseSet" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_SubverseSetList') THEN ALTER TABLE "SubverseSetList" ADD CONSTRAINT "PK_SubverseSetList" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_SubverseSetSubscription') THEN ALTER TABLE "SubverseSetSubscription" ADD CONSTRAINT "PK_SubverseSetSubscription" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_UserBadge') THEN ALTER TABLE "UserBadge" ADD CONSTRAINT "PK_UserBadge" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_UserBlockedUser') THEN ALTER TABLE "UserBlockedUser" ADD CONSTRAINT "PK_UserBlockedUser" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_UserContribution') THEN ALTER TABLE "UserContribution" ADD CONSTRAINT "PK_UserContribution" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_UserPreference') THEN ALTER TABLE "UserPreference" ADD CONSTRAINT "PK_UserPreference" PRIMARY KEY ("UserName"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_ViewStatistic') THEN ALTER TABLE "ViewStatistic" ADD CONSTRAINT "PK_ViewStatistic" PRIMARY KEY ("SubmissionID","ViewerID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_Vote') THEN ALTER TABLE "Vote" ADD CONSTRAINT "PK_Vote" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_VoteOption') THEN ALTER TABLE "VoteOption" ADD CONSTRAINT "PK_VoteOption" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_VoteOutcome') THEN ALTER TABLE "VoteOutcome" ADD CONSTRAINT "PK_VoteOutcome" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_VoteRestriction') THEN ALTER TABLE "VoteRestriction" ADD CONSTRAINT "PK_VoteRestriction" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'PK_VoteTracker') THEN ALTER TABLE "VoteTracker" ADD CONSTRAINT "PK_VoteTracker" PRIMARY KEY ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'IX_SitePage_Name') THEN ALTER TABLE "SitePage" ADD CONSTRAINT "IX_SitePage_Name" UNIQUE ("Name"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'IX_Subverse') THEN ALTER TABLE "Subverse" ADD CONSTRAINT "IX_Subverse" UNIQUE ("ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_ApiClient_ApiPermissionPolicy') THEN ALTER TABLE "ApiClient" ADD CONSTRAINT "FK_ApiClient_ApiPermissionPolicy" FOREIGN KEY ("ApiPermissionPolicyID") REFERENCES "ApiPermissionPolicy" ( "ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_ApiClient_ApiThrottlePolicy') THEN ALTER TABLE "ApiClient" ADD CONSTRAINT "FK_ApiClient_ApiThrottlePolicy" FOREIGN KEY ("ApiThrottlePolicyID") REFERENCES "ApiThrottlePolicy" ( "ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Comment_Submission') THEN ALTER TABLE "Comment" ADD CONSTRAINT "FK_Comment_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_CommentRemovalLog_Comment') THEN ALTER TABLE "CommentRemovalLog" ADD CONSTRAINT "FK_CommentRemovalLog_Comment" FOREIGN KEY ("CommentID") REFERENCES "Comment" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_CommentSaveTracker_Comment') THEN ALTER TABLE "CommentSaveTracker" ADD CONSTRAINT "FK_CommentSaveTracker_Comment" FOREIGN KEY ("CommentID") REFERENCES "Comment" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_CommentVoteTracker_Comment') THEN ALTER TABLE "CommentVoteTracker" ADD CONSTRAINT "FK_CommentVoteTracker_Comment" FOREIGN KEY ("CommentID") REFERENCES "Comment" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_DefaultSubverse_Subverse') THEN ALTER TABLE "DefaultSubverse" ADD CONSTRAINT "FK_DefaultSubverse_Subverse" FOREIGN KEY ("Subverse") REFERENCES "Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_RuleReport_RuleSet') THEN ALTER TABLE "RuleReport" ADD CONSTRAINT "FK_RuleReport_RuleSet" FOREIGN KEY ("RuleSetID") REFERENCES "RuleSet" ( "ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_StickiedSubmission_Submission') THEN ALTER TABLE "StickiedSubmission" ADD CONSTRAINT "FK_StickiedSubmission_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ( "ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_StickiedSubmission_Subverse') THEN ALTER TABLE "StickiedSubmission" ADD CONSTRAINT "FK_StickiedSubmission_Subverse" FOREIGN KEY ("Subverse") REFERENCES "Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Submission_Subverse') THEN ALTER TABLE "Submission" ADD CONSTRAINT "FK_Submission_Subverse" FOREIGN KEY ("Subverse") REFERENCES "Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_SubmissionRemovalLog_Submission') THEN ALTER TABLE "SubmissionRemovalLog" ADD CONSTRAINT "FK_SubmissionRemovalLog_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_SubmissionSaveTracker_Submission') THEN ALTER TABLE "SubmissionSaveTracker" ADD CONSTRAINT "FK_SubmissionSaveTracker_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_SubmissionVoteTracker_Submission') THEN ALTER TABLE "SubmissionVoteTracker" ADD CONSTRAINT "FK_SubmissionVoteTracker_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ( "ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_SubverseBan_Subverse') THEN ALTER TABLE "SubverseBan" ADD CONSTRAINT "FK_SubverseBan_Subverse" FOREIGN KEY ("Subverse") REFERENCES "Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_SubverseFlair_Subverse') THEN ALTER TABLE "SubverseFlair" ADD CONSTRAINT "FK_SubverseFlair_Subverse" FOREIGN KEY ("Subverse") REFERENCES "Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_SubverseModerator_Subverse') THEN ALTER TABLE "SubverseModerator" ADD CONSTRAINT "FK_SubverseModerator_Subverse" FOREIGN KEY ("Subverse") REFERENCES "Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_SubverseSetList_Subverse') THEN ALTER TABLE "SubverseSetList" ADD CONSTRAINT "FK_SubverseSetList_Subverse" FOREIGN KEY ("SubverseID") REFERENCES "Subverse" ( "ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_SubverseSetList_SubverseSet') THEN ALTER TABLE "SubverseSetList" ADD CONSTRAINT "FK_SubverseSetList_SubverseSet" FOREIGN KEY ("SubverseSetID") REFERENCES "SubverseSet" ( "ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_SubverseSetSubscription_Set') THEN ALTER TABLE "SubverseSetSubscription" ADD CONSTRAINT "FK_SubverseSetSubscription_Set" FOREIGN KEY ("SubverseSetID") REFERENCES "SubverseSet" ( "ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_UserBadge_Badge') THEN ALTER TABLE "UserBadge" ADD CONSTRAINT "FK_UserBadge_Badge" FOREIGN KEY ("BadgeID") REFERENCES "Badge" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_ViewStatistic_Submission') THEN ALTER TABLE "ViewStatistic" ADD CONSTRAINT "FK_ViewStatistic_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE; END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_VoteOption_Vote') THEN ALTER TABLE "VoteOption" ADD CONSTRAINT "FK_VoteOption_Vote" FOREIGN KEY ("VoteID") REFERENCES "Vote" ( "ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_VoteOutcome_VoteOption') THEN ALTER TABLE "VoteOutcome" ADD CONSTRAINT "FK_VoteOutcome_VoteOption" FOREIGN KEY ("VoteOptionID") REFERENCES "VoteOption" ( "ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_VoteRestriction_Vote') THEN ALTER TABLE "VoteRestriction" ADD CONSTRAINT "FK_VoteRestriction_Vote" FOREIGN KEY ("VoteID") REFERENCES "Vote" ( "ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_VoteTracker_Vote') THEN ALTER TABLE "VoteTracker" ADD CONSTRAINT "FK_VoteTracker_Vote" FOREIGN KEY ("VoteID") REFERENCES "Vote" ( "ID"); END IF;
IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_VoteTracker_VoteOption') THEN ALTER TABLE "VoteTracker" ADD CONSTRAINT "FK_VoteTracker_VoteOption" FOREIGN KEY ("VoteOptionID") REFERENCES "VoteOption" ( "ID"); END IF;
END
$$;
ALTER TABLE "Ad" ALTER COLUMN "ID" SET DEFAULT nextval('"ad_id_seq"');
ALTER TABLE "Ad" ALTER COLUMN "IsActive" SET DEFAULT true;
ALTER TABLE "AdminLog" ALTER COLUMN "ID" SET DEFAULT nextval('"adminlog_id_seq"');
ALTER TABLE "ApiClient" ALTER COLUMN "ID" SET DEFAULT nextval('"apiclient_id_seq"');
ALTER TABLE "ApiClient" ALTER COLUMN "IsActive" SET DEFAULT true;
ALTER TABLE "ApiCorsPolicy" ALTER COLUMN "ID" SET DEFAULT nextval('"apicorspolicy_id_seq"');
ALTER TABLE "ApiLog" ALTER COLUMN "ID" SET DEFAULT nextval('"apilog_id_seq"');
ALTER TABLE "ApiPermissionPolicy" ALTER COLUMN "ID" SET DEFAULT nextval('"apipermissionpolicy_id_seq"');
ALTER TABLE "ApiThrottlePolicy" ALTER COLUMN "ID" SET DEFAULT nextval('"apithrottlepolicy_id_seq"');
ALTER TABLE "BannedDomain" ALTER COLUMN "ID" SET DEFAULT nextval('"banneddomain_id_seq"');
ALTER TABLE "BannedUser" ALTER COLUMN "ID" SET DEFAULT nextval('"banneduser_id_seq"');
ALTER TABLE "Comment" ALTER COLUMN "DownCount" SET DEFAULT 0;
ALTER TABLE "Comment" ALTER COLUMN "ID" SET DEFAULT nextval('"comment_id_seq"');
ALTER TABLE "Comment" ALTER COLUMN "IsAnonymized" SET DEFAULT false;
ALTER TABLE "Comment" ALTER COLUMN "IsDeleted" SET DEFAULT false;
ALTER TABLE "Comment" ALTER COLUMN "IsDistinguished" SET DEFAULT false;
ALTER TABLE "Comment" ALTER COLUMN "UpCount" SET DEFAULT 0;
ALTER TABLE "CommentSaveTracker" ALTER COLUMN "ID" SET DEFAULT nextval('"commentsavetracker_id_seq"');
ALTER TABLE "CommentVoteTracker" ALTER COLUMN "ID" SET DEFAULT nextval('"commentvotetracker_id_seq"');
ALTER TABLE "CommentVoteTracker" ALTER COLUMN "VoteValue" SET DEFAULT 0;
ALTER TABLE "EventLog" ALTER COLUMN "ID" SET DEFAULT nextval('"eventlog_id_seq"');
ALTER TABLE "Featured" ALTER COLUMN "ID" SET DEFAULT nextval('"featured_id_seq"');
ALTER TABLE "Filter" ALTER COLUMN "ID" SET DEFAULT nextval('"filter_id_seq"');
ALTER TABLE "Filter" ALTER COLUMN "IsActive" SET DEFAULT true;
ALTER TABLE "Message" ALTER COLUMN "ID" SET DEFAULT nextval('"message_id_seq"');
ALTER TABLE "ModeratorInvitation" ALTER COLUMN "ID" SET DEFAULT nextval('"moderatorinvitation_id_seq"');
ALTER TABLE "RuleReport" ALTER COLUMN "ID" SET DEFAULT nextval('"rulereport_id_seq"');
ALTER TABLE "RuleSet" ALTER COLUMN "ID" SET DEFAULT nextval('"ruleset_id_seq"');
ALTER TABLE "RuleSet" ALTER COLUMN "IsActive" SET DEFAULT true;
ALTER TABLE "SitePage" ALTER COLUMN "ID" SET DEFAULT nextval('"sitepage_id_seq"');
ALTER TABLE "Submission" ALTER COLUMN "DownCount" SET DEFAULT 0;
ALTER TABLE "Submission" ALTER COLUMN "ID" SET DEFAULT nextval('"submission_id_seq"');
ALTER TABLE "Submission" ALTER COLUMN "IsAdult" SET DEFAULT false;
ALTER TABLE "Submission" ALTER COLUMN "IsAnonymized" SET DEFAULT false;
ALTER TABLE "Submission" ALTER COLUMN "IsDeleted" SET DEFAULT false;
ALTER TABLE "Submission" ALTER COLUMN "Rank" SET DEFAULT 0;
ALTER TABLE "Submission" ALTER COLUMN "RelativeRank" SET DEFAULT 0;
ALTER TABLE "Submission" ALTER COLUMN "UpCount" SET DEFAULT 1;
ALTER TABLE "Submission" ALTER COLUMN "Views" SET DEFAULT 1;
ALTER TABLE "SubmissionSaveTracker" ALTER COLUMN "ID" SET DEFAULT nextval('"submissionsavetracker_id_seq"');
ALTER TABLE "SubmissionVoteTracker" ALTER COLUMN "ID" SET DEFAULT nextval('"submissionvotetracker_id_seq"');
ALTER TABLE "SubmissionVoteTracker" ALTER COLUMN "VoteValue" SET DEFAULT 0;
ALTER TABLE "Subverse" ALTER COLUMN "ExcludeSitewideBans" SET DEFAULT false;
ALTER TABLE "Subverse" ALTER COLUMN "ID" SET DEFAULT nextval('"subverse_id_seq"');
ALTER TABLE "Subverse" ALTER COLUMN "IsAdminPrivate" SET DEFAULT false;
ALTER TABLE "Subverse" ALTER COLUMN "IsAdult" SET DEFAULT false;
ALTER TABLE "Subverse" ALTER COLUMN "IsAnonymized" SET DEFAULT false;
ALTER TABLE "Subverse" ALTER COLUMN "IsAuthorizedOnly" SET DEFAULT false;
ALTER TABLE "Subverse" ALTER COLUMN "IsPrivate" SET DEFAULT false;
ALTER TABLE "Subverse" ALTER COLUMN "IsThumbnailEnabled" SET DEFAULT true;
ALTER TABLE "Subverse" ALTER COLUMN "MinCCPForDownvote" SET DEFAULT 0;
ALTER TABLE "SubverseBan" ALTER COLUMN "ID" SET DEFAULT nextval('"subverseban_id_seq"');
ALTER TABLE "SubverseFlair" ALTER COLUMN "ID" SET DEFAULT nextval('"subverseflair_id_seq"');
ALTER TABLE "SubverseModerator" ALTER COLUMN "ID" SET DEFAULT nextval('"subversemoderator_id_seq"');
ALTER TABLE "SubverseSet" ALTER COLUMN "ID" SET DEFAULT nextval('"subverseset_id_seq"');
ALTER TABLE "SubverseSet" ALTER COLUMN "IsPublic" SET DEFAULT true;
ALTER TABLE "SubverseSet" ALTER COLUMN "SubscriberCount" SET DEFAULT 0;
ALTER TABLE "SubverseSetList" ALTER COLUMN "ID" SET DEFAULT nextval('"subversesetlist_id_seq"');
ALTER TABLE "SubverseSetSubscription" ALTER COLUMN "ID" SET DEFAULT nextval('"subversesetsubscription_id_seq"');
ALTER TABLE "UserBadge" ALTER COLUMN "ID" SET DEFAULT nextval('"userbadge_id_seq"');
ALTER TABLE "UserBlockedUser" ALTER COLUMN "ID" SET DEFAULT nextval('"userblockeduser_id_seq"');
ALTER TABLE "UserContribution" ALTER COLUMN "ID" SET DEFAULT nextval('"usercontribution_id_seq"');
ALTER TABLE "UserContribution" ALTER COLUMN "IsReceived" SET DEFAULT true;
ALTER TABLE "UserContribution" ALTER COLUMN "VoteCount" SET DEFAULT 0;
ALTER TABLE "UserPreference" ALTER COLUMN "BlockAnonymized" SET DEFAULT false;
ALTER TABLE "UserPreference" ALTER COLUMN "DisplayAds" SET DEFAULT false;
ALTER TABLE "UserPreference" ALTER COLUMN "DisplaySubscriptions" SET DEFAULT false;
ALTER TABLE "UserPreference" ALTER COLUMN "UseSubscriptionsMenu" SET DEFAULT true;
ALTER TABLE "Vote" ALTER COLUMN "ID" SET DEFAULT nextval('"vote_id_seq"');
ALTER TABLE "VoteOption" ALTER COLUMN "ID" SET DEFAULT nextval('"voteoption_id_seq"');
ALTER TABLE "VoteOutcome" ALTER COLUMN "ID" SET DEFAULT nextval('"voteoutcome_id_seq"');
ALTER TABLE "VoteRestriction" ALTER COLUMN "ID" SET DEFAULT nextval('"voterestriction_id_seq"');
ALTER TABLE "VoteTracker" ALTER COLUMN "ID" SET DEFAULT nextval('"votetracker_id_seq"');
ALTER TABLE "Ad" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "AdminLog" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "ApiClient" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "ApiCorsPolicy" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "ApiLog" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "EventLog" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "Filter" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "RuleReport" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "RuleSet" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "SessionTracker" ALTER COLUMN "CreationDate" SET DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE "SitePage" ALTER COLUMN "LastUpdateDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "SubverseSet" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "SubverseSetList" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "SubverseSetSubscription" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "UserBlockedUser" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "UserContribution" ALTER COLUMN "LastUpdateDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "Vote" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "VoteTracker" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
-- Resynchronize sequences with table contents. COALESCE + is_called=false keeps this
-- valid on empty tables (max() is NULL) and makes the next generated id max + 1.
select setval('"ad_id_seq"', COALESCE((select max("ID") from "Ad"), 0) + 1, false);
select setval('"adminlog_id_seq"', COALESCE((select max("ID") from "AdminLog"), 0) + 1, false);
select setval('"apiclient_id_seq"', COALESCE((select max("ID") from "ApiClient"), 0) + 1, false);
select setval('"apicorspolicy_id_seq"', COALESCE((select max("ID") from "ApiCorsPolicy"), 0) + 1, false);
select setval('"apilog_id_seq"', COALESCE((select max("ID") from "ApiLog"), 0) + 1, false);
select setval('"apipermissionpolicy_id_seq"', COALESCE((select max("ID") from "ApiPermissionPolicy"), 0) + 1, false);
select setval('"apithrottlepolicy_id_seq"', COALESCE((select max("ID") from "ApiThrottlePolicy"), 0) + 1, false);
select setval('"banneddomain_id_seq"', COALESCE((select max("ID") from "BannedDomain"), 0) + 1, false);
select setval('"banneduser_id_seq"', COALESCE((select max("ID") from "BannedUser"), 0) + 1, false);
select setval('"comment_id_seq"', COALESCE((select max("ID") from "Comment"), 0) + 1, false);
select setval('"commentsavetracker_id_seq"', COALESCE((select max("ID") from "CommentSaveTracker"), 0) + 1, false);
select setval('"commentvotetracker_id_seq"', COALESCE((select max("ID") from "CommentVoteTracker"), 0) + 1, false);
select setval('"eventlog_id_seq"', COALESCE((select max("ID") from "EventLog"), 0) + 1, false);
select setval('"featured_id_seq"', COALESCE((select max("ID") from "Featured"), 0) + 1, false);
select setval('"filter_id_seq"', COALESCE((select max("ID") from "Filter"), 0) + 1, false);
select setval('"message_id_seq"', COALESCE((select max("ID") from "Message"), 0) + 1, false);
select setval('"moderatorinvitation_id_seq"', COALESCE((select max("ID") from "ModeratorInvitation"), 0) + 1, false);
select setval('"rulereport_id_seq"', COALESCE((select max("ID") from "RuleReport"), 0) + 1, false);
select setval('"ruleset_id_seq"', COALESCE((select max("ID") from "RuleSet"), 0) + 1, false);
select setval('"sitepage_id_seq"', COALESCE((select max("ID") from "SitePage"), 0) + 1, false);
select setval('"submission_id_seq"', COALESCE((select max("ID") from "Submission"), 0) + 1, false);
select setval('"submissionsavetracker_id_seq"', COALESCE((select max("ID") from "SubmissionSaveTracker"), 0) + 1, false);
select setval('"submissionvotetracker_id_seq"', COALESCE((select max("ID") from "SubmissionVoteTracker"), 0) + 1, false);
select setval('"subverse_id_seq"', COALESCE((select max("ID") from "Subverse"), 0) + 1, false);
select setval('"subverseban_id_seq"', COALESCE((select max("ID") from "SubverseBan"), 0) + 1, false);
select setval('"subverseflair_id_seq"', COALESCE((select max("ID") from "SubverseFlair"), 0) + 1, false);
select setval('"subversemoderator_id_seq"', COALESCE((select max("ID") from "SubverseModerator"), 0) + 1, false);
select setval('"subverseset_id_seq"', COALESCE((select max("ID") from "SubverseSet"), 0) + 1, false);
select setval('"subversesetlist_id_seq"', COALESCE((select max("ID") from "SubverseSetList"), 0) + 1, false);
select setval('"subversesetsubscription_id_seq"', COALESCE((select max("ID") from "SubverseSetSubscription"), 0) + 1, false);
select setval('"userbadge_id_seq"', COALESCE((select max("ID") from "UserBadge"), 0) + 1, false);
select setval('"userblockeduser_id_seq"', COALESCE((select max("ID") from "UserBlockedUser"), 0) + 1, false);
select setval('"usercontribution_id_seq"', COALESCE((select max("ID") from "UserContribution"), 0) + 1, false);
select setval('"vote_id_seq"', COALESCE((select max("ID") from "Vote"), 0) + 1, false);
select setval('"voteoption_id_seq"', COALESCE((select max("ID") from "VoteOption"), 0) + 1, false);
select setval('"voteoutcome_id_seq"', COALESCE((select max("ID") from "VoteOutcome"), 0) + 1, false);
select setval('"voterestriction_id_seq"', COALESCE((select max("ID") from "VoteRestriction"), 0) + 1, false);
select setval('"votetracker_id_seq"', COALESCE((select max("ID") from "VoteTracker"), 0) + 1, false);

COMMIT;
