/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
*/

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Whoaverse.Models;
using Whoaverse.Utils;

namespace Whoaverse.Controllers
{
    public class SubmissionsController : Controller
    {
        private whoaverseEntities db = new whoaverseEntities();

        // POST: apply a link flair to given submission
        [Authorize]
        [HttpPost]
        public ActionResult ApplyLinkFlair(int? submissionId, int? flairId)
        {
            if (submissionId != null && flairId != null)
            {
                // get model for selected submission
                var submissionModel = db.Messages.Find(submissionId);

                if (submissionModel != null)
                {
                    // check if caller is subverse moderator, if not, deny posting
                    if (Whoaverse.Utils.User.IsUserSubverseModerator(User.Identity.Name, submissionModel.Subverse) || Whoaverse.Utils.User.IsUserSubverseAdmin(User.Identity.Name, submissionModel.Subverse))
                    {
                        // find flair by id, apply it to submission
                        var flairModel = db.Subverseflairsettings.Find(flairId);
                        if (flairModel != null && flairModel.Subversename == submissionModel.Subverse)
                        {
                            // apply flair and save submission
                            submissionModel.FlairCss = flairModel.CssClass;
                            submissionModel.FlairLabel = flairModel.Label;
                            db.SaveChanges();
                            return new HttpStatusCodeResult(HttpStatusCode.OK);
                        }

                        // flar model was not found, return badrequest httpstatuscode
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    else
                    {
                        return new HttpUnauthorizedResult();
                    }
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // POST: clear link flair from a given submission
        [Authorize]
        [HttpPost]
        public ActionResult ClearLinkFlair(int? submissionId)
        {
            if (submissionId != null)
            {
                // get model for selected submission
                var submissionModel = db.Messages.Find(submissionId);

                if (submissionModel != null)
                {
                    // check if caller is subverse moderator, if not, deny posting
                    if (Whoaverse.Utils.User.IsUserSubverseModerator(User.Identity.Name, submissionModel.Subverse) || Whoaverse.Utils.User.IsUserSubverseAdmin(User.Identity.Name, submissionModel.Subverse))
                    {
                        // clear flair and save submission
                        submissionModel.FlairCss = null;
                        submissionModel.FlairLabel = null;
                        db.SaveChanges();
                        return new HttpStatusCodeResult(HttpStatusCode.OK);
                    }
                    else
                    {
                        return new HttpUnauthorizedResult();
                    }
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // POST: toggle sticky status of a submission
        [Authorize]
        [HttpPost]
        public ActionResult ToggleSticky(int submissionId)
        {
            // get model for selected submission
            var submissionModel = db.Messages.Find(submissionId);

            if (submissionModel != null)
            {
                // check if caller is subverse moderator, if not, deny change
                if (Whoaverse.Utils.User.IsUserSubverseModerator(User.Identity.Name, submissionModel.Subverse) || Whoaverse.Utils.User.IsUserSubverseAdmin(User.Identity.Name, submissionModel.Subverse))
                {
                    try
                    {
                        // find and clear current sticky if toggling
                        var existingSticky = db.Stickiedsubmissions.Where(s => s.Submission_id == submissionId).FirstOrDefault();
                        if (existingSticky != null)
                        {
                            db.Stickiedsubmissions.Remove(existingSticky);
                            db.SaveChanges();
                            return new HttpStatusCodeResult(HttpStatusCode.OK);
                        }

                        // remove all stickies for subverse matching submission subverse
                        db.Stickiedsubmissions.RemoveRange(db.Stickiedsubmissions.Where(s => s.Subversename == submissionModel.Subverse));

                        // set new submission as sticky
                        var stickyModel = new Stickiedsubmission();
                        stickyModel.Submission_id = submissionId;
                        stickyModel.Stickied_by = User.Identity.Name;
                        stickyModel.Stickied_date = DateTime.Now;
                        stickyModel.Subversename = submissionModel.Subverse;

                        db.Stickiedsubmissions.Add(stickyModel);
                        db.SaveChanges();

                        return new HttpStatusCodeResult(HttpStatusCode.OK);
                    }
                    catch (Exception)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    }
                }
                else
                {
                    return new HttpUnauthorizedResult();
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // POST: vote/{messageId}/{typeOfVote}
        [Authorize]
        public JsonResult Vote(int messageId, int typeOfVote)
        {
            string loggedInUser = User.Identity.Name;

            if (typeOfVote == 1)
            {
                if (Karma.CommentKarma(loggedInUser) > 20)
                {
                    // perform upvoting or resetting
                    Voting.UpvoteSubmission(messageId, loggedInUser);
                }
                else if (Whoaverse.Utils.User.TotalVotesUsedInPast24Hours(User.Identity.Name) < 11)
                {
                    // perform upvoting or resetting even if user has no CCP but only allow 10 votes per 24 hours
                    Voting.UpvoteSubmission(messageId, loggedInUser);
                }
            }
            else if (typeOfVote == -1)
            {
                // ignore downvote if user link karma is below certain treshold
                if (Karma.CommentKarma(loggedInUser) > 100)
                {
                    // perform downvoting or resetting
                    Voting.DownvoteSubmission(messageId, loggedInUser);
                }
            }
            return Json("Voting ok", JsonRequestBehavior.AllowGet);
        }

        // POST: editsubmission
        [Authorize]
        [HttpPost]
        public ActionResult EditSubmission(EditSubmission model)
        {
            var existingSubmission = db.Messages.Find(model.SubmissionId);

            if (existingSubmission != null)
            {
                if (existingSubmission.Name.Trim() == User.Identity.Name)
                {
                    existingSubmission.MessageContent = model.SubmissionContent;
                    existingSubmission.LastEditDate = System.DateTime.Now;
                    db.SaveChanges();

                    // parse the new submission through markdown formatter and then return the formatted submission so that it can replace the existing html submission which just got modified
                    string formattedSubmission = Utils.Formatting.FormatMessage(model.SubmissionContent);
                    return Json(new { response = formattedSubmission });
                }
                else
                {
                    return Json("Unauthorized edit.", JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                return Json("Unauthorized edit or submission not found.", JsonRequestBehavior.AllowGet);
            }

        }

        // POST: deletesubmission
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> DeleteSubmission(int submissionId)
        {
            Message submissionToDelete = db.Messages.Find(submissionId);

            if (submissionToDelete != null)
            {
                if (submissionToDelete.Name == User.Identity.Name)
                {
                    submissionToDelete.Name = "deleted";

                    if (submissionToDelete.Type == 1)
                    {
                        submissionToDelete.MessageContent = "deleted by author at " + System.DateTime.Now;
                    }
                    else
                    {
                        submissionToDelete.MessageContent = "http://whoaverse.com";
                    }

                    await db.SaveChangesAsync();
                }
                // delete submission if delete request is issued by subverse moderator
                else if (Whoaverse.Utils.User.IsUserSubverseAdmin(User.Identity.Name, submissionToDelete.Subverse) || Whoaverse.Utils.User.IsUserSubverseModerator(User.Identity.Name, submissionToDelete.Subverse))
                {

                    if (submissionToDelete.Type == 1)
                    {
                        // notify submission author that his submission has been deleted by a moderator
                        Utils.MesssagingUtility.SendPrivateMessage(
                            "Whoaverse",
                            submissionToDelete.Name,
                            "Your submission has been deleted by a moderator",
                            "Your [submission](/v/" + submissionToDelete.Subverse + "/comments/" + submissionToDelete.Id + ") has been deleted by: " +
                            "[" + User.Identity.Name + "](/u/" + User.Identity.Name + ")" + " at " + System.DateTime.Now + "  " + Environment.NewLine +
                            "Original submission content was: " + Environment.NewLine +
                            "---" + Environment.NewLine +
                            "Submission title: " + submissionToDelete.Title + ", " + Environment.NewLine +
                            "Submission content: " + submissionToDelete.MessageContent
                            );

                        submissionToDelete.MessageContent = "deleted by a moderator at " + System.DateTime.Now;
                        submissionToDelete.Name = "deleted";
                    }
                    else
                    {
                        // notify submission author that his submission has been deleted by a moderator
                        Utils.MesssagingUtility.SendPrivateMessage(
                            "Whoaverse",
                            submissionToDelete.Name,
                            "Your submission has been deleted by a moderator",
                            "Your [submission](/v/" + submissionToDelete.Subverse + "/comments/" + submissionToDelete.Id + ") has been deleted by: " +
                            "[" + User.Identity.Name + "](/u/" + User.Identity.Name + ")" + " at " + System.DateTime.Now + "  " + Environment.NewLine +
                            "Original submission content was: " + Environment.NewLine +
                            "---" + Environment.NewLine +
                            "Link description: " + submissionToDelete.Linkdescription + ", " + Environment.NewLine +
                            "Link URL: " + submissionToDelete.MessageContent
                            );

                        submissionToDelete.MessageContent = "http://whoaverse.com";
                        submissionToDelete.Name = "deleted";
                    }

                    await db.SaveChangesAsync();
                }
            }

            string url = this.Request.UrlReferrer.AbsolutePath;
            return Redirect(url);
        }
    }

}