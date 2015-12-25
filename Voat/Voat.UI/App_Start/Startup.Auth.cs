using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Web.Configuration;

namespace Voat
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            var settings = new CookieAuthenticationOptions
            {
                SlidingExpiration = true,
                ExpireTimeSpan = TimeSpan.FromDays(30.0),
                CookieName = "WhoaverseLogin", //We keeping it old school with the cookies.
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
            };

            //for local testing don't set cookiedomain
            var domain = WebConfigurationManager.AppSettings["CookieDomain"];
            if (!String.IsNullOrEmpty(domain))
            {
                settings.CookieDomain = domain;
            }

            app.UseCookieAuthentication(settings);

        }
    }
}