using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Logging;

namespace Voat.Tests.Logging
{
    //To enable these tests change the enabled property in the App.config to true for Log4NetLogger:
    //<logger enabled="true" name="Log4Net" type="Voat.Logging.Log4NetLogger, Voat.Logging" />

    [TestClass]
    public class LoggingTests : BaseUnitTest
    {
        private ILogger log = null;

        [TestInitialize]
        public void TestInitialize()
        {
            var logEntry = LogSection.Instance.Loggers.FirstOrDefault(x => x.Name == "Log4Net");
            if (logEntry.Enabled)
            {
                log = (ILogger)logEntry.Construct();
            }
        }

        [TestMethod]
        [TestCategory("Logging")]
        public void TestLogEntry()
        {
            if (log == null)
            {
                Assert.Inconclusive("Log4Net Logger disabled");
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

                using (var db = new voatEntities())
                {
                    var entry = db.EventLogs.FirstOrDefault(x => x.ActivityID == info.ActivityID.ToString());
                    Assert.IsNotNull(entry, "Crickie! Where is the log data at!?");
                    Assert.AreEqual(info.Type.ToString(), entry.Type);
                    Assert.AreEqual(info.Origin, entry.Origin);
                    Assert.AreEqual(info.Category, entry.Category);
                    Assert.AreEqual(info.Message, entry.Message);
                    Assert.AreEqual(JsonConvert.SerializeObject(new { url = "http://www.com" }), entry.Data);
                    Assert.AreEqual(info.ActivityID.ToString().ToUpper(), entry.ActivityID);
                }

            }

        }

        [TestMethod]
        [TestCategory("Logging")]
        public void TestLogException()
        {
            if (log == null)
            {
                Assert.Inconclusive("Log4Net Logger disabled");
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
                    using (var db = new voatEntities())
                    {
                        var entry = db.EventLogs.FirstOrDefault(x => x.ActivityID == activityID.ToString().ToUpper());
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
                Assert.Inconclusive("Log4Net Logger disabled");
            }
            else
            {
                var activityID = Guid.NewGuid();
                using (var durationLog = new DurationLogger(log, new LogInformation() { ActivityID = activityID, Type = LogType.Debug, Category = "Duration", UserName = "", Message = $"{this.GetType().Name}" }))
                {
                    System.Threading.Thread.Sleep(1000);
                }
                using (var db = new voatEntities())
                {
                    var entry = db.EventLogs.FirstOrDefault(x => x.ActivityID == activityID.ToString().ToUpper());
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
                Assert.Inconclusive("Log4Net Logger disabled");
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
                using (var db = new voatEntities())
                {
                    var entry = db.EventLogs.FirstOrDefault(x => x.ActivityID == activityID.ToString().ToUpper());
                    Assert.IsNull(entry, "Should not have log entry with specified minimum");
                }
            }
        }
    }
}
