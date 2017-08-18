#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Voat.Common;

namespace Voat
{
    public static class UIExtensions
    {
        public static string GetFirstErrorMessage(this ModelStateDictionary modelState)
        {
            var message = ErrorMessages(modelState).FirstOrDefault();
            return message;
        }
        private static IEnumerable<string> ErrorMessages(ModelStateDictionary modelState)
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

        public static bool IsCookiePresent(this HttpRequest request, string keyName, string setValue, HttpResponse response = null, TimeSpan? expiration = null)
        {
            var result = false;

            var cookie = request.Cookies[keyName];

            if (cookie != null && setValue.IsEqual(cookie))
            {
                result = true;
            }



            if (response != null)
            {
                //Set if present in url
                var qsKeyValue = request.Query[keyName].FirstOrDefault();
                if (!String.IsNullOrEmpty(qsKeyValue))
                {
                    if (qsKeyValue.IsEqual(setValue))
                    {
                        //var newCookie = new HttpCookie(keyName);
                        //if (expiration.HasValue)
                        //{
                        //    newCookie.Expires = DateTime.UtcNow.Add(expiration.Value); 
                        //}
                        //newCookie.Value = setValue;
                        //response.SetCookie(newCookie);
                        response.Cookies.Append(keyName, setValue, new CookieOptions() { Expires = DateTime.UtcNow.Add(expiration.Value) });
                        result = true;
                    }
                    else
                    {
                        response.Cookies.Append(keyName, setValue, new CookieOptions() { Expires = DateTime.UtcNow.AddDays(-7) });
                        result = false;
                    }
                }
            }

            return result;
        }

        private static Dictionary<string, string> replacements = new Dictionary<string, string>() { { "%40", "@" } };

        public static string RouteUrlPretty(this IUrlHelper urlHelper, string routeName, RouteValueDictionary routeValues)
        {
            
            var routedUrl = urlHelper.RouteUrl(routeName, routeValues);

            return replacements.Aggregate(routedUrl, (value, keyPair) => value.Replace(keyPair.Key, keyPair.Value));

        }
        public static string ActionPretty(this IUrlHelper urlHelper, string routeName, RouteValueDictionary routeValues)
        {
            var routedUrl = urlHelper.Action(routeName, routeValues);

            return replacements.Aggregate(routedUrl, (value, keyPair) => value.Replace(keyPair.Key, keyPair.Value));

        }
        public static IHtmlContent PartialFor<TModel, TProperty>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression, string partialViewName)
        {
            string name = ExpressionHelper.GetExpressionText(expression);
            object model = ExpressionMetadataProvider.FromLambdaExpression(expression, helper.ViewData, helper.MetadataProvider).Model;
            var viewData = new ViewDataDictionary(helper.ViewData);

            var previousName = helper.ViewData.TemplateInfo.HtmlFieldPrefix;
            var fullName = String.IsNullOrEmpty(previousName) ? name : $"{previousName}.{name}";
            viewData.TemplateInfo.HtmlFieldPrefix = fullName;

            return helper.Partial(partialViewName, model, viewData);
        }

    }
}
