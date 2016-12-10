using System;
using System.Configuration;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Voat.Configuration;

namespace Voat.Caching
{
    [Serializable]
    [XmlRoot("voat.CacheHandler")]
    public class CacheHandlerSection : IConfigurationSectionHandler
    {
        private static CacheHandlerSection _config = (CacheHandlerSection)ConfigurationManager.GetSection("voat.CacheHandler");

        public static CacheHandlerSection Instance
        {
            get { return CacheHandlerSection._config; }
            set { CacheHandlerSection._config = value; }
        }

        [Serializable]
        public class HandlerInfo
        {
            [XmlAttribute("enabled")]
            public bool Enabled { get; set; }

            [XmlAttribute("type")]
            public string Type { get; set; }

            [XmlAttribute("arguments")]
            public string Arguments { get; set; }

            public ICacheHandler Construct()
            {
                var type = System.Type.GetType(this.Type);
                if (type != null)
                {
                    object[] args = ArgumentParser.Parse(Arguments);
                    return (ICacheHandler)Activator.CreateInstance(type, args);
                }
                throw new InvalidOperationException(String.Format("Can not construct CacheHandler: {0}", this.Type));
            }
        }

        public HandlerInfo Handler
        {
            get
            {
                if (Handlers != null)
                {
                    return Handlers.FirstOrDefault(x => x.Enabled);
                }
                return null;
            }
        }

        [XmlElement("handler")]
        public HandlerInfo[] Handlers
        {
            get;
            set;
        }

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            try
            {
                CacheHandlerSection sectionConfig = null;
                XmlSerializer serializer = new XmlSerializer(typeof(CacheHandlerSection));
                using (var reader = new XmlNodeReader(section))
                {
                    sectionConfig = (CacheHandlerSection)serializer.Deserialize(reader);
                }
                return sectionConfig;
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException("Error parsing configuration for type " + typeof(CacheHandlerSection).FullName, ex, section);
            }
        }
    }
}
