using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Command;
using Voat.Domain.Command.Base;

namespace Voat.Tests.CommandTests
{
    public class CounterCommand : QueuedCommand<CommandResponse>
    {
        public static int Counter { get; set; } = 0;


        public override Task<CommandResponse> Execute()
        {
            Counter = Counter + 1;
            return Task.FromResult(CommandResponse.FromStatus(Status.Success));
        }
    }

    [TestClass]
    public class QueuedCommandTests : BaseUnitTest
    {

        //A really simple queued command test
        [TestMethod]
        public void TestQueuedCounterCommand()
        {
            int flushCount = 5;
            CounterCommand.FlushCount = flushCount;
            CounterCommand.FlushSpan = TimeSpan.Zero;

            for (int i = 0; i < flushCount * 4; i++)
            {
                var cmd = new CounterCommand();
                cmd.Append();

                var expectedCount = flushCount * ((i + 1) / flushCount);
                Assert.AreEqual(expectedCount, CounterCommand.Counter);
            }
        }
    }
}
