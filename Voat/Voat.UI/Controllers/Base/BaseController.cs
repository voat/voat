using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Voat.Utilities.Components;

namespace Voat.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnException(ExceptionContext filterContext)
        {
            //EventLogger.Log(filterContext.Exception);
            base.OnException(filterContext);
        }
    }
}