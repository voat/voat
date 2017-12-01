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
using System.Linq;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Configuration;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Query;
using Voat.Tests.Infrastructure;
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
            var userName = "TestUser02";
            var user = TestHelper.SetPrincipal(userName);
            var s = new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Whatever, Title = "This is a title", Url = "http://www.yahoo.com" };
            var cmd = new CreateSubmissionCommand(s).SetUserContext(user);

            var r = cmd.Execute().Result;

            VoatAssert.IsValid(r);
            Assert.IsNotNull(r.Response, "Expecting a non null response");
            Assert.AreNotEqual(0, r.Response.ID);
            Assert.AreEqual(userName, r.Response.UserName);
            Assert.AreEqual(s.Subverse, r.Response.Subverse);
            Assert.AreEqual(s.Title, r.Response.Title);
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void CreateSubmissionTrapJSInUrl()
        {
            var user = TestHelper.SetPrincipal("TestUser06");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Whatever, Title = "This is a title", Url = "javascript:alert('arst');" }).SetUserContext(user);
            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r, Status.Invalid);
            Assert.AreEqual(r.Message, "The url you are trying to submit is invalid");
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void CreateSubmissionTrapJSMarkdown()
        {
            var user = TestHelper.SetPrincipal("TestUser02");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Whatever, Title = "This is a title", Content = "[Click here... Please. For research.](javascript:alert('arst');)" }).SetUserContext(user);
            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r);
            //Assert.AreEqual(r.Message, "The url you are trying to submit is invalid");
            var q = new QuerySubmission(r.Response.ID);
            var submission = q.Execute();
            Assert.IsNotNull(submission, "Submission not found");
            Assert.AreEqual(submission.FormattedContent, "<p><a href=\"#\" data-ScriptStrip=\"/* script detected: javascript:alert('arst'); */\">Click here... Please. For research.</a></p>");

        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post"), TestCategory("Anon")]
        public void CreateSubmission_NonAnon_InAnonSub()
        {
            string userName = "TestUser04";
            var user = TestHelper.SetPrincipal(userName);

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Anon, Title = "This is a title", Url = "http://www.yahoo.com", IsAnonymized = true }).SetUserContext(user);

            var r = cmd.Execute().Result;

            VoatAssert.IsValid(r);
            Assert.IsNotNull(r.Response, "Expecting a non null response");
            Assert.AreNotEqual(0, r.Response.ID, "Expected a valid ID");
            Assert.AreNotEqual("TestUser02", r.Response.UserName);
            Assert.AreEqual(true, r.Response.IsAnonymized);
            Assert.AreNotEqual(userName, r.Response.UserName);

        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post"), TestCategory("Anon")]
        public void CreateSubmission_Anon_InAnonSub()
        {
            string userName = "TestUser04";
            var user = TestHelper.SetPrincipal(userName);

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Anon, Title = "This is a title", Url = "http://www.yahoo.com/someurl", IsAnonymized = true }).SetUserContext(user);

            var r = cmd.Execute().Result;

            VoatAssert.IsValid(r);
            Assert.IsNotNull(r.Response, "Expecting a non null response");
            Assert.AreNotEqual(0, r.Response.ID, "Expected a valid ID");
            Assert.AreNotEqual("TestUser02", r.Response.UserName);
            Assert.AreEqual(true, r.Response.IsAnonymized);
            Assert.AreNotEqual(userName, r.Response.UserName);

        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post"), TestCategory("Anon")]
        public async Task CreateSubmission_Anon_InNonAnonSub()
        {
            string userName = "TestUser04";
            var user = TestHelper.SetPrincipal(userName);

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "This is a title", Url = "http://www.yahoo.com", IsAnonymized = true }).SetUserContext(user).SetUserContext(user);

            var r = await cmd.Execute();
            VoatAssert.IsValid(r, Status.Denied);
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post"), TestCategory("Anon")]
        public async Task CreateSubmission_NSFW()
        {
            string userName = "TestUser04";
            var user = TestHelper.SetPrincipal(userName);

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "This is a title", Url = "http://www.yahoo.com", IsAdult = true }).SetUserContext(user);

            var r = await cmd.Execute();

            VoatAssert.IsValid(r);
            Assert.IsNotNull(r.Response, "Expecting a non null response");
            Assert.AreNotEqual(0, r.Response.ID, "Expected a valid ID");
            Assert.AreEqual(true, r.Response.IsAdult);

        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post"), TestCategory("Anon")]
        public async Task CreateSubmission_NonNSFW_InNSFWSub()
        {
            string userName = "TestUser14";
            var user = TestHelper.SetPrincipal(userName);

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "NSFW", Title = "This is a title", Url = "http://www.yahoo.com", IsAdult = false }).SetUserContext(user);

            var r = await cmd.Execute();

            VoatAssert.IsValid(r);
            Assert.IsNotNull(r.Response, "Expecting a non null response");
            Assert.AreNotEqual(0, r.Response.ID, "Expected a valid ID");
            Assert.AreEqual(true, r.Response.IsAdult);

        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post"), TestCategory("Anon")]
        public async Task CreateSubmission_NSFW_TitleOnly()
        {
            string userName = "TestUser15";
            var user = TestHelper.SetPrincipal(userName);

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "This is a title [nSfW]", Url = "http://www.yahoo.com/someotherurl/", IsAdult = false }).SetUserContext(user);

            var r = await cmd.Execute();

            VoatAssert.IsValid(r);
            Assert.IsNotNull(r.Response, "Expecting a non null response");
            Assert.AreNotEqual(0, r.Response.ID, "Expected a valid ID");
            Assert.AreEqual(true, r.Response.IsAdult);

        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public async Task DeleteSubmission_Owner()
        {
            var user = TestHelper.SetPrincipal("TestUser12");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "This is a title", Content = "This is content in this test" }).SetUserContext(user);

            var r = await cmd.Execute();

            VoatAssert.IsValid(r);
            Assert.IsNotNull(r.Response, "Expecting a non null response");
            Assert.AreNotEqual(0, r.Response.ID);

            var d = new DeleteSubmissionCommand(r.Response.ID).SetUserContext(user);
            var r2 = await d.Execute();

            VoatAssert.IsValid(r2);

            //verify
            using (var db = new Voat.Data.Repository(user))
            {
                var s = db.GetSubmission(r.Response.ID);
                Assert.AreEqual(true, s.IsDeleted);

                //Content should remain unchanged in mod deletion
                //Assert.AreEqual(s.Content, r.Response.Content);
                //Assert.AreEqual(s.FormattedContent, r.Response.FormattedContent);
                Assert.IsTrue(s.Content.StartsWith("Deleted by"));
                Assert.AreEqual(s.FormattedContent, Utilities.Formatting.FormatMessage(s.Content));
            }

        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public async Task DeleteSubmission_Moderator()
        {
            var user = TestHelper.SetPrincipal("TestUser13");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "This is a title", Content = "This is content a mod would hate" }).SetUserContext(user);

            var r = await cmd.Execute();
            VoatAssert.IsValid(r);
            Assert.IsNotNull(r.Response, "Expecting a non null response");
            Assert.AreNotEqual(0, r.Response.ID);

            user = TestHelper.SetPrincipal(USERNAMES.Unit);
            var d = new DeleteSubmissionCommand(r.Response.ID, "This is content I hate").SetUserContext(user);
            var r2 = await d.Execute();
            VoatAssert.IsValid(r2);

            //verify
            using (var db = new Voat.Data.Repository(user))
            {
                var s = db.GetSubmission(r.Response.ID);
                Assert.AreEqual(true, s.IsDeleted);

                //Content should remain unchanged in mod deletion
                Assert.AreEqual(s.Content, r.Response.Content);
                Assert.AreEqual(s.FormattedContent, r.Response.FormattedContent);
                //Assert.IsTrue(s.Content.StartsWith("Deleted by"));
                //Assert.AreEqual(s.FormattedContent, Utilities.Formatting.FormatMessage(s.Content));
            }

        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]

        public void Edit_Submission_Title_Content()
        {
            var user = TestHelper.SetPrincipal("anon");

            var x = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Anon, Title = "xxxxxxxxxxxx", Content = "xxxxxxxxxxxx" }).SetUserContext(user);
            var s = x.Execute().Result;

            Assert.IsNotNull(s, "Response is null");
            Assert.IsTrue(s.Success, s.Message);

            var cmd = new EditSubmissionCommand(s.Response.ID, new Domain.Models.UserSubmission() { Title = "yyyyyyyyyyyy", Content = "yyyyyyyyyyyy" }).SetUserContext(user);
            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r, message: "Edit Submission failed to return true: " + r.Message);

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
            var user = TestHelper.SetPrincipal("TestUser02");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Whatever, Title = "http://www.yahoo.com", Url = "http://www.yahoo.com" }).SetUserContext(user);

            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r, Status.Denied);
            Assert.AreEqual(r.Message, "Submission title may not be the same as the URL you are trying to submit. Why would you even think about doing this?! Why?");
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post"), TestCategory("Bug")]
        public void PreventUrlTitlePosts_BugTrap()
        {
            //BUGFIX: https://voat.co/v/FGC/1349484
            var user = TestHelper.SetPrincipal("TestUser02");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Whatever, Title = "http://www.besthealthmarket.org/cianix-male-enhancement/", Content = "Cianix is the best available product in the market. It has long lasting anti-aging effects. It made up of natural products and has no side effects. Therefore it is a highly recommended  product for anti-aging as well as healthy skin. Signs of Healthy Skin We talk a lot about healthy skin and methods of achieving healthy skin. But what healthy skin actually is only a few of us know it. Here are few points that a healthy skin has: Even color:" }).SetUserContext(user);

            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r, Status.Denied);
            Assert.AreEqual(r.Message, "Submission title is a url? Why would you even think about doing this?! Why?");
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void PreventPartialUrlTitlePosts()
        {
            var user = TestHelper.SetPrincipal("TestUser02");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Whatever, Title = "www.yahoo.com", Url = "http://www.yahoo.com" }).SetUserContext(user);
            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r, Status.Denied);
            Assert.AreEqual(r.Message, "Submission title may not be the same as the URL you are trying to submit. Why would you even think about doing this?! Why?");
            //Assert.AreNotEqual(0, r.Response.ID);
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void PreventPartialUrlTitlePosts_Bug()
        {
            var user = TestHelper.SetPrincipal("TestUser09");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Whatever, Title = "speedtesting", Url = "http://beta.speedtesting.net/" }).SetUserContext(user);
            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r, Status.Success);
            //Assert.AreNotEqual(0, r.Response.ID);
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void LongUrl_Bug()
        {
            var user = TestHelper.SetPrincipal("TestUser09");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Whatever, Title = "Long Url Bug", Url = "http://kkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkk.com/" }).SetUserContext(user);
            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r, Status.Success);
            //Assert.AreNotEqual(0, r.Response.ID);
        }
        
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void PreventInvalidUrlTitlePosts()
        {
            var user = TestHelper.SetPrincipal("TestUser07");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Whatever, Title = "Super rad website", Url = "http//www.yahoo.com" }).SetUserContext(user);

            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r, Status.Invalid);
            Assert.AreEqual("The url you are trying to submit is invalid", r.Message);
            //Assert.AreNotEqual(0, r.Response.ID);
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]

        public void PreventNoSubversePost()
        {
            var user = TestHelper.SetPrincipal("TestUser02");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "", Title = "Hello Man", Url = "http://www.yahoo.com" }).SetUserContext(user);

            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, "Expecting a false response");
            Assert.AreEqual("A subverse must be provided", r.Message);
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        [TestCategory("Ban"), TestCategory("Ban.Domain")]
        public void PreventBannedDomainPost()
        {
            using (var repo = new VoatDataContext())
            {
                repo.BannedDomain.Add(new BannedDomain() { Domain = "saiddit.com", Reason = "No one really likes you.", CreatedBy = "UnitTest", CreationDate = DateTime.UtcNow });
                repo.SaveChanges(); 
            }

            var user = TestHelper.SetPrincipal("TestUser02");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "Hello Man - Longer because of Rules", Url = "http://www.saiddit.com/images/feelsgoodman.jpg" }).SetUserContext(user);
            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "Submission contains banned domains");

            cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "Hello Man - Longer because of Rules", Content = "Check out this cool image I found using dogpile.com: http://saiddit.com/images/feelsgoodman.jpg" }).SetUserContext(user);
            r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "Submission contains banned domains");

            cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "Test Embedded Banned Domain",
                Content = "http://yahoo.com/index.html Check out this cool image I found using compuserve.com: http://saiddit.com/images/feelsgoodman.jpg. http://www2.home.geocities.com/index.html"
            }).SetUserContext(user);
            r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "Submission contains banned domains");

            //Test URLEncoding Markdown Matching
            cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit,
                Title = "Hello Man - Longer because of Rules",
                Content = "[https://www.some-fake-domain.com/surl/start](https://www.google.com/url?q=http%3A%2F%2Fww2.saiddit.com%2F%3Flnk%26keyword%3Dsome%2Bkey%2Bword%26charset%3Dutf-8)"
            }).SetUserContext(user);
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
            using (var repo = new VoatDataContext())
            {
                repo.BannedDomain.Add(new BannedDomain() { Domain = "one.two.three.com", Reason = "People hate counting", CreatedBy = "UnitTest", CreationDate = DateTime.UtcNow });
                repo.SaveChanges();
            }

            var user = TestHelper.SetPrincipal("TestUser02");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "Hello Man - Longer because of Rules", Url = "http://www.one.two.three.com/images/feelsgoodman.jpg" }).SetUserContext(user);
            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "Submission contains banned domains");

            cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "Hello Man - Longer because of Rules", Content = "Check out this cool image I found using dogpile.com: HTTP://one.TWO.three.com/images/feelsgoodman.jpg" }).SetUserContext(user);
            r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "Submission contains banned domains");

        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public async Task PreventSubmittingSameLinkTwice()
        {
            var user = TestHelper.SetPrincipal("UnitTestUser30");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "My name is Fuzzy and my fixes are way cooler than puttputts", Url = "http://i.deleted.this.by/accident.jpg" }).SetUserContext(user);
            var r = await cmd.Execute();
            Assert.IsNotNull(r, "Response was null");
            Assert.IsTrue(r.Success, r.Message);
            var id = r.Response.ID;

            cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "Hello Man - Longer because of Rules", Url = "http://i.deleted.this.by/accident.jpg" }).SetUserContext(user);
            r = await cmd.Execute();
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, $"Sorry, this link has already been submitted recently. {(VoatSettings.Instance.ForceHTTPS ? "https" : "http")}://{VoatSettings.Instance.SiteDomain}/v/unit/" + id);
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public async Task AllowSubmittingDeletedLink()
        {
            var user = TestHelper.SetPrincipal("UnitTestUser31");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "I need to think of better titles but I don't want to get in trouble", Url = "http://i.deleted.this.on/purpose.jpg" }).SetUserContext(user);
            var r = await cmd.Execute();
            VoatAssert.IsValid(r);

            var id = r.Response.ID;
            var cmd2 = new DeleteSubmissionCommand(id).SetUserContext(user);
            var r2 = await cmd2.Execute();
            VoatAssert.IsValid(r);

            cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "Hello Man - Longer because of Rules", Url = "http://i.deleted.this.on/purpose.jpg" }).SetUserContext(user);
            r = await cmd.Execute();
            VoatAssert.IsValid(r);
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void PreventShortTitlePosts()
        {
            var user = TestHelper.SetPrincipal("TestUser02");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "What", Url = "http://www.hellogoodbye.com/images/feelsgoodman.jpg" }).SetUserContext(user);
            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r, Status.Invalid);
            Assert.AreEqual(r.Message, $"A title must be between {VoatSettings.Instance.MinimumTitleLength} and {VoatSettings.Instance.MaximumTitleLength} characters");
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        [TestCategory("Ban"), TestCategory("Ban.User")]
        public void PreventGlobalBannedUsers()
        {
            var user = TestHelper.SetPrincipal("BannedGlobally");
            var userSubmission = new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = Guid.NewGuid().ToString(), Url = "http://www.SendhelpImStuckInUnitTests.com/images/feelsgoodman.jpg" };
            var cmd = new CreateSubmissionCommand(userSubmission).SetUserContext(user);
            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r, Status.Denied);
            Assert.AreEqual(r.Message, "User is globally banned");
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        [TestCategory("Ban"), TestCategory("Ban.User")]
        public void PreventSubverseBannedUsers()
        {
            var user = TestHelper.SetPrincipal("BannedFromVUnit");
            var userSubmission = new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = Guid.NewGuid().ToString(), Url = "http://www.SuperAwesomeDomainName.com/images/feelsgoodman.jpg" };
            var cmd = new CreateSubmissionCommand(userSubmission).SetUserContext(user);
            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r, Status.Denied);
            Assert.AreEqual(r.Message, $"User is banned from v/{userSubmission.Subverse}");
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void PreventUserFromPostingToAuthorizedOnlySubverses()
        {
            var user = TestHelper.SetPrincipal("TestUser09");
            var userSubmission = new Domain.Models.UserSubmission() { Subverse = SUBVERSES.AuthorizedOnly, Title = Guid.NewGuid().ToString(), Url = "http://www.digit.com/images/feelsgoodman.jpg" };
            var cmd = new CreateSubmissionCommand(userSubmission).SetUserContext(user);
            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r, Status.Denied);
            Assert.AreEqual(r.Message, "You are not authorized to submit links or start discussions in this subverse. Please contact subverse moderators for authorization");

        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void PreventUserFromPostingCompromisedTitle1()
        {
            var user = TestHelper.SetPrincipal("TestUser01");
            var userSubmission = new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Whatever, Title = "\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000", Content = "cookies" };
            var cmd = new CreateSubmissionCommand(userSubmission).SetUserContext(user);
            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r, Status.Denied);
            Assert.AreEqual(r.Message, "Submission title can not contain Unicode or unprintable characters");
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void PreventPostingToDisabledSub()
        {
            var user = TestHelper.SetPrincipal("TestUser06");
            var userSubmission = new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Disabled, Title = "I am not paying attention", Content = "Why was this sub disabled?" };
            var cmd = new CreateSubmissionCommand(userSubmission).SetUserContext(user);
            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r, Status.Denied);
            Assert.AreEqual("Subverse is disabled", r.Message);
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        public void TestNegativeSCPSubmission()
        {
            var userName = "NegativeSCP";
            //Create user
            TestDataInitializer.CreateUser(userName, DateTime.UtcNow.AddDays(-450));
            //Add submission with negatives directly to db
            using (var context = new VoatDataContext())
            {
                
                var s = context.Submission.Add(new Submission()
                {
                    CreationDate = DateTime.UtcNow.AddHours(-12),
                    Subverse = SUBVERSES.Unit,
                    Title = "Test Negative SCP",
                    Url = "https://www.youtube.com/watch?v=pnbJEg9r1o8",
                    Type = 2,
                    UpCount = 2,
                    DownCount = 13,
                    UserName = userName
                });
                context.SaveChanges();
            }



            var user = TestHelper.SetPrincipal(userName);
            var userSubmission = new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "Le Censorship!", Content = "Will this work?" };
            var cmd = new CreateSubmissionCommand(userSubmission).SetUserContext(user);
            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r);

        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        [TestCategory("Ban"), TestCategory("Ban.Domain")]
        public async Task PreventBannedDomainPost_MultiPartDomains_Excessive_Bug()
        {
            //using (var repo = new VoatDataContext())
            //{
            //    repo.BannedDomain.Add(new BannedDomain() { Domain = "a.a.a.co", Reason = "FUZZY!", CreatedBy = "UnitTest", CreationDate = DateTime.UtcNow });
            //    repo.SaveChanges();
            //}
            var user = TestHelper.SetPrincipal("TestUser19");

            string url;
            CreateSubmissionCommand cmd;
            CommandResponse<Domain.Models.Submission> r;

            url = "https://this.will.break.url.com/a?&b=***&c=1234567890-&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?&=*?.,\0\0\0^$";
            cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "Hello Man - Longer because of Rules", Url = url }).SetUserContext(user);
            r = await cmd.Execute();
            VoatAssert.IsValid(r, Status.Denied);

            url = "http://a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.a.co/#?";
            cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "Hello Man - Longer because of Rules", Url = url }).SetUserContext(user);
            r = await cmd.Execute();
            VoatAssert.IsValid(r);

            cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "Hello Man - Longer because of Rules", Content = $"Check out this cool image I found using dogpile.com: {url}" }).SetUserContext(user);
            r = await cmd.Execute();
            VoatAssert.IsValid(r);
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        [TestCategory("Validation"), TestCategory("Submission.Validation")]
        public async Task Submission_Length_Validations()
        {
            var user = TestHelper.SetPrincipal("TestUser20");

            var createCmd = new CreateSubmissionCommand(
                new Domain.Models.UserSubmission() {
                    Subverse = SUBVERSES.Unit,
                    Title = "Can you hear me now?".RepeatUntil(201),
                    Content = null
                }).SetUserContext(user);
            var r = await createCmd.Execute();
            VoatAssert.IsValid(r, Status.Invalid);

            createCmd = new CreateSubmissionCommand(
                new Domain.Models.UserSubmission()
                {
                    Subverse = SUBVERSES.Unit,
                    Title = "Can you hear me now?",
                    Content = "Great!".RepeatUntil(10001)
                }).SetUserContext(user);
            r = await createCmd.Execute();
            VoatAssert.IsValid(r, Status.Invalid);
            Assert.IsTrue(r.ValidationErrors.Any(x => x.MemberNames.Any(m => m.IsEqual("content"))));

            createCmd = new CreateSubmissionCommand(
                new Domain.Models.UserSubmission()
                {
                    Subverse = SUBVERSES.Unit,
                    Title = "Can you hear me now?",
                    Content = "Great!"
                }).SetUserContext(user);
            r = await createCmd.Execute();
            VoatAssert.IsValid(r);

            var editCmd = new EditSubmissionCommand(r.Response.ID, new Domain.Models.UserSubmission()
            {
                Subverse = SUBVERSES.Unit,
                Title = "Can you hear me now?",
                Content = "Great!".RepeatUntil(10001),
            }).SetUserContext(user);
            r = await editCmd.Execute();
            VoatAssert.IsValid(r, Status.Invalid);

            //editCmd = new EditSubmissionCommand(r.Response.ID, new Domain.Models.UserSubmission()
            //{
            //    Subverse = SUBVERSES.Unit,
            //    Title = "Can you hear me now?",
            //    Content = "Great!".RepeatUntil(10001),
            //}).SetUserContext(user);
            //r = await createCmd.Execute();
            //VoatAssert.IsValid(r, Status.Denied);

        }
    }
}
