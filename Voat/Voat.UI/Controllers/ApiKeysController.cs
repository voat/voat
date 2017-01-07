using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Voat.Configuration;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Query;
using Voat.Models.ViewModels;

namespace Voat.Controllers
{
    [Authorize]
    public class ApiKeysController : BaseController
    {
        // GET: ApiKey
        public async Task<ActionResult> Index()
        {
            var cmd = new QueryUserApiKeys();
            var keys = await cmd.ExecuteAsync();
            return View(keys);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(ApiKeyCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                var cmd = new EditApiKeyCommand(request.ID, request.Name, request.Description, request.AboutUrl, request.RedirectUrl);
                var response = await cmd.Execute();
            }
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(string id)
        {
            var q = new QueryAPIKey(id);
            var r = await q.ExecuteAsync().ConfigureAwait(false);

            if (r != null && r.IsActive && User.Identity.Name.IsEqual(r.UserName))
            {
                var model = new ApiKeyCreateRequest() { AboutUrl = r.AppAboutUrl, Description = r.AppDescription, ID = r.PublicKey, Name = r.AppName, RedirectUrl = r.RedirectUrl };
                return View("Edit", model);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }


        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            //delete key
            var cmd = new DeleteApiKeyCommand(id);
            var result = await cmd.Execute();
            return RedirectToAction("Index");
        }

        public ActionResult Create()
        {
            return View("Edit");
        }

        [HttpPost]
        public async Task<ActionResult> Create(ApiKeyCreateRequest model)
        {
            if (!Settings.ApiKeyCreationEnabled)
            {
                return RedirectToAction("Index");
            }
            else if (ModelState.IsValid)
            {
                var cmd = new CreateApiKeyCommand(model.Name, model.Description, model.AboutUrl, model.RedirectUrl);
                await cmd.Execute();
                return RedirectToAction("Index");
            }
            return View("Edit");
        }

    }
}