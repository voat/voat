BEGIN;
CREATE EXTENSION IF NOT EXISTS citext;
SET TIME ZONE 'UTC';

CREATE TABLE "Ad"( 
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

CREATE TABLE "AdminLog"( 
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

CREATE TABLE "ApiClient"( 
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

CREATE TABLE "ApiCorsPolicy"( 
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

CREATE TABLE "ApiLog"( 
	"ID" int NOT NULL,
	"ApiClientID" int NOT NULL,
	"Method" varchar(10) NOT NULL,
	"Url" varchar(500) NOT NULL,
	"Headers" varchar,
	"Body" varchar,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE "ApiPermissionPolicy"( 
	"ID" int NOT NULL,
	"Name" varchar(100) NOT NULL,
	"Policy" varchar(2000) NOT NULL);

CREATE TABLE "ApiThrottlePolicy"( 
	"ID" int NOT NULL,
	"Name" varchar(100) NOT NULL,
	"Policy" varchar(2000) NOT NULL);

CREATE TABLE "Badge"( 
	"ID" varchar(50) NOT NULL,
	"Graphic" varchar(50) NOT NULL,
	"Title" varchar(300) NOT NULL,
	"Name" varchar(50) NOT NULL);

CREATE TABLE "BannedDomain"( 
	"ID" int NOT NULL,
	"Domain" varchar(100) NOT NULL,
	"CreatedBy" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"Reason" varchar(500) NOT NULL);

CREATE TABLE "BannedUser"( 
	"ID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"Reason" varchar(500) NOT NULL,
	"CreatedBy" varchar(50) NOT NULL);

CREATE TABLE "Comment"( 
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

CREATE TABLE "CommentRemovalLog"( 
	"CommentID" int NOT NULL,
	"Moderator" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"Reason" varchar(500) NOT NULL);

CREATE TABLE "CommentSaveTracker"( 
	"ID" int NOT NULL,
	"CommentID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE "CommentVoteTracker"( 
	"ID" int NOT NULL,
	"CommentID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"VoteStatus" int NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"IPAddress" varchar(90),
	"VoteValue" double precision NOT NULL);

CREATE TABLE "DefaultSubverse"( 
	"Subverse" varchar(20) NOT NULL,
	"Order" int NOT NULL);

CREATE TABLE "EventLog"( 
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

CREATE TABLE "Featured"( 
	"ID" int NOT NULL,
	"DomainType" int NOT NULL,
	"DomainID" int NOT NULL,
	"Title" varchar(100),
	"Description" varchar(500),
	"StartDate" timestamp NOT NULL,
	"EndDate" timestamp,
	"CreatedBy" varchar(50) NOT NULL);

CREATE TABLE "Filter"( 
	"ID" int NOT NULL,
	"IsActive" boolean NOT NULL,
	"Name" varchar(100) NOT NULL,
	"Description" varchar(1000),
	"Pattern" varchar(100) NOT NULL,
	"Replacement" varchar(1000),
	"AppliesTo" int,
	"Action" int,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE "Message"( 
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

CREATE TABLE "ModeratorInvitation"( 
	"ID" int NOT NULL,
	"CreatedBy" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"Recipient" varchar(50) NOT NULL,
	"Subverse" varchar(50) NOT NULL,
	"Power" int NOT NULL);

CREATE TABLE "RuleReport"( 
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

CREATE TABLE "RuleSet"( 
	"ID" int NOT NULL,
	"IsActive" boolean NOT NULL,
	"Subverse" varchar(50),
	"ContentType" smallint,
	"SortOrder" int,
	"Name" varchar(200) NOT NULL,
	"Description" varchar(1000),
	"CreatedBy" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE "SessionTracker"( 
	"SessionID" varchar(90) NOT NULL,
	"Subverse" varchar(20) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE "StickiedSubmission"( 
	"SubmissionID" int NOT NULL,
	"Subverse" varchar(20) NOT NULL,
	"CreatedBy" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE "Submission"( 
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

CREATE TABLE "SubmissionRemovalLog"( 
	"SubmissionID" int NOT NULL,
	"Moderator" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"Reason" varchar(500) NOT NULL);

CREATE TABLE "SubmissionSaveTracker"( 
	"ID" int NOT NULL,
	"SubmissionID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE "SubmissionVoteTracker"( 
	"ID" int NOT NULL,
	"SubmissionID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"VoteStatus" int NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"IPAddress" varchar(90),
	"VoteValue" double precision NOT NULL);

CREATE TABLE "Subverse"( 
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

CREATE TABLE "SubverseBan"( 
	"ID" int NOT NULL,
	"Subverse" varchar(20) NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"CreatedBy" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL,
	"Reason" varchar(500) NOT NULL);

CREATE TABLE "SubverseFlair"( 
	"ID" int NOT NULL,
	"Subverse" varchar(20) NOT NULL,
	"Label" varchar(50),
	"CssClass" varchar(50));

CREATE TABLE "SubverseModerator"( 
	"ID" int NOT NULL,
	"Subverse" varchar(20) NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"Power" int NOT NULL,
	"CreatedBy" varchar(50),
	"CreationDate" timestamp);

CREATE TABLE "SubverseSet"( 
	"ID" int NOT NULL,
	"Name" varchar(20) NOT NULL,
	"Title" varchar(100),
	"Description" varchar(500),
	"UserName" varchar(50),
	"Type" int NOT NULL,
	"IsPublic" boolean NOT NULL,
	"SubscriberCount" int NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE "SubverseSetList"( 
	"ID" int NOT NULL,
	"SubverseSetID" int NOT NULL,
	"SubverseID" int NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE "SubverseSetSubscription"( 
	"ID" int NOT NULL,
	"SubverseSetID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE "UserBadge"( 
	"ID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"BadgeID" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE "UserBlockedUser"( 
	"ID" int NOT NULL,
	"BlockUser" varchar(50) NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE TABLE "UserContribution"( 
	"ID" int NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"IsReceived" boolean NOT NULL,
	"ContentType" int NOT NULL,
	"VoteStatus" int NOT NULL,
	"VoteCount" int NOT NULL,
	"VoteValue" double precision NOT NULL,
	"ValidThroughDate" timestamp NOT NULL,
	"LastUpdateDate" timestamp NOT NULL);

CREATE TABLE "UserPreference"( 
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

CREATE TABLE "ViewStatistic"( 
	"SubmissionID" int NOT NULL,
	"ViewerID" varchar(90) NOT NULL);

CREATE TABLE "Vote"( 
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

CREATE TABLE "VoteOption"( 
	"ID" int NOT NULL,
	"VoteID" int NOT NULL,
	"Title" varchar(200) NOT NULL,
	"Content" varchar,
	"FormattedContent" varchar,
	"SortOrder" int NOT NULL);

CREATE TABLE "VoteOutcome"( 
	"ID" int NOT NULL,
	"VoteOptionID" int NOT NULL,
	"Type" varchar(1000) NOT NULL,
	"Data" varchar NOT NULL);

CREATE TABLE "VoteRestriction"( 
	"ID" int NOT NULL,
	"VoteID" int NOT NULL,
	"Type" varchar(1000) NOT NULL,
	"Data" varchar NOT NULL);

CREATE TABLE "VoteTracker"( 
	"ID" int NOT NULL,
	"VoteID" int NOT NULL,
	"VoteOptionID" int NOT NULL,
	"RestrictionsPassed" boolean NOT NULL,
	"UserName" varchar(50) NOT NULL,
	"CreationDate" timestamp NOT NULL);

CREATE SEQUENCE "ad_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "Ad"."ID";
CREATE SEQUENCE "adminlog_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "AdminLog"."ID";
CREATE SEQUENCE "apiclient_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "ApiClient"."ID";
CREATE SEQUENCE "apicorspolicy_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "ApiCorsPolicy"."ID";
CREATE SEQUENCE "apilog_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "ApiLog"."ID";
CREATE SEQUENCE "apipermissionpolicy_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "ApiPermissionPolicy"."ID";
CREATE SEQUENCE "apithrottlepolicy_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "ApiThrottlePolicy"."ID";
CREATE SEQUENCE "banneddomain_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "BannedDomain"."ID";
CREATE SEQUENCE "banneduser_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "BannedUser"."ID";
CREATE SEQUENCE "comment_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "Comment"."ID";
CREATE SEQUENCE "commentsavetracker_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "CommentSaveTracker"."ID";
CREATE SEQUENCE "commentvotetracker_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "CommentVoteTracker"."ID";
CREATE SEQUENCE "eventlog_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "EventLog"."ID";
CREATE SEQUENCE "featured_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "Featured"."ID";
CREATE SEQUENCE "filter_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "Filter"."ID";
CREATE SEQUENCE "message_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "Message"."ID";
CREATE SEQUENCE "moderatorinvitation_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "ModeratorInvitation"."ID";
CREATE SEQUENCE "rulereport_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "RuleReport"."ID";
CREATE SEQUENCE "ruleset_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "RuleSet"."ID";
CREATE SEQUENCE "submission_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "Submission"."ID";
CREATE SEQUENCE "submissionsavetracker_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SubmissionSaveTracker"."ID";
CREATE SEQUENCE "submissionvotetracker_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SubmissionVoteTracker"."ID";
CREATE SEQUENCE "subverse_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "Subverse"."ID";
CREATE SEQUENCE "subverseban_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SubverseBan"."ID";
CREATE SEQUENCE "subverseflair_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SubverseFlair"."ID";
CREATE SEQUENCE "subversemoderator_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SubverseModerator"."ID";
CREATE SEQUENCE "subverseset_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SubverseSet"."ID";
CREATE SEQUENCE "subversesetlist_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SubverseSetList"."ID";
CREATE SEQUENCE "subversesetsubscription_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "SubverseSetSubscription"."ID";
CREATE SEQUENCE "userbadge_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "UserBadge"."ID";
CREATE SEQUENCE "userblockeduser_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "UserBlockedUser"."ID";
CREATE SEQUENCE "usercontribution_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "UserContribution"."ID";
CREATE SEQUENCE "vote_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "Vote"."ID";
CREATE SEQUENCE "voteoption_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "VoteOption"."ID";
CREATE SEQUENCE "voteoutcome_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "VoteOutcome"."ID";
CREATE SEQUENCE "voterestriction_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "VoteRestriction"."ID";
CREATE SEQUENCE "votetracker_id_seq" INCREMENT BY 1 MINVALUE 1 START WITH 1 OWNED BY "VoteTracker"."ID";
ALTER TABLE "Ad" ADD CONSTRAINT "PK_Ad" PRIMARY KEY ("ID");
ALTER TABLE "AdminLog" ADD CONSTRAINT "PK_AdminLog" PRIMARY KEY ("ID");
ALTER TABLE "ApiClient" ADD CONSTRAINT "PK_ApiClient" PRIMARY KEY ("ID");
ALTER TABLE "ApiCorsPolicy" ADD CONSTRAINT "PK_ApiCorsPolicy" PRIMARY KEY ("ID");
ALTER TABLE "ApiLog" ADD CONSTRAINT "PK_ApiLog" PRIMARY KEY ("ID");
ALTER TABLE "ApiPermissionPolicy" ADD CONSTRAINT "PK_ApiPermissionPolicy" PRIMARY KEY ("ID");
ALTER TABLE "ApiThrottlePolicy" ADD CONSTRAINT "PK_ApiThrottlePolicy" PRIMARY KEY ("ID");
ALTER TABLE "Badge" ADD CONSTRAINT "PK_Badge" PRIMARY KEY ("ID");
ALTER TABLE "BannedDomain" ADD CONSTRAINT "PK_BannedDomain" PRIMARY KEY ("ID");
ALTER TABLE "BannedUser" ADD CONSTRAINT "PK_BannedUser" PRIMARY KEY ("ID");
ALTER TABLE "Comment" ADD CONSTRAINT "PK_Comment" PRIMARY KEY ("ID");
ALTER TABLE "CommentRemovalLog" ADD CONSTRAINT "PK_CommentRemovalLog" PRIMARY KEY ("CommentID");
ALTER TABLE "CommentSaveTracker" ADD CONSTRAINT "PK_CommentSaveTracker" PRIMARY KEY ("ID");
ALTER TABLE "CommentVoteTracker" ADD CONSTRAINT "PK_CommentVoteTracker" PRIMARY KEY ("ID");
ALTER TABLE "DefaultSubverse" ADD CONSTRAINT "PK_DefaultSubverse" PRIMARY KEY ("Subverse");
ALTER TABLE "EventLog" ADD CONSTRAINT "PK_EventLog" PRIMARY KEY ("ID");
ALTER TABLE "Featured" ADD CONSTRAINT "PK_Featured" PRIMARY KEY ("ID");
ALTER TABLE "Filter" ADD CONSTRAINT "PK_Filter" PRIMARY KEY ("ID");
ALTER TABLE "Message" ADD CONSTRAINT "PK_Message" PRIMARY KEY ("ID");
ALTER TABLE "ModeratorInvitation" ADD CONSTRAINT "PK_ModeratorInvitation" PRIMARY KEY ("ID");
ALTER TABLE "RuleReport" ADD CONSTRAINT "PK_RuleReport" PRIMARY KEY ("ID");
ALTER TABLE "RuleSet" ADD CONSTRAINT "PK_RuleSet" PRIMARY KEY ("ID");
ALTER TABLE "SessionTracker" ADD CONSTRAINT "PK_SessionTracker" PRIMARY KEY ("SessionID","Subverse");
ALTER TABLE "StickiedSubmission" ADD CONSTRAINT "PK_StickiedSubmission" PRIMARY KEY ("SubmissionID","Subverse");
ALTER TABLE "Submission" ADD CONSTRAINT "PK_Submission" PRIMARY KEY ("ID");
ALTER TABLE "SubmissionRemovalLog" ADD CONSTRAINT "PK_SubmissionRemovalLog" PRIMARY KEY ("SubmissionID");
ALTER TABLE "SubmissionSaveTracker" ADD CONSTRAINT "PK_SubmissionSaveTracker" PRIMARY KEY ("ID");
ALTER TABLE "SubmissionVoteTracker" ADD CONSTRAINT "PK_SubmissionVoteTracker" PRIMARY KEY ("ID");
ALTER TABLE "Subverse" ADD CONSTRAINT "PK_Subverse" PRIMARY KEY ("Name");
ALTER TABLE "SubverseBan" ADD CONSTRAINT "PK_SubverseBan" PRIMARY KEY ("ID");
ALTER TABLE "SubverseFlair" ADD CONSTRAINT "PK_SubverseFlair" PRIMARY KEY ("ID");
ALTER TABLE "SubverseModerator" ADD CONSTRAINT "PK_SubverseModerator" PRIMARY KEY ("ID");
ALTER TABLE "SubverseSet" ADD CONSTRAINT "PK_SubverseSet" PRIMARY KEY ("ID");
ALTER TABLE "SubverseSetList" ADD CONSTRAINT "PK_SubverseSetList" PRIMARY KEY ("ID");
ALTER TABLE "SubverseSetSubscription" ADD CONSTRAINT "PK_SubverseSetSubscription" PRIMARY KEY ("ID");
ALTER TABLE "UserBadge" ADD CONSTRAINT "PK_UserBadge" PRIMARY KEY ("ID");
ALTER TABLE "UserBlockedUser" ADD CONSTRAINT "PK_UserBlockedUser" PRIMARY KEY ("ID");
ALTER TABLE "UserContribution" ADD CONSTRAINT "PK_UserContribution" PRIMARY KEY ("ID");
ALTER TABLE "UserPreference" ADD CONSTRAINT "PK_UserPreference" PRIMARY KEY ("UserName");
ALTER TABLE "ViewStatistic" ADD CONSTRAINT "PK_ViewStatistic" PRIMARY KEY ("SubmissionID","ViewerID");
ALTER TABLE "Vote" ADD CONSTRAINT "PK_Vote" PRIMARY KEY ("ID");
ALTER TABLE "VoteOption" ADD CONSTRAINT "PK_VoteOption" PRIMARY KEY ("ID");
ALTER TABLE "VoteOutcome" ADD CONSTRAINT "PK_VoteOutcome" PRIMARY KEY ("ID");
ALTER TABLE "VoteRestriction" ADD CONSTRAINT "PK_VoteRestriction" PRIMARY KEY ("ID");
ALTER TABLE "VoteTracker" ADD CONSTRAINT "PK_VoteTracker" PRIMARY KEY ("ID");
ALTER TABLE "Subverse" ADD CONSTRAINT "IX_Subverse" UNIQUE ("ID");
ALTER TABLE "ApiClient" ADD CONSTRAINT "FK_ApiClient_ApiPermissionPolicy" FOREIGN KEY ("ApiPermissionPolicyID") REFERENCES "ApiPermissionPolicy" ( "ID");
ALTER TABLE "ApiClient" ADD CONSTRAINT "FK_ApiClient_ApiThrottlePolicy" FOREIGN KEY ("ApiThrottlePolicyID") REFERENCES "ApiThrottlePolicy" ( "ID");
ALTER TABLE "Comment" ADD CONSTRAINT "FK_Comment_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "CommentRemovalLog" ADD CONSTRAINT "FK_CommentRemovalLog_Comment" FOREIGN KEY ("CommentID") REFERENCES "Comment" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "CommentSaveTracker" ADD CONSTRAINT "FK_CommentSaveTracker_Comment" FOREIGN KEY ("CommentID") REFERENCES "Comment" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "CommentVoteTracker" ADD CONSTRAINT "FK_CommentVoteTracker_Comment" FOREIGN KEY ("CommentID") REFERENCES "Comment" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "DefaultSubverse" ADD CONSTRAINT "FK_DefaultSubverse_Subverse" FOREIGN KEY ("Subverse") REFERENCES "Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "RuleReport" ADD CONSTRAINT "FK_RuleReport_RuleSet" FOREIGN KEY ("RuleSetID") REFERENCES "RuleSet" ( "ID");
ALTER TABLE "StickiedSubmission" ADD CONSTRAINT "FK_StickiedSubmission_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ( "ID");
ALTER TABLE "StickiedSubmission" ADD CONSTRAINT "FK_StickiedSubmission_Subverse" FOREIGN KEY ("Subverse") REFERENCES "Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "Submission" ADD CONSTRAINT "FK_Submission_Subverse" FOREIGN KEY ("Subverse") REFERENCES "Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "SubmissionRemovalLog" ADD CONSTRAINT "FK_SubmissionRemovalLog_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "SubmissionSaveTracker" ADD CONSTRAINT "FK_SubmissionSaveTracker_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "SubmissionVoteTracker" ADD CONSTRAINT "FK_SubmissionVoteTracker_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ( "ID");
ALTER TABLE "SubverseBan" ADD CONSTRAINT "FK_SubverseBan_Subverse" FOREIGN KEY ("Subverse") REFERENCES "Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "SubverseFlair" ADD CONSTRAINT "FK_SubverseFlair_Subverse" FOREIGN KEY ("Subverse") REFERENCES "Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "SubverseModerator" ADD CONSTRAINT "FK_SubverseModerator_Subverse" FOREIGN KEY ("Subverse") REFERENCES "Subverse" ( "Name") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "SubverseSetList" ADD CONSTRAINT "FK_SubverseSetList_Subverse" FOREIGN KEY ("SubverseID") REFERENCES "Subverse" ( "ID");
ALTER TABLE "SubverseSetList" ADD CONSTRAINT "FK_SubverseSetList_SubverseSet" FOREIGN KEY ("SubverseSetID") REFERENCES "SubverseSet" ( "ID");
ALTER TABLE "SubverseSetSubscription" ADD CONSTRAINT "FK_SubverseSetSubscription_Set" FOREIGN KEY ("SubverseSetID") REFERENCES "SubverseSet" ( "ID");
ALTER TABLE "UserBadge" ADD CONSTRAINT "FK_UserBadge_Badge" FOREIGN KEY ("BadgeID") REFERENCES "Badge" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "ViewStatistic" ADD CONSTRAINT "FK_ViewStatistic_Submission" FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ( "ID") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "VoteOption" ADD CONSTRAINT "FK_VoteOption_Vote" FOREIGN KEY ("VoteID") REFERENCES "Vote" ( "ID");
ALTER TABLE "VoteOutcome" ADD CONSTRAINT "FK_VoteOutcome_VoteOption" FOREIGN KEY ("VoteOptionID") REFERENCES "VoteOption" ( "ID");
ALTER TABLE "VoteRestriction" ADD CONSTRAINT "FK_VoteRestriction_Vote" FOREIGN KEY ("VoteID") REFERENCES "Vote" ( "ID");
ALTER TABLE "VoteTracker" ADD CONSTRAINT "FK_VoteTracker_Vote" FOREIGN KEY ("VoteID") REFERENCES "Vote" ( "ID");
ALTER TABLE "VoteTracker" ADD CONSTRAINT "FK_VoteTracker_VoteOption" FOREIGN KEY ("VoteOptionID") REFERENCES "VoteOption" ( "ID");
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
ALTER TABLE "SubverseSet" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "SubverseSetList" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "SubverseSetSubscription" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "UserBlockedUser" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "UserContribution" ALTER COLUMN "LastUpdateDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "Vote" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
ALTER TABLE "VoteTracker" ALTER COLUMN "CreationDate" SET DEFAULT (now() at time zone 'utc');
select setval('"ad_id_seq"',(select max("ID") from "Ad")::bigint);
select setval('"adminlog_id_seq"',(select max("ID") from "AdminLog")::bigint);
select setval('"apiclient_id_seq"',(select max("ID") from "ApiClient")::bigint);
select setval('"apicorspolicy_id_seq"',(select max("ID") from "ApiCorsPolicy")::bigint);
select setval('"apilog_id_seq"',(select max("ID") from "ApiLog")::bigint);
select setval('"apipermissionpolicy_id_seq"',(select max("ID") from "ApiPermissionPolicy")::bigint);
select setval('"apithrottlepolicy_id_seq"',(select max("ID") from "ApiThrottlePolicy")::bigint);
select setval('"banneddomain_id_seq"',(select max("ID") from "BannedDomain")::bigint);
select setval('"banneduser_id_seq"',(select max("ID") from "BannedUser")::bigint);
select setval('"comment_id_seq"',(select max("ID") from "Comment")::bigint);
select setval('"commentsavetracker_id_seq"',(select max("ID") from "CommentSaveTracker")::bigint);
select setval('"commentvotetracker_id_seq"',(select max("ID") from "CommentVoteTracker")::bigint);
select setval('"eventlog_id_seq"',(select max("ID") from "EventLog")::bigint);
select setval('"featured_id_seq"',(select max("ID") from "Featured")::bigint);
select setval('"filter_id_seq"',(select max("ID") from "Filter")::bigint);
select setval('"message_id_seq"',(select max("ID") from "Message")::bigint);
select setval('"moderatorinvitation_id_seq"',(select max("ID") from "ModeratorInvitation")::bigint);
select setval('"rulereport_id_seq"',(select max("ID") from "RuleReport")::bigint);
select setval('"ruleset_id_seq"',(select max("ID") from "RuleSet")::bigint);
select setval('"submission_id_seq"',(select max("ID") from "Submission")::bigint);
select setval('"submissionsavetracker_id_seq"',(select max("ID") from "SubmissionSaveTracker")::bigint);
select setval('"submissionvotetracker_id_seq"',(select max("ID") from "SubmissionVoteTracker")::bigint);
select setval('"subverse_id_seq"',(select max("ID") from "Subverse")::bigint);
select setval('"subverseban_id_seq"',(select max("ID") from "SubverseBan")::bigint);
select setval('"subverseflair_id_seq"',(select max("ID") from "SubverseFlair")::bigint);
select setval('"subversemoderator_id_seq"',(select max("ID") from "SubverseModerator")::bigint);
select setval('"subverseset_id_seq"',(select max("ID") from "SubverseSet")::bigint);
select setval('"subversesetlist_id_seq"',(select max("ID") from "SubverseSetList")::bigint);
select setval('"subversesetsubscription_id_seq"',(select max("ID") from "SubverseSetSubscription")::bigint);
select setval('"userbadge_id_seq"',(select max("ID") from "UserBadge")::bigint);
select setval('"userblockeduser_id_seq"',(select max("ID") from "UserBlockedUser")::bigint);
select setval('"usercontribution_id_seq"',(select max("ID") from "UserContribution")::bigint);
select setval('"vote_id_seq"',(select max("ID") from "Vote")::bigint);
select setval('"voteoption_id_seq"',(select max("ID") from "VoteOption")::bigint);
select setval('"voteoutcome_id_seq"',(select max("ID") from "VoteOutcome")::bigint);
select setval('"voterestriction_id_seq"',(select max("ID") from "VoteRestriction")::bigint);
select setval('"votetracker_id_seq"',(select max("ID") from "VoteTracker")::bigint);

COMMIT;
