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
using Voat.Configuration;

namespace Voat.Utilities
{
    public class VoatUrlFormatter
    {
        //TODO: port this value to the config settings file
        private static string _defaultDomain = "voat.co";

        private static string _defaultProtocol = "https";
        private static string _domain = null;

        static VoatUrlFormatter()
        {
            _domain = _defaultDomain;
            try
            {
                //set domain if in .config file
                if (!String.IsNullOrEmpty(VoatSettings.Instance.SiteDomain))
                {
                    _domain = VoatSettings.Instance.SiteDomain;
                }
            }
            catch
            {
                /*no-op*/
            }
        }

        public static string UserProfile(string userName, string protocol = null)
        {
            if (!String.IsNullOrEmpty(userName))
            {
                return $"{GetProtocol(protocol)}//{_domain}/user/{userName}";
            }
            return "#";
        }

        public static string Subverse(string subverse, string protocol = null)
        {
            if (!String.IsNullOrEmpty(subverse))
            {
                return $"{GetProtocol(protocol)}//{_domain}/v/{subverse}";
            }
            return "#";
        }

        private static string GetProtocol(string protocol)
        {
            if (protocol == null)
            {
                return $"{_defaultProtocol}:";
            }
            else if (protocol.Length == 0)
            {
                return "";
            }
            else
            {
                return $"{protocol}:";
            }
        }
    }
}
