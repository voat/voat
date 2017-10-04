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

using System.Collections.Generic;
using System.Linq;
using System.Net;
using Voat.Models;
using Voat.Models.ViewModels;
using Voat.Utilities;
using Voat.Caching;
using Voat.Common;
using Voat.Domain.Query;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Voat.Data.Models;
using Voat.Configuration;
using System;

namespace Voat.Controllers
{
    public class AjaxGatewayController : BaseController
    {
        private readonly VoatOutOfRepositoryDataContextAccessor _db = new VoatOutOfRepositoryDataContextAccessor();
        
        //// GET: MessageContent
        //public async Task<ActionResult> MessageContent(int? messageId)
        //{
        //    if (messageId.HasValue)
        //    {
        //        var q = new QuerySubmission(messageId.Value);
        //        var result = await q.ExecuteAsync();

        //        if (result != null)
        //        {
        //            var mpm = new MarkdownPreviewModel();

        //            if (!String.IsNullOrEmpty(result.Content))
        //            {
        //                mpm.MessageContent = (String.IsNullOrEmpty(result.FormattedContent) ? Formatting.FormatMessage(result.Content) : result.FormattedContent);
        //            }
        //            else
        //            {
        //                mpm.MessageContent = "<p>This submission only has a title.</p>"; //"format" this content
        //            }

        //            return PartialView("~/Views/AjaxViews/_MessageContent.cshtml", mpm);
        //        }
        //    }
        //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //}

        // GET: subverse link flairs for selected subverse
        [Authorize]
        public async Task<ActionResult> SubverseLinkFlairs(string subverse, int? id)
        {
            // get model for selected subverse
            var subverseObject = DataCache.Subverse.Retrieve(subverse);

            if (subverseObject == null || id == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
            }

            var submission = DataCache.Submission.Retrieve(id);

            if (submission == null || submission.Subverse != subverse)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }

            // check if caller is subverse owner or moderator, if not, deny listing
            if (!ModeratorPermission.HasPermission(User, subverse, Domain.Models.ModeratorAction.AssignFlair))
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.Unauthorized));
            }

            var q = new QuerySubverseFlair(subverseObject.Name);
            var flairs = await q.ExecuteAsync();

            ViewBag.SubmissionId = id;
            ViewBag.SubverseName = subverse;

            return PartialView("~/Views/AjaxViews/_LinkFlairSelectDialog.cshtml", flairs);
        }

        // GET: title from Uri
        [Authorize]
        public async Task<JsonResult> TitleFromUri()
        {
            if (VoatSettings.Instance.OutgoingTraffic.Enabled)
            {
                var uri = Request.Query["uri"].FirstOrDefault();
                uri = uri.TrimSafe();

                if (!string.IsNullOrEmpty(uri) && UrlUtility.IsUriValid(uri, true, true))
                {
                    //Old Code:
                    //string title = UrlUtility.GetTitleFromUri(uri);
                    using (var httpResource = new HttpResource(
                        new Uri(uri), 
                        new HttpResourceOptions() { AllowAutoRedirect = true }, 
                        VoatSettings.Instance.OutgoingTraffic.Proxy.ToWebProxy()))
                    {
                        await httpResource.GiddyUp();

                        string title = httpResource.Title;

                        if (title != null)
                        {
                            title = title.StripUnicode();
                            var resultList = new List<string>
                        {
                            title
                        };

                            return Json(resultList /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
                        }
                    }
                }
            }
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json("Bad request." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        }

        // GET: subverse names containing search term (used for autocomplete on new submission views)
        public async Task<JsonResult> AutocompleteSubverseName(string term, bool exact = false)
        {

            var q = new QuerySubmissionSubverseSettings(term, exact);
            var resultList = await q.ExecuteAsync().ConfigureAwait(false);
            return Json(resultList);
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
            return HybridError(ErrorViewModel.GetErrorViewModel(HttpStatusCode.BadRequest));
            //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // POST: preview stylesheet
        [Authorize]
        public ActionResult PreviewStylesheet(string subverse, bool previewMode)
        {
            return RedirectToRoute(Models.ROUTE_NAMES.SUBVERSE_INDEX, new { subverse = subverse, previewMode = previewMode });
        }

        //// GET: subverse basic info used for V2 sets layout
        //public ActionResult SubverseBasicInfo(int setId, string subverseName)
        //{
        //    var userSetDefinition = _db.UserSetLists.FirstOrDefault(s => s.UserSetID == setId && s.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase));

        //    return PartialView("~/Views/AjaxViews/_SubverseInfo.cshtml", userSetDefinition);
        //}

        // GET: basic info about a user
        //[OutputCache(Duration = 600, VaryByParam = "*")]
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
