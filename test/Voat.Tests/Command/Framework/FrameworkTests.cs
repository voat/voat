using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Command;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.CommandTests.Framework
{
    [TestClass]
    public class FrameworkTests : BaseUnitTest
    {
        [TestMethod]
        public async Task Command_Stage_Tests()
        {
            CommandResponse response = null;
            CommandStage? commandStage = null;
            TestCommand command = null;

            command = new TestCommand(commandStage);
            response = await command.Execute();
            Assert.AreEqual(Status.Success, response.Status);
            Assert.AreEqual(commandStage.HasValue ? commandStage.Value.ToString() : "", response.Message);

            commandStage = CommandStage.OnAuthorization;
            command = new TestCommand(commandStage, false);
            response = await command.Execute();
            Assert.AreNotEqual(Status.Success, response.Status);
            Assert.AreEqual(commandStage.HasValue ? commandStage.Value.ToString() : "", response.Message);

            commandStage = CommandStage.OnValidation;
            command = new TestCommand(commandStage, false);
            response = await command.Execute();
            Assert.AreNotEqual(Status.Success, response.Status);
            Assert.AreEqual(commandStage.HasValue ? commandStage.Value.ToString() : "", response.Message);

            commandStage = CommandStage.OnQueuing;
            command = new TestCommand(commandStage, false);
            response = await command.Execute();
            Assert.AreNotEqual(Status.Success, response.Status);
            Assert.AreEqual(commandStage.HasValue ? commandStage.Value.ToString() : "", response.Message);

            commandStage = CommandStage.OnExecuting;
            command = new TestCommand(commandStage, false);
            response = await command.Execute();
            Assert.AreNotEqual(Status.Success, response.Status);
            Assert.AreEqual(commandStage.HasValue ? commandStage.Value.ToString() : "", response.Message);

            commandStage = CommandStage.OnExecuted;
            command = new TestCommand(commandStage, false);
            response = await command.Execute();
            Assert.AreNotEqual(Status.Success, response.Status);
            Assert.AreEqual(commandStage.HasValue ? commandStage.Value.ToString() : "", response.Message);

            //We expect the stagemask to skip the stage throwing an error
            commandStage = CommandStage.OnAuthorization;
            command = new TestCommand(commandStage, false);
            command.SetComandStageMask = CommandStage.OnValidation;
            response = await command.Execute();
            Assert.AreEqual(Status.Success, response.Status);

        }
    }
}
