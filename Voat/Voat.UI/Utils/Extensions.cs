using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Voat
{
    public static class UIExtensions
    {
        public static string GetFirstErrorMessage(this System.Web.Mvc.ModelStateDictionary modelState)
        {
            var message = ErrorMessages(modelState).FirstOrDefault();
            return message;
        }
        private static IEnumerable<string> ErrorMessages(System.Web.Mvc.ModelStateDictionary modelState)
        {
            foreach (var kp in modelState)
            {
                foreach (var e in kp.Value.Errors)
                {
                    if (!String.IsNullOrEmpty(e.ErrorMessage))
                    {
                        yield return e.ErrorMessage;
                    }
                }
            }
        }

        public static bool IsCookiePresent(this HttpRequestBase request, string keyName, string setValue, HttpResponseBase response = null, TimeSpan? expiration = null)
        {
            var result = false;

            var cookie = request.Cookies[keyName];

            if (cookie != null)
            {
                result = true;
            }



            if (response != null)
            {
                //Set if present in url
                var qsKeyValue = request.QueryString[keyName];
                if (!String.IsNullOrEmpty(qsKeyValue))
                {
                    if (qsKeyValue.Equals(setValue))
                    {
                        var newCookie = new HttpCookie(keyName);
                        if (expiration.HasValue)
                        {
                            newCookie.Expires = DateTime.UtcNow.Add(expiration.Value); 
                        }
                        response.SetCookie(newCookie);
                        result = true;
                    }
                    else
                    {
                        response.SetCookie(new HttpCookie(keyName) { Expires = DateTime.UtcNow.AddDays(-7) });
                        result = false;
                    }
                }
            }

            return result;
        }

        private static Dictionary<string, string> replacements = new Dictionary<string, string>() { { "%40", "@" } };

        public static string RouteUrlPretty(this UrlHelper urlHelper, string routeName, RouteValueDictionary routeValues)
        {
            
            var routedUrl = urlHelper.RouteUrl(routeName, routeValues);

            return replacements.Aggregate(routedUrl, (value, keyPair) => value.Replace(keyPair.Key, keyPair.Value));

        }
        public static string ActionPretty(this UrlHelper urlHelper, string routeName, RouteValueDictionary routeValues)
        {
            var routedUrl = urlHelper.Action(routeName, routeValues);

            return replacements.Aggregate(routedUrl, (value, keyPair) => value.Replace(keyPair.Key, keyPair.Value));

        }

    }
}