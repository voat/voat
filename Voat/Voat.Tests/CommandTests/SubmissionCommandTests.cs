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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Query;
using Voat.Tests.Repository;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class SubmissionCommandTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void CreateSubmission()
        {
            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() {Subverse= "whatever", Title = "This is a title", Url = "http://www.yahoo.com" });

            var r = cmd.Execute().Result;

            Assert.IsNotNull(r, "Response is null");
            Assert.IsTrue(r.Success, r.Message);
            Assert.IsNotNull(r.Response, "Expecting a non null response");
            Assert.AreNotEqual(0, r.Response.ID);
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void CreateSubmissionTrapJSInUrl()
        {
            TestHelper.SetPrincipal("TestUser6");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "whatever", Title = "This is a title", Url = "javascript:alert('arst');" });
            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response is null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "The url you are trying to submit is invalid");
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void CreateSubmissionTrapJSMarkdown()
        {
            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "whatever", Title = "This is a title", Content = "[Click here... Please. For research.](javascript:alert('arst');)" });
            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response is null");
            Assert.IsTrue(r.Success, r.Message);
            //Assert.AreEqual(r.Message, "The url you are trying to submit is invalid");
            var q = new QuerySubmission(r.Response.ID);
            var submission = q.Execute();
            Assert.IsNotNull(submission, "Submission not found");
            Assert.AreEqual(submission.FormattedContent, "<p><a href=\"#\" data-ScriptStrip=\"/* script detected: javascript:alert('arst'); */\">Click here... Please. For research.</a></p>");

        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post"), TestCategory("Anon")]
        public void CreateSubmission_Anon()
        {
            string userName = "TestUser4";
            TestHelper.SetPrincipal(userName);

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "anon", Title = "This is a title", Url = "http://www.yahoo.com" });

            var r = cmd.Execute().Result;

            Assert.IsNotNull(r, "Response is null");
            Assert.IsTrue(r.Success, r.Message);
            Assert.IsNotNull(r.Response, "Expecting a non null response");
            Assert.AreNotEqual(0, r.Response.ID, "Expected a valid ID");
            Assert.AreNotEqual("TestUser2", r.Response.UserName);
            Assert.AreEqual(true, r.Response.IsAnonymized);
            Assert.AreNotEqual(userName, r.Response.UserName);

        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public async Task DeleteSubmission()
        {
            TestHelper.SetPrincipal("anon");

            var cmd = new DeleteSubmissionCommand(3);
            var r = await cmd.Execute();

            Assert.IsTrue(r.Success, r.Message);
            //Assert.Inconclusive();
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]

        public void Edit_Submission_Title_Content()
        {
            TestHelper.SetPrincipal("anon");

            var x = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "anon", Title = "xxxxxxxxxxxx", Content = "xxxxxxxxxxxx" });
            var s = x.Execute().Result;

            Assert.IsNotNull(s, "Response is null");
            Assert.IsTrue(s.Success, s.Message);

            var cmd = new EditSubmissionCommand(s.Response.ID, new Domain.Models.UserSubmission() { Title = "yyyyyyyyyyyy", Content = "yyyyyyyyyyyy" });
            var r = cmd.Execute().Result;

            Assert.IsNotNull(r, "Response is null");
            Assert.IsTrue(r.Success, "Edit Submission failed to return true: " + r.Message);

            using (var repo = new Voat.Data.Repository())
            {
                var submission = repo.GetSubmission(s.Response.ID);
                Assert.IsNotNull(submission, "Can't find submission from repo");
                Assert.AreEqual("yyyyyyyyyyyy", submission.Title);
                Assert.AreEqual("yyyyyyyyyyyy", submission.Content);
            }

            //Assert.Inconclusive();
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void PreventUrlTitlePosts()
        {
            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "whatever", Title = "http://www.yahoo.com", Url = "http://www.yahoo.com" });

            var r = cmd.Execute().Result;
            Assert.IsFalse(r.Success);
            Assert.AreEqual(r.Message, "Submission title may not be the same as the URL you are trying to submit. Why would you even think about doing this?! Why?");
            //Assert.AreNotEqual(0, r.Response.ID);
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post"), TestCategory("Bug")]
        public void PreventUrlTitlePosts_BugTrap()
        {
            //BUGFIX: https://voat.co/v/FGC/1349484
            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "whatever", Title = "http://www.besthealthmarket.org/cianix-male-enhancement/", Content = "Cianix is the best available product in the market. It has long lasting anti-aging effects. It made up of natural products and has no side effects. Therefore it is a highly recommended  product for anti-aging as well as healthy skin. Signs of Healthy Skin We talk a lot about healthy skin and methods of achieving healthy skin. But what healthy skin actually is only a few of us know it. Here are few points that a healthy skin has: Even color:" });

            var r = cmd.Execute().Result;
            Assert.IsFalse(r.Success);
            Assert.AreEqual(r.Message, "Submission title is a url? Why would you even think about doing this?! Why?");
            //Assert.AreNotEqual(0, r.Response.ID);
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void PreventPartialUrlTitlePosts()
        {
            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "whatever", Title = "www.yahoo.com", Url = "http://www.yahoo.com" });
            var r = cmd.Execute().Result;
            Assert.IsFalse(r.Success);
            Assert.AreEqual(r.Message, "Submission title may not be the same as the URL you are trying to submit. Why would you even think about doing this?! Why?");
            //Assert.AreNotEqual(0, r.Response.ID);
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void PreventInvalidUrlTitlePosts()
        {
            TestHelper.SetPrincipal("TestUser7");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "whatever", Title = "Super rad website", Url = "http//www.yahoo.com" });

            var r = cmd.Execute().Result;
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "The url you are trying to submit is invalid");
            //Assert.AreNotEqual(0, r.Response.ID);
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]

        public void PreventNoSubversePost()
        {
            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "", Title = "Hello Man", Url = "http://www.yahoo.com" });

            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, "Expecting a false response");
            Assert.AreEqual(r.Message, "A subverse must be provided");
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        [TestCategory("Ban"), TestCategory("Ban.Domain")]
        public void PreventBannedDomainPost()
        {
            using (var repo = new voatEntities())
            {
                repo.BannedDomains.Add(new BannedDomain() { Domain = "saiddit.com", Reason = "No one really likes you.", CreatedBy = "UnitTest", CreationDate = DateTime.UtcNow });
                repo.SaveChanges(); 
            }

            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "unit", Title = "Hello Man - Longer because of Rules", Url = "http://www.saiddit.com/images/feelsgoodman.jpg" });
            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "Submission contains banned domains");

            cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "unit", Title = "Hello Man - Longer because of Rules", Content = "Check out this cool image I found using dogpile.com: http://saiddit.com/images/feelsgoodman.jpg" });
            r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "Submission contains banned domains");

            cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "unit", Title = "Test Embedded Banned Domain",
                Content = "http://yahoo.com/index.html Check out this cool image I found using compuserve.com: http://saiddit.com/images/feelsgoodman.jpg. http://www2.home.geocities.com/index.html"
            });
            r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "Submission contains banned domains");

        }


        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        [TestCategory("Ban"), TestCategory("Ban.Domain")]
        public void PreventBannedDomainPost_MultiPartDomains()
        {
            using (var repo = new voatEntities())
            {
                repo.BannedDomains.Add(new BannedDomain() { Domain = "one.two.three.com", Reason = "People hate counting", CreatedBy = "UnitTest", CreationDate = DateTime.UtcNow });
                repo.SaveChanges();
            }

            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "unit", Title = "Hello Man - Longer because of Rules", Url = "http://www.one.two.three.com/images/feelsgoodman.jpg" });
            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "Submission contains banned domains");

            cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "unit", Title = "Hello Man - Longer because of Rules", Content = "Check out this cool image I found using dogpile.com: HTTP://one.TWO.three.com/images/feelsgoodman.jpg" });
            r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "Submission contains banned domains");

        }
        
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public async Task PreventSubmittingSameLinkTwice()
        {
            TestHelper.SetPrincipal("UnitTestUser30");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "unit", Title = "My name is Fuzzy and my fixes are way cooler than puttputts", Url = "http://i.deleted.this.by/accident.jpg" });
            var r = await cmd.Execute();
            Assert.IsNotNull(r, "Response was null");
            Assert.IsTrue(r.Success, r.Message);
            var id = r.Response.ID;

            cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "unit", Title = "Hello Man - Longer because of Rules", Url = "http://i.deleted.this.by/accident.jpg" });
            r = await cmd.Execute();
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "Sorry, this link has already been submitted recently. https://voat.co/v/unit/" + id);
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public async Task AllowSubmittingDeletedLink()
        {
            TestHelper.SetPrincipal("UnitTestUser31");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "unit", Title = "I need to think of better titles but I don't want to get in trouble", Url = "http://i.deleted.this.on/purpose.jpg" });
            var r = await cmd.Execute();
            Assert.IsNotNull(r, "Response was null");
            Assert.IsTrue(r.Success, r.Message);
            var id = r.Response.ID;

            var cmd2 = new DeleteSubmissionCommand(id);
            var r2 = await cmd2.Execute();

            Assert.IsTrue(r2.Success, r2.Message);

            cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "unit", Title = "Hello Man - Longer because of Rules", Url = "http://i.deleted.this.on/purpose.jpg" });
            r = await cmd.Execute();
            Assert.IsNotNull(r, "Response was null");
            Assert.IsTrue(r.Success, r.Message);
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void PreventShortTitlePosts()
        {
            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "unit", Title = "What", Url = "http://www.hellogoodbye.com/images/feelsgoodman.jpg" });
            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "A title may not be less than 5 characters");
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        [TestCategory("Ban"), TestCategory("Ban.User")]
        public void PreventGlobalBannedUsers()
        {
            TestHelper.SetPrincipal("BannedGlobally");
            var userSubmission = new Domain.Models.UserSubmission() { Subverse = "unit", Title = Guid.NewGuid().ToString(), Url = "http://www.SendhelpImStuckInUnitTests.com/images/feelsgoodman.jpg" };
            var cmd = new CreateSubmissionCommand(userSubmission);
            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "User is globally banned");
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        [TestCategory("Ban"), TestCategory("Ban.User")]
        public void PreventSubverseBannedUsers()
        {
            TestHelper.SetPrincipal("BannedFromVUnit");
            var userSubmission = new Domain.Models.UserSubmission() { Subverse = "unit", Title = Guid.NewGuid().ToString(), Url = "http://www.SuperAwesomeDomainName.com/images/feelsgoodman.jpg" };
            var cmd = new CreateSubmissionCommand(userSubmission);
            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, $"User is banned from v/{userSubmission.Subverse}");
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void PreventUserFromPostingToAuthorizedOnlySubverses()
        {
            TestHelper.SetPrincipal("TestUser9");
            var userSubmission = new Domain.Models.UserSubmission() { Subverse = "AuthorizedOnly", Title = Guid.NewGuid().ToString(), Url = "http://www.digit.com/images/feelsgoodman.jpg" };
            var cmd = new CreateSubmissionCommand(userSubmission);
            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "You are not authorized to submit links or start discussions in this subverse. Please contact subverse moderators for authorization");

        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void PreventUserFromPostingCompromisedTitle1()
        {
            TestHelper.SetPrincipal("TestUser1");
            var userSubmission = new Domain.Models.UserSubmission() { Subverse = "whatever", Title = "\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000", Content = "cookies" };
            var cmd = new CreateSubmissionCommand(userSubmission);
            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "Submission title can not contain Unicode or unprintable characters");
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void PreventPostingToDisabledSub()
        {
            TestHelper.SetPrincipal("TestUser6");
            var userSubmission = new Domain.Models.UserSubmission() { Subverse = "disabled", Title = "I am not paying attention", Content = "Why was this sub disabled?" };
            var cmd = new CreateSubmissionCommand(userSubmission);
            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "Subverse is disabled");
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void TestNegativeSCPSubmission()
        {
            var userName = "NegativeSCP";
            //Create user
            VoatDataInitializer.CreateUser(userName, DateTime.UtcNow.AddDays(-450));
            //Add submission with negatives directly to db
            using (var context = new voatEntities())
            {
                
                var s = context.Submissions.Add(new Submission()
                {
                    CreationDate = DateTime.UtcNow.AddHours(-12),
                    Subverse = "unit",
                    Title = "Test Negative SCP",
                    Url = "https://www.youtube.com/watch?v=pnbJEg9r1o8",
                    Type = 2,
                    UpCount = 2,
                    DownCount = 13,
                    UserName = userName
                });
                context.SaveChanges();
            }



            TestHelper.SetPrincipal(userName);
            var userSubmission = new Domain.Models.UserSubmission() { Subverse = "unit", Title = "Le Censorship!", Content = "Will this work?" };
            var cmd = new CreateSubmissionCommand(userSubmission);
            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsTrue(r.Success, r.Message);
        }
    }
}
