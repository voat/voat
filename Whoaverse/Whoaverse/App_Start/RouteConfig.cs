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
            routes.LowercaseUrls = true;

            // /subverses/create
            routes.MapRoute(
                name: "CreateSubverse",
                url: "subverses/create",
                defaults: new { controller = "Subverses", action = "CreateSubverse" }
            );

            // search
            routes.MapRoute(
                name: "Search",
                url: "search",
                defaults: new { controller = "Search", action = "SearchResults" }
            );

            // /subverses
            routes.MapRoute(
                name: "Subverses",
                url: "subverses/",
                defaults: new { controller = "Subverses", action = "Subverses" }
            );

            // /new
            routes.MapRoute(
                name: "SubversesNew",
                url: "subverses/{sortingmode}",
                defaults: new { controller = "Subverses", action = "NewestSubverses" }
            );            

            // comments/4
            routes.MapRoute(
                name: "comments",
                url: "comments/{id}",
                defaults: new { controller = "Home", action = "Comments" }
            );

            // user/someuserhere/thingtoshow
            routes.MapRoute(
                name: "userprofilefilter",
                url: "user/{id}/{whattodisplay}",
                defaults: new { controller = "Home", action = "UserProfile" }
            );

            // user/someuserhere
            routes.MapRoute(
                name: "user",
                url: "user/{id}",
                defaults: new { controller = "Home", action = "UserProfile" }
            );

            // help/pagetoshow
            routes.MapRoute(
                name: "Help",
                url: "help/{*pagetoshow}",
                defaults: new { controller = "Home", action = "Help" }
            );

            // about/pagetoshow
            routes.MapRoute(
                name: "About",
                url: "about/{*pagetoshow}",
                defaults: new { controller = "Home", action = "About" }
            );

            // cla
            routes.MapRoute(
                name: "cla",
                url: "cla/",
                defaults: new { controller = "Home", action = "Cla" }
            );

            // subscribe
            routes.MapRoute(
                name: "subscribe",
                url: "subscribe/{subverseName}",
                defaults: new { controller = "Home", action = "Subscribe" }
            );

            // unsubscribe
            routes.MapRoute(
                name: "unsubscribe",
                url: "unsubscribe/{subverseName}",
                defaults: new { controller = "Home", action = "UnSubscribe" }
            );

            // vote
            routes.MapRoute(
                name: "vote",
                url: "vote/{messageId}/{typeOfVote}",
                defaults: new { controller = "Home", action = "Vote" }
            );

            // vote comment
            routes.MapRoute(
                name: "votecomment",
                url: "votecomment/{commentId}/{typeOfVote}",
                defaults: new { controller = "Comment", action = "VoteComment" }
            );

            // editcomment
            routes.MapRoute(
                  "editcomment",
                  "editcomment/{id}",
                  new { controller = "Home", action = "Editcomment", id = UrlParameter.Optional }
             );

            // deletecomment
            routes.MapRoute(
                  "deletecomment",
                  "deletecomment/{id}",
                  new { controller = "Home", action = "DeleteComment", id = UrlParameter.Optional }
             );

            // editsubmission
            routes.MapRoute(
                  "editsubmission",
                  "editsubmission/{id}",
                  new { controller = "Home", action = "EditSubmission", id = UrlParameter.Optional }
             );

            // deletesubmission
            routes.MapRoute(
                  "deletesubmission",
                  "deletesubmission/{id}",
                  new { controller = "Home", action = "DeleteSubmission", id = UrlParameter.Optional }
             );

            // submit
            routes.MapRoute(
                name: "submit",
                url: "submit",
                defaults: new { controller = "Home", action = "Submit" }
            );

            // v/selectedsubverse/submit
            routes.MapRoute(
                name: "submitpost",
                url: "v/{selectedsubverse}/submit",
                defaults: new { controller = "Home", action = "Submit" }
            );

            // submitcomment
            routes.MapRoute(
                name: "submitcomment",
                url: "submitcomment",
                defaults: new { controller = "Home", action = "Submitcomment" }
            );

            // /random
            routes.MapRoute(
                name: "randomSubverse",
                url: "random/",
                defaults: new { controller = "Subverses", action = "Random" }
            );

            // /new
            routes.MapRoute(
                name: "FrontpageLatestPosts",
                url: "{sortingmode}",
                defaults: new { controller = "Home", action = "New" }
            );

            // v/subversetoedit/about/edit
            routes.MapRoute(
                name: "subverseSettings",
                url: "v/{subversetoshow}/about/edit",
                defaults: new { controller = "Subverses", action = "SubverseSettings" }
            );

            // v/subversetoshow
            routes.MapRoute(
                name: "SubverseIndex",
                url: "v/{subversetoshow}",
                defaults: new { controller = "Subverses", action = "Index" }
            );

            // domains/domainname.com
            routes.MapRoute(
                name: "DomainIndex",
                url: "domains/{domainname}.{ext}",
                defaults: new { controller = "Domains", action = "Index" }
            );

            // domains/domainname.com/new
            routes.MapRoute(
                name: "DomainIndexSorted",
                url: "domains/{domainname}.{ext}/{sortingmode}",
                defaults: new { controller = "Domains", action = "New", sortingmode = UrlParameter.Optional }
            );

            // v/subversetoshow/comments/123456
            routes.MapRoute(
                name: "SubverseComments",
                url: "v/{subversetoshow}/comments/{id}",
                defaults: new { controller = "Home", action = "Comments" }
            );

            // v/subversetoshow/sortingmode
            routes.MapRoute(
                name: "SubverseLatestPosts",
                url: "v/{subversetoshow}/{sortingmode}",
                defaults: new { controller = "Subverses", action = "New" }
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
