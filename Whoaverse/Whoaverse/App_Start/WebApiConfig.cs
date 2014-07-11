using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace Whoaverse
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration configuration)
        {
            // set default API response to JSON
            configuration.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            // configure routes
            configuration.Routes.MapHttpRoute("API list default subverses", "api/defaultsubverses",
                new { controller = "WebApi", action = "DefaultSubverses" });

            configuration.Routes.MapHttpRoute("API list banned hostnames", "api/bannedhostnames",
                new { controller = "WebApi", action = "BannedHostnames" });

            configuration.Routes.MapHttpRoute("API list top 200 subverses", "api/top200subverses",
                new { controller = "WebApi", action = "Top200Subverses" });

            configuration.Routes.MapHttpRoute("API frontpage", "api/frontpage",
                new { controller = "WebApi", action = "Frontpage" });  
        }
    }
}
