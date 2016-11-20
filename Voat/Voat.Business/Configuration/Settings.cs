using System.Collections.Generic;

namespace Voat.Configuration
{
    public class Settings
    {
        #region AppSettings Accessors

        internal static Dictionary<string, object> configValues = new Dictionary<string, object>();

        public static bool IsVoatBranded
        {
            get
            {
                return SiteName.Equals("Voat", System.StringComparison.OrdinalIgnoreCase);
            }
        }

        public static bool AdsEnabled
        {
            get
            {
                return GetValue(CONFIGURATION.AdsEnabled, false);
            }
        }

        public static bool ApiKeyCreationEnabled
        {
            get
            {
                return GetValue(CONFIGURATION.ApiKeyCreationEnabled, false);
            }
        }

        public static bool CacheDisabled
        {
            get
            {
                return GetValue(CONFIGURATION.CacheDisabled, false);
            }
        }

        public static bool CaptchaDisabled
        {
            get
            {
                return GetValue(CONFIGURATION.CaptchaDisabled, false);
            }
        }

        public static int DailyCommentPostingQuota
        {
            get
            {
                return GetValue(CONFIGURATION.DailyCommentPostingQuota, 20);
            }
        }

        public static int DailyCommentPostingQuotaForNegativeScore
        {
            get
            {
                return GetValue(CONFIGURATION.DailyCommentPostingQuotaForNegativeScore, 10);
            }
        }

        public static int DailyCrossPostingQuota
        {
            get
            {
                return GetValue(CONFIGURATION.DailyCrossPostingQuota, 2);
            }
        }

        public static int DailyGlobalPostingQuota
        {
            get
            {
                return GetValue(CONFIGURATION.DailyGlobalPostingQuota, 5);
            }
        }

        public static int DailyPostingQuotaForNegativeScore
        {
            get
            {
                return GetValue(CONFIGURATION.DailyPostingQuotaForNegativeScore, 3);
            }
        }

        public static int DailyPostingQuotaPerSub
        {
            get
            {
                return GetValue(CONFIGURATION.DailyPostingQuotaPerSub, 10);
            }
        }

        public static int DailyVotingQuota
        {
            get
            {
                return GetValue(CONFIGURATION.DailyVotingQuota, 100);
            }
        }

        public static string DestinationPathAvatars
        {
            get
            {
                return GetValue(CONFIGURATION.DestinationPathAvatars, "~/Storage/Avatars");
            }
        }

        public static string DestinationPathThumbs
        {
            get
            {
                return GetValue(CONFIGURATION.DestinationPathThumbs, "~/Storage/Thumbs");
            }
        }

        public static string EmailServiceKey
        {
            get
            {
                return GetValue(CONFIGURATION.EmailServiceKey, "");
            }
        }

        public static bool ForceHTTPS
        {
            get
            {
                return GetValue(CONFIGURATION.ForceHTTPS, true);
            }
        }
        public static string FooterText
        {
            get
            {
                return GetValue(CONFIGURATION.FooterText, "");
            }
        }
        public static int HourlyCommentPostingQuota
        {
            get
            {
                return GetValue(CONFIGURATION.HourlyCommentPostingQuota, 10);
            }
        }

        public static int HourlyGlobalPostingQuota
        {
            get
            {
                return GetValue(CONFIGURATION.HourlyGlobalPostingQuota, 3);
            }
        }

        public static int HourlyPostingQuotaPerSub
        {
            get
            {
                return GetValue(CONFIGURATION.HourlyPostingQuotaPerSub, 3);
            }
        }

        public static bool LegacyApiEnabled
        {
            get
            {
                return GetValue(CONFIGURATION.LegacyApiEnabled, false);
            }
        }

        public static int MaxAllowedAccountsFromSingleIP
        {
            get
            {
                return GetValue(CONFIGURATION.MaxAllowedAccountsFromSingleIP, 100);
            }
        }

        public static int MaximumOwnedSets
        {
            get
            {
                return GetValue(CONFIGURATION.MaximumOwnedSets, 10);
            }
        }

        public static int MaximumOwnedSubs
        {
            get
            {
                return GetValue(CONFIGURATION.MaximumOwnedSubs, 10);
            }
        }
        public static int MinimumAccountAgeInDaysForSubverseCreation
        {
            get
            {
                return GetValue(CONFIGURATION.MinimumAccountAgeInDaysForSubverseCreation, 30);
            }
        }

        public static int MinimumCommentPointsForSubverseCreation
        {
            get
            {
                return GetValue(CONFIGURATION.MinimumCommentPointsForSubverseCreation, 10);
            }
        }
      

        public static int MinimumSubmissionPointsForSubverseCreation
        {
            get
            {
                return GetValue(CONFIGURATION.MinimumSubmissionPointsForSubverseCreation, 10);
            }
        }
        public static int MinimumCommentPointsForCaptchaMessaging
        {
            get
            {
                return GetValue(CONFIGURATION.MinimumCommentPointsForCaptchaMessaging, 100);
            }
        }
        public static int MinimumCommentPointsForCaptchaSubmission
        {
            get
            {
                return GetValue(CONFIGURATION.MinimumCommentPointsForCaptchaSubmission, 25);
            }
        }
        public static Domain.Models.Origin Origin
        {
            get
            {
                return GetValue(CONFIGURATION.Origin, Domain.Models.Origin.Unknown);
            }
        }
        public static string RecaptchaPrivateKey
        {
            get
            {
                return GetValue(CONFIGURATION.RecaptchaPrivateKey, "");
            }
        }

        public static string RecaptchaPublicKey
        {
            get
            {
                return GetValue(CONFIGURATION.RecaptchaPublicKey, "");
            }
        }
        /// <summary>
        /// Will redirect incoming requests that don't match the site domain to the value specified in siteDomain
        /// </summary>
        public static bool RedirectToSiteDomain
        {
            get
            {
                return GetValue(CONFIGURATION.RedirectToSiteDomain, true);
            }
        }
        
        public static bool RegistrationDisabled
        {
            get
            {
                return GetValue(CONFIGURATION.RegistrationDisabled, false);
            }
        }

        public static bool SetsDisabled
        {
            get
            {
                return GetValue(CONFIGURATION.SetsDisabled, true);
            }
        }

        public static bool SignalRDisabled
        {
            get
            {
                return GetValue(CONFIGURATION.SignalRDisabled, true);
            }
        }

        public static string SiteDescription
        {
            get
            {
                return GetValue(CONFIGURATION.SiteDescription, "A community platform where you can have your say. No censorship.");
            }
        }

        public static string SiteDomain
        {
            get
            {
                return GetValue(CONFIGURATION.SiteDomain, "voat.co");
            }
        }

        public static string SiteKeywords
        {
            get
            {
                return GetValue(CONFIGURATION.SiteKeywords, "voat, voat.co, vote, submit, comment");
            }
        }

        public static string SiteLogo
        {
            get
            {
                return GetValue(CONFIGURATION.SiteLogo, "/Graphics/voat-logo.png");
            }
        }

        public static string SiteName
        {
            get
            {
                return GetValue(CONFIGURATION.SiteName, "Voat");
            }
        }

        public static string SiteSlogan
        {
            get
            {
                return GetValue(CONFIGURATION.SiteSlogan, "Voat - have your say");
            }
        }

        public static bool UseContentDeliveryNetwork
        {
            get
            {
                return GetValue(CONFIGURATION.UseContentDeliveryNetwork, false);
            }
        }

        private static T GetValue<T>(string key, T defaultIfMissing)
        {
            if (configValues.ContainsKey(key))
            {
                return (T)configValues[key];
            }
            return defaultIfMissing;
        }
        #endregion AppSettings Accessors

    }
}
