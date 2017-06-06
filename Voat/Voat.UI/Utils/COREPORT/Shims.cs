using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Data.Models;

namespace Voat
{
    //All UI based access of EF context should go through this object.
    //At a later date will will throw errors in this object to force no usage from the UI project 
    public class VoatUIDataContextAccessor : VoatDataContext
    {
        public VoatUIDataContextAccessor(string name = null) : base(name) { }
    }

    public static class Extensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            throw new NotImplementedException("Core port not implemented");
        }

        public static void Add(this IResponseCookies cookies, HttpCookie cookie)
        {
            cookies.Append(cookie.Name, cookie.Value, new CookieOptions() { Expires = cookie.Expires });
        }
        public static void SetCookie(this HttpResponse response, HttpCookie cookie)
        {
            response.Cookies.Add(cookie);
        }
        
    }
    public class HttpCookie
    {
        public HttpCookie(string name, string value = "")
        {
            this.Name = name;
            this.Value = value;
        }
        public string Name { get; set; }
        public string Value { get; set; }
        public DateTime Expires { get; set; }

    }
    public class ChildActionOnlyAttribute : Attribute
    {
        public ChildActionOnlyAttribute()
        {
            //throw new NotImplementedException("Core Port Shim. This code needs to be refactored to proper standards");
        }
    }

    public class HttpStatusCodeResult : ActionResult
    {
        public HttpStatusCodeResult(System.Net.HttpStatusCode status, string message = null)
        {
            throw new NotImplementedException("Core Port Shim. This code needs to be refactored to proper standards");
        }
    }
    public class HttpUnauthorizedResult : ActionResult
    {
        public HttpUnauthorizedResult()
        {
            throw new NotImplementedException("Core Port Shim. This code needs to be refactored to proper standards");
        }
    }
    public class OutputCacheAttribute : Attribute
    {
        public int Duration { get; set; }
        public string VaryByParam { get; set; }

    }

    //public class HttpNotFound : ActionResult
    //{
    //    public HttpNotFound()
    //    {
    //        throw new NotImplementedException("Core Port Shim. This code needs to be refactored to proper standards");
    //    }
    //}
}
