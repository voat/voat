using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Logging;
using Voat.Utilities.Components;

namespace Voat.Data
{
    public class VoatCategoryLogger : BaseLogger
    {
        private readonly string _categoryName;
        public VoatCategoryLogger(string categoryName)
        {
            _categoryName = categoryName;
        }
        protected override void ProtectedLog(ILogInformation info)
        {
            info.Category = _categoryName;
            EventLogger.Instance.Log(info);
        }
        
    }
    public class VoatLoggerProvider : ILoggerProvider
    {
        //private Func<string, LogLevel, bool> _filter;
        //public VoatLoggerProvider(Func<string, LogLevel, bool> filter)
        //{
        //    _filter = filter;
        //}

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return new VoatCategoryLogger(categoryName);
        }

        public void Dispose()
        {
            
        }
    }

    public class DatabaseLogger : QueuedLogger
    {
        public DatabaseLogger() : this(1, TimeSpan.Zero, LogType.All)
        { }
        public DatabaseLogger(int flushCount, TimeSpan flushSpan, LogType logLevel) : base(flushCount, flushSpan, logLevel)
        {

        }

        protected override async Task ProcessBatch(IEnumerable<ILogInformation> batch)
        {
            //Logging to EventLog
            using (var repo = new Repository()){
                await repo.Log(Map(batch));
            }
        }
        private IEnumerable<EventLog> Map(IEnumerable<ILogInformation> info)
        {
            return info.Select(x => Map(x)).ToList();
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
                if (info.Exception != null)
                {
                    e.Exception = Newtonsoft.Json.JsonConvert.SerializeObject(info.Exception, JsonSettings.FriendlySerializationSettings);
                }
                e.Data = info.DataSerialized;
                e.CreationDate = info.CreationDate;
                e.UserName = info.UserName;
            }

            return e;
        }
    }
}
