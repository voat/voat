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
                if (!String.IsNullOrEmpty(Settings.SiteDomain))
                {
                    _domain = Settings.SiteDomain;
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
