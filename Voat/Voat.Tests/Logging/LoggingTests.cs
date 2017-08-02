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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Logging;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.Logging
{
    [TestClass]
    public class LoggingTests : BaseUnitTest
    {
        private static ILogger log = null;
        private const string loggerName = "VoatDatabaseLogger";

        public override void ClassInitialize()
        {
            var logEntry = LoggingConfigurationSettings.Instance.Handlers.FirstOrDefault(x => x.Enabled);
            if (logEntry.Enabled)
            {
                log = logEntry.Construct<ILogger>();
            }
        }
        [TestMethod]
        [TestCategory("Logging")]
        public void TestLogLevels()
        {

            var dummyLogger = new DummyLogger(LogType.All);

            dummyLogger.LogLevel = LogType.All;
            Assert.AreEqual(true, dummyLogger.IsEnabledFor(LogType.All));
            Assert.AreEqual(true, dummyLogger.IsEnabledFor(LogType.Audit));
            Assert.AreEqual(true, dummyLogger.IsEnabledFor(LogType.Critical));
            Assert.AreEqual(true, dummyLogger.IsEnabledFor(LogType.Debug));
            Assert.AreEqual(true, dummyLogger.IsEnabledFor(LogType.Exception));
            Assert.AreEqual(true, dummyLogger.IsEnabledFor(LogType.Information));
            Assert.AreEqual(true, dummyLogger.IsEnabledFor(LogType.Trace));
            Assert.AreEqual(true, dummyLogger.IsEnabledFor(LogType.Warning));
            Assert.AreEqual(false, dummyLogger.IsEnabledFor(LogType.Off));

            dummyLogger.LogLevel = LogType.Off;
            Assert.AreEqual(false, dummyLogger.IsEnabledFor(LogType.All));
            Assert.AreEqual(false, dummyLogger.IsEnabledFor(LogType.Audit));
            Assert.AreEqual(false, dummyLogger.IsEnabledFor(LogType.Critical));
            Assert.AreEqual(false, dummyLogger.IsEnabledFor(LogType.Debug));
            Assert.AreEqual(false, dummyLogger.IsEnabledFor(LogType.Exception));
            Assert.AreEqual(false, dummyLogger.IsEnabledFor(LogType.Information));
            Assert.AreEqual(false, dummyLogger.IsEnabledFor(LogType.Trace));
            Assert.AreEqual(false, dummyLogger.IsEnabledFor(LogType.Warning));
            Assert.AreEqual(false, dummyLogger.IsEnabledFor(LogType.Off));

            dummyLogger.LogLevel = LogType.Information;
            Assert.AreEqual(true, dummyLogger.IsEnabledFor(LogType.All));
            Assert.AreEqual(true, dummyLogger.IsEnabledFor(LogType.Audit));
            Assert.AreEqual(true, dummyLogger.IsEnabledFor(LogType.Critical));
            Assert.AreEqual(false, dummyLogger.IsEnabledFor(LogType.Debug));
            Assert.AreEqual(true, dummyLogger.IsEnabledFor(LogType.Exception));
            Assert.AreEqual(true, dummyLogger.IsEnabledFor(LogType.Information));
            Assert.AreEqual(false, dummyLogger.IsEnabledFor(LogType.Trace));
            Assert.AreEqual(true, dummyLogger.IsEnabledFor(LogType.Warning));
            Assert.AreEqual(false, dummyLogger.IsEnabledFor(LogType.Off));


        }

        [TestMethod]
        [TestCategory("Logging")]
        public void TestLogEntry()
        {
            if (log == null)
            {
                Assert.Inconclusive($"{loggerName} Logger disabled");
            }
            else
            {
                LogInformation info = new LogInformation();
                info.Type = LogType.Information;
                info.Origin = "UnitTests";
                info.Category = "Unit Test";
                info.Message = "Test Message";
                info.Data = new { url = "http://www.com" };
                info.ActivityID = Guid.NewGuid();

                log.Log(info);

                System.Threading.Thread.Sleep(1000);

                using (var db = new VoatDataContext())
                {
                    var entry = db.EventLog.FirstOrDefault(x => x.ActivityID.ToUpper() == info.ActivityID.ToString().ToUpper());
                    Assert.IsNotNull(entry, "Crickie! Where is the log data at!?");
                    Assert.AreEqual(info.Type.ToString(), entry.Type);
                    Assert.AreEqual(info.Origin, entry.Origin);
                    Assert.AreEqual(info.Category, entry.Category);
                    Assert.AreEqual(info.Message, entry.Message);
                    Assert.AreEqual(JsonConvert.SerializeObject(info.Data), entry.Data);
                    Assert.AreEqual(info.ActivityID.ToString().ToUpper(), entry.ActivityID.ToUpper());
                }

            }

        }
        [TestMethod]
        [TestCategory("Logging")]
        public void TestLogEntryLevel()
        {
            if (log == null)
            {
                Assert.Inconclusive($"{loggerName} Logger disabled");
            }
            else
            {
                var level = log.LogLevel;
                try
                {
                    log.LogLevel = LogType.Critical;

                    LogInformation info = new LogInformation();
                    info.Type = LogType.Information;
                    info.Origin = "UnitTests";
                    info.Category = "Unit Test";
                    info.Message = "Test Message";
                    info.Data = new { url = "http://www.com" };
                    info.ActivityID = Guid.NewGuid();

                    log.Log(info);

                    using (var db = new VoatDataContext())
                    {
                        var entry = db.EventLog.FirstOrDefault(x => x.ActivityID == info.ActivityID.ToString());
                        Assert.IsNull(entry, "Crickie! Data magically appeared when it shouldn't have!?");
                    }

                }
                finally {
                    log.LogLevel = level;
                }

            }

        }
        [TestMethod]
        [TestCategory("Logging")]
        public void TestLogException()
        {
            if (log == null)
            {
                Assert.Inconclusive($"{loggerName} Logger disabled");
            }
            else
            {
                try
                {
                    //Open the worm hole to narnia
                    int wormhole = 42;
                    int idontcarewhathappens = 0;
                    int narnia = wormhole / idontcarewhathappens;
                }
                catch (Exception ex)
                {
                    var activityID = Guid.NewGuid();
                    log.Log(ex, (Guid?)activityID);
                    System.Threading.Thread.Sleep(1000);// batched logger writes on seperate thread, need to wait a bit
                    using (var db = new VoatDataContext())
                    {
                        var entry = db.EventLog.FirstOrDefault(x => x.ActivityID.ToUpper() == activityID.ToString().ToUpper());
                        Assert.IsNotNull(entry, "If this isn't here, perhaps the narnia wormhole is open?");
                    }
                }
            }
        }
        [TestMethod]
        [TestCategory("Logging")]
        public void TestLogDuration()
        {
            if (log == null)
            {
                Assert.Inconclusive($"{loggerName} Logger disabled");
            }
            else
            {
                var activityID = Guid.NewGuid();
                using (var durationLog = new DurationLogger(log, new LogInformation() { ActivityID = activityID, Type = LogType.Debug, Category = "Duration", UserName = "", Message = $"{this.GetType().Name}", Data = new { testNested = true, doesItWork = "Who knows" } }))
                {
                    System.Threading.Thread.Sleep(1000);
                }
                System.Threading.Thread.Sleep(1000);// batched logger writes on seperate thread, need to wait a bit
                using (var db = new VoatDataContext())
                {
                    var entry = db.EventLog.FirstOrDefault(x => x.ActivityID.ToUpper() == activityID.ToString().ToUpper());
                    Assert.IsNotNull(entry, "Well, that didn't work now did it");
                }
            }
        }
        [TestMethod]
        [TestCategory("Logging")]
        public void TestLogDurationWithMinimum()
        {
            if (log == null)
            {
                Assert.Inconclusive($"{loggerName} Logger disabled");
            }
            else
            {
                var activityID = Guid.NewGuid();
                using (var durationLog = new DurationLogger(log, 
                        new LogInformation() {
                            ActivityID = activityID,
                            Type = LogType.Debug,
                            Category = "Duration",
                            UserName = "",
                            Message = $"{this.GetType().Name}" },
                        TimeSpan.FromSeconds(10)))
                {
                    System.Threading.Thread.Sleep(1000);
                }
                using (var db = new VoatDataContext())
                {
                    var entry = db.EventLog.FirstOrDefault(x => x.ActivityID == activityID.ToString().ToUpper());
                    Assert.IsNull(entry, "Should not have log entry with specified minimum");
                }
            }
        }
        public class DummyLogger : BaseLogger
        {
            public DummyLogger(LogType logType) : base(logType) { }
            protected override void ProtectedLog(ILogInformation info)
            {
                throw new NotImplementedException();
            }
        }
    }
}
