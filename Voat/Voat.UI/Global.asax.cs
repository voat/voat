using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Voat.Configuration;
using Voat.UI.Utilities;
using Voat.Utilities;
using Voat.Utilities.Components;

namespace Voat
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            var formatters = GlobalConfiguration.Configuration.Formatters;
            formatters.Remove(formatters.XmlFormatter);

            LiveConfigurationManager.Reload(ConfigurationManager.AppSettings);
            LiveConfigurationManager.Start();

            if (!Settings.SignalRDisabled)
            {
                Microsoft.AspNet.SignalR.GlobalHost.DependencyResolver.Register(typeof(Microsoft.AspNet.SignalR.Hubs.IJavaScriptMinifier), () => new HubMinifier());
            }
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            ModelMetadataProviders.Current = new CachedDataAnnotationsModelMetadataProvider();

            ContentProcessor.UserNotificationChanged = new Action<string>(recipient => {
                //get count of unread notifications
                int unreadNotifications = UserHelper.UnreadTotalNotificationsCount(recipient);
                // send SignalR realtime notification to recipient
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();
                hubContext.Clients.User(recipient).setNotificationsPending(unreadNotifications);
            });

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
            if (Settings.SiteDisabled && !HttpContext.Current.Request.IsLocal)
            {
                Server.Transfer("~/inactive.min.htm");
                return;
            }

            if (Settings.ForceHTTPS) {
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
            //    Session["UserTheme"] = UserHelper.UserStylePreference(User.Identity.Name);
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
        

    }
}
