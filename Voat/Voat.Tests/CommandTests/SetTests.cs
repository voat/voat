using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Voat.Domain.Command;
using Voat.Domain.Models;

namespace Voat.Tests.CommandTests
{

    [TestClass]
    public class SetTests
    {

        [TestMethod]
        [TestCategory("Set"), TestCategory("Command.Set")]
        public async Task CreateSet_Test()
        {
            var userName = "TestUser01";
            TestHelper.SetPrincipal(userName);

            var set = new Set() { Name = "HolyHell", Title = "Some Title", Type = SetType.Normal, UserName = userName };
            var cmd = new UpdateSetCommand(set);
            var result = await cmd.Execute();
            Assert.IsTrue(result.Success, result.Message);
            Assert.AreNotEqual(0, result.Response.ID);
            var setID = result.Response.ID;
            VerifySubscriber(set, userName, 1);

            //Subscribe another user
            userName = "TestUser02";
            TestHelper.SetPrincipal(userName);
            var subCmd = new SubscribeCommand(new DomainReference(DomainType.Set, set.Name, set.UserName), SubscriptionAction.Subscribe);
            var subResult = await subCmd.Execute();
            Assert.IsTrue(subResult.Success, subResult.Message);
            VerifySubscriber(set, userName, 2);

            //unsub user
            subCmd = new SubscribeCommand(new DomainReference(DomainType.Set, set.Name, set.UserName), SubscriptionAction.Unsubscribe);
            subResult = await subCmd.Execute();
            Assert.IsTrue(subResult.Success, subResult.Message);
            VerifySubscriber(set, userName, 1, false);


        }
        private void VerifySubscriber(Set set, string userName, int expectedCount, bool exists = true)
        {
            //check data
            using (var db = new Voat.Data.Models.voatEntities())
            {
                var dbSet = db.SubverseSets.FirstOrDefault(x => x.Name == set.Name && x.UserName == set.UserName && x.Type == (int)set.Type);
                Assert.IsNotNull(dbSet, "Can not find set as created");
                Assert.AreEqual(expectedCount, dbSet.SubscriberCount, "Subscriber Count Off on set");

                var subscription = db.SubverseSetSubscriptions.FirstOrDefault(x => x.SubverseSetID == dbSet.ID && x.UserName == userName);
                if (exists)
                {
                    Assert.IsNotNull(subscription, "Expecting a subscription but couldn't find it");
                }
                else
                {
                    Assert.IsNull(subscription, "Expecting to not find a subscription, but found one");
                }

                var count = db.SubverseSetSubscriptions.Count(x => x.SubverseSetID == dbSet.ID);
                Assert.AreEqual(expectedCount, count, "SubverseSetSubscription record entries are off from expected count");

            }
        }

        [TestMethod]
        [TestCategory("Set"), TestCategory("Command.Set")]
        public async Task CreateSet_Test_WhiteSpaceName()
        {
            var userName = "TestUser01";
            TestHelper.SetPrincipal(userName);

            var set = new Set() { Name = "Holy Hell", Title = "Some Title", Type = SetType.Normal, UserName = userName };
            var cmd = new UpdateSetCommand(set);
            var result = await cmd.Execute();
            Assert.IsFalse(result.Success, result.Message);

        }
        [TestMethod]
        [TestCategory("Set"), TestCategory("Command.Set")]
        public async Task CreateSet_Test_UnicodeName()
        {
            var userName = "TestUser01";
            TestHelper.SetPrincipal(userName);

            var set = new Set() { Name = "ÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ", Title = "Some Title", Type = SetType.Normal, UserName = userName };
            var cmd = new UpdateSetCommand(set);
            var result = await cmd.Execute();
            Assert.IsFalse(result.Success, result.Message);

        }


        [TestMethod]
        [TestCategory("Set"), TestCategory("Set.Permissions")]
        public void Set_Permission_Tests()
        {
            Set s = null;


            SetPermission perms = null;
            string loggedInUserName = null;


            s = new Set()
            {
                Name = "Front",
                Type = SetType.Front,
                UserName = "Joe",
                IsPublic = true,
            };
            //Unathenticated on Public Set
            TestHelper.SetPrincipal(null);
            loggedInUserName = (Thread.CurrentPrincipal.Identity.IsAuthenticated ? Thread.CurrentPrincipal.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, Thread.CurrentPrincipal.Identity);
            Assert.AreEqual(true, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

            //Owner on Non-Normal Set
            TestHelper.SetPrincipal("Joe");
            loggedInUserName = (Thread.CurrentPrincipal.Identity.IsAuthenticated ? Thread.CurrentPrincipal.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, Thread.CurrentPrincipal.Identity);
            Assert.AreEqual(true, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(true, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

            s = new Set()
            {
                Name = "RandomSet",
                Type = SetType.Normal,
                UserName = "Joe",
                IsPublic = false,
            };
            //Unathenticated on Private Set
            TestHelper.SetPrincipal(null);
            loggedInUserName = (Thread.CurrentPrincipal.Identity.IsAuthenticated ? Thread.CurrentPrincipal.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, Thread.CurrentPrincipal.Identity);
            Assert.AreEqual(false, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

            //Owner on Normal Private Set
            TestHelper.SetPrincipal("Joe");
            loggedInUserName = (Thread.CurrentPrincipal.Identity.IsAuthenticated ? Thread.CurrentPrincipal.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, Thread.CurrentPrincipal.Identity);
            Assert.AreEqual(true, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(true, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(true, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(true, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

            //Non-owner on Private Set
            TestHelper.SetPrincipal("Eddy");
            loggedInUserName = (Thread.CurrentPrincipal.Identity.IsAuthenticated ? Thread.CurrentPrincipal.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, Thread.CurrentPrincipal.Identity);
            Assert.AreEqual(false, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

            s = new Set()
            {
                Name = "SystemSet",
                Type = SetType.Normal,
                UserName = null,
                IsPublic = true,
            };
            //Unathenticated on Private Set
            TestHelper.SetPrincipal(null);
            loggedInUserName = (Thread.CurrentPrincipal.Identity.IsAuthenticated ? Thread.CurrentPrincipal.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, Thread.CurrentPrincipal.Identity);
            Assert.AreEqual(true, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

            //Non-Owner on Normal System Public Set
            TestHelper.SetPrincipal("Joe");
            loggedInUserName = (Thread.CurrentPrincipal.Identity.IsAuthenticated ? Thread.CurrentPrincipal.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, Thread.CurrentPrincipal.Identity);
            Assert.AreEqual(true, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");



        }

    }
}
