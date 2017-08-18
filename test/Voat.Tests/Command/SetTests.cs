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
using System.Threading;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Tests.Infrastructure;
using Voat.Tests.Repository;

namespace Voat.Tests.CommandTests
{

    [TestClass]
    public class SetTests : BaseUnitTest
    {

        [TestMethod]
        [TestCategory("Set"), TestCategory("Command.Set")]
        public async Task Block_Lifecycle_Tests()
        {
            var userName = "BloqueSoze";
            var subName = SUBVERSES.Unit;

            TestDataInitializer.CreateUser(userName);
            var user = TestHelper.SetPrincipal(userName);

            //Verify No Front / Blocked Sets
            var userBlockQuery = new QueryUserBlocks(userName).SetUserContext(user);
            var userBlockResults = await userBlockQuery.ExecuteAsync();

            Assert.IsNotNull(userBlockResults, "Old McDonald had a farm ee i ee i o. And on that farm he shot some guys. Badda boom badda bing bang boom.");
            Assert.AreEqual(0, userBlockResults.Count(), "He is supposed to be Turkish. Some say his father was German. Nobody believed he was real.");

            var userSetQuery = new QueryUserSets(userName).SetUserContext(user);
            var userSetResults = await userSetQuery.ExecuteAsync();

            Assert.IsNotNull(userSetResults, "One cannot be betrayed if one has no people.");
            Assert.AreEqual(0, userSetResults.Count(), "A man can convince anyone he's somebody else, but never himself.");

            var currentSubscriberCount = 0;
            using (var db = new VoatDataContext())
            {
                var count = db.Subverse.First(x => x.Name == subName).SubscriberCount;
                currentSubscriberCount = count.HasValue ? count.Value : 0;

            }

            //Sub a user to front
            var domainReference = new DomainReference(DomainType.Subverse, subName);
            var blockCmd = new BlockCommand(domainReference.Type, domainReference.Name, true).SetUserContext(user);
            var blockResult = await blockCmd.Execute();

            //Verify Front is created
            userBlockQuery = new QueryUserBlocks(userName).SetUserContext(user);
            userBlockResults = await userBlockQuery.ExecuteAsync();

            Assert.IsNotNull(userBlockResults, "What about it, pretzel man? What's your story?");
            Assert.AreEqual(1, userBlockResults.Count(), "First day on the job, you know what I learned? How to spot a murderer.");
            Assert.IsTrue(userBlockResults.Any(x => x.Type == DomainType.Subverse && x.Name == subName) , "It was Keyser Soze, Agent Kujan. I mean the Devil himself. How do you shoot the Devil in the back? What if you miss?");
        
            userSetQuery = new QueryUserSets(userName).SetUserContext(user);
            userSetResults = await userSetQuery.ExecuteAsync();

            Assert.IsNotNull(userSetResults, "What the cops never figured out, and what I know now, was that these men would never break, never lie down, never bend over for anybody");
            Assert.AreEqual(1, userSetResults.Count(x => x.Type == SetType.Blocked), "Is it Friday already? ");
            var set = userSetResults.First(x => x.Type == SetType.Blocked);

            Assert.AreEqual(SetType.Blocked, set.Type, "I got a whole new problem when I post bail.");
            Assert.AreEqual(1, set.SubscriberCount, "I got a whole new problem when I post bail.");

            //Ensure Subverse Subscriber Count Updated
            using (var db = new VoatDataContext())
            {
                var tc = db.Subverse.First(x => x.Name == subName).SubscriberCount;
                var count = tc.HasValue ? tc.Value : 0;
                Assert.AreEqual(currentSubscriberCount, count, "");
                currentSubscriberCount = count;
            }

            //verify FRONT set has sub
            using (var repo = new Voat.Data.Repository())
            {
                var setList = await repo.GetSetListDescription(SetType.Blocked.ToString(), userName);
                Assert.IsTrue(setList.Any(x => x.Name == subName));
            }

            //Unsubscribe
            blockCmd = new BlockCommand(domainReference.Type, domainReference.Name, true).SetUserContext(user);
            blockResult = await blockCmd.Execute();
            VoatAssert.IsValid(blockResult);

            //Ensure Subverse Subscriber Count Updated
            using (var db = new VoatDataContext())
            {
                var count = db.Subverse.First(x => x.Name == subName).SubscriberCount;
                Assert.AreEqual(currentSubscriberCount, count.HasValue ? count.Value : 0, "");
            }

            //verify FRONT set has not sub
            using (var repo = new Voat.Data.Repository())
            {
                var setList = await repo.GetSetListDescription(SetType.Blocked.ToString(), userName);
                Assert.IsFalse(setList.Any(x => x.Name == subName));
            }

        }


        [TestMethod]
        [TestCategory("Set"), TestCategory("Command.Set")]
        public async Task FrontPage_Lifecycle_Tests()
        {
            var userName = "KeyserSoze";
            var subName = SUBVERSES.Unit;

            TestDataInitializer.CreateUser(userName);
            var user = TestHelper.SetPrincipal(userName);

            //Verify No Front / Blocked Sets
            var userSubQuery = new QueryUserSubscriptions(userName);
            var userSubResults = await userSubQuery.ExecuteAsync();

            Assert.IsNotNull(userSubResults, "Old McDonald had a farm ee i ee i o. And on that farm he shot some guys. Badda boom badda bing bang boom.");
            Assert.AreEqual(0, userSubResults[DomainType.Set].Count(), "He is supposed to be Turkish. Some say his father was German. Nobody believed he was real.");

            var userSetQuery = new QueryUserSets(userName);
            var userSetResults = await userSetQuery.ExecuteAsync();

            Assert.IsNotNull(userSetResults, "One cannot be betrayed if one has no people.");
            Assert.AreEqual(0, userSetResults.Count(), "A man can convince anyone he's somebody else, but never himself.");

            var currentSubscriberCount = 0;
            using (var db = new VoatDataContext())
            {
                var count = db.Subverse.First(x => x.Name == subName).SubscriberCount;
                currentSubscriberCount = count.HasValue ? count.Value : 0;

            }

            //Sub a user to front
            var domainReference = new DomainReference(DomainType.Subverse, subName);
            var subCmd = new SubscribeCommand(domainReference, SubscriptionAction.Toggle).SetUserContext(user);
            var subResult = await subCmd.Execute();
            VoatAssert.IsValid(subResult);

            //Verify Front is created
            userSubQuery = new QueryUserSubscriptions(userName, CachePolicy.None);
            userSubResults = await userSubQuery.ExecuteAsync();

            Assert.IsNotNull(userSubResults, "What about it, pretzel man? What's your story?");
            Assert.AreEqual(1, userSubResults[DomainType.Set].Count(), "First day on the job, you know what I learned? How to spot a murderer.");
            Assert.IsTrue(userSubResults[DomainType.Set].First() == new DomainReference(DomainType.Set, "Front", userName).FullName, "It was Keyser Soze, Agent Kujan. I mean the Devil himself. How do you shoot the Devil in the back? What if you miss?");

            userSetQuery = new QueryUserSets(userName).SetUserContext(user);
            userSetResults = await userSetQuery.ExecuteAsync();

            Assert.IsNotNull(userSetResults, "What the cops never figured out, and what I know now, was that these men would never break, never lie down, never bend over for anybody");
            Assert.AreEqual(1, userSetResults.Count(), "Is it Friday already? ");
            var set = userSetResults.First();

            Assert.AreEqual(SetType.Front, set.Type, "I got a whole new problem when I post bail.");
            Assert.AreEqual(1, set.SubscriberCount, "I got a whole new problem when I post bail.");

            //Ensure Subverse Subscriber Count Updated
            using (var db = new VoatDataContext())
            {
                var tc = db.Subverse.First(x => x.Name == subName).SubscriberCount;
                var count = tc.HasValue ? tc.Value : 0;
                Assert.AreEqual(currentSubscriberCount + 1,  count, "");
                currentSubscriberCount = count;
            }
            
            //verify FRONT set has sub
            using (var repo = new Voat.Data.Repository())
            {
                var setList = await repo.GetSetListDescription(SetType.Front.ToString(), userName);
                Assert.IsTrue(setList.Any(x => x.Name == subName));
            }

            //Unsubscribe
            subCmd = new SubscribeCommand(domainReference, SubscriptionAction.Toggle).SetUserContext(user);
            subResult = await subCmd.Execute();
            VoatAssert.IsValid(subResult);

            //Ensure Subverse Subscriber Count Updated
            using (var db = new VoatDataContext())
            {
                var count = db.Subverse.First(x => x.Name == subName).SubscriberCount;
                Assert.AreEqual(currentSubscriberCount - 1, count.HasValue ? count.Value : 0, "");
            }

            //verify FRONT set has not sub
            using (var repo = new Voat.Data.Repository())
            {
                var setList = await repo.GetSetListDescription(SetType.Front.ToString(), userName);
                Assert.IsFalse(setList.Any(x => x.Name == subName));
            }

        }


        [TestMethod]
        [TestCategory("Set"), TestCategory("Command.Set")]
        public async Task CreateSet_Test()
        {
            var userName = "TestUser01";
            var user = TestHelper.SetPrincipal(userName);

            var set = new Set() { Name = "HolyHell", Title = "Some Title", Type = SetType.Normal, UserName = userName };
            var cmd = new UpdateSetCommand(set).SetUserContext(user);
            var result = await cmd.Execute();
            VoatAssert.IsValid(result);

            Assert.AreNotEqual(0, result.Response.ID);
            var setID = result.Response.ID;
            VerifySubscriber(set, userName, 1);

            //Subscribe another user
            userName = "TestUser02";
            user = TestHelper.SetPrincipal(userName);
            var subCmd = new SubscribeCommand(new DomainReference(DomainType.Set, set.Name, set.UserName), SubscriptionAction.Subscribe).SetUserContext(user);
            var subResult = await subCmd.Execute();
            VoatAssert.IsValid(result);
            VerifySubscriber(set, userName, 2);

            //unsub user
            subCmd = new SubscribeCommand(new DomainReference(DomainType.Set, set.Name, set.UserName), SubscriptionAction.Unsubscribe).SetUserContext(user);
            subResult = await subCmd.Execute();
            VoatAssert.IsValid(result);
            VerifySubscriber(set, userName, 1, false);


        }
        private void VerifySubscriber(Set set, string userName, int expectedCount, bool exists = true)
        {
            //check data
            using (var db = new Voat.Data.Models.VoatDataContext())
            {
                var dbSet = db.SubverseSet.FirstOrDefault(x => x.Name == set.Name && x.UserName == set.UserName && x.Type == (int)set.Type);
                Assert.IsNotNull(dbSet, "Can not find set as created");
                Assert.AreEqual(expectedCount, dbSet.SubscriberCount, "Subscriber Count Off on set");

                var subscription = db.SubverseSetSubscription.FirstOrDefault(x => x.SubverseSetID == dbSet.ID && x.UserName == userName);
                if (exists)
                {
                    Assert.IsNotNull(subscription, "Expecting a subscription but couldn't find it");
                }
                else
                {
                    Assert.IsNull(subscription, "Expecting to not find a subscription, but found one");
                }

                var count = db.SubverseSetSubscription.Count(x => x.SubverseSetID == dbSet.ID);
                Assert.AreEqual(expectedCount, count, "SubverseSetSubscription record entries are off from expected count");

            }
        }

        [TestMethod]
        [TestCategory("Set"), TestCategory("Command.Set")]
        public async Task CreateSet_Test_WhiteSpaceName()
        {
            var userName = "TestUser01";
            var user = TestHelper.SetPrincipal(userName);

            var set = new Set() { Name = "Holy Hell", Title = "Some Title", Type = SetType.Normal, UserName = userName };
            var cmd = new UpdateSetCommand(set);
            var result = await cmd.Execute();
            Assert.IsFalse(result.Success, result.Message);

        }

        [TestMethod]
        [TestCategory("Set"), TestCategory("Command.Set")]
        public async Task CreateSet_Test_ReservedName_SetType()
        {
            var userName = "TestUser01";
            var user = TestHelper.SetPrincipal(userName);

            var set = new Set() { Name = SetType.Front.ToString(), Title = "Some Title", Type = SetType.Normal, UserName = userName };
            var cmd = new UpdateSetCommand(set);
            var result = await cmd.Execute();
            Assert.IsFalse(result.Success, result.Message);
        }

        [TestMethod]
        [TestCategory("Set"), TestCategory("Command.Set")]
        public async Task CreateSet_Test_ReservedName_SortAlgorithm()
        {
            var userName = "TestUser01";
            var user = TestHelper.SetPrincipal(userName);

            var set = new Set() { Name = SortAlgorithm.Hot.ToString(), Title = "Some Title", Type = SetType.Normal, UserName = userName };
            var cmd = new UpdateSetCommand(set);
            var result = await cmd.Execute();
            Assert.IsFalse(result.Success, result.Message);

        }

        [TestMethod]
        [TestCategory("Set"), TestCategory("Command.Set")]
        public async Task CreateSet_Test_UnicodeName()
        {
            var userName = "TestUser01";
            var user = TestHelper.SetPrincipal(userName);

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
            var user = TestHelper.SetPrincipal(null);
            loggedInUserName = (user.Identity.IsAuthenticated ? user.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, user.Identity);
            Assert.AreEqual(true, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

            //Owner on Non-Normal Set
            user = TestHelper.SetPrincipal("Joe");
            loggedInUserName = (user.Identity.IsAuthenticated ? user.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, user.Identity);
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
            user = TestHelper.SetPrincipal(null);
            loggedInUserName = (user.Identity.IsAuthenticated ? user.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, user.Identity);
            Assert.AreEqual(false, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

            //Owner on Normal Private Set
            user = TestHelper.SetPrincipal("Joe");
            loggedInUserName = (user.Identity.IsAuthenticated ? user.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, user.Identity);
            Assert.AreEqual(true, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(true, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(true, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(true, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

            //Non-owner on Private Set
            user = TestHelper.SetPrincipal("Eddy");
            loggedInUserName = (user.Identity.IsAuthenticated ? user.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, user.Identity);
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
            loggedInUserName = (user.Identity.IsAuthenticated ? user.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, user.Identity);
            Assert.AreEqual(true, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

            //Non-Owner on Normal System Public Set
            user = TestHelper.SetPrincipal("Joe");
            loggedInUserName = (user.Identity.IsAuthenticated ? user.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, user.Identity);
            Assert.AreEqual(true, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");



        }

    }
}
