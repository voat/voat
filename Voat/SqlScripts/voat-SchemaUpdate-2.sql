/****** Object:  Table [dbo].[AutoModComment]    Script Date: 9/8/2015 11:24:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AutoModComment](
	[ID] [INT] IDENTITY(1,1) NOT NULL,
	[CreationDate] [NCHAR](10) NULL,
	[RuleAuthor] [NVARCHAR](50) NULL,
	[ActionToTake] [NVARCHAR](50) NOT NULL,
	[CommentAuthor] [NVARCHAR](50) NULL,
	[CommentBody] [NVARCHAR](200) NULL,
	[MinBodyLength] [INT] NULL,
	[MaxBodyLength] [INT] NULL,
	[MinAuthorCCP] [INT] NULL,
	[MinAuthorSCP] [INT] NULL,
	[MinAuthorAccountAgeInDays] [INT] NULL,
	[AutoModPM] [NVARCHAR](200) NULL,
	[AutoAuthorPM] [NVARCHAR](200) NULL,
	[NotifyAuthor] [BIT] NULL,
	[NotifyMod] [BIT] NULL,
	[CheckEdited] [BIT] NULL,
 CONSTRAINT [PK_AutoModComments] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[AutoModComment] ADD  CONSTRAINT [DF_AutoModComments_notifyAuthor]  DEFAULT ((1)) FOR [NotifyAuthor]
GO

ALTER TABLE [dbo].[AutoModComment] ADD  CONSTRAINT [DF_AutoModComments_notifyMod]  DEFAULT ((1)) FOR [NotifyMod]
GO

ALTER TABLE [dbo].[AutoModComment] ADD  CONSTRAINT [DF_AutoModComments_checkEdited]  DEFAULT ((1)) FOR [CheckEdited]
GO

/****** Object:  Table [dbo].[AutoModSubmission]    Script Date: 9/8/2015 11:25:38 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AutoModSubmission](
	[ID] [INT] IDENTITY(1,1) NOT NULL,
	[CreationDate] [DATETIME] NOT NULL,
	[RuleAuthor] [NVARCHAR](50) NOT NULL,
	[ActionToTake] [NVARCHAR](50) NOT NULL,
	[SubmissionTitle] [NVARCHAR](50) NULL,
	[SubmissionDomain] [NVARCHAR](50) NULL,
	[SubmissionAuthor] [NVARCHAR](50) NULL,
	[SubmissionType] [INT] NULL,
	[SubmissionText] [NCHAR](10) NULL,
	[MinAuthorAccountAgeInDays] [INT] NULL,
	[MinAuthorCCP] [INT] NULL,
	[MinAuthorSCP] [INT] NULL,
	[MinBodyLength] [INT] NULL,
	[MaxBodyLength] [INT] NULL,
	[AutoComment] [NVARCHAR](200) NULL,
	[AutoModPM] [NVARCHAR](200) NULL,
	[AutoAuthorPM] [NVARCHAR](200) NULL,
	[NotifyAuthor] [BIT] NULL,
	[NotifyMod] [BIT] NULL,
	[CheckEdited] [BIT] NULL,
 CONSTRAINT [PK_AutoModSubmissions] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[AutoModSubmission] ADD  CONSTRAINT [DF_AutoModSubmissions_notifyAuthor]  DEFAULT ((1)) FOR [NotifyAuthor]
GO

ALTER TABLE [dbo].[AutoModSubmission] ADD  CONSTRAINT [DF_AutoModSubmissions_notifyMod]  DEFAULT ((1)) FOR [NotifyMod]
GO

ALTER TABLE [dbo].[AutoModSubmission] ADD  CONSTRAINT [DF_AutoModSubmissions_checkEdited]  DEFAULT ((1)) FOR [CheckEdited]
GO




EXEC sp_rename 'dbo.Badges', 'Badge';
EXEC sp_rename 'dbo.Badge.BadgeId', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.Badge.BadgeGraphics', 'Graphic', 'COLUMN';
EXEC sp_rename 'dbo.Badge.BadgeTitle', 'Title', 'COLUMN';
EXEC sp_rename 'dbo.Badge.BadgeName', 'Name', 'COLUMN';

EXEC sp_rename 'dbo.Banneddomains', 'BannedDomain';
EXEC sp_rename 'dbo.BannedDomain.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.BannedDomain.Hostname', 'Domain', 'COLUMN';
EXEC sp_rename 'dbo.BannedDomain.Added_on', 'CreationDate', 'COLUMN';
EXEC sp_rename 'dbo.BannedDomain.Added_by', 'CreatedBy', 'COLUMN';

EXEC sp_rename 'dbo.Bannedusers', 'BannedUser';
EXEC sp_rename 'dbo.BannedUser.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.BannedUser.Username', 'UserName', 'COLUMN';
EXEC sp_rename 'dbo.BannedUser.Banned_by', 'CreatedBy', 'COLUMN';
EXEC sp_rename 'dbo.BannedUser.Date_Banned', 'CreationDate', 'COLUMN';

--EXEC sp_rename 'dbo.CommentRemovalLog', 'CommentRemovalLog';
EXEC sp_rename 'dbo.CommentRemovalLog.CommentId', 'CommentID', 'COLUMN';
--EXEC sp_rename 'dbo.CommentRemovalLog.Moderator', 'CreatedBy', 'COLUMN';
EXEC sp_rename 'dbo.CommentRemovalLog.RemovalTimestamp', 'CreationDate', 'COLUMN';
EXEC sp_rename 'dbo.CommentRemovalLog.ReasonForRemoval', 'Reason', 'COLUMN';

EXEC sp_rename 'dbo.Commentreplynotifications', 'CommentReplyNotification';
EXEC sp_rename 'dbo.CommentReplyNotification.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.CommentReplyNotification.CommentId', 'CommentID', 'COLUMN';
EXEC sp_rename 'dbo.CommentReplyNotification.SubmissionId', 'SubmissionID', 'COLUMN';
EXEC sp_rename 'dbo.CommentReplyNotification.Timestamp', 'CreationDate', 'COLUMN';
EXEC sp_rename 'dbo.CommentReplyNotification.Markedasunread', 'MarkedAsUnread', 'COLUMN';
EXEC sp_rename 'dbo.CommentReplyNotification.Status', 'IsUnread', 'COLUMN';

EXEC sp_rename 'dbo.Comments', 'Comment';
EXEC sp_rename 'dbo.Comment.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.Comment.Date', 'CreationDate', 'COLUMN';
EXEC sp_rename 'dbo.Comment.Name', 'UserName', 'COLUMN';
EXEC sp_rename 'dbo.Comment.MessageId', 'SubmissionID', 'COLUMN';
EXEC sp_rename 'dbo.Comment.ParentId', 'ParentID', 'COLUMN';
EXEC sp_rename 'dbo.Comment.Anonymized', 'IsAnonymized', 'COLUMN';
EXEC sp_rename 'dbo.Comment.Likes', 'UpCount', 'COLUMN';
EXEC sp_rename 'dbo.Comment.Dislikes', 'DownCount', 'COLUMN';
EXEC sp_rename 'dbo.Comment.CommentContent', 'Content', 'COLUMN';

EXEC sp_rename 'dbo.Commentsavingtracker', 'CommentSaveTracker';
EXEC sp_rename 'dbo.CommentSaveTracker.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.CommentSaveTracker.Timestamp', 'CreationDate', 'COLUMN';
EXEC sp_rename 'dbo.CommentSaveTracker.CommentId', 'CommentID', 'COLUMN';

EXEC sp_rename 'dbo.Commentvotingtracker', 'CommentVoteTracker';
EXEC sp_rename 'dbo.CommentVoteTracker.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.CommentVoteTracker.Timestamp', 'CreationDate', 'COLUMN';
EXEC sp_rename 'dbo.CommentVoteTracker.CommentId', 'CommentID', 'COLUMN';
EXEC sp_rename 'dbo.CommentVoteTracker.ClientIpAddress', 'IPAddress', 'COLUMN';

EXEC sp_rename 'dbo.Defaultsubverses', 'DefaultSubverse';
EXEC sp_rename 'dbo.DefaultSubverse.name', 'Subverse', 'COLUMN';
EXEC sp_rename 'dbo.DefaultSubverse.position', 'Order', 'COLUMN';

EXEC sp_rename 'dbo.Featuredsubs', 'FeaturedSubverse';
EXEC sp_rename 'dbo.FeaturedSubverse.Feature_id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.FeaturedSubverse.Subname', 'Subverse', 'COLUMN';
EXEC sp_rename 'dbo.FeaturedSubverse.Featured_by', 'CreatedBy', 'COLUMN';
EXEC sp_rename 'dbo.FeaturedSubverse.Featured_on', 'CreationDate', 'COLUMN';

EXEC sp_rename 'dbo.Messages', 'Submission';
EXEC sp_rename 'dbo.Submission.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.Submission.MessageContent', 'Content', 'COLUMN';
EXEC sp_rename 'dbo.Submission.Name', 'UserName', 'COLUMN';
EXEC sp_rename 'dbo.Submission.Linkdescription', 'LinkDescription', 'COLUMN';
EXEC sp_rename 'dbo.Submission.Date', 'CreationDate', 'COLUMN';
EXEC sp_rename 'dbo.Submission.Likes', 'UpCount', 'COLUMN';
EXEC sp_rename 'dbo.Submission.Dislikes', 'DownCount', 'COLUMN';
EXEC sp_rename 'dbo.Submission.Anonymized', 'IsAnonymized', 'COLUMN';

EXEC sp_rename 'dbo.Moderatorinvitations', 'ModeratorInvitation';
EXEC sp_rename 'dbo.ModeratorInvitation.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.ModeratorInvitation.Sent_by', 'CreatedBy', 'COLUMN';
EXEC sp_rename 'dbo.ModeratorInvitation.Sent_on', 'CreationDate', 'COLUMN';
EXEC sp_rename 'dbo.ModeratorInvitation.Sent_to', 'Recipient', 'COLUMN';

EXEC sp_rename 'dbo.Postreplynotifications', 'SubmissionReplyNotification';
EXEC sp_rename 'dbo.SubmissionReplyNotification.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.SubmissionReplyNotification.CommentId', 'CommentID', 'COLUMN';
EXEC sp_rename 'dbo.SubmissionReplyNotification.SubmissionId', 'SubmissionID', 'COLUMN';
EXEC sp_rename 'dbo.SubmissionReplyNotification.Timestamp', 'CreationDate', 'COLUMN';
EXEC sp_rename 'dbo.SubmissionReplyNotification.Status', 'IsUnread', 'COLUMN';
EXEC sp_rename 'dbo.SubmissionReplyNotification.Markedasunread', 'MarkedAsUnread', 'COLUMN';

EXEC sp_rename 'dbo.Privatemessages', 'PrivateMessage';
EXEC sp_rename 'dbo.PrivateMessage.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.PrivateMessage.Timestamp', 'CreationDate', 'COLUMN';
EXEC sp_rename 'dbo.PrivateMessage.Markedasunread', 'MarkedAsUnread', 'COLUMN';
EXEC sp_rename 'dbo.PrivateMessage.Status', 'IsUnread', 'COLUMN';

EXEC sp_rename 'dbo.Promotedsubmissions', 'PromotedSubmission';
EXEC sp_rename 'dbo.PromotedSubmission.promoted_submission_id', 'SubmissionID', 'COLUMN';
EXEC sp_rename 'dbo.PromotedSubmission.promoted_by', 'CreatedBy', 'COLUMN';

EXEC sp_rename 'dbo.Savingtracker', 'SubmissionSaveTracker';
EXEC sp_rename 'dbo.SubmissionSaveTracker.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.SubmissionSaveTracker.MessageId', 'SubmissionID', 'COLUMN';
EXEC sp_rename 'dbo.SubmissionSaveTracker.Timestamp', 'CreationDate', 'COLUMN';

EXEC sp_rename 'dbo.Sessiontracker', 'SessionTracker';
EXEC sp_rename 'dbo.SessionTracker.SessionId', 'SessionID', 'COLUMN';
EXEC sp_rename 'dbo.SessionTracker.Timestamp', 'CreationDate', 'COLUMN';

EXEC sp_rename 'dbo.Stickiedsubmissions', 'StickiedSubmission';
EXEC sp_rename 'dbo.StickiedSubmission.Submission_id', 'SubmissionID', 'COLUMN';
EXEC sp_rename 'dbo.StickiedSubmission.Subversename', 'Subverse', 'COLUMN';
EXEC sp_rename 'dbo.StickiedSubmission.Stickied_by', 'CreatedBy', 'COLUMN';
EXEC sp_rename 'dbo.StickiedSubmission.Stickied_date', 'CreationDate', 'COLUMN';

EXEC sp_rename 'dbo.SubmissionRemovalLog.SubmissionId', 'SubmissionID', 'COLUMN';
EXEC sp_rename 'dbo.SubmissionRemovalLog.RemovalTimestamp', 'CreationDate', 'COLUMN';
EXEC sp_rename 'dbo.SubmissionRemovalLog.ReasonForRemoval', 'Reason', 'COLUMN';

EXEC sp_rename 'dbo.Subscriptions', 'SubverseSubscription';
EXEC sp_rename 'dbo.SubverseSubscription.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.SubverseSubscription.SubverseName', 'Subverse', 'COLUMN';
EXEC sp_rename 'dbo.SubverseSubscription.Username', 'UserName', 'COLUMN';


EXEC sp_rename 'dbo.SubverseAdmins', 'SubverseModerator';
EXEC sp_rename 'dbo.SubverseModerator.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.SubverseModerator.SubverseName', 'Subverse', 'COLUMN';
EXEC sp_rename 'dbo.SubverseModerator.Username', 'UserName', 'COLUMN';
EXEC sp_rename 'dbo.SubverseModerator.Added_by', 'CreatedBy', 'COLUMN';
EXEC sp_rename 'dbo.SubverseModerator.Added_on', 'CreationDate', 'COLUMN';

EXEC sp_rename 'dbo.SubverseBans', 'SubverseBan';
EXEC sp_rename 'dbo.SubverseBan.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.SubverseBan.SubverseName', 'Subverse', 'COLUMN';
EXEC sp_rename 'dbo.SubverseBan.Username', 'UserName', 'COLUMN';
EXEC sp_rename 'dbo.SubverseBan.BanAddedOn', 'CreationDate', 'COLUMN';
EXEC sp_rename 'dbo.SubverseBan.BanReason', 'Reason', 'COLUMN';
EXEC sp_rename 'dbo.SubverseBan.BannedBy', 'CreatedBy', 'COLUMN';

EXEC sp_rename 'dbo.Subverseflairsettings', 'SubverseFlair';
EXEC sp_rename 'dbo.SubverseFlair.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.SubverseFlair.Subversename', 'Subverse', 'COLUMN';

EXEC sp_rename 'dbo.Subverses', 'Subverse';
EXEC sp_rename 'dbo.Subverse.name', 'Name', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.title', 'Title', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.type', 'Type', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.description', 'Description', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.sidebar', 'SideBar', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.submission_text', 'SubmissionText', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.language', 'Language', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.label_submit_new_link', 'SubmitLinkLabel', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.label_sumit_new_selfpost', 'SubmitPostLabel', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.spam_filter_links', 'SpamFilterLink', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.spam_filter_selfpost', 'SpamFilterPost', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.spam_filter_comments', 'SpamFilterComment', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.rated_adult', 'IsAdult', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.allow_default', 'IsDefaultAllowed', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.enable_thumbnails', 'IsThumbnailEnabled', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.exclude_sitewide_bans', 'ExcludeSitewideBans', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.traffic_stats_public', 'IsTrafficStatsPublic', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.minutes_to_hide_comments', 'MinutesToHideComments', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.creation_date', 'CreationDate', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.stylesheet', 'Stylesheet', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.subscribers', 'SubscriberCount', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.private_subverse', 'IsPrivate', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.authorized_submitters_only', 'IsAuthorizedOnly', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.anonymized_mode', 'IsAnonymized', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.last_submission_received', 'LastSubmissionDate', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.minimumdownvoteccp', 'MinCCPForDownvote', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.forced_private', 'IsAdminPrivate', 'COLUMN';
EXEC sp_rename 'dbo.Subverse.admin_disabled', 'IsAdminDisabled', 'COLUMN';

EXEC sp_rename 'dbo.Userbadges', 'UserBadge';
EXEC sp_rename 'dbo.UserBadge.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.UserBadge.Username', 'UserName', 'COLUMN';
EXEC sp_rename 'dbo.UserBadge.BadgeId', 'BadgeID', 'COLUMN';
EXEC sp_rename 'dbo.UserBadge.Awarded', 'CreationDate', 'COLUMN';
--Add CreatedBy to UserBadge - Populate it with 'atko'


EXEC sp_rename 'dbo.UserBlockedSubverses', 'UserBlockedSubverse';
EXEC sp_rename 'dbo.UserBlockedSubverse.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.UserBlockedSubverse.SubverseName', 'Subverse', 'COLUMN';
EXEC sp_rename 'dbo.UserBlockedSubverse.Username', 'UserName', 'COLUMN';

EXEC sp_rename 'dbo.Userpreferences', 'UserPreference';
EXEC sp_rename 'dbo.UserPreference.Username', 'UserName', 'COLUMN';
EXEC sp_rename 'dbo.UserPreference.Disable_custom_css', 'DisableCSS', 'COLUMN';
EXEC sp_rename 'dbo.UserPreference.Night_mode', 'NightMode', 'COLUMN';
EXEC sp_rename 'dbo.UserPreference.Clicking_mode', 'OpenInNewWindow', 'COLUMN';
EXEC sp_rename 'dbo.UserPreference.Enable_adult_content', 'EnableAdultContent', 'COLUMN';
EXEC sp_rename 'dbo.UserPreference.Public_votes', 'DisplayVotes', 'COLUMN';
EXEC sp_rename 'dbo.UserPreference.Public_subscriptions', 'DisplaySubscriptions', 'COLUMN';
EXEC sp_rename 'dbo.UserPreference.Topmenu_from_subscriptions', 'UseSubscriptionsMenu', 'COLUMN';
EXEC sp_rename 'dbo.UserPreference.Shortbio', 'Bio', 'COLUMN';

EXEC sp_rename 'dbo.Userscore', 'UserScore';
EXEC sp_rename 'dbo.UserScore.Username', 'UserName', 'COLUMN';

EXEC sp_rename 'dbo.Usersetdefinitions', 'UserSetList';
EXEC sp_rename 'dbo.UserSetList.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.UserSetList.Set_id', 'UserSetID', 'COLUMN';
EXEC sp_rename 'dbo.UserSetList.Subversename', 'Subverse', 'COLUMN';


EXEC sp_rename 'dbo.Usersets', 'UserSet';
EXEC sp_rename 'dbo.UserSet.Set_id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.UserSet.Created_by', 'CreatedBy', 'COLUMN';
EXEC sp_rename 'dbo.UserSet.Created_on', 'CreationDate', 'COLUMN';
EXEC sp_rename 'dbo.UserSet.Public', 'IsPublic', 'COLUMN';
EXEC sp_rename 'dbo.UserSet.Subscribers', 'SubscriberCount', 'COLUMN';
EXEC sp_rename 'dbo.UserSet.Default', 'IsDefault', 'COLUMN';

EXEC sp_rename 'dbo.Usersetsubscriptions', 'UserSetSubscription';
EXEC sp_rename 'dbo.UserSetSubscription.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.UserSetSubscription.Set_Id', 'UserSetID', 'COLUMN';
EXEC sp_rename 'dbo.UserSetSubscription.Username', 'UserName', 'COLUMN';

EXEC sp_rename 'dbo.Viewstatistics', 'ViewStatistic';
EXEC sp_rename 'dbo.ViewStatistic.submissionId', 'SubmissionID', 'COLUMN';
EXEC sp_rename 'dbo.ViewStatistic.viewerId', 'ViewerID', 'COLUMN';

EXEC sp_rename 'dbo.Votingtracker', 'SubmissionVoteTracker';
EXEC sp_rename 'dbo.SubmissionVoteTracker.Id', 'ID', 'COLUMN';
EXEC sp_rename 'dbo.SubmissionVoteTracker.MessageId', 'SubmissionID', 'COLUMN';
EXEC sp_rename 'dbo.SubmissionVoteTracker.Timestamp', 'CreationDate', 'COLUMN';
EXEC sp_rename 'dbo.SubmissionVoteTracker.ClientIpAddress', 'IPAddress', 'COLUMN';

GO

--Add CreatedBy to SubverseTable - Populate it with mod level 1's
ALTER TABLE Subverse ADD CreatedBy NVARCHAR(50) NULL
GO
UPDATE s SET s.CreatedBy = ISNULL(m.UserName, 'system') FROM Subverse s
INNER JOIN dbo.SubverseModerator m ON (s.Name = m.Subverse AND m.[Power] = 1)

GO
IF EXISTS (SELECT * FROM sys.sysobjects WHERE name = 'Sessions' AND type = 'U')
	DROP TABLE dbo.[Sessions]
GO

ALTER PROC usp_CommentTree
	@SubmissionID INT,
	@Depth INT = NULL,
	@ParentID INT = NULL
AS

/*
I spent 14 hours working on this. Not. Even. Joking. 
Man, <**censored**> SQL. 
For at *least* two hours I was just randomly changing 
things in the hopes that I would accidently fix it.
*/

DECLARE @tree TABLE (
   SubmissionID INT,
   UserName NVARCHAR(30),
   ParentID INT,
   ChildID INT
);

IF @ParentID IS NULL 
	INSERT INTO @tree
	SELECT p.SubmissionID, p.UserName, ID, 0 FROM Comment p 
	WHERE 
		1 = 1
		AND p.ParentID IS NULL
		AND p.SubmissionID = @SubmissionID
	UNION ALL
	SELECT c.SubmissionID, c.UserName, c.ParentID, c.ID FROM Comment c
	WHERE
		1 = 1 
		AND c.ParentID IS NOT NULL
		AND c.SubmissionID = @SubmissionID
ELSE 
	INSERT INTO @tree
	SELECT p.SubmissionID, p.UserName, ID, 0 FROM Comment p 
	WHERE 
		1 = 1
		AND p.ParentID = @ParentID
		AND p.SubmissionID = @SubmissionID
	UNION ALL
	SELECT c.SubmissionID, c.UserName, c.ParentID, c.ID FROM Comment c
	WHERE
		1 = 1
		AND c.ParentID IS NOT NULL 
		AND c.ParentID > @ParentID
		AND c.SubmissionID = @SubmissionID

;WITH CommentHierarchy
     AS (
		SELECT 
			SubmissionID,
			UserName,
			RootID = ParentID, 
			[Depth] = CASE WHEN ChildID = 0 THEN 0 ELSE 1 END, 
			[Path] = CAST(ParentID AS VARCHAR(MAX)) + CASE WHEN ChildID != 0 THEN '\' + CAST(ChildID AS VARCHAR(MAX)) ELSE '' END,
			ChildID = ChildID ,
			ParentID = ParentID 
        FROM @tree
        WHERE NOT ParentID IN (SELECT ChildID FROM @tree)
        UNION ALL
			SELECT
				P.SubmissionID, 
				C.UserName,
				P.RootID,
				P.[Depth] + 1 ,
				P.[Path] + '\' + CAST(C.ChildID AS VARCHAR(MAX)) ,
				C.ChildID,
				C.ParentID
			FROM CommentHierarchy P
			INNER JOIN @tree C ON P.ChildID = C.ParentID
       )
SELECT 
	ChildCount = (
			SELECT COUNT(*)  FROM CommentHierarchy 
			WHERE
				1 = 1 
				AND c.ID = ParentID 
				AND ChildID != 0
		), 
	--h.*,
	h.Depth,
	h.Path,
	m.Subverse,
	c.*
FROM CommentHierarchy h
INNER JOIN Comment c ON (c.ID = CASE WHEN ChildID IS NULL OR ChildID = 0 THEN h.ParentID ELSE ChildID END)
INNER JOIN Submission m ON (c.SubmissionID = m.ID)
WHERE 
	([Depth] <= @Depth OR @Depth IS NULL)
--ORDER BY ID, Depth, ParentID
	--AND (h.RootID = @ParentID OR @ParentID IS NULL)

/*
	LOAD ALL COMMENTS FOR SUBMISSION
	usp_CommentTree 2441
	, NULL, 6116
	usp_CommentTree 2510, 1, 7407
	usp_CommentTree 2510, 1, 7408
	usp_CommentTree 2510, 1, 7409
		usp_CommentTree 2441, NULL, 6177
	usp_CommentTree 2441, 1, 6116
	usp_CommentTree 2441, NULL, 6113
	usp_CommentTree 2441, NULL, 6287
	SELECT p.MessageID, ID, 0 FROM Comments p 
	WHERE 
		p.ID = 6177
	UNION ALL

	SELECT c.MessageID, c.ParentID, c.ID FROM Comments c
	WHERE
		c.MessageID = 2441
		AND
		c.ParentID != 6177
*/
GO


SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO
BEGIN TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Comment]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Comment] ADD
[IsDeleted] [bit] NOT NULL CONSTRAINT [DF_Comments_IsDeleted] DEFAULT ((0))
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Submission]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Submission] ADD
[IsDeleted] [bit] NOT NULL CONSTRAINT [DF_Messages_IsDeleted] DEFAULT ((0))
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
COMMIT TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DECLARE @Success AS BIT
SET @Success = 1
SET NOEXEC OFF
IF (@Success = 1) PRINT 'The database update succeeded'
ELSE BEGIN
	IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
	PRINT 'The database update failed'
END
GO