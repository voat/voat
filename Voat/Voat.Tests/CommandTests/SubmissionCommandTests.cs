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
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Tests.Repository;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class SubmissionCommandTests 
    {
        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Submission")]
        [TestCategory("Command.Submission.Post")]
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
        [TestCategory("Command")]
        [TestCategory("Submission")]
        [TestCategory("Command.Submission.Post")]
        public void CreateAnonSubmission()
        {
            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "anon", Title = "This is a title", Url = "http://www.yahoo.com" });

            var r = cmd.Execute().Result;

            Assert.IsNotNull(r, "Response is null");
            Assert.IsTrue(r.Success, r.Message);
            Assert.IsNotNull(r.Response, "Expecting a non null response");
            Assert.AreNotEqual(0, r.Response.ID, "Expected a valid ID");
            Assert.AreNotEqual("TestUser2", r.Response.UserName);
            Assert.AreEqual(true, r.Response.IsAnonymized);

        }
        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Submission")]
        [TestCategory("Command.Submission.Post")]
        public void DeleteSubmission()
        {
            TestHelper.SetPrincipal("anon");

            var cmd = new DeleteSubmissionCommand(1);
            var r = cmd.Execute().Result;

            Assert.IsTrue(r.Success);
            //Assert.Inconclusive();
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Submission")]
        [TestCategory("Command.Submission.Post")]

        public void Edit_Submission_Title_Content()
        {
            TestHelper.SetPrincipal("anon");

            var x = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "anon", Title = "xxxxxx", Content = "xxxxxx" });
            var s = x.Execute().Result;

            Assert.IsNotNull(s, "Response is null");
            Assert.IsTrue(s.Success, s.Message);

            var cmd = new EditSubmissionCommand(s.Response.ID, new Domain.Models.UserSubmission() { Title = "yyyyyy", Content = "yyyyyy" });
            var r = cmd.Execute().Result;

            Assert.IsNotNull(r, "Response is null");
            Assert.IsTrue(r.Success, "Edit Submission failed to return true: " + r.Message);

            using (var repo = new Voat.Data.Repository())
            {
                var submission = repo.GetSubmission(s.Response.ID);
                Assert.IsNotNull(submission, "Can't find submission from repo");
                Assert.AreEqual("yyyyyy", submission.Title);
                Assert.AreEqual("yyyyyy", submission.Content);
            }

            //Assert.Inconclusive();
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Submission")]
        [TestCategory("Command.Submission.Post")]
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
        [TestCategory("Command")]
        [TestCategory("Submission")]
        [TestCategory("Command.Submission.Post")]
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
        [TestCategory("Command")]
        [TestCategory("Submission")]
        [TestCategory("Command.Submission.Post")]
        public void PreventInvalidUrlTitlePosts()
        {
            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "whatever", Title = "Super rad website", Url = "http//www.yahoo.com" });

            var r = cmd.Execute().Result;
            Assert.IsFalse(r.Success);
            Assert.AreEqual(r.Message, "The url you are trying to submit is invalid");
            //Assert.AreNotEqual(0, r.Response.ID);
        }
        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Submission")]
        [TestCategory("Command.Submission.Post")]

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
        [TestCategory("Command")]
        [TestCategory("Submission")]
        [TestCategory("Command.Submission.Post")]

        public void PreventBannedDomainPost()
        {
            using (var repo = new voatEntities())
            {
                repo.BannedDomains.Add(new BannedDomain() { Domain = "saiddit.com", Reason = "No one really likes you.", CreatedBy = "UnitTest", CreationDate = DateTime.UtcNow });
                repo.SaveChanges(); 
            }

            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "unit", Title = "Hello Man", Url = "http://www.saiddit.com/images/feelsgoodman.jpg" });
            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "Submission contains banned domains");

            cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "unit", Title = "Hello Man", Content = "Check out this cool image I found using dogpile.com: http://saiddit.com/images/feelsgoodman.jpg" });
            r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "Submission contains banned domains");

        }

        public void PreventShortTitlePosts()
        {
            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = "unit", Title = "What", Url = "http://www.saiddit.com/images/feelsgoodman.jpg" });
            var r = cmd.Execute().Result;
            Assert.IsNotNull(r, "Response was null");
            Assert.IsFalse(r.Success, r.Message);
            Assert.AreEqual(r.Message, "A title may not be less than 5 characters");


        }
    }
}
