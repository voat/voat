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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Voat.Models;
using Voat.Models.ViewModels;

using Voat.Data.Models;
using Voat.Utilities;
using Voat.Caching;
using Voat.Common;
using Voat.Domain.Query;
using System.Threading.Tasks;

namespace Voat.Controllers
{
    public class AjaxGatewayController : BaseController
    {
        private readonly voatEntities _db = new voatEntities();
        
        // GET: MessageContent
        public async Task<ActionResult> MessageContent(int? messageId)
        {
            if (messageId.HasValue)
            {
                var q = new QuerySubmission(messageId.Value);
                var result = await q.ExecuteAsync();

                if (result != null)
                {
                    var mpm = new MarkdownPreviewModel();

                    if (!String.IsNullOrEmpty(result.Content))
                    {
                        mpm.MessageContent = (String.IsNullOrEmpty(result.FormattedContent) ? Formatting.FormatMessage(result.Content) : result.FormattedContent);
                    }
                    else
                    {
                        mpm.MessageContent = "<p>This submission only has a title.</p>"; //"format" this content
                    }

                    return PartialView("~/Views/AjaxViews/_MessageContent.cshtml", mpm);
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // GET: EmbedVideo
        public ActionResult VideoPlayer(int? messageId)
        {

            var message = DataCache.Submission.Retrieve(messageId.Value);

            if (message != null)
            {
                if (message.Content != null)
                {
                    return PartialView("~/Views/AjaxViews/_VideoPlayer.cshtml", message);
                }

                message.Content = "There was a problem loading video.";
                return PartialView("~/Views/AjaxViews/_VideoPlayer.cshtml", message);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // GET: subverse link flairs for selected subverse
        [Authorize]
        public ActionResult SubverseLinkFlairs(string subversetoshow, int? messageId)
        {
            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subversetoshow);
            //var subverseModel = _db.Subverse.Find(subversetoshow);

            if (subverseModel == null || messageId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            var submission = DataCache.Submission.Retrieve(messageId);

            if (submission == null || submission.Subverse != subversetoshow)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // check if caller is subverse owner or moderator, if not, deny listing
            if (!ModeratorPermission.HasPermission(User.Identity.Name, subversetoshow, Domain.Models.ModeratorAction.AssignFlair))
            {
                return new HttpUnauthorizedResult();
            }

            var subverseLinkFlairs = _db.SubverseFlairs
                .Where(n => n.Subverse == subversetoshow)
                .Take(10)
                .ToList()
                .OrderBy(s => s.ID);

            ViewBag.SubmissionId = messageId;
            ViewBag.SubverseName = subversetoshow;

            return PartialView("~/Views/AjaxViews/_LinkFlairSelectDialog.cshtml", subverseLinkFlairs);
        }

        // GET: title from Uri
        [Authorize]
        public JsonResult TitleFromUri()
        {
            var uri = Request.Params["uri"];
            string title = UrlUtility.GetTitleFromUri(uri);

            if (title != null)
            {
                title = Formatting.StripUnicode(title);
                var resultList = new List<string>
                {
                    title
                };

                return Json(resultList, JsonRequestBehavior.AllowGet);
            }

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json("Bad request.", JsonRequestBehavior.AllowGet);
        }

        // GET: subverse names containing search term (used for autocomplete on new submission views)
        public JsonResult AutocompleteSubverseName(string term)
        {
            var resultList = new List<string>();

            var subverseNameSuggestions = _db.Subverses
                .Where(s => s.Name.ToLower().StartsWith(term))
                .Take(10).ToArray();

            // jquery UI doesn't play nice with key value pairs so we have to build a simple string array
            if (!subverseNameSuggestions.Any()) return Json(resultList, JsonRequestBehavior.AllowGet);
            resultList.AddRange(subverseNameSuggestions.Select(item => item.Name));

            return Json(resultList, JsonRequestBehavior.AllowGet);
        }

        // GET: markdown format a submission and return rendered result
        [Authorize]
        [HttpPost]
        public ActionResult RenderSubmission(MarkdownPreviewModel submissionModel)
        {
            if (submissionModel != null)
            {
                submissionModel.MessageContent = Formatting.FormatMessage(submissionModel.MessageContent, true);
                return PartialView("~/Views/AjaxViews/_MessageContent.cshtml", submissionModel);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // POST: preview stylesheet
        [Authorize]
        public ActionResult PreviewStylesheet(string subversetoshow, bool previewMode)
        {
            return RedirectToRoute("SubverseIndex", new { subverse = subversetoshow, previewMode = previewMode });
        }

        // GET: subverse basic info used for V2 sets layout
        public ActionResult SubverseBasicInfo(int setId, string subverseName)
        {
            var userSetDefinition = _db.UserSetLists.FirstOrDefault(s => s.UserSetID == setId && s.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase));

            return PartialView("~/Views/AjaxViews/_SubverseInfo.cshtml", userSetDefinition);
        }

        // GET: basic info about a user
        [OutputCache(Duration = 600, VaryByParam = "*")]
        public ActionResult UserBasicInfo(string userName)
        {
            //acceptable constructor call
            var userData = new Domain.UserData(userName);
            var info = userData.Information;

            var memberFor = Age.ToRelative(info.RegistrationDate);
            var scp = info.SubmissionPoints.Sum;
            var ccp = info.CommentPoints.Sum;

            var userInfoModel = new BasicUserInfo()
            {
                MemberSince = memberFor,
                Ccp = ccp,
                Scp = scp,
                Bio = info.Bio
            };

            return PartialView("~/Views/AjaxViews/_BasicUserInfo.cshtml", userInfoModel);
        }
    }
}