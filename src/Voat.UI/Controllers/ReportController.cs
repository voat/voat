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
using System;
using System.Linq;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Data;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Http.Filters;
using Voat.Models.ViewModels;
using Voat.UI.Utilities;
using Voat.Utilities;

namespace Voat.Controllers
{
    public class ReportController : BaseController
    {
        #region NEW CODE
        
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Reports(string subverse, Domain.Models.ContentType? type = null, int days = 1, Domain.Models.ReviewStatus status = Domain.Models.ReviewStatus.Unreviewed, int[] ruleid = null)
        {
            days = days.EnsureRange(1, 7);

            if (subverse.IsEqual("all"))
            {
                subverse = null;
            }
            else
            {
                //check perms
                if (!ModeratorPermission.HasPermission(User, subverse, Domain.Models.ModeratorAction.AccessReports))
                {
                    return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.Unauthorized));
                }
            }
            //TODO: Implement Command/Query - Remove direct Repository access
            using (var repo = new Repository(User))
            {
                var data = await repo.GetRuleReports(subverse, type, days * 24, status, ruleid);
                ViewBag.Days = days;
                ViewBag.RuleID = ruleid;
                ViewBag.ReviewStatus = status;

                //Add Mod Menu
                if (!subverse.IsEqual("all"))
                {
                    ViewBag.NavigationViewModel = new Models.ViewModels.NavigationViewModel()
                    {
                        Description = "Moderation",
                        Name = subverse,
                        MenuType = Models.ViewModels.MenuType.Moderator,
                        BasePath = String.Format("/v/{0}/about", subverse),
                        Sort = null
                    };
                }

                return View(data);
            }
        }

        [Authorize]
        public async Task<ActionResult> Mark(string subverse, ContentType type, int id)
        {

            var cmd = new MarkReportsCommand(subverse, type, id).SetUserContext(User);
            var result = await cmd.Execute();
            return JsonResult(result);
        }

        [Authorize]
        public async Task<ActionResult> UserReportDialog(string subverse, ContentType type, int id)
        {
            //returns all rulesets for sub and global
            var q = new QuerySubverseRuleSets(subverse);
            var rulesets = await q.ExecuteAsync();

            var filtered = rulesets.Where(x => x.ContentType == null || x.ContentType == (byte)type).AsEnumerable();

            return PartialView(new ReportContentModel() { Subverse = subverse, ContentType = type, ID = id, Rules = filtered });

        }
        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(10)]
        public async Task<ActionResult> ReportContent(ReportContentModel model)
        {
            if (ModelState.IsValid)
            {
                var cmd = new SaveRuleReportCommand(model.ContentType, model.ID, model.RuleSetID.Value).SetUserContext(User);
                var result = await cmd.Execute();
                return JsonResult(result);
            }
            else
            {
                PreventSpamAttribute.Reset(HttpContext);
                return JsonResult(CommandResponse.FromStatus(Status.Error, ModelState.GetFirstErrorMessage()));
            }
        }

        private ActionResult JsonResult(string v)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
