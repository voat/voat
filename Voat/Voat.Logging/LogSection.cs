
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Voat.Logging
{
    [Serializable]
    [XmlRoot("voat.Logging")]
    public class LogSection : IConfigurationSectionHandler
    {
        private static LogSection _configSection = (LogSection)ConfigurationManager.GetSection("voat.Logging");
        private Logger[] _loggers = null;

        public static LogSection Instance
        {
            get { return _configSection; }
            set { _configSection = value; }
        }

        public ILogger GetDefault()
        {
            //Just looks for a default log type and attempts to construct it.
            try
            {
                var logEntry = Loggers.FirstOrDefault(x => x.Enabled);
                return logEntry.Construct();
            }
            catch (Exception ex)
            {
                //can't construct
                Debug.Print(ex.ToString());
                return new NullLogger();
            }
        }
        [XmlAttribute("configFile")]
        public string ConfigFile { get; set; }

        [XmlElement("logger")]
        public Logger[] Loggers
        {
            get { return _loggers; }
            set { _loggers = value; }
        }

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            try
            {
                LogSection sectionConfig = null;
                XmlSerializer serializer = new XmlSerializer(typeof(LogSection));
                using (var reader = new XmlNodeReader(section))
                {
                    sectionConfig = (LogSection)serializer.Deserialize(reader);
                }
                return sectionConfig;
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException(String.Format("Error parsing configuration for type {0}", this.GetType().FullName), ex, section);
            }
        }

    }

    [Serializable]
    public class Logger
    {
        [XmlAttribute("enabled")]
        public bool Enabled { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        public ILogger Construct()
        {
            var logType = System.Type.GetType(Type);
            return (ILogger)Activator.CreateInstance(logType);
        }
    }
}
