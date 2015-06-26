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
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Voat.Models;
using Voat.Utils;

namespace Voat.Controllers
{
    public class SubmissionsController : Controller
    {
        private readonly whoaverseEntities _db = new whoaverseEntities();

        // POST: apply a link flair to given submission
        [Authorize]
        [HttpPost]
        public ActionResult ApplyLinkFlair(int? submissionId, int? flairId)
        {
            if (submissionId == null || flairId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var submissionModel = _db.Messages.Find(submissionId);
            if (submissionModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // check if caller is subverse moderator, if not, deny posting
            if (!Utils.User.IsUserSubverseModerator(User.Identity.Name, submissionModel.Subverse) &&
                !Utils.User.IsUserSubverseAdmin(User.Identity.Name, submissionModel.Subverse))
                return new HttpUnauthorizedResult();

            // find flair by id, apply it to submission
            var flairModel = _db.Subverseflairsettings.Find(flairId);
            if (flairModel == null || flairModel.Subversename != submissionModel.Subverse) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // apply flair and save submission
            submissionModel.FlairCss = flairModel.CssClass;
            submissionModel.FlairLabel = flairModel.Label;
            _db.SaveChanges();

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // POST: clear link flair from a given submission
        [Authorize]
        [HttpPost]
        public ActionResult ClearLinkFlair(int? submissionId)
        {
            if (submissionId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // get model for selected submission
            var submissionModel = _db.Messages.Find(submissionId);

            if (submissionModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // check if caller is subverse moderator, if not, deny posting
            if (!Utils.User.IsUserSubverseModerator(User.Identity.Name, submissionModel.Subverse) && !Utils.User.IsUserSubverseAdmin(User.Identity.Name, submissionModel.Subverse)) return new HttpUnauthorizedResult();
            // clear flair and save submission
            submissionModel.FlairCss = null;
            submissionModel.FlairLabel = null;
            _db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // POST: toggle sticky status of a submission
        [Authorize]
        [HttpPost]
        public ActionResult ToggleSticky(int submissionId)
        {
            // get model for selected submission
            var submissionModel = _db.Messages.Find(submissionId);

            if (submissionModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // check if caller is subverse moderator, if not, deny change
            if (!Utils.User.IsUserSubverseModerator(User.Identity.Name, submissionModel.Subverse) &&
                !Utils.User.IsUserSubverseAdmin(User.Identity.Name, submissionModel.Subverse))
                return new HttpUnauthorizedResult();
            try
            {
                // find and clear current sticky if toggling
                var existingSticky = _db.Stickiedsubmissions.FirstOrDefault(s => s.Submission_id == submissionId);
                if (existingSticky != null)
                {
                    _db.Stickiedsubmissions.Remove(existingSticky);
                    _db.SaveChanges();
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }

                // remove all stickies for subverse matching submission subverse
                _db.Stickiedsubmissions.RemoveRange(_db.Stickiedsubmissions.Where(s => s.Subversename == submissionModel.Subverse));

                // set new submission as sticky
                var stickyModel = new Stickiedsubmission
                {
                    Submission_id = submissionId,
                    Stickied_by = User.Identity.Name,
                    Stickied_date = DateTime.Now,
                    Subversename = submissionModel.Subverse
                };

                _db.Stickiedsubmissions.Add(stickyModel);
                _db.SaveChanges();

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
            int dailyVotingQuota = MvcApplication.DailyVotingQuota;
            var loggedInUser = User.Identity.Name;
            var userCcp = Karma.CommentKarma(loggedInUser);
            var scaledDailyVotingQuota = Math.Max(dailyVotingQuota, userCcp / 2);
            var totalVotesUsedInPast24Hours = Utils.User.TotalVotesUsedInPast24Hours(User.Identity.Name);

            switch (typeOfVote)
            {
                case 1:
                    if (userCcp >= 20)
                    {
                        if (totalVotesUsedInPast24Hours < scaledDailyVotingQuota)
                        {
                            // perform upvoting or resetting
                            Voting.UpvoteSubmission(messageId, loggedInUser, IpHash.CreateHash(Utils.User.UserIpAddress(Request)));
                        }
                    }
                    else if (totalVotesUsedInPast24Hours < 11)
                    {
                        // perform upvoting or resetting even if user has no CCP but only allow 10 votes per 24 hours
                        Voting.UpvoteSubmission(messageId, loggedInUser, IpHash.CreateHash(Utils.User.UserIpAddress(Request)));
                    }
                    break;
                case -1:
                    if (userCcp >= 100)
                    {
                        if (totalVotesUsedInPast24Hours < scaledDailyVotingQuota)
                        {
                            // perform downvoting or resetting
                            Voting.DownvoteSubmission(messageId, loggedInUser, IpHash.CreateHash(Utils.User.UserIpAddress(Request)));
                        }
                    }
                    break;
            }
            return Json("Voting ok", JsonRequestBehavior.AllowGet);
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
            var existingSubmission = _db.Messages.Find(model.SubmissionId);

            if (existingSubmission == null) return Json("Unauthorized edit or submission not found.", JsonRequestBehavior.AllowGet);
            if (existingSubmission.Name.Trim() != User.Identity.Name) return Json("Unauthorized edit.", JsonRequestBehavior.AllowGet);
            
            existingSubmission.MessageContent = model.SubmissionContent;
            existingSubmission.LastEditDate = DateTime.Now;

            _db.SaveChanges();

            // parse the new submission through markdown formatter and then return the formatted submission so that it can replace the existing html submission which just got modified
            var formattedSubmission = Formatting.FormatMessage(model.SubmissionContent);
            return Json(new { response = formattedSubmission });
        }

        // POST: deletesubmission
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> DeleteSubmission(int submissionId)
        {
            var submissionToDelete = _db.Messages.Find(submissionId);

            if (submissionToDelete != null)
            {
                // delete submission if delete request is issued by submission author
                if (submissionToDelete.Name == User.Identity.Name)
                {
                    submissionToDelete.Name = "deleted";

                    if (submissionToDelete.Type == 1)
                    {
                        submissionToDelete.MessageContent = "deleted by author at " + DateTime.Now;
                    }
                    else
                    {
                        submissionToDelete.MessageContent = "http://voat.co";
                    }

                    // remove sticky if submission was stickied
                    var existingSticky = _db.Stickiedsubmissions.FirstOrDefault(s => s.Submission_id == submissionId);
                    if (existingSticky != null)
                    {
                        _db.Stickiedsubmissions.Remove(existingSticky);
                    }

                    await _db.SaveChangesAsync();
                }

                // delete submission if delete request is issued by subverse moderator
                else if (Utils.User.IsUserSubverseAdmin(User.Identity.Name, submissionToDelete.Subverse) || Utils.User.IsUserSubverseModerator(User.Identity.Name, submissionToDelete.Subverse))
                {
                    // mark submission as deleted (TODO: don't use name, add a new bit field to messages table instead)
                    submissionToDelete.Name = "deleted";

                    // move the submission to removal log
                    var removalLog = new SubmissionRemovalLog
                    {
                        SubmissionId = submissionToDelete.Id,
                        Moderator = User.Identity.Name,
                        ReasonForRemoval = "This feature is not yet implemented",
                        RemovalTimestamp = DateTime.Now
                    };

                    _db.SubmissionRemovalLogs.Add(removalLog);

                    if (submissionToDelete.Type == 1)
                    {
                        // notify submission author that his submission has been deleted by a moderator
                        MesssagingUtility.SendPrivateMessage(
                            "Voat",
                            submissionToDelete.Name,
                            "Your submission has been deleted by a moderator",
                            "Your [submission](/v/" + submissionToDelete.Subverse + "/comments/" + submissionToDelete.Id + ") has been deleted by: " +
                            "/u/" + User.Identity.Name + " at " + DateTime.Now + "  " + Environment.NewLine +
                            "Original submission content was: " + Environment.NewLine +
                            "---" + Environment.NewLine +
                            "Submission title: " + submissionToDelete.Title + ", " + Environment.NewLine +
                            "Submission content: " + submissionToDelete.MessageContent
                            );
                    }
                    else
                    {
                        // notify submission author that his submission has been deleted by a moderator
                        MesssagingUtility.SendPrivateMessage(
                            "Voat",
                            submissionToDelete.Name,
                            "Your submission has been deleted by a moderator",
                            "Your [submission](/v/" + submissionToDelete.Subverse + "/comments/" + submissionToDelete.Id + ") has been deleted by: " +
                            "/u/" + User.Identity.Name + " at " + DateTime.Now + "  " + Environment.NewLine +
                            "Original submission content was: " + Environment.NewLine +
                            "---" + Environment.NewLine +
                            "Link description: " + submissionToDelete.Linkdescription + ", " + Environment.NewLine +
                            "Link URL: " + submissionToDelete.MessageContent
                            );
                    }

                    // remove sticky if submission was stickied
                    var existingSticky = _db.Stickiedsubmissions.FirstOrDefault(s => s.Submission_id == submissionId);
                    if (existingSticky != null)
                    {
                        _db.Stickiedsubmissions.Remove(existingSticky);
                    }

                    await _db.SaveChangesAsync();
                }
            }

            string url = Request.UrlReferrer.AbsolutePath;
            return Redirect(url);
        }
    }

}