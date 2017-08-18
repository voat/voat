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
using Voat.Tests.Infrastructure;

namespace Voat.Tests.CommandTests
{
    //public class CounterCommand : QueuedCommand<CommandResponse>
    //{
    //    public static int Counter { get; set; } = 0;

    //    public static FlushDetector FlushSettings { get; set; } = new FlushDetector(10, TimeSpan.FromMinutes(5));

    //    protected override FlushDetector Flusher => FlushSettings;


    //    public override Task<CommandResponse> Execute()
    //    {
    //        Counter = Counter + 1;
    //        return Task.FromResult(CommandResponse.FromStatus(Status.Success));
    //    }
    //}
    //public class Counter2Command : QueuedCommand<CommandResponse>
    //{
    //    public static int Counter { get; set; } = 0;

    //    public static FlushDetector FlushSettings { get; set; } = new FlushDetector(10, TimeSpan.FromMinutes(5));

    //    protected override FlushDetector Flusher => FlushSettings;

    //    public override Task<CommandResponse> Execute()
    //    {
    //        Counter = Counter + 1;
    //        return Task.FromResult(CommandResponse.FromStatus(Status.Success));
    //    }
    //}
    [TestClass]
    public class QueuedCommandTests : BaseUnitTest
    {
        //A really simple queued command test
        [TestMethod]
        public void TestQueuedCounterCommand()
        {
            //int flushCount = 5;
            //CounterCommand.FlushSettings.FlushCount = flushCount;
            //CounterCommand.FlushSettings.FlushSpan = TimeSpan.Zero;

            //Counter2Command.FlushSettings.FlushCount = 7;
            //Counter2Command.FlushSettings.FlushSpan = TimeSpan.FromMilliseconds(100);

            //for (int i = 0; i < flushCount * 4; i++)
            //{
            //    var cmd = new CounterCommand();
            //    cmd.Append();

            //    var expectedCount = CounterCommand.FlushSettings.FlushCount * ((i + 1) / CounterCommand.FlushSettings.FlushCount);
            //    Assert.AreEqual(expectedCount, CounterCommand.Counter);
            //}


            //for (int i = 0; i < flushCount * 4; i++)
            //{
            //    var cmd = new Counter2Command();
            //    cmd.Append();

            //    var expectedCount = Counter2Command.FlushSettings.FlushCount * ((i + 1) / Counter2Command.FlushSettings.FlushCount);
            //    Assert.AreEqual(expectedCount, Counter2Command.Counter);
            //}



        }
    }
}
