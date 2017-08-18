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
using Voat.Tests.Infrastructure;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class CreateSubverseTests : BaseCommandTest
    {

        [TestMethod]
        [TestCategory("Command"), TestCategory("Command.Subverse")]
        public async Task NoAuth()
        {
            var cmd = new CreateSubverseCommand("mysub", "Some title", null);
            var response = await cmd.Execute();
            Assert.AreEqual(Status.Invalid, response.Status);
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Command.Subverse")]
        public async Task NullSubverseName()
        {
            var user = TestHelper.SetPrincipal("TestUser500CCP");
            var cmd = new CreateSubverseCommand(null, "Some title", null).SetUserContext(user);
            var response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, response.Message);
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Command.Subverse")]
        public async Task EmptySubverseName()
        {
            var user = TestHelper.SetPrincipal("TestUser500CCP");
            var cmd = new CreateSubverseCommand("", "Some title", null).SetUserContext(user);
            var response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, response.Message);
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Command.Subverse")]
        public async Task WhiteSpaceSubverseName()
        {
            var user = TestHelper.SetPrincipal("TestUser500CCP");
            var cmd = new CreateSubverseCommand("    ", "Some title", null).SetUserContext(user);
            var response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, response.Message);
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Command.Subverse")]
        public async Task InvalidSubverseName()
        {
            var user = TestHelper.SetPrincipal("TestUser500CCP");
            var cmd = new CreateSubverseCommand("My Subverse", "Some title", null).SetUserContext(user);
            var response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, response.Message);
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Command.Subverse")]
        public async Task ValidCreationTest()
        {
            var user = TestHelper.SetPrincipal(USERNAMES.User500CCP);
            var name = "UnitTestSubverse";
            var title = "Some title";
            var description = "Some Description";
            var cmd = new CreateSubverseCommand(name, title, description).SetUserContext(user);
            var response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status, response.Message);

            using (var db = new VoatDataContext())
            {
                var subverse = db.Subverse.FirstOrDefault(x => x.Name == name);
                Assert.IsNotNull(subverse, "Expecting to find subverse");
                Assert.AreEqual(name, subverse.Name);
                Assert.AreEqual(description, subverse.Description);
                //for backwards compat - original code set title automatically
                //Assert.AreEqual($"/v/{name}", subverse.Title);
                Assert.AreEqual(title, subverse.Title);
            }
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Command.Subverse")]
        public async Task DeniedCreationTest()
        {
            var user = TestHelper.SetPrincipal(USERNAMES.User0CCP);
            var name = "UnitTestSubverse2";
            var title = "Some title";
            var description = "Some Description";
            var cmd = new CreateSubverseCommand(name, title, description).SetUserContext(user);
            var response = await cmd.Execute();
            VoatAssert.IsValid(response, Status.Denied);

            //using (var db = new voatEntities())
            //{
            //    var subverse = db.Subverses.FirstOrDefault(x => x.Name == name);
            //    Assert.IsNotNull(subverse, "Expecting to find subverse");
            //    Assert.AreEqual(name, subverse.Name);
            //    Assert.AreEqual(title, subverse.Title);
            //    Assert.AreEqual(description, subverse.Description);
            //}
        }

    }
}
