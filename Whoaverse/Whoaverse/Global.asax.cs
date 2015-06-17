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
    using Autofac;
    using Autofac.Integration.Mvc;

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

            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof (MvcApplication).Assembly);
            builder.RegisterFilterProvider();
            DependencyInjection.RegisterComponents(builder);
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

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
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["forceHTTPS"])) {
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
    }

    
}
