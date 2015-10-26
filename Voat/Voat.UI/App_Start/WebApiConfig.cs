/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat, Inc.
All Rights Reserved.
*/

using System.Net.Http.Headers;
using System.Web.Http;
using Newtonsoft.Json;
using WebApiThrottle;

namespace Voat
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration configuration)
        {
            // configure throttling handler
            configuration.MessageHandlers.Add(new ThrottlingHandler
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

            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Error;

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

            configuration.Routes.MapHttpRoute("API comments for a single submission", "api/submissioncomments",
                new { controller = "WebApi", action = "SubmissionComments" });

            configuration.Routes.MapHttpRoute("Top 100 images by date", "api/top100imagesbydate",
                new { controller = "WebApi", action = "Top100ImagesByDate" });
        }
    }
}
