using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Voat.Utils;

namespace Voat
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            LiveConfigurationManager.Reload(ConfigurationManager.AppSettings);
            LiveConfigurationManager.Start();

            if (!MvcApplication.SignalRDisabled)
            {
                Microsoft.AspNet.SignalR.GlobalHost.DependencyResolver.Register(typeof(Microsoft.AspNet.SignalR.Hubs.IJavaScriptMinifier), () => new Utils.HubMinifier());
            }
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            ModelMetadataProviders.Current = new CachedDataAnnotationsModelMetadataProvider();

            // USE ONLY FOR DEBUG: clear all sessions used for online users count
            // SessionTracker.RemoveAllSessions();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            if (ex is HttpException && ((HttpException)ex).GetHttpCode() == 404)
            {
                Response.RedirectToRoute(
                    new
                    {
                        controller = "Error",
                        action = "NotFound"
                    });
            }
        }

        // force SSL for every request if enabled in Web.config
        protected void Application_BeginRequest(Object sender, EventArgs e) {

            //Need to be able to kill connections for certain db tasks... This intercepts calls and redirects
            if (MvcApplication.SiteDisabled && !HttpContext.Current.Request.IsLocal)
            {
                Server.Transfer("~/inactive.min.htm");
                return;
            }

            if (MvcApplication.ForceHTTPS) {
                if (HttpContext.Current.Request.IsSecureConnection.Equals(false) && HttpContext.Current.Request.IsLocal.Equals(false)) {
                    Response.Redirect("https://" + Request.ServerVariables["HTTP_HOST"] + HttpContext.Current.Request.RawUrl);
                }
            }
            //change formatting culture for .NET
            try {
                System.Threading.Thread.CurrentThread.CurrentCulture =  new CultureInfo(Request.UserLanguages[0]);
            } catch { }
        }

        // fire each time a new session is created     
        protected void Session_Start(object sender, EventArgs e)
        {
            //if (User.Identity.IsAuthenticated)
            //{
            //    // read style preference
            //    Session["UserTheme"] = Utils.User.UserStylePreference(User.Identity.Name);
            //}
            //else
            //{
            //    // set default theme to light
            //    Session["UserTheme"] = "light";
            //}            
        }

        // fire when a session is abandoned or expires
        protected void Session_End(object sender, EventArgs e)
        {
            // experimental
            try
            {
                // session removal is executed in a background, standalone task
                // SessionTracker.Remove(Session.SessionID);
            }
            catch (Exception)
            {
                //
            }
        }

        #region AppSettings Accessors 

        internal static Dictionary<string, object> configValues = new Dictionary<string, object>();


        public static int DailyCommentPostingQuotaForNegativeScore {
            get {
                return (int)configValues[CONFIGURATION.DailyCommentPostingQuotaForNegativeScore];
            }
        }
        
        public static int DailyCrossPostingQuota 
            {
            get {
                return (int)configValues[CONFIGURATION.DailyCrossPostingQuota];
            }
        }
        public static int DailyPostingQuotaForNegativeScore {
            get {
                return (int)configValues[CONFIGURATION.DailyPostingQuotaForNegativeScore];
            }
        }
        public static int DailyPostingQuotaPerSub
        {
            get {
                return (int)configValues[CONFIGURATION.DailyPostingQuotaPerSub];
            }
        }
        public static int DailyVotingQuota {
            get {
                return (int)configValues[CONFIGURATION.DailyVotingQuota];
            }
        }
        public static bool ForceHTTPS {
            get {
                return (bool)configValues[CONFIGURATION.ForceHTTPS];
            }
        }
        public static int HourlyPostingQuotaPerSub {
            get {
                return (int)configValues[CONFIGURATION.HourlyPostingQuotaPerSub];
            }
        }
        public static int MaximumOwnedSets {
            get {
                return (int)configValues[CONFIGURATION.MaximumOwnedSets];
            }
        }
        public static int MaximumOwnedSubs {
            get {
                return (int)configValues[CONFIGURATION.MaximumOwnedSubs];
            }
        }
        public static int MinimumCcp {
            get {
                return (int)configValues[CONFIGURATION.MinimumCcp];
            }
        }
        public static int MaxAllowedAccountsFromSingleIP {
            get {
                return (int)configValues[CONFIGURATION.MaxAllowedAccountsFromSingleIP];
            }
        }
        public static string RecaptchaPrivateKey {
            get {
                return (string)configValues[CONFIGURATION.RecaptchaPrivateKey];
            }
        }
        public static string RecaptchaPublicKey {
            get {
                return (string)configValues[CONFIGURATION.RecaptchaPublicKey];
            }
        }
        public static string SiteDescription {
            get {
                return (string)configValues[CONFIGURATION.SiteDescription];
            }
        }
        public static string SiteKeywords {
            get {
                return (string)configValues[CONFIGURATION.SiteKeywords];
            }
        }
        public static string SiteLogo {
            get {
                return (string)configValues[CONFIGURATION.SiteLogo];
            }
        }
        public static string SiteName {
            get {
                return (string)configValues[CONFIGURATION.SiteName];
            }
        }
        public static string SiteSlogan {
            get {
                return (string)configValues[CONFIGURATION.SiteSlogan];
            }
        }
        public static bool SignalRDisabled {
            get {
                return (bool)configValues[CONFIGURATION.SignalRDisabled];
            }
        }
        public static bool SiteDisabled {
            get {
                return (bool)configValues[CONFIGURATION.SiteDisabled];
            }
        }
        #endregion 

    }
}
