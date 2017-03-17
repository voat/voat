using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

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

        public static bool IsCookiePresent(this HttpRequestBase request, string keyName, string setValue, HttpResponseBase response = null)
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
                        response.SetCookie(new HttpCookie(keyName) { Expires = DateTime.UtcNow.AddDays(7) });
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


    }
}