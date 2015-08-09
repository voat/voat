/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2014 Voat
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

namespace Voat.Controllers
{
    public class AjaxGatewayController : Controller
    {
        private readonly voatEntities _db = new voatEntities();

        // GET: MessageContent
        public ActionResult MessageContent(int? messageId)
        {

            var message = DataCache.Submission.Retrieve(messageId);

            if (message != null)
            {
                var mpm = new MarkdownPreviewModel();

                if (message.MessageContent != null)
                {
                    mpm.MessageContent = message.MessageContent;
                    return PartialView("~/Views/AjaxViews/_MessageContent.cshtml", mpm);
                }

                mpm.MessageContent = "This submission only has a title.";
                return PartialView("~/Views/AjaxViews/_MessageContent.cshtml", mpm);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // GET: EmbedVideo
        public ActionResult VideoPlayer(int? messageId)
        {

            var message = DataCache.Submission.Retrieve(messageId.Value);

            if (message != null)
            {
                if (message.MessageContent != null)
                {
                    return PartialView("~/Views/AjaxViews/_VideoPlayer.cshtml", message);
                }

                message.MessageContent = "There was a problem loading video.";
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

            if (subverseModel == null || messageId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);


            Message submission = DataCache.Submission.Retrieve(messageId);

            if (submission == null || submission.Subverse != subversetoshow)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // check if caller is subverse owner or moderator, if not, deny listing
            if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, subversetoshow))
            {
                return new HttpUnauthorizedResult();
            }

            var subverseLinkFlairs = _db.Subverseflairsettings
                .Where(n => n.Subversename == subversetoshow)
                .Take(10)
                .ToList()
                .OrderBy(s => s.Id);

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
                .Where(s => s.name.ToLower().StartsWith(term))
                .Take(10).ToArray();

            // jquery UI doesn't play nice with key value pairs so we have to build a simple string array
            if (!subverseNameSuggestions.Any()) return Json(resultList, JsonRequestBehavior.AllowGet);
            resultList.AddRange(subverseNameSuggestions.Select(item => item.name));

            return Json(resultList, JsonRequestBehavior.AllowGet);
        }

        // GET: markdown format a submission and return rendered result
        [Authorize]
        [HttpPost]
        public ActionResult RenderSubmission(MarkdownPreviewModel submissionModel)
        {
            if (submissionModel != null)
            {
                return PartialView("~/Views/AjaxViews/_MessageContent.cshtml", submissionModel);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // POST: preview stylesheet
        [Authorize]
        public ActionResult PreviewStylesheet(string subversetoshow, bool previewMode)
        {
            return RedirectToRoute("SubverseIndex", new { subversetoshow, previewMode });
        }

        // GET: subverse basic info used for V2 sets layout
        public ActionResult SubverseBasicInfo(int setId, string subverseName)
        {
            var userSetDefinition = _db.Usersetdefinitions.FirstOrDefault(s => s.Set_id == setId && s.Subversename.Equals(subverseName, StringComparison.OrdinalIgnoreCase));

            return PartialView("~/Views/AjaxViews/_SubverseInfo.cshtml", userSetDefinition);
        }

        // GET: basic info about a user
        [OutputCache(Duration = 600, VaryByParam = "*")]
        public ActionResult UserBasicInfo(string userName)
        {
            var userRegistrationDateTime = UserHelper.GetUserRegistrationDateTime(userName);
            var memberFor = Submissions.CalcSubmissionAge(userRegistrationDateTime);
            var scp = Karma.LinkKarma(userName);
            var ccp = Karma.CommentKarma(userName);

            var userInfoModel = new BasicUserInfo()
            {
                MemberSince = memberFor,
                Ccp = ccp,
                Scp = scp
            };

            return PartialView("~/Views/AjaxViews/_BasicUserInfo.cshtml", userInfoModel);
        }
    }
}