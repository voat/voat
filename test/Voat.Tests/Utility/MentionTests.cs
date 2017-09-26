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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Tests.Infrastructure;
using Voat.Tests.Repository;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class MentionTests : BaseUnitTest
    {

        private static Domain.Models.Submission submission;
        //private static Domain.Models.Submission submission;

        public override void ClassInitialize()
        {
            string owner = "UnitTestUser04";
            submission = TestHelper.ContentCreation.CreateSubmission(owner, new Domain.Models.UserSubmission() { Title = GetMethodName(), Content = GetMethodName(true), Subverse = SUBVERSES.Unit });
            Assert.IsNotNull(submission, "Couldn't create test submission");
            Assert.AreNotEqual(0, submission.ID, "Doesn't appear we have a valid submission id");
        }

        [TestMethod]
        [TestCategory("Mention")]
        public async Task MentionUser_Duplicate_Test()
        {
            string user1 = "UnitTestUser05";
            string user2 = "UnitTestUser06";

            var user = TestHelper.SetPrincipal(user1);

            string mentionTwiceContent = $"Hello @{user2}, I am mentioning you twice using two different forms ok. So here: /u/{user2} ha ha";
            var cmd = new CreateCommentCommand(submission.ID, null, mentionTwiceContent).SetUserContext(user);
            var result = await cmd.Execute();
            Assert.IsTrue(result.Success, result.Message);

            using (var db = new VoatDataContext())
            {
                var count = db.Message.Where(x => x.Sender == user1 && x.Recipient == user2 && x.CommentID == result.Response.ID).Count();
                Assert.AreEqual(1, count, "Received duplicates and now users are annoyed and burning down the village! Run!");
            }


            //change casing test
            user = TestHelper.SetPrincipal(user2);
            mentionTwiceContent = $"Hello @{user1.ToLower()}, I am mentioning you twice using two different forms ok. So here: /u/{user1.ToUpper()} ha ha";
            cmd = new CreateCommentCommand(submission.ID, null, mentionTwiceContent).SetUserContext(user);
            result = await cmd.Execute();
            Assert.IsTrue(result.Success, result.Message);

            using (var db = new VoatDataContext())
            {
                var count = db.Message.Where(x => x.Sender == user2 && x.Recipient == user1 && x.CommentID == result.Response.ID).Count();
                Assert.AreEqual(1, count, "Received duplicates and now users are annoyed and burning down the village! Run!");
            }
        }

        //[TestMethod]
        [TestCategory("Mention")]
        public async Task MentionUser_MentionWithReply_Duplicate_Test()
        {

            // This test will require rewriting the notification code. Current this does not pass and as such has it's test method commented out.

            //We have two pipelines for sending notifications. They need to be centralized to allow this test to pass. Future people, solve this.

            string user1 = "UnitTestUser09";
            string user2 = "UnitTestUser10";

            var user = TestHelper.SetPrincipal(user1);
            string commentContent = $"Some ground breaking inciteful comment here";
            var cmd = new CreateCommentCommand(submission.ID, null, commentContent);
            var result = await cmd.Execute();
            Assert.IsTrue(result.Success, result.Message);

            user = TestHelper.SetPrincipal(user2);
            commentContent = $"Hey @{user1} I'm replying to your comment and mentioning you because I'm super annoying. Like you.";
            cmd = new CreateCommentCommand(submission.ID, result.Response.ID, commentContent);
            result = await cmd.Execute();
            Assert.IsTrue(result.Success, result.Message);

            using (var db = new VoatDataContext())
            {
                var count = db.Message.Where(x => x.Sender == user2 && x.Recipient == user1 && x.CommentID == result.Response.ID).Count();
                Assert.AreEqual(1, count, "Received duplicates and now users are annoyed and burning down the village! Run!");
            }

        }


        [TestMethod]
        [TestCategory("Mention")]
        public async Task MentionUser_Comment_Test()
        {
            string user1 = "UnitTestUser07";
            string user2 = "UnitTestUser08";

            var user = TestHelper.SetPrincipal(user1);
            string mentionTwiceContent = $"PSA: @{user2} is a shill. I saw him getting ready for work and his socks were standard shill issue.";
            var cmd = new CreateCommentCommand(submission.ID, null, mentionTwiceContent).SetUserContext(user);
            var result = await cmd.Execute();
            VoatAssert.IsValid(result);

            using (var db = new VoatDataContext())
            {
                var count = db.Message.Where(x => x.Sender == user1 && x.Recipient == user2 && x.CommentID == result.Response.ID).Count();
                Assert.AreEqual(1, count, "Expected to receive shill mention! THIS IS CENSORSHIP!");
            }
        }
        [TestMethod]
        [TestCategory("Mention")]
        public async Task MentionUser_Comment_Test_InvalidUser()
        {
            string user1 = "UnitTestUser07";
            var users = new[] { "UnitTestUser10", "NotGunnaWorkHere", "UnitTestUser11" };

            var user = TestHelper.SetPrincipal(user1);
            string mentionTwiceContent = $"PSA: {String.Join(", ", users.Select(x => $"@{x}"))} are shills. I saw them getting ready for work and their socks were standard shill issue.";
            var cmd = new CreateCommentCommand(submission.ID, null, mentionTwiceContent).SetUserContext(user);
            var result = await cmd.Execute();
            VoatAssert.IsValid(result);

            using (var db = new VoatDataContext())
            {
                var userName = users.First();
                var count = db.Message.Where(x => x.Sender == user1 && x.Recipient == userName && x.CommentID == result.Response.ID).Count();
                Assert.AreEqual(1, count, $"Expected to receive shill mention for {userName}! THIS IS CENSORSHIP!");

                userName = users.Last();
                count = db.Message.Where(x => x.Sender == user1 && x.Recipient == userName && x.CommentID == result.Response.ID).Count();
                Assert.AreEqual(1, count, $"Expected to receive shill mention for {userName}! THIS IS CENSORSHIP!");
            }
        }

        [TestMethod]
        [TestCategory("Mention")]
        public async Task MentionUser_Anon_NoBlock_Test()
        {
            string user1 = "UnitTestUser11";
            string user2 = "UnitTestUser12";

            var user = TestHelper.SetPrincipal(user1);

            //Submission
            var anonSubmission = TestHelper.ContentCreation.CreateSubmission(user1, new Domain.Models.UserSubmission() { Title = $"I'm harrassing @{user2}!", Content = GetMethodName(true), Subverse = SUBVERSES.Anon });
            Assert.IsNotNull(anonSubmission, "Couldn't create test submission");
            Assert.AreNotEqual(0, anonSubmission.ID, "Doesn't appear we have a valid submission id");

            Thread.Sleep(2000);

            using (var db = new VoatDataContext())
            {
                var count = db.Message.Where(x => 
                    x.Sender == user1 
                    && x.Recipient == user2 
                    && x.SubmissionID == anonSubmission.ID 
                    && x.IsAnonymized == true
                    && x.Type == (int)Domain.Models.MessageType.SubmissionMention
                    ).Count();
                Assert.AreEqual(1, count, "Where is the harassment submission message!?!?!?");
            }

            //Comment
            string mentionTwiceContent = $"Hello @{user2}, I am mentioning you in an anon thread because I want to make you feel scared";
            var cmd = new CreateCommentCommand(anonSubmission.ID, null, mentionTwiceContent).SetUserContext(user);
            var result = await cmd.Execute();
            VoatAssert.IsValid(result);

            using (var db = new VoatDataContext())
            {
                var count = db.Message.Where(x => 
                    x.Sender == user1 
                    && x.Recipient == user2 
                    && x.CommentID == result.Response.ID 
                    && x.IsAnonymized == true
                    && x.Type == (int)Domain.Models.MessageType.CommentMention

                  ).Count();
                Assert.AreEqual(1, count, "Where is the harassment comment message!?!?!?");
            }


        }

        [TestMethod]
        [TestCategory("Mention")]
        public async Task MentionUser_Anon_Block_Test()
        {
            string user1 = "UnitTestUser13";
            string user2 = "BlocksAnonTestUser01";

            TestDataInitializer.CreateUser(user2);

            var user = TestHelper.SetPrincipal(user2);
            var prefCmd = new UpdateUserPreferencesCommand(new Domain.Models.UserPreferenceUpdate() { BlockAnonymized = true }).SetUserContext(user);
            var prefResult = await prefCmd.Execute();
            VoatAssert.IsValid(prefResult);

            //Submission Mention - NO NO
            user = TestHelper.SetPrincipal(user1);
            var anonSubmission = TestHelper.ContentCreation.CreateSubmission(user1, new Domain.Models.UserSubmission() { Title = $"I'm harrassing @{user2}!", Content = $"Hey everyone isn't /u/{user2} a shill tornado?", Subverse = SUBVERSES.Anon });
            Assert.IsNotNull(anonSubmission, "Couldn't create test submission");
            Assert.AreNotEqual(0, anonSubmission.ID, "Doesn't appear we have a valid submission id");

            using (var db = new VoatDataContext())
            {
                var count = db.Message.Where(x => 
                    x.Sender == user1 
                    && x.Recipient == user2 
                    && x.SubmissionID == anonSubmission.ID 
                    && x.IsAnonymized == true
                    && x.Type == (int)Domain.Models.MessageType.SubmissionMention
                    ).Count();
                Assert.AreEqual(0, count, "Expecting No Submission Mentions!");
            }

            //Comment Mention - NO NO
            string commentContent = $"Hello @{user2}, I am mentioning you in an anon thread because I want to make you feel scared";
            var cmd = new CreateCommentCommand(anonSubmission.ID, null, commentContent).SetUserContext(user);
            var result = await cmd.Execute();
            VoatAssert.IsValid(result);

            using (var db = new VoatDataContext())
            {
                var count = db.Message.Where(x => 
                    x.Sender == user1 
                    && x.Recipient == user2 
                    && x.CommentID == result.Response.ID 
                    && x.IsAnonymized == true
                    && x.Type == (int)Domain.Models.MessageType.CommentMention
                    ).Count();
                Assert.AreEqual(0, count, "Received duplicates and now users are annoyed and burning down the village! Run!");
            }

            //Comment Reply - YES YES
            user = TestHelper.SetPrincipal(user2);
            commentContent = $"I'm {user2} won't someone reply to me so I can see if reply notifications work?";
            cmd = new CreateCommentCommand(anonSubmission.ID, null, commentContent).SetUserContext(user);
            result = await cmd.Execute();
            VoatAssert.IsValid(result);


            user = TestHelper.SetPrincipal(user1);
            commentContent = $"I'm following you!";
            cmd = new CreateCommentCommand(anonSubmission.ID, result.Response.ID, commentContent).SetUserContext(user);
            result = await cmd.Execute();
            VoatAssert.IsValid(result);

            using (var db = new VoatDataContext())
            {
                var count = db.Message.Where(x => 
                    x.Sender == user1 
                    && x.Recipient == user2 
                    && x.CommentID == result.Response.ID 
                    && x.IsAnonymized == true
                    && x.Type == (int)Domain.Models.MessageType.CommentReply
                    ).Count();

                Assert.AreEqual(1, count, "Replies should work!!!!!!");
            }

            //Submission Reply - YES YES
            user = TestHelper.SetPrincipal(user2);
            anonSubmission = TestHelper.ContentCreation.CreateSubmission(user2, new Domain.Models.UserSubmission() { Title = $"Is this working?", Content = $"Someeone, anyone, am I alone?", Subverse = SUBVERSES.Anon });
            Assert.IsNotNull(anonSubmission, "Couldn't create test submission");
            Assert.AreNotEqual(0, anonSubmission.ID, "Doesn't appear we have a valid submission id");

            user = TestHelper.SetPrincipal(user1);
            commentContent = $"I know who you are and I've been following you this entire unit test. I might be in love with you, if stalking is a form of love.";
            cmd = new CreateCommentCommand(anonSubmission.ID, null, commentContent).SetUserContext(user);
            result = await cmd.Execute();
            VoatAssert.IsValid(result);

            using (var db = new VoatDataContext())
            {
                var count = db.Message.Where(x =>
                    x.Sender == user1
                    && x.Recipient == user2
                    && x.SubmissionID == anonSubmission.ID
                    && x.IsAnonymized == true
                    && x.Type == (int)Domain.Models.MessageType.SubmissionReply
                    ).Count();

                Assert.AreEqual(1, count, "Replies should work!!!!!!");
            }
        }


    }
}
