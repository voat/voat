using System;

namespace Voat.Utilities
{
    /// <summary>
    /// This utility can resolve image paths for Voat. The API benifits from qualified urls and the MVC UI benifits from partials which this utility supports
    /// </summary>
    public static class VoatPathHelper
    {

        private static string SiteRoot(bool provideProtocol)
        {

            //Defaults
            string domain = "voat.co";
            string protocol = "https";
            try
            {
                if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null)
                {
                    domain = System.Web.HttpContext.Current.Request.Url.Authority;
                    protocol = System.Web.HttpContext.Current.Request.Url.Scheme;
                }
            }
            catch { }

            return String.Format("{0}//{1}", (provideProtocol ? protocol + ":" : ""), domain);
        }

        public static string ThumbnailPath(string thumbnailFile, bool fullyQualified = false, bool provideProtocol = false)
        {

            if (String.IsNullOrEmpty(thumbnailFile))
            {
                return thumbnailFile;
            }


            return String.Format("{0}/Thumbs/{1}",
                (fullyQualified ? SiteRoot(provideProtocol) : "~"),
                thumbnailFile
                );
        }
        public static string BadgePath(string badgeFile, bool fullyQualified = false, bool provideProtocol = false)
        {

            if (String.IsNullOrEmpty(badgeFile))
            {
                return badgeFile;
            }

            return String.Format("{0}/Graphics/Badges/{1}",
                (fullyQualified ? SiteRoot(provideProtocol) : "~"),
                badgeFile
                );
        }
        public static string AvatarPath(string username, bool fullyQualified = false, bool provideProtocol = false, bool forceResolve = false)
        {

            if (String.IsNullOrEmpty(username))
            {
                return username;
            }

            if (UserHelper.HasAvatar(username) != null || forceResolve)
            {
                return String.Format("{0}/Storage/Avatars/{1}.jpg",
                    (fullyQualified ? SiteRoot(provideProtocol) : "~"),
                    username
                    );
            }
            else
            {
                return String.Format("{0}/Graphics/thumb-placeholder.png", (fullyQualified ? SiteRoot(provideProtocol) : "~"));
            }
        }
    }
}