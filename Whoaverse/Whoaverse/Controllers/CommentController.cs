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
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Voat.Models;
using Voat.Utils;

namespace Voat.Controllers
{
    public class CommentController : Controller
    {
        private readonly whoaverseEntities _db = new whoaverseEntities();
        readonly Random _rnd = new Random();

        // POST: votecomment/{commentId}/{typeOfVote}
        [Authorize]
        public JsonResult VoteComment(int commentId, int typeOfVote)
        {
            var loggedInUser = User.Identity.Name;

            switch (typeOfVote)
            {
                case 1:
                    if (Karma.CommentKarma(loggedInUser) > 20)
                    {
                        // perform upvoting or resetting
                        VotingComments.UpvoteComment(commentId, loggedInUser);
                    }
                    else if (Utils.User.TotalVotesUsedInPast24Hours(User.Identity.Name) < 11)
                    {
                        // perform upvoting or resetting even if user has no CCP but only allow 10 votes per 24 hours
                        VotingComments.UpvoteComment(commentId, loggedInUser);
                    }
                    break;
                case -1:
                    if (Karma.CommentKarma(loggedInUser) > 100)
                    {
                        // perform downvoting or resetting
                        VotingComments.DownvoteComment(commentId, loggedInUser);
                    }
                    break;
            }

            Response.StatusCode = 200;
            return Json("Voting ok", JsonRequestBehavior.AllowGet);
        }

        // GET: comments for a given submission
        public ActionResult Comments(int? id, string subversetoshow, int? startingcommentid, string sort)
        {
            var subverse = _db.Subverses.Find(subversetoshow);

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

                var message = _db.Messages.Find(id);

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
                    var currentSubverse = (string)RouteData.Values["subversetoshow"];
                    SessionTracker.Add(currentSubverse, Session.SessionID);
                }
                catch (Exception)
                {
                    //
                }

                // check if this is a new view and register it
                string clientIpAddress = String.Empty;

                if (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
                {
                    clientIpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                }
                else if (Request.UserHostAddress.Length != 0)
                {
                    clientIpAddress = Request.UserHostAddress;
                }

                if (clientIpAddress == String.Empty) return View("~/Views/Home/Comments.cshtml", message);

                // generate salted hash of client IP address
                string ipHash = IpHash.CreateHash(clientIpAddress);

                // check if this hash is present for this submission id in viewstatistics table
                var existingView = _db.Viewstatistics.Find(message.Id, ipHash);

                // this hash is already registered, display the submission and don't register the view
                if (existingView != null) return View("~/Views/Home/Comments.cshtml", message);

                // this is a new view, register it for this submission
                var view = new Viewstatistic { submissionId = message.Id, viewerId = ipHash };
                _db.Viewstatistics.Add(view);
                message.Views++;
                _db.SaveChanges();

                return View("~/Views/Home/Comments.cshtml", message);
            }
            return View("~/Views/Errors/Error_404.cshtml");
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
        public async Task<ActionResult> SubmitComment([Bind(Include = "Id, CommentContent, MessageId, ParentId")] Comment comment)
        {
            comment.Date = DateTime.Now;
            comment.Name = User.Identity.Name;
            comment.Votes = 0;
            comment.Likes = 0;

            if (ModelState.IsValid)
            {
                // flag the comment as anonymized if it was submitted to a sub which has active anonymized_mode
                var message = _db.Messages.Find(comment.MessageId);
                if (message != null && (message.Anonymized || message.Subverses.anonymized_mode))
                {
                    comment.Anonymized = true;
                }

                // check if author is banned, don't save the comment or send notifications if true
                if (!Utils.User.IsUserBanned(User.Identity.Name))
                {
                    _db.Comments.Add(comment);
                    await _db.SaveChangesAsync();

                    // send comment reply notification to parent comment author if the comment is not a new root comment
                    if (comment.ParentId != null && comment.CommentContent != null)
                    {
                        // find the parent comment and its author
                        var parentComment = _db.Comments.Find(comment.ParentId);
                        if (parentComment != null)
                        {
                            // check if recipient exists
                            if (Utils.User.UserExists(parentComment.Name))
                            {
                                // do not send notification if author is the same as comment author
                                if (parentComment.Name != User.Identity.Name)
                                {
                                    // send the message
                                    var commentReplyNotification = new Commentreplynotification();
                                    var commentMessage = _db.Messages.Find(comment.MessageId);
                                    if (commentMessage != null)
                                    {
                                        commentReplyNotification.CommentId = comment.Id;
                                        commentReplyNotification.SubmissionId = commentMessage.Id;
                                        commentReplyNotification.Recipient = parentComment.Name;
                                        if (parentComment.Message.Anonymized || parentComment.Message.Subverses.anonymized_mode)
                                        {
                                            commentReplyNotification.Sender = _rnd.Next(10000, 20000).ToString(CultureInfo.InvariantCulture);
                                        }
                                        else
                                        {
                                            commentReplyNotification.Sender = User.Identity.Name;
                                        }
                                        commentReplyNotification.Body = comment.CommentContent;
                                        commentReplyNotification.Subverse = commentMessage.Subverse;
                                        commentReplyNotification.Status = true;
                                        commentReplyNotification.Timestamp = DateTime.Now;

                                        // self = type 1, url = type 2
                                        commentReplyNotification.Subject = parentComment.Message.Type == 1 ? parentComment.Message.Title : parentComment.Message.Linkdescription;

                                        _db.Commentreplynotifications.Add(commentReplyNotification);

                                        await _db.SaveChangesAsync();
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
                        var commentMessage = _db.Messages.Find(comment.MessageId);
                        if (commentMessage != null)
                        {
                            // check if recipient exists
                            if (Utils.User.UserExists(commentMessage.Name))
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
                                        postReplyNotification.Sender = _rnd.Next(10000, 20000).ToString(CultureInfo.InvariantCulture);
                                    }
                                    else
                                    {
                                        postReplyNotification.Sender = User.Identity.Name;
                                    }

                                    postReplyNotification.Body = comment.CommentContent;
                                    postReplyNotification.Subverse = commentMessage.Subverse;
                                    postReplyNotification.Status = true;
                                    postReplyNotification.Timestamp = DateTime.Now;

                                    // self = type 1, url = type 2
                                    postReplyNotification.Subject = commentMessage.Type == 1 ? commentMessage.Title : commentMessage.Linkdescription;

                                    _db.Postreplynotifications.Add(postReplyNotification);

                                    await _db.SaveChangesAsync();
                                }
                            }
                        }
                        else
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                        }

                    }
                }
                
                if (Request.UrlReferrer != null)
                {
                    var url = Request.UrlReferrer.AbsolutePath;
                    return Redirect(url);
                }
            }
            if (Request.IsAjaxRequest())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ModelState.AddModelError(String.Empty, "Sorry, you are doing that too fast. Please try again in 2 minutes.");
            return View("~/Views/Help/SpeedyGonzales.cshtml");
        }

        // POST: editcomment
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [PreventSpam(DelayRequest = 120, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public async Task<ActionResult> EditComment([Bind(Include = "CommentId, CommentContent")] EditComment model)
        {
            if (!ModelState.IsValid) return Json("HTML is not allowed.", JsonRequestBehavior.AllowGet);

            var existingComment = _db.Comments.Find(model.CommentId);

            if (existingComment != null)
            {
                if (existingComment.Name.Trim() == User.Identity.Name)
                {
                    var escapedCommentContent = WebUtility.HtmlEncode(model.CommentContent);
                    existingComment.CommentContent = escapedCommentContent;
                    existingComment.LastEditDate = DateTime.Now;
                    await _db.SaveChangesAsync();

                    // parse the new comment through markdown formatter and then return the formatted comment so that it can replace the existing html comment which just got modified
                    var formattedComment = Formatting.FormatMessage(model.CommentContent);
                    return Json(new {response = formattedComment});
                }
                return Json("Unauthorized edit.", JsonRequestBehavior.AllowGet);
            }
            return Json("Unauthorized edit or comment not found.", JsonRequestBehavior.AllowGet);
        }

        // POST: deletecomment
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> DeleteComment(int commentId)
        {
            var commentToDelete = _db.Comments.Find(commentId);

            if (commentToDelete != null)
            {
                var commentSubverse = commentToDelete.Message.Subverse;

                // delete comment if the comment author is currently logged in user
                if (commentToDelete.Name == User.Identity.Name)
                {
                    commentToDelete.Name = "deleted";
                    commentToDelete.CommentContent = "deleted by author at " + DateTime.Now;
                    await _db.SaveChangesAsync();
                }
                // delete comment if delete request is issued by subverse moderator
                else if (Utils.User.IsUserSubverseAdmin(User.Identity.Name, commentSubverse) || Utils.User.IsUserSubverseModerator(User.Identity.Name, commentSubverse))
                {
                    // notify comment author that his comment has been deleted by a moderator
                    MesssagingUtility.SendPrivateMessage(
                        "Whoaverse",
                        commentToDelete.Name,
                        "Your comment has been deleted by a moderator",
                        "Your [comment](/v/" + commentSubverse + "/comments/" + commentToDelete.MessageId + "/" + commentToDelete.Id + ") has been deleted by: " +
                        "[" + User.Identity.Name + "](/u/" + User.Identity.Name + ")" + " on: " + DateTime.Now + "  " + Environment.NewLine +
                        "Original comment content was: " + Environment.NewLine +
                        "---" + Environment.NewLine +
                        commentToDelete.CommentContent
                        );

                    commentToDelete.Name = "deleted";
                    commentToDelete.CommentContent = "deleted by a moderator at " + DateTime.Now;
                    await _db.SaveChangesAsync();
                }
            }

            var url = Request.UrlReferrer.AbsolutePath;
            return Redirect(url);
        }
    }
}