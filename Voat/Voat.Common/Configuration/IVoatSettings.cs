namespace Voat.Common.Configuration
{
    public interface IVoatSettings
    {
        bool AdsEnabled { get; }
        bool ApiKeyCreationEnabled { get; }
        bool CaptchaEnabled { get; }
        bool ChatEnabled { get; }
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
        Origin Origin { get; }
        string RecaptchaPrivateKey { get; }
        string RecaptchaPublicKey { get; }
        bool RedirectToSiteDomain { get; }
        bool RegistrationEnabled { get; }
        RuntimeStateSetting RuntimeState { get; }
        bool SearchEnabled { get; }
        bool SetCreationEnabled { get; }
        bool SetsEnabled { get; }
        bool SignalrEnabled { get; }
        string SiteDescription { get; }
        string SiteDomain { get; }
        string SiteKeywords { get; }
        string SiteLogo { get; }
        string SiteName { get; }
        string SiteSlogan { get; }
        int SubverseUpdateTimeLockInHours { get; }
        bool UseContentDeliveryNetwork { get; }
    }
}