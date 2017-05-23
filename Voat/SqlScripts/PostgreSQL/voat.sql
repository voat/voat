DROP SCHEMA IF EXISTS dbo CASCADE;
CREATE SCHEMA dbo;
SET SCHEMA 'dbo';

/* Make dbo schema the default one */
ALTER DATABASE voat SET search_path TO dbo;


CREATE SEQUENCE Submission_seq;

CREATE TABLE "Submission"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('Submission_seq') ,
	"IsArchived" Boolean  NOT NULL DEFAULT ((FALSE)),
	"Votes" int NULL,
	"UserName" Varchar (50) NOT NULL,
	"Content" Text  NULL,
	"CreationDate" Timestamp(3) NOT NULL,
	"Type" int NOT NULL,
	"Title" Varchar (200) NULL,
	"Rank" Double precision NOT NULL DEFAULT ((0)),
	"Subverse" Varchar (20) NULL,
	"UpCount" bigint NOT NULL DEFAULT ((1)),
	"DownCount" bigint NOT NULL DEFAULT ((0)),
	"Thumbnail" Char (40) NULL,
	"LastEditDate" Timestamp(3) NULL,
	"FlairLabel" Varchar (50) NULL,
	"FlairCss" Varchar (50) NULL,
	"IsAnonymized" Boolean  NOT NULL DEFAULT ((FALSE)),
	"Views" Double precision NOT NULL DEFAULT ((1)),
	"IsDeleted" Boolean  NOT NULL DEFAULT ((FALSE)),
	"RelativeRank" Double precision NOT NULL DEFAULT ((0)),
	"Url" Varchar (3000) NULL,
	"FormattedContent" Text  NULL
);

ALTER TABLE "Submission" ADD CONSTRAINT PK_Messages PRIMARY KEY ("ID");

CREATE SEQUENCE Comment_seq;

CREATE TABLE "Comment"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('Comment_seq') ,
	"Votes" int NULL,
	"UserName" Varchar (50) NOT NULL,
	"Content" Text  NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL,
	"LastEditDate" Timestamp(3) NULL,
	"SubmissionID" int NULL,
	"UpCount" bigint NOT NULL DEFAULT ((1)),
	"DownCount" bigint NOT NULL DEFAULT ((0)),
	"ParentID" int NULL,
	"IsAnonymized" Boolean  NOT NULL DEFAULT ((FALSE)),
	"IsDistinguished" Boolean  NOT NULL DEFAULT ((FALSE)),
	"FormattedContent" Text  NULL,
	"IsDeleted" Boolean  NOT NULL DEFAULT ((FALSE))
);

ALTER TABLE "Comment" ADD CONSTRAINT PK_Comments PRIMARY KEY ("ID");

CREATE SEQUENCE SubverseModerator_seq;

CREATE TABLE "SubverseModerator"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('SubverseModerator_seq') ,
	"Subverse" Varchar (20) NOT NULL,
	"UserName" Varchar (50) NOT NULL,
	"Power" int NOT NULL,
	"CreatedBy" Varchar (50) NULL,
	"CreationDate" Timestamp(3) NULL
);

ALTER TABLE "SubverseModerator" ADD CONSTRAINT PK_SubverseAdmins PRIMARY KEY ("ID");

CREATE TABLE "Subverse"
(
	"Name" Varchar (20) NOT NULL,
	"Title" Varchar (500) NOT NULL,
	"Description" Varchar (500) NULL,
	"SideBar" Varchar (4000) NULL,
	"SubmissionText" Varchar (500) NULL,
	"Language" Varchar (10) NULL,
	"Type" Varchar (10) NOT NULL,
	"SubmitLinkLabel" Varchar (50) NULL,
	"SubmitPostLabel" Varchar (50) NULL,
	"SpamFilterLink" Varchar (10) NULL,
	"SpamFilterPost" Varchar (10) NULL,
	"SpamFilterComment" Varchar (10) NULL,
	"IsAdult" Boolean  NOT NULL DEFAULT ((FALSE)),
	"IsDefaultAllowed" Boolean  NOT NULL DEFAULT ((TRUE)),
	"IsThumbnailEnabled" Boolean  NOT NULL DEFAULT ((TRUE)),
	"ExcludeSitewideBans" Boolean  NOT NULL DEFAULT ((FALSE)),
	"IsTrafficStatsPublic" Boolean  NULL DEFAULT ((FALSE)),
	"MinutesToHideComments" int NULL,
	"CreationDate" Timestamp(3) NOT NULL,
	"Stylesheet" Text  NULL,
	"SubscriberCount" int NULL,
	"IsPrivate" Boolean  NOT NULL DEFAULT ((FALSE)),
	"IsAuthorizedOnly" Boolean  NOT NULL DEFAULT ((FALSE)),
	"IsAnonymized" Boolean  NOT NULL DEFAULT ((FALSE)),
	"LastSubmissionDate" Timestamp(3) NULL,
	"MinCCPForDownvote" int NOT NULL DEFAULT ((0)),
	"IsAdminPrivate" Boolean  NOT NULL DEFAULT ((FALSE)),
	"IsAdminDisabled" Boolean NULL,
	"CreatedBy" Varchar (50) NULL
);

ALTER TABLE "Subverse" ADD CONSTRAINT PK_Subverses PRIMARY KEY ("Name");

CREATE SEQUENCE SubmissionVoteTracker_seq;

CREATE TABLE "SubmissionVoteTracker"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('SubmissionVoteTracker_seq') ,
	"SubmissionID" int NOT NULL,
	"UserName" Varchar (50) NULL,
	"VoteStatus" int NULL,
	"CreationDate" Timestamp(3) NULL,
	"IPAddress" Varchar (90) NULL
);

ALTER TABLE "SubmissionVoteTracker" ADD CONSTRAINT PK_Votingtracker PRIMARY KEY ("ID");

CREATE SEQUENCE CommentVoteTracker_seq;

CREATE TABLE "CommentVoteTracker"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('CommentVoteTracker_seq') ,
	"CommentID" int NOT NULL,
	"UserName" Varchar (50) NULL,
	"VoteStatus" int NULL,
	"CreationDate" Timestamp(3) NULL,
	"IPAddress" Varchar (90) NULL
);

ALTER TABLE "CommentVoteTracker" ADD CONSTRAINT PK_Commentvotingtracker PRIMARY KEY ("ID");

CREATE SEQUENCE UserSetList_seq;

CREATE TABLE "UserSetList"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('UserSetList_seq') ,
	"UserSetID" int NOT NULL,
	"Subverse" Varchar (20) NOT NULL
);

ALTER TABLE "UserSetList" ADD CONSTRAINT PK_Usersetdefinitions PRIMARY KEY ("ID");

CREATE SEQUENCE UserBlockedSubverse_seq;

CREATE TABLE "UserBlockedSubverse"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('UserBlockedSubverse_seq') ,
	"Subverse" Varchar (20) NOT NULL,
	"UserName" Varchar (50) NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL DEFAULT (now() at time zone 'utc')
);

ALTER TABLE "UserBlockedSubverse" ADD CONSTRAINT PK_UserBlockedSubverses PRIMARY KEY ("ID");

CREATE SEQUENCE SubverseSubscription_seq;

CREATE TABLE "SubverseSubscription"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('SubverseSubscription_seq') ,
	"Subverse" Varchar (20) NOT NULL,
	"UserName" Varchar (50) NOT NULL
);

ALTER TABLE "SubverseSubscription" ADD CONSTRAINT PK_Subscriptions PRIMARY KEY ("ID");

CREATE SEQUENCE SubverseFlair_seq;

CREATE TABLE "SubverseFlair"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('SubverseFlair_seq') ,
	"Subverse" Varchar (20) NOT NULL,
	"Label" Varchar (50) NULL,
	"CssClass" Varchar (50) NULL
);

ALTER TABLE "SubverseFlair" ADD CONSTRAINT PK_SubverseFlairSettings PRIMARY KEY ("ID");

CREATE SEQUENCE SubverseBan_seq;

CREATE TABLE "SubverseBan"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('SubverseBan_seq') ,
	"Subverse" Varchar (20) NOT NULL,
	"UserName" Varchar (50) NOT NULL,
	"CreatedBy" Varchar (50) NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL,
	"Reason" Varchar (500) NOT NULL
);

ALTER TABLE "SubverseBan" ADD CONSTRAINT PK_SubverseBans PRIMARY KEY ("ID");

CREATE TABLE "StickiedSubmission"
(
	"SubmissionID" int NOT NULL,
	"Subverse" Varchar (20) NOT NULL,
	"CreatedBy" Varchar (50) NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL
);

ALTER TABLE "StickiedSubmission" ADD CONSTRAINT PK_Stickiedsubmissions PRIMARY KEY ("SubmissionID");

CREATE TABLE "SessionTracker"
(
	"SessionID" Varchar (90) NOT NULL,
	"Subverse" Varchar (20) NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL DEFAULT (now())
);

ALTER TABLE "SessionTracker" ADD CONSTRAINT PK_Sessiontracker_1 PRIMARY KEY ("SessionID", "Subverse");

CREATE SEQUENCE ModeratorInvitation_seq;

CREATE TABLE "ModeratorInvitation"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('ModeratorInvitation_seq') ,
	"CreatedBy" Varchar (50) NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL,
	"Recipient" Varchar (50) NOT NULL,
	"Subverse" Varchar (50) NOT NULL,
	"Power" int NOT NULL
);

ALTER TABLE "ModeratorInvitation" ADD CONSTRAINT PK_Moderatorinvitations PRIMARY KEY ("ID");

CREATE SEQUENCE FeaturedSubverse_seq;

CREATE TABLE "FeaturedSubverse"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('FeaturedSubverse_seq') ,
	"Subverse" Varchar (20) NOT NULL,
	"CreatedBy" Varchar (50) NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL
);

ALTER TABLE "FeaturedSubverse" ADD CONSTRAINT PK_Featuredsubs PRIMARY KEY ("ID");

CREATE TABLE "DefaultSubverse"
(
	"Subverse" Varchar (20) NOT NULL,
	"Order" int NOT NULL
);

ALTER TABLE "DefaultSubverse" ADD CONSTRAINT PK_Defaultsubverses PRIMARY KEY ("Subverse");

CREATE SEQUENCE ApiClient_seq;

CREATE TABLE "ApiClient"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('ApiClient_seq'),
	"IsActive" Boolean  NOT NULL DEFAULT ((TRUE)),
	"UserName" Varchar (100) NULL,
	"AppName" Varchar (50) NOT NULL,
	"AppDescription" Varchar (2000) NULL,
	"AppAboutUrl" Varchar (200) NULL,
	"RedirectUrl" Varchar (200) NULL,
	"PublicKey" Varchar (100) NOT NULL,
	"PrivateKey" Varchar (100) NOT NULL,
	"LastAccessDate" Timestamp(3) NULL,
	"CreationDate" Timestamp(3) NOT NULL DEFAULT (now() at time zone 'utc'),
	"ApiThrottlePolicyID" int NULL,
	"ApiPermissionPolicyID" int NULL
);

ALTER TABLE "ApiClient" ADD CONSTRAINT PK_ApiClient PRIMARY KEY ("ID");

CREATE SEQUENCE Ad_seq;

CREATE TABLE "Ad"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('Ad_seq'),
	"IsActive" Boolean  NOT NULL DEFAULT ((TRUE)),
	"GraphicUrl" Varchar (100) NOT NULL,
	"DestinationUrl" Varchar (1000) NULL,
	"Name" Varchar (100) NOT NULL,
	"Description" Varchar (2000) NOT NULL,
	"StartDate" Timestamp(3) NULL,
	"EndDate" Timestamp(3) NULL,
	"Subverse" Varchar (50) NULL,
	"CreationDate" Timestamp(3) NOT NULL DEFAULT (now() at time zone 'utc')
);

ALTER TABLE "Ad" ADD CONSTRAINT PK_Ad PRIMARY KEY ("ID");

CREATE SEQUENCE UserBlockedUser_seq;

CREATE TABLE "UserBlockedUser"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('UserBlockedUser_seq') ,
	"BlockUser" Varchar (50) NOT NULL,
	"UserName" Varchar (50) NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL DEFAULT (now() at time zone 'utc')
);

ALTER TABLE "UserBlockedUser" ADD CONSTRAINT PK_UserBlockedUser PRIMARY KEY ("ID");

CREATE TABLE "UserPreference"
(
	"UserName" Varchar (50) NOT NULL,
	"DisableCSS" Boolean NOT NULL,
	"NightMode" Boolean NOT NULL,
	"Language" Varchar (50) NOT NULL,
	"OpenInNewWindow" Boolean NOT NULL,
	"EnableAdultContent" Boolean NOT NULL,
	"DisplayVotes" Boolean NOT NULL,
	"DisplaySubscriptions" Boolean  NOT NULL DEFAULT ((FALSE)),
	"UseSubscriptionsMenu" Boolean  NOT NULL DEFAULT ((TRUE)),
	"Bio" Varchar (100) NULL,
	"Avatar" Varchar (50) NULL,
	"DisplayAds" Boolean  NOT NULL DEFAULT ((FALSE)),
	"DisplayCommentCount" int NULL,
	"HighlightMinutes" int NULL,
	"VanityTitle" varchar (50) NULL,
	"CollapseCommentLimit" int NULL
);

ALTER TABLE "UserPreference" ADD CONSTRAINT PK_Userpreferences PRIMARY KEY ("UserName");

CREATE SEQUENCE ApiLog_seq;

CREATE TABLE "ApiLog"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('ApiLog_seq'),
	"ApiClientID" int NOT NULL,
	"Method" varchar (10) NOT NULL,
	"Url" varchar (500) NOT NULL,
	"Headers" Text  NULL,
	"Body" Text  NULL,
	"CreationDate" Timestamp(3) NOT NULL DEFAULT (now() at time zone 'utc')
);

ALTER TABLE "ApiLog" ADD CONSTRAINT PK_ApiLog PRIMARY KEY ("ID");

CREATE SEQUENCE UserSet_seq;

CREATE TABLE "UserSet"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('UserSet_seq') ,
	"Name" Varchar (20) NOT NULL,
	"Description" Varchar (200) NOT NULL,
	"CreatedBy" Varchar (20) NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL,
	"IsPublic" Boolean  NOT NULL DEFAULT ((TRUE)),
	"SubscriberCount" int NOT NULL DEFAULT ((1)),
	"IsDefault" Boolean  NOT NULL DEFAULT ((FALSE))
);

ALTER TABLE "UserSet" ADD CONSTRAINT PK_Usersets PRIMARY KEY ("ID");

CREATE SEQUENCE UserSetSubscription_seq;

CREATE TABLE "UserSetSubscription"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('UserSetSubscription_seq') ,
	"UserSetID" int NOT NULL,
	"Order" int NOT NULL DEFAULT ((0)),
	"UserName" Varchar (20) NOT NULL
);

ALTER TABLE "UserSetSubscription" ADD CONSTRAINT PK_Usersetsubscriptions PRIMARY KEY ("ID");

CREATE SEQUENCE EventLog_seq;

CREATE TABLE "EventLog"(
	"ID" int DEFAULT NEXTVAL ('EventLog_seq') NOT NULL,
	"ParentID" int NULL,
	"ActivityID" varchar(50) NULL,
	"UserName" Varchar(100)  NULL,
	"Origin" varchar(20)  NULL,
	"Type" varchar(300) NOT NULL,
	"Message" varchar(1500) NOT NULL,
	"Catery" varchar(1000) NOT NULL,
	"Exception" Text NULL,
	"Data" Text  NULL,
	"CreationDate" Timestamp(3) NOT NULL
);


ALTER TABLE "EventLog" ADD CONSTRAINT PK_EventLog PRIMARY KEY ("ID");

CREATE SEQUENCE ApiCorsPolicy_seq;

CREATE TABLE "ApiCorsPolicy"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('ApiCorsPolicy_seq'),
	"IsActive" Boolean NOT NULL,
	"AllowOrigin" Varchar (100) NOT NULL,
	"AllowMethods" Varchar (100) NOT NULL,
	"AllowHeaders" Varchar (100) NOT NULL,
	"AllowCredentials" Boolean NULL,
	"MaxAge" int NULL,
	"UserName" Varchar (100) NULL,
	"Description" Varchar (500) NULL,
	"CreatedBy" Varchar (100) NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL DEFAULT (now() at time zone 'utc')
);

ALTER TABLE "ApiCorsPolicy" ADD CONSTRAINT PK_ApiCorsPolicy PRIMARY KEY ("ID");

CREATE SEQUENCE ApiPermissionPolicy_seq;

CREATE TABLE "ApiPermissionPolicy"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('ApiPermissionPolicy_seq'),
	"Name" Varchar (100) NOT NULL,
	"Policy" Varchar (2000) NOT NULL
);

ALTER TABLE "ApiPermissionPolicy" ADD CONSTRAINT PK_ApiPermissionPolicy PRIMARY KEY ("ID");

CREATE SEQUENCE ApiThrottlePolicy_seq;

CREATE TABLE "ApiThrottlePolicy"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('ApiThrottlePolicy_seq'),
	"Name" Varchar (100) NOT NULL,
	"Policy" Varchar (2000) NOT NULL
);

ALTER TABLE "ApiThrottlePolicy" ADD CONSTRAINT PK_ApiThrottlePolicy PRIMARY KEY ("ID");

CREATE TABLE "CommentRemovalLog"
(
	"CommentID" int NOT NULL,
	"Moderator" Varchar (50) NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL,
	"Reason" Varchar (500) NOT NULL
);

ALTER TABLE "CommentRemovalLog" ADD CONSTRAINT PK_CommentRemovalLog PRIMARY KEY ("CommentID");

CREATE SEQUENCE CommentSaveTracker_seq;

CREATE TABLE "CommentSaveTracker"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('CommentSaveTracker_seq') ,
	"CommentID" int NOT NULL,
	"UserName" Varchar (50) NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL
);

ALTER TABLE "CommentSaveTracker" ADD CONSTRAINT PK_Commentsavingtracker PRIMARY KEY ("ID");

CREATE SEQUENCE SubmissionSaveTracker_seq;

CREATE TABLE "SubmissionSaveTracker"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('SubmissionSaveTracker_seq') ,
	"SubmissionID" int NOT NULL,
	"UserName" Varchar (50) NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL
);

ALTER TABLE "SubmissionSaveTracker" ADD CONSTRAINT PK_Savingtracker PRIMARY KEY ("ID");

CREATE TABLE "SubmissionRemovalLog"
(
	"SubmissionID" int NOT NULL,
	"Moderator" Varchar (50) NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL,
	"Reason" Varchar (500) NOT NULL
);

ALTER TABLE "SubmissionRemovalLog" ADD CONSTRAINT PK_SubmissionRemovalLog PRIMARY KEY ("SubmissionID");

CREATE TABLE "Badge"
(
	"ID" Varchar (50) NOT NULL,
	"Graphic" Varchar (50) NOT NULL,
	"Title" Varchar (300) NOT NULL,
	"Name" Varchar (50) NOT NULL
);

ALTER TABLE "Badge" ADD CONSTRAINT PK_Badges PRIMARY KEY ("ID");

CREATE SEQUENCE UserBadge_seq;

CREATE TABLE "UserBadge"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('UserBadge_seq') ,
	"UserName" Varchar (50) NOT NULL,
	"BadgeID" Varchar (50) NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL
);

ALTER TABLE "UserBadge" ADD CONSTRAINT PK_Userbadges PRIMARY KEY ("ID");

CREATE TABLE "ViewStatistic"
(
	"SubmissionID" int NOT NULL,
	"ViewerID" Varchar (90) NOT NULL
);

ALTER TABLE "ViewStatistic" ADD CONSTRAINT PK_Viewstatistics PRIMARY KEY ("SubmissionID", "ViewerID");

CREATE SEQUENCE BannedDomain_seq;

CREATE TABLE "BannedDomain"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('BannedDomain_seq') ,
	"Domain" Varchar (50) NOT NULL,
	"CreatedBy" Varchar (50) NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL,
	"Reason" Varchar (500) NOT NULL
);

ALTER TABLE "BannedDomain" ADD CONSTRAINT PK_Banneddomains PRIMARY KEY ("ID");

CREATE SEQUENCE BannedUser_seq;

CREATE TABLE "BannedUser"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('BannedUser_seq') ,
	"UserName" Varchar (50) NOT NULL,
	"CreationDate" Timestamp(3) NOT NULL,
	"Reason" Varchar (500) NOT NULL,
	"CreatedBy" Varchar (50) NOT NULL
);

ALTER TABLE "BannedUser" ADD CONSTRAINT PK_Bannedusers PRIMARY KEY ("ID");

CREATE SEQUENCE Message_seq;

CREATE TABLE "Message"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('Message_seq'),
	"CorrelationID" Varchar (36) NOT NULL,
	"ParentID" int NULL,
	"Type" int NOT NULL,
	"Sender" Varchar (50) NOT NULL,
	"SenderType" int NOT NULL,
	"Recipient" Varchar (50) NOT NULL,
	"RecipientType" int NOT NULL,
	"Title" Varchar (500) NULL,
	"Content" Text  NULL,
	"FormattedContent" Text  NULL,
	"Subverse" Varchar (20) NULL,
	"SubmissionID" int NULL,
	"CommentID" int NULL,
	"IsAnonymized" Boolean NOT NULL,
	"ReadDate" Timestamp(3) NULL,
	"CreatedBy" Varchar (50) NULL,
	"CreationDate" Timestamp(3) NOT NULL
);

ALTER TABLE "Message" ADD CONSTRAINT PK_Message PRIMARY KEY ("ID");

CREATE SEQUENCE UserVisit_seq;

CREATE TABLE "UserVisit"
(
	"ID" int NOT NULL DEFAULT NEXTVAL ('UserVisit_seq'),
	"SubmissionID" int NOT NULL,
	"UserName" varchar (50) NOT NULL,
	"LastVisitDate" Timestamp(0) NOT NULL
);

ALTER TABLE "UserVisit" ADD CONSTRAINT PK_UserVisit PRIMARY KEY ("ID");

CREATE TABLE "__MigrationHistory"
(
	"MigrationId" Varchar (150) NOT NULL,
	"ContextKey" Varchar (300) NOT NULL,
	"Model" Bytea  NOT NULL,
	"ProductVersion" Varchar (32) NOT NULL
);

ALTER TABLE "__MigrationHistory" ADD CONSTRAINT PK___MigrationHistory PRIMARY KEY ("MigrationId", "ContextKey");

ALTER TABLE "UserBadge" ADD CONSTRAINT FK_Userbadges_Badges FOREIGN KEY ("BadgeID") REFERENCES "Badge" ("ID") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "CommentRemovalLog" ADD CONSTRAINT FK_CommentRemovalLog_Comments FOREIGN KEY ("CommentID") REFERENCES "Comment" ("ID") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "CommentSaveTracker" ADD CONSTRAINT FK_Commentsavingtracker_Comments FOREIGN KEY ("CommentID") REFERENCES "Comment" ("ID") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "CommentVoteTracker" ADD CONSTRAINT FK_Commentvotingtracker_Comments FOREIGN KEY ("CommentID") REFERENCES "Comment" ("ID") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "Comment" ADD CONSTRAINT FK_Comments_Messages FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ("ID") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "DefaultSubverse" ADD CONSTRAINT FK_Defaultsubverses_Subverses FOREIGN KEY ("Subverse") REFERENCES "Subverse" ("Name") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "FeaturedSubverse" ADD CONSTRAINT FK_Featuredsubs_Subverses FOREIGN KEY ("Subverse") REFERENCES "Subverse" ("Name");

ALTER TABLE "StickiedSubmission" ADD CONSTRAINT FK_Stickiedsubmissions_Messages FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ("ID");

ALTER TABLE "StickiedSubmission" ADD CONSTRAINT FK_Stickiedsubmissions_Subverses FOREIGN KEY ("Subverse") REFERENCES "Subverse" ("Name") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "SubmissionRemovalLog" ADD CONSTRAINT FK_SubmissionRemovalLog_Messages FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ("ID") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "SubmissionSaveTracker" ADD CONSTRAINT FK_Savingtracker_Messages FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ("ID") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "SubmissionVoteTracker" ADD CONSTRAINT FK_Votingtracker_Messages FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ("ID");

ALTER TABLE "ViewStatistic" ADD CONSTRAINT FK_Viewstatistics_Messages FOREIGN KEY ("SubmissionID") REFERENCES "Submission" ("ID") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "SubverseBan" ADD CONSTRAINT FK_SubverseBans_Subverses FOREIGN KEY ("Subverse") REFERENCES "Subverse" ("Name") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "SubverseFlair" ADD CONSTRAINT FK_Subverseflairsettings_Subverses1 FOREIGN KEY ("Subverse") REFERENCES "Subverse" ("Name") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "SubverseModerator" ADD CONSTRAINT FK_SubverseAdmins_Subverses FOREIGN KEY ("Subverse") REFERENCES "Subverse" ("Name") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "SubverseSubscription" ADD CONSTRAINT FK_Subscriptions_Subverses FOREIGN KEY ("Subverse") REFERENCES "Subverse" ("Name") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "UserBlockedSubverse" ADD CONSTRAINT FK_UserBlockedSubverses_Subverses FOREIGN KEY ("Subverse") REFERENCES "Subverse" ("Name") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "UserSetList" ADD CONSTRAINT FK_Usersetdefinitions_Subverses FOREIGN KEY ("Subverse") REFERENCES "Subverse" ("Name") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "UserSetList" ADD CONSTRAINT FK_Usersetdefinitions_Usersets FOREIGN KEY ("UserSetID") REFERENCES "UserSet" ("ID") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "UserSetSubscription" ADD CONSTRAINT FK_Usersetsubscriptions_Usersets FOREIGN KEY ("UserSetID") REFERENCES "UserSet" ("ID") ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "ApiClient" ADD CONSTRAINT FK_ApiClient_ApiThrottlePolicy FOREIGN KEY ("ApiThrottlePolicyID") REFERENCES "ApiThrottlePolicy" ("ID");

ALTER TABLE "ApiClient" ADD CONSTRAINT FK_ApiClient_ApiPermissionPolicy FOREIGN KEY ("ApiPermissionPolicyID") REFERENCES "ApiPermissionPolicy" ("ID");
