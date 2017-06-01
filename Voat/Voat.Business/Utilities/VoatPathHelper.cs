#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using Microsoft.AspNetCore.Http;
using System;
using System.Configuration;
using Voat.Configuration;
using Voat.Domain.Models;

namespace Voat.Utilities
{
    //TODO: This class needs to be rewritten, this is all patched together over time and it's nasty imo - like warm beer. 
    //The above comment is even more so true now with the port to core. Pathing needs to be thought about a bit and simplified. 

    /// <summary>
    /// This utility can resolve image paths for Voat. The API benifits from qualified urls and the MVC UI benifits from partials which this utility supports
    /// </summary>
    public static class VoatPathHelper
    {
        //CORE_PORT: HttpContext Hack, backwards compat with HttpContext
        public static string ThumbnailPath(string thumbnailFile, bool fullyQualified = false, bool provideProtocol = false)
        {
            return ThumbnailPath(null, thumbnailFile, fullyQualified, provideProtocol);
        }

        public static string BadgePath(string badgeFile, bool fullyQualified = false, bool provideProtocol = false)
        {
            return BadgePath(null, badgeFile, fullyQualified, provideProtocol);
        }

        public static string AvatarPath(string username, string avatarFileName, bool fullyQualified = false, bool provideProtocol = false, bool forceResolve = false)
        {
            return AvatarPath(null, username, avatarFileName, fullyQualified, provideProtocol, forceResolve);
        }


        private static string SiteRoot(HttpContext context, bool provideProtocol, bool supportsContentDelivery, string forceDomain = null)
        {
            //Defaults
            string domain = Settings.SiteDomain;
            string protocol = Settings.ForceHTTPS ? "https" : "http";

            if (supportsContentDelivery && Settings.UseContentDeliveryNetwork)
            {
                domain = "cdn.voat.co";
            }
            try
            {
                if (context != null && context.Request != null)
                {
                    //domain = System.Web.HttpContext.Current.Request.Url.Authority;
                    protocol = context.Request.Scheme;
                }
            }
            catch { }
            if (!String.IsNullOrEmpty(forceDomain))
            {
                domain = forceDomain;
            }

            return String.Format("{0}//{1}", (provideProtocol ? protocol + ":" : ""), domain);
        }

        public static string ThumbnailPath(HttpContext context, string thumbnailFile, bool fullyQualified = false, bool provideProtocol = false)
        {
            if (String.IsNullOrEmpty(thumbnailFile))
            {
                return thumbnailFile;
            }

            return String.Format("{0}/thumbs/{1}", (fullyQualified ? SiteRoot(context, provideProtocol, true) : "~"), thumbnailFile);
        }

        public static string BadgePath(HttpContext context, string badgeFile, bool fullyQualified = false, bool provideProtocol = false)
        {
            //Badges were rendering http://api-preview.voat.co/path... They need to point to the UI site as a root, so we change.
            var forceDomain = ConfigurationManager.AppSettings["ui.domain"];

            if (String.IsNullOrEmpty(badgeFile))
            {
                return badgeFile;
            }

            return String.Format("{0}/Graphics/Badges/{1}", (fullyQualified ? SiteRoot(context, provideProtocol, false, forceDomain) : "~"), badgeFile);
        }

        //This method is now an abination of everything badly I can think of. It started out like an innocent child but has been so corrupted throughout it's poor
        //life that what is left serves no purpose. Still, every method has a right to exist.
        //https://cdn.voat.co/avatars/@(userName).jpg
        public static string AvatarPath(HttpContext context, string username, string avatarFileName, bool fullyQualified = false, bool provideProtocol = false, bool forceResolve = false)
        {
            if (String.IsNullOrEmpty(username))
            {
                return username;
            }

            var avatarUrl = String.Empty;
            //var avatarFileName = UserHelper.GetAvatar(username);
            if (forceResolve || !String.IsNullOrEmpty(avatarFileName))
            {
                avatarFileName = String.IsNullOrEmpty(avatarFileName) ? $"{username}.jpg" : avatarFileName;
                //different paths depending on cdn or not
                avatarUrl = $"{(fullyQualified ? SiteRoot(context, provideProtocol, true) : "~")}/{(Settings.UseContentDeliveryNetwork ? "avatars" : "Storage/Avatars")}/{avatarFileName}";
            }
            else
            {
                avatarUrl = String.Format("{0}/Graphics/thumb-placeholder.png", (fullyQualified ? SiteRoot(context, provideProtocol, false) : "~"));
            }

            return avatarUrl;

        }

        public static string CommentsPagePath(string subverse, int submissionID, int? commentID = null)
        {
            var commentPath = commentID.HasValue ? $"/{commentID.Value.ToString()}" : "";

            //long
            //return $"/v/{subverse}/comments/{submissionID.ToString()}{commentPath}";
            //short
            return $"/v/{subverse}/{submissionID.ToString()}{commentPath}";
        }
        public static string CommentsPagePath(string subverse, int submissionID, CommentSortAlgorithm sort, object queryString = null)
        {
            //var sortPath = $"/{sort.Value.ToString().ToLower()}" : "";

            //long
            //return $"/v/{subverse}/comments/{submissionID.ToString()}{commentPath}";
            //short
            return $"/v/{subverse}/{submissionID.ToString()}/{sort.ToString().ToLower()}";
        }

        public static string BasePath(DomainReference domainReference)
        {
            string basePath = "";
            switch (domainReference.Type)
            {
                case DomainType.Set:
                    basePath = $"/s/{domainReference.Name}" + (!String.IsNullOrEmpty(domainReference.OwnerName) ? CONSTANTS.SET_SEPERATOR + $"{domainReference.OwnerName}" : "");
                    break;
                case DomainType.Subverse:
                    basePath = $"/v/{domainReference.Name}";
                    break;
                case DomainType.User:
                    basePath = $"/user/{domainReference.Name}";
                    break;
            }
            return basePath;

        }
    }
}
