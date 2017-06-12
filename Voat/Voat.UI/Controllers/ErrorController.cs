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
        public ActionResult Index()
        {
            return Type("index");
        }
        public ActionResult Type(string type)
        {
            
            var viewName = "Index";
            var errorModel = GetErrorViewModel(type);
            return View(viewName, errorModel);
        }
        public static ViewResult ErrorView(string type, ErrorViewModel model = null)
        {
            var viewData = new ViewDataDictionary<ErrorViewModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            viewData.Model = model == null ? GetErrorViewModel(type) : model;
            return new ViewResult()
            {
                ViewName = "~/Views/Error/Index.cshtml",
                ViewData = viewData
            };
        }
        public static ErrorViewModel GetErrorViewModel(string type)
        {
            var errorModel = new ErrorViewModel();
            type = type.TrimSafe();
            if (!string.IsNullOrEmpty(type))
            {
                switch (type.ToLower())
                {
                    case "disabled":
                        errorModel.Title = "Subverse Disabled";
                        errorModel.Description = @"<h1>The subverse you were looking for has been disabled and is no longer accessible</h1>
                                                    <p>If you are a moderator of this subverse you may contact Voat for information regarding why this subverse is no longer active</p>";
                        //errorModel.FooterMessage = "";
                        break;
                    case "exists":
                        errorModel.Title = "Mesosad!";
                        errorModel.Description = "<h1>The subverse you were trying to create already exists. Sorry about that. Try another name?</h1>";
                        errorModel.Footer = "Care to go back and try another name?";
                        break;
                    case "unathorized":
                        errorModel.Title = "Hold on there fella!";
                        errorModel.Description = "<h1>You were not supposed to be poking around here.</h1>";
                        errorModel.Footer = "How about you stop poking around? :)";
                        break;
                    case "others":
                        //errorModel.ShowGoat = false;
                        errorModel.Title = "The Others";
                        errorModel.Description = "<span style=\"font-size:10em;font-family:Verdana;\">O_o</span><p>This is not the place you think it is</p>";
                        errorModel.Footer = "Hmmm...";
                        break;
                    case "subversenotfound":
                        //errorModel.ShowGoat = false;
                        errorModel.Title = "Whoops!";
                        errorModel.Description = "<h1>The subverse you were looking for could not be found. Are you sure you typed it right? Also, I may have umm... eaten it.</h1>";
                        errorModel.Footer = "Pushing F5 repeatedly will not help";
                        break;
                    case "notfound":
                        //errorModel.ShowGoat = false;
                        errorModel.Title = "Whoops!";
                        errorModel.Description = "<h1>The thing you were looking for could not be found. Are you sure you typed it right? Also, I may have eaten it.</h1>";
                        errorModel.Footer = "Pushing F5 repeatedly will not help";
                        break;
                    case "throw":
                        throw new InvalidProgramException();
                        break;
                }
            }
            return errorModel;
        }
    }
}
