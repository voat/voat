using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Domain.Command;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.CommandTests.Framework
{
    [TestClass]
    public class FrameworkTests : BaseUnitTest
    {
        [TestMethod]
        public async Task Command_Stage_MaskTest()
        {
            CommandResponse response = null;
            CommandStage commandStage = CommandStage.All;
            TestCommand command = null;

            command = new TestCommand(commandStage);
            command.SetComandStageMask = commandStage;
            response = await command.Execute();
            Assert.AreEqual(4, command.StagesExecuted.Count);


            commandStage = CommandStage.None;
            command = new TestCommand(commandStage);
            command.SetComandStageMask = commandStage;
            response = await command.Execute();
            Assert.AreEqual(0, command.StagesExecuted.Count);


            commandStage = CommandStage.OnValidation;
            command = new TestCommand(commandStage);
            command.SetComandStageMask = commandStage;
            response = await command.Execute();
            Assert.AreEqual(1, command.StagesExecuted.Count);
        }



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

            //Queuing stages will be accomplished later and have been taken out of default stage pipeline
            //commandStage = CommandStage.OnQueuing;
            //command = new TestCommand(commandStage, false);
            //response = await command.Execute();
            //Assert.AreNotEqual(Status.Success, response.Status);
            //Assert.AreEqual(commandStage.HasValue ? commandStage.Value.ToString() : "", response.Message);

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
