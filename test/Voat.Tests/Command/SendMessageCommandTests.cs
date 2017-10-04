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
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class SendMessageCommandTests : BaseUnitTest
    { 
        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendBadRecipientInfo()
        {

            var id = Guid.NewGuid().ToString();
            var sender = USERNAMES.User100CCP;

            var user = TestHelper.SetPrincipal(sender);
            var cmd = new SendMessageCommand(new Domain.Models.SendMessage() { Recipient = "Do you like chocolate", Message = id, Subject = "All That Matters" }, false, false).SetUserContext(user);
            var response = await cmd.Execute();
            VoatAssert.IsValid(response, Status.Invalid);

            using (var db = new VoatDataContext())
            {
                var count = (from x in db.Message
                             where
                               x.Content == id
                             select x).Count();
                Assert.AreEqual(0, count, "Expecting no messages to make it through");
            }
        }
        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendBadRecipientInfo_CheckExists()
        {

            var id = Guid.NewGuid().ToString();
            var sender = USERNAMES.User100CCP;

            var user = TestHelper.SetPrincipal(sender);
            var cmd = new SendMessageCommand(new Domain.Models.SendMessage() { Recipient = "Do you like chocolate", Message = id, Subject = "All That Matters" }, false, true).SetUserContext(user);
            var response = await cmd.Execute();

            VoatAssert.IsValid(response, Status.Error);

            Assert.AreNotEqual("Comment points too low to send messages. Need at least 10 CCP.", response.Message);
            
            using (var db = new VoatDataContext())
            {
                var count = (from x in db.Message
                              where
                                x.Content == id
                              select x).Count();
                Assert.AreEqual(0, count, "Expecting no messages to make it through");
            }
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendPrivateMessageDeniedCCP()
        {
            var id = Guid.NewGuid().ToString();
            var sender = USERNAMES.Unit;
            var recipient = USERNAMES.Anon;

            var user = TestHelper.SetPrincipal(sender);

            var message = new Domain.Models.SendMessage()
            {
                //Sender = User.Identity.Name,
                Recipient = recipient,
                Subject = id,
                Message = id
            };
            var cmd = new SendMessageCommand(message).SetUserContext(user);
            var response = await cmd.Execute();

            VoatAssert.IsValid(response, Status.Ignored);
            Assert.IsNotNull(response, "Response is null");
            Assert.IsFalse(response.Success, "Expecting not success response");

        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendPrivateMessage() {

            var id = Guid.NewGuid().ToString();
            var sender = USERNAMES.User100CCP;
            var recipient = USERNAMES.Anon;

            var user = TestHelper.SetPrincipal(sender);

            var message = new Domain.Models.SendMessage()
            {
                //Sender = User.Identity.Name,
                Recipient = recipient,
                Subject = id,
                Message = id
            };
            var cmd = new SendMessageCommand(message).SetUserContext(user);
            var response = await cmd.Execute();

            VoatAssert.IsValid(response);

            using (var db = new VoatDataContext())
            {
                var record = (from x in db.Message
                              where 
                                x.Recipient == recipient 
                                && x.Sender == sender 
                                && x.Title == id
                                && x.Content == id
                                && x.Subverse == null
                                && x.CommentID == null
                                && x.SubmissionID == null
                                && x.Type == (int)Domain.Models.MessageType.Sent
                                //&& x.Direction == (int)Domain.Models.MessageDirection.OutBound
                                select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find outbound in database");

                record = (from x in db.Message
                              where
                                x.Recipient == recipient
                                && x.Sender == sender
                                && x.Title == id
                                && x.Content == id
                                && x.Subverse == null
                                && x.CommentID == null
                                && x.SubmissionID == null
                                && x.Type == (int)Domain.Models.MessageType.Private
                                //&& x.Direction == (int)Domain.Models.MessageDirection.InBound
                              select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find inbound in database");
            }
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendPrivateMessageReply()
        {
            var id = Guid.NewGuid().ToString();
            var sender = USERNAMES.User100CCP;
            var recipient = "anon";

            var user = TestHelper.SetPrincipal(sender);

            var message = new Domain.Models.SendMessage()
            {
                //Sender = User.Identity.Name,
                Recipient = recipient,
                Subject = id,
                Message = id
            };
            var cmd = new SendMessageCommand(message).SetUserContext(user);
            var response = await cmd.Execute();
            VoatAssert.IsValid(response);

            var firstMessage = response.Response;
            var idToRespondTo = 0;
            //Ensure first msg is in db
            using (var db = new VoatDataContext())
            {
                var record = (from x in db.Message
                              where
                                x.Recipient == recipient
                                && x.Sender == sender
                                && x.Title == id
                                && x.Content == id
                                && x.Subverse == null
                                && x.CommentID == null
                                && x.SubmissionID == null
                                && x.Type == (int)Domain.Models.MessageType.Sent
                                //&& x.Direction == (int)Domain.Models.MessageDirection.OutBound
                              select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find sent in database");

                record = (from x in db.Message
                          where
                            x.Recipient == recipient
                            && x.Sender == sender
                            && x.Title == id
                            && x.Content == id
                            && x.Subverse == null
                            && x.CommentID == null
                            && x.SubmissionID == null
                            && x.Type == (int)Domain.Models.MessageType.Private
                            //&& x.Direction == (int)Domain.Models.MessageDirection.InBound
                          select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find message in database");

                idToRespondTo = record.ID;
            }

            //reply
            user = TestHelper.SetPrincipal(recipient);
            var replyCmd = new SendMessageReplyCommand(idToRespondTo, $"Reply to {idToRespondTo.ToString()}").SetUserContext(user);
            var replyResponse = await replyCmd.Execute();
            VoatAssert.IsValid(replyResponse);

            var replyMessage = replyResponse.Response;

            Assert.AreEqual(idToRespondTo, replyMessage.ParentID);

        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendPrivateMessageToSubverse()
        {
            var id = Guid.NewGuid().ToString();
            var sender = USERNAMES.User100CCP;
            var subverse = SUBVERSES.Unit;

            //ensure v/Unit has moderators
            using (var db = new VoatDataContext())
            {
                var record = (from x in db.SubverseModerator
                              where
                                x.Subverse == subverse
                              select x).ToList();
                var mod = USERNAMES.Anon;
                if (!record.Any(x => x.UserName == mod))
                {
                    db.SubverseModerator.Add(new Voat.Data.Models.SubverseModerator() { UserName = mod, Subverse = subverse, CreatedBy = "UnitTesting", CreationDate = DateTime.UtcNow, Power = 1 });
                }
                mod = USERNAMES.Unit;
                if (!record.Any(x => x.UserName == mod))
                {
                    db.SubverseModerator.Add(new Voat.Data.Models.SubverseModerator() { UserName = mod, Subverse = subverse, CreatedBy = "UnitTesting", CreationDate = DateTime.UtcNow, Power = 1 });
                }
                db.SaveChanges();
            }

            var user = TestHelper.SetPrincipal(sender);

            var message = new Domain.Models.SendMessage()
            {
                //Sender = User.Identity.Name,
                Recipient = $"v/{subverse}",
                Subject = id,
                Message = id
            };
            var cmd = new SendMessageCommand(message).SetUserContext(user);
            var response = await cmd.Execute();

            Assert.IsNotNull(response, "Response is null");
            Assert.IsTrue(response.Success, response.Message);

            using (var db = new VoatDataContext())
            {
                var record = (from x in db.Message
                              where
                                x.Sender == sender
                                && x.SenderType == (int)Domain.Models.IdentityType.User
                                && x.Recipient == subverse
                                && x.RecipientType == (int)Domain.Models.IdentityType.Subverse
                                && x.Title == id
                                && x.Content == id
                              select x).ToList();
                Assert.IsNotNull(record, "Can not find message in database");
                Assert.AreEqual(2, record.Count, "Expecting 2 PMs");
            }
        }
        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendPrivateMessageFromSubverse()
        {
            
            var id = Guid.NewGuid().ToString();
            var sender = "unit";
            var recipient = USERNAMES.User100CCP;

            var user = TestHelper.SetPrincipal(sender);

            var message = new Domain.Models.SendMessage()
            {
                Sender = $"v/{sender}",
                Recipient = recipient,
                Subject = id,
                Message = id
            };
            var cmd = new SendMessageCommand(message).SetUserContext(user);
            var r = await cmd.Execute();

            VoatAssert.IsValid(r);

            using (var db = new VoatDataContext())
            {
                var record = (from x in db.Message
                              where
                                x.Sender == sender
                                && x.SenderType == (int)Domain.Models.IdentityType.Subverse
                                && x.Recipient == recipient
                                && x.RecipientType == (int)Domain.Models.IdentityType.User
                                && x.Title == id
                                && x.Content == id
                              select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find message in database");
            }
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Messaging"), TestCategory("Command.Messaging"), TestCategory("Process")]
        public async Task SendMessageReply_ToCommentRelatedMention()
        {
            //create a comment and ping another user aka comment mention

            var user = TestHelper.SetPrincipal("TestUser14");
            var c = new CreateCommentCommand(1, null, "This is my comment @TestUser15, do you like it?").SetUserContext(user);
            var r = await c.Execute();
            VoatAssert.IsValid(r);

            //read ping message and reply to it using message gateway
            var userName = "TestUser15";
            user = TestHelper.SetPrincipal(userName);
            var mq = new Domain.Query.QueryMessages(user, MessageTypeFlag.CommentMention, MessageState.Unread).SetUserContext(user);
            var messages = await mq.ExecuteAsync();
            Assert.IsTrue(messages.Any(), "Didn't return any messages");

            var m = messages.FirstOrDefault(x => x.CommentID == r.Response.ID);
            Assert.IsNotNull(m, "Cant find message");

            var content = "I reply to you from messages";
            var cmd = new SendMessageReplyCommand(m.ID, content).SetUserContext(user);
            var response = await cmd.Execute();
            VoatAssert.IsValid(response);
     
            //ensure comment exists in thread as message gateway should submit a comment based on message type and info
            using (var db = new VoatDataContext())
            {
                var record = (from x in db.Comment
                              where
                                x.SubmissionID == m.SubmissionID.Value
                                && x.ParentID == m.CommentID.Value
                                && x.Content == content
                                && x.UserName == userName
                              select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find message in database");
            }
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendCommentNotificationMessage()
        {
            await TestCommentNotification(SUBVERSES.Unit, "UnitTestUser18", "UnitTestUser17");

        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendCommentNotificationMessage_AnonSub()
        {
            await TestCommentNotification(SUBVERSES.Anon, "TestUser03", "TestUser04");
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendUserMentionNotificationMessage_Anon()
        {
            await TestUserMentionNotification(SUBVERSES.Anon, "UnitTestUser08", "UnitTestUser09", "UnitTestUser11");
        }

        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendUserMentionNotificationMessage()
        {
            await TestUserMentionNotification(SUBVERSES.Unit, "UnitTestUser05", "UnitTestUser10", "UnitTestUser07");
        }
        
        #region HelperMethods


        public async Task TestCommentNotification(string sub, string user1, string user2)
        {
            var id = Guid.NewGuid().ToString();

            //Post submission as TestUser1
            var user = TestHelper.SetPrincipal(user1);
            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = sub, Title = "Let's be creative!", Content = "No" }).SetUserContext(user);
            var response = await cmd.Execute();
            VoatAssert.IsValid(response);

            var submission = response.Response;
            Assert.IsNotNull(submission, "Expected a non-null submission response");

            //Reply to comment as TestUser2
            user = TestHelper.SetPrincipal(user2);
            var commentCmd = new CreateCommentCommand(submission.ID, null, "Important Comment").SetUserContext(user);
            var responseComment = await commentCmd.Execute();
            VoatAssert.IsValid(responseComment);

            var comment = responseComment.Response;
            Assert.IsNotNull(comment, "Expected a non-null comment response");


            //Check to see if Comment notification exists in messages
            using (var db = new VoatDataContext())
            {
                var record = (from x in db.Message
                              where
                                x.Recipient == user1
                                && x.Sender == user2
                                && x.IsAnonymized == submission.IsAnonymized
                                && x.IsAnonymized == comment.IsAnonymized
                                && x.CommentID == comment.ID
                                && x.SubmissionID == submission.ID
                                && x.Subverse == submission.Subverse
                                && x.Type == (int)Domain.Models.MessageType.SubmissionReply
                               // && x.Direction == (int)Domain.Models.MessageDirection.InBound
                              select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find message in database");
            }
        }

        public async Task TestUserMentionNotification(string sub, string user1, string user2, string user3)
        {
            var id = Guid.NewGuid().ToString();
            //var user1 = "UnitTestUser10";
            //var user2 = "UnitTestUser20";

            //Post submission as TestUser1
            var user = TestHelper.SetPrincipal(user1);
            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = sub, Title = "I love you more than butter in my coffee", Content = $"Did you hear that @{user2} or /u/{user2}?" }).SetUserContext(user);
            var response = await cmd.Execute();
            VoatAssert.IsValid(response);
            var submission = response.Response;
            Assert.IsNotNull(submission, "Expected a non-null submission response");


            //Reply to comment as TestUser2
            user = TestHelper.SetPrincipal(user2);
            var commentCmd = new CreateCommentCommand(submission.ID, null, $"I bet @{user3} could think of something!").SetUserContext(user);
            var responseComment = await commentCmd.Execute();
            VoatAssert.IsValid(responseComment);
            var comment = responseComment.Response;
            Assert.IsNotNull(comment, "Expected a non-null comment response");

            //Check to see if Comment notification exists in messages
            using (var db = new VoatDataContext())
            {
                var subverse = (from x in db.Subverse
                                where x.Name.ToLower() == sub.ToLower()
                                select x).FirstOrDefault();

                Assert.AreEqual(submission.IsAnonymized, subverse.IsAnonymized, "Expecting matching anon settings");
                Assert.AreEqual(comment.IsAnonymized, subverse.IsAnonymized, "Expecting matching anon settings comment");

                //test for submission notification
                var record = (from x in db.Message
                              where
                                x.Recipient == user2
                                && x.Sender == user1
                                && x.IsAnonymized == submission.IsAnonymized
                                //&& x.IsAnonymized == comment.IsAnonymized
                                && x.CommentID == null
                                && x.SubmissionID == submission.ID
                                && x.Subverse == submission.Subverse
                                && x.Type == (int)Domain.Models.MessageType.SubmissionMention
                                //&& x.Direction == (int)Domain.Models.MessageDirection.InBound
                              select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find submission message in database");

                //test for comment notification
                record = (from x in db.Message
                              where
                                x.Recipient == user3
                                && x.Sender == user2
                                && x.IsAnonymized == submission.IsAnonymized
                                && x.IsAnonymized == comment.IsAnonymized
                                && x.CommentID == comment.ID
                                && x.SubmissionID == submission.ID
                                && x.Subverse == submission.Subverse
                                && x.Type == (int)Domain.Models.MessageType.CommentMention
                                //&& x.Direction == (int)Domain.Models.MessageDirection.InBound
                              select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find comment message in database");
            }
        }

        #endregion
    }
}
