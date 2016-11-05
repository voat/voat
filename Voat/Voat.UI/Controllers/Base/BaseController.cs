using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Voat.Caching;
using Voat.Domain;
using Voat.Models.ViewModels;
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

        #region Error View Accessors
        public ViewResult NotFoundErrorView()
        {
            return View("~/Views/Error/404.cshtml");
        }
        public ViewResult GenericErrorView(ErrorViewModel model = null)
        {
            if (model == null)
            {
                model = new ErrorViewModel();
            }
            return View("~/Views/Error/Generic.cshtml", model);
        }
        public ViewResult UnAuthorizedErrorView()
        {
            return View("~/Views/Error/UnAuthorized.cshtml");
        }
        public ViewResult HeavyLoadErrorView()
        {
            return View("~/Views/Error/DbNotResponding.cshtml");
        }
        public ViewResult SubverseDisabledErrorView()
        {
            return View("~/Views/Error/SubverseDisabled.cshtml");
        }
        public ViewResult SubverseNotFoundErrorView()
        {
            return View("~/Views/Error/Subversenotfound.cshtml");
        }

        #endregion
    }
}