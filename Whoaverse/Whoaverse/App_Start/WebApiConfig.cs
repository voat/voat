﻿using System.Net.Http.Headers;
using System.Web.Http;
using WebApiThrottle;

namespace Whoaverse
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration configuration)
        {
            // configure throttling handler
            configuration.MessageHandlers.Add(new ThrottlingHandler()
            {
                Policy = new ThrottlePolicy(perSecond: 1, perMinute: 20, perHour: 200, perDay: 1500, perWeek: 3000)
                {
                    IpThrottling = true,
                    EndpointThrottling = true
                },
                Repository = new CacheRepository()
            });

            // set default API response to JSON
            configuration.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));            

            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Error;

            //configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize;
            //configuration.Formatters.JsonFormatter.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects; 

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

            configuration.Routes.MapHttpRoute("API details for a subverse", "api/subverseinfo",
                new { controller = "WebApi", action = "SubverseInfo" });

            configuration.Routes.MapHttpRoute("API details for a user", "api/userinfo",
                new { controller = "WebApi", action = "UserInfo" });

            configuration.Routes.MapHttpRoute("API details for a badge", "api/badgeinfo",
                new { controller = "WebApi", action = "BadgeInfo" });
        }
    }
}
