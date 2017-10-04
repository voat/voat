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

//using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Voat.Caching;
using Voat.Common;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Domain.Models.Input;
using Voat.Domain.Query;
using Voat.Http;
using Voat.Http.Filters;
using Voat.Models;
using Voat.Models.ViewModels;
using Voat.UI.Utilities;
using Voat.Utilities;
using Voat.Utilities.Components;

namespace Voat.Controllers
{
    public class CommentController : BaseController
    {
        private CommentSortAlgorithm GetSortMode(string sort)
        {
            var sortMode = CommentSortAlgorithm.Top;
                        
            if (String.IsNullOrEmpty(sort))
            {
                if (User.Identity.IsAuthenticated)
                {
                    var prefs = UserData.Preferences;
                    if (prefs != null)
                    {
                        sortMode = prefs.CommentSort;
                    }
                }
            }
            else if (!Enum.TryParse(sort, out sortMode))
            {
                sortMode = CommentSortAlgorithm.Top;
            }
            return sortMode;
        }

        //// POST: votecomment/{commentId}/{typeOfVote}
        //[HttpPost]
        //[Authorize]
        //[VoatValidateAntiForgeryToken]
        //public async Task<JsonResult> VoteComment(int commentId, int typeOfVote)
        //{
        //    var cmd = new CommentVoteCommand(commentId, typeOfVote, IpHash.CreateHash(UserHelper.UserIpAddress(this.Request)));
        //    var result = await cmd.Execute();
        //    return Json(result);
        //}

        

        // GET: Renders Primary Submission Comments Page
        public async Task<ActionResult> Comments(int? submissionID, string subverseName, int? commentID, string sort, int? context)
        {
            #region Validation

            if (submissionID == null)
            {
                return ErrorView(new ErrorViewModel() { Description = "Can not find what was requested because input is not valid" });
            }

            var q = new QuerySubmission(submissionID.Value, true).SetUserContext(User);

            var submission = await q.ExecuteAsync().ConfigureAwait(false);

            if (submission == null)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }

            // make sure that the combination of selected subverse and submission subverse are linked
            if (!submission.Subverse.IsEqual(subverseName))
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }

            var subverse = DataCache.Subverse.Retrieve(subverseName);

            if (subverse == null)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }

            if (subverse.IsAdminDisabled.HasValue && subverse.IsAdminDisabled.Value)
            {
                ViewBag.Subverse = subverse.Name;
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseDisabled));
            }

            #endregion

            if (commentID != null)
            {
                ViewBag.StartingCommentId = commentID;
                ViewBag.CommentToHighLight = commentID;
            }

            #region Set ViewBag
            ViewBag.Subverse = subverse;
            ViewBag.Submission = submission;
            //This is a required view bag property in _Layout.cshtml - Update: hmmm, don't think so but too lazy to look 
            ViewBag.SelectedSubverse = subverse.Name;

            var sortingMode = GetSortMode(sort);

            ViewBag.SortingMode = sortingMode;

            #endregion

            var cmd = new LogVisitCommand(null, submissionID.Value, Request.RemoteAddress()).SetUserContext(User);
            cmd.Execute();

            CommentSegment model = null;
            if (commentID != null)
            {
                ViewBag.CommentToHighLight = commentID.Value;
                model = await GetCommentContext(submission.ID, commentID.Value, context, sortingMode);
            }
            else
            {
                model = await GetCommentSegment(submission.ID, null, 0, sortingMode);
            }
            
            var q2 = new QuerySubverseModerators(subverseName).SetUserContext(User);
            ViewBag.ModeratorList = await q2.ExecuteAsync();



            ViewBag.NavigationViewModel = new NavigationViewModel()
            {
                Description = "Subverse",
                Name = subverseName,
                MenuType = MenuType.Subverse,
                BasePath = VoatUrlFormatter.BasePath(new DomainReference(DomainType.Subverse, subverseName)),
                Sort = null
            };

            return View("~/Views/Home/Comments.cshtml", model);

        }

        private async Task<CommentSegment> GetCommentSegment(int submissionID, int? parentID, int startingIndex, CommentSortAlgorithm sort)
        {
            var q = new QueryCommentSegment(submissionID, parentID, startingIndex, sort).SetUserContext(User);
            var results = await q.ExecuteAsync();
            return results;
        }
        private async Task<CommentSegment> GetCommentContext(int submissionID, int commentID, int? contextCount, CommentSortAlgorithm sort)
        {
            var q = new QueryCommentContext(submissionID, commentID, contextCount, sort).SetUserContext(User);
            var results = await q.ExecuteAsync();
            return results;
        }

        // url: "/comments/" + submission + "/" + parentId + "/" + command + "/" + startingIndex + "/" + count + "/" + nestingLevel + "/" + sort + "/",
        // GET: Renders a Section of Comments within the already existing tree
        //Leaving (string command) in for backwards compat with mobile html clients. this is no longer used
        public async Task<ActionResult> CommentSegment(int submissionID, int? parentID, string command, int startingIndex, string sort)
        {
            #region Validation

            if (submissionID <= 0)
            {
                return ErrorView(new ErrorViewModel() { Description = "Can not find what was requested because input is not valid" });
            }

            var q = new QuerySubmission(submissionID, false);
            var submission = await q.ExecuteAsync().ConfigureAwait(false);

            if (submission == null)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }

            var subverse = DataCache.Subverse.Retrieve(submission.Subverse);

            if (subverse == null)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }

            if (subverse.IsAdminDisabled.HasValue && subverse.IsAdminDisabled.Value)
            {
                ViewBag.Subverse = subverse.Name;
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseDisabled));
            }

            #endregion

            #region Set ViewBag
            ViewBag.Subverse = subverse;
            ViewBag.Submission = submission;

            var sortingMode = GetSortMode(sort);
            ViewBag.SortingMode = sortingMode;

            #endregion

            var q2 = new QuerySubverseModerators(subverse.Name);
            ViewBag.ModeratorList = await q2.ExecuteAsync();

            var results = await GetCommentSegment(submissionID, parentID, startingIndex, sortingMode);
            return PartialView("~/Views/Shared/Comments/_CommentSegment.cshtml", results);
        }

        // GET: Renders a New Comment Tree
        public async Task<ActionResult> CommentTree(int submissionID, string sort)
        {
            #region Validation

            if (submissionID <= 0)
            {
                return ErrorView(new ErrorViewModel() { Description = "Can not find what was requested because input is not valid" });
            }

            var submission = DataCache.Submission.Retrieve(submissionID);

            if (submission == null)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }

            var subverse = DataCache.Subverse.Retrieve(submission.Subverse);

            if (subverse == null)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }

            if (subverse.IsAdminDisabled.HasValue && subverse.IsAdminDisabled.Value)
            {
                ViewBag.Subverse = subverse.Name;
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseDisabled));
            }

            #endregion

            #region Set ViewBag
            ViewBag.Subverse = subverse;
            ViewBag.Submission = submission;

            var sortingMode = GetSortMode(sort);
            ViewBag.SortingMode = sortingMode;

            #endregion

            var results = await GetCommentSegment(submissionID, null, 0, sortingMode);
            return PartialView("~/Views/Shared/Comments/_CommentTree.cshtml", results);
        }

        // GET: submitcomment
        public ActionResult SubmitComment()
        {
            return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
        }

        // POST: submitcomment, adds a new root comment
        [HttpPost]
        [Authorize]
        [PreventSpam(30)]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitComment(CommentInput commentModel)
        {
            if (!ModelState.IsValid)
            {
                //we want to reset spam filter if the modelstate was not triggered by preventspam
                PreventSpamAttribute.Reset(HttpContext);
                return JsonResult(CommandResponse.FromStatus(Status.Error, ModelState.GetFirstErrorMessage()));
            }
            else
            {
                var cmd = new CreateCommentCommand(commentModel.SubmissionID.Value, commentModel.ParentID, commentModel.Content).SetUserContext(User);
                var result = await cmd.Execute();

                if (result.Success)
                {
                    //if good return formatted comment
                    if (Request.IsAjaxRequest())
                    {
                        var comment = result.Response;
                        comment.IsOwner = true;
                        ViewBag.CommentId = comment.ID; //why?
                        ViewBag.rootComment = comment.ParentID == null; //why?
                        return PartialView("~/Views/Shared/Comments/_SubmissionComment.cshtml", comment);
                    }
                    else
                    {
                        return new EmptyResult();
                    }
                }
                else
                {
                    PreventSpamAttribute.Reset(HttpContext);
                    return JsonResult(result);
                }
            }
        }

        // POST: editcomment
        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(15)]
        public async Task<ActionResult> EditComment([FromBody()] CommentEditInput commentModel)
        {
            if (ModelState.IsValid)
            {
                var cmd = new EditCommentCommand(commentModel.ID, commentModel.Content).SetUserContext(User);
                var result = await cmd.Execute();

                if (!result.Success)
                {
                    PreventSpamAttribute.Reset(HttpContext);
                }
                return JsonResult(result);
            }
            else
            {
                PreventSpamAttribute.Reset(HttpContext);
                return JsonResult(CommandResponse.FromStatus(Status.Error, ModelState.GetFirstErrorMessage()));
            }
        }
        // POST: deletecomment
        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(15)]
        public async Task<ActionResult> DeleteComment(int id)
        {
            if (ModelState.IsValid)
            {
                var cmd = new DeleteCommentCommand(id, "This feature is not yet implemented").SetUserContext(User);
                var result = await cmd.Execute();
                return base.JsonResult(result);
            }
            else
            {
                return base.JsonResult(CommandResponse.FromStatus(Status.Error, ModelState.GetFirstErrorMessage()));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        #region MOVE TO SubverseModeration CONTROLLER

        // POST: comments/distinguish/{commentId}
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PreventSpam(15)]
        public async Task<JsonResult> DistinguishComment(int commentId)
        {
            using (var repo = new Repository(User))
            {
                var result = await repo.DistinguishComment(commentId);
                return JsonResult(result);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> ModeratorDelete(string subverse, int submissionID, int commentID)
        {

            if (!ModeratorPermission.HasPermission(User, subverse, Domain.Models.ModeratorAction.DeleteComments))
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.Unauthorized));
            }
            var q = new QueryComment(commentID);
            var comment = await q.ExecuteAsync();

            if (comment == null || comment.SubmissionID != submissionID)
            {
                ModelState.AddModelError("", "Can not find comment. Who did this?");
                return View(new ModeratorDeleteContentViewModel());
            }
            if (!comment.Subverse.IsEqual(subverse))
            {
                ModelState.AddModelError("", "Data mismatch detected");
                return View(new ModeratorDeleteContentViewModel());
            }

            ViewBag.Comment = comment;
            return View(new ModeratorDeleteContentViewModel() { ID = commentID });
        }

        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> ModeratorDelete(string subverse, int submissionID, ModeratorDeleteContentViewModel model)
        {
            var q = new QueryComment(model.ID);
            var comment = await q.ExecuteAsync();

            if (comment == null || comment.SubmissionID != submissionID)
            {
                ModelState.AddModelError("", "Can not find comment. Who did this?");
                return View(new ModeratorDeleteContentViewModel());
            }
            if (!comment.Subverse.IsEqual(subverse))
            {
                ModelState.AddModelError("", "Data mismatch detected");
                return View(new ModeratorDeleteContentViewModel());
            }

            if (!ModeratorPermission.HasPermission(User, comment.Subverse, Domain.Models.ModeratorAction.DeleteComments))
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.Unauthorized));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cmd = new DeleteCommentCommand(model.ID, model.Reason).SetUserContext(User);
            var r = await cmd.Execute();
            if (r.Success)
            {
                     return RedirectToRoute("SubverseCommentsWithSort_Short", new { subverseName = subverse, submissionID = submissionID });
            }
            else
            {
                ModelState.AddModelError("", r.Message);
                return View(model);
            }




        }

        #endregion 
    }
}
