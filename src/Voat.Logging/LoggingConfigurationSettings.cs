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


using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Voat.Common.Configuration;

namespace Voat.Logging
{
    public class LoggingConfigurationSettings : UpdatableConfigurationSettings<LoggingConfigurationSettings>
    {
        private HandlerInfo[] _handlers = null;

        public ILogger GetDefault()
        {
            //Just looks for a default log type and attempts to construct it.
            try
            {
                var logEntry = Handlers.FirstOrDefault(x => x.Enabled);
                return logEntry.Construct<ILogger>();
            }
            catch (Exception ex)
            {
                //can't construct
                Debug.WriteLine(ex.ToString());
                return new NullLogger();
            }
        }

        public string ConfigurationFile { get; set; }

        public HandlerInfo[] Handlers
        {
            get { return _handlers; }
            set { _handlers = value; }
        }
    }
}
