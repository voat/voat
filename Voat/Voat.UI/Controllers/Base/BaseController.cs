using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Voat.Caching;
using Voat.Domain;
using Voat.Domain.Command;
using Voat.Domain.Models;
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
        protected JsonResult JsonError(string type, string message)
        {
            //Response.StatusCode = 400;
            return Json(new { success = false, error = new { type = type, message = message } });
        }
        protected JsonResult JsonError(string message)
        {
            return JsonError("General", message);
        }
        protected JsonResult JsonResult(CommandResponse response)
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
        protected JsonResult JsonResult<T>(CommandResponse<T> response)
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
        protected ViewResult NotFoundErrorView()
        {
            return View("~/Views/Error/404.cshtml");
        }
        protected ViewResult GenericErrorView(ErrorViewModel model = null)
        {
            if (model == null)
            {
                model = new ErrorViewModel();
            }
            return View("~/Views/Error/Generic.cshtml", model);
        }
        protected ViewResult UnAuthorizedErrorView()
        {
            return View("~/Views/Error/UnAuthorized.cshtml");
        }
        protected ViewResult HeavyLoadErrorView()
        {
            return View("~/Views/Error/DbNotResponding.cshtml");
        }
        protected ViewResult SubverseDisabledErrorView()
        {
            return View("~/Views/Error/SubverseDisabled.cshtml");
        }
        protected ViewResult SubverseNotFoundErrorView()
        {
            return View("~/Views/Error/Subversenotfound.cshtml");
        }


        protected string ViewPath(DomainReference domainReference)
        {
            //var isList = !Request.IsCookiePresent("view", "list", this.Response);
            //return (isList ? VIEW_PATH.SUBMISSION_LIST_RETRO : VIEW_PATH.SUBMISSION_LIST);


            var isRetro = Request.IsCookiePresent("view", "retro", this.Response, TimeSpan.FromDays(7));
            return (isRetro ? VIEW_PATH.SUBMISSION_LIST_RETRO : VIEW_PATH.SUBMISSION_LIST);
        }


        #endregion

    }
}