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
using System.Net;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Http;
using Voat.Http.Filters;
using Voat.Models;
using Voat.Models.ViewModels;
using Voat.Utilities;


namespace Voat.Controllers
{
    public class SubmissionsController : BaseController
    {
        private readonly VoatOutOfRepositoryDataContextAccessor _db = new VoatOutOfRepositoryDataContextAccessor();

        // POST: apply a link flair to given submission
        [Authorize]
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        public ActionResult ApplyLinkFlair(int? submissionID, int? flairId)
        {
            if (submissionID == null || flairId == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var submission = _db.Submission.Find(submissionID);
            if (submission == null || submission.IsDeleted)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!ModeratorPermission.HasPermission(User, submission.Subverse, Domain.Models.ModeratorAction.AssignFlair))
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.Unauthorized));
                //return new HttpUnauthorizedResult();
            }

            // find flair by id, apply it to submission
            var flairModel = _db.SubverseFlair.Find(flairId);
            if (flairModel == null || flairModel.Subverse != submission.Subverse)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }
            //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // apply flair and save submission
            submission.FlairCss = flairModel.CssClass;
            submission.FlairLabel = flairModel.Label;
            _db.SaveChanges();
            DataCache.Submission.Remove(submissionID.Value);

            return JsonResult(CommandResponse.FromStatus(Status.Success));
            //return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // POST: clear link flair from a given submission
        [Authorize]
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(5)]
        public ActionResult ClearLinkFlair(int? submissionID)
        {
            if (submissionID == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // get model for selected submission
            var submissionModel = _db.Submission.Find(submissionID);

            if (submissionModel == null || submissionModel.IsDeleted)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // check if caller is subverse moderator, if not, deny posting
            if (!ModeratorPermission.HasPermission(User, submissionModel.Subverse, Domain.Models.ModeratorAction.AssignFlair))
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.Unauthorized));
            }

            // clear flair and save submission
            submissionModel.FlairCss = null;
            submissionModel.FlairLabel = null;
            _db.SaveChanges();
            DataCache.Submission.Remove(submissionID.Value);

            return JsonResult(CommandResponse.FromStatus(Status.Success));
            //return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // POST: toggle sticky status of a submission
        [Authorize]
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(5)]
        public async Task<ActionResult> ToggleSticky(int submissionID)
        {
            using (var repo = new Repository(User))
            {
                var response = await repo.ToggleSticky(submissionID);
                return JsonResult(response);
            }
        }

        [Authorize]
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(5)]
        public async Task<ActionResult> ToggleNSFW(int submissionID)
        {
            using (var repo = new Repository(User))
            {
                var response = await repo.ToggleNSFW(submissionID);
                return JsonResult(response);
            }
        }
        
        // POST: editsubmission
        [Authorize]
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(30)]
        public async Task<ActionResult> EditSubmission([FromBody] EditSubmission model)
        {
            if (ModelState.IsValid)
            {
                var cmd = new EditSubmissionCommand(model.SubmissionId, new Domain.Models.UserSubmission() { Content = model.SubmissionContent }).SetUserContext(User);
                var response = await cmd.Execute();

                if (response.Success)
                {
                    DataCache.Submission.Remove(model.SubmissionId);
                    CacheHandler.Instance.Remove(CachingKey.Submission(model.SubmissionId));
                    //return Json(new { response = response.Response.FormattedContent });

                }
                return JsonResult(response);
            }

            return JsonResult(CommandResponse.FromStatus(Status.Error, ModelState.GetFirstErrorMessage()));
            
        }

        // POST: deletesubmission
        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(30)]
        public async Task<ActionResult> DeleteSubmission(int id)
        {
            var cmd = new DeleteSubmissionCommand(id, "This feature is not yet implemented").SetUserContext(User);
            var result = await cmd.Execute();

            return JsonResult(result);
            
        }
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> ModeratorDelete(string subverse, int submissionID)
        {

            if (!ModeratorPermission.HasPermission(User, subverse, Domain.Models.ModeratorAction.DeletePosts))
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.Unauthorized));
            }

            //New Domain Submission
            //var q = new QuerySubmission(submissionID);
            //var s = await q.ExecuteAsync();
            
            //Legacy Data Submission (Since views expect this type, we use it for now)
            var s = DataCache.Submission.Retrieve(submissionID);
            ViewBag.Submission = s;

            if (s == null)
            {
                ModelState.AddModelError("", "Can not find submission. Is it hiding?");
                return View(new ModeratorDeleteContentViewModel());
            }
            if (s.IsDeleted)
            {
                ModelState.AddModelError("", "Can not delete a deleted post. Do you have no standards?");
            }

            return View(new ModeratorDeleteContentViewModel() { ID = s.ID });
        }

        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(10)]
        public async Task<ActionResult> ModeratorDelete(string subverse, int submissionID, ModeratorDeleteContentViewModel model)
        {
            //New Domain Submission
            //var q = new QuerySubmission(submissionID);
            //var s = await q.ExecuteAsync();

            //Legacy Data Submission (Since views expect this type, we use it for now)
            var s = DataCache.Submission.Retrieve(submissionID);
            ViewBag.Submission = s;
            if (s == null || s.ID != model.ID)
            {
                ModelState.AddModelError("", "Can not find submission. Is it hiding?");
                return View(model);
            }
            if (s.IsDeleted)
            {
                ModelState.AddModelError("", "Can not delete a deleted post. Do you have no standards?");
                return View(model);
            }

            if (!ModeratorPermission.HasPermission(User, s.Subverse, Domain.Models.ModeratorAction.DeletePosts))
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.Unauthorized));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cmd = new DeleteSubmissionCommand(model.ID, model.Reason).SetUserContext(User);
            var r = await cmd.Execute();
            if (r.Success)
            {
                return RedirectToRoute(Models.ROUTE_NAMES.SUBVERSE_INDEX, new { subverse = s.Subverse });
            }
            else
            {
                ModelState.AddModelError("", r.Message);
                return View(model);
            }
        }
    }
}
