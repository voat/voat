#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE


using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Voat.Caching;
using Voat.Common;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Tests.Data;
using Voat.Utilities;

namespace Voat.Tests.Infrastructure
{
    public class TestDataInitializer 
    {

        public virtual void InitializeDatabase(VoatDataContext context, bool seed = true)
        {
            CreateSchema(context);
            if (seed)
            {
                Seed(context);
            }
        }

        protected virtual void CreateSchema(VoatDataContext context)
        {
            //Parse and run sql scripts
            var connection = context.Connection;
            var dbName = connection.Database;
            var originalConnectionString = connection.ConnectionString;
        
            try
            {

                var builder = new DbConnectionStringBuilder();
                builder.ConnectionString = originalConnectionString;
                builder["database"] = DataConfigurationSettings.Instance.StoreType == DataStoreType.SqlServer ? "master" : "postgres";
                builder["Application Name"] = "Voat Unit Tests";
                connection.ConnectionString = builder.ConnectionString;

                var cmd = connection.CreateCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                if (cmd.Connection.State != System.Data.ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }

                try
                {
                    //Kill connections
                    cmd.CommandText = DataConfigurationSettings.Instance.StoreType == DataStoreType.SqlServer ?
                                           $"ALTER DATABASE {dbName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE" :
                                           $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE pid <> pg_backend_pid() AND datname = '{dbName}'";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }


                cmd.CommandText = DataConfigurationSettings.Instance.StoreType == DataStoreType.SqlServer ?
                                        $"IF EXISTS (SELECT name FROM sys.databases WHERE name = '{dbName}') DROP DATABASE {dbName}" :
                                        $"DROP DATABASE IF EXISTS {dbName}";
                cmd.ExecuteNonQuery();

                cmd.CommandText = $"CREATE DATABASE {dbName}";
                cmd.ExecuteNonQuery();

                connection.ChangeDatabase(dbName);

                //Run Scripts in repo folder
                var sqlFolderPathPublicRepo = TestEnvironmentSettings.SqlScriptRelativePath;

                var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var scriptFolder = Path.GetFullPath(Path.Combine(dir, sqlFolderPathPublicRepo));

                var scriptFiles = new string[] {
                    Path.Combine(scriptFolder, "voat.sql"),
                    Path.Combine(scriptFolder, "voat_users.sql"),
                    Path.Combine(scriptFolder, "procedures.sql"),
                    Path.Combine(scriptFolder, "data.sql")
                };

                foreach (var scriptFile in scriptFiles)
                {
                    if (!File.Exists(scriptFile))
                    {
                        throw new InvalidOperationException($"Setup can not find script '{scriptFile}'");
                    }

                    using (var sr = new StreamReader(scriptFile))
                    {
                        var contents = sr.ReadToEnd();

                        switch (DataConfigurationSettings.Instance.StoreType)
                        {
                            case DataStoreType.PostgreSql:
                            case DataStoreType.SqlServer:
                                var segments = contents.Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var batch in segments)
                                {
                                    cmd.CommandText = batch.Replace("{dbName}", dbName);
                                    cmd.ExecuteNonQuery();
                                }
                                break;
                        }


                    }
                }
            }
            finally
            {
                //revert connection 
                if (connection.State != System.Data.ConnectionState.Closed)
                {
                    connection.Close();
                }
                connection.ConnectionString = originalConnectionString;
            }
        }


        protected virtual void Seed(VoatDataContext context)
        {
            //*******************************************************************************************************
            //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

            /*
             *
             *
             *                      DO NOT EDIT EXISTING SEED CODE, ALWAYS APPEND TO IT.
             *
             *      EXISTING DATA BASED UNIT TESTS ARE BUILT UPON WHAT IS SPECIFIED HERE AND IF CHANGED WILL FAIL
             *
             *        UPON SECOND THOUGHT THIS SOUNDS WRONG BUT THIS ALSO SOUNDS LIKE A FUTURE PERSON PROBLEM
             *        
            */

            //*******************************************************************************************************
            //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

            #region Subverses

            //ID:1 (Standard Subverse)
            var unitSubverse = context.Subverse.Add(new Subverse()
            {
                Name = SUBVERSES.Unit,
                Title = "v/unit",
                Description = "Unit test Subverse",
                SideBar = "For Unit Testing",
                //Type = "link",
                IsAnonymized = false,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,
            }).Entity;

            //ID:2 (Anon Subverse)
            var anonSubverse = context.Subverse.Add(new Subverse()
            {
                Name = SUBVERSES.Anon,
                Title = "v/anon",
                Description = "Anonymous Subverse",
                SideBar = "For Anonymous Testing",
               // Type = "link",
                IsAnonymized = true,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,

            }).Entity;

            //ID:4 (Min Subverse)
            var minCCPSubverse = context.Subverse.Add(new Subverse()
            {
                Name = SUBVERSES.MinCCP,
                Title = "v/minCCP",
                Description = "Min CCP for Testing",
                SideBar = "Min CCP for Testing",
                //Type = "link",
                IsAnonymized = false,
                MinCCPForDownvote = 5000,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,

            }).Entity;

            //ID:5 (Private Subverse)
            var privateSubverse = context.Subverse.Add(new Subverse()
            {
                Name = SUBVERSES.Private,
                Title = "v/private",
                Description = "Private for Testing",
                SideBar = "Private for Testing",
                //Type = "link",
                IsAnonymized = false,
                IsAuthorizedOnly = false,
                IsPrivate = true,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,

            }).Entity;

            //ID:6 (AskVoat Subverse)
            var askSubverse = context.Subverse.Add(new Subverse()
            {
                Name = SUBVERSES.AskVoat,
                Title = "v/AskVoat",
                Description = "Ask Voat.",
                SideBar = "Ask Me",
                //Type = "link",
                IsAnonymized = false,
                IsAuthorizedOnly = false,
                IsPrivate = false,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,

            }).Entity;

            //ID:7 (whatever Subverse)
            var whatever = context.Subverse.Add(new Subverse()
            {
                Name = SUBVERSES.Whatever,
                Title = "v/whatever",
                Description = "What Ever",
                SideBar = "What Ever goes here",
               // Type = "link",
                IsAnonymized = false,
                IsAuthorizedOnly = false,
                IsPrivate = false,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,

            }).Entity;

            //ID:8 (news Subverse)
            var news = context.Subverse.Add(new Subverse()
            {
                Name = SUBVERSES.News,
                Title = "v/news",
                Description = "News",
                SideBar = "News",
                //Type = "link",
                IsAnonymized = false,
                IsAuthorizedOnly = false,
                IsPrivate = false,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,

            }).Entity;

            //ID:9 (AuthorizedOnly Subverse)
            var authOnly = context.Subverse.Add(new Subverse()
            {
                Name = SUBVERSES.AuthorizedOnly,
                Title = "v/AuthorizedOnly",
                Description = "Authorized Only",
                SideBar = "Authorized Only",
                //Type = "link",
                IsAnonymized = false,
                IsAuthorizedOnly = true,
                IsPrivate = false,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,

            }).Entity;

            //ID:10 (nsfw Subverse)
            var nsfwOnly = context.Subverse.Add(new Subverse()
            {
                Name = SUBVERSES.NSFW,
                Title = "v/NSFW",
                Description = "NSFW Only",
                SideBar = "NSFW Only",
                //Type = "link",
                IsAdult = true,
                IsAnonymized = false,
                IsAuthorizedOnly = false,
                IsPrivate = false,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,
            }).Entity;

            //ID:11 (allowAnon Subverse)
            var allowAnon = context.Subverse.Add(new Subverse()
            {
                Name = SUBVERSES.AllowAnon,
                Title = "v/AllowAnon",
                Description = "AllowAnon",
                SideBar = "AllowAnon",
                //Type = "link",
                IsAdult = false,
                IsAnonymized = null, //allows users to submit anon/non-anon content
                IsAuthorizedOnly = false,
                IsPrivate = false,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,
            }).Entity;

            var arst = context.Subverse.Add(new Subverse()
            {
                Name = "arst",
                Title = "v/arst",
                Description = "Colemak short hand sub arst",
                SideBar = "For those who are properly fingering: arst",
                //Type = "link",
                IsAdult = false,
                IsAnonymized = null, //allows users to submit anon/non-anon content
                IsAuthorizedOnly = false,
                IsPrivate = false,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,
                CreatedBy = "SurelyPutts"
            }).Entity;

            var asdf = context.Subverse.Add(new Subverse()
            {
                Name = "asdf",
                Title = "v/asdf",
                Description = "Qwerty short hand sub asdf",
                SideBar = "For those who are improperly fingering: asdf",
                //Type = "link",
                IsAdult = false,
                IsAnonymized = null, //allows users to submit anon/non-anon content
                IsAuthorizedOnly = false,
                IsPrivate = false,
                CreationDate = DateTime.UtcNow.AddDays(-7),
                IsAdminDisabled = false,
                CreatedBy = "NotPutts"
            }).Entity;

            context.SaveChanges();

            context.SubverseModerator.Add(new SubverseModerator() { Subverse = SUBVERSES.AuthorizedOnly, CreatedBy = USERNAMES.Unit, CreationDate = DateTime.UtcNow, Power = 1, UserName = USERNAMES.Unit });
            context.SubverseModerator.Add(new SubverseModerator() { Subverse = SUBVERSES.Unit, CreatedBy = null, CreationDate = null, Power = 1, UserName = USERNAMES.Unit });
            context.SubverseModerator.Add(new SubverseModerator() { Subverse = SUBVERSES.Anon, CreatedBy = null, CreationDate = null, Power = 1, UserName = USERNAMES.Anon });

            context.SaveChanges();

            #endregion Subverses

            #region Submissions

            Comment c;

            //ID:1
            var unitSubmission = context.Submission.Add(new Submission()
            {
                CreationDate = DateTime.UtcNow.AddHours(-12),
                Subverse = SUBVERSES.Unit,
                Title = "Favorite YouTube Video",
                Url = "https://www.youtube.com/watch?v=pnbJEg9r1o8",
                Type = 2,
                UserName = USERNAMES.Anon
            }).Entity;
            context.SaveChanges();
            //ID: 1
            c = context.Comment.Add(new Comment()
            {
                UserName = USERNAMES.Unit,
                Content = "This is a comment",
                CreationDate = DateTime.UtcNow,
                SubmissionID = unitSubmission.ID,
                ParentID = null
            }).Entity;
            context.SaveChanges();
            Debug.WriteLine("Comment ID: {0}", c.ID);
            //ID: 2
            c = context.Comment.Add(new Comment()
            {
                UserName = USERNAMES.Unit,
                Content = "This is a comment",
                CreationDate = DateTime.UtcNow,
                SubmissionID = unitSubmission.ID,
                ParentID = c.ID
            }).Entity;
            context.SaveChanges();
            Debug.WriteLine("Comment ID: {0}", c.ID);

            //ID:2 (Anon Subverse submission)
            var anonSubmission = context.Submission.Add(new Submission()
            {
                CreationDate = DateTime.UtcNow.AddHours(-36),
                Content = "Hello @tester, it's sure nice to be at /v/anon. No one knows me.",
                Subverse = anonSubverse.Name,
                Title = "First Anon Post",
                Type = 1,
                UserName = USERNAMES.Anon,
                IsAnonymized = true
            }).Entity;
            context.SaveChanges();
            //ID: 3
            c = context.Comment.Add(new Comment()
            {
                UserName = USERNAMES.Anon,
                Content = "You can't see my name with the data repository",
                CreationDate = DateTime.UtcNow,
                SubmissionID = anonSubmission.ID,
                IsAnonymized = true,
                ParentID = null
            }).Entity;
            context.SaveChanges();
            Debug.WriteLine("Comment ID: {0}", c.ID);

            //ID: 4
            c = context.Comment.Add(new Comment()
            {
                UserName = USERNAMES.Unit,
                Content = "You can't see my name with the data repository, right?",
                CreationDate = DateTime.UtcNow,
                SubmissionID = anonSubmission.ID,
                IsAnonymized = true,
                ParentID = null
            }).Entity;
            context.SaveChanges();
            Debug.WriteLine("Comment ID: {0}", c.ID);

            //ID:3 (MinCCP Subverse submission)
            var minCCPSubmission = context.Submission.Add(new Submission()
            {
                CreationDate = DateTime.UtcNow,
                Content = "Hello @tester, it's sure nice to be at /v/minCCP.",
                Subverse = minCCPSubverse.Name,
                Title = "First MinCCP Post",
                Type = 1,
                UserName = USERNAMES.Anon,
                IsAnonymized = false
            }).Entity;
            context.SaveChanges();
            //ID: 5
            c = context.Comment.Add(new Comment()
            {
                UserName = USERNAMES.Anon,
                Content = "This is a comment in v/MinCCP Sub from user anon",
                CreationDate = DateTime.UtcNow,
                SubmissionID = minCCPSubmission.ID,
                ParentID = null
            }).Entity;
            context.SaveChanges();
            Debug.WriteLine("Comment ID: {0}", c.ID);

            //ID: 6
            c = context.Comment.Add(new Comment()
            {
                UserName = USERNAMES.Unit,
                Content = "This is a comment in v/MinCCP Sub from user unit",
                CreationDate = DateTime.UtcNow.AddHours(-4),
                SubmissionID = minCCPSubmission.ID,
                ParentID = null
            }).Entity;
            context.SaveChanges();
            Debug.WriteLine("Comment ID: {0}", c.ID);

            #endregion Submissions

            #region UserPrefernces

            context.UserPreference.Add(new UserPreference()
            {
                UserName = USERNAMES.Unit,
                DisableCSS = false,
                NightMode = true,
                Language = "en",
                //OpenInNewWindow = true,
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
            Debug.WriteLine("Comment ID: {0}", c.ID);

            #endregion UserPrefernces

            #region Create Test Users

            //default users
            CreateUser(USERNAMES.Unit);
            CreateUser(USERNAMES.Anon);

            //these blocks are used for testing individual operations
            CreateUserBatch(CONSTANTS.UNIT_TEST_USER_TEMPLATE, 0, 50);
            CreateUserBatch(CONSTANTS.TEST_USER_TEMPLATE, 0, 50);

            //Users with varying levels of CCP
            CreateUser(USERNAMES.User0CCP);
            CreateUser(USERNAMES.User50CCP);
            CreateUser(USERNAMES.User100CCP, DateTime.UtcNow.AddDays(-45));
            CreateUser(USERNAMES.User500CCP, DateTime.UtcNow.AddDays(-60));


            var s = context.Submission.Add(new Submission()
            {

                UserName = USERNAMES.User500CCP,
                Title = "Test Submission",
                Type = 1,
                Subverse = SUBVERSES.Unit,
                Content = String.Format("Test Submission w/Upvotes 50"),
                CreationDate = DateTime.UtcNow,
                //SubmissionID = unitSubmission.ID,
                UpCount = 500,
            }).Entity;
            context.SaveChanges();
            VoteContent(context, Domain.Models.ContentType.Submission, s.ID, 500, Domain.Models.VoteValue.Up);

            c = context.Comment.Add(new Comment()
            {
                UserName = USERNAMES.User50CCP,
                Content = String.Format("Test Comment w/Upvotes 50"),
                CreationDate = DateTime.UtcNow,
                SubmissionID = unitSubmission.ID,
                UpCount = 50,
                ParentID = null
            }).Entity;
            context.SaveChanges();
            VoteContent(context, Domain.Models.ContentType.Comment, c.ID, 50, Domain.Models.VoteValue.Up);

            c = context.Comment.Add(new Comment()
            {
                UserName = USERNAMES.User100CCP,
                Content = String.Format("Test Comment w/Upvotes 100"),
                CreationDate = DateTime.UtcNow,
                SubmissionID = unitSubmission.ID,
                UpCount = 100,
                ParentID = null
            }).Entity;
            context.SaveChanges();
            VoteContent(context, Domain.Models.ContentType.Comment, c.ID, 100, Domain.Models.VoteValue.Up);

            c = context.Comment.Add(new Comment()
            {
                UserName = USERNAMES.User500CCP,
                Content = String.Format("Test Comment w/Upvotes 500"),
                CreationDate = DateTime.UtcNow,
                SubmissionID = unitSubmission.ID,
                UpCount = 500,
                ParentID = null
            }).Entity;
            context.SaveChanges();
            VoteContent(context, Domain.Models.ContentType.Comment, c.ID, 500, Domain.Models.VoteValue.Up);

            #endregion Create Test Users

            #region Banned Test Data

            CreateUser("BannedFromVUnit");
            CreateUser("BannedGlobally");

            context.BannedUser.Add(new BannedUser() { CreatedBy = USERNAMES.Unit, CreationDate = DateTime.UtcNow.AddDays(-30), Reason = "Unit Testing Global Ban", UserName = "BannedGlobally" });
            context.SubverseBan.Add(new SubverseBan() { Subverse = SUBVERSES.Unit, CreatedBy = USERNAMES.Unit, CreationDate = DateTime.UtcNow.AddDays(-30), Reason = "Unit Testing v/Unit Ban", UserName = "BannedFromVUnit" });
            context.BannedDomain.Add(new BannedDomain() { CreatedBy = USERNAMES.Unit, CreationDate = DateTime.UtcNow.AddDays(-15), Domain = "fleddit.com", Reason = "Turned Digg migrants into jelly fish" });

            context.SaveChanges();

            #endregion BannedUsers Test Data

            #region Disabled Test Data

            context.Subverse.Add(new Subverse() { Name = SUBVERSES.Disabled, Title = "Disabled", IsAdminDisabled = true, CreatedBy = USERNAMES.Unit, CreationDate = DateTime.UtcNow.AddDays(-100), SideBar = "We will never be disabled"});

            context.SaveChanges();

            #endregion BannedUsers Test Data

            #region AddDefaultSubs

            context.DefaultSubverse.Add(new DefaultSubverse() { Subverse = SUBVERSES.AskVoat, Order = 1 });
            context.DefaultSubverse.Add(new DefaultSubverse() { Subverse = SUBVERSES.Whatever, Order = 2 });
            context.DefaultSubverse.Add(new DefaultSubverse() { Subverse = SUBVERSES.News, Order = 3 });
            context.SaveChanges();

            #endregion AddDefaultSubs


            #region User500CCP Comment UpVotes
            //This user needs some upvotes in order to downvoat without triggering the mean rules

            for (int i = 0; i < 5; i++)
            {
                var user = TestHelper.SetPrincipal(USERNAMES.User500CCP);
                var submission = TestHelper.ContentCreation.CreateSubmission(TestHelper.NextUserName(), new Domain.Models.UserSubmission() { Title = $"This is an UpVote Farm Thread! ({i})", Content = "This Submission and all comments will be upvoted by User500CCP. Get in here!", Subverse = SUBVERSES.Whatever });
                var scmd = new SubmissionVoteCommand(submission.ID, 1, Guid.NewGuid().ToString()).SetUserContext(user);
                var sr = scmd.Execute().Result;
                VoatAssert.IsValid(sr);

                var comment = TestHelper.ContentCreation.CreateComment(TestHelper.NextUserName(), submission.ID, "Please UpVote Me!");
                user = TestHelper.SetPrincipal(USERNAMES.User500CCP);
                var cmd = new CommentVoteCommand(comment.ID, 1, Guid.NewGuid().ToString()).SetUserContext(user);
                var r = cmd.Execute().Result;
                VoatAssert.IsValid(r);
            }
            CacheHandler.Instance.Remove(CachingKey.UserInformation(USERNAMES.User500CCP));

            #endregion

           
            //******************************************************************************************************************
            // ADD YOUR STUFF BELOW - DO NOT EDIT THE ABOVE CODE - NOT EVEN ONCE - I'LL SO FIGHT YOU IF YOU DO AND I FIGHT DIRTY
            //******************************************************************************************************************
        }
        public static void CreateSorted(string subverse) {
            using (var context = new VoatDataContext())
            {
                var sortSubverse = context.Subverse.Add(new Subverse()
                {
                    Name =$"{subverse}",
                    Title = $"v/{subverse}",
                    Description = "Unit test Sort Testing",
                    SideBar = "For Sort Testing",
                    //Type = "link",
                    IsAnonymized = false,
                    CreationDate = DateTime.UtcNow.AddDays(-70),
                    IsAdminDisabled = false
                }).Entity;
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
                    submission.UserName = USERNAMES.Anon;
                    submission.DownCount = (even ? i : i / 2);
                    submission.UpCount = (!even ? i : i * 2);
                    submission.Views = i * i;
                    Ranking.RerankSubmission(submission);
                    context.Submission.Add(submission);
                    context.SaveChanges();
                }
            }
        }
        public static int BuildCommentTree(string subverse, string commentContent, int rootDepth, int nestedDepth, int recurseCount)
        {
            using (var db = new VoatDataContext())
            {
                var s = db.Subverse.Where(x => x.Name == subverse).FirstOrDefault();
                //create submission
                var submission = new Submission()
                {
                    CreationDate = DateTime.UtcNow,
                    Content = $"Comment Tree for v/{subverse}",
                    Subverse = subverse,
                    Title = $"Comment Tree for v/{subverse}",
                    Type = 1,
                    UserName = "TestUser01",
                    IsAnonymized = s.IsAnonymized.HasValue ? s.IsAnonymized.Value : false,
                };
                db.Submission.Add(submission);
                db.SaveChanges();

                Func<VoatDataContext, int?, string, string, int> createComment = (context, parentCommentID, content, userName) => {

                    var comment = new Comment() {
                        SubmissionID = submission.ID,
                        UserName = userName,
                        Content = content,
                        FormattedContent = Formatting.FormatMessage(content),
                        ParentID = parentCommentID,
                        IsAnonymized = submission.IsAnonymized,
                        CreationDate = DateTime.UtcNow
                    };
                    context.Comment.Add(comment);
                    context.SaveChanges();
                    System.Threading.Thread.Sleep(2);
                    return comment.ID;
                };
                Action<VoatDataContext, int?, int, int, string, int> createNestedComments = null;
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

            var manager = VoatUserManager.Create();

            //if (!UserHelper.UserExists(userName))
            //{
                var user = new Voat.Data.Models.VoatIdentityUser() { UserName = userName, RegistrationDateTime = (registrationDate.HasValue ? registrationDate.Value : DateTime.UtcNow), LastLoginDateTime = new DateTime(1900, 1, 1, 0, 0, 0, 0), LastLoginFromIp = "0.0.0.0" };

                string pwd = userName.GetUnitTestUserPassword();
                
                var result = manager.Create(user, pwd);

                if (!result.Succeeded)
                {
                    throw new Exception("Error creating test user " + userName);
                }
           // }
        }
        public static void VoteContent(VoatDataContext context, Domain.Models.ContentType contentType, int id, int count, Domain.Models.VoteValue vote)
        {
            for (int i = 0; i < count; i++)
            {
                string userName = String.Format("VoteUser{0}", i.ToString().PadLeft(4, '0'));
                if (contentType == Domain.Models.ContentType.Comment)
                {
                    context.CommentVoteTracker.Add(
                        new CommentVoteTracker()
                        {
                            UserName = userName,
                            IPAddress = Guid.NewGuid().ToString(),
                            CommentID = id,
                            VoteStatus = (int)vote,
                            VoteValue = (int)vote,
                            CreationDate = DateTime.UtcNow
                        });

                }
                else if (contentType == Domain.Models.ContentType.Submission)
                {
                    context.SubmissionVoteTracker.Add(
                        new SubmissionVoteTracker()
                        {
                            UserName = userName,
                            IPAddress = Guid.NewGuid().ToString(),
                            SubmissionID = id,
                            VoteStatus = (int)vote,
                            VoteValue = (int)vote,
                            CreationDate = DateTime.UtcNow
                        });

                }
            }
            context.SaveChanges();
        }
        public static void CreateUserBatch(string userNameTemplate, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                CreateUser(String.Format(userNameTemplate, i.ToString().PadLeft(2, '0')));
            }
        }
    }
}
