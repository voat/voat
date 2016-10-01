﻿/*
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
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Models;
using Voat.UI.Utilities;
using Voat.Utilities;

namespace Voat.Controllers
{
    public class ReportController : Controller
    {
        private readonly voatEntities _db = new voatEntities();

        // POST: reportcomment
        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(DelayRequest = 30, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public async Task<ActionResult> ReportComment(int id)
        {
            var commentToReport = _db.Comments.Find(id);

            if (commentToReport != null)
            {
                // prepare report headers
                var commentSubverse = commentToReport.Submission.Subverse;

                //don't allow banned users to send reports
                if (!UserHelper.IsUserBannedFromSubverse(User.Identity.Name, commentSubverse) && !UserHelper.IsUserGloballyBanned(User.Identity.Name))
                {
                    var reportTimeStamp = Repository.CurrentDate.ToString(CultureInfo.InvariantCulture);
                    try
                    {
                        string cacheKeyCommentReport = String.Format("legacy:report.comment.{0}", id.ToString());

                        //see if comment has been reported before
                        if (CacheHandler.Instance.Retrieve<object>(cacheKeyCommentReport) == null)
                        {
                            //mark comment in cache as having been reported
                            CacheHandler.Instance.Register(cacheKeyCommentReport, new Func<object>(() => { return new object(); }), TimeSpan.FromHours(6), -1);

                                
                            string userName = User.Identity.Name;
                            string cacheKeyUserReportCount = String.Format("legacy:report.comment.{0}.count", userName);
                            int reportsPerUserThreshold = 5;

                            var reportCountViaUser = CacheHandler.Instance.Retrieve<int?>(cacheKeyUserReportCount);
                            //ensure user is below reporting threshold
                            if (reportCountViaUser == null || reportCountViaUser.Value <= reportsPerUserThreshold)
                            {
                                //add or update cache with current user reports
                                if (reportCountViaUser.HasValue)
                                {
                                    CacheHandler.Instance.Replace<int?>(cacheKeyUserReportCount, new Func<int?, int?>((x) => { return (int?)(x.Value + 1); }));
                                }
                                else
                                {
                                    CacheHandler.Instance.Register<int?>(cacheKeyUserReportCount, new Func<int?>(() => { return (int?)1; }), TimeSpan.FromMinutes(120), -1);
                                }

                                string body = String.Format("This comment has been reported as spam:\r\n\r\nhttps://voat.co/v/{0}/comments/{1}/{2}/{2}#{2}\r\n\r\n\r\nReport Spammers to v/ReportSpammers.", commentSubverse, commentToReport.SubmissionID, id);

                                var message = new Domain.Models.SendMessage()
                                {
                                    Sender = commentToReport.IsAnonymized ? "Anon" : userName,
                                    Recipient = $"v/{commentSubverse}",
                                    Subject = "Comment Spam Report",
                                    Message = body
                                };
                                var cmd = new SendMessageCommand(message);
                                await cmd.Execute();

                                //MesssagingUtility.SendPrivateMessage(commentToReport.IsAnonymized ? "Anon" : userName, String.Format("v/{0}", commentSubverse), "Comment Spam Report", body);


                            }

                        }

                    }
                    catch (Exception)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.ServiceUnavailable, "Service Unavailable");
                    }
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK, "OK");
        }
    }
}