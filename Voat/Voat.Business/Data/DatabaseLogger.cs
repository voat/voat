using System;
using System.Collections.Generic;
using System.Text;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Logging;

namespace Voat.Data
{
    public class DatabaseLogger : QueuedLogger
    {
        public DatabaseLogger() : this(1, TimeSpan.Zero, LogType.All)
        { }
        public DatabaseLogger(int flushCount, TimeSpan flushSpan, LogType logLevel) : base(flushCount, flushSpan, logLevel)
        {

        }

        protected override void ProcessBatch(IEnumerable<ILogInformation> batch)
        {
            //Logging to EventLog
            using (var repo = new Repository())
            {
                foreach (var logEntry in batch)
                {
                    repo.Log(Map(logEntry));
                }
            }
        }
        private EventLog Map(ILogInformation info)
        {
            EventLog e = null;
            if (info != null)
            {
                e = new EventLog();

                e.Message = info.Message;
                e.Origin = info.Origin;
                e.Type = info.Type.ToString();
                e.Category = info.Category;
                e.ActivityID = info.ActivityID?.ToString();
                e.Exception = Newtonsoft.Json.JsonConvert.SerializeObject(info.Exception, JsonSettings.FriendlySerializationSettings);
                e.Data = Newtonsoft.Json.JsonConvert.SerializeObject(info.Data, JsonSettings.FriendlySerializationSettings);
                e.CreationDate = info.CreationDate;
                e.UserName = info.UserName;
            }

         

            return e;
        }
    }
}
