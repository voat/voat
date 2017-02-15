using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Domain.Command;
using Voat.Domain.Query;
using Voat.Tests.Repository;
using Voat.Utilities;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class UserCommandTests : BaseUnitTest
    {

        [TestMethod]
        [TestCategory("User"), TestCategory("Command"), TestCategory("Query")]
        public async Task UpdateUserPreferences()
        {
            var userName = "TestUser5";
            var bio = Guid.NewGuid().ToString();
            TestHelper.SetPrincipal(userName);

            var q = new QueryUserPreferences(userName);
            var prefs = await q.ExecuteAsync();

            Assert.IsNotNull(prefs, "Pref query returned null");
            Assert.AreNotEqual(prefs.Bio, bio, "Bio returned unexpected data");

            var cmd = new UpdateUserPreferencesCommand(new Domain.Models.UserPreference() { Bio = bio });
            var result = await cmd.Execute();

            Assert.IsNotNull(result, "UpdatePref command returned null");
            Assert.AreEqual(true, result.Success, "UpdatePref command returned non success");

            q = new QueryUserPreferences(userName);
            prefs = await q.ExecuteAsync();

            Assert.IsNotNull(prefs, "Pref requery returned null");
            Assert.AreEqual(prefs.Bio, bio, "Bio not updated");

        }

        [TestMethod]
        [TestCategory("User"), TestCategory("Command"), TestCategory("Query")]
        public async Task UserSaves()
        {
            var userName = "UnitTestUser20";
            var bio = Guid.NewGuid().ToString();
            TestHelper.SetPrincipal(userName);

            //save start test data
            using (var repo = new Voat.Data.Repository())
            {
                var r = await repo.Save(Domain.Models.ContentType.Submission, 1);
                Assert.AreEqual(Status.Success, r.Status);

                r = await repo.Save(Domain.Models.ContentType.Comment, 1);
                Assert.AreEqual(Status.Success, r.Status);

            }

            var q = new QueryUserSaves(Domain.Models.ContentType.Submission);
            var d = await q.ExecuteAsync();
            Assert.AreEqual(true, d.Contains(1));
            Assert.AreEqual(1, d.Count);
            Assert.AreEqual(true, CacheHandler.Instance.Exists(CachingKey.UserSavedItems(Domain.Models.ContentType.Submission, userName)));

            q = new QueryUserSaves(Domain.Models.ContentType.Comment);
            d = await q.ExecuteAsync();
            Assert.AreEqual(true, d.Contains(1));
            Assert.AreEqual(1, d.Count);
            Assert.AreEqual(true, CacheHandler.Instance.Exists(CachingKey.UserSavedItems(Domain.Models.ContentType.Comment, userName)));


            //check helper object
            Assert.AreEqual(true, UserHelper.IsSaved(Domain.Models.ContentType.Submission, 1));
            Assert.AreEqual(true, UserHelper.IsSaved(Domain.Models.ContentType.Comment, 1));

            Assert.AreEqual(false, UserHelper.IsSaved(Domain.Models.ContentType.Submission, 2));
            Assert.AreEqual(false, UserHelper.IsSaved(Domain.Models.ContentType.Comment, 2));


            var cmd = new SaveCommand(Domain.Models.ContentType.Submission, 2);
            var response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status);


            cmd = new SaveCommand(Domain.Models.ContentType.Comment, 2);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status);


            Assert.AreEqual(true, UserHelper.IsSaved(Domain.Models.ContentType.Submission, 2));
            Assert.AreEqual(true, UserHelper.IsSaved(Domain.Models.ContentType.Comment, 2));

            cmd = new SaveCommand(Domain.Models.ContentType.Submission, 1);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status);


            cmd = new SaveCommand(Domain.Models.ContentType.Comment, 1);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status);

            Assert.AreEqual(false, UserHelper.IsSaved(Domain.Models.ContentType.Submission, 1));
            Assert.AreEqual(false, UserHelper.IsSaved(Domain.Models.ContentType.Comment, 1));

        }

        [TestMethod]
        [TestCategory("User"), TestCategory("Command"), TestCategory("User.DeleteAccount")]
        public async Task DeleteAccount_Failures()
        {
            var userName = "TestDeleteUserBad";

            VoatDataInitializer.CreateUser(userName);

            TestHelper.SetPrincipal(userName);
            //username doesnt match
            var cmd = new DeleteAccountCommand(
                new Domain.Models.DeleteAccountOptions()
                {
                    UserName = userName,
                    ConfirmUserName = "Tom"
                });
            var result = await cmd.Execute();
            Assert.IsFalse(result.Success, result.Message);

            //password doesn't match
            cmd = new DeleteAccountCommand(
                new Domain.Models.DeleteAccountOptions()
                {
                    UserName = userName,
                    ConfirmUserName = userName,
                    CurrentPassword = "NotCorrect"
                });
            result = await cmd.Execute();
            Assert.IsFalse(result.Success, result.Message);

            //wrong user
            TestHelper.SetPrincipal("TestUser01");

            cmd = new DeleteAccountCommand(
                new Domain.Models.DeleteAccountOptions()
                {
                    UserName = userName,
                    ConfirmUserName = userName,
                    CurrentPassword = userName
                });
            result = await cmd.Execute();
            Assert.IsFalse(result.Success, result.Message);


        }

        [TestMethod]
        [TestCategory("User"), TestCategory("Command"), TestCategory("User.DeleteAccount")]
        public async Task DeleteAccount_Basic()
        {
            var userName = "TestDeleteUser01";

            VoatDataInitializer.CreateUser(userName);
            TestHelper.SetPrincipal(userName);
            DeleteAccountCommand cmd;
            CommandResponse result;

            var options = new Domain.Models.DeleteAccountOptions()
            {
                UserName = userName,
                ConfirmUserName = userName,
                CurrentPassword = userName,
                Comments = Domain.Models.DeleteOption.Anonymize,
                LinkSubmissions = Domain.Models.DeleteOption.Anonymize,
                TextSubmissions = Domain.Models.DeleteOption.Anonymize
            };

            var submission = TestHelper.ContentCreation.CreateSubmission(options.UserName, new Domain.Models.UserSubmission()
            {
                Subverse = "unit",
                Title = "Test Submission Yeah",
                Content = "Test Submission Content"
            });

            var comment = TestHelper.ContentCreation.CreateComment(options.UserName, submission.ID, "This is a test comment");

            //Anon it all 
            cmd = new DeleteAccountCommand(options);
            result = await cmd.Execute();
            Assert.IsTrue(result.Success, result.Message);
            VerifyDelete(options);


            userName = "TestDeleteUser02";

            VoatDataInitializer.CreateUser(userName);
            TestHelper.SetPrincipal(userName);

            options = new Domain.Models.DeleteAccountOptions()
            {
                UserName = userName,
                ConfirmUserName = userName,
                CurrentPassword = userName,
                Comments = Domain.Models.DeleteOption.Delete,
                LinkSubmissions = Domain.Models.DeleteOption.Delete,
                TextSubmissions = Domain.Models.DeleteOption.Delete
            };

            submission = TestHelper.ContentCreation.CreateSubmission(options.UserName, new Domain.Models.UserSubmission()
            {
                Subverse = "unit",
                Title = "Test Submission Yeah",
                Content = "Test Submission Content"
            });

            comment = TestHelper.ContentCreation.CreateComment(options.UserName, submission.ID, "This is a test comment");

            //Delete
            cmd = new DeleteAccountCommand(options);
            result = await cmd.Execute();
            Assert.IsTrue(result.Success, result.Message);
            VerifyDelete(options);



            userName = "TestDeleteUser03";

            VoatDataInitializer.CreateUser(userName);
            TestHelper.SetPrincipal(userName);

            options = new Domain.Models.DeleteAccountOptions()
            {
                UserName = userName,
                ConfirmUserName = userName,
                CurrentPassword = userName,
                Comments = Domain.Models.DeleteOption.Delete,
                LinkSubmissions = Domain.Models.DeleteOption.Delete,
                TextSubmissions = Domain.Models.DeleteOption.Delete,
                RecoveryEmailAddress = "bads@motherfather.com",
                ConfirmRecoveryEmailAddress = "bads@motherfather.com",
                Reason = "I need a break from the racialists"
            };

            submission = TestHelper.ContentCreation.CreateSubmission(options.UserName, new Domain.Models.UserSubmission()
            {
                Subverse = "unit",
                Title = "Test Submission Yeah",
                Content = "Test Submission Content"
            });

            comment = TestHelper.ContentCreation.CreateComment(options.UserName, submission.ID, "This is a test comment");

            //Delete
            cmd = new DeleteAccountCommand(options);
            result = await cmd.Execute();
            Assert.IsTrue(result.Success, result.Message);
            VerifyDelete(options);

        }


        private void VerifyDelete(Domain.Models.DeleteAccountOptions options)
        {
            using (var db = new Voat.Data.Models.voatEntities())
            {
                int count = 0;
                switch (options.Comments)
                {
                    case Domain.Models.DeleteOption.Anonymize:

                        count = db.Comments.Count(x => x.UserName.Equals(options.UserName, StringComparison.OrdinalIgnoreCase) && !x.IsAnonymized);
                        Assert.AreEqual(0, count, $"Comment {options.Comments.ToString()} setting found violations");

                        break;
                    case Domain.Models.DeleteOption.Delete:

                        count = db.Comments.Count(x => x.UserName.Equals(options.UserName, StringComparison.OrdinalIgnoreCase) && !x.IsDeleted);
                        Assert.AreEqual(0, count, $"Comment {options.Comments.ToString()} setting found violations");
                        break;
                }

                var checkSubmissions = new Action<string, Domain.Models.SubmissionType, Domain.Models.DeleteOption>((userName, submissionType, deleteSetting) =>
                {
                    switch (deleteSetting)
                    {
                        case Domain.Models.DeleteOption.Anonymize:

                            count = db.Submissions.Count(x => x.UserName.Equals(options.UserName, StringComparison.OrdinalIgnoreCase) && !x.IsAnonymized && x.Type == (int)submissionType);
                            Assert.AreEqual(0, count, $"{submissionType.ToString()} Submission {deleteSetting.ToString()} setting found violations");

                            break;
                        case Domain.Models.DeleteOption.Delete:

                            count = db.Submissions.Count(x => x.UserName.Equals(options.UserName, StringComparison.OrdinalIgnoreCase) && !x.IsDeleted && x.Type == (int)submissionType);
                            Assert.AreEqual(0, count, $"{submissionType.ToString()} Submission {deleteSetting.ToString()} setting found violations");
                            break;
                    }
                });

                checkSubmissions(options.UserName, Domain.Models.SubmissionType.Text, options.TextSubmissions);
                checkSubmissions(options.UserName, Domain.Models.SubmissionType.Link, options.LinkSubmissions);

                //Check account settings.
                using (var userManager = new UserManager<Voat.Data.Models.VoatUser>(new UserStore<Voat.Data.Models.VoatUser>(new Voat.Data.Models.ApplicationDbContext())))
                {
                    var userAccount = userManager.FindByName(options.UserName);

                    Assert.IsNotNull(userAccount, "Can't find user account using manager");
                    if (!String.IsNullOrEmpty(options.RecoveryEmailAddress))
                    {
                        //Verify recovery info
                        Assert.AreEqual(userAccount.Email, options.RecoveryEmailAddress);
                        Assert.IsTrue(userAccount.LockoutEnabled, "Lockout should be enabled");
                        Assert.IsNotNull(userAccount.LockoutEndDateUtc, "Lockout should not be null");
                        Assert.IsTrue(userAccount.LockoutEndDateUtc.Value.Subtract(DateTime.UtcNow) >= TimeSpan.FromDays(89), "Lockout be set to roughly 90 days");
                    }
                    else
                    {
                        Assert.AreEqual(userAccount.Email, null);
                        Assert.IsFalse(userAccount.LockoutEnabled, "Lockout should be enabled");
                    }

                    //Make sure password is reset
                    var passwordAccess = userManager.Find(options.UserName, options.CurrentPassword);
                    Assert.IsNull(passwordAccess, "Can access user account with old password");

                }
            }
        }
    }
}
