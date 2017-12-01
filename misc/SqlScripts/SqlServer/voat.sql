
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ad](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[GraphicUrl] [nvarchar](100) NOT NULL,
	[DestinationUrl] [nvarchar](1000) NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](2000) NOT NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[Subverse] [nvarchar](50) NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Ad] PRIMARY KEY CLUSTERED 
(
	[ID] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AdminLog]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdminLog](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](20) NOT NULL,
	[RefUserName] [nvarchar](20) NULL,
	[RefSubverse] [nvarchar](50) NULL,
	[RefUrl] [nvarchar](200) NULL,
	[RefCommentID] [int] NULL,
	[RefSubmissionID] [int] NULL,
	[Type] [nvarchar](100) NULL,
	[Action] [nvarchar](100) NOT NULL,
	[Details] [nvarchar](1000) NOT NULL,
	[InternalDetails] [nvarchar](max) NULL,
	[CreationDate] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_AdminLog] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ApiClient]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ApiClient](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[UserName] [nvarchar](100) NULL,
	[AppName] [nvarchar](50) NOT NULL,
	[AppDescription] [nvarchar](2000) NULL,
	[AppAboutUrl] [nvarchar](200) NULL,
	[RedirectUrl] [nvarchar](200) NULL,
	[PublicKey] [nvarchar](100) NOT NULL,
	[PrivateKey] [nvarchar](100) NOT NULL,
	[LastAccessDate] [datetime] NULL,
	[CreationDate] [datetime] NOT NULL,
	[ApiThrottlePolicyID] [int] NULL,
	[ApiPermissionPolicyID] [int] NULL,
 CONSTRAINT [PK_ApiClient] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ApiCorsPolicy]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ApiCorsPolicy](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[AllowOrigin] [nvarchar](100) NOT NULL,
	[AllowMethods] [nvarchar](100) NOT NULL,
	[AllowHeaders] [nvarchar](100) NOT NULL,
	[AllowCredentials] [bit] NULL,
	[MaxAge] [int] NULL,
	[UserName] [nvarchar](100) NULL,
	[Description] [nvarchar](500) NULL,
	[CreatedBy] [nvarchar](100) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ApiCorsPolicy] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ApiLog]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ApiLog](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ApiClientID] [int] NOT NULL,
	[Method] [varchar](10) NOT NULL,
	[Url] [varchar](500) NOT NULL,
	[Headers] [varchar](max) NULL,
	[Body] [varchar](max) NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ApiLog] PRIMARY KEY CLUSTERED 
(
	[ID] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ApiPermissionPolicy]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ApiPermissionPolicy](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Policy] [nvarchar](2000) NOT NULL,
 CONSTRAINT [PK_ApiPermissionPolicy] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ApiThrottlePolicy]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ApiThrottlePolicy](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Policy] [nvarchar](2000) NOT NULL,
 CONSTRAINT [PK_ApiThrottlePolicy] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Badge]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Badge](
	[ID] [nvarchar](50) NOT NULL,
	[Graphic] [nvarchar](50) NOT NULL,
	[Title] [nvarchar](300) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Badge] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BannedDomain]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BannedDomain](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[Domain] [nvarchar](100) NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[Reason] [nvarchar](500) NOT NULL,
 CONSTRAINT [PK_BannedDomain] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BannedUser]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BannedUser](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[Reason] [nvarchar](500) NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_BannedUser] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Comment]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Comment](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[LastEditDate] [datetime] NULL,
	[SubmissionID] [int] NULL,
	[UpCount] [int] NOT NULL,
	[DownCount] [int] NOT NULL,
	[ParentID] [int] NULL,
	[IsAnonymized] [bit] NOT NULL,
	[IsDistinguished] [bit] NOT NULL,
	[FormattedContent] [nvarchar](max) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Comment] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 95) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CommentRemovalLog]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommentRemovalLog](
	[CommentID] [int] NOT NULL,
	[Moderator] [nvarchar](50) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[Reason] [nvarchar](500) NOT NULL,
 CONSTRAINT [PK_CommentRemovalLog] PRIMARY KEY CLUSTERED 
(
	[CommentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CommentSaveTracker]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommentSaveTracker](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[CommentID] [int] NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_CommentSaveTracker] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CommentVoteTracker]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommentVoteTracker](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[CommentID] [int] NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[VoteStatus] [int] NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[IPAddress] [nvarchar](90) NULL,
	[VoteValue] [float] NOT NULL,
 CONSTRAINT [PK_CommentVoteTracker] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DefaultSubverse]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DefaultSubverse](
	[Subverse] [nvarchar](20) NOT NULL,
	[Order] [int] NOT NULL,
 CONSTRAINT [PK_DefaultSubverse] PRIMARY KEY CLUSTERED 
(
	[Subverse] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[EventLog]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventLog](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ParentID] [int] NULL,
	[ActivityID] [varchar](50) NULL,
	[UserName] [nvarchar](100) NULL,
	[Origin] [varchar](100) NULL,
	[Type] [varchar](300) NOT NULL,
	[Message] [varchar](1500) NOT NULL,
	[Category] [varchar](1000) NOT NULL,
	[Exception] [varchar](max) NULL,
	[Data] [varchar](max) NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_EventLog] PRIMARY KEY CLUSTERED 
(
	[ID] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Featured]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Featured](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DomainType] [int] NOT NULL,
	[DomainID] [int] NOT NULL,
	[Title] [nvarchar](100) NULL,
	[Description] [nvarchar](500) NULL,
	[StartDate] [smalldatetime] NOT NULL,
	[EndDate] [smalldatetime] NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Featured] PRIMARY KEY CLUSTERED 
(
	[ID] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Filter]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Filter](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Description] [varchar](1000) NULL,
	[Pattern] [varchar](100) NOT NULL,
	[Replacement] [varchar](1000) NULL,
	[AppliesTo] [int] NULL,
	[Action] [int] NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Filter] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Message]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Message](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CorrelationID] [nvarchar](36) NOT NULL,
	[ParentID] [int] NULL,
	[Type] [int] NOT NULL,
	[Sender] [nvarchar](50) NOT NULL,
	[SenderType] [int] NOT NULL,
	[Recipient] [nvarchar](50) NOT NULL,
	[RecipientType] [int] NOT NULL,
	[Title] [nvarchar](500) NULL,
	[Content] [nvarchar](max) NULL,
	[FormattedContent] [nvarchar](max) NULL,
	[Subverse] [nvarchar](20) NULL,
	[SubmissionID] [int] NULL,
	[CommentID] [int] NULL,
	[IsAnonymized] [bit] NOT NULL,
	[ReadDate] [datetime] NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Message] PRIMARY KEY CLUSTERED 
(
	[ID] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ModeratorInvitation]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ModeratorInvitation](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[Recipient] [nvarchar](50) NOT NULL,
	[Subverse] [nvarchar](50) NOT NULL,
	[Power] [int] NOT NULL,
 CONSTRAINT [PK_ModeratorInvitation] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[RuleReport]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RuleReport](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Subverse] [nvarchar](50) NULL,
	[UserName] [nvarchar](100) NULL,
	[SubmissionID] [int] NULL,
	[CommentID] [int] NULL,
	[RuleSetID] [int] NOT NULL,
	[ReviewedBy] [nvarchar](100) NULL,
	[ReviewedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_RuleReport] PRIMARY KEY CLUSTERED 
(
	[ID] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[RuleSet]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RuleSet](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[Subverse] [nvarchar](50) NULL,
	[ContentType] [tinyint] NULL,
	[SortOrder] [int] NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](1000) NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_RuleSet] PRIMARY KEY CLUSTERED 
(
	[ID] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SessionTracker]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SessionTracker](
	[SessionID] [nvarchar](90) NOT NULL,
	[Subverse] [nvarchar](20) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SessionTracker] PRIMARY KEY CLUSTERED 
(
	[SessionID] ASC,
	[Subverse] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[StickiedSubmission]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StickiedSubmission](
	[SubmissionID] [int] NOT NULL,
	[Subverse] [nvarchar](20) NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_StickiedSubmission] PRIMARY KEY CLUSTERED 
(
	[SubmissionID] ASC,
	[Subverse] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Submission]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Submission](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[Content] [nvarchar](max) NULL,
	[CreationDate] [datetime] NOT NULL,
	[Type] [int] NOT NULL,
	[Title] [nvarchar](200) NULL,
	[Rank] [float] NOT NULL,
	[Subverse] [nvarchar](20) NULL,
	[UpCount] [int] NOT NULL,
	[DownCount] [int] NOT NULL,
	[Thumbnail] [nvarchar](150) NULL,
	[LastEditDate] [datetime] NULL,
	[FlairLabel] [nvarchar](50) NULL,
	[FlairCss] [nvarchar](50) NULL,
	[IsAnonymized] [bit] NOT NULL,
	[Views] [float] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[RelativeRank] [float] NOT NULL,
	[Url] [nvarchar](3000) NULL,
	[DomainReversed] [nvarchar](3000) NULL,
	[FormattedContent] [nvarchar](max) NULL,
	[IsAdult] [bit] NOT NULL,
	[ArchiveDate] [smalldatetime] NULL,
 CONSTRAINT [PK_Submission] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 95) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SubmissionRemovalLog]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubmissionRemovalLog](
	[SubmissionID] [int] NOT NULL,
	[Moderator] [nvarchar](50) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[Reason] [nvarchar](500) NOT NULL,
 CONSTRAINT [PK_SubmissionRemovalLog] PRIMARY KEY CLUSTERED 
(
	[SubmissionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SubmissionSaveTracker]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubmissionSaveTracker](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[SubmissionID] [int] NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_SubmissionSaveTracker] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SubmissionVoteTracker]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubmissionVoteTracker](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[SubmissionID] [int] NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[VoteStatus] [int] NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[IPAddress] [nvarchar](90) NULL,
	[VoteValue] [float] NOT NULL,
 CONSTRAINT [PK_SubmissionVoteTracker] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Subverse]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Subverse](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](20) NOT NULL,
	[Title] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[SideBar] [nvarchar](4000) NULL,
	[IsAdult] [bit] NOT NULL,
	[IsThumbnailEnabled] [bit] NOT NULL,
	[ExcludeSitewideBans] [bit] NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[Stylesheet] [nvarchar](max) NULL,
	[SubscriberCount] [int] NULL,
	[IsPrivate] [bit] NOT NULL,
	[IsAuthorizedOnly] [bit] NOT NULL,
	[IsAnonymized] [bit] NULL,
	[LastSubmissionDate] [datetime] NULL,
	[MinCCPForDownvote] [int] NOT NULL,
	[IsAdminPrivate] [bit] NOT NULL,
	[IsAdminDisabled] [bit] NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[LastUpdateDate] [smalldatetime] NULL,
 CONSTRAINT [PK_Subverse] PRIMARY KEY CLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY],
 CONSTRAINT [IX_Subverse] UNIQUE NONCLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SubverseBan]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubverseBan](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[Subverse] [nvarchar](20) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[Reason] [nvarchar](500) NOT NULL,
 CONSTRAINT [PK_SubverseBan] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SubverseFlair]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubverseFlair](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[Subverse] [nvarchar](20) NOT NULL,
	[Label] [nvarchar](50) NULL,
	[CssClass] [nvarchar](50) NULL,
 CONSTRAINT [PK_SubverseFlair] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SubverseModerator]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubverseModerator](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[Subverse] [nvarchar](20) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[Power] [int] NOT NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[CreationDate] [datetime] NULL,
 CONSTRAINT [PK_SubverseModerator] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SubverseSet]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubverseSet](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[Name] [nvarchar](20) NOT NULL,
	[Title] [nvarchar](100) NULL,
	[Description] [nvarchar](500) NULL,
	[UserName] [nvarchar](50) NULL,
	[Type] [int] NOT NULL,
	[IsPublic] [bit] NOT NULL,
	[SubscriberCount] [int] NOT NULL,
	[CreationDate] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_SubverseSet] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 75) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SubverseSetList]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubverseSetList](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[SubverseSetID] [int] NOT NULL,
	[SubverseID] [int] NOT NULL,
	[CreationDate] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_SubverseSetList] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 75) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SubverseSetSubscription]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubverseSetSubscription](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[SubverseSetID] [int] NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[CreationDate] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_SubverseSetSubscription] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 75) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UserBadge]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserBadge](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[BadgeID] [nvarchar](50) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_UserBadge] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 85) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UserBlockedUser]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserBlockedUser](
	[ID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[BlockUser] [nvarchar](50) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_UserBlockedUser] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UserContribution]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserContribution](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[IsReceived] [bit] NOT NULL,
	[ContentType] [int] NOT NULL,
	[VoteStatus] [int] NOT NULL,
	[VoteCount] [int] NOT NULL,
	[VoteValue] [float] NOT NULL,
	[ValidThroughDate] [datetime] NOT NULL,
	[LastUpdateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_UserContribution] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UserPreference]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserPreference](
	[UserName] [nvarchar](50) NOT NULL,
	[DisableCSS] [bit] NOT NULL,
	[NightMode] [bit] NOT NULL,
	[Language] [nvarchar](50) NOT NULL,
	[OpenInNewWindow] [bit] NOT NULL,
	[EnableAdultContent] [bit] NOT NULL,
	[DisplayVotes] [bit] NOT NULL,
	[DisplaySubscriptions] [bit] NOT NULL,
	[UseSubscriptionsMenu] [bit] NOT NULL,
	[Bio] [nvarchar](100) NULL,
	[Avatar] [nvarchar](50) NULL,
	[DisplayAds] [bit] NOT NULL,
	[DisplayCommentCount] [int] NULL,
	[HighlightMinutes] [int] NULL,
	[VanityTitle] [varchar](50) NULL,
	[CollapseCommentLimit] [int] NULL,
	[BlockAnonymized] [bit] NOT NULL,
	[CommentSort] [int] NULL,
	[DisplayThumbnails] bit NOT NULL
 CONSTRAINT [PK_UserPreference] PRIMARY KEY CLUSTERED 
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ViewStatistic]    Script Date: 5/18/2017 8:40:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ViewStatistic](
	[SubmissionID] [int] NOT NULL,
	[ViewerID] [nvarchar](90) NOT NULL,
 CONSTRAINT [PK_ViewStatistic] PRIMARY KEY CLUSTERED 
(
	[SubmissionID] ASC,
	[ViewerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[Ad] ADD  CONSTRAINT [DF_Ad_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Ad] ADD  CONSTRAINT [DF_Ad_CreationDate]  DEFAULT (getutcdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[AdminLog] ADD  CONSTRAINT [DF_AdminLog_CreationDate]  DEFAULT (getutcdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[ApiClient] ADD  CONSTRAINT [DF_ApiClient_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ApiClient] ADD  CONSTRAINT [DF_ApiClient_CreationDate]  DEFAULT (getutcdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[ApiCorsPolicy] ADD  CONSTRAINT [DF_ApiCorsPolicy_CreationDate]  DEFAULT (getutcdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[ApiLog] ADD  CONSTRAINT [DF_ApiLog_CreationDate]  DEFAULT (getutcdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Comment] ADD  CONSTRAINT [DF_Comment_UpCount]  DEFAULT ((0)) FOR [UpCount]
GO
ALTER TABLE [dbo].[Comment] ADD  CONSTRAINT [DF_Comment_DownCount]  DEFAULT ((0)) FOR [DownCount]
GO
ALTER TABLE [dbo].[Comment] ADD  CONSTRAINT [DF_Comment_IsAnonymized]  DEFAULT ((0)) FOR [IsAnonymized]
GO
ALTER TABLE [dbo].[Comment] ADD  CONSTRAINT [DF_Comment_IsDistinguished]  DEFAULT ((0)) FOR [IsDistinguished]
GO
ALTER TABLE [dbo].[Comment] ADD  CONSTRAINT [DF_Comment_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[CommentVoteTracker] ADD  CONSTRAINT [DF_CommentVoteTracker_VoteValue]  DEFAULT ((0)) FOR [VoteValue]
GO
ALTER TABLE [dbo].[EventLog] ADD  CONSTRAINT [DF_EventLog_CreationDate]  DEFAULT (getutcdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Filter] ADD  CONSTRAINT [DF_Filter_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Filter] ADD  CONSTRAINT [DF_Filter_CreationDate]  DEFAULT (getutcdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[RuleReport] ADD  CONSTRAINT [DF_RuleReport_CreationDate]  DEFAULT (getutcdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[RuleSet] ADD  CONSTRAINT [DF_RuleSet_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[RuleSet] ADD  CONSTRAINT [DF_RuleSet_CreationDate]  DEFAULT (getutcdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[SessionTracker] ADD  CONSTRAINT [DF_SessionTracker_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Submission] ADD  CONSTRAINT [DF_Submission_Rank]  DEFAULT ((0)) FOR [Rank]
GO
ALTER TABLE [dbo].[Submission] ADD  CONSTRAINT [DF_Submission_UpCount]  DEFAULT ((1)) FOR [UpCount]
GO
ALTER TABLE [dbo].[Submission] ADD  CONSTRAINT [DF_Submission_DownCount]  DEFAULT ((0)) FOR [DownCount]
GO
ALTER TABLE [dbo].[Submission] ADD  CONSTRAINT [DF_Submission_IsAnonymized]  DEFAULT ((0)) FOR [IsAnonymized]
GO
ALTER TABLE [dbo].[Submission] ADD  CONSTRAINT [DF_Submission_Views]  DEFAULT ((1)) FOR [Views]
GO
ALTER TABLE [dbo].[Submission] ADD  CONSTRAINT [DF_Submission_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Submission] ADD  CONSTRAINT [DF_Submission_RelativeRank]  DEFAULT ((0)) FOR [RelativeRank]
GO
ALTER TABLE [dbo].[Submission] ADD  CONSTRAINT [DF_Submission_IsAdult]  DEFAULT ((0)) FOR [IsAdult]
GO
ALTER TABLE [dbo].[SubmissionVoteTracker] ADD  CONSTRAINT [DF_SubmissionVoteTracker_VoteValue]  DEFAULT ((0)) FOR [VoteValue]
GO
ALTER TABLE [dbo].[Subverse] ADD  CONSTRAINT [DF_Subverse_IsAdult]  DEFAULT ((0)) FOR [IsAdult]
GO
ALTER TABLE [dbo].[Subverse] ADD  CONSTRAINT [DF_Subverse_IsThumbnailEnabled]  DEFAULT ((1)) FOR [IsThumbnailEnabled]
GO
ALTER TABLE [dbo].[Subverse] ADD  CONSTRAINT [DF_Subverse_ExcludeSitewideBans]  DEFAULT ((0)) FOR [ExcludeSitewideBans]
GO
ALTER TABLE [dbo].[Subverse] ADD  CONSTRAINT [DF_Subverse_IsPrivate]  DEFAULT ((0)) FOR [IsPrivate]
GO
ALTER TABLE [dbo].[Subverse] ADD  CONSTRAINT [DF_Subverse_IsAuthorizedOnly]  DEFAULT ((0)) FOR [IsAuthorizedOnly]
GO
ALTER TABLE [dbo].[Subverse] ADD  CONSTRAINT [DF_Subverse_IsAnonymized]  DEFAULT ((0)) FOR [IsAnonymized]
GO
ALTER TABLE [dbo].[Subverse] ADD  CONSTRAINT [DF_Subverse_MinCCPForDownvote]  DEFAULT ((0)) FOR [MinCCPForDownvote]
GO
ALTER TABLE [dbo].[Subverse] ADD  CONSTRAINT [DF_Subverse_IsAdminPrivate]  DEFAULT ((0)) FOR [IsAdminPrivate]
GO
ALTER TABLE [dbo].[SubverseSet] ADD  CONSTRAINT [DF_SubverseSet_IsPublic]  DEFAULT ((1)) FOR [IsPublic]
GO
ALTER TABLE [dbo].[SubverseSet] ADD  CONSTRAINT [DF_SubverseSet_SubscriberCount]  DEFAULT ((0)) FOR [SubscriberCount]
GO
ALTER TABLE [dbo].[SubverseSet] ADD  CONSTRAINT [DF_SubverseSet_CreationDate]  DEFAULT (getutcdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[SubverseSetList] ADD  CONSTRAINT [DF_SubverseSetList_CreationDate]  DEFAULT (getutcdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[SubverseSetSubscription] ADD  CONSTRAINT [DF_SubverseSetSubscription_CreationDate]  DEFAULT (getutcdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[UserBlockedUser] ADD  CONSTRAINT [DF_UserBlockedUser_CreationDate]  DEFAULT (getutcdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[UserContribution] ADD  CONSTRAINT [DF_UserContribution_IsReceived]  DEFAULT ((1)) FOR [IsReceived]
GO
ALTER TABLE [dbo].[UserContribution] ADD  CONSTRAINT [DF_UserContribution_VoteCount]  DEFAULT ((0)) FOR [VoteCount]
GO
ALTER TABLE [dbo].[UserContribution] ADD  CONSTRAINT [DF_UserContribution_LastUpdateDate]  DEFAULT (getutcdate()) FOR [LastUpdateDate]
GO
ALTER TABLE [dbo].[UserPreference] ADD  CONSTRAINT [DF_UserPreference_DisplaySubscriptions]  DEFAULT ((0)) FOR [DisplaySubscriptions]
GO
ALTER TABLE [dbo].[UserPreference] ADD  CONSTRAINT [DF_UserPreference_UseSubscriptionsMenu]  DEFAULT ((1)) FOR [UseSubscriptionsMenu]
GO
ALTER TABLE [dbo].[UserPreference] ADD  CONSTRAINT [DF_UserPreference_DisplayAds]  DEFAULT ((0)) FOR [DisplayAds]
GO
ALTER TABLE [dbo].[UserPreference] ADD  CONSTRAINT [DF_UserPreference_BlockAnonymized]  DEFAULT ((0)) FOR [BlockAnonymized]
GO
ALTER TABLE [dbo].[ApiClient]  WITH CHECK ADD  CONSTRAINT [FK_ApiClient_ApiPermissionPolicy] FOREIGN KEY([ApiPermissionPolicyID])
REFERENCES [dbo].[ApiPermissionPolicy] ([ID])
GO
ALTER TABLE [dbo].[ApiClient] CHECK CONSTRAINT [FK_ApiClient_ApiPermissionPolicy]
GO
ALTER TABLE [dbo].[ApiClient]  WITH CHECK ADD  CONSTRAINT [FK_ApiClient_ApiThrottlePolicy] FOREIGN KEY([ApiThrottlePolicyID])
REFERENCES [dbo].[ApiThrottlePolicy] ([ID])
GO
ALTER TABLE [dbo].[ApiClient] CHECK CONSTRAINT [FK_ApiClient_ApiThrottlePolicy]
GO
ALTER TABLE [dbo].[Comment]  WITH NOCHECK ADD  CONSTRAINT [FK_Comment_Submission] FOREIGN KEY([SubmissionID])
REFERENCES [dbo].[Submission] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Comment] CHECK CONSTRAINT [FK_Comment_Submission]
GO
ALTER TABLE [dbo].[CommentRemovalLog]  WITH NOCHECK ADD  CONSTRAINT [FK_CommentRemovalLog_Comment] FOREIGN KEY([CommentID])
REFERENCES [dbo].[Comment] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommentRemovalLog] CHECK CONSTRAINT [FK_CommentRemovalLog_Comment]
GO
ALTER TABLE [dbo].[CommentSaveTracker]  WITH NOCHECK ADD  CONSTRAINT [FK_CommentSaveTracker_Comment] FOREIGN KEY([CommentID])
REFERENCES [dbo].[Comment] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommentSaveTracker] CHECK CONSTRAINT [FK_CommentSaveTracker_Comment]
GO
ALTER TABLE [dbo].[CommentVoteTracker]  WITH NOCHECK ADD  CONSTRAINT [FK_CommentVoteTracker_Comment] FOREIGN KEY([CommentID])
REFERENCES [dbo].[Comment] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommentVoteTracker] CHECK CONSTRAINT [FK_CommentVoteTracker_Comment]
GO
ALTER TABLE [dbo].[DefaultSubverse]  WITH NOCHECK ADD  CONSTRAINT [FK_DefaultSubverse_Subverse] FOREIGN KEY([Subverse])
REFERENCES [dbo].[Subverse] ([Name])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DefaultSubverse] CHECK CONSTRAINT [FK_DefaultSubverse_Subverse]
GO
ALTER TABLE [dbo].[RuleReport]  WITH CHECK ADD  CONSTRAINT [FK_RuleReport_RuleSet] FOREIGN KEY([RuleSetID])
REFERENCES [dbo].[RuleSet] ([ID])
GO
ALTER TABLE [dbo].[RuleReport] CHECK CONSTRAINT [FK_RuleReport_RuleSet]
GO
ALTER TABLE [dbo].[StickiedSubmission]  WITH NOCHECK ADD  CONSTRAINT [FK_StickiedSubmission_Submission] FOREIGN KEY([SubmissionID])
REFERENCES [dbo].[Submission] ([ID])
GO
ALTER TABLE [dbo].[StickiedSubmission] CHECK CONSTRAINT [FK_StickiedSubmission_Submission]
GO
ALTER TABLE [dbo].[StickiedSubmission]  WITH NOCHECK ADD  CONSTRAINT [FK_StickiedSubmission_Subverse] FOREIGN KEY([Subverse])
REFERENCES [dbo].[Subverse] ([Name])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StickiedSubmission] CHECK CONSTRAINT [FK_StickiedSubmission_Subverse]
GO
ALTER TABLE [dbo].[Submission]  WITH NOCHECK ADD  CONSTRAINT [FK_Submission_Subverse] FOREIGN KEY([Subverse])
REFERENCES [dbo].[Subverse] ([Name])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Submission] CHECK CONSTRAINT [FK_Submission_Subverse]
GO
ALTER TABLE [dbo].[SubmissionRemovalLog]  WITH NOCHECK ADD  CONSTRAINT [FK_SubmissionRemovalLog_Submission] FOREIGN KEY([SubmissionID])
REFERENCES [dbo].[Submission] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SubmissionRemovalLog] CHECK CONSTRAINT [FK_SubmissionRemovalLog_Submission]
GO
ALTER TABLE [dbo].[SubmissionSaveTracker]  WITH NOCHECK ADD  CONSTRAINT [FK_SubmissionSaveTracker_Submission] FOREIGN KEY([SubmissionID])
REFERENCES [dbo].[Submission] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SubmissionSaveTracker] CHECK CONSTRAINT [FK_SubmissionSaveTracker_Submission]
GO
ALTER TABLE [dbo].[SubmissionVoteTracker]  WITH NOCHECK ADD  CONSTRAINT [FK_SubmissionVoteTracker_Submission] FOREIGN KEY([SubmissionID])
REFERENCES [dbo].[Submission] ([ID])
GO
ALTER TABLE [dbo].[SubmissionVoteTracker] NOCHECK CONSTRAINT [FK_SubmissionVoteTracker_Submission]
GO
ALTER TABLE [dbo].[SubverseBan]  WITH NOCHECK ADD  CONSTRAINT [FK_SubverseBan_Subverse] FOREIGN KEY([Subverse])
REFERENCES [dbo].[Subverse] ([Name])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SubverseBan] CHECK CONSTRAINT [FK_SubverseBan_Subverse]
GO
ALTER TABLE [dbo].[SubverseFlair]  WITH NOCHECK ADD  CONSTRAINT [FK_SubverseFlair_Subverse] FOREIGN KEY([Subverse])
REFERENCES [dbo].[Subverse] ([Name])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SubverseFlair] CHECK CONSTRAINT [FK_SubverseFlair_Subverse]
GO
ALTER TABLE [dbo].[SubverseModerator]  WITH NOCHECK ADD  CONSTRAINT [FK_SubverseModerator_Subverse] FOREIGN KEY([Subverse])
REFERENCES [dbo].[Subverse] ([Name])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SubverseModerator] CHECK CONSTRAINT [FK_SubverseModerator_Subverse]
GO
ALTER TABLE [dbo].[SubverseSetList]  WITH CHECK ADD  CONSTRAINT [FK_SubverseSetList_Subverse] FOREIGN KEY([SubverseID])
REFERENCES [dbo].[Subverse] ([ID])
GO
ALTER TABLE [dbo].[SubverseSetList] CHECK CONSTRAINT [FK_SubverseSetList_Subverse]
GO
ALTER TABLE [dbo].[SubverseSetList]  WITH CHECK ADD  CONSTRAINT [FK_SubverseSetList_SubverseSet] FOREIGN KEY([SubverseSetID])
REFERENCES [dbo].[SubverseSet] ([ID])
GO
ALTER TABLE [dbo].[SubverseSetList] CHECK CONSTRAINT [FK_SubverseSetList_SubverseSet]
GO
ALTER TABLE [dbo].[SubverseSetSubscription]  WITH NOCHECK ADD  CONSTRAINT [FK_SubverseSetSubscription_Set] FOREIGN KEY([SubverseSetID])
REFERENCES [dbo].[SubverseSet] ([ID])
GO
ALTER TABLE [dbo].[SubverseSetSubscription] CHECK CONSTRAINT [FK_SubverseSetSubscription_Set]
GO
ALTER TABLE [dbo].[UserBadge]  WITH NOCHECK ADD  CONSTRAINT [FK_UserBadge_Badge] FOREIGN KEY([BadgeID])
REFERENCES [dbo].[Badge] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserBadge] CHECK CONSTRAINT [FK_UserBadge_Badge]
GO
ALTER TABLE [dbo].[ViewStatistic]  WITH NOCHECK ADD  CONSTRAINT [FK_ViewStatistic_Submission] FOREIGN KEY([SubmissionID])
REFERENCES [dbo].[Submission] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ViewStatistic] CHECK CONSTRAINT [FK_ViewStatistic_Submission]
GO

/*** VOTE SCHEMA ***/

/****** Object:  Table [dbo].[Vote]    Script Date: 8/31/2017 1:40:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Vote](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Content] [nvarchar](max) NULL,
	[FormattedContent] [nvarchar](max) NULL,
	[Subverse] [nvarchar](50) NULL,
	[SubmissionID] [int] NULL,
	[DisplayStatistics] [bit] NOT NULL,
	[Status] [int] NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[LastEditDate] [datetime] NULL,
	[ProcessedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Vote] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[VoteOption]    Script Date: 8/31/2017 1:40:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VoteOption](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[VoteID] [int] NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Content] [nvarchar](max) NULL,
	[FormattedContent] [nvarchar](max) NULL,
	[SortOrder] [int] NOT NULL,
 CONSTRAINT [PK_VoteOption] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[VoteOptionOutcome]    Script Date: 8/31/2017 1:40:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VoteOutcome](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[VoteOptionID] [int] NOT NULL,
	[Type] [nvarchar](1000) NOT NULL,
	[Data] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_VoteOutcome] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[VoteRestriction]    Script Date: 8/31/2017 1:40:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VoteRestriction](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[VoteID] [int] NOT NULL,
	[Type] [nvarchar](1000) NOT NULL,
	[Data] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_VoteRestriction] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[VoteTracker]    Script Date: 8/31/2017 1:40:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VoteTracker](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[VoteID] [int] NOT NULL,
	[VoteOptionID] [int] NOT NULL,
	[RestrictionsPassed] [bit] NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_VoteTracker] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[Vote] ADD  CONSTRAINT [DF_Vote_CreationDate]  DEFAULT (getutcdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[VoteTracker] ADD  CONSTRAINT [DF_VoteTracker_CreationDate]  DEFAULT (getutcdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[VoteOption]  WITH CHECK ADD  CONSTRAINT [FK_VoteOption_Vote] FOREIGN KEY([VoteID])
REFERENCES [dbo].[Vote] ([ID])
GO
ALTER TABLE [dbo].[VoteOption] CHECK CONSTRAINT [FK_VoteOption_Vote]
GO
ALTER TABLE [dbo].[VoteOutcome]  WITH CHECK ADD  CONSTRAINT [FK_VoteOutcome_VoteOption] FOREIGN KEY([VoteOptionID])
REFERENCES [dbo].[VoteOption] ([ID])
GO
ALTER TABLE [dbo].[VoteOutcome] CHECK CONSTRAINT [FK_VoteOutcome_VoteOption]
GO
ALTER TABLE [dbo].[VoteRestriction]  WITH CHECK ADD  CONSTRAINT [FK_VoteRestriction_Vote] FOREIGN KEY([VoteID])
REFERENCES [dbo].[Vote] ([ID])
GO
ALTER TABLE [dbo].[VoteRestriction] CHECK CONSTRAINT [FK_VoteRestriction_Vote]
GO
ALTER TABLE [dbo].[VoteTracker]  WITH CHECK ADD  CONSTRAINT [FK_VoteTracker_Vote] FOREIGN KEY([VoteID])
REFERENCES [dbo].[Vote] ([ID])
GO
ALTER TABLE [dbo].[VoteTracker] CHECK CONSTRAINT [FK_VoteTracker_Vote]
GO
ALTER TABLE [dbo].[VoteTracker]  WITH CHECK ADD  CONSTRAINT [FK_VoteTracker_VoteOption] FOREIGN KEY([VoteOptionID])
REFERENCES [dbo].[VoteOption] ([ID])
GO
ALTER TABLE [dbo].[VoteTracker] CHECK CONSTRAINT [FK_VoteTracker_VoteOption]
GO

