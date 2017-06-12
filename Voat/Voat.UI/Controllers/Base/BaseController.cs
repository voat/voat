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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using Voat.Domain;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Models.ViewModels;

namespace Voat.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }
        protected UserData UserData
        {
            get
            {
                return UserData.GetContextUserData(HttpContext);
            }
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
            return ErrorController.ErrorView("notfound");
        }
        protected ViewResult GenericErrorView(ErrorViewModel model = null)
        {
            return ErrorController.ErrorView("generic", model);
        }
        protected ViewResult UnAuthorizedErrorView()
        {
            return ErrorController.ErrorView("unathorized");
        }
        protected ViewResult SubverseDisabledErrorView()
        {
            return ErrorController.ErrorView("disabled");
        }
        protected ActionResult SubverseNotFoundErrorView()
        {
            return ErrorController.ErrorView("subversenotfound");
        }

        #endregion

        protected string ViewPath(DomainReference domainReference)
        {
            var isRetro = Request.IsCookiePresent("view", "retro", this.Response, TimeSpan.FromDays(7));
            return (isRetro ? VIEW_PATH.SUBMISSION_LIST_RETRO : VIEW_PATH.SUBMISSION_LIST);
        }

    }
}
