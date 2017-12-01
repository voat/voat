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
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Query;
using Voat.Tests.Infrastructure;
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
            var user = TestHelper.SetPrincipal(userName);

            var cmd = new CreateCommentCommand(2, null, "This is my data").SetUserContext(user);
            var c = cmd.Execute().Result;

            VoatAssert.IsValid(c);
            Assert.AreNotEqual(0, c.Response.ID);
            Assert.AreEqual(true, c.Response.IsAnonymized);
            Assert.AreNotEqual(cmd.Content, c.Response.FormattedContent);

            //verify in db
            using (var db = new Voat.Data.Repository(user))
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
            var user = TestHelper.SetPrincipal("TestUser02");

            var cmd = new CreateCommentCommand(1, null, "This is my data").SetUserContext(user);
            var c = await cmd.Execute();

            VoatAssert.IsValid(c);
            Assert.AreNotEqual(0, c.Response.ID);

            //verify in db
            using (var db = new Voat.Data.Repository(user))
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
            var user = TestHelper.SetPrincipal("TestUser12");

            var cmd = new CreateCommentCommand(1, null, "          ").SetUserContext(user);
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
            var user = TestHelper.SetPrincipal("TestUser11");

            var cmd = new CreateCommentCommand(1, null, "This is a unit test and I like it.").SetUserContext(user);
            var c = cmd.Execute().Result;

            VoatAssert.IsValid(c);

            var editCmd = new EditCommentCommand(c.Response.ID, "            ").SetUserContext(user);
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
            var user = TestHelper.SetPrincipal("TestUser15");

            var cmd = new CreateCommentCommand(1, null, "This is a unit test and I like it.").SetUserContext(user);
            var c = cmd.Execute().Result;

            VoatAssert.IsValid(c);

            user = TestHelper.SetPrincipal("TestUser12");
            var editCmd = new EditCommentCommand(c.Response.ID, "All your comment are belong to us!").SetUserContext(user);
            var editResult = editCmd.Execute().Result;

            VoatAssert.IsValid(editResult, Status.Denied);
            Assert.AreEqual("User does not have permissions to perform requested action", editResult.Message);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        [TestCategory("Ban"), TestCategory("Ban.Domain")]
        public void CreateComment_BannedDomain()
        {
            var user = TestHelper.SetPrincipal("TestUser02");

            var cmd = new CreateCommentCommand(1, null, "[Check out this killer website](http://fleddit.com/f/3hen3k/Look_at_this_cat_just_Looook_awww)!").SetUserContext(user);
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
            var user = TestHelper.SetPrincipal("TestUser02");

            var cmd = new CreateCommentCommand(1, null, "[Check out this killer website](//fleddit.com/f/3hen3k/Look_at_this_cat_just_Looook_awww)!").SetUserContext(user);
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
            var user = TestHelper.SetPrincipal("TestUser02");

            var cmd = new CreateCommentCommand(1, null, "This is a unit test and I like it.").SetUserContext(user);
            var c = cmd.Execute().Result;

            VoatAssert.IsValid(c);

            var editCmd = new EditCommentCommand(c.Response.ID, "[Check out this killer website](http://fleddit.com/f/3hen3k/Look_at_this_cat_just_Looook_awww)!").SetUserContext(user);
            var editResult = editCmd.Execute().Result;
            VoatAssert.IsValid(editResult, Status.Denied, "Expecting Denied Status");
            Assert.AreEqual("Comment contains banned domains", editResult.Message);
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Comment")]
        [TestCategory("Comment.Post")]
        [TestCategory("Ban"), TestCategory("Ban.User")]
        public void CreateComment_WithBannedSubUser()
        {
            var user = TestHelper.SetPrincipal("BannedFromVUnit");

            var cmd = new CreateCommentCommand(1, null, "This is my data with banned user").SetUserContext(user);
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
            var user = TestHelper.SetPrincipal("BannedGlobally");

            var cmd = new CreateCommentCommand(1, null, "This is my data").SetUserContext(user);
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

            var user = TestHelper.SetPrincipal("TestUser01");
            var cmdcreate = new CreateCommentCommand(1, null, "This is my data too you know").SetUserContext(user);
            var c = cmdcreate.Execute().Result;

            VoatAssert.IsValid(c);

            int id = c.Response.ID;

            var cmd = new DeleteCommentCommand(id).SetUserContext(user);
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
            var user = TestHelper.SetPrincipal("TestUser01");
            var cmdcreate = new CreateCommentCommand(1, null, content).SetUserContext(user);
            var c = cmdcreate.Execute().Result;

            VoatAssert.IsValid(c);

            int id = c.Response.ID;

            //switch to mod of sub
            user = TestHelper.SetPrincipal(USERNAMES.Unit);
            var cmd = new DeleteCommentCommand(id, "This is spam").SetUserContext(user);
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
            var user = TestHelper.SetPrincipal(USERNAMES.Unit);
            var cmd = new EditCommentCommand(1, content).SetUserContext(user);
            var r = await cmd.Execute();

            VoatAssert.IsValid(r);
            Assert.AreEqual(content, r.Response.Content);
            Assert.AreEqual(Formatting.FormatMessage(content), r.Response.FormattedContent);

            //verify
            using (var db = new Voat.Data.Repository(user))
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
                    Subverse = SUBVERSES.Disabled,
                    Title = "Super Sneaky",
                    Content = "How can I post to disabled subs?",
                    UserName = USERNAMES.Unit,
                    CreationDate = DateTime.UtcNow
                };
                db.Submission.Add(submission);
                db.SaveChanges();
            }

            var user = TestHelper.SetPrincipal("TestUser05");
            var cmd = new CreateCommentCommand(submission.ID, null, "Are you @FuzzyWords?").SetUserContext(user);
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
            var user = TestHelper.SetPrincipal(userName);
            var body = Guid.NewGuid().ToString();
            var cmd = new CreateCommentCommand(1, 2, body).SetUserContext(user);
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
                var notice = db.Message.FirstOrDefault(x => x.Sender == userName && x.Recipient == USERNAMES.Unit && x.SubmissionID == 1 && x.CommentID == c.Response.ID);
                Assert.IsNotNull(notice, "Did not find a reply notification");
            }
        }
        [TestMethod]
        [TestCategory("Command"),TestCategory("Comment"),TestCategory("Comment.Post"), TestCategory("Notifications")]
        public async Task CreateComment_TestSubmissionCommentNotification()
        {
            var userName = "UnitTestUser19";
            var user = TestHelper.SetPrincipal(userName);
            var body = Guid.NewGuid().ToString();
            var cmd = new CreateCommentCommand(1, null, body).SetUserContext(user);
            var c = await cmd.Execute();

            VoatAssert.IsValid(c);

            //check for comment reply entry
            using (var db = new VoatDataContext())
            {
                var notice = db.Message.FirstOrDefault(x => x.Sender == userName && x.Recipient == "anon" && x.SubmissionID == 1);
                Assert.IsNotNull(notice, "Did not find a reply notification");
            }
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Submission"), TestCategory("Command.Submission.Post")]
        [TestCategory("Validation"), TestCategory("Submission.Validation")]
        public async Task Comment_Length_Validations()
        {
            var user = TestHelper.SetPrincipal("TestUser20");

            var createCmd = new CreateCommentCommand(1, null, "Can you hear me now?".RepeatUntil(10001)).SetUserContext(user);
            var r = await createCmd.Execute();
            VoatAssert.IsValid(r, Status.Denied);

            createCmd = new CreateCommentCommand(1, null, "Can you hear me now?").SetUserContext(user);
            r = await createCmd.Execute();
            VoatAssert.IsValid(r);

            var editCmd = new EditCommentCommand(r.Response.ID, "Can you hear me now?".RepeatUntil(100001)).SetUserContext(user);
            r = await editCmd.Execute();
            VoatAssert.IsValid(r, Status.Denied);
        }
    }
}
