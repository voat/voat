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
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Whoaverse.Models;
using Whoaverse.Utils;

namespace Whoaverse.Controllers
{
    public class CommentController : Controller
    {
        private whoaverseEntities db = new whoaverseEntities();
        Random rnd = new Random();

        // POST: votecomment/{commentId}/{typeOfVote}
        [Authorize]
        public JsonResult VoteComment(int commentId, int typeOfVote)
        {
            string loggedInUser = User.Identity.Name;

            if (typeOfVote == 1)
            {
                if (Karma.CommentKarma(loggedInUser) > 20)
                {
                    // perform upvoting or resetting
                    VotingComments.UpvoteComment(commentId, loggedInUser);
                }
                else if (Whoaverse.Utils.User.TotalVotesUsedInPast24Hours(User.Identity.Name) < 11)
                {
                    // perform upvoting or resetting even if user has no CCP but only allow 10 votes per 24 hours
                    VotingComments.UpvoteComment(commentId, loggedInUser);
                }
            }
            else if (typeOfVote == -1)
            {
                // ignore downvote if user comment karma is below certain treshold
                if (Karma.CommentKarma(loggedInUser) > 100)
                {
                    // perform downvoting or resetting
                    VotingComments.DownvoteComment(commentId, loggedInUser);
                }
            }

            Response.StatusCode = 200;
            return Json("Voting ok", JsonRequestBehavior.AllowGet);
        }

        // GET: comments for a given submission
        public ActionResult Comments(int? id, string subversetoshow, int? startingcommentid, string sort)
        {
            var subverse = db.Subverses.Find(subversetoshow);

            if (subverse != null)
            {
                ViewBag.SelectedSubverse = subverse.name;
                ViewBag.SubverseAnonymized = subverse.anonymized_mode;

                if (startingcommentid != null)
                {
                    ViewBag.StartingCommentId = startingcommentid;
                }

                if (sort != null)
                {
                    ViewBag.SortingMode = sort;
                }

                if (id == null)
                {
                    return View("~/Views/Errors/Error.cshtml");
                }

                Message message = db.Messages.Find(id);

                if (message == null)
                {
                    return View("~/Views/Errors/Error_404.cshtml");
                }

                // make sure that the combination of selected subverse and message subverse are linked
                if (!message.Subverse.Equals(subversetoshow, StringComparison.OrdinalIgnoreCase))
                {
                    return View("~/Views/Errors/Error_404.cshtml");
                }

                // experimental
                // register a new session for this subverse
                try
                {
                    string currentSubverse = (string)this.RouteData.Values["subversetoshow"];
                    SessionTracker.Add(currentSubverse, Session.SessionID);
                }
                catch (Exception)
                {
                    //
                }

                return View("~/Views/Home/Comments.cshtml", message);
            }
            else
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }
        }

        // GET: submitcomment
        public ActionResult SubmitComment()
        {
            return View("~/Views/Errors/Error_404.cshtml");
        }

        // POST: submitcomment, adds a new root comment
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [PreventSpam(DelayRequest = 120, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitComment([Bind(Include = "Id,CommentContent,MessageId,ParentId")] Comment comment)
        {
            comment.Date = System.DateTime.Now;
            comment.Name = User.Identity.Name;
            comment.Votes = 0;
            comment.Likes = 0;

            if (ModelState.IsValid)
            {
                // flag the comment as anonymized if it was submitted to a sub which has active anonymized_mode
                Message message = db.Messages.Find(comment.MessageId);
                if (message != null && message.Anonymized || message.Subverses.anonymized_mode)
                {
                    comment.Anonymized = true;
                }

                // check if user is banned, don't save the comment if true
                if (!Utils.User.IsUserBanned(User.Identity.Name))
                {
                    db.Comments.Add(comment);
                    await db.SaveChangesAsync();
                }

                // send comment reply notification to parent comment author if the comment is not a new root comment
                if (comment.ParentId != null && comment.CommentContent != null)
                {
                    // find the parent comment and its author
                    var parentComment = db.Comments.Find(comment.ParentId);
                    if (parentComment != null)
                    {
                        // check if recipient exists
                        if (Whoaverse.Utils.User.UserExists(parentComment.Name))
                        {
                            // do not send notification if author is the same as comment author
                            if (parentComment.Name != User.Identity.Name)
                            {
                                // send the message
                                var commentReplyNotification = new Commentreplynotification();
                                var commentMessage = db.Messages.Find(comment.MessageId);
                                if (commentMessage != null)
                                {
                                    commentReplyNotification.CommentId = comment.Id;
                                    commentReplyNotification.SubmissionId = commentMessage.Id;
                                    commentReplyNotification.Recipient = parentComment.Name;
                                    if (parentComment.Message.Anonymized || parentComment.Message.Subverses.anonymized_mode)
                                    {
                                        commentReplyNotification.Sender = rnd.Next(10000, 20000).ToString();
                                    }
                                    else
                                    {
                                        commentReplyNotification.Sender = User.Identity.Name;
                                    }
                                    commentReplyNotification.Body = comment.CommentContent;
                                    commentReplyNotification.Subverse = commentMessage.Subverse;
                                    commentReplyNotification.Status = true;
                                    commentReplyNotification.Timestamp = System.DateTime.Now;

                                    // self = type 1, url = type 2
                                    if (parentComment.Message.Type == 1)
                                    {
                                        commentReplyNotification.Subject = parentComment.Message.Title;
                                    }
                                    else
                                    {
                                        commentReplyNotification.Subject = parentComment.Message.Linkdescription;
                                    }

                                    db.Commentreplynotifications.Add(commentReplyNotification);

                                    await db.SaveChangesAsync();
                                }
                                else
                                {
                                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // comment reply is sent to a root comment which has no parent id, trigger post reply notification
                    var commentMessage = db.Messages.Find(comment.MessageId);
                    if (commentMessage != null)
                    {
                        // check if recipient exists
                        if (Whoaverse.Utils.User.UserExists(commentMessage.Name))
                        {
                            // do not send notification if author is the same as comment author
                            if (commentMessage.Name != User.Identity.Name)
                            {
                                // send the message
                                var postReplyNotification = new Postreplynotification();

                                postReplyNotification.CommentId = comment.Id;
                                postReplyNotification.SubmissionId = commentMessage.Id;
                                postReplyNotification.Recipient = commentMessage.Name;

                                if (commentMessage.Anonymized || commentMessage.Subverses.anonymized_mode)
                                {
                                    postReplyNotification.Sender = rnd.Next(10000, 20000).ToString();
                                }
                                else
                                {
                                    postReplyNotification.Sender = User.Identity.Name;
                                }

                                postReplyNotification.Body = comment.CommentContent;
                                postReplyNotification.Subverse = commentMessage.Subverse;
                                postReplyNotification.Status = true;
                                postReplyNotification.Timestamp = System.DateTime.Now;

                                // self = type 1, url = type 2
                                if (commentMessage.Type == 1)
                                {
                                    postReplyNotification.Subject = commentMessage.Title;
                                }
                                else
                                {
                                    postReplyNotification.Subject = commentMessage.Linkdescription;
                                }

                                db.Postreplynotifications.Add(postReplyNotification);

                                await db.SaveChangesAsync();
                            }
                        }
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }

                }
                string url = this.Request.UrlReferrer.AbsolutePath;
                return Redirect(url);
            }
            else
            {
                if (Request.IsAjaxRequest())
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                ModelState.AddModelError(String.Empty, "Sorry, you are doing that too fast. Please try again in 2 minutes.");
                return View("~/Views/Help/SpeedyGonzales.cshtml");
            }
        }

        // POST: editcomment
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        public ActionResult EditComment(EditComment model)
        {
            var existingComment = db.Comments.Find(model.CommentId);

            if (existingComment != null)
            {
                if (existingComment.Name.Trim() == User.Identity.Name)
                {
                    existingComment.CommentContent = model.CommentContent;
                    existingComment.LastEditDate = System.DateTime.Now;
                    db.SaveChanges();

                    //parse the new comment through markdown formatter and then return the formatted comment so that it can replace the existing html comment which just got modified
                    string formattedComment = Utils.Formatting.FormatMessage(model.CommentContent);
                    return Json(new { response = formattedComment });
                }
                else
                {
                    return Json("Unauthorized edit.", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Unauthorized edit or comment not found.", JsonRequestBehavior.AllowGet);
            }
        }

        // POST: deletecomment
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> DeleteComment(int commentId)
        {
            Comment commentToDelete = db.Comments.Find(commentId);

            if (commentToDelete != null)
            {
                string commentSubverse = commentToDelete.Message.Subverse;

                // delete comment if the comment author is currently logged in user
                if (commentToDelete.Name == User.Identity.Name)
                {
                    commentToDelete.Name = "deleted";
                    commentToDelete.CommentContent = "deleted by author at " + System.DateTime.Now;
                    await db.SaveChangesAsync();
                }
                // delete comment if delete request is issued by subverse moderator
                else if (Whoaverse.Utils.User.IsUserSubverseAdmin(User.Identity.Name, commentSubverse) || Whoaverse.Utils.User.IsUserSubverseModerator(User.Identity.Name, commentSubverse))
                {
                    // notify comment author that his comment has been deleted by a moderator
                    Utils.MesssagingUtility.SendPrivateMessage(
                        "Whoaverse",
                        commentToDelete.Name,
                        "Your comment has been deleted by a moderator",
                        "Your [comment](/v/" + commentSubverse + "/comments/" + commentToDelete.MessageId + "/" + commentToDelete.Id + ") has been deleted by: " +
                        "[" + User.Identity.Name + "](/u/" + User.Identity.Name + ")" + " on: " + System.DateTime.Now + "  " + Environment.NewLine +
                        "Original comment content was: " + Environment.NewLine +
                        "---" + Environment.NewLine +
                        commentToDelete.CommentContent
                        );

                    commentToDelete.Name = "deleted";
                    commentToDelete.CommentContent = "deleted by a moderator at " + System.DateTime.Now;
                    await db.SaveChangesAsync();
                }
            }

            string url = this.Request.UrlReferrer.AbsolutePath;
            return Redirect(url);
        }
    }
}