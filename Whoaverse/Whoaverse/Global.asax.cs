using System;
using System.Configuration;
using System.Globalization;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Voat
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            Microsoft.AspNet.SignalR.GlobalHost.DependencyResolver.Register(typeof(Microsoft.AspNet.SignalR.Hubs.IJavaScriptMinifier), () => new Utils.HubMinifier());

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
            if (User.Identity.IsAuthenticated)
            {
                // read style preference
                Session["UserTheme"] = Utils.User.UserStylePreference(User.Identity.Name);
            }
            else
            {
                // set default theme to light
                Session["UserTheme"] = "light";
            }            
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

        public static readonly int DailyCommentPostingQuotaForNegativeScore = Convert.ToInt32(ConfigurationManager.AppSettings["dailyCommentPostingQuotaForNegativeScore"]);
        public static readonly int DailyCrossPostingQuota = Convert.ToInt32(ConfigurationManager.AppSettings["dailyCrossPostingQuota"]);
        public static readonly int DailyPostingQuotaForNegativeScore = Convert.ToInt32(ConfigurationManager.AppSettings["dailyPostingQuotaForNegativeScore"]);
        public static readonly int DailyPostingQuotaPerSub = Convert.ToInt32(ConfigurationManager.AppSettings["dailyPostingQuotaPerSub"]);
        public static readonly int DailyVotingQuota = Convert.ToInt32(ConfigurationManager.AppSettings["dailyVotingQuota"]);
        public static readonly bool ForceHTTPS = Convert.ToBoolean(ConfigurationManager.AppSettings["forceHTTPS"]);
        public static readonly int HourlyPostingQuotaPerSub = Convert.ToInt32(ConfigurationManager.AppSettings["hourlyPostingQuotaPerSub"]);
        public static readonly int MaximumOwnedSets = Convert.ToInt32(ConfigurationManager.AppSettings["maximumOwnedSets"]);
        public static readonly int MaximumOwnedSubs = Convert.ToInt32(ConfigurationManager.AppSettings["maximumOwnedSubs"]);
        public static readonly int MinimumCcp = Convert.ToInt32(ConfigurationManager.AppSettings["minimumCcp"]);
        public static readonly string RecaptchaPrivateKey = ConfigurationManager.AppSettings["recaptchaPrivateKey"];
        public static readonly string RecaptchaPublicKey = ConfigurationManager.AppSettings["recaptchaPublicKey"];
        public static readonly string SiteDescription = ConfigurationManager.AppSettings["siteDescription"];
        public static readonly string SiteKeywords = ConfigurationManager.AppSettings["siteKeywords"];
        public static readonly string SiteLogo = ConfigurationManager.AppSettings["siteLogo"];
        public static readonly string SiteName = ConfigurationManager.AppSettings["siteName"];
        public static readonly string SiteSlogan = ConfigurationManager.AppSettings["siteSlogan"];
    }
}
