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
using System.Threading.Tasks;
using Voat.Common;
using Voat.Data;
using Voat.Domain.Models;
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
                if (!ModeratorPermission.HasPermission(User.Identity.Name, subverse, Domain.Models.ModeratorAction.AccessReports))
                {
                    return UnAuthorizedErrorView();
                }
            }

            using (var repo = new Repository())
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
            //TODO: Move into Query
            using (var repo = new Repository())
            {
                var result = await repo.MarkReportsAsReviewed(subverse, type, id);
                return JsonResult(result);
            }
        }

        [Authorize]
        public async Task<ActionResult> UserReportDialog(string subverse, ContentType type, int id)
        {
            //TODO: Move into Query
            using (var repo = new Repository())
            {
                var data = await repo.GetRuleSets(subverse, type);
                return PartialView(new Models.ViewModels.ReportContentModel() { Subverse = subverse, ContentType = type, ID = id, Rules = data });
            }
        }
        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(DelayRequest = 30, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public async Task<ActionResult> ReportContent(ReportContentModel model)
        {
            if (ModelState.IsValid)
            {
                using (var repo = new Repository())
                {
                    var result = await repo.SaveRuleReport(model.ContentType, model.ID, model.RuleSetID.Value);
                    return JsonResult(result);
                }
            }
            else
            {
                PreventSpamAttribute.Reset();
                return JsonError(ModelState.GetFirstErrorMessage());
            }
        }
        #endregion

        //#region OLD CODE
        //private readonly voatEntities _db = new VoatUIDataContextAccessor();

        //private TimeSpan contentReportTimeOut = TimeSpan.FromHours(6);
        //private TimeSpan contentUserCountTimeOut = TimeSpan.FromHours(2);

        ////// POST: ReportContent
        ////[HttpPost]
        ////[Authorize]
        ////[VoatValidateAntiForgeryToken]
        ////[PreventSpam(DelayRequest = 30, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        ////public async Task<ActionResult> ReportContent(ContentType type, int id)
        ////{
        ////    ActionResult result = null;
        ////    switch (type)
        ////    {
        ////        case ContentType.Comment:
        ////            result = await ReportComment(id);
        ////            break;
        ////        case ContentType.Submission:
        ////            result = await ReportSubmission(id);
        ////            break;
        ////        default:
        ////            result = new EmptyResult();
        ////            break;
        ////    }
        ////    return result;
        ////}

        //public async Task<ActionResult> ReportSubmission(int id)
        //{
        //    var s = _db.Submissions.Find(id);

        //    if (s != null)
        //    {
        //        // prepare report headers
        //        var subverse = s.Subverse;

        //        //don't allow banned users to send reports
        //        if (!UserHelper.IsUserBannedFromSubverse(User.Identity.Name, subverse) && !UserHelper.IsUserGloballyBanned(User.Identity.Name))
        //        {
        //            var reportTimeStamp = Repository.CurrentDate.ToString(CultureInfo.InvariantCulture);
        //            try
        //            {
        //                string cacheKeyReport = CachingKey.ReportKey(ContentType.Submission, id);

        //                //see if comment has been reported before
        //                if (CacheHandler.Instance.Retrieve<object>(cacheKeyReport) == null)
        //                {
        //                    //mark comment in cache as having been reported
        //                    CacheHandler.Instance.Register(cacheKeyReport, new Func<object>(() => { return new object(); }), contentReportTimeOut, -1);


        //                    string userName = User.Identity.Name;
        //                    string cacheKeyUserReportCount = CachingKey.ReportCountUserKey(ContentType.Submission, userName);
        //                    int reportsPerUserThreshold = 5;

        //                    var reportCountViaUser = CacheHandler.Instance.Retrieve<int?>(cacheKeyUserReportCount);
        //                    //ensure user is below reporting threshold
        //                    if (reportCountViaUser == null || reportCountViaUser.Value <= reportsPerUserThreshold)
        //                    {
        //                        //add or update cache with current user reports
        //                        if (reportCountViaUser.HasValue)
        //                        {
        //                            CacheHandler.Instance.Replace<int?>(cacheKeyUserReportCount, new Func<int?, int?>((x) => { return (int?)(x.Value + 1); }), contentUserCountTimeOut);
        //                        }
        //                        else
        //                        {
        //                            CacheHandler.Instance.Register<int?>(cacheKeyUserReportCount, new Func<int?>(() => { return (int?)1; }), contentUserCountTimeOut, -1);
        //                        }

        //                        string body = String.Format("This submission has been reported:\r\n\r\nhttps://voat.co/v/{0}/{1}\r\n\r\n\r\nReport Spammers to v/ReportSpammers.", s.Subverse, s.ID);

        //                        var message = new Domain.Models.SendMessage()
        //                        {
        //                            Sender = userName,
        //                            Recipient = $"v/{subverse}",
        //                            Subject = "Submission Spam Report",
        //                            Message = body
        //                        };
        //                        var cmd = new SendMessageCommand(message, true);
        //                        await cmd.Execute();

        //                        //MesssagingUtility.SendPrivateMessage(commentToReport.IsAnonymized ? "Anon" : userName, String.Format("v/{0}", commentSubverse), "Comment Spam Report", body);
        //                    }
        //                }
        //            }
        //            catch (Exception)
        //            {
        //                return new HttpStatusCodeResult(HttpStatusCode.ServiceUnavailable, "Service Unavailable");
        //            }
        //        }
        //    }
        //    else
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");
        //    }

        //    return new HttpStatusCodeResult(HttpStatusCode.OK, "OK");
        //}

        //public async Task<ActionResult> ReportComment(int id)
        //{
        //    var comment = _db.Comments.Find(id);

        //    if (comment != null)
        //    {
        //        // prepare report headers
        //        var subverse = comment.Submission.Subverse;

        //        //don't allow banned users to send reports
        //        if (!UserHelper.IsUserBannedFromSubverse(User.Identity.Name, subverse) && !UserHelper.IsUserGloballyBanned(User.Identity.Name))
        //        {
        //            var reportTimeStamp = Repository.CurrentDate.ToString(CultureInfo.InvariantCulture);
        //            try
        //            {
        //                string cacheKeyReport = CachingKey.ReportKey(ContentType.Comment, id);

        //                //see if comment has been reported before
        //                if (CacheHandler.Instance.Retrieve<object>(cacheKeyReport) == null)
        //                {
        //                    //mark comment in cache as having been reported
        //                    CacheHandler.Instance.Register(cacheKeyReport, new Func<object>(() => { return new object(); }), contentReportTimeOut, -1);


        //                    string userName = User.Identity.Name;
        //                    string cacheKeyUserReportCount = CachingKey.ReportCountUserKey(ContentType.Comment, userName);
        //                    int reportsPerUserThreshold = 5;

        //                    var reportCountViaUser = CacheHandler.Instance.Retrieve<int?>(cacheKeyUserReportCount);
        //                    //ensure user is below reporting threshold
        //                    if (reportCountViaUser == null || reportCountViaUser.Value <= reportsPerUserThreshold)
        //                    {
        //                        //add or update cache with current user reports
        //                        if (reportCountViaUser.HasValue)
        //                        {
        //                            CacheHandler.Instance.Replace<int?>(cacheKeyUserReportCount, new Func<int?, int?>((x) => { return (int?)(x.Value + 1); }), contentUserCountTimeOut);
        //                        }
        //                        else
        //                        {
        //                            CacheHandler.Instance.Register<int?>(cacheKeyUserReportCount, new Func<int?>(() => { return (int?)1; }), contentUserCountTimeOut, -1);
        //                        }

        //                        string body = String.Format("This comment has been reported:\r\n\r\nhttps://voat.co/v/{0}/{1}/{2}?context=10\r\n\r\n\r\nReport Spammers to v/ReportSpammers.", subverse, comment.SubmissionID, id);

        //                        var message = new Domain.Models.SendMessage()
        //                        {
        //                            Sender = userName,
        //                            Recipient = $"v/{subverse}",
        //                            Subject = "Comment Spam Report",
        //                            Message = body
        //                        };
        //                        var cmd = new SendMessageCommand(message, true);
        //                        await cmd.Execute();

        //                        //MesssagingUtility.SendPrivateMessage(commentToReport.IsAnonymized ? "Anon" : userName, String.Format("v/{0}", commentSubverse), "Comment Spam Report", body);
        //                    }
        //                }
        //            }
        //            catch (Exception)
        //            {
        //                return new HttpStatusCodeResult(HttpStatusCode.ServiceUnavailable, "Service Unavailable");
        //            }
        //        }
        //    }
        //    else
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");
        //    }

        //    return new HttpStatusCodeResult(HttpStatusCode.OK, "OK");
        //}
        //#endregion 
    }
}
