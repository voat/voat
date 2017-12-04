using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Voat.Common.Models;

namespace Voat.Common.Configuration
{
    public interface IVoatSettings
    {
        bool AdsEnabled { get; }
        bool ApiKeyCreationEnabled { get; }
        bool CaptchaEnabled { get; }
        bool ChatEnabled { get; }

        string CookieDomain { get; }
        string CookieName { get; }
        string SiteThemeDefault { get; set; }
        int DailyCommentPostingQuota { get; }
        int DailyCommentPostingQuotaForNegativeScore { get; }
        int DailyCrossPostingQuota { get; }
        int DailyGlobalPostingQuota { get; }
        int DailyPostingQuotaForNegativeScore { get; }
        int DailyPostingQuotaPerSub { get; }
        int DailyVotingQuota { get; }
        string DestinationPathAvatars { get; }
        string DestinationPathThumbs { get; }
        string EmailAddress { get; }
        string EmailServiceKey { get; }
        string FooterText { get; }
        bool ForceHTTPS { get; }
        int HourlyCommentPostingQuota { get; }
        int HourlyGlobalPostingQuota { get; }
        int HourlyPostingQuotaPerSub { get; }
        bool IsVoatBranded { get; }
        bool LegacyApiEnabled { get; }
        int MaxAllowedAccountsFromSingleIP { get; }
        int MaximumOwnedSets { get; }
        int MaximumOwnedSubs { get; }
        int MaximumSetSubverseCount { get; }
        int MinimumAccountAgeInDaysForSubverseCreation { get; }
        int MinimumCommentPointsForCaptchaMessaging { get; }
        int MinimumCommentPointsForCaptchaSubmission { get; }
        int MinimumCommentPointsForSendingChatMessages { get; }
        int MinimumCommentPointsForSendingMessages { get; }
        int MinimumCommentPointsForSubverseCreation { get; }
        int MinimumSubmissionPointsForSubverseCreation { get; }
        int MinimumCommentPointsForSubmissionCreation { get; }
        

        string Origin { get; }
        string RecaptchaPrivateKey { get; }
        string RecaptchaPublicKey { get; }
        bool RedirectToSiteDomain { get; }
        bool RegistrationEnabled { get; }
        RuntimeStateSetting RuntimeState { get; }
        bool SearchEnabled { get; }
        bool DomainSearchEnabled { get; }
        bool SetCreationEnabled { get; }
        bool SetsEnabled { get; }
        bool SignalrEnabled { get; }
        string SiteDescription { get; }
        string SiteDomain { get; }
        string SiteKeywords { get; }
        string SiteLogo { get; }
        string SiteName { get; }
        string SiteSlogan { get; }
        string SiteUserName { get; }
        int SubverseUpdateTimeLockInHours { get; }
        //string ManagementAreaName { get; set; }
        ApiInfo ApiInfo { get; set; } 
        IList<FileUploadLimit> FileUploadLimits { get; set; }

        IDictionary<string, LogLevel> LogLevels { get; set; }
        IDictionary<string, string> AreaMaps { get; set; }

        OutgoingTraffic OutgoingTraffic { get; set; }

        PasswordOptions PasswordOptions { get; set; }

        string SigningKey { get; set; }

        /// <summary>
        /// Stores misc settings and config values such as api values that aren't global 
        /// </summary>
        IDictionary<string, string> Properties { get; set; }
    }

}