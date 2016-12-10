#region LICENSE

/*

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All portions of the code written by Voat, Inc. are Copyright(c) Voat, Inc.

    All Rights Reserved.

*/

#endregion LICENSE

using Microsoft.AspNet.Identity;

using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using Voat.Data.Models;
using Voat.Tests.Data;
using Voat.Utilities;

namespace Voat.Tests.Repository
{
    public class VoatDataInitializer : DropCreateDatabaseAlways<voatEntities>
    {

        public override void InitializeDatabase(voatEntities context)
        {
            base.InitializeDatabase(context);
        }

        protected override void Seed(voatEntities context)
        {
            CreateUserSchema(context);
            
            //*******************************************************************************************************
            //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

            /*
             *
             *
             *                      DO NOT EDIT EXISTING SEED CODE, ALWAYS APPEND TO IT.
             *
             *      EXISTING DATA BASED UNIT TESTS ARE BUILD UPON WHAT IS SPECIFIED HERE AND IF CHANGED WILL FAIL
             *
             *
            */

            //*******************************************************************************************************
            //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

            #region Subverses

            //ID:1 (Standard Subverse)
            var unitSubverse = context.Subverses.Add(new Subverse()
            {
                Name = "unit",
                Title = "v/unit",
                Description = "Unit test Subverse",
                SideBar = "For Unit Testing",
                Type = "link",
                IsAnonymized = false,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,
            });

            //ID:2 (Anon Subverse)
            var anonSubverse = context.Subverses.Add(new Subverse()
            {
                Name = "anon",
                Title = "v/anon",
                Description = "Anonymous Subverse",
                SideBar = "For Anonymous Testing",
                Type = "link",
                IsAnonymized = true,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,

            });

            //ID:4 (Min Subverse)
            var minCCPSubverse = context.Subverses.Add(new Subverse()
            {
                Name = "minCCP",
                Title = "v/minCCP",
                Description = "Min CCP for Testing",
                SideBar = "Min CCP for Testing",
                Type = "link",
                IsAnonymized = false,
                MinCCPForDownvote = 5000,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,

            });

            //ID:5 (Private Subverse)
            var privateSubverse = context.Subverses.Add(new Subverse()
            {
                Name = "private",
                Title = "v/private",
                Description = "Private for Testing",
                SideBar = "Private for Testing",
                Type = "link",
                IsAnonymized = false,
                IsAuthorizedOnly = true,
                IsPrivate = true,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,

            });

            //ID:6 (AskVoat Subverse)
            var askSubverse = context.Subverses.Add(new Subverse()
            {
                Name = "AskVoat",
                Title = "v/AskVoat",
                Description = "Ask Voat.",
                SideBar = "Ask Me",
                Type = "link",
                IsAnonymized = false,
                IsAuthorizedOnly = false,
                IsPrivate = false,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,

            });

            //ID:7 (whatever Subverse)
            var whatever = context.Subverses.Add(new Subverse()
            {
                Name = "whatever",
                Title = "v/whatever",
                Description = "What Ever",
                SideBar = "What Ever goes here",
                Type = "link",
                IsAnonymized = false,
                IsAuthorizedOnly = false,
                IsPrivate = false,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,

            });

            //ID:8 (news Subverse)
            var news = context.Subverses.Add(new Subverse()
            {
                Name = "news",
                Title = "v/news",
                Description = "News",
                SideBar = "News",
                Type = "link",
                IsAnonymized = false,
                IsAuthorizedOnly = false,
                IsPrivate = false,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,

            });

            //ID:9 (AuthorizedOnly Subverse)
            var authOnly = context.Subverses.Add(new Subverse()
            {
                Name = "AuthorizedOnly",
                Title = "v/AuthorizedOnly",
                Description = "Authorized Only",
                SideBar = "Authorized Only",
                Type = "link",
                IsAnonymized = false,
                IsAuthorizedOnly = true,
                IsPrivate = false,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,

            });

            context.SubverseModerators.Add(new SubverseModerator() { Subverse = "AuthorizedOnly", CreatedBy = "unit", CreationDate = DateTime.UtcNow, Power = 1, UserName = "unit" });
            context.SubverseModerators.Add(new SubverseModerator() { Subverse = "unit", CreatedBy = null, CreationDate = null, Power = 1, UserName = "unit" });
            context.SubverseModerators.Add(new SubverseModerator() { Subverse = "anon", CreatedBy = null, CreationDate = null, Power = 1, UserName = "anon" });

            context.SaveChanges();

            #endregion Subverses

            #region Submissions

            Comment c;

            //ID:1
            var unitSubmission = context.Submissions.Add(new Submission()
            {
                CreationDate = DateTime.UtcNow.AddHours(-12),
                Subverse = "unit",
                Title = "Favorite YouTube Video",
                Url = "https://www.youtube.com/watch?v=pnbJEg9r1o8",
                Type = 2,
                UserName = "anon"
            });
            context.SaveChanges();
            //ID: 1
            c = context.Comments.Add(new Comment()
            {
                UserName = "unit",
                Content = "This is a comment",
                CreationDate = DateTime.UtcNow,
                SubmissionID = unitSubmission.ID,
                ParentID = null
            });
            context.SaveChanges();
            Debug.Print("Comment ID: {0}", c.ID);
            //ID: 2
            c = context.Comments.Add(new Comment()
            {
                UserName = "unit",
                Content = "This is a comment",
                CreationDate = DateTime.UtcNow,
                SubmissionID = unitSubmission.ID,
                ParentID = c.ID
            });
            context.SaveChanges();
            Debug.Print("Comment ID: {0}", c.ID);

            //ID:2 (Anon Subverse submission)
            var anonSubmission = context.Submissions.Add(new Submission()
            {
                CreationDate = DateTime.UtcNow.AddHours(-36),
                Content = "Hello @tester, it's sure nice to be at /v/anon. No one knows me.",
                Subverse = anonSubverse.Name,
                Title = "First Anon Post",
                Type = 1,
                UserName = "anon",
                IsAnonymized = true
            });
            context.SaveChanges();
            //ID: 3
            c = context.Comments.Add(new Comment()
            {
                UserName = "anon",
                Content = "You can't see my name with the data repository",
                CreationDate = DateTime.UtcNow,
                SubmissionID = anonSubmission.ID,
                IsAnonymized = true,
                ParentID = null
            });
            context.SaveChanges();
            Debug.Print("Comment ID: {0}", c.ID);

            //ID: 4
            c = context.Comments.Add(new Comment()
            {
                UserName = "unit",
                Content = "You can't see my name with the data repository, right?",
                CreationDate = DateTime.UtcNow,
                SubmissionID = anonSubmission.ID,
                IsAnonymized = true,
                ParentID = null
            });
            context.SaveChanges();
            Debug.Print("Comment ID: {0}", c.ID);

            //ID:3 (MinCCP Subverse submission)
            var minCCPSubmission = context.Submissions.Add(new Submission()
            {
                CreationDate = DateTime.UtcNow,
                Content = "Hello @tester, it's sure nice to be at /v/minCCP.",
                Subverse = minCCPSubverse.Name,
                Title = "First MinCCP Post",
                Type = 1,
                UserName = "anon",
                IsAnonymized = false
            });
            context.SaveChanges();
            //ID: 5
            c = context.Comments.Add(new Comment()
            {
                UserName = "anon",
                Content = "This is a comment in v/MinCCP Sub from user anon",
                CreationDate = DateTime.UtcNow,
                SubmissionID = minCCPSubmission.ID,
                ParentID = null
            });
            context.SaveChanges();
            Debug.Print("Comment ID: {0}", c.ID);

            //ID: 6
            c = context.Comments.Add(new Comment()
            {
                UserName = "unit",
                Content = "This is a comment in v/MinCCP Sub from user unit",
                CreationDate = DateTime.UtcNow.AddHours(-4),
                SubmissionID = minCCPSubmission.ID,
                ParentID = null
            });
            context.SaveChanges();
            Debug.Print("Comment ID: {0}", c.ID);

            #endregion Submissions

            #region UserPrefernces

            context.UserPreferences.Add(new UserPreference()
            {
                UserName = "unit",
                DisableCSS = false,
                NightMode = true,
                Language = "en",
                OpenInNewWindow = true,
                EnableAdultContent = false,
                DisplayVotes = true,
                DisplaySubscriptions = false,
                UseSubscriptionsMenu = true,
                Bio = "User unit's short bio",
                Avatar = "somepath_i_think.jpg"
            });
            context.SaveChanges();

            //c = context.Comments.Add(new Comment()
            //{
            //    UserName = "DownvoteUser",
            //    Content = String.Format("Test Comment w/Upvotes"),
            //    CreationDate = DateTime.UtcNow,
            //    ID = unitSubmission.ID,
            //    UpCount = 101,
            //    ParentID = null
            //});
            context.SaveChanges();
            Debug.Print("Comment ID: {0}", c.ID);

            #endregion UserPrefernces

            #region Create Test Users

            //default users
            CreateUser("unit");
            CreateUser("anon");

            //these blocks are used for testing individual operations
            CreateUserBatch(UNIT_TEST_CONSTANTS.UNIT_TEST_USER_TEMPLATE, 0, 50);
            CreateUserBatch(UNIT_TEST_CONSTANTS.TEST_USER_TEMPLATE, 0, 15);

            //Users with varying levels of CCP
            CreateUser("User0CCP");
            CreateUser("User50CCP");
            CreateUser("User100CCP", DateTime.UtcNow.AddDays(-45));
            CreateUser("User500CCP", DateTime.UtcNow.AddDays(-60));


            var s = context.Submissions.Add(new Submission()
            {

                UserName = "User500CCP",
                Title = "Test Submission",
                Type = 1,
                Subverse = "unit",
                Content = String.Format("Test Submission w/Upvotes 50"),
                CreationDate = DateTime.UtcNow,
                //SubmissionID = unitSubmission.ID,
                UpCount = 500,
            });
            context.SaveChanges();

            c = context.Comments.Add(new Comment()
            {
                UserName = "User50CCP",
                Content = String.Format("Test Comment w/Upvotes 50"),
                CreationDate = DateTime.UtcNow,
                SubmissionID = unitSubmission.ID,
                UpCount = 50,
                ParentID = null
            });
            context.SaveChanges();

            c = context.Comments.Add(new Comment()
            {
                UserName = "User100CCP",
                Content = String.Format("Test Comment w/Upvotes 100"),
                CreationDate = DateTime.UtcNow,
                SubmissionID = unitSubmission.ID,
                UpCount = 100,
                ParentID = null
            });
            context.SaveChanges();

            c = context.Comments.Add(new Comment()
            {
                UserName = "User500CCP",
                Content = String.Format("Test Comment w/Upvotes 500"),
                CreationDate = DateTime.UtcNow,
                SubmissionID = unitSubmission.ID,
                UpCount = 500,
                ParentID = null
            });
            context.SaveChanges();

            #endregion Create Test Users

            #region Banned Test Data

            CreateUser("BannedFromVUnit");
            CreateUser("BannedGlobally");

            context.BannedUsers.Add(new BannedUser() { CreatedBy = "unit", CreationDate = DateTime.UtcNow.AddDays(-30), Reason = "Unit Testing Global Ban", UserName = "BannedGlobally" });
            context.SubverseBans.Add(new SubverseBan() { Subverse = "unit", CreatedBy = "unit", CreationDate = DateTime.UtcNow.AddDays(-30), Reason = "Unit Testing v/Unit Ban", UserName = "BannedFromVUnit" });
            context.BannedDomains.Add(new BannedDomain() { CreatedBy = "unit", CreationDate = DateTime.UtcNow.AddDays(-15), Domain = "fleddit.com", Reason = "Turned Digg migrants into jelly fish" });

            context.SaveChanges();

            #endregion BannedUsers Test Data

            #region Disabled Test Data

            context.Subverses.Add(new Subverse() { Name = "Disabled", Title = "Disabled", IsAdminDisabled = true, CreatedBy = "unit", CreationDate = DateTime.Now.AddDays(-100), SideBar = "We will never be disabled", Type = "link" });

            context.SaveChanges();

            #endregion BannedUsers Test Data

            #region AddDefaultSubs

            context.DefaultSubverses.Add(new DefaultSubverse() { Subverse = "AskVoat", Order = 1 });
            context.DefaultSubverses.Add(new DefaultSubverse() { Subverse = "whatever", Order = 2 });
            context.DefaultSubverses.Add(new DefaultSubverse() { Subverse = "news", Order = 3 });
            context.SaveChanges();

            #endregion AddDefaultSubs

            //******************************************************************************************************************
            // ADD YOUR STUFF BELOW - DO NOT EDIT THE ABOVE CODE - NOT EVEN ONCE - I'LL SO FIGHT YOU IF YOU DO AND I FIGHT DIRTY
            //******************************************************************************************************************
        }
        public static void CreateSorted(string subverse) {
            using (var context = new voatEntities())
            {
                var sortSubverse = context.Subverses.Add(new Subverse()
                {
                    Name =$"{subverse}",
                    Title = $"v/{subverse}",
                    Description = "Unit test Sort Testing",
                    SideBar = "For Sort Testing",
                    Type = "link",
                    IsAnonymized = false,
                    CreationDate = DateTime.UtcNow.AddDays(-70),
                    IsAdminDisabled = false
                });
                context.SaveChanges();

                for (int i = 0; i < 100; i++)
                {
                    var submission = new Submission();
                    bool even = i % 2 == 0;

                    submission.CreationDate = DateTime.UtcNow.AddDays(even ? i * -1 : i * -2);
                    submission.Subverse = sortSubverse.Name;
                    submission.Title = String.Format("Sort entry {0}", i);
                    submission.Content = "Sort this mang";
                    submission.Type = 1;
                    submission.UserName = "anon";
                    submission.DownCount = (even ? i : i / 2);
                    submission.UpCount = (!even ? i : i * 2);
                    submission.Views = i * i;
                    Ranking.RerankSubmission(submission);
                    context.Submissions.Add(submission);
                    context.SaveChanges();
                }
            }
        }
        public static int BuildCommentTree(string subverse, string commentContent, int rootDepth, int nestedDepth, int recurseCount)
        {
            using (var db = new voatEntities())
            {
                var s = db.Subverses.Where(x => x.Name == subverse).FirstOrDefault();


                //create submission
                var submission = new Submission()
                {
                    CreationDate = DateTime.UtcNow,
                    Content = $"Comment Tree for v/{subverse}",
                    Subverse = subverse,
                    Title = $"Comment Tree for v/{subverse}",
                    Type = 1,
                    UserName = "TestUser1",
                    IsAnonymized = s.IsAnonymized,
                };
                db.Submissions.Add(submission);
                db.SaveChanges();

                Func<voatEntities, int?, string, string, int> createComment = (context, parentCommentID, content, userName) => {

                    var comment = new Comment() {
                        SubmissionID = submission.ID,
                        UserName = userName,
                        Content = content,
                        FormattedContent = Formatting.FormatMessage(content),
                        ParentID = parentCommentID,
                        IsAnonymized = submission.IsAnonymized,
                        CreationDate = DateTime.UtcNow
                    };
                    context.Comments.Add(comment);
                    context.SaveChanges();
                    System.Threading.Thread.Sleep(2);
                    return comment.ID;
                };
                Action<voatEntities, int?, int, int, string, int> createNestedComments = null;
                createNestedComments = (context, parentCommentID, depth, currentDepth, path, recurseDepth) => {
                    if (currentDepth <= depth)
                    {
                        var newParentCommentID = parentCommentID;
                        currentDepth += 1;
                        for (int i = 0; i < depth; i++)
                        {
                            var newPath = String.Format("{0}:{1}", path, i + 1);
                            newParentCommentID = createComment(context, parentCommentID, $"{commentContent} - Path {newPath}", String.Format("CommentTreeUser{0}", i + 1));
                            if (currentDepth < recurseDepth)
                            {
                                createNestedComments(db, newParentCommentID, (depth), 0, newPath, recurseDepth - 1);
                            }
                        }
                    }
                };
                for (int i = 0; i < rootDepth; i++)
                {
                    int parentCommentID = createComment(db, null, $"{commentContent} - Path {i + 1}", String.Format("CommentTreeUser{0}", i + 1));
                    createNestedComments(db, parentCommentID, nestedDepth, 0, $"{i + 1}", recurseCount);
                }
                return submission.ID;
            }
        }
        public static void CreateUser(string userName, DateTime? registrationDate = null)
        {
            //SchemaInitializerApplicationDbContext.ReferenceEquals(null, new object());

            var manager = new UserManager<VoatUser>(new UserStore<VoatUser>(new ApplicationDbContext()));

            //if (!UserHelper.UserExists(userName))
            //{
                var user = new Voat.Data.Models.VoatUser() { UserName = userName, RegistrationDateTime = (registrationDate.HasValue ? registrationDate.Value : DateTime.UtcNow), LastLoginDateTime = new DateTime(1900, 1, 1, 0, 0, 0, 0), LastLoginFromIp = "0.0.0.0" };

                string pwd = userName;
                while (pwd.Length < 6)
                {
                    pwd += userName;
                }

                var result = manager.Create(user, pwd);

                if (!result.Succeeded)
                {
                    throw new Exception("Error creating test user " + userName);
                }
           // }
        }
        public static void CreateUserBatch(string userNameTemplate, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                CreateUser(String.Format(userNameTemplate, i.ToString()));
            }
        }
        private void CreateUserSchema(voatEntities context)
        {
            //Got sick of wasting time messing around with database initializers for
            //the Identity classes since we are using a single database for testing. Here
            //is the schema script to create all the neccesary tables for AspNet Entity Identity

            #region Create AspNet Schema

            var script = @"
                        SET NUMERIC_ROUNDABORT OFF
                        ;
                        SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
                        ;
                        SET XACT_ABORT ON
                        ;
                        SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
                        ;
                        BEGIN TRANSACTION
                        ;
                        IF @@ERROR <> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[AspNetUsers]'
                        ;
                        CREATE TABLE[dbo].[AspNetUsers]
                        (
                        [Id][nvarchar](128) COLLATE Latin1_General_CI_AS NOT NULL,
                        [UserName] [nvarchar] (800) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
                        [PasswordHash] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL,
                        [SecurityStamp]
                                [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL,
                        [Email]
                                [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL,
                        [IsConfirmed]
                                [bit]
                                NULL CONSTRAINT[DF__AspNetUse__IsCon__37A5467C] DEFAULT((0)),
                        [EmailConfirmed]
                                [bit]
                                NULL,
                        [PhoneNumber]
                                [nchar] (10) COLLATE Latin1_General_CI_AS NULL,
                        [PhoneNumberConfirmed]
                                [bit]
                                NULL,
                        [TwoFactorEnabled]
                                [bit]
                                NULL,
                        [LockoutEndDateUtc]
                                [datetime]
                                NULL,
                        [LockoutEnabled]
                                [bit]
                                NULL,
                        [AccessFailedCount]
                                [int] NULL,
                        [RegistrationDateTime]
                                [datetime]
                                NOT NULL CONSTRAINT[DF__AspNetUse__Regis__47DBAE45] DEFAULT('1900-01-01T00:00:00.000'),
                        [RecoveryQuestion]
                                [nvarchar] (50) COLLATE Latin1_General_CI_AS NULL,
                        [Answer]
                                [nvarchar] (50) COLLATE Latin1_General_CI_AS NULL,
                        [Partner]
                                [bit]
                                NOT NULL CONSTRAINT[DF__AspNetUse__Partn__74AE54BC] DEFAULT((0)),
                        [LastLoginFromIp]
                                [nvarchar] (50) COLLATE Latin1_General_CI_AS NULL,
                        [LastLoginDateTime]
                                [datetime]
                                NOT NULL CONSTRAINT[DF__AspNetUse__LastL__18EBB532] DEFAULT('1900-01-01T00:00:00.000')
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230838] on [dbo].[AspNetUsers]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230838] ON[dbo].[AspNetUsers] ([Id])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [IX_UserName] on [dbo].[AspNetUsers]'
                        ;
                        CREATE NONCLUSTERED INDEX[IX_UserName] ON[dbo].[AspNetUsers] ([UserName])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[AspNetRoles]'
                        ;
                        CREATE TABLE[dbo].[AspNetRoles]
                        (
                        [Id]
                                [nvarchar] (128) COLLATE Latin1_General_CI_AS NOT NULL,
                        [Name] [nvarchar] (max) COLLATE Latin1_General_CI_AS NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230754] on [dbo].[AspNetRoles]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230754] ON[dbo].[AspNetRoles] ([Id])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_AspNetRoles] on [dbo].[AspNetRoles]'
                        ;
                        ALTER TABLE[dbo].[AspNetRoles]
                                ADD CONSTRAINT[PK_AspNetRoles] PRIMARY KEY NONCLUSTERED([Id])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[AspNetUserClaims]'
                        ;
                        CREATE TABLE[dbo].[AspNetUserClaims]
                        (
                        [Id]
                                [int] NOT NULL,
                           [ClaimType] [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL,
                        [ClaimValue]
                                [nvarchar] (max) COLLATE Latin1_General_CI_AS NULL,
                        [UserId]
                                [nvarchar] (128) COLLATE Latin1_General_CI_AS NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230805] on [dbo].[AspNetUserClaims]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230805] ON[dbo].[AspNetUserClaims] ([Id])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_AspNetUserClaims] on [dbo].[AspNetUserClaims]'
                        ;
                        ALTER TABLE[dbo].[AspNetUserClaims]
                                ADD CONSTRAINT[PK_AspNetUserClaims] PRIMARY KEY NONCLUSTERED([Id])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[AspNetUserLogins]'
                        ;
                        CREATE TABLE[dbo].[AspNetUserLogins]
                        (
                        [UserId]
                                [nvarchar] (128) COLLATE Latin1_General_CI_AS NOT NULL,
                        [LoginProvider] [nvarchar] (128) COLLATE Latin1_General_CI_AS NOT NULL,
                        [ProviderKey] [nvarchar] (128) COLLATE Latin1_General_CI_AS NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230815] on [dbo].[AspNetUserLogins]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230815] ON[dbo].[AspNetUserLogins] ([UserId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_AspNetUserLogins] on [dbo].[AspNetUserLogins]'
                        ;
                        ALTER TABLE[dbo].[AspNetUserLogins]
                                ADD CONSTRAINT[PK_AspNetUserLogins] PRIMARY KEY NONCLUSTERED([UserId], [LoginProvider], [ProviderKey])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[AspNetUserRoles]'
                        ;
                        CREATE TABLE[dbo].[AspNetUserRoles]
                        (
                        [UserId]
                                [nvarchar] (128) COLLATE Latin1_General_CI_AS NOT NULL,
                        [RoleId] [nvarchar] (128) COLLATE Latin1_General_CI_AS NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230826] on [dbo].[AspNetUserRoles]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230826] ON[dbo].[AspNetUserRoles] ([RoleId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_AspNetUserRoles] on [dbo].[AspNetUserRoles]'
                        ;
                        ALTER TABLE[dbo].[AspNetUserRoles]
                                ADD CONSTRAINT[PK_AspNetUserRoles] PRIMARY KEY NONCLUSTERED([UserId], [RoleId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[Sessions]'
                        ;
                        CREATE TABLE[dbo].[Sessions]
                        (
                        [SessionId]
                                [nvarchar] (88) COLLATE Latin1_General_CI_AS NOT NULL,
                        [Created] [datetime]
                                NOT NULL,
                        [Expires] [datetime]
                                NOT NULL,
                        [LockDate] [datetime]
                                NOT NULL,
                        [LockCookie] [int] NOT NULL,
                        [Locked] [bit]
                                NOT NULL,
                        [SessionItem] [image]
                                NULL,
                        [Flags]
                                [int] NOT NULL,
                        [Timeout] [int] NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230903] on [dbo].[Sessions]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230903] ON[dbo].[Sessions] ([SessionId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[__MigrationHistory]'
                        ;
                        CREATE TABLE[dbo].[__MigrationHistory]
                        (
                        [MigrationId]
                                [nvarchar] (150) COLLATE Latin1_General_CI_AS NOT NULL,
                        [ContextKey] [nvarchar] (300) COLLATE Latin1_General_CI_AS NOT NULL,
                        [Model] [varbinary] (max) NOT NULL,
                        [ProductVersion] [nvarchar] (32) COLLATE Latin1_General_CI_AS NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230514] on [dbo].[__MigrationHistory]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230514] ON[dbo].[__MigrationHistory] ([MigrationId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK___MigrationHistory] on [dbo].[__MigrationHistory]'
                        ;
                        ALTER TABLE[dbo].[__MigrationHistory]
                                ADD CONSTRAINT[PK___MigrationHistory] PRIMARY KEY NONCLUSTERED([MigrationId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[aspnet_Applications]'
                        ;
                        CREATE TABLE[dbo].[aspnet_Applications]
                        (
                        [ApplicationName]
                                [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [LoweredApplicationName] [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [ApplicationId] [uniqueidentifier]
                                NOT NULL,
                        [Description] [nvarchar] (256) COLLATE Latin1_General_CI_AS NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230529] on [dbo].[aspnet_Applications]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230529] ON[dbo].[aspnet_Applications] ([ApplicationId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_aspnet_Applications] on [dbo].[aspnet_Applications]'
                        ;
                        ALTER TABLE[dbo].[aspnet_Applications]
                                ADD CONSTRAINT[PK_aspnet_Applications] PRIMARY KEY NONCLUSTERED([ApplicationId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[aspnet_Membership]'
                        ;
                        CREATE TABLE[dbo].[aspnet_Membership]
                        (
                        [ApplicationId]
                                [uniqueidentifier]
                                NOT NULL,
                        [UserId] [uniqueidentifier]
                                NOT NULL,
                        [Password] [nvarchar] (128) COLLATE Latin1_General_CI_AS NOT NULL,
                        [PasswordFormat] [int] NOT NULL,
                        [PasswordSalt] [nvarchar] (128) COLLATE Latin1_General_CI_AS NOT NULL,
                        [MobilePIN] [nvarchar] (16) COLLATE Latin1_General_CI_AS NULL,
                        [Email]
                                [nvarchar] (256) COLLATE Latin1_General_CI_AS NULL,
                        [LoweredEmail]
                                [nvarchar] (256) COLLATE Latin1_General_CI_AS NULL,
                        [PasswordQuestion]
                                [nvarchar] (256) COLLATE Latin1_General_CI_AS NULL,
                        [PasswordAnswer]
                                [nvarchar] (128) COLLATE Latin1_General_CI_AS NULL,
                        [IsApproved]
                                [bit]
                                NOT NULL,
                        [IsLockedOut] [bit]
                                NOT NULL,
                        [CreateDate] [datetime]
                                NOT NULL,
                        [LastLoginDate] [datetime]
                                NOT NULL,
                        [LastPasswordChangedDate] [datetime]
                                NOT NULL,
                        [LastLockoutDate] [datetime]
                                NOT NULL,
                        [FailedPasswordAttemptCount] [int] NOT NULL,
                        [FailedPasswordAttemptWindowStart] [datetime]
                                NOT NULL,
                        [FailedPasswordAnswerAttemptCount] [int] NOT NULL,
                        [FailedPasswordAnswerAttemptWindowStart] [datetime]
                                NOT NULL,
                        [Comment] [ntext]
                                COLLATE Latin1_General_CI_AS NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230542] on [dbo].[aspnet_Membership]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230542] ON[dbo].[aspnet_Membership] ([ApplicationId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_aspnet_Membership] on [dbo].[aspnet_Membership]'
                        ;
                        ALTER TABLE[dbo].[aspnet_Membership]
                                ADD CONSTRAINT[PK_aspnet_Membership] PRIMARY KEY NONCLUSTERED([ApplicationId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[aspnet_Paths]'
                        ;
                        CREATE TABLE[dbo].[aspnet_Paths]
                        (
                        [ApplicationId]
                                [uniqueidentifier]
                                NOT NULL,
                        [PathId] [uniqueidentifier]
                                NOT NULL,
                        [Path] [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [LoweredPath] [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230607] on [dbo].[aspnet_Paths]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230607] ON[dbo].[aspnet_Paths] ([PathId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_aspnet_Paths] on [dbo].[aspnet_Paths]'
                        ;
                        ALTER TABLE[dbo].[aspnet_Paths]
                                ADD CONSTRAINT[PK_aspnet_Paths] PRIMARY KEY NONCLUSTERED([PathId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[aspnet_PersonalizationAllUsers]'
                        ;
                        CREATE TABLE[dbo].[aspnet_PersonalizationAllUsers]
                        (
                        [PathId]
                                [uniqueidentifier]
                                NOT NULL,
                        [PageSettings] [image]
                                NOT NULL,
                        [LastUpdatedDate] [datetime]
                                NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230616] on [dbo].[aspnet_PersonalizationAllUsers]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230616] ON[dbo].[aspnet_PersonalizationAllUsers] ([PathId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_aspnet_PersonalizationAllUsers] on [dbo].[aspnet_PersonalizationAllUsers]'
                        ;
                        ALTER TABLE[dbo].[aspnet_PersonalizationAllUsers]
                                ADD CONSTRAINT[PK_aspnet_PersonalizationAllUsers] PRIMARY KEY NONCLUSTERED([PathId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[aspnet_PersonalizationPerUser]'
                        ;
                        CREATE TABLE[dbo].[aspnet_PersonalizationPerUser]
                        (
                        [Id]
                                [uniqueidentifier]
                                NOT NULL,
                        [PathId] [uniqueidentifier]
                                NULL,
                        [UserId]
                                [uniqueidentifier]
                                NULL,
                        [PageSettings]
                                [image]
                                NOT NULL,
                        [LastUpdatedDate] [datetime]
                                NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230628] on [dbo].[aspnet_PersonalizationPerUser]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230628] ON[dbo].[aspnet_PersonalizationPerUser] ([Id])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_aspnet_PersonalizationPerUser] on [dbo].[aspnet_PersonalizationPerUser]'
                        ;
                        ALTER TABLE[dbo].[aspnet_PersonalizationPerUser]
                                ADD CONSTRAINT[PK_aspnet_PersonalizationPerUser] PRIMARY KEY NONCLUSTERED([Id])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[aspnet_Profile]'
                        ;
                        CREATE TABLE[dbo].[aspnet_Profile]
                        (
                        [UserId]
                                [uniqueidentifier]
                                NOT NULL,
                        [PropertyNames] [ntext]
                                COLLATE Latin1_General_CI_AS NOT NULL,
                        [PropertyValuesString] [ntext]
                                COLLATE Latin1_General_CI_AS NOT NULL,
                        [PropertyValuesBinary] [image]
                                NOT NULL,
                        [LastUpdatedDate] [datetime]
                                NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230638] on [dbo].[aspnet_Profile]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230638] ON[dbo].[aspnet_Profile] ([UserId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_aspnet_Profile] on [dbo].[aspnet_Profile]'
                        ;
                        ALTER TABLE[dbo].[aspnet_Profile]
                                ADD CONSTRAINT[PK_aspnet_Profile] PRIMARY KEY NONCLUSTERED([UserId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[aspnet_Roles]'
                        ;
                        CREATE TABLE[dbo].[aspnet_Roles]
                        (
                        [ApplicationId]
                                [uniqueidentifier]
                                NOT NULL,
                        [RoleId] [uniqueidentifier]
                                NOT NULL,
                        [RoleName] [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [LoweredRoleName] [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [Description] [nvarchar] (256) COLLATE Latin1_General_CI_AS NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230647] on [dbo].[aspnet_Roles]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230647] ON[dbo].[aspnet_Roles] ([RoleId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_aspnet_Roles] on [dbo].[aspnet_Roles]'
                        ;
                        ALTER TABLE[dbo].[aspnet_Roles]
                                ADD CONSTRAINT[PK_aspnet_Roles] PRIMARY KEY NONCLUSTERED([RoleId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[aspnet_SchemaVersions]'
                        ;
                        CREATE TABLE[dbo].[aspnet_SchemaVersions]
                        (
                        [Feature]
                                [nvarchar] (128) COLLATE Latin1_General_CI_AS NOT NULL,
                        [CompatibleSchemaVersion] [nvarchar] (128) COLLATE Latin1_General_CI_AS NOT NULL,
                        [IsCurrentVersion] [bit]
                                NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230658] on [dbo].[aspnet_SchemaVersions]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230658] ON[dbo].[aspnet_SchemaVersions] ([IsCurrentVersion])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_aspnet_SchemaVersions] on [dbo].[aspnet_SchemaVersions]'
                        ;
                        ALTER TABLE[dbo].[aspnet_SchemaVersions]
                                ADD CONSTRAINT[PK_aspnet_SchemaVersions] PRIMARY KEY NONCLUSTERED([Feature], [CompatibleSchemaVersion])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[aspnet_UsersInRoles]'
                        ;
                        CREATE TABLE[dbo].[aspnet_UsersInRoles]
                        (
                        [UserId]
                                [uniqueidentifier]
                                NOT NULL,
                        [RoleId] [uniqueidentifier]
                                NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230730] on [dbo].[aspnet_UsersInRoles]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230730] ON[dbo].[aspnet_UsersInRoles] ([RoleId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_aspnet_UsersInRoles] on [dbo].[aspnet_UsersInRoles]'
                        ;
                        ALTER TABLE[dbo].[aspnet_UsersInRoles]
                                ADD CONSTRAINT[PK_aspnet_UsersInRoles] PRIMARY KEY NONCLUSTERED([RoleId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[aspnet_Users]'
                        ;
                        CREATE TABLE[dbo].[aspnet_Users]
                        (
                        [ApplicationId]
                                [uniqueidentifier]
                                NOT NULL,
                        [UserId] [uniqueidentifier]
                                NOT NULL,
                        [UserName] [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [LoweredUserName] [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [MobileAlias] [nvarchar] (16) COLLATE Latin1_General_CI_AS NULL,
                        [IsAnonymous]
                                [bit]
                                NOT NULL,
                        [LastActivityDate] [datetime]
                                NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230719] on [dbo].[aspnet_Users]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230719] ON[dbo].[aspnet_Users] ([UserId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_aspnet_Users] on [dbo].[aspnet_Users]'
                        ;
                        ALTER TABLE[dbo].[aspnet_Users]
                                ADD CONSTRAINT[PK_aspnet_Users] PRIMARY KEY NONCLUSTERED([UserId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[aspnet_WebEvent_Events]'
                        ;
                        CREATE TABLE[dbo].[aspnet_WebEvent_Events]
                        (
                        [EventId]
                                [char] (32) COLLATE Latin1_General_CI_AS NOT NULL,
                        [EventTimeUtc] [datetime]
                                NOT NULL,
                        [EventTime] [datetime]
                                NOT NULL,
                        [EventType] [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [EventSequence] [decimal] (19, 0) NOT NULL,
                        [EventOccurrence] [decimal] (19, 0) NOT NULL,
                        [EventCode] [int] NOT NULL,
                        [EventDetailCode] [int] NOT NULL,
                        [Message] [nvarchar] (1024) COLLATE Latin1_General_CI_AS NULL,
                        [ApplicationPath]
                                [nvarchar] (256) COLLATE Latin1_General_CI_AS NULL,
                        [ApplicationVirtualPath]
                                [nvarchar] (256) COLLATE Latin1_General_CI_AS NULL,
                        [MachineName]
                                [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [RequestUrl] [nvarchar] (1024) COLLATE Latin1_General_CI_AS NULL,
                        [ExceptionType]
                                [nvarchar] (256) COLLATE Latin1_General_CI_AS NULL,
                        [Details]
                                [ntext]
                                COLLATE Latin1_General_CI_AS NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230743] on [dbo].[aspnet_WebEvent_Events]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230743] ON[dbo].[aspnet_WebEvent_Events] ([EventId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_aspnet_WebEvent_Events] on [dbo].[aspnet_WebEvent_Events]'
                        ;
                        ALTER TABLE[dbo].[aspnet_WebEvent_Events]
                                ADD CONSTRAINT[PK_aspnet_WebEvent_Events] PRIMARY KEY NONCLUSTERED([EventId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[vw_aspnet_Applications]'
                        ;
                        CREATE TABLE[dbo].[vw_aspnet_Applications]
                        (
                        [ApplicationName]
                                [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [LoweredApplicationName] [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [ApplicationId] [uniqueidentifier]
                                NOT NULL,
                        [Description] [nvarchar] (256) COLLATE Latin1_General_CI_AS NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230915] on [dbo].[vw_aspnet_Applications]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230915] ON[dbo].[vw_aspnet_Applications] ([ApplicationId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_vw_aspnet_Applications] on [dbo].[vw_aspnet_Applications]'
                        ;
                        ALTER TABLE[dbo].[vw_aspnet_Applications]
                                ADD CONSTRAINT[PK_vw_aspnet_Applications] PRIMARY KEY NONCLUSTERED([ApplicationId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[vw_aspnet_MembershipUsers]'
                        ;
                        CREATE TABLE[dbo].[vw_aspnet_MembershipUsers]
                        (
                        [UserId]
                                [uniqueidentifier]
                                NOT NULL,
                        [PasswordFormat] [int] NOT NULL,
                        [MobilePIN] [nvarchar] (16) COLLATE Latin1_General_CI_AS NULL,
                        [Email]
                                [nvarchar] (256) COLLATE Latin1_General_CI_AS NULL,
                        [LoweredEmail]
                                [nvarchar] (256) COLLATE Latin1_General_CI_AS NULL,
                        [PasswordQuestion]
                                [nvarchar] (256) COLLATE Latin1_General_CI_AS NULL,
                        [PasswordAnswer]
                                [nvarchar] (128) COLLATE Latin1_General_CI_AS NULL,
                        [IsApproved]
                                [bit]
                                NOT NULL,
                        [IsLockedOut] [bit]
                                NOT NULL,
                        [CreateDate] [datetime]
                                NOT NULL,
                        [LastLoginDate] [datetime]
                                NOT NULL,
                        [LastPasswordChangedDate] [datetime]
                                NOT NULL,
                        [LastLockoutDate] [datetime]
                                NOT NULL,
                        [FailedPasswordAttemptCount] [int] NOT NULL,
                        [FailedPasswordAttemptWindowStart] [datetime]
                                NOT NULL,
                        [FailedPasswordAnswerAttemptCount] [int] NOT NULL,
                        [FailedPasswordAnswerAttemptWindowStart] [datetime]
                                NOT NULL,
                        [Comment] [ntext]
                                COLLATE Latin1_General_CI_AS NULL,
                        [ApplicationId]
                                [uniqueidentifier]
                                NOT NULL,
                        [UserName] [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [MobileAlias] [nvarchar] (16) COLLATE Latin1_General_CI_AS NULL,
                        [IsAnonymous]
                                [bit]
                                NOT NULL,
                        [LastActivityDate] [datetime]
                                NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230926] on [dbo].[vw_aspnet_MembershipUsers]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230926] ON[dbo].[vw_aspnet_MembershipUsers] ([UserId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_vw_aspnet_MembershipUsers] on [dbo].[vw_aspnet_MembershipUsers]'
                        ;
                        ALTER TABLE[dbo].[vw_aspnet_MembershipUsers]
                                ADD CONSTRAINT[PK_vw_aspnet_MembershipUsers] PRIMARY KEY NONCLUSTERED([UserId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[vw_aspnet_Profiles]'
                        ;
                        CREATE TABLE[dbo].[vw_aspnet_Profiles]
                        (
                        [UserId]
                                [uniqueidentifier]
                                NOT NULL,
                        [LastUpdatedDate] [datetime]
                                NOT NULL,
                        [DataSize] [int] NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230937] on [dbo].[vw_aspnet_Profiles]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230937] ON[dbo].[vw_aspnet_Profiles] ([UserId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_vw_aspnet_Profiles] on [dbo].[vw_aspnet_Profiles]'
                        ;
                        ALTER TABLE[dbo].[vw_aspnet_Profiles]
                                ADD CONSTRAINT[PK_vw_aspnet_Profiles] PRIMARY KEY NONCLUSTERED([UserId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[vw_aspnet_Roles]'
                        ;
                        CREATE TABLE[dbo].[vw_aspnet_Roles]
                        (
                        [ApplicationId]
                                [uniqueidentifier]
                                NOT NULL,
                        [RoleId] [uniqueidentifier]
                                NOT NULL,
                        [RoleName] [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [LoweredRoleName] [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [Description] [nvarchar] (256) COLLATE Latin1_General_CI_AS NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230946] on [dbo].[vw_aspnet_Roles]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230946] ON[dbo].[vw_aspnet_Roles] ([RoleId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_vw_aspnet_Roles] on [dbo].[vw_aspnet_Roles]'
                        ;
                        ALTER TABLE[dbo].[vw_aspnet_Roles]
                                ADD CONSTRAINT[PK_vw_aspnet_Roles] PRIMARY KEY NONCLUSTERED([ApplicationId], [RoleId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[vw_aspnet_UsersInRoles]'
                        ;
                        CREATE TABLE[dbo].[vw_aspnet_UsersInRoles]
                        (
                        [UserId]
                                [uniqueidentifier]
                                NOT NULL,
                        [RoleId] [uniqueidentifier]
                                NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-231004] on [dbo].[vw_aspnet_UsersInRoles]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 231004] ON[dbo].[vw_aspnet_UsersInRoles] ([RoleId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_vw_aspnet_UsersInRoles] on [dbo].[vw_aspnet_UsersInRoles]'
                        ;
                        ALTER TABLE[dbo].[vw_aspnet_UsersInRoles]
                                ADD CONSTRAINT[PK_vw_aspnet_UsersInRoles] PRIMARY KEY NONCLUSTERED([UserId], [RoleId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[vw_aspnet_Users]'
                        ;
                        CREATE TABLE[dbo].[vw_aspnet_Users]
                        (
                        [ApplicationId]
                                [uniqueidentifier]
                                NOT NULL,
                        [UserId] [uniqueidentifier]
                                NOT NULL,
                        [UserName] [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [LoweredUserName] [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [MobileAlias] [nvarchar] (16) COLLATE Latin1_General_CI_AS NULL,
                        [IsAnonymous]
                                [bit]
                                NOT NULL,
                        [LastActivityDate] [datetime]
                                NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-230955] on [dbo].[vw_aspnet_Users]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 230955] ON[dbo].[vw_aspnet_Users] ([UserId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[vw_aspnet_WebPartState_Paths]'
                        ;
                        CREATE TABLE[dbo].[vw_aspnet_WebPartState_Paths]
                        (
                        [ApplicationId]
                                [uniqueidentifier]
                                NOT NULL,
                        [PathId] [uniqueidentifier]
                                NOT NULL,
                        [Path] [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL,
                        [LoweredPath] [nvarchar] (256) COLLATE Latin1_General_CI_AS NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-231014] on [dbo].[vw_aspnet_WebPartState_Paths]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 231014] ON[dbo].[vw_aspnet_WebPartState_Paths] ([PathId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_vw_aspnet_WebPartState_Paths] on [dbo].[vw_aspnet_WebPartState_Paths]'
                        ;
                        ALTER TABLE[dbo].[vw_aspnet_WebPartState_Paths]
                                ADD CONSTRAINT[PK_vw_aspnet_WebPartState_Paths] PRIMARY KEY NONCLUSTERED([ApplicationId], [PathId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[vw_aspnet_WebPartState_Shared]'
                        ;
                        CREATE TABLE[dbo].[vw_aspnet_WebPartState_Shared]
                        (
                        [PathId]
                                [uniqueidentifier]
                                NOT NULL,
                        [DataSize] [int] NULL,
                        [LastUpdatedDate]
                                [datetime]
                                NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-231023] on [dbo].[vw_aspnet_WebPartState_Shared]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 231023] ON[dbo].[vw_aspnet_WebPartState_Shared] ([PathId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_vw_aspnet_WebPartState_Shared] on [dbo].[vw_aspnet_WebPartState_Shared]'
                        ;
                        ALTER TABLE[dbo].[vw_aspnet_WebPartState_Shared]
                                ADD CONSTRAINT[PK_vw_aspnet_WebPartState_Shared] PRIMARY KEY NONCLUSTERED([PathId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating [dbo].[vw_aspnet_WebPartState_User]'
                        ;
                        CREATE TABLE[dbo].[vw_aspnet_WebPartState_User]
                        (
                        [PathId]
                                [uniqueidentifier]
                                NULL,
                        [UserId]
                                [uniqueidentifier]
                                NOT NULL,
                        [DataSize] [int] NULL,
                        [LastUpdatedDate]
                                [datetime]
                                NOT NULL
                        )
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating index [ClusteredIndex-20141217-231032] on [dbo].[vw_aspnet_WebPartState_User]'
                        ;
                        CREATE CLUSTERED INDEX[ClusteredIndex - 20141217 - 231032] ON[dbo].[vw_aspnet_WebPartState_User] ([UserId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        PRINT N'Creating primary key [PK_vw_aspnet_WebPartState_User] on [dbo].[vw_aspnet_WebPartState_User]'
                        ;
                        ALTER TABLE[dbo].[vw_aspnet_WebPartState_User]
                                ADD CONSTRAINT[PK_vw_aspnet_WebPartState_User] PRIMARY KEY NONCLUSTERED([UserId])
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        COMMIT TRANSACTION
                        ;
                        IF @@ERROR<> 0 SET NOEXEC ON
                        ;
                        DECLARE @Success AS BIT
                        SET @Success = 1
                        SET NOEXEC OFF
                        IF(@Success = 1) PRINT 'The database update succeeded'
                        ELSE BEGIN
                            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
                            PRINT 'The database update failed'
                        END
                        ;";
            context.Database.ExecuteSqlCommand(script);

            #endregion Create AspNet Schema

            //Create Needed Procs

            #region Create usp_CommentTree

            var proc = @"CREATE PROC dbo.usp_CommentTree
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
                        */";
            context.Database.ExecuteSqlCommand(proc);

            #endregion Create usp_CommentTree
        }
    }

    public class VoatUsersInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        public override void InitializeDatabase(ApplicationDbContext context)
        {
            //context.Database.Create();
            //base.InitializeDatabase(context);
        }
    }
}
