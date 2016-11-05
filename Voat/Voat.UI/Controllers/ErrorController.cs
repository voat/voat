/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat, Inc.
All Rights Reserved.
*/

using System.Web.Mvc;
using Voat.Models.ViewModels;

namespace Voat.Controllers
{
    public class ErrorController : BaseController
    {
        public ViewResult NotFound()
        {
            ViewBag.SelectedSubverse = string.Empty;
            //Response.StatusCode = 404;
            return View("~/Views/Error/404.cshtml");
        }

        public ActionResult CriticalError()
        {
            return View("~/Views/Error/Error.cshtml");
        }

        public ActionResult HeavyLoad()
        {
            return View("~/Views/Error/DbNotResponding.cshtml");
        }

        public ActionResult UnAuthorized()
        {
            return View("~/Views/Error/UnAuthorized.cshtml");
        }
        public ActionResult Generic(ErrorViewModel model = null)
        {
            if (model == null)
            {
                model = new ErrorViewModel();
            }
            return View("~/Views/Error/Generic.cshtml", model);
        }

        public ActionResult Unhandled()
        {
            throw new System.InvalidProgramException("This is an unhandled exception");
        }

        public ActionResult Others(string name, string url)
        {
            return View();
        }
    }
}