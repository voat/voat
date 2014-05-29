using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Whoaverse.Controllers
{
    public class ErrorController : Controller
    {
        public ViewResult NotFound()
        {
            ViewBag.SelectedSubverse = string.Empty;
            //Response.StatusCode = 404;
            return View("~/Views/Errors/Error_404.cshtml");
        }
        public ActionResult Index()
        {
            return View("~/Views/Errors/Error.cshtml");
        }
    }
}