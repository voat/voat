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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Whoaverse.Models;

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
    }

}