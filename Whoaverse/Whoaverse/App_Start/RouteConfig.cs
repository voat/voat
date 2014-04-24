/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Whoaverse
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // comments/4
            routes.MapRoute(
                name: "comments",
                url: "comments/{id}",
                defaults: new
                {
                    controller = "Home",
                    action = "comments",
                }
            );

            // user/someuserhere/thingtoshow
            routes.MapRoute(
                name: "userprofilefilter",
                url: "user/{id}/{whattodisplay}",
                defaults: new
                {
                    controller = "Home",
                    action = "user",
                }
            );

            // user/someuserhere
            routes.MapRoute(
                name: "user",
                url: "user/{id}",
                defaults: new
                {
                    controller = "Home",
                    action = "user",
                }
            );

            // help/pagetoshow
            routes.MapRoute(
                name: "Help",
                url: "help/{*pagetoshow}",
                defaults: new { controller = "Home", action = "help" }
            );

            // about/pagetoshow
            routes.MapRoute(
                name: "About",
                url: "about/{*pagetoshow}",
                defaults: new { controller = "Home", action = "about" }
            );

            // cla
            routes.MapRoute(
                name: "cla",
                url: "cla/",
                defaults: new
                {
                    controller = "Home",
                    action = "cla",
                }
            );

            // submit
            routes.MapRoute(
                name: "submit",
                url: "submit",
                defaults: new
                {
                    controller = "Home",
                    action = "submit",
                }
            );

            // submitcomment
            routes.MapRoute(
                name: "submitcomment",
                url: "submitcomment",
                defaults: new
                {
                    controller = "Home",
                    action = "submitcomment",
                }
            );

            // /random
            routes.MapRoute(
                name: "randomSubverse",
                url: "random/",
                defaults: new { controller = "Subverses", action = "random" }
            );

            // /new
            routes.MapRoute(
                name: "FrontpageLatestPosts",
                url: "{sortingmode}",
                defaults: new { controller = "Home", action = "new" }
            );

            // v/subversetoshow
            routes.MapRoute(
                name: "SubverseIndex",
                url: "v/{subversetoshow}",
                defaults: new { controller = "Subverses", action = "Index" }
            );

            // v/subversetoshow/comments/123456
            routes.MapRoute(
                name: "SubverseComments",
                url: "v/{subversetoshow}/comments/{id}",
                defaults: new { controller = "Home", action = "comments" }
            );

            // v/subversetoshow/sortingmode
            routes.MapRoute(
                name: "SubverseLatestPosts",
                url: "v/{subversetoshow}/{sortingmode}",
                defaults: new { controller = "Subverses", action = "new" }
            );


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new
                {
                    controller = "Home",
                    action = "Index",
                    id = UrlParameter.Optional
                }
            );


        }
    }
}
