using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Http
{
    public class HttpCookie
    {
        public HttpCookie(string name, string value = "")
        {
            this.Name = name;
            this.Value = value;
        }
        public string Name { get; set; }
        public string Value { get; set; }
        public DateTime Expires { get; set; }

    }
}
