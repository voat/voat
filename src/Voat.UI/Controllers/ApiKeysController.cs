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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Configuration;
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
            var cmd = new QueryUserApiKeys().SetUserContext(User);
            var keys = await cmd.ExecuteAsync();
            return View(keys);
        }

        [HttpPost]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ApiKeyCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                var cmd = new EditApiKeyCommand(request.ID, request.Name, request.Description, request.AboutUrl, request.RedirectUrl).SetUserContext(User);
                var response = await cmd.Execute();
            }
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(string id)
        {
            var q = new QueryAPIKey(id).SetUserContext(User);
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
            var cmd = new DeleteApiKeyCommand(id).SetUserContext(User);
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
            if (!VoatSettings.Instance.ApiKeyCreationEnabled)
            {
                return RedirectToAction("Index");
            }
            else if (ModelState.IsValid)
            {
                var cmd = new CreateApiKeyCommand(model.Name, model.Description, model.AboutUrl, model.RedirectUrl).SetUserContext(User);
                await cmd.Execute();
                return RedirectToAction("Index");
            }
            return View("Edit");
        }

    }
}
