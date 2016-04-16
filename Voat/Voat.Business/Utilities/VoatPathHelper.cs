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

        private static string SiteRoot(bool provideProtocol, string forceDomain = null)
        {

            //Defaults
            string domain = "voat.co";
            string protocol = "https";

            if (Settings.UseContentDeliveryNetwork)
            {
                domain = "cdn.voat.co";
            }

            try
            {
                if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null)
                {
                    domain = System.Web.HttpContext.Current.Request.Url.Authority;
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

            return String.Format("{0}/thumbs/{1}", (fullyQualified ? SiteRoot(provideProtocol) : "~"), thumbnailFile);
        }
        public static string BadgePath(string badgeFile, bool fullyQualified = false, bool provideProtocol = false)
        {
            //Badges were rendering http://api-preview.voat.co/path... They need to point to the UI site as a root, so we change.
            var forceDomain = ConfigurationManager.AppSettings["ui.domain"];
            
            if (String.IsNullOrEmpty(badgeFile))
            {
                return badgeFile;
            }

            return String.Format("{0}/Graphics/Badges/{1}", (fullyQualified ? SiteRoot(provideProtocol, forceDomain) : "~"), badgeFile);
        }
        //https://cdn.voat.co/avatars/@(userName).jpg
        public static string AvatarPath(string username, bool fullyQualified = false, bool provideProtocol = false, bool forceResolve = false)
        {

            //if (Settings.UseContentDeliveryNetwork)
            //{
            //    < img src = "https://cdn.voat.co/avatars/@(userName).jpg" alt = "" class="user-avatar">
            //}
            //else
            //{
            //    <img src = "~/Storage/Avatars/@(userName).jpg" alt="" class="user-avatar">
            //}

            if (String.IsNullOrEmpty(username))
            {
                return username;
            }

            if (UserHelper.HasAvatar(username) != null || forceResolve)
            {
                //different paths depending on cdn or not
                return String.Format("{0}/{2}/{1}.jpg", (fullyQualified ? SiteRoot(provideProtocol) : "~"), username, (Settings.UseContentDeliveryNetwork ? "avatars" : "Storage/Avatars"));
            }
            else
            {
                return String.Format("{0}/Graphics/thumb-placeholder.png", (fullyQualified ? SiteRoot(provideProtocol) : "~"));
            }
        }
    }
}