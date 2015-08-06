USE [voat]
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
PRINT N'Dropping foreign keys from [dbo].[AspNetUserRoles]'
GO
ALTER TABLE [dbo].[AspNetUserRoles] DROP CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId]
ALTER TABLE [dbo].[AspNetUserRoles] DROP CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[AspNetUserClaims]'
GO
ALTER TABLE [dbo].[AspNetUserClaims] DROP CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[AspNetUserLogins]'
GO
ALTER TABLE [dbo].[AspNetUserLogins] DROP CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[Userbadges]'
GO
ALTER TABLE [dbo].[Userbadges] DROP CONSTRAINT [FK_Userbadges_Badges]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[Defaultsubverses]'
GO
ALTER TABLE [dbo].[Defaultsubverses] DROP CONSTRAINT [FK_Defaultsubverses_Subverses]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[Featuredsubs]'
GO
ALTER TABLE [dbo].[Featuredsubs] DROP CONSTRAINT [FK_Featuredsubs_Subverses]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[Messages]'
GO
ALTER TABLE [dbo].[Messages] DROP CONSTRAINT [FK_Messages_Subverses]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[Stickiedsubmissions]'
GO
ALTER TABLE [dbo].[Stickiedsubmissions] DROP CONSTRAINT [FK_Stickiedsubmissions_Subverses]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[Subscriptions]'
GO
ALTER TABLE [dbo].[Subscriptions] DROP CONSTRAINT [FK_Subscriptions_Subverses]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[SubverseAdmins]'
GO
ALTER TABLE [dbo].[SubverseAdmins] DROP CONSTRAINT [FK_SubverseAdmins_Subverses]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[SubverseBans]'
GO
ALTER TABLE [dbo].[SubverseBans] DROP CONSTRAINT [FK_SubverseBans_Subverses]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[Subverseflairsettings]'
GO
ALTER TABLE [dbo].[Subverseflairsettings] DROP CONSTRAINT [FK_Subverseflairsettings_Subverses1]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[UserBlockedSubverses]'
GO
ALTER TABLE [dbo].[UserBlockedSubverses] DROP CONSTRAINT [FK_UserBlockedSubverses_Subverses]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[Usersetdefinitions]'
GO
ALTER TABLE [dbo].[Usersetdefinitions] DROP CONSTRAINT [FK_Usersetdefinitions_Subverses]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping foreign keys from [dbo].[Viewstatistics]'
GO
ALTER TABLE [dbo].[Viewstatistics] DROP CONSTRAINT [FK_Viewstatistics_Messages]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[AspNetRoles]'
GO
ALTER TABLE [dbo].[AspNetRoles] DROP CONSTRAINT [PK_dbo.AspNetRoles]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[AspNetUserClaims]'
GO
ALTER TABLE [dbo].[AspNetUserClaims] DROP CONSTRAINT [PK_dbo.AspNetUserClaims]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[AspNetUserLogins]'
GO
ALTER TABLE [dbo].[AspNetUserLogins] DROP CONSTRAINT [PK_dbo.AspNetUserLogins]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[AspNetUserRoles]'
GO
ALTER TABLE [dbo].[AspNetUserRoles] DROP CONSTRAINT [PK_dbo.AspNetUserRoles]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[AspNetUsers]'
GO
ALTER TABLE [dbo].[AspNetUsers] DROP CONSTRAINT [PK_dbo.AspNetUsers]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[Badges]'
GO
ALTER TABLE [dbo].[Badges] DROP CONSTRAINT [PK_Badges]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[Defaultsubverses]'
GO
ALTER TABLE [dbo].[Defaultsubverses] DROP CONSTRAINT [PK_Defaultsubverses]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[Sessions]'
GO
ALTER TABLE [dbo].[Sessions] DROP CONSTRAINT [PK_Sessions]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[Sessiontracker]'
GO
ALTER TABLE [dbo].[Sessiontracker] DROP CONSTRAINT [PK_Sessiontracker_1]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[Subverses]'
GO
ALTER TABLE [dbo].[Subverses] DROP CONSTRAINT [PK_Subverses]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[Userpreferences]'
GO
ALTER TABLE [dbo].[Userpreferences] DROP CONSTRAINT [PK_Userpreferences]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[Userscore]'
GO
ALTER TABLE [dbo].[Userscore] DROP CONSTRAINT [PK_Userscore]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[Viewstatistics]'
GO
ALTER TABLE [dbo].[Viewstatistics] DROP CONSTRAINT [PK_Viewstatistics]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[__MigrationHistory]'
GO
ALTER TABLE [dbo].[__MigrationHistory] DROP CONSTRAINT [PK_dbo.__MigrationHistory]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [RoleNameIndex] from [dbo].[AspNetRoles]'
GO
DROP INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [IX_UserId] from [dbo].[AspNetUserClaims]'
GO
DROP INDEX [IX_UserId] ON [dbo].[AspNetUserClaims]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [IX_UserId] from [dbo].[AspNetUserLogins]'
GO
DROP INDEX [IX_UserId] ON [dbo].[AspNetUserLogins]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [IX_UserId] from [dbo].[AspNetUserRoles]'
GO
DROP INDEX [IX_UserId] ON [dbo].[AspNetUserRoles]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [IX_RoleId] from [dbo].[AspNetUserRoles]'
GO
DROP INDEX [IX_RoleId] ON [dbo].[AspNetUserRoles]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [UserNameIndex] from [dbo].[AspNetUsers]'
GO
DROP INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [_dta_index_Commentreplynotifications_7_1131151075__K4_K10_K8] from [dbo].[Commentreplynotifications]'
GO
DROP INDEX [_dta_index_Commentreplynotifications_7_1131151075__K4_K10_K8] ON [dbo].[Commentreplynotifications]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [_dta_index_Comments_7_1163151189__K3_K7_K1_9] from [dbo].[Comments]'
GO
DROP INDEX [_dta_index_Comments_7_1163151189__K3_K7_K1_9] ON [dbo].[Comments]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [_dta_index_Comments_7_1163151189__K3_K7_K1_8] from [dbo].[Comments]'
GO
DROP INDEX [_dta_index_Comments_7_1163151189__K3_K7_K1_8] ON [dbo].[Comments]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [NonClusteredIndex-20141105-141850] from [dbo].[Commentvotingtracker]'
GO
DROP INDEX [NonClusteredIndex-20141105-141850] ON [dbo].[Commentvotingtracker]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [_dta_index_Commentvotingtracker_7_2132202646__K3_K4_K1_5] from [dbo].[Commentvotingtracker]'
GO
DROP INDEX [_dta_index_Commentvotingtracker_7_2132202646__K3_K4_K1_5] ON [dbo].[Commentvotingtracker]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [_dta_index_Messages_7_939150391__K3_K10_K1_11] from [dbo].[Messages]'
GO
DROP INDEX [_dta_index_Messages_7_939150391__K3_K10_K1_11] ON [dbo].[Messages]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [_dta_index_Messages_7_939150391__K10_K3_K1_4364] from [dbo].[Messages]'
GO
DROP INDEX [_dta_index_Messages_7_939150391__K10_K3_K1_4364] ON [dbo].[Messages]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [Subverse-NonClusteredIndex-20141102-194539] from [dbo].[Messages]'
GO
DROP INDEX [Subverse-NonClusteredIndex-20141102-194539] ON [dbo].[Messages]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [_dta_index_Postreplynotifications_7_1355151873__K4_K10_K8_1_2_3_5_6_7_9_11] from [dbo].[Postreplynotifications]'
GO
DROP INDEX [_dta_index_Postreplynotifications_7_1355151873__K4_K10_K8_1_2_3_5_6_7_9_11] ON [dbo].[Postreplynotifications]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [_dta_index_Privatemessages_7_1387151987__K7_K8_K3_4149] from [dbo].[Privatemessages]'
GO
DROP INDEX [_dta_index_Privatemessages_7_1387151987__K7_K8_K3_4149] ON [dbo].[Privatemessages]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [NonClusteredIndex-20140929-165519] from [dbo].[Stickiedsubmissions]'
GO
DROP INDEX [NonClusteredIndex-20140929-165519] ON [dbo].[Stickiedsubmissions]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [NonClusteredIndex-20140924-135851] from [dbo].[Subscriptions]'
GO
DROP INDEX [NonClusteredIndex-20140924-135851] ON [dbo].[Subscriptions]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [_dta_index_Subscriptions_7_1106102981__K3_K2_K1_4364] from [dbo].[Subscriptions]'
GO
DROP INDEX [_dta_index_Subscriptions_7_1106102981__K3_K2_K1_4364] ON [dbo].[Subscriptions]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [NonClusteredIndex-20140924-135902] from [dbo].[Subscriptions]'
GO
DROP INDEX [NonClusteredIndex-20140924-135902] ON [dbo].[Subscriptions]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [NonClusteredIndex-20140924-135638] from [dbo].[SubverseAdmins]'
GO
DROP INDEX [NonClusteredIndex-20140924-135638] ON [dbo].[SubverseAdmins]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [NonClusteredIndex-20140924-140216] from [dbo].[Subverseflairsettings]'
GO
DROP INDEX [NonClusteredIndex-20140924-140216] ON [dbo].[Subverseflairsettings]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [_dta_index_Subverses_7_1668200993__K13_K22_K27_K1_9850] from [dbo].[Subverses]'
GO
DROP INDEX [_dta_index_Subverses_7_1668200993__K13_K22_K27_K1_9850] ON [dbo].[Subverses]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [NonClusteredIndex-20140924-140240] from [dbo].[Subverses]'
GO
DROP INDEX [NonClusteredIndex-20140924-140240] ON [dbo].[Subverses]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [NonClusteredIndex-20150506-152839] from [dbo].[UserBlockedSubverses]'
GO
DROP INDEX [NonClusteredIndex-20150506-152839] ON [dbo].[UserBlockedSubverses]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [NonClusteredIndex-20150506-152859] from [dbo].[UserBlockedSubverses]'
GO
DROP INDEX [NonClusteredIndex-20150506-152859] ON [dbo].[UserBlockedSubverses]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [NonClusteredIndex-20140924-140311] from [dbo].[Userbadges]'
GO
DROP INDEX [NonClusteredIndex-20140924-140311] ON [dbo].[Userbadges]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [_dta_index_Votingtracker_7_2084202475__K2_K3_K1_4_5_6_1040] from [dbo].[Votingtracker]'
GO
DROP INDEX [_dta_index_Votingtracker_7_2084202475__K2_K3_K1_4_5_6_1040] ON [dbo].[Votingtracker]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [NonClusteredIndex-20141105-141950] from [dbo].[Votingtracker]'
GO
DROP INDEX [NonClusteredIndex-20141105-141950] ON [dbo].[Votingtracker]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping index [_dta_index_Votingtracker_7_2084202475__K3_K5_K1_1912] from [dbo].[Votingtracker]'
GO
DROP INDEX [_dta_index_Votingtracker_7_2084202475__K3_K5_K1_1912] ON [dbo].[Votingtracker]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping [dbo].[AspNetUserRoles]'
GO
DROP TABLE [dbo].[AspNetUserRoles]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping [dbo].[AspNetRoles]'
GO
DROP TABLE [dbo].[AspNetRoles]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping [dbo].[AspNetUserLogins]'
GO
DROP TABLE [dbo].[AspNetUserLogins]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping [dbo].[AspNetUserClaims]'
GO
DROP TABLE [dbo].[AspNetUserClaims]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping [dbo].[AspNetUsers]'
GO
DROP TABLE [dbo].[AspNetUsers]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Commentvotingtracker]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Commentvotingtracker] ALTER COLUMN [UserName] [nvarchar] (50) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Commentvotingtracker] ALTER COLUMN [ClientIpAddress] [nvarchar] (90) COLLATE Latin1_General_CI_AS NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [NonClusteredIndex-20141105-141850] on [dbo].[Commentvotingtracker]'
GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20141105-141850] ON [dbo].[Commentvotingtracker] ([UserName])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [_dta_index_Commentvotingtracker_7_2132202646__K3_K4_K1_5] on [dbo].[Commentvotingtracker]'
GO
CREATE NONCLUSTERED INDEX [_dta_index_Commentvotingtracker_7_2132202646__K3_K4_K1_5] ON [dbo].[Commentvotingtracker] ([UserName], [VoteStatus], [Id]) INCLUDE ([Timestamp])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Comments]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Comments] ALTER COLUMN [Name] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Comments] ALTER COLUMN [CommentContent] [nvarchar] (max) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Comments] ALTER COLUMN [FormattedContent] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [_dta_index_Comments_7_1163151189__K3_K7_K1_9] on [dbo].[Comments]'
GO
CREATE NONCLUSTERED INDEX [_dta_index_Comments_7_1163151189__K3_K7_K1_9] ON [dbo].[Comments] ([Name], [MessageId], [Id]) INCLUDE ([Dislikes])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [_dta_index_Comments_7_1163151189__K3_K7_K1_8] on [dbo].[Comments]'
GO
CREATE NONCLUSTERED INDEX [_dta_index_Comments_7_1163151189__K3_K7_K1_8] ON [dbo].[Comments] ([Name], [MessageId], [Id]) INCLUDE ([Likes])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1163151189_11_3_7] on [dbo].[Comments]'
GO
CREATE STATISTICS [_dta_stat_1163151189_11_3_7] ON [dbo].[Comments] ([Anonymized], [Name], [MessageId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1163151189_5_3] on [dbo].[Comments]'
GO
CREATE STATISTICS [_dta_stat_1163151189_5_3] ON [dbo].[Comments] ([Date], [Name])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1163151189_12_3] on [dbo].[Comments]'
GO
CREATE STATISTICS [_dta_stat_1163151189_12_3] ON [dbo].[Comments] ([IsDistinguished], [Name])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1163151189_7_3_5] on [dbo].[Comments]'
GO
CREATE STATISTICS [_dta_stat_1163151189_7_3_5] ON [dbo].[Comments] ([MessageId], [Name], [Date])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1163151189_3_1] on [dbo].[Comments]'
GO
CREATE STATISTICS [_dta_stat_1163151189_3_1] ON [dbo].[Comments] ([Name], [Id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1163151189_5_1] on [dbo].[Comments]'
GO
CREATE STATISTICS [_dta_stat_1163151189_5_1] ON [dbo].[Comments] ([Date], [Id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1163151189_9] on [dbo].[Comments]'
GO
CREATE STATISTICS [_dta_stat_1163151189_9] ON [dbo].[Comments] ([Dislikes])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1163151189_1_7] on [dbo].[Comments]'
GO
CREATE STATISTICS [_dta_stat_1163151189_1_7] ON [dbo].[Comments] ([Id], [MessageId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1163151189_12_1] on [dbo].[Comments]'
GO
CREATE STATISTICS [_dta_stat_1163151189_12_1] ON [dbo].[Comments] ([IsDistinguished], [Id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1163151189_7_11] on [dbo].[Comments]'
GO
CREATE STATISTICS [_dta_stat_1163151189_7_11] ON [dbo].[Comments] ([MessageId], [Anonymized])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Subverses]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Subverses] ALTER COLUMN [name] [nvarchar] (20) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Subverses] ALTER COLUMN [title] [nvarchar] (500) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Subverses] ALTER COLUMN [description] [nvarchar] (500) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Subverses] ALTER COLUMN [sidebar] [nvarchar] (4000) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Subverses] ALTER COLUMN [submission_text] [nvarchar] (500) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Subverses] ALTER COLUMN [language] [nvarchar] (10) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Subverses] ALTER COLUMN [type] [nvarchar] (10) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Subverses] ALTER COLUMN [label_submit_new_link] [nvarchar] (50) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Subverses] ALTER COLUMN [label_sumit_new_selfpost] [nvarchar] (50) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Subverses] ALTER COLUMN [spam_filter_links] [nvarchar] (10) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Subverses] ALTER COLUMN [spam_filter_selfpost] [nvarchar] (10) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Subverses] ALTER COLUMN [spam_filter_comments] [nvarchar] (10) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Subverses] ALTER COLUMN [stylesheet] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Subverses] on [dbo].[Subverses]'
GO
ALTER TABLE [dbo].[Subverses] ADD CONSTRAINT [PK_Subverses] PRIMARY KEY CLUSTERED  ([name])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [_dta_index_Subverses_7_1668200993__K13_K22_K27_K1_9850] on [dbo].[Subverses]'
GO
CREATE NONCLUSTERED INDEX [_dta_index_Subverses_7_1668200993__K13_K22_K27_K1_9850] ON [dbo].[Subverses] ([rated_adult], [private_subverse], [forced_private], [name])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [NonClusteredIndex-20140924-140240] on [dbo].[Subverses]'
GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20140924-140240] ON [dbo].[Subverses] ([title])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[SubverseAdmins]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[SubverseAdmins] ALTER COLUMN [SubverseName] [nvarchar] (20) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[SubverseAdmins] ALTER COLUMN [Username] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[SubverseAdmins] ALTER COLUMN [Added_by] [nvarchar] (50) COLLATE Latin1_General_CI_AS NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [NonClusteredIndex-20140924-135638] on [dbo].[SubverseAdmins]'
GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20140924-135638] ON [dbo].[SubverseAdmins] ([SubverseName], [Username])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Postreplynotifications]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Postreplynotifications] ALTER COLUMN [Recipient] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Postreplynotifications] ALTER COLUMN [Sender] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Postreplynotifications] ALTER COLUMN [Subject] [nvarchar] (200) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Postreplynotifications] ALTER COLUMN [Body] [nvarchar] (4000) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Postreplynotifications] ALTER COLUMN [Subverse] [nvarchar] (20) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [_dta_index_Postreplynotifications_7_1355151873__K4_K10_K8_1_2_3_5_6_7_9_11] on [dbo].[Postreplynotifications]'
GO
CREATE NONCLUSTERED INDEX [_dta_index_Postreplynotifications_7_1355151873__K4_K10_K8_1_2_3_5_6_7_9_11] ON [dbo].[Postreplynotifications] ([Recipient], [Markedasunread], [Status]) INCLUDE ([Body], [CommentId], [Id], [Sender], [Subject], [SubmissionId], [Subverse], [Timestamp])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1355151873_2_4] on [dbo].[Postreplynotifications]'
GO
CREATE STATISTICS [_dta_stat_1355151873_2_4] ON [dbo].[Postreplynotifications] ([CommentId], [Recipient])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1355151873_10_8_4] on [dbo].[Postreplynotifications]'
GO
CREATE STATISTICS [_dta_stat_1355151873_10_8_4] ON [dbo].[Postreplynotifications] ([Markedasunread], [Status], [Recipient])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Messages]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Messages] ALTER COLUMN [Name] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Messages] ALTER COLUMN [MessageContent] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Messages] ALTER COLUMN [Linkdescription] [nvarchar] (200) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Messages] ALTER COLUMN [Title] [nvarchar] (200) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Messages] ALTER COLUMN [Subverse] [nvarchar] (20) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Messages] ALTER COLUMN [Thumbnail] [nchar] (40) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Messages] ALTER COLUMN [FlairLabel] [nvarchar] (50) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Messages] ALTER COLUMN [FlairCss] [nvarchar] (50) COLLATE Latin1_General_CI_AS NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [_dta_index_Messages_7_939150391__K3_K10_K1_11] on [dbo].[Messages]'
GO
CREATE NONCLUSTERED INDEX [_dta_index_Messages_7_939150391__K3_K10_K1_11] ON [dbo].[Messages] ([Name], [Subverse], [Id]) INCLUDE ([Likes])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [_dta_index_Messages_7_939150391__K10_K3_K1_4364] on [dbo].[Messages]'
GO
CREATE NONCLUSTERED INDEX [_dta_index_Messages_7_939150391__K10_K3_K1_4364] ON [dbo].[Messages] ([Subverse], [Name], [Id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [Subverse-NonClusteredIndex-20141102-194539] on [dbo].[Messages]'
GO
CREATE NONCLUSTERED INDEX [Subverse-NonClusteredIndex-20141102-194539] ON [dbo].[Messages] ([Subverse])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Sessiontracker]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Sessiontracker] ALTER COLUMN [SessionId] [nvarchar] (90) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Sessiontracker] ALTER COLUMN [Subverse] [nvarchar] (20) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Sessiontracker_1] on [dbo].[Sessiontracker]'
GO
ALTER TABLE [dbo].[Sessiontracker] ADD CONSTRAINT [PK_Sessiontracker_1] PRIMARY KEY CLUSTERED  ([SessionId], [Subverse])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[usp_CommentTree]'
GO


CREATE PROC [dbo].[usp_CommentTree]
	@SubmissionID INT,
	@Depth INT = NULL,
	@ParentID INT = NULL
AS

/*
I spent 14 hours working on this. Not. Even. Joking. 
			   Man, Fuuuuuck SQL. 
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
	SELECT p.MessageID, p.Name, ID, 0 FROM Comments p 
	WHERE 
		1 = 1
		AND p.ParentID IS NULL
		AND p.MessageID = @SubmissionID
	UNION ALL
	SELECT c.MessageID, c.Name, c.ParentID, c.ID FROM Comments c
	WHERE
		1 = 1 
		AND c.ParentID IS NOT NULL
		AND c.MessageID = @SubmissionID
ELSE 
	INSERT INTO @tree
	SELECT p.MessageID, p.Name, ID, 0 FROM Comments p 
	WHERE 
		1 = 1
		AND p.ParentID = @ParentID
		AND p.MessageID = @SubmissionID
	UNION ALL
	SELECT c.MessageID, c.Name, c.ParentID, c.ID FROM Comments c
	WHERE
		1 = 1
		AND c.ParentID IS NOT NULL 
		AND c.ParentID > @ParentID
		AND c.MessageID = @SubmissionID

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
INNER JOIN Comments c ON (c.ID = CASE WHEN ChildID IS NULL OR ChildID = 0 THEN h.ParentID ELSE ChildID END)
INNER JOIN Messages m ON (c.MessageID = m.ID)
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
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Votingtracker]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Votingtracker] ALTER COLUMN [UserName] [nvarchar] (50) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Votingtracker] ALTER COLUMN [ClientIpAddress] [nvarchar] (90) COLLATE Latin1_General_CI_AS NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [_dta_index_Votingtracker_7_2084202475__K2_K3_K1_4_5_6_1040] on [dbo].[Votingtracker]'
GO
CREATE NONCLUSTERED INDEX [_dta_index_Votingtracker_7_2084202475__K2_K3_K1_4_5_6_1040] ON [dbo].[Votingtracker] ([MessageId], [UserName], [Id]) INCLUDE ([ClientIpAddress], [Timestamp], [VoteStatus])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [NonClusteredIndex-20141105-141950] on [dbo].[Votingtracker]'
GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20141105-141950] ON [dbo].[Votingtracker] ([UserName])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [_dta_index_Votingtracker_7_2084202475__K3_K5_K1_1912] on [dbo].[Votingtracker]'
GO
CREATE NONCLUSTERED INDEX [_dta_index_Votingtracker_7_2084202475__K3_K5_K1_1912] ON [dbo].[Votingtracker] ([UserName], [Timestamp], [Id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Userpreferences]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Userpreferences] ALTER COLUMN [Username] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Userpreferences] ALTER COLUMN [Language] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Userpreferences] ALTER COLUMN [Shortbio] [nvarchar] (100) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Userpreferences] ALTER COLUMN [Avatar] [nvarchar] (50) COLLATE Latin1_General_CI_AS NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Userpreferences] on [dbo].[Userpreferences]'
GO
ALTER TABLE [dbo].[Userpreferences] ADD CONSTRAINT [PK_Userpreferences] PRIMARY KEY CLUSTERED  ([Username])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Usersets]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Usersets] ALTER COLUMN [Name] [nvarchar] (20) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Usersets] ALTER COLUMN [Description] [nvarchar] (200) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Usersets] ALTER COLUMN [Created_by] [nvarchar] (20) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Usersetsubscriptions]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Usersetsubscriptions] ALTER COLUMN [Username] [nvarchar] (20) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Commentsavingtracker]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Commentsavingtracker] ALTER COLUMN [UserName] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Defaultsubverses]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Defaultsubverses] ALTER COLUMN [name] [nvarchar] (20) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Defaultsubverses] on [dbo].[Defaultsubverses]'
GO
ALTER TABLE [dbo].[Defaultsubverses] ADD CONSTRAINT [PK_Defaultsubverses] PRIMARY KEY CLUSTERED  ([name])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Featuredsubs]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Featuredsubs] ALTER COLUMN [Subname] [nvarchar] (20) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Featuredsubs] ALTER COLUMN [Featured_by] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Savingtracker]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Savingtracker] ALTER COLUMN [UserName] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Stickiedsubmissions]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Stickiedsubmissions] ALTER COLUMN [Subversename] [nvarchar] (20) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Stickiedsubmissions] ALTER COLUMN [Stickied_by] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [NonClusteredIndex-20140929-165519] on [dbo].[Stickiedsubmissions]'
GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20140929-165519] ON [dbo].[Stickiedsubmissions] ([Subversename])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[SubmissionRemovalLog]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[SubmissionRemovalLog] ALTER COLUMN [Moderator] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[SubmissionRemovalLog] ALTER COLUMN [ReasonForRemoval] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Subscriptions]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Subscriptions] ALTER COLUMN [SubverseName] [nvarchar] (20) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Subscriptions] ALTER COLUMN [Username] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [NonClusteredIndex-20140924-135851] on [dbo].[Subscriptions]'
GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20140924-135851] ON [dbo].[Subscriptions] ([SubverseName])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [_dta_index_Subscriptions_7_1106102981__K3_K2_K1_4364] on [dbo].[Subscriptions]'
GO
CREATE NONCLUSTERED INDEX [_dta_index_Subscriptions_7_1106102981__K3_K2_K1_4364] ON [dbo].[Subscriptions] ([Username], [SubverseName], [Id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [NonClusteredIndex-20140924-135902] on [dbo].[Subscriptions]'
GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20140924-135902] ON [dbo].[Subscriptions] ([Username])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[SubverseBans]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[SubverseBans] ALTER COLUMN [SubverseName] [nvarchar] (20) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[SubverseBans] ALTER COLUMN [Username] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[SubverseBans] ALTER COLUMN [BannedBy] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[SubverseBans] ALTER COLUMN [BanReason] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Subverseflairsettings]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Subverseflairsettings] ALTER COLUMN [Subversename] [nvarchar] (20) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Subverseflairsettings] ALTER COLUMN [Label] [nvarchar] (50) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[Subverseflairsettings] ALTER COLUMN [CssClass] [nvarchar] (50) COLLATE Latin1_General_CI_AS NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [NonClusteredIndex-20140924-140216] on [dbo].[Subverseflairsettings]'
GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20140924-140216] ON [dbo].[Subverseflairsettings] ([Subversename])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Badges]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Badges] ALTER COLUMN [BadgeId] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Badges] ALTER COLUMN [BadgeGraphics] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Badges] ALTER COLUMN [BadgeTitle] [nvarchar] (300) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Badges] ALTER COLUMN [BadgeName] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Badges] on [dbo].[Badges]'
GO
ALTER TABLE [dbo].[Badges] ADD CONSTRAINT [PK_Badges] PRIMARY KEY CLUSTERED  ([BadgeId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Userbadges]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Userbadges] ALTER COLUMN [Username] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Userbadges] ALTER COLUMN [BadgeId] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [NonClusteredIndex-20140924-140311] on [dbo].[Userbadges]'
GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20140924-140311] ON [dbo].[Userbadges] ([Username])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[UserBlockedSubverses]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[UserBlockedSubverses] ALTER COLUMN [SubverseName] [nvarchar] (20) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[UserBlockedSubverses] ALTER COLUMN [Username] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [NonClusteredIndex-20150506-152839] on [dbo].[UserBlockedSubverses]'
GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20150506-152839] ON [dbo].[UserBlockedSubverses] ([SubverseName])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [NonClusteredIndex-20150506-152859] on [dbo].[UserBlockedSubverses]'
GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20150506-152859] ON [dbo].[UserBlockedSubverses] ([Username])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Usersetdefinitions]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Usersetdefinitions] ALTER COLUMN [Subversename] [nvarchar] (20) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Viewstatistics]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Viewstatistics] ALTER COLUMN [viewerId] [nvarchar] (90) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Viewstatistics] on [dbo].[Viewstatistics]'
GO
ALTER TABLE [dbo].[Viewstatistics] ADD CONSTRAINT [PK_Viewstatistics] PRIMARY KEY CLUSTERED  ([submissionId], [viewerId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Banneddomains]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Banneddomains] ALTER COLUMN [Hostname] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Banneddomains] ALTER COLUMN [Added_by] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Banneddomains] ALTER COLUMN [Reason] [nvarchar] (500) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Bannedusers]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Bannedusers] ALTER COLUMN [Username] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Bannedusers] ALTER COLUMN [Reason] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Bannedusers] ALTER COLUMN [Banned_by] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Commentreplynotifications]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Commentreplynotifications] ALTER COLUMN [Recipient] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Commentreplynotifications] ALTER COLUMN [Sender] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Commentreplynotifications] ALTER COLUMN [Subject] [nvarchar] (200) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Commentreplynotifications] ALTER COLUMN [Body] [nvarchar] (4000) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Commentreplynotifications] ALTER COLUMN [Subverse] [nvarchar] (20) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [_dta_index_Commentreplynotifications_7_1131151075__K4_K10_K8] on [dbo].[Commentreplynotifications]'
GO
CREATE NONCLUSTERED INDEX [_dta_index_Commentreplynotifications_7_1131151075__K4_K10_K8] ON [dbo].[Commentreplynotifications] ([Recipient], [Markedasunread], [Status])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1131151075_2_4] on [dbo].[Commentreplynotifications]'
GO
CREATE STATISTICS [_dta_stat_1131151075_2_4] ON [dbo].[Commentreplynotifications] ([CommentId], [Recipient])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1131151075_8_10] on [dbo].[Commentreplynotifications]'
GO
CREATE STATISTICS [_dta_stat_1131151075_8_10] ON [dbo].[Commentreplynotifications] ([Status], [Markedasunread])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Moderatorinvitations]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Moderatorinvitations] ALTER COLUMN [Sent_by] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Moderatorinvitations] ALTER COLUMN [Sent_to] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Moderatorinvitations] ALTER COLUMN [Subverse] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[PartnerInformations]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[PartnerInformations] ALTER COLUMN [FirstName] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[PartnerInformations] ALTER COLUMN [LastName] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[PartnerInformations] ALTER COLUMN [PartnerPaymentForm] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[PartnerInformations] ALTER COLUMN [PartnerBankAccountNumber] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[PartnerInformations] ALTER COLUMN [PartnerNameOfAccountHolder] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[PartnerInformations] ALTER COLUMN [PartnerSwiftCode] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[PartnerInformations] ALTER COLUMN [PartnerBankName] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[PartnerInformations] ALTER COLUMN [PartnerIFSC] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[PartnerInformations] ALTER COLUMN [PartnerBIK] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[PartnerInformations] ALTER COLUMN [PartnerPaymentCurrency] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[PartnerInformations] ALTER COLUMN [PartnerPhoneNumber] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[PartnerInformations] ALTER COLUMN [PartnerPayeeName] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[PartnerInformations] ALTER COLUMN [PartnerCity] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[PartnerInformations] ALTER COLUMN [PartnerCountry] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[PartnerInformations] ALTER COLUMN [PartnerZip] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
ALTER TABLE [dbo].[PartnerInformations] ALTER COLUMN [PartnerStreet] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Privatemessages]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Privatemessages] ALTER COLUMN [Sender] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Privatemessages] ALTER COLUMN [Recipient] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Privatemessages] ALTER COLUMN [Subject] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[Privatemessages] ALTER COLUMN [Body] [nvarchar] (4000) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1387151987_4_2_3_8_7] on [dbo].[Privatemessages]'
GO
CREATE STATISTICS [_dta_stat_1387151987_4_2_3_8_7] ON [dbo].[Privatemessages] ([Status], [Timestamp], [Sender], [Recipient], [Markedasunread])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [_dta_index_Privatemessages_7_1387151987__K7_K8_K3_4149] on [dbo].[Privatemessages]'
GO
CREATE NONCLUSTERED INDEX [_dta_index_Privatemessages_7_1387151987__K7_K8_K3_4149] ON [dbo].[Privatemessages] ([Status], [Markedasunread], [Recipient])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1387151987_3_4] on [dbo].[Privatemessages]'
GO
CREATE STATISTICS [_dta_stat_1387151987_3_4] ON [dbo].[Privatemessages] ([Recipient], [Timestamp])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1387151987_7_8_3_4] on [dbo].[Privatemessages]'
GO
CREATE STATISTICS [_dta_stat_1387151987_7_8_3_4] ON [dbo].[Privatemessages] ([Status], [Markedasunread], [Recipient], [Timestamp])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating statistics [_dta_stat_1387151987_7_3] on [dbo].[Privatemessages]'
GO
CREATE STATISTICS [_dta_stat_1387151987_7_3] ON [dbo].[Privatemessages] ([Status], [Recipient])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Promotedsubmissions]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Promotedsubmissions] ALTER COLUMN [promoted_by] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Sessions]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Sessions] ALTER COLUMN [SessionId] [nvarchar] (88) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Sessions] on [dbo].[Sessions]'
GO
ALTER TABLE [dbo].[Sessions] ADD CONSTRAINT [PK_Sessions] PRIMARY KEY CLUSTERED  ([SessionId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[Userscore]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[Userscore] ALTER COLUMN [Username] [nvarchar] (50) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Userscore] on [dbo].[Userscore]'
GO
ALTER TABLE [dbo].[Userscore] ADD CONSTRAINT [PK_Userscore] PRIMARY KEY CLUSTERED  ([Username])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[__MigrationHistory]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[__MigrationHistory] ALTER COLUMN [MigrationId] [nvarchar] (150) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[__MigrationHistory] ALTER COLUMN [ContextKey] [nvarchar] (300) COLLATE Latin1_General_CI_AS NOT NULL
ALTER TABLE [dbo].[__MigrationHistory] ALTER COLUMN [ProductVersion] [nvarchar] (32) COLLATE Latin1_General_CI_AS NOT NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_dbo.__MigrationHistory] on [dbo].[__MigrationHistory]'
GO
ALTER TABLE [dbo].[__MigrationHistory] ADD CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY CLUSTERED  ([MigrationId], [ContextKey])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[Userbadges]'
GO
ALTER TABLE [dbo].[Userbadges] WITH NOCHECK  ADD CONSTRAINT [FK_Userbadges_Badges] FOREIGN KEY ([BadgeId]) REFERENCES [dbo].[Badges] ([BadgeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[Defaultsubverses]'
GO
ALTER TABLE [dbo].[Defaultsubverses] WITH NOCHECK  ADD CONSTRAINT [FK_Defaultsubverses_Subverses] FOREIGN KEY ([name]) REFERENCES [dbo].[Subverses] ([name]) ON DELETE CASCADE ON UPDATE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[Featuredsubs]'
GO
ALTER TABLE [dbo].[Featuredsubs] WITH NOCHECK  ADD CONSTRAINT [FK_Featuredsubs_Subverses] FOREIGN KEY ([Subname]) REFERENCES [dbo].[Subverses] ([name])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[Messages]'
GO
ALTER TABLE [dbo].[Messages] WITH NOCHECK  ADD CONSTRAINT [FK_Messages_Subverses] FOREIGN KEY ([Subverse]) REFERENCES [dbo].[Subverses] ([name]) ON DELETE CASCADE ON UPDATE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[Stickiedsubmissions]'
GO
ALTER TABLE [dbo].[Stickiedsubmissions] WITH NOCHECK  ADD CONSTRAINT [FK_Stickiedsubmissions_Subverses] FOREIGN KEY ([Subversename]) REFERENCES [dbo].[Subverses] ([name]) ON DELETE CASCADE ON UPDATE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[Subscriptions]'
GO
ALTER TABLE [dbo].[Subscriptions] WITH NOCHECK  ADD CONSTRAINT [FK_Subscriptions_Subverses] FOREIGN KEY ([SubverseName]) REFERENCES [dbo].[Subverses] ([name]) ON DELETE CASCADE ON UPDATE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[SubverseAdmins]'
GO
ALTER TABLE [dbo].[SubverseAdmins] WITH NOCHECK  ADD CONSTRAINT [FK_SubverseAdmins_Subverses] FOREIGN KEY ([SubverseName]) REFERENCES [dbo].[Subverses] ([name]) ON DELETE CASCADE ON UPDATE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[SubverseBans]'
GO
ALTER TABLE [dbo].[SubverseBans] WITH NOCHECK  ADD CONSTRAINT [FK_SubverseBans_Subverses] FOREIGN KEY ([SubverseName]) REFERENCES [dbo].[Subverses] ([name]) ON DELETE CASCADE ON UPDATE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[Subverseflairsettings]'
GO
ALTER TABLE [dbo].[Subverseflairsettings] WITH NOCHECK  ADD CONSTRAINT [FK_Subverseflairsettings_Subverses1] FOREIGN KEY ([Subversename]) REFERENCES [dbo].[Subverses] ([name]) ON DELETE CASCADE ON UPDATE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[UserBlockedSubverses]'
GO
ALTER TABLE [dbo].[UserBlockedSubverses] WITH NOCHECK  ADD CONSTRAINT [FK_UserBlockedSubverses_Subverses] FOREIGN KEY ([SubverseName]) REFERENCES [dbo].[Subverses] ([name]) ON DELETE CASCADE ON UPDATE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[Usersetdefinitions]'
GO
ALTER TABLE [dbo].[Usersetdefinitions] WITH NOCHECK  ADD CONSTRAINT [FK_Usersetdefinitions_Subverses] FOREIGN KEY ([Subversename]) REFERENCES [dbo].[Subverses] ([name]) ON DELETE CASCADE ON UPDATE CASCADE
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[Viewstatistics]'
GO
ALTER TABLE [dbo].[Viewstatistics] WITH NOCHECK  ADD CONSTRAINT [FK_Viewstatistics_Messages] FOREIGN KEY ([submissionId]) REFERENCES [dbo].[Messages] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
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
