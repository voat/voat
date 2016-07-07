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
using System.Linq;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Tests.Repository;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class CommentCommandTests 
    {
        #region Anon Comment Tests

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public void CreateComment_Anon()
        {
            TestHelper.SetPrincipal("TestUser1");

            var cmd = new CreateCommentCommand(2, null, "This is my data");
            var c = cmd.Execute().Result;

            Assert.IsTrue(c.Success);
            Assert.IsNotNull(c.Response);
            Assert.AreNotEqual(0, c.Response.ID);
            Assert.AreEqual(true, c.Response.IsAnonymized);
            Assert.AreNotEqual(cmd.Content, c.Response.FormattedContent);

            //verify in db
            using (var db = new Voat.Data.Repository())
            {
                var comment = db.GetComment(c.Response.ID);
                Assert.IsNotNull(comment, "Couldn't find comment in db", c.Response.ID);

                Assert.AreEqual(c.Response.ID, comment.ID);
                Assert.AreEqual(comment.ID.ToString(), comment.UserName);
                Assert.AreEqual(c.Response.Content, comment.Content);
                Assert.IsTrue(comment.IsAnonymized);
                Assert.AreEqual(c.Response.IsAnonymized, comment.IsAnonymized);
            }
        }

        #endregion Anon Comment Tests

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public void CreateComment()
        {
            TestHelper.SetPrincipal("TestUser2");

            var cmd = new CreateCommentCommand(1, null, "This is my data");
            var c = cmd.Execute().Result;

            Assert.IsTrue(c.Success);
            Assert.IsNotNull(c.Response);
            Assert.AreNotEqual(0, c.Response.ID);

            //verify in db
            using (var db = new Voat.Data.Repository())
            {
                var comment = db.GetComment(c.Response.ID);
                Assert.IsNotNull(comment, "Couldn't find comment in db", c.Response.ID);
                Assert.AreEqual(c.Response.ID, comment.ID);
                Assert.AreEqual(c.Response.UserName, comment.UserName);
                Assert.AreEqual(c.Response.Content, comment.Content);
            }
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public void CreateComment_WithBannedSubUser()
        {
            TestHelper.SetPrincipal("BannedFromVUnit");

            var cmd = new CreateCommentCommand(1, null, "This is my data with banned user");
            var c = cmd.Execute().Result;

            Assert.IsFalse(c.Success, "User should be banned from commenting");
            Assert.AreEqual(Status.Denied, c.Status);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public void CreateComment_WithGloballyBannedUser()
        {
            TestHelper.SetPrincipal("BannedGlobally");

            var cmd = new CreateCommentCommand(1, null, "This is my data");
            var c = cmd.Execute().Result;

            Assert.IsFalse(c.Success, "User should be banned from commenting");
            Assert.AreEqual(Status.Denied, c.Status);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public void DeleteComment()
        {
            TestHelper.SetPrincipal("TestUser1");
            var cmdcreate = new CreateCommentCommand(1, null, "This is my data too you know");
            var c = cmdcreate.Execute().Result;

            Assert.IsNotNull(c, "response null");
            Assert.IsTrue(c.Success, c.Message);
            Assert.IsNotNull(c.Response, "Response payload null");

            int id = c.Response.ID;

            var cmd = new DeleteCommentCommand(id);
            var r = cmd.Execute().Result;
            Assert.IsTrue(r.Success);

            //verify
            using (var db = new Voat.Data.Repository())
            {
                var comment = db.GetComment(id);
                Assert.AreEqual(true, comment.IsDeleted);
                Assert.AreNotEqual(c.Response.Content, comment.Content);
            }
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public void EditComment()
        {
            TestHelper.SetPrincipal("unit");
            var cmd = new EditCommentCommand(1, "This is data [howdy](http://www.howdy.com)");
            var r = cmd.Execute().Result;
            Assert.IsTrue(r.Success);

            //verify
            using (var db = new Voat.Data.Repository())
            {
                var comment = db.GetComment(1);
                Assert.IsNotNull(comment.LastEditDate);
                Assert.AreEqual(cmd.Content, comment.Content);
            }
        }
        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public void CreateComment_DisabledSubverse()
        {
            //insert post via db into disabled sub
            Submission submission = null;
            using (var db = new voatEntities())
            {
                submission = new Submission() {
                    Subverse = "Disabled",
                    Title = "Super Sneaky",
                    Content = "How can I post to disabled subs?",
                    UserName = "unit",
                    CreationDate = DateTime.UtcNow
                };
                db.Submissions.Add(submission);
                db.SaveChanges();
            }

            TestHelper.SetPrincipal("TestUser5");
            var cmd = new CreateCommentCommand(submission.ID, null, "Are you @FuzzyWords?");
            var c = cmd.Execute().Result;

            Assert.IsFalse(c.Success, "Disabled subs should not allow comments");
            Assert.AreEqual(Status.Denied, c.Status);
            Assert.AreEqual(c.Message, "Subverse is disabled");

        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Comment"), TestCategory("Comment.Post"), TestCategory("Notifications")]
        public async Task CreateComment_TestCommentReplyNotification()
        {

            var userName = "UnitTestUser18";
            TestHelper.SetPrincipal(userName);
            var body = Guid.NewGuid().ToString();
            var cmd = new CreateCommentCommand(1, 2, body);
            var c = await cmd.Execute();
            Assert.IsNotNull(c, "response null");
            if (!c.Success)
            {
                if (c.Exception != null)
                {
                    Assert.Fail(c.Exception.ToString());
                }
                else
                {
                    Assert.Fail(c.Message);
                }
            }
            Assert.AreEqual(Status.Success, c.Status);

            //check for comment reply entry
            using (var db = new voatEntities())
            {
                var notice = db.CommentReplyNotifications.FirstOrDefault(x => x.Sender == userName && x.Recipient == "unit" && x.SubmissionID == 1 && x.CommentID == c.Response.ID && x.Body == body);
                Assert.IsNotNull(notice, "Did not find a reply notification");
            }
        }
        [TestMethod]
        [TestCategory("Command"),TestCategory("Comment"),TestCategory("Comment.Post"), TestCategory("Notifications")]
        public async Task CreateComment_TestSubmissionCommentNotification()
        {
            var userName = "UnitTestUser19";
            TestHelper.SetPrincipal(userName);
            var body = Guid.NewGuid().ToString();
            var cmd = new CreateCommentCommand(1, null, body);
            var c = await cmd.Execute();
            Assert.IsNotNull(c, "response null");
            if (!c.Success)
            {
                if (c.Exception != null)
                {
                    Assert.Fail(c.Exception.ToString());
                }
                else
                {
                    Assert.Fail(c.Message);
                }
            }
            Assert.AreEqual(Status.Success, c.Status);

            //check for comment reply entry
            using (var db = new voatEntities())
            {
                var notice = db.SubmissionReplyNotifications.FirstOrDefault(x => x.Sender == userName && x.Recipient == "anon" && x.SubmissionID == 1 && x.Body == body);
                Assert.IsNotNull(notice, "Did not find a reply notification");
            }
        }
    }
}
