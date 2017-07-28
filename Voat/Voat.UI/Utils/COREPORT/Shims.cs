using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Voat.Data.Models;

namespace Voat
{
    
    public class ChildActionOnlyAttribute : Attribute
    {
        public ChildActionOnlyAttribute()
        {
            //throw new NotImplementedException("Core Port Shim. This code needs to be refactored to proper standards");
        }
    }

    public class HttpStatusCodeResult : ActionResult
    {
        private HttpStatusCode _status;
        public HttpStatusCodeResult(System.Net.HttpStatusCode status, string message = null)
        {
            //throw new NotImplementedException("Core Port Shim. This code needs to be refactored to proper standards");
            _status = status;
        }
        public override void ExecuteResult(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)_status;
            //context.HttpContext.Response.m
            base.ExecuteResult(context);
        }
    }
    public class HttpUnauthorizedResult : HttpStatusCodeResult
    {
        public HttpUnauthorizedResult() : base(HttpStatusCode.Unauthorized)
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
