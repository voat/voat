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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using Voat.Common;
using Voat.Models.ViewModels;

namespace Voat.Controllers
{
    public class ErrorController : BaseController
    {
        public ActionResult Status(int statusCode)
        {
            var type = ErrorType.Default;
            switch (statusCode)
            {
                case 404:
                    type = ErrorType.NotFound;
                    break;
            }

            return Type(type);
        }
        public ActionResult Index()
        {
            return Status(500);
        }
        public ActionResult Type(ErrorType type)
        {
            var viewName = "Index";
            var errorModel = ErrorViewModel.GetErrorViewModel(type);
            return View(viewName, errorModel);
        }
        public static ViewResult ErrorView(ErrorType type, ErrorViewModel model = null)
        {
            var viewData = new ViewDataDictionary<ErrorViewModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            viewData.Model = model == null ? ErrorViewModel.GetErrorViewModel(type) : model;
            return new ViewResult()
            {
                ViewName = "~/Views/Error/Index.cshtml",
                ViewData = viewData
            };
        }
    }
}
