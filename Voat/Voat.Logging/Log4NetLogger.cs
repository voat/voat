

using Voat.Logging;
using log4net;
using log4net.Core;
using log4net.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Voat.Logging
{
    public class Log4NetLogger : BaseLogger
    {
        private static JsonSerializerSettings _jsonSettings;
        private string _loggerName;

        static Log4NetLogger() 
        {
            string path = String.Format("{0}.config", Assembly.GetExecutingAssembly().CodeBase);
            string fileName = LogSection.Instance.ConfigFile;
            if (!String.IsNullOrEmpty(fileName))
            {
                if (Path.GetFileName(fileName) == fileName)
                {
                    //it's just a file name, use current directory
                    path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), fileName);
                }
                else
                {
                    //assume it's rooted
                    path = fileName;
                }
            }

            log4net.Config.XmlConfigurator.Configure(new Uri(path));

            _jsonSettings = new JsonSerializerSettings();
            _jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            _jsonSettings.Converters.Add(new StringEnumConverter());
            _jsonSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
            _jsonSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;

        }
        public Log4NetLogger() : this("General")
        {

        }
        public Log4NetLogger(string loggerName) 
        {
            _loggerName = loggerName;
        }

        private Level Map(LogType type)
        {
            var level = Level.Off;

            switch (type)
            {
                case LogType.Critical:
                    level = Level.Critical;
                    break;
                case LogType.Trace:
                case LogType.Debug:
                    level = Level.Debug;
                    break;
                case LogType.Warning:
                    level = Level.Warn;
                    break;
                case LogType.Exception:
                    level = Level.Error;
                    break;
                default:
                    level = Level.Info;
                    break;
            }

            return level;
        }

        protected override void ProtectedLog(ILogInformation info) 
        {

            Debug.Print(info.ToString());

            ILog _log = LogManager.GetLogger(_loggerName);
            
            if (_log != null)
            {
                var level = Map(info.Type);
                if (_log.Logger.IsEnabledFor(level))
                {

                    LoggingEventData logData = new LoggingEventData();
                    logData.Message = info.Message;
                    logData.Level = level;

                    //copy info into log4net world to log.
                    logData.Properties = new PropertiesDictionary();
                    logData.Properties["UserName"] = info.UserName;
                    logData.Properties["Origin"] = info.Origin;
                    logData.Properties["LogType"] = info.Type.ToString();
                    logData.Properties["Message"] = info.Message;
                    logData.Properties["Category"] = info.Category;
                    logData.Properties["ActivityID"] = info.ActivityID;
                    logData.Properties["Data"] = (info.Data != null ? JsonConvert.SerializeObject(info.Data, _jsonSettings) : null);
                    logData.Properties["CreationDate"] = DateTime.UtcNow;

                    if (info.Exception != null)
                    {
                        logData.Properties["Exception"] = info.Exception.ToString();
                    }

                    _log.Logger.Log(new LoggingEvent(logData));
                }
            }
        }
    }
}
