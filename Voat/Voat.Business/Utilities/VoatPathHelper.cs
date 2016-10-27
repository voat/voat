using System;
using System.Configuration;
using Voat.Configuration;

namespace Voat.Utilities
{
    /// <summary>
    /// This utility can resolve image paths for Voat. The API benifits from qualified urls and the MVC UI benifits from partials which this utility supports
    /// </summary>
    public static class VoatPathHelper
    {
        private static string SiteRoot(bool provideProtocol, bool supportsContentDelivery, string forceDomain = null)
        {
            //Defaults
            string domain = "voat.co";
            string protocol = "https";

            if (supportsContentDelivery && Settings.UseContentDeliveryNetwork)
            {
                domain = "cdn.voat.co";
            }

            try
            {
                if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null)
                {
                    //domain = System.Web.HttpContext.Current.Request.Url.Authority;
                    protocol = System.Web.HttpContext.Current.Request.Url.Scheme;
                }
            }
            catch { }

            if (!String.IsNullOrEmpty(forceDomain))
            {
                domain = forceDomain;
            }

            return String.Format("{0}//{1}", (provideProtocol ? protocol + ":" : ""), domain);
        }

        public static string ThumbnailPath(string thumbnailFile, bool fullyQualified = false, bool provideProtocol = false)
        {
            if (String.IsNullOrEmpty(thumbnailFile))
            {
                return thumbnailFile;
            }

            //@if(Settings.UseContentDeliveryNetwork)
            //{
            //    < img src = "https://cdn.voat.co/thumbs/@Model.Thumbnail" alt = "@Model.LinkDescription" />
            //}
            //else
            //{
            //    < img src = "~/Thumbs/@Model.Thumbnail" alt = "@Model.LinkDescription" />
            //}

            return String.Format("{0}/thumbs/{1}", (fullyQualified ? SiteRoot(provideProtocol, true) : "~"), thumbnailFile);
        }

        public static string BadgePath(string badgeFile, bool fullyQualified = false, bool provideProtocol = false)
        {
            //Badges were rendering http://api-preview.voat.co/path... They need to point to the UI site as a root, so we change.
            var forceDomain = ConfigurationManager.AppSettings["ui.domain"];

            if (String.IsNullOrEmpty(badgeFile))
            {
                return badgeFile;
            }

            return String.Format("{0}/Graphics/Badges/{1}", (fullyQualified ? SiteRoot(provideProtocol, false, forceDomain) : "~"), badgeFile);
        }

        //This method is now an abination of everything badly I can think of. It started out like an innocent child but has been so corrupted throughout it's poor
        //life that what is left serves no purpose. Still, every method has a right to exist.
        //https://cdn.voat.co/avatars/@(userName).jpg
        public static string AvatarPath(string username, string avatarFileName, bool fullyQualified = false, bool provideProtocol = false, bool forceResolve = false)
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
                avatarUrl = $"{(fullyQualified ? SiteRoot(provideProtocol, true) : "~")}/{(Settings.UseContentDeliveryNetwork ? "avatars" : "Storage/Avatars")}/{avatarFileName}";
            }
            else
            {
                avatarUrl = String.Format("{0}/Graphics/thumb-placeholder.png", (fullyQualified ? SiteRoot(provideProtocol, false) : "~"));
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
    }
}
