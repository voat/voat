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
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Tests.Infrastructure;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.QueryTests
{
    [TestClass]
    public class QueryMessageTests : BaseUnitTest
    {
        private string userName = USERNAMES.User50CCP;

        private static List<Domain.Models.Message> messages = new List<Domain.Models.Message>();

        public override void ClassInitialize()
        {
            var user2 = "TestUser01";
            //create user inbox
            Message message = null;

            var user = TestHelper.SetPrincipal(userName);
            var msg = "1";
            var cmd = new SendMessageCommand(new Domain.Models.SendMessage() { Subject = "Chain", Recipient = "TestUser01", Message = msg }).SetUserContext(user);
            var response = cmd.Execute().Result;
            VoatAssert.IsValid(response);

            message = response.Response;
            messages.Add(message);

            user = TestHelper.SetPrincipal(user2);
            //Find message
            var query = new QueryMessages(user, MessageTypeFlag.Private, MessageState.Unread, false).SetUserContext(user);
            var inbox = query.Execute();
            VoatAssert.IsValid(inbox);
            message = inbox.FirstOrDefault(x => x.Sender == USERNAMES.User50CCP && x.Content == msg);
            Assert.IsNotNull(message, "Can not find message in recipient inbox 1");

            msg = "1.1";
            var cmdReply = new SendMessageReplyCommand(message.ID, msg).SetUserContext(user);
            var responseReply = cmdReply.Execute().Result;
            VoatAssert.IsValid(responseReply);
            messages.Add(message);


            user = TestHelper.SetPrincipal(userName);
            //Find message
            query = new QueryMessages(user, MessageTypeFlag.Private, MessageState.Unread, false).SetUserContext(user);
            inbox = query.Execute();
            VoatAssert.IsValid(inbox);
            message = inbox.FirstOrDefault(x => x.Sender == user2 && x.Content == msg);
            Assert.IsNotNull(message, "Can not find message in recipient inbox 2");

            msg = "1.1.1";
            cmdReply = new SendMessageReplyCommand(message.ID, msg).SetUserContext(user);
            responseReply = cmdReply.Execute().Result;
            VoatAssert.IsValid(responseReply);

            message = responseReply.Response;
            messages.Add(message);

            user = TestHelper.SetPrincipal(user2);
            //Find message
            query = new QueryMessages(user, MessageTypeFlag.Private, MessageState.Unread, false).SetUserContext(user);
            inbox = query.Execute();
            VoatAssert.IsValid(inbox);
            message = inbox.FirstOrDefault(x => x.Sender == USERNAMES.User50CCP && x.Content == msg);
            Assert.IsNotNull(message, "Can not find message in recipient inbox 3");

            msg = "1.1.1.1";
            cmdReply = new SendMessageReplyCommand(message.ID, msg).SetUserContext(user);
            responseReply = cmdReply.Execute().Result;
            VoatAssert.IsValid(responseReply);

            message = responseReply.Response;
            messages.Add(message);
        }


        [TestMethod]
        [TestCategory("Query"), TestCategory("Messaging"), TestCategory("Query.Messages")]
        public async Task GetUnreadInbox()
        {

            var user = TestHelper.SetPrincipal(userName);
            var q = new QueryMessages(user, MessageTypeFlag.All, MessageState.Unread, false).SetUserContext(user);
            var m = await q.ExecuteAsync();

            Assert.IsNotNull(m, "Assert 1");

            var qc = new QueryMessageCounts(user, MessageTypeFlag.All, MessageState.Unread).SetUserContext(user);
            var mc = await qc.ExecuteAsync();

            Assert.IsNotNull(mc, "Assert 2");

        }
    }
}
