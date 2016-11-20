using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Web;
using System.Xml;
using Voat.Caching;
using Voat.Data.Models;
using Voat.Utilities.Components;

namespace Voat.Configuration
{
    public static class CONFIGURATION
    {
        public const string AdsEnabled = "adsEnabled";
        public const string ApiKeyCreationEnabled = "apiKeyCreationEnabled";
        public const string CacheDisabled = "cacheDisabled";
        public const string CaptchaDisabled = "captchaDisabled";
        public const string DailyCommentPostingQuota = "dailyCommentPostingQuota";
        public const string DailyCommentPostingQuotaForNegativeScore = "dailyCommentPostingQuotaForNegativeScore";
        public const string DailyCrossPostingQuota = "dailyCrossPostingQuota";
        public const string DailyGlobalPostingQuota = "dailyGlobalPostingQuota";
        public const string DailyPostingQuotaForNegativeScore = "dailyPostingQuotaForNegativeScore";
        public const string DailyPostingQuotaPerSub = "dailyPostingQuotaPerSub";
        public const string DailyVotingQuota = "dailyVotingQuota";
        public const string DestinationPathAvatars = "destinationPathAvatars";
        public const string DestinationPathThumbs = "destinationPathThumbs";
        public const string EmailServiceKey = "emailServiceKey";
        public const string ForceHTTPS = "forceHTTPS";
        public const string FooterText = "footerText";
        public const string HourlyCommentPostingQuota = "hourlyCommentPostingQuota";
        public const string HourlyGlobalPostingQuota = "hourlyGlobalPostingQuota";
        public const string HourlyPostingQuotaPerSub = "hourlyPostingQuotaPerSub";
        public const string LegacyApiEnabled = "legacyApiEnabled";
        public const string MaxAllowedAccountsFromSingleIP = "maxAllowedAccountsFromSingleIP";
        public const string MaximumOwnedSets = "maximumOwnedSets";
        public const string MaximumOwnedSubs = "maximumOwnedSubs";
        
        public const string MinimumAccountAgeInDaysForSubverseCreation = "minimumAccountAgeInDaysForSubverseCreation";
        public const string MinimumCommentPointsForSubverseCreation = "minimumCommentPointsForSubverseCreation";
        public const string MinimumSubmissionPointsForSubverseCreation = "minimumSubmissionPointsForSubverseCreation";
        public const string MinimumCommentPointsForCaptchaSubmission = "minimumCommentPointsForCaptchaSubmission";
        public const string MinimumCommentPointsForCaptchaMessaging = "minimumCommentPointsForCaptchaMessaging";

        public const string RuntimeState = "runtimeState";
        public const string RecaptchaPrivateKey = "recaptchaPrivateKey";
        public const string RedirectToSiteDomain = "redirectToSiteDomain";
        public const string RecaptchaPublicKey = "recaptchaPublicKey";
        public const string RegistrationDisabled = "registrationDisabled";
        public const string SetsDisabled = "setsDisabled";
        public const string SignalRDisabled = "signalrDisabled";
        public const string SiteDescription = "siteDescription";
        public const string SiteDisabled = "siteDisabled";
        public const string SiteDomain = "siteDomain";
        public const string SiteKeywords = "siteKeywords";
        public const string SiteLogo = "siteLogo";
        public const string SiteName = "siteName";
        public const string SiteSlogan = "siteSlogan";
        public const string Origin = "origin";

        public const string UseContentDeliveryNetwork = "useContentDeliveryNetwork";
    }

    public class LiveConfigurationManager
    {
        private static FileSystemWatcher _thewatchmen;

        private static FileSystemWatcher Watcher
        {
            get
            {
                if (_thewatchmen == null)
                {
                    lock (typeof(LiveConfigurationManager))
                    {
                        if (_thewatchmen == null)
                        {
                            if (HttpContext.Current != null)
                            {
                                _thewatchmen = new FileSystemWatcher(HttpContext.Current.Server.MapPath("~/"), "Web.config.live");
                            }
                            else
                            {
                                _thewatchmen = new FileSystemWatcher(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Web.config.live");
                            }

                            _thewatchmen.NotifyFilter = NotifyFilters.LastWrite;

                            _thewatchmen.Changed += (object sender, FileSystemEventArgs e) =>
                            {
                                Reload(e.FullPath);
                            };
                        }
                    }
                }
                return _thewatchmen;
            }
        }

        public static void Reload(NameValueCollection section)
        {
            if (section != null && section.Count > 0)
            {
                SetValueIfPresent<string>(CONFIGURATION.RecaptchaPublicKey, section[CONFIGURATION.RecaptchaPublicKey]);
                SetValueIfPresent<string>(CONFIGURATION.RecaptchaPrivateKey, section[CONFIGURATION.RecaptchaPrivateKey]);
                SetValueIfPresent<string>(CONFIGURATION.EmailServiceKey, section[CONFIGURATION.EmailServiceKey]);
                SetValueIfPresent<string>(CONFIGURATION.SiteName, section[CONFIGURATION.SiteName]);
                SetValueIfPresent<string>(CONFIGURATION.SiteSlogan, section[CONFIGURATION.SiteSlogan]);
                SetValueIfPresent<string>(CONFIGURATION.SiteDescription, section[CONFIGURATION.SiteDescription]);
                SetValueIfPresent<string>(CONFIGURATION.SiteKeywords, section[CONFIGURATION.SiteKeywords]);
                SetValueIfPresent<string>(CONFIGURATION.SiteLogo, section[CONFIGURATION.SiteLogo]);
                SetValueIfPresent<string>(CONFIGURATION.DestinationPathAvatars, section[CONFIGURATION.DestinationPathAvatars]);
                SetValueIfPresent<string>(CONFIGURATION.DestinationPathThumbs, section[CONFIGURATION.DestinationPathThumbs]);
                SetValueIfPresent<string>(CONFIGURATION.FooterText, section[CONFIGURATION.FooterText]);
                
                SetValueIfPresent<int>(CONFIGURATION.MaximumOwnedSubs, section[CONFIGURATION.MaximumOwnedSubs]);
                SetValueIfPresent<int>(CONFIGURATION.MaximumOwnedSets, section[CONFIGURATION.MaximumOwnedSets]);
                SetValueIfPresent<int>(CONFIGURATION.DailyPostingQuotaPerSub, section[CONFIGURATION.DailyPostingQuotaPerSub]);
                SetValueIfPresent<int>(CONFIGURATION.HourlyPostingQuotaPerSub, section[CONFIGURATION.HourlyPostingQuotaPerSub]);
                SetValueIfPresent<int>(CONFIGURATION.HourlyGlobalPostingQuota, section[CONFIGURATION.HourlyGlobalPostingQuota]);
                SetValueIfPresent<int>(CONFIGURATION.DailyVotingQuota, section[CONFIGURATION.DailyVotingQuota]);
                SetValueIfPresent<int>(CONFIGURATION.DailyCrossPostingQuota, section[CONFIGURATION.DailyCrossPostingQuota]);
                SetValueIfPresent<int>(CONFIGURATION.DailyPostingQuotaForNegativeScore, section[CONFIGURATION.DailyPostingQuotaForNegativeScore]);
                SetValueIfPresent<int>(CONFIGURATION.DailyGlobalPostingQuota, section[CONFIGURATION.DailyGlobalPostingQuota]);
                SetValueIfPresent<int>(CONFIGURATION.DailyCommentPostingQuotaForNegativeScore, section[CONFIGURATION.DailyCommentPostingQuotaForNegativeScore]);
                SetValueIfPresent<int>(CONFIGURATION.DailyCommentPostingQuota, section[CONFIGURATION.DailyCommentPostingQuota]);
                SetValueIfPresent<int>(CONFIGURATION.HourlyCommentPostingQuota, section[CONFIGURATION.HourlyCommentPostingQuota]);
                SetValueIfPresent<int>(CONFIGURATION.MaxAllowedAccountsFromSingleIP, section[CONFIGURATION.MaxAllowedAccountsFromSingleIP]);

                SetValueIfPresent<int>(CONFIGURATION.MinimumAccountAgeInDaysForSubverseCreation, section[CONFIGURATION.MinimumAccountAgeInDaysForSubverseCreation]);
                SetValueIfPresent<int>(CONFIGURATION.MinimumCommentPointsForSubverseCreation, section[CONFIGURATION.MinimumCommentPointsForSubverseCreation]);
                SetValueIfPresent<int>(CONFIGURATION.MinimumSubmissionPointsForSubverseCreation, section[CONFIGURATION.MinimumSubmissionPointsForSubverseCreation]);
                SetValueIfPresent<int>(CONFIGURATION.MinimumCommentPointsForCaptchaMessaging, section[CONFIGURATION.MinimumCommentPointsForCaptchaMessaging]);
                SetValueIfPresent<int>(CONFIGURATION.MinimumCommentPointsForCaptchaSubmission, section[CONFIGURATION.MinimumCommentPointsForCaptchaSubmission]);


                SetValueIfPresent<bool>(CONFIGURATION.ForceHTTPS, section[CONFIGURATION.ForceHTTPS]);
                SetValueIfPresent<bool>(CONFIGURATION.SignalRDisabled, section[CONFIGURATION.SignalRDisabled]);
                SetValueIfPresent<bool>(CONFIGURATION.SetsDisabled, section[CONFIGURATION.SetsDisabled]);
                SetValueIfPresent<bool>(CONFIGURATION.CacheDisabled, section[CONFIGURATION.CacheDisabled]);
                SetValueIfPresent<bool>(CONFIGURATION.RegistrationDisabled, section[CONFIGURATION.RegistrationDisabled]);
                SetValueIfPresent<bool>(CONFIGURATION.RedirectToSiteDomain, section[CONFIGURATION.RedirectToSiteDomain]);
                SetValueIfPresent<bool>(CONFIGURATION.UseContentDeliveryNetwork, section[CONFIGURATION.UseContentDeliveryNetwork]);

                SetValueIfPresent<bool>(CONFIGURATION.AdsEnabled, section[CONFIGURATION.AdsEnabled]);
                SetValueIfPresent<string>(CONFIGURATION.SiteDomain, section[CONFIGURATION.SiteDomain]);
                SetValueIfPresent<bool>(CONFIGURATION.LegacyApiEnabled, section[CONFIGURATION.LegacyApiEnabled]);

                SetValueIfPresent<bool>(CONFIGURATION.ApiKeyCreationEnabled, section[CONFIGURATION.ApiKeyCreationEnabled]);
                SetValueIfPresent<bool>(CONFIGURATION.CaptchaDisabled, section[CONFIGURATION.CaptchaDisabled]);

                SetValueIfPresent<Domain.Models.Origin>(CONFIGURATION.Origin, section[CONFIGURATION.Origin]);

                //HACK ATTACK
                CacheHandler.Instance.CacheEnabled = !Settings.CacheDisabled;
            }
        }

        public static void Start()
        {
            Watcher.EnableRaisingEvents = true;
        }

        public static void Stop()
        {
            Watcher.EnableRaisingEvents = false;
        }
        private static void Reload(string fullFilePath)
        {
            if (File.Exists(fullFilePath))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(fullFilePath);
                    XmlNodeList nodes = doc.SelectNodes("/configuration/appSettings/add");

                    foreach (XmlNode node in nodes)
                    {
                        string key = node.Attributes["key"].Value;
                        //add condition for RuntimeState as it has it's own handler
                        if (String.Equals(key, CONFIGURATION.RuntimeState, StringComparison.OrdinalIgnoreCase))
                        {
                            RuntimeState.Refresh(node.Attributes["value"].Value);
                        }
                        else
                        {
                            SetValueIfPresent<bool>(node.Attributes["key"].Value, node.Attributes["value"].Value, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLogger.Log(ex);
                }
            }
        }
        private static void SetValueIfPresent<T>(string key, string value, bool updateOnly = false)
        {
            if (!String.IsNullOrEmpty(key))
            {
                try
                {
                    object saveValue = null;
                    if (typeof(T) == typeof(string))
                    {
                        saveValue = value;
                    }
                    else if (typeof(T) == typeof(bool))
                    {
                        //seperate logic for bool because we want accuracy for true settings
                        bool conValue = false;
                        if (!bool.TryParse(value, out conValue))
                        {
                            conValue = false;
                        }
                        saveValue = conValue;
                    }
                    else if (typeof(T).IsEnum)
                    {
                        T conValue = (T)Enum.Parse(typeof(T), value);
                        saveValue = conValue;
                    }
                    else
                    {
                        T conValue = (T)Convert.ChangeType(value, typeof(T));
                        saveValue = conValue;
                    }
                    if (!updateOnly || (updateOnly && Settings.configValues.ContainsKey(key)))
                    {
                        Settings.configValues[key] = saveValue;
                    }
                }
                catch (Exception ex)
                {
                    EventLogger.Log(ex);
                }
            }
        }
    }
}
