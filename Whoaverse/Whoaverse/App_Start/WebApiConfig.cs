using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Whoaverse
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration configuration)
        {
            configuration.Routes.MapHttpRoute("API list default subverses", "api/defaultsubverses",
                new { controller = "WebApi", action = "DefaultSubverses" });
        }
    }
}
