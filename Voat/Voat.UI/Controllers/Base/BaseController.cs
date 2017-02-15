using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Voat.Caching;
using Voat.Domain;
using Voat.Domain.Command;
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

        #region JSON Responses
        //These are beginning port to api structures
        public ActionResult JsonError(string type, string message)
        {
            //Response.StatusCode = 400;
            return Json(new { success = false, error = new { type = type, message = message } });
        }
        public ActionResult JsonError(string message)
        {
            return JsonError("General", message);
        }
        public ActionResult JsonResult(CommandResponse response)
        {
            if (response.Success)
            {
                return Json(new { success = true });
            }
            else
            {
                return JsonError(response.Message);
            }
        }
        public ActionResult JsonResult<T>(CommandResponse<T> response)
        {
            if (response.Success)
            {
                return Json(new { success = true, data = response.Response });
            }
            else
            {
                return JsonError(response.Message);
            }
        }
        #endregion
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