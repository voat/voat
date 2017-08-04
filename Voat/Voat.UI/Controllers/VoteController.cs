using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Controllers;

namespace Voat.UI.Controllers
{
    [Authorize]
    public class VoteController : BaseController
    {
        [HttpGet]
        public ActionResult Index(int id)
        {
            object model = null;
            return View("Index", model);
        }

        public ActionResult Create()
        {
            object model = null;
            return View("Edit", model);
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            object model = null;
            return View("Edit", model);
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            object model = null;
            return View("Delete", model);
        }
    }
}
