using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Voat.Caching;
using Voat.Domain;
using Voat.Utilities.Components;

namespace Voat.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }
        protected UserData UserData
        {
            get
            {
                return UserData.GetContextUserData();
            }
        }
        protected override void OnException(ExceptionContext filterContext)
        {
            //EventLogger.Log(filterContext.Exception);
            base.OnException(filterContext);
        }
    }
}