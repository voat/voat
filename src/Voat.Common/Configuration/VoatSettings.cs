using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Voat.Common;
using Voat.Common.Configuration;
using Voat.Common.Models;
using Voat.Configuration;

namespace Voat.Configuration
{
    public class VoatSettings : UpdatableConfigurationSettings<VoatSettings>, IVoatSettings
    {
        public VoatSettings()
        {
            base.OnUpdate += (object sender, VoatSettings newSettings) => {
                //The IsDevelopment flag is set in startup, here we copy it to then settings files when settings get updated.
                newSettings.IsDevelopment = IsDevelopment;
            };
        }

        private T GetValue<T>(string key, T defaultIfMissing)
        {
            //if (configValues.ContainsKey(key))
            //{
            //    var value = (T)configValues[key];
            //    //I'm NOT liking where this is going. And I forgot exactly what situation I originally wrote this for. As Fuzzy and Dan say, "It's a future person problem."
            //    if (typeof(T) == typeof(bool) || typeof(T) == typeof(int) || !value.IsDefault())
            //    {
            //        return value;
            //    }
            //}
            return defaultIfMissing;
        }
        #region AppSettings Accessors

        public bool IsVoatBranded
        {
            get
            {
                return SiteName.Equals("Voat", System.StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool AdsEnabled { get; set; } = false;

        public bool ApiKeyCreationEnabled { get; set; } = false;

        public bool CaptchaEnabled { get; set; } = true;

        public bool ChatEnabled { get; set; } = false;

        public string CookieDomain { get; set; } = "voat.co";
        public string CookieName { get; set; } = null;

        public int DailyCommentPostingQuota { get; set; } = 20;

        public int DailyCommentPostingQuotaForNegativeScore { get; set; } = 10;

        public int DailyCrossPostingQuota { get; set; } = 2;

        public int DailyGlobalPostingQuota { get; set; } = 5;

        public int DailyPostingQuotaForNegativeScore { get; set; } = 3;

        public int DailyPostingQuotaPerSub { get; set; } = 10;

        public int DailyVotingQuota { get; set; } = 100;

        public int DailyVotingQuotaScaledMinimum { get; set; } = 10;
        //dailyVotingQuotaScaledMinimum

        public string DestinationPathAvatars { get; set; } = "~/Storage/Avatars";

        public string DestinationPathThumbs { get; set; } = "~/Storage/Thumbs";

        public string EmailAddress { get; set; } = "noreply@voat.co";

        public string EmailServiceKey { get; set; } = "";

        public bool ForceHTTPS { get; set; } = true;
        public string FooterText { get; set; } = "";

        public int HourlyCommentPostingQuota { get; set; } = 10;

        public int HourlyGlobalPostingQuota { get; set; } = 3;

        public int HourlyPostingQuotaPerSub { get; set; } = 3;

        public bool LegacyApiEnabled { get; set; } = false;

        public int MaxAllowedAccountsFromSingleIP { get; set; } = 100;

        public int MaximumOwnedSets { get; set; } = 10;

        public int MaximumOwnedSubs { get; set; } = 10;

        public int MaximumSetSubverseCount { get; set; } = 50;

        public int MinimumAccountAgeInDaysForSubverseCreation { get; set; } = 30;
        public int MinimumCommentPointsForSendingMessages { get; set; } = 10;

        public int MinimumCommentPointsForSubverseCreation { get; set; } = 10;

        public int MinimumSubmissionPointsForSubverseCreation { get; set; } = 10;
        public int MinimumCommentPointsForSubmissionCreation { get; set; } = 0;
        
        public int MinimumCommentPointsForCaptchaMessaging { get; set; } = 100;
        public int MinimumCommentPointsForCaptchaSubmission { get; set; } = 25;
        public int MinimumCommentPointsForSendingChatMessages { get; set; } = 100;

        public int MinimumCommentPointsForDownvoting { get; set; } = 100;
        public int MinimumCommentPointsForUpvoting { get; set; } = 20;

        public string Origin { get; set; } = "Unknown";
        public string RecaptchaPrivateKey { get; set; } = "";

        public string RecaptchaPublicKey { get; set; } = "";
        /// <summary>
        /// Will redirect incoming requests that don't match the site domain to the value specified in siteDomain
        /// </summary>
        public bool RedirectToSiteDomain { get; set; } = true;

        public bool RegistrationEnabled { get; set; } = true;
        public bool SearchEnabled { get; set; } = true;
        public bool DomainSearchEnabled { get; set; } = true;
        public bool SetsEnabled { get; set; } = true;
        public bool SetCreationEnabled { get; set; } = true;
        public bool SignalrEnabled { get; set; } = false;

        public string SiteDescription { get; set; } = "A community platform where you can have your say. No censorship.";

        public string SiteDomain { get; set; } = "";

        public string ContentDeliveryDomain { get; set; } = "cdn.voat.co";

        public string SiteKeywords { get; set; } = "voat, voat.co, vote, submit, comment";

        public string SiteLogo { get; set; } = "/images/voat-logo.png";

        public string SiteName { get; set; } = "Voat";

        public string SiteSlogan { get; set; } = "Voat - have your say";

        public string SiteUserName { get; set; } = "Voat";
        public string SiteThemeDefault { get; set; } = "light"; //valid values: light or dark
        public int SubverseUpdateTimeLockInHours { get; set; } = 48;

        public RuntimeStateSetting RuntimeState { get; set; } = RuntimeStateSetting.Enabled;

        public bool ThumbnailsEnabled { get; set; } = true;

        public bool AllowUnicodeInTitles { get; set; } = false;
        public int MinimumTitleLength { get; set; } = 10;
        public int MaximumTitleLength { get; set; } = 200;
        public ApiInfo ApiInfo { get; set; } = new ApiInfo();
        
        public string[] ReservedSubverseNames { get; set; } = new string[] { };
        public string[] ReservedUserNames { get; set; } = new string[] { };

        public Size ThumbnailSize { get; set; } = new Size(100, 100);
        public Size AvatarSize { get; set; } = new Size(100, 100);

        public bool IsDevelopment { get; set; } = false;

        public bool EnableRoles { get; set; } = false;
        public string[] EnabledRoles { get; set; } = new string[] { };

        public bool EnableVotes { get; set; } = false;

        public IList<FileUploadLimit> FileUploadLimits { get; set; } = new List<FileUploadLimit>();

        public IDictionary<string, LogLevel> LogLevels { get; set; } = new Dictionary<string, LogLevel>();

        public IDictionary<string, string> AreaMaps { get; set; } = new Dictionary<string, string>();

        public OutgoingTraffic OutgoingTraffic { get; set; } = new OutgoingTraffic();

        public PasswordOptions PasswordOptions { get; set; } = new PasswordOptions() { RequiredLength = 6 };
        public string SigningKey { get; set; } = "";


        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        #endregion AppSettings Accessors

    }
}
