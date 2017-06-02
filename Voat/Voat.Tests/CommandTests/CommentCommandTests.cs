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
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Query;
using Voat.Tests.Repository;
using Voat.Utilities;

namespace Voat.Tests.CommandTests
{
   


    [TestClass]
    public class CommentCommandTests : BaseCommandTest
    {
        #region Anon Comment Tests

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        [TestCategory("Anon")]
        public async Task CreateComment_Anon()
        {
            string userName = "TestUser01";
            TestHelper.SetPrincipal(userName);

            var cmd = new CreateCommentCommand(2, null, "This is my data");
            var c = cmd.Execute().Result;

            VoatAssert.IsValid(c);
            Assert.AreNotEqual(0, c.Response.ID);
            Assert.AreEqual(true, c.Response.IsAnonymized);
            Assert.AreNotEqual(cmd.Content, c.Response.FormattedContent);

            //verify in db
            using (var db = new Voat.Data.Repository())
            {
                var comment = await db.GetComment(c.Response.ID);
                Assert.IsNotNull(comment, "Couldn't find comment in db", c.Response.ID);

                Assert.AreEqual(c.Response.ID, comment.ID);
                Assert.AreEqual(comment.ID.ToString(), comment.UserName);
                Assert.AreEqual(c.Response.Content, comment.Content);
                Assert.IsTrue(comment.IsAnonymized);
                Assert.AreEqual(c.Response.IsAnonymized, comment.IsAnonymized);
            }

            base.VerifyCommentIsProtected(c.Response.SubmissionID.Value, c.Response.ID, userName);

        }



        #endregion Anon Comment Tests

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public async Task CreateComment()
        {
            TestHelper.SetPrincipal("TestUser02");

            var cmd = new CreateCommentCommand(1, null, "This is my data");
            var c = await cmd.Execute();

            VoatAssert.IsValid(c);
            Assert.AreNotEqual(0, c.Response.ID);

            //verify in db
            using (var db = new Voat.Data.Repository())
            {
                var comment = await db.GetComment(c.Response.ID);
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
        public void CreateComment_Empty()
        {
            TestHelper.SetPrincipal("TestUser12");

            var cmd = new CreateCommentCommand(1, null, "          ");
            var c = cmd.Execute().Result;

            VoatAssert.IsValid(c, Status.Denied);
            Assert.AreEqual("Empty comments not allowed", c.Message);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public void EditComment_Empty()
        {
            TestHelper.SetPrincipal("TestUser11");

            var cmd = new CreateCommentCommand(1, null, "This is a unit test and I like it.");
            var c = cmd.Execute().Result;

            VoatAssert.IsValid(c);

            var editCmd = new EditCommentCommand(c.Response.ID, "            ");
            var editResult = editCmd.Execute().Result;

            Assert.IsFalse(editResult.Success, editResult.Message);
            Assert.AreEqual("Empty comments not allowed", editResult.Message);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public void EditComment_WrongOwner()
        {
            TestHelper.SetPrincipal("TestUser15");

            var cmd = new CreateCommentCommand(1, null, "This is a unit test and I like it.");
            var c = cmd.Execute().Result;

            VoatAssert.IsValid(c);

            TestHelper.SetPrincipal("TestUser12");
            var editCmd = new EditCommentCommand(c.Response.ID, "All your comment are belong to us!");
            var editResult = editCmd.Execute().Result;

            VoatAssert.IsValid(editResult, Status.Denied);
            Assert.AreEqual("User doesn't have permissions to perform requested action", editResult.Message);
        }



        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        [TestCategory("Ban"), TestCategory("Ban.Domain")]
        public void CreateComment_BannedDomain()
        {
            TestHelper.SetPrincipal("TestUser02");

            var cmd = new CreateCommentCommand(1, null, "[Check out this killer website](http://fleddit.com/f/3hen3k/Look_at_this_cat_just_Looook_awww)!");
            var c = cmd.Execute().Result;

            VoatAssert.IsValid(c, Status.Denied);
            Assert.AreEqual("Comment contains banned domains", c.Message);

        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        [TestCategory("Ban"), TestCategory("Ban.Domain")]
        public void CreateComment_BannedDomain_NoProtocol()
        {
            TestHelper.SetPrincipal("TestUser02");

            var cmd = new CreateCommentCommand(1, null, "[Check out this killer website](//fleddit.com/f/3hen3k/Look_at_this_cat_just_Looook_awww)!");
            var c = cmd.Execute().Result;

            VoatAssert.IsValid(c, Status.Denied);
            Assert.AreEqual("Comment contains banned domains", c.Message);

        }
        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        [TestCategory("Ban"), TestCategory("Ban.Domain")]
        public void EditComment_BannedDomain()
        {
            TestHelper.SetPrincipal("TestUser02");

            var cmd = new CreateCommentCommand(1, null, "This is a unit test and I like it.");
            var c = cmd.Execute().Result;

            VoatAssert.IsValid(c);

            var editCmd = new EditCommentCommand(c.Response.ID, "[Check out this killer website](http://fleddit.com/f/3hen3k/Look_at_this_cat_just_Looook_awww)!");
            var editResult = editCmd.Execute().Result;
            VoatAssert.IsValid(c, Status.Denied, "Expecting Denied Status");
            Assert.AreEqual("Comment contains banned domains", editResult.Message);
        }



        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        [TestCategory("Ban"), TestCategory("Ban.User")]
        public void CreateComment_WithBannedSubUser()
        {
            TestHelper.SetPrincipal("BannedFromVUnit");

            var cmd = new CreateCommentCommand(1, null, "This is my data with banned user");
            var c = cmd.Execute().Result;

            VoatAssert.IsValid(c, Status.Denied, "User should be banned from commenting");
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        [TestCategory("Ban"), TestCategory("Ban.User")]
        public void CreateComment_WithGloballyBannedUser()
        {
            TestHelper.SetPrincipal("BannedGlobally");

            var cmd = new CreateCommentCommand(1, null, "This is my data");
            var c = cmd.Execute().Result;

            VoatAssert.IsValid(c, Status.Denied, "User should be banned from commenting");
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public void DeleteComment_Owner()
        {
            //Assert.Inconclusive("Complete this test");

            TestHelper.SetPrincipal("TestUser01");
            var cmdcreate = new CreateCommentCommand(1, null, "This is my data too you know");
            var c = cmdcreate.Execute().Result;

            VoatAssert.IsValid(c);

            int id = c.Response.ID;

            var cmd = new DeleteCommentCommand(id);
            var r = cmd.Execute().Result;
            VoatAssert.IsValid(r);

            //verify
            using (var db = new VoatDataContext())
            {
                var comment = db.Comment.FirstOrDefault(x => x.ID == id);
                Assert.AreEqual(true, comment.IsDeleted);
                Assert.AreNotEqual(c.Response.Content, comment.Content);

                //Ensure content is replaced in moderator deletion
                Assert.IsTrue(comment.Content.StartsWith("Deleted by"));
                Assert.AreEqual(comment.FormattedContent, Formatting.FormatMessage(comment.Content));

            }
        }
        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public async Task DeleteComment_Moderator()
        {
            //Assert.Inconclusive("Complete this test");
            var content = "This is my data too you know 2";
            TestHelper.SetPrincipal("TestUser01");
            var cmdcreate = new CreateCommentCommand(1, null, content);
            var c = cmdcreate.Execute().Result;

            VoatAssert.IsValid(c);

            int id = c.Response.ID;

            //switch to mod of sub
            TestHelper.SetPrincipal("unit");
            var cmd = new DeleteCommentCommand(id, "This is spam");
            var r = await cmd.Execute();
            VoatAssert.IsValid(r);

            //verify
            using (var db = new VoatDataContext())
            {
                var comment = db.Comment.FirstOrDefault(x => x.ID == id);
                Assert.AreEqual(true, comment.IsDeleted);
                
                //Content should remain unchanged in mod deletion
                Assert.AreEqual(comment.Content, content);
                Assert.AreEqual(comment.FormattedContent, Formatting.FormatMessage(content));

            }
        }
        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        public async Task EditComment()
        {
            string content = "This is data [howdy](http://www.howdy.com)";
            TestHelper.SetPrincipal("unit");
            var cmd = new EditCommentCommand(1, content);
            var r = await cmd.Execute();

            VoatAssert.IsValid(r);
            Assert.AreEqual(content, r.Response.Content);
            Assert.AreEqual(Formatting.FormatMessage(content), r.Response.FormattedContent);

            //verify
            using (var db = new Voat.Data.Repository())
            {
                var comment = await db.GetComment(1);
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
            using (var db = new VoatDataContext())
            {
                submission = new Submission() {
                    Subverse = "Disabled",
                    Title = "Super Sneaky",
                    Content = "How can I post to disabled subs?",
                    UserName = "unit",
                    CreationDate = DateTime.UtcNow
                };
                db.Submission.Add(submission);
                db.SaveChanges();
            }

            TestHelper.SetPrincipal("TestUser05");
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
            using (var db = new VoatDataContext())
            {
                var notice = db.Message.FirstOrDefault(x => x.Sender == userName && x.Recipient == "unit" && x.SubmissionID == 1 && x.CommentID == c.Response.ID);
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

            VoatAssert.IsValid(c);

            //check for comment reply entry
            using (var db = new VoatDataContext())
            {
                var notice = db.Message.FirstOrDefault(x => x.Sender == userName && x.Recipient == "anon" && x.SubmissionID == 1);
                Assert.IsNotNull(notice, "Did not find a reply notification");
            }
        }
    }
}
