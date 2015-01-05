using System;
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
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            ModelMetadataProviders.Current = new CachedDataAnnotationsModelMetadataProvider();

            // clear all sessions used for online users count
            SessionTracker.RemoveAllSessions();
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
                SessionTracker.Remove(Session.SessionID);
            }
            catch (Exception)
            {
                //
            }
        }
        
    }
}
