using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Voat.Common.Models
{
    public class OutgoingTraffic
    {
        public bool Enabled { get; set; } = true;

        public ProxySettings Proxy { get; set; } = new ProxySettings();

    }
    public class ProxySettings
    {
        public string Address { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public IWebProxy ToWebProxy()
        {
            WebProxy p = null;
            if (!String.IsNullOrEmpty(Address))
            {
                p = new WebProxy(new Uri(Address), true);
                if (!String.IsNullOrEmpty(UserName))
                {
                    p.Credentials = new NetworkCredential(UserName, Password);
                }
            }
            return p;
        }

    }
}
