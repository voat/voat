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
using Voat.Domain.Models;
using Voat.Utilities;
using Voat.Tests.Infrastructure;
using Voat.Notifications;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class EventNotificationTests : BaseUnitTest
    {
        [TestMethod]
        public void TestNotificationEvents()
        {
            string userName = "Puttster";
            var n = new EventNotification();
            var msgReceived = false;
            //chat
            n.OnChatMessageReceived += (s, e) => {
                msgReceived = e.TargetUserName == userName && e.SendingUserName == "Atko" && e.Message == "TestMessage" && e.Chatroom == "TestChatRoom";
            };
            n.SendChatMessageNotice(userName, "Atko", "TestChatRoom", "TestMessage");
            Assert.IsTrue(msgReceived, "OnChatMessageReceived failed");
            msgReceived = false;

            ////comment replies
            //n.OnCommentReplyReceived += (s, e) => {
            //    msgReceived = e.UserName == userName && e.MessageType == MessageType.Comment && e.ReferenceID == 34;
            //};
            //n.SendCommentReplyNotice(userName, MessageType.Comment, 34);
            //Assert.IsTrue(msgReceived, "OnCommentReplyReceived failed");
            //msgReceived = false;
            
            //headbutts
            n.OnHeadButtReceived += (s, e) => {
                msgReceived = e.TargetUserName == userName && e.SendingUserName == "Atko" && e.Message == "blah";
            };
            n.SendHeadButtNotice(userName, "Atko", "blah");
            Assert.IsTrue(msgReceived, "OnHeadButtReceived failed");
            msgReceived = false;

            //votes
            n.OnVoteReceived += (s, e) => {
                msgReceived = e.TargetUserName == userName && e.SendingUserName == "Atko" && e.ReferenceType == ContentType.Comment && e.ReferenceID == 123 && e.ChangeValue == -1;
            };
            n.SendVoteNotice(userName, "Atko", ContentType.Comment, 123, -1);
            Assert.IsTrue(msgReceived, "OnVoteReceived failed");
            msgReceived = false;

            //mention
            n.OnMentionReceived += (s, e) => {
                msgReceived = e.TargetUserName == userName && e.MessageType == MessageTypeFlag.CommentMention && e.ReferenceType == ContentType.Comment && e.ReferenceID == 34 && e.Message == "Howdy @Puttster";
            };
            n.SendMentionNotice(userName, "Atko", ContentType.Comment, 34, "Howdy @Puttster");
            Assert.IsTrue(msgReceived, "OnMentionReceived failed");
            msgReceived = false;

            n.OnMentionReceived += (s, e) => {
                msgReceived = e.TargetUserName == userName && e.MessageType == MessageTypeFlag.CommentMention && e.ReferenceType == ContentType.Comment && e.ReferenceID == 34 && e.Message == null;
            };
            n.SendMentionNotice(userName, "Atko", ContentType.Comment, 34, null);
            Assert.IsTrue(msgReceived, "OnMentionReceived failed - no message");
            msgReceived = false;

            //message
            n.OnMessageReceived += (s, e) => {
                msgReceived = e.TargetUserName == userName && e.MessageType == MessageTypeFlag.CommentReply && e.ReferenceType == ContentType.Comment && e.ReferenceID == 34;
            };
            n.SendMessageNotice(userName, "Atko", MessageTypeFlag.CommentReply, ContentType.Comment, 34);
            Assert.IsTrue(msgReceived, "OnMessageReceived failed");
            msgReceived = false;
        }
    }
}
