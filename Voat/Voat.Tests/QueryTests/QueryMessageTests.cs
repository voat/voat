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
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Domain.Query;

namespace Voat.Tests.QueryTests
{
    [TestClass]
    public class QueryMessageTests : BaseUnitTest
    {
        private string userName = "User50CCP";
        private string subName = "unit";

        private List<Domain.Models.Message> messages = new List<Domain.Models.Message>();

        public override void ClassInitialize()
        {
            //create user inbox
            Message message = null;

            TestHelper.SetPrincipal(userName);
            var msg = "1";
            var cmd = new SendMessageCommand(new Domain.Models.SendMessage() { Subject = "Chain", Recipient = "TestUser01", Message = msg });
            var response = cmd.Execute().Result;
            Assert.IsTrue(response.Success, response.Message);
            Assert.IsNotNull(response.Response, "Expecting non-null response");
            message = response.Response;
            messages.Add(message);

            TestHelper.SetPrincipal("TestUser01");
            msg = "1.1";
            var cmdReply = new SendMessageReplyCommand(message.ID, msg);
            var responseReply = cmdReply.Execute().Result;
            Assert.IsTrue(responseReply.Success, "Expecting true on send");
            Assert.IsNotNull(responseReply.Response, "Expecting non-null response");
            message = responseReply.Response;
            messages.Add(message);


            TestHelper.SetPrincipal(userName);
            msg = "1.1.1";
            cmdReply = new SendMessageReplyCommand(message.ID, msg);
            responseReply = cmdReply.Execute().Result;
            Assert.IsTrue(responseReply.Success, "Expecting true on send");
            Assert.IsNotNull(responseReply.Response, "Expecting non-null response");
            message = responseReply.Response;
            messages.Add(message);

            TestHelper.SetPrincipal("TestUser01");
            msg = "1.1.1.1";
            cmdReply = new SendMessageReplyCommand(message.ID, msg);
            responseReply = cmdReply.Execute().Result;
            Assert.IsTrue(responseReply.Success, "Expecting true on send");
            Assert.IsNotNull(responseReply.Response, "Expecting non-null response");
            message = responseReply.Response;
            messages.Add(message);
        }


        [TestMethod]
        [TestCategory("Query"), TestCategory("Messaging"), TestCategory("Query.Messages")]
        public async Task GetUnreadInbox()
        {

            TestHelper.SetPrincipal(userName);
            var q = new QueryMessages(MessageTypeFlag.All, MessageState.Unread, false);
            var m = await q.ExecuteAsync();

            Assert.IsNotNull(m, "Assert 1");

            var qc = new QueryMessageCounts(MessageTypeFlag.All, MessageState.Unread);
            var mc = await qc.ExecuteAsync();

            Assert.IsNotNull(mc, "Assert 2");

        }
    }
}
