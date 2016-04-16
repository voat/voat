#region LICENSE

/*

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All portions of the code written by Voat, Inc. are Copyright(c) Voat, Inc.

    All Rights Reserved.

*/

#endregion LICENSE

using System;

using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;

namespace Voat.RulesEngine
{
    // <voat.Rules>
    //  <rules discoverRules = "false" >
    //    <add enabled="true" type="Voat.Rules.UpVoteSubmissionRule, Voat.Business" />
    //    <add enabled="true" type="Voat.Rules.UpVoteSubmissionRule, Voat.Business" />
    //    <add enabled="true" type="Voat.Rules.UpVoteSubmissionRule, Voat.Business" />
    //    <add enabled="true" type="Voat.Rules.UpVoteSubmissionRule, Voat.Business" />
    //    <add enabled="true" type="Voat.Rules.UpVoteSubmissionRule, Voat.Business" />
    //  </rules>
    //</voat.Rules>
    [Serializable]
    [XmlRoot("voat.Rules")]
    public class RuleSection : IConfigurationSectionHandler
    {
        private static RuleSection _instance = null;

        static RuleSection()
        {
            _instance = (RuleSection)ConfigurationManager.GetSection("voat.Rules");
        }

        public object Create(object parent, object configContext, XmlNode section)
        {
            try
            {
                RuleSection sectionConfig = null;
                XmlSerializer serializer = new XmlSerializer(typeof(RuleSection));
                using (var reader = new XmlNodeReader(section))
                {
                    sectionConfig = (RuleSection)serializer.Deserialize(reader);
                }
                return sectionConfig;
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException("Error parsing configuration for type " + typeof(RuleSection).FullName, ex, section);
            }
        }

        [XmlElement("rules")]
        public RulesConfiguration Configuration
        {
            get;
            set;
        }

        public static RuleSection Instance
        {
            get
            {
                return _instance;
            }
        }
    }

    [Serializable]
    public class RulesConfiguration
    {
        public RulesConfiguration()
        {
            Rules = new List<RuleEntry>();
        }

        [XmlAttribute("enabled")]
        public bool Enabled { get; set; }

        [XmlAttribute("discoverRules")]
        public bool DiscoverRules { get; set; }

        [XmlAttribute("discoverAssemblies")]
        public string DiscoverAssemblies { get; set; }

        [XmlElement("rule")]
        public List<RuleEntry> Rules { get; set; }
    }

    [Serializable]
    public class RuleEntry
    {
        [XmlAttribute("enabled")]
        public bool Enabled { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }
    }
}
