using System.Net.Http.Headers;
using System.Web.Http;

namespace Whoaverse
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration configuration)
        {
            // set default API response to JSON
            configuration.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Error;

            // configure routes
            configuration.Routes.MapHttpRoute("API list default subverses", "api/defaultsubverses",
                new { controller = "WebApi", action = "DefaultSubverses" });

            configuration.Routes.MapHttpRoute("API list banned hostnames", "api/bannedhostnames",
                new { controller = "WebApi", action = "BannedHostnames" });

            configuration.Routes.MapHttpRoute("API list top 200 subverses", "api/top200subverses",
                new { controller = "WebApi", action = "Top200Subverses" });

            configuration.Routes.MapHttpRoute("API frontpage", "api/frontpage",
                new { controller = "WebApi", action = "Frontpage" });

            configuration.Routes.MapHttpRoute("API frontpage for given subverse", "api/subversefrontpage",
                new { controller = "WebApi", action = "SubverseFrontpage" });

            configuration.Routes.MapHttpRoute("API details for single submission", "api/singlesubmission",
                new { controller = "WebApi", action = "SingleSubmission" });

            configuration.Routes.MapHttpRoute("API details for single comment", "api/singlecomment",
                new { controller = "WebApi", action = "SingleComment" });
        }
    }
}
