using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Domain.Command;
using Voat.UI.Areas.Admin.Models;

namespace Voat.UI.Areas.Admin.Controllers
{
    
    public class SpamController : BaseAdminController
    {

        [Authorize(Roles = "GlobalAdmin,Admin,DelegateAdmin,GlobalBans")]
        public ActionResult Ban()
        {
            return View(new BanViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "GlobalAdmin,Admin,DelegateAdmin,GlobalBans")]
        public async Task<ActionResult> Ban(BanViewModel model)
        {
            if (ModelState.IsValid)
            {
                var banList = model.Name.Split(',', ';').Select(x => 
                    new Domain.Models.GenericReference<Domain.Models.BanType>() {
                        Name = x.TrimSafe(),
                        Type = model.BanType
                    }).ToList();


                var cmd = new GlobalBanCommand(banList, model.Reason).SetUserContext(User);
                var response = await cmd.Execute();
                if (response.Success)
                {
                    ViewBag.SuccessMessage = "Bans Saved";
                }
                else
                {
                    ModelState.AddModelError("", response.Message);
                    return View("Ban", model);
                }
            }
            return View("Ban", new BanViewModel());
        }
    }
}
