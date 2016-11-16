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
            TestHelper.SetPrincipal("TestUser500CCP");
            var cmd = new CreateSubverseCommand(null, "Some title", null);
            var response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status);
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Command.Subverse")]
        public async Task EmptySubverseName()
        {
            TestHelper.SetPrincipal("TestUser500CCP");
            var cmd = new CreateSubverseCommand("", "Some title", null);
            var response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status);
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Command.Subverse")]
        public async Task WhiteSpaceSubverseName()
        {
            TestHelper.SetPrincipal("TestUser500CCP");
            var cmd = new CreateSubverseCommand("    ", "Some title", null);
            var response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status);
        }
        [TestMethod]
        [TestCategory("Command"), TestCategory("Command.Subverse")]
        public async Task InvalidSubverseName()
        {
            TestHelper.SetPrincipal("TestUser500CCP");
            var cmd = new CreateSubverseCommand("My Subverse", "Some title", null);
            var response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status);
        }

        [TestMethod]
        [TestCategory("Command"), TestCategory("Command.Subverse")]
        public async Task ValidCreationTest()
        {
            TestHelper.SetPrincipal("User500CCP");
            var name = "UnitTestSubverse";
            var title = "Some title";
            var description = "Some Description";
            var cmd = new CreateSubverseCommand(name, title, description);
            var response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status);

            using (var db = new voatEntities())
            {
                var subverse = db.Subverses.FirstOrDefault(x => x.Name == name);
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
            TestHelper.SetPrincipal("User0CCP");
            var name = "UnitTestSubverse2";
            var title = "Some title";
            var description = "Some Description";
            var cmd = new CreateSubverseCommand(name, title, description);
            var response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status);

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
