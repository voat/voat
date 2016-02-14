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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Voat.Configuration;
using Voat.Data.Models;
using Voat.Models;
using Voat.Utilities;


namespace Voat.Controllers
{
    public class SubmissionsController : Controller
    {
        private readonly voatEntities _db = new voatEntities();
        private static readonly object _locker = new object();

        // POST: apply a link flair to given submission
        [Authorize]
        [HttpPost]
        public ActionResult ApplyLinkFlair(int? submissionID, int? flairId)
        {
            if (submissionID == null || flairId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var submission = _db.Submissions.Find(submissionID);
            if (submission == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            } 
            // check if caller is subverse moderator, if not, deny posting
            if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, submission.Subverse)) return new HttpUnauthorizedResult();

            // find flair by id, apply it to submission
            var flairModel = _db.SubverseFlairs.Find(flairId);
            if (flairModel == null || flairModel.Subverse != submission.Subverse) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // apply flair and save submission
            submission.FlairCss = flairModel.CssClass;
            submission.FlairLabel = flairModel.Label;
            _db.SaveChanges();
            DataCache.Submission.Remove(submissionID.Value);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // POST: clear link flair from a given submission
        [Authorize]
        [HttpPost]
        public ActionResult ClearLinkFlair(int? submissionID)
        {
            if (submissionID == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // get model for selected submission
            var submissionModel = _db.Submissions.Find(submissionID);

            if (submissionModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // check if caller is subverse moderator, if not, deny posting
            if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, submissionModel.Subverse)) return new HttpUnauthorizedResult();
            // clear flair and save submission
            submissionModel.FlairCss = null;
            submissionModel.FlairLabel = null;
            _db.SaveChanges();
            DataCache.Submission.Remove(submissionID.Value);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // POST: toggle sticky status of a submission
        [Authorize]
        [HttpPost]
        public ActionResult ToggleSticky(int submissionID)
        {
            // get model for selected submission
            var submissionModel = _db.Submissions.Find(submissionID);

            if (submissionModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // check if caller is subverse moderator, if not, deny change
            if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, submissionModel.Subverse)) return new HttpUnauthorizedResult();
            try
            {
                // find and clear current sticky if toggling
                var existingSticky = _db.StickiedSubmissions.FirstOrDefault(s => s.SubmissionID == submissionID);
                if (existingSticky != null)
                {
                    _db.StickiedSubmissions.Remove(existingSticky);
                    _db.SaveChanges();
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }

                // remove all stickies for subverse matching submission subverse
                _db.StickiedSubmissions.RemoveRange(_db.StickiedSubmissions.Where(s => s.Subverse == submissionModel.Subverse));

                // set new submission as sticky
                var stickyModel = new StickiedSubmission
                {
                    SubmissionID = submissionID,
                    CreatedBy = User.Identity.Name,
                    CreationDate = DateTime.Now,
                    Subverse = submissionModel.Subverse
                };

                _db.StickiedSubmissions.Add(stickyModel);
                _db.SaveChanges();

                DataCache.Submission.Remove(submissionID);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        // vote on a submission
        // POST: vote/{messageId}/{typeOfVote}
        [Authorize]
        public JsonResult Vote(int messageId, int typeOfVote)
        {
            lock (_locker)
            {
                int dailyVotingQuota = Settings.DailyVotingQuota;
                var loggedInUser = User.Identity.Name;
                var userCcp = Karma.CommentKarma(loggedInUser);
                var scaledDailyVotingQuota = Math.Max(dailyVotingQuota, userCcp / 2);
                var totalVotesUsedInPast24Hours = UserHelper.TotalVotesUsedInPast24Hours(User.Identity.Name);

                switch (typeOfVote)
                {
                    case 1:
                        if (userCcp >= 20)
                        {
                            if (totalVotesUsedInPast24Hours < scaledDailyVotingQuota)
                            {
                                // perform upvoting or resetting
                                Voting.UpvoteSubmission(messageId, loggedInUser, IpHash.CreateHash(UserHelper.UserIpAddress(Request)));
                            }
                        }
                        else if (totalVotesUsedInPast24Hours < 11)
                        {
                            // perform upvoting or resetting even if user has no CCP but only allow 10 votes per 24 hours
                            Voting.UpvoteSubmission(messageId, loggedInUser, IpHash.CreateHash(UserHelper.UserIpAddress(Request)));
                        }
                        break;
                    case -1:
                        if (userCcp >= 100)
                        {
                            if (totalVotesUsedInPast24Hours < scaledDailyVotingQuota)
                            {
                                // perform downvoting or resetting
                                Voting.DownvoteSubmission(messageId, loggedInUser, IpHash.CreateHash(UserHelper.UserIpAddress(Request)));
                            }
                        }
                        break;
                }
                return Json("Voting ok", JsonRequestBehavior.AllowGet);
            }
        }

        // save a submission
        // POST: save/{messageId}
        [Authorize]
        public JsonResult Save(int messageId)
        {
            var loggedInUser = User.Identity.Name;
            // perform saving or unsaving
            Saving.SaveSubmission(messageId, loggedInUser);
            return Json("Saving ok", JsonRequestBehavior.AllowGet);
        }

        // POST: editsubmission
        [Authorize]
        [HttpPost]
        public ActionResult EditSubmission(EditSubmission model)
        {
            var existingSubmission = _db.Submissions.Find(model.SubmissionId);

            if (existingSubmission == null) return Json("Unauthorized edit or submission not found.", JsonRequestBehavior.AllowGet);

            if (existingSubmission.IsDeleted) return Json("This submission has been deleted.", JsonRequestBehavior.AllowGet);
            if (existingSubmission.UserName.Trim() != User.Identity.Name) return Json("Unauthorized edit.", JsonRequestBehavior.AllowGet);

            existingSubmission.Content = model.SubmissionContent;
            existingSubmission.LastEditDate = DateTime.Now;

            _db.SaveChanges();
            DataCache.Submission.Remove(model.SubmissionId);

            // parse the new submission through markdown formatter and then return the formatted submission so that it can replace the existing html submission which just got modified
            var formattedSubmission = Formatting.FormatMessage(model.SubmissionContent);
            return Json(new { response = formattedSubmission });
        }

        // POST: deletesubmission
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> DeleteSubmission(int submissionId)
        {
            var submissionToDelete = _db.Submissions.Find(submissionId);

            if (submissionToDelete != null)
            {
                // delete submission if delete request is issued by submission author
                if (submissionToDelete.UserName == User.Identity.Name)
                {
                    submissionToDelete.IsDeleted = true;

                    if (submissionToDelete.Type == 1)
                    {
                        submissionToDelete.Content = "deleted by author at " + DateTime.Now;
                    }
                    else
                    {
                        submissionToDelete.Content = "http://voat.co";
                    }

                    // remove sticky if submission was stickied
                    var existingSticky = _db.StickiedSubmissions.FirstOrDefault(s => s.SubmissionID == submissionId);
                    if (existingSticky != null)
                    {
                        _db.StickiedSubmissions.Remove(existingSticky);
                    }

                    await _db.SaveChangesAsync();
                    DataCache.Submission.Remove(submissionId);

                }

                // delete submission if delete request is issued by subverse moderator
                else if (UserHelper.IsUserSubverseModerator(User.Identity.Name, submissionToDelete.Subverse))
                {
                    // mark submission as deleted
                    submissionToDelete.IsDeleted = true;

                    // move the submission to removal log
                    var removalLog = new SubmissionRemovalLog
                    {
                        SubmissionID = submissionToDelete.ID,
                        Moderator = User.Identity.Name,
                        Reason = "This feature is not yet implemented",
                        CreationDate = DateTime.Now
                    };

                    _db.SubmissionRemovalLogs.Add(removalLog);

                    if (submissionToDelete.Type == 1)
                    {
                        // notify submission author that his submission has been deleted by a moderator
                        MesssagingUtility.SendPrivateMessage(
                            "Voat",
                            submissionToDelete.UserName,
                            "Your submission has been deleted by a moderator",
                            "Your [submission](/v/" + submissionToDelete.Subverse + "/comments/" + submissionToDelete.ID + ") has been deleted by: " +
                            "/u/" + User.Identity.Name + " at " + DateTime.Now + "  " + Environment.NewLine +
                            "Original submission content was: " + Environment.NewLine +
                            "---" + Environment.NewLine +
                            "Submission title: " + submissionToDelete.Title + ", " + Environment.NewLine +
                            "Submission content: " + submissionToDelete.Content
                            );
                    }
                    else
                    {
                        // notify submission author that his submission has been deleted by a moderator
                        MesssagingUtility.SendPrivateMessage(
                            "Voat",
                            submissionToDelete.UserName,
                            "Your submission has been deleted by a moderator",
                            "Your [submission](/v/" + submissionToDelete.Subverse + "/comments/" + submissionToDelete.ID + ") has been deleted by: " +
                            "/u/" + User.Identity.Name + " at " + DateTime.Now + "  " + Environment.NewLine +
                            "Original submission content was: " + Environment.NewLine +
                            "---" + Environment.NewLine +
                            "Link description: " + submissionToDelete.LinkDescription + ", " + Environment.NewLine +
                            "Link URL: " + submissionToDelete.Content
                            );
                    }

                    // remove sticky if submission was stickied
                    var existingSticky = _db.StickiedSubmissions.FirstOrDefault(s => s.SubmissionID == submissionId);
                    if (existingSticky != null)
                    {
                        _db.StickiedSubmissions.Remove(existingSticky);
                    }

                    await _db.SaveChangesAsync();
                    DataCache.Submission.Remove(submissionId);

                }
            }

            string url = Request.UrlReferrer.AbsolutePath;
            return Redirect(url);
        }
    }

}