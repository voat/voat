using System.Collections.Generic;

namespace Voat.Configuration
{
    public class Settings
    {
        #region AppSettings Accessors 

        internal static Dictionary<string, object> configValues = new Dictionary<string, object>();


        public static int DailyCommentPostingQuotaForNegativeScore
        {
            get
            {
                return (int)configValues[CONFIGURATION.DailyCommentPostingQuotaForNegativeScore];
            }
        }

        public static int DailyCrossPostingQuota
        {
            get
            {
                return (int)configValues[CONFIGURATION.DailyCrossPostingQuota];
            }
        }
        public static int DailyPostingQuotaForNegativeScore
        {
            get
            {
                return (int)configValues[CONFIGURATION.DailyPostingQuotaForNegativeScore];
            }
        }
        public static int DailyPostingQuotaPerSub
        {
            get
            {
                return (int)configValues[CONFIGURATION.DailyPostingQuotaPerSub];
            }
        }
        public static int DailyGlobalPostingQuota
        {
            get
            {
                return (int)configValues[CONFIGURATION.DailyGlobalPostingQuota];
            }
        }
        public static int DailyVotingQuota
        {
            get
            {
                return (int)configValues[CONFIGURATION.DailyVotingQuota];
            }
        }
        public static bool ForceHTTPS
        {
            get
            {
                return (bool)configValues[CONFIGURATION.ForceHTTPS];
            }
        }
        public static int HourlyPostingQuotaPerSub
        {
            get
            {
                return (int)configValues[CONFIGURATION.HourlyPostingQuotaPerSub];
            }
        }
        public static int HourlyGlobalPostingQuota
        {
            get
            {
                return (int)configValues[CONFIGURATION.HourlyGlobalPostingQuota];
            }
        }
        public static int MaximumOwnedSets
        {
            get
            {
                return (int)configValues[CONFIGURATION.MaximumOwnedSets];
            }
        }
        public static int MaximumOwnedSubs
        {
            get
            {
                return (int)configValues[CONFIGURATION.MaximumOwnedSubs];
            }
        }
        public static int MinimumCcp
        {
            get
            {
                return (int)configValues[CONFIGURATION.MinimumCcp];
            }
        }
        public static int MaxAllowedAccountsFromSingleIP
        {
            get
            {
                return (int)configValues[CONFIGURATION.MaxAllowedAccountsFromSingleIP];
            }
        }
        public static string RecaptchaPrivateKey
        {
            get
            {
                return (string)configValues[CONFIGURATION.RecaptchaPrivateKey];
            }
        }
        public static string RecaptchaPublicKey
        {
            get
            {
                return (string)configValues[CONFIGURATION.RecaptchaPublicKey];
            }
        }

        public static string EmailServiceKey
        {
            get
            {
                return (string)configValues[CONFIGURATION.EmailServiceKey];
            }
        }

        public static string SiteDescription
        {
            get
            {
                return (string)configValues[CONFIGURATION.SiteDescription];
            }
        }
        public static string SiteKeywords
        {
            get
            {
                return (string)configValues[CONFIGURATION.SiteKeywords];
            }
        }
        public static string SiteLogo
        {
            get
            {
                return (string)configValues[CONFIGURATION.SiteLogo];
            }
        }
        public static string SiteName
        {
            get
            {
                return (string)configValues[CONFIGURATION.SiteName];
            }
        }
        public static string SiteSlogan
        {
            get
            {
                return (string)configValues[CONFIGURATION.SiteSlogan];
            }
        }
        public static bool SignalRDisabled
        {
            get
            {
                return (bool)configValues[CONFIGURATION.SignalRDisabled];
            }
        }

        public static bool SiteDisabled
        {
            get
            {
                return (bool)configValues[CONFIGURATION.SiteDisabled];
            }
        }

        public static bool SetsDisabled
        {
            get
            {
                return (bool)configValues[CONFIGURATION.SetsDisabled];
            }
        }
        public static bool CacheDisabled
        {
            get
            {
                return (bool)configValues[CONFIGURATION.CacheDisabled];
            }
        }

        public static bool RegistrationDisabled
        {
            get
            {
                return (bool)configValues[CONFIGURATION.RegistrationDisabled];
            }
        }

        public static bool UseContentDeliveryNetwork
        {
            get
            {
                return (bool)configValues[CONFIGURATION.UseContentDeliveryNetwork];
            }
        }

        public static string DestinationPathThumbs
        {
            get
            {
                return (string)configValues[CONFIGURATION.DestinationPathThumbs];
            }
        }

        public static string DestinationPathAvatars
        {
            get
            {
                return (string)configValues[CONFIGURATION.DestinationPathAvatars];
            }
        }
        #endregion 
    }
}
