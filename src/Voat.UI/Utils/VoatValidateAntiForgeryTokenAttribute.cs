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

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Voat
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class VoatValidateAntiForgeryTokenAttribute : ValidateAntiForgeryTokenAttribute //, IAuthorizationFilter
    {
        //CORE_PORT: Don't think we need to implement this like we did before. Framework should cover our uses
        //public void OnAuthorization(AuthorizationFilterContext filterContext)
        //{

        //    IAntiforgery x = (IAntiforgery)filterContext.HttpContext.RequestServices.GetService(typeof(IAntiforgery));
            
        //    //CORE_PORT: Not Supported
        //    throw new NotImplementedException("Core port issues");
        //    //if (filterContext == null)
        //    //{
        //    //    throw new ArgumentNullException("filterContext");
        //    //}
        //    //const string KEY_NAME = "__RequestVerificationToken";

        //    //var httpContext = filterContext.HttpContext;
        //    //var cookie = httpContext.Request.Cookies[AntiForgeryConfig.CookieName];

        //    //string token = httpContext.Request.Form[KEY_NAME];

        //    //if (String.IsNullOrEmpty(token))
        //    //{
        //    //    //look in headers collection 
        //    //    token = httpContext.Request.Headers[KEY_NAME];
        //    //}

        //    //AntiForgery.Validate(cookie != null ? cookie.Value : null, token);
        //}
    }
}
