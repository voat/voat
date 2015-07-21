using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace Voat.Utils
{

    public static class CONFIGURATION {
        public const string DailyCommentPostingQuotaForNegativeScore = "dailyCommentPostingQuotaForNegativeScore";
        public const string DailyCrossPostingQuota = "dailyCrossPostingQuota";
        public const string DailyPostingQuotaForNegativeScore = "dailyPostingQuotaForNegativeScore";
        public const string DailyPostingQuotaPerSub = "dailyPostingQuotaPerSub";
        public const string DailyVotingQuota = "dailyVotingQuota";
        public const string ForceHTTPS = "forceHTTPS";
        public const string HourlyPostingQuotaPerSub = "hourlyPostingQuotaPerSub";
        public const string MaximumOwnedSets = "maximumOwnedSets";
        public const string MaximumOwnedSubs = "maximumOwnedSubs";
        public const string MinimumCcp = "minimumCcp";
        public const string MaxAllowedAccountsFromSingleIP = "maxAllowedAccountsFromSingleIP";
        public const string RecaptchaPrivateKey = "recaptchaPrivateKey";
        public const string RecaptchaPublicKey = "recaptchaPublicKey";
        public const string SiteDescription = "siteDescription";
        public const string SiteKeywords = "siteKeywords";
        public const string SiteLogo = "siteLogo";
        public const string SiteName = "siteName";
        public const string SiteSlogan = "siteSlogan";
        public const string SignalRDisabled = "signalrDisabled";
        public const string SiteDisabled = "siteDisabled";
        public const string SetsDisabled = "setsDisabled";
        public const string CacheDisabled = "cacheDisabled";
        public const string RegistrationDisabled = "registrationDisabled";
    }

    public class LiveConfigurationManager
    {
        private static FileSystemWatcher _thewatchmen;

        public static void Start()
        {
            Watcher.EnableRaisingEvents = true;
        }
        public static void Stop()
        {
            Watcher.EnableRaisingEvents = false;
        }

        private static FileSystemWatcher Watcher 
        {
            get {
                if (_thewatchmen == null) 
                {
                    lock (typeof(LiveConfigurationManager))
                    {
                        if (_thewatchmen == null)
                        {
                            _thewatchmen = new FileSystemWatcher(HttpContext.Current.Server.MapPath("~/"), "Web.config.live");
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
                        SetValueIfPresent<bool>(node.Attributes["key"].Value, node.Attributes["value"].Value, true);
                    }
                }
                catch (Exception ex) { 
                    /*no-op*/
                }
            }
        }

        public static void Reload(NameValueCollection section) 
        {
            if (section != null && section.Count > 0) 
            {
                SetValueIfPresent<string>(CONFIGURATION.RecaptchaPublicKey, section[CONFIGURATION.RecaptchaPublicKey]);
                SetValueIfPresent<string>(CONFIGURATION.RecaptchaPrivateKey, section[CONFIGURATION.RecaptchaPrivateKey]);
                SetValueIfPresent<string>(CONFIGURATION.SiteName, section[CONFIGURATION.SiteName]);
                SetValueIfPresent<string>(CONFIGURATION.SiteSlogan, section[CONFIGURATION.SiteSlogan]);
                SetValueIfPresent<string>(CONFIGURATION.SiteDescription, section[CONFIGURATION.SiteDescription]);
                SetValueIfPresent<string>(CONFIGURATION.SiteKeywords, section[CONFIGURATION.SiteKeywords]);
                SetValueIfPresent<string>(CONFIGURATION.SiteLogo, section[CONFIGURATION.SiteLogo]);

                SetValueIfPresent<int>(CONFIGURATION.MinimumCcp, section[CONFIGURATION.MinimumCcp]);
                SetValueIfPresent<int>(CONFIGURATION.MaximumOwnedSubs, section[CONFIGURATION.MaximumOwnedSubs]);
                SetValueIfPresent<int>(CONFIGURATION.MaximumOwnedSets, section[CONFIGURATION.MaximumOwnedSets]);
                SetValueIfPresent<int>(CONFIGURATION.DailyPostingQuotaPerSub, section[CONFIGURATION.DailyPostingQuotaPerSub]);
                SetValueIfPresent<int>(CONFIGURATION.HourlyPostingQuotaPerSub, section[CONFIGURATION.HourlyPostingQuotaPerSub]);
                SetValueIfPresent<int>(CONFIGURATION.DailyVotingQuota, section[CONFIGURATION.DailyVotingQuota]);
                SetValueIfPresent<int>(CONFIGURATION.DailyCrossPostingQuota, section[CONFIGURATION.DailyCrossPostingQuota]);
                SetValueIfPresent<int>(CONFIGURATION.DailyPostingQuotaForNegativeScore, section[CONFIGURATION.DailyPostingQuotaForNegativeScore]);
                SetValueIfPresent<int>(CONFIGURATION.DailyCommentPostingQuotaForNegativeScore, section[CONFIGURATION.DailyCommentPostingQuotaForNegativeScore]);
                SetValueIfPresent<int>(CONFIGURATION.MaxAllowedAccountsFromSingleIP, section[CONFIGURATION.MaxAllowedAccountsFromSingleIP]);


                SetValueIfPresent<bool>(CONFIGURATION.ForceHTTPS, section[CONFIGURATION.ForceHTTPS]);
                SetValueIfPresent<bool>(CONFIGURATION.SiteDisabled, section[CONFIGURATION.SiteDisabled]);
                SetValueIfPresent<bool>(CONFIGURATION.SignalRDisabled, section[CONFIGURATION.SignalRDisabled]);
                SetValueIfPresent<bool>(CONFIGURATION.SetsDisabled, section[CONFIGURATION.SetsDisabled]);
                SetValueIfPresent<bool>(CONFIGURATION.CacheDisabled, section[CONFIGURATION.CacheDisabled]);
                SetValueIfPresent<bool>(CONFIGURATION.RegistrationDisabled, section[CONFIGURATION.RegistrationDisabled]);

                //HACK ATTACK
                CacheHandler.CacheEnabled = !MvcApplication.CacheDisabled;
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
                        if (!bool.TryParse(value, out conValue)) {
                            conValue = false;
                        }
                        saveValue = conValue;
                    }
                    else 
                    {
                        T conValue = (T)Convert.ChangeType(value, typeof(T));
                        saveValue = conValue;
                    }
                    if (!updateOnly || (updateOnly && MvcApplication.configValues.ContainsKey(key)))
                    {
                        MvcApplication.configValues[key] = saveValue;
                    }
                }
                catch { }
            }
        }

    }
}