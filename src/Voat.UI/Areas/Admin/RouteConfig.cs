using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using System;
using Voat.Configuration;
using Voat.Domain.Models;
using Voat.Utilities;
using Microsoft.AspNetCore.Routing.Constraints;
using Voat.Models.ViewModels;
using Voat.Common;

namespace Voat.UI.Areas.Admin
{
    public class RouteConfig
    {
        public static void RegisterRoutes(IRouteBuilder routes)
        {
            if (!VoatSettings.Instance.ManagementAreaName.IsEqual("Admin"))
            {
                routes.MapRoute(
                   name: "hideadmin",
                   template: "Admin/{*url}",
                   defaults: new { area = "", controller = "Error", action = "Type", type = ErrorType.Default }
                   );
            }

            routes.MapRoute(
                name: "admin",
                template: "{area}/{controller=Spam}/{action=Ban}");
        }
    }
}
