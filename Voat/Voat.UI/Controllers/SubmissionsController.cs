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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Voat.Caching;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain;
using Voat.Domain.Command;
using Voat.Models;
using Voat.Utilities;


namespace Voat.Controllers
{
    public class SubmissionsController : Controller
    {
        private readonly voatEntities _db = new voatEntities();

        // POST: apply a link flair to given submission
        [Authorize]
        [HttpPost]
        [VoatValidateAntiForgeryToken]
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
        [VoatValidateAntiForgeryToken]
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
        [VoatValidateAntiForgeryToken]
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
                    CreationDate = Repository.CurrentDate,
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
        public async Task<JsonResult> Vote(int submissionID, int typeOfVote)
        {
            var cmd = new SubmissionVoteCommand(submissionID, typeOfVote, IpHash.CreateHash(UserGateway.UserIpAddress(this.Request)));
            var result = await cmd.Execute();
            return Json(result);
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
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> EditSubmission(EditSubmission model)
        {

            var cmd = new EditSubmissionCommand(model.SubmissionId, new Domain.Models.UserSubmission() { Content = model.SubmissionContent });
            var response = await cmd.Execute();

            if (response.Success)
            {
                DataCache.Submission.Remove(model.SubmissionId);
                CacheHandler.Instance.Remove(CachingKey.Submission(model.SubmissionId));
                return Json(new { response = response.Response.FormattedContent });

            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, response.Message);
            }
            
        }

        // POST: deletesubmission
        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
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
                        submissionToDelete.Content = "deleted by author at " + Repository.CurrentDate;
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
                        CreationDate = Repository.CurrentDate
                    };

                    _db.SubmissionRemovalLogs.Add(removalLog);

                    // notify submission author that his submission has been deleted by a moderator
                    var message = new Domain.Models.SendMessage()
                    {
                        Sender = $"v/{submissionToDelete.Subverse}",
                        Recipient = submissionToDelete.UserName,
                        Subject = "Your submission has been deleted by a moderator",
                        Message =   "Your [submission](/v/" + submissionToDelete.Subverse + "/comments/" + submissionToDelete.ID + ") has been deleted by: " +
                                    "/u/" + User.Identity.Name + " at " + Repository.CurrentDate + "  " + Environment.NewLine +
                                    "Original submission content was: " + Environment.NewLine +
                                    "---" + Environment.NewLine +
                                    (submissionToDelete.Type == 1 ? 
                                    "Submission title: " + submissionToDelete.Title + ", " + Environment.NewLine +
                                    "Submission content: " + submissionToDelete.Content
                                    :
                                    "Link description: " + submissionToDelete.Title + ", " + Environment.NewLine +
                                    "Link URL: " + submissionToDelete.Url
                                    )
                    };
                    var cmd = new SendMessageCommand(message);
                    await cmd.Execute();

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