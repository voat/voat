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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System;
using System.IO;
using System.Net;
using Voat.Configuration;
using Voat.Domain;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Http;
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
       
        protected JsonResult JsonResult(CommandResponse response)
        {
            return Json(MapToApiShimResponse(response));
            //if (response.Success)
            //{
            //    return Json(new { success = true });
            //}
            //else
            //{
            //    return JsonError(response.Message);
            //}
        }
        protected JsonResult JsonResult<T>(CommandResponse<T> response) 
        {
            return Json(MapToApiShimResponse(response));
            //if (response.Success)
            //{
            //    return Json(new { success = true, data = response.Response });
            //}
            //else
            //{
            //    return JsonError(response.Message);
            //}
        }
        private object MapToApiShimResponse(CommandResponse response)
        {
            // {
            //    success: false, //all errors return false.
            //    error: {
            //        type: "NotFound", //the type of error that occured
            //        message: "Subverse 'IJustMakeUpStuff' does not exist." //a message describing error
            //    }
            //}

            if (response != null)
            {
                if (response.Success)
                {
                    return new { success = true };
                    //return new ApiDataResponse<T>(response.Response);
                }
                else
                {
                    if (response.Exception != null)
                    {
                        if (VoatSettings.Instance.IsDevelopment)
                        {
                            return new { success = false, error = new { type = response.Exception.GetType().FullName, message = response.Exception.ToString() } };
                        }
                        else
                        {
                            return new { success = false, error = new { type = "ServerException", message = "An exception has been encountered." } };
                        }
                    }
                    else
                    {
                        return new { success = false, error = new { type = response.Status, message = response.Message } };
                    }
                }
            }
            else
            {
                return new { success = true, error = new { type = "NotFound", message = "Not Found" } };
            }
        }
        private object MapToApiShimResponse<T>(CommandResponse<T> response)
        {
            // {
            //    success: false, //all errors return false.
            //    error: {
            //        type: "NotFound", //the type of error that occured
            //        message: "Subverse 'IJustMakeUpStuff' does not exist." //a message describing error
            //    }
            //}
            
            if (response != null)
            {
                if (response.Success)
                {
                    return new { success = true, data = response.Response };
                }
                else
                {
                    //Use defaults
                    if (response.Exception != null)
                    {
                        if (VoatSettings.Instance.IsDevelopment)
                        {
                            return new { success = false, error = new { type = response.Exception.GetType().FullName, message = response.Exception.ToString() }, data = response.Response };
                        }
                        else
                        {
                            return new { success = false, error = new { type = "ServerException", message = "An exception has been encountered." }, data = response.Response };
                        }
                    }
                    else
                    {
                        return new { success = false, error = new { type = response.Status, message = response.Message }, data = response.Response };
                    }
                }
            }
            else
            {
                return new { success = true, error = new { type = "NotFound", message = "Not Found" } };
            }
        }
        #endregion

        #region Error View Accessors

        protected ViewResult ErrorView(ErrorViewModel model = null)
        {
            return ErrorController.ErrorView(ErrorType.Default, model);
        }
        #endregion

        #region Hybrid Ajax Errors
        //These are meant to bridge the gap between the UI and ajax calls.

        protected ActionResult HybridError(ErrorViewModel errorViewModel)
        {
            if (Request.IsAjaxRequest())
            {
                return JsonResult(CommandResponse.FromStatus(Status.Error, errorViewModel.Description));
            }
            else
            {
                return ErrorView(errorViewModel);
            }
        }


        #endregion
        protected string ViewPath(DomainReference domainReference)
        {
            var isRetro = Request.IsCookiePresent("view", "retro", this.Response, TimeSpan.FromDays(7));
            return (isRetro ? VIEW_PATH.SUBMISSION_LIST_RETRO : VIEW_PATH.SUBMISSION_LIST);
        }
    }
}
