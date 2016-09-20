using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Voat
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class VoatValidateAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            const string KEY_NAME = "__RequestVerificationToken";

            var httpContext = filterContext.HttpContext;
            var cookie = httpContext.Request.Cookies[AntiForgeryConfig.CookieName];

            string token = httpContext.Request.Form[KEY_NAME];

            if (String.IsNullOrEmpty(token))
            {
                //look in headers collection 
                token = httpContext.Request.Headers[KEY_NAME];
            }

            AntiForgery.Validate(cookie != null ? cookie.Value : null, token);
        }
    }
}