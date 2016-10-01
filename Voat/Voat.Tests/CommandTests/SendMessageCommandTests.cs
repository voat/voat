using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Domain.Command;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class SendMessageCommandTests
    {


        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendPrivateMessageDeniedCCP()
        {
            var id = Guid.NewGuid().ToString();
            var sender = "unit";
            var recipient = "anon";

            TestHelper.SetPrincipal(sender);

            var message = new Domain.Models.SendMessage()
            {
                //Sender = User.Identity.Name,
                Recipient = recipient,
                Subject = id,
                Message = id
            };
            var cmd = new SendMessageCommand(message);
            var response = await cmd.Execute();

            Assert.IsNotNull(response, "Response is null");
            Assert.IsFalse(response.Success, "Expecting not success response");

        }
        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendPrivateMessage() {

            var id = Guid.NewGuid().ToString();
            var sender = "User100CCP";
            var recipient = "anon";

            TestHelper.SetPrincipal(sender);

            var message = new Domain.Models.SendMessage()
            {
                //Sender = User.Identity.Name,
                Recipient = recipient,
                Subject = id,
                Message = id
            };
            var cmd = new SendMessageCommand(message);
            var response = await cmd.Execute();

            Assert.IsNotNull(response, "Response is null");
            Assert.IsTrue(response.Success, response.Status.ToString());

            using (var db = new voatEntities())
            {
                var record = (from x in db.PrivateMessages
                              where 
                                x.Recipient == recipient 
                                && x.Sender == sender 
                                && x.Subject == id
                                && x.Body == id
                                select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find message in database");
            }
        }
        [TestMethod]
        [TestCategory("Command")]
        [TestCategory("Messaging")]
        [TestCategory("Command.Messaging")]
        public async Task SendPrivateMessageToSubverse()
        {
            var id = Guid.NewGuid().ToString();
            var sender = "User100CCP";
            var subverse = "unit";

            //ensure v/Unit has moderators
            using (var db = new voatEntities())
            {
                var record = (from x in db.SubverseModerators
                              where
                                x.Subverse == subverse
                              select x).ToList();
                var mod = "anon";
                if (!record.Any(x => x.UserName == mod))
                {
                    db.SubverseModerators.Add(new SubverseModerator() { UserName = mod, Subverse = subverse, CreatedBy = "UnitTesting", CreationDate = DateTime.UtcNow, Power = 1 });
                }
                mod = "unit";
                if (!record.Any(x => x.UserName == mod))
                {
                    db.SubverseModerators.Add(new SubverseModerator() { UserName = mod, Subverse = subverse, CreatedBy = "UnitTesting", CreationDate = DateTime.UtcNow, Power = 1 });
                }
                db.SaveChanges();
            }

            TestHelper.SetPrincipal(sender);

            var message = new Domain.Models.SendMessage()
            {
                //Sender = User.Identity.Name,
                Recipient = $"v/{subverse}",
                Subject = id,
                Message = id
            };
            var cmd = new SendMessageCommand(message);
            var response = await cmd.Execute();

            Assert.IsNotNull(response, "Response is null");
            Assert.IsTrue(response.Success, response.Status.ToString());

            using (var db = new voatEntities())
            {
                var record = (from x in db.PrivateMessages
                              where
                                x.Sender == sender
                                //&& x.Subject == $"[v/{subverse}] {id}"
                                && x.Body == id
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
            var sender = "v/unit";
            var recipient = "User100CCP";

            TestHelper.SetPrincipal("User500CCP");

            var message = new Domain.Models.SendMessage()
            {
                Sender = sender,
                Recipient = recipient,
                Subject = id,
                Message = id
            };
            var cmd = new SendMessageCommand(message);
            var response = await cmd.Execute();

            Assert.IsNotNull(response, "Response is null");
            Assert.IsTrue(response.Success, response.Status.ToString());

            using (var db = new voatEntities())
            {
                var record = (from x in db.PrivateMessages
                              where
                                x.Recipient == recipient
                                && x.Sender == sender
                                && x.Subject == id
                                && x.Body == id
                              select x).FirstOrDefault();
                Assert.IsNotNull(record, "Can not find message in database");
            }
        }
    }
}
