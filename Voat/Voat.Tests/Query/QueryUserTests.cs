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
using Voat.Data;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Tests.Repository;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.QueryTests
{
    [TestClass]
    public class QueryUserTests : BaseUnitTest
    {
        
        [TestMethod, TestCategory("UserData")]
        public void UserData_ValidateUser_Constructor_1()
        {
            VoatAssert.Throws<ArgumentException>(() => {
                string user = "";
                var voatData = new Domain.UserData(user, true);
            });
        }
        [TestMethod, TestCategory("UserData")]
        public void UserData_ValidateUser_Constructor_2()
        {
            VoatAssert.Throws<ArgumentException>(() => {
                string user = null;
                var voatData = new Domain.UserData(user, true);
            });

        }
        [TestMethod, TestCategory("UserData")]
        public void UserData_ValidateUser_Constructor_3()
        {
            VoatAssert.Throws<ArgumentException>(() => {
                string user = "____________Doesn't__Exist_________";
                var voatData = new Domain.UserData(user, true);
            });
        }

        [TestMethod, TestCategory("UserData")]
        public void UserData_ValidateUser_Constructor_4()
        {
            VoatAssert.Throws<VoatNotFoundException>(() => {
                string user = "MyName-Is-Me";
                var voatData = new Domain.UserData(user, true);
            });
        }
        [TestMethod, TestCategory("UserData")]
        public void UserData_ValidateUser_Constructor_5()
        {
            VoatAssert.Throws<VoatNotFoundException>(() => {
                string user = "DoesntExist";
                var voatData = new Domain.UserData(user, true);
            });
        }

        [TestMethod]
        [TestCategory("UserData"), TestCategory("Subscriptions")]
        public async Task UserData_Information_SubscriptionTests()
        {
            //ensure users with no subscriptions don't error out
            var noSubUserName = "NoSubs";
            TestDataInitializer.CreateUser(noSubUserName);

            var userData = new Domain.UserData(noSubUserName);
            //var userData = new Domain.UserData(noSubUserName);
            Assert.AreEqual(0, userData.SubverseSubscriptions.Count());
            Assert.AreEqual(false, userData.HasSubscriptions());
            Assert.AreEqual(false, userData.HasSubscriptions(DomainType.Subverse));

            //test subscription
            var subUserName = "HasSubs";
            TestDataInitializer.CreateUser(subUserName);

            var user = TestHelper.SetPrincipal(subUserName);
            var cmd = new SubscribeCommand(new DomainReference(DomainType.Subverse, SUBVERSES.Unit), Domain.Models.SubscriptionAction.Subscribe).SetUserContext(user);
            var x = await cmd.Execute();
            VoatAssert.IsValid(x);

            userData = new Domain.UserData(subUserName);

            Assert.AreEqual(1, userData.SubverseSubscriptions.Count());
            Assert.AreEqual(SUBVERSES.Unit, userData.SubverseSubscriptions.First());
            Assert.AreEqual(true, userData.HasSubscriptions());
            Assert.AreEqual(true, userData.HasSubscriptions(DomainType.Subverse));
            Assert.AreEqual(true, userData.IsUserSubverseSubscriber(SUBVERSES.Unit));

        }
        [TestMethod]
        [TestCategory("Query"), TestCategory("Query.Comment"), TestCategory("Comment"), TestCategory("UserData")]
        public async Task QueryUserComments()
        {
            var userName = "UnitTestUser21";
            var user = TestHelper.SetPrincipal(userName);
            var cmd = new CreateCommentCommand(1, null, "My pillow looks like jello").SetUserContext(user);
            var x = await cmd.Execute();
            VoatAssert.IsValid(x);

            var q = new QueryUserComments(userName, SearchOptions.Default).SetUserContext(user);
            var r = await q.ExecuteAsync();
            Assert.AreEqual(true, r.Any(w => w.Content == "My pillow looks like jello"));

        }
        [TestMethod]
        [TestCategory("Query"), TestCategory("Query.Comment"), TestCategory("Anon"), TestCategory("Comment"), TestCategory("Comment.Segment")]
        public async Task QueryUserComments_Anon()
        {
            var userName = "UnitTestUser22";
            var user = TestHelper.SetPrincipal(userName);
            var cmd = new CreateCommentCommand(2, null, "You can never know I said this: Bollocks").SetUserContext(user);
            var x = await cmd.Execute();
            VoatAssert.IsValid(x);

            var q = new QueryUserComments(userName, SearchOptions.Default);
            var r = await q.ExecuteAsync();
            Assert.AreEqual(false, r.Any(w => w.Content == "You can never know I said this: Bollocks"));
        }

        [TestMethod]
        [TestCategory("Query"), TestCategory("Query.Submission"), TestCategory("Submission"), TestCategory("UserData")]
        public async Task QueryUserSubmissions()
        {
            var userName = "UnitTestUser23";
            var content = "@Fuzzy made fun of my if statements. Says my if statements look *off* and that they aren't as good as other peoples if statements. :(";
            var user = TestHelper.SetPrincipal(userName);
            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Unit, Title = "This broke my heart", Content = content }).SetUserContext(user);
            var x = await cmd.Execute();
            VoatAssert.IsValid(x);

            var q = new QueryUserSubmissions(userName, SearchOptions.Default).SetUserContext(user);
            var r = await q.ExecuteAsync();
            Assert.AreEqual(true, r.Any(w => w.Content == content));
        }


        [TestMethod]
        [TestCategory("Query"), TestCategory("Query.Comment"), TestCategory("Anon"), TestCategory("Comment"), TestCategory("Comment.Segment")]
        public async Task QueryUserSubmissions_Anon()
        {
            var userName = "UnitTestUser24";
            var content = "I have emotional issues whenever I see curly braces";
            var user = TestHelper.SetPrincipal(userName);
            var cmd = new CreateSubmissionCommand(new Domain.Models.UserSubmission() { Subverse = SUBVERSES.Anon, Title = "This is my biggest secret", Content = content }).SetUserContext(user);
            var x = await cmd.Execute();
            VoatAssert.IsValid(x);

            var q = new QueryUserSubmissions(userName, SearchOptions.Default).SetUserContext(user);
            var r = await q.ExecuteAsync();
            Assert.AreEqual(false, r.Any(w => w.Content == content));
        }
    }
}
