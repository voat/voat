using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Domain.Command;
using Voat.Domain.Query;
using Voat.Tests.Repository;

namespace Voat.Tests.QueryTests
{
    [TestClass]
    public class QueryUserTests 
    {
        [TestMethod]
        [TestCategory("UserData")]
        public void UserData_Returns_Null_on_Invalid_User_Name()
        {
            string user = "";
            var q = new QueryUserData(user);
            var userData = q.ExecuteAsync().Result;
            //var userData = new Domain.UserData(user);
            Assert.IsNull(userData, String.Format("UserData expected to be null for value: '{0}'", user));

            user = null;
            q = new QueryUserData(null);
            userData = q.ExecuteAsync().Result;
            //userData = new Domain.UserData(null);
            Assert.IsNull(userData, String.Format("UserData expected to be null for value: '{0}'", "null"));

            user = "____________Doesn't__Exist_________";
            q = new QueryUserData(user);
            userData = q.ExecuteAsync().Result;
            //userData = new Domain.UserData(user);
            Assert.IsNull(userData, String.Format("UserData expected to be null for value: '{0}'", user));

        }
        [TestMethod, TestCategory("UserData"), ExpectedException(typeof(VoatNotFoundException))]
        public void UserData_ValidateUser_Constructor_1()
        {
            string user = "";
            var voatData = new Domain.UserData(user, true);
        }
        [TestMethod, TestCategory("UserData"), ExpectedException(typeof(VoatNotFoundException))]
        public void UserData_ValidateUser_Constructor_2()
        {
            string user = null;
            var voatData = new Domain.UserData(user, true);

        }
        [TestMethod, TestCategory("UserData"), ExpectedException(typeof(VoatNotFoundException))]
        public void UserData_ValidateUser_Constructor_3()
        {
            string user = "____________Doesn't__Exist_________";
            var voatData = new Domain.UserData(user, true);
        }
        [TestMethod]
        [TestCategory("UserData"), TestCategory("Subscriptions")]
        public async Task UserData_Information_SubscriptionTests()
        {
            //ensure users with no subscriptions don't error out
            var noSubUserName = "NoSubs";
            VoatDataInitializer.CreateUser(noSubUserName);

            var q = new QueryUserData(noSubUserName);
            var userData = q.Execute();
            //var userData = new Domain.UserData(noSubUserName);
            Assert.AreEqual(0, userData.Subscriptions.Count());

            //test subscription
            var subUserName = "HasSubs";
            VoatDataInitializer.CreateUser(subUserName);

            TestHelper.SetPrincipal(subUserName);
            var cmd = new SubscriptionCommand(Domain.Models.DomainType.Subverse, Domain.Models.SubscriptionAction.Subscribe, "unit");
            var x = await cmd.Execute();

            q = new QueryUserData(subUserName);
            userData = q.Execute();
            //userData = new Domain.UserData(subUserName);
            Assert.AreEqual(1, userData.Subscriptions.Count());
            Assert.AreEqual("unit", userData.Subscriptions.First());


        }
    }
}
