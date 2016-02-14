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

//using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Voat.Configuration;
using Voat.Data.Models;
using Voat.Models;
using Voat.UI.Utilities;
using Voat.Utilities;
using Voat.Utilities.Components;

namespace Voat.Controllers
{
    public class CommentController : Controller
    {
        private readonly voatEntities _db = new voatEntities();
        private static readonly object _locker = new object();

        // POST: votecomment/{commentId}/{typeOfVote}
        [Authorize]
        public JsonResult VoteComment(int commentId, int typeOfVote)
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
                                VotingComments.UpvoteComment(commentId, loggedInUser, IpHash.CreateHash(UserHelper.UserIpAddress(Request)));
                            }
                        }
                        else if (totalVotesUsedInPast24Hours < 11)
                        {
                            // perform upvoting or resetting even if user has no CCP but only allow 10 votes per 24 hours
                            VotingComments.UpvoteComment(commentId, loggedInUser, IpHash.CreateHash(UserHelper.UserIpAddress(Request)));
                        }
                        break;
                    case -1:
                        if (userCcp >= 100)
                        {
                            if (totalVotesUsedInPast24Hours < scaledDailyVotingQuota)
                            {
                                // perform downvoting or resetting
                                VotingComments.DownvoteComment(commentId, loggedInUser, IpHash.CreateHash(UserHelper.UserIpAddress(Request)));
                            }
                        }
                        break;
                }

                Response.StatusCode = 200;
                return Json("Voting ok", JsonRequestBehavior.AllowGet);
            }
        }

        // POST: savecomment/{commentId}
        [Authorize]
        public JsonResult SaveComment(int commentId)
        {
            var loggedInUser = User.Identity.Name;
            // perform saving or unsaving
            SavingComments.SaveComment(commentId, loggedInUser);

            Response.StatusCode = 200;
            return Json("Saving ok", JsonRequestBehavior.AllowGet);
        }
        
        private List<CommentVoteTracker> UserCommentVotesBySubmission(int submissionID) {
            List<CommentVoteTracker> vCache = new List<CommentVoteTracker>();

            if (User.Identity.IsAuthenticated){
                vCache = (from cv in _db.CommentVoteTrackers.AsNoTracking()
                            join c in _db.Comments on cv.CommentID equals c.ID
                            where c.SubmissionID == submissionID && cv.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)
                            select cv).ToList();    
            }
            return vCache;
        }
        
        private List<CommentSaveTracker> UserSavedCommentsBySubmission(int submissionID)
        {
            List<CommentSaveTracker> vCache = new List<CommentSaveTracker>();

            if (User.Identity.IsAuthenticated)
            {
                vCache = (from cv in _db.CommentSaveTrackers.AsNoTracking()
                          join c in _db.Comments on cv.CommentID equals c.ID
                          where c.SubmissionID == submissionID && cv.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) && !c.IsDeleted
                          select cv).ToList();
            }
            return vCache;
        }
        
        // GET: comments for a given submission
        public ActionResult Comments(int? id, string subversetoshow, int? startingcommentid, string sort, int? commentToHighLight)
        {
            #region Validation 
            
            if (id == null)
            {
                return View("~/Views/Errors/Error.cshtml");
            }

            var submission = _db.Submissions.Find(id.Value);

            if (submission == null)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // make sure that the combination of selected subverse and submission subverse are linked
            if (!submission.Subverse.Equals(subversetoshow, StringComparison.OrdinalIgnoreCase))
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            var subverse = DataCache.Subverse.Retrieve(subversetoshow);
            //var subverse = _db.Subverse.Find(subversetoshow);

            if (subverse == null)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            //HACK: Disable subverse
            if (subverse.IsAdminDisabled.HasValue && subverse.IsAdminDisabled.Value)
            {
                ViewBag.Subverse = subverse.Name;
                return View("~/Views/Errors/SubverseDisabled.cshtml");
            }

            #endregion 

            ViewBag.SelectedSubverse = subverse.Name;
            ViewBag.SubverseAnonymized = subverse.IsAnonymized;

            //Temp cache user votes for this thread
            ViewBag.VoteCache = UserCommentVotesBySubmission(id.Value);
            ViewBag.SavedCommentCache = UserSavedCommentsBySubmission(id.Value);
            ViewBag.CCP = Karma.CommentKarma(User.Identity.Name);

            if (startingcommentid != null)
            {
                ViewBag.StartingCommentId = startingcommentid;
            }

            if (commentToHighLight != null)
            {
                ViewBag.CommentToHighLight = commentToHighLight;
            }

            var SortingMode = (sort == null ? "top" : sort).ToLower();
            ViewBag.SortingMode = SortingMode;

          

            // experimental: register a new session for this subverse
            string clientIpAddress = String.Empty;

            if (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            {
                clientIpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            }
            else if (Request.UserHostAddress.Length != 0)
            {
                clientIpAddress = Request.UserHostAddress;
            }

            if (clientIpAddress != String.Empty)
            {
                // generate salted hash of client IP address
                string ipHash = IpHash.CreateHash(clientIpAddress);

                var currentSubverse = (string)RouteData.Values["subversetoshow"];

                // register a new session for this subverse
                SessionHelper.Add(currentSubverse, ipHash);

                // register a new view for this thread
                // check if this hash is present for this submission id in viewstatistics table
                var existingView = _db.ViewStatistics.Find(submission.ID, ipHash);

                // this IP has already viwed this thread, skip registering a new view
                if (existingView == null)
                {
                    // this is a new view, register it for this submission
                    var view = new ViewStatistic { SubmissionID = submission.ID, ViewerID = ipHash };
                    _db.ViewStatistics.Add(view);

                    submission.Views++;

                    _db.SaveChanges();
                }
            }

            var commentTree = DataCache.CommentTree.Retrieve<usp_CommentTree_Result>(submission.ID, null, null);

            var model = new CommentBucketViewModel()
            {
                StartingIndex = 0,
                EndingIndex = 5,
                Subverse = subverse,
                Submission = submission,
                CommentTree = commentTree,
                //DisplayTree = displayTree,
                ParentID = null,
                Sort = (CommentSort)Enum.Parse(typeof(CommentSort), SortingMode, true)
            };

            IQueryable<usp_CommentTree_Result> displayTree = commentTree.AsQueryable().Where(x => x.ParentID == null);
            model.TotalInDisplayBranch = displayTree.Count();

            if (model.Sort == CommentSort.Top) {
                displayTree = displayTree.OrderByDescending(x => x.UpCount - x.DownCount).Take(model.EndingIndex);
            } else {
                displayTree = displayTree.OrderByDescending(x => x.CreationDate).Take(model.EndingIndex);
            }
            model.DisplayTree = displayTree;
           

            return View("~/Views/Home/Comments.cshtml", model);
        }
        
        // url: "/comments/" + submission + "/" + parentId + "/" + command + "/" + startingIndex + "/" + count + "/" + nestingLevel + "/" + sort + "/",
        // GET: comments for a given submission
        public ActionResult BucketOfComments(int submissionId, int? parentId, string command, int startingIndex, string sort)
        {
            #region Validation

            if (submissionId <= 0)
            {
                return View("~/Views/Errors/Error.cshtml");
            }

            var submission = DataCache.Submission.Retrieve(submissionId);

            if (submission == null)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            var subverse = DataCache.Subverse.Retrieve(submission.Subverse);
            //var subverse = _db.Subverse.Find(subversetoshow);

            if (subverse == null)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            //HACK: Disable subverse
            if (subverse.IsAdminDisabled.HasValue && subverse.IsAdminDisabled.Value)
            {
                ViewBag.Subverse = subverse.Name;
                return View("~/Views/Errors/SubverseDisabled.cshtml");
            }

            #endregion
            
            ViewBag.SelectedSubverse = subverse.Name;
            ViewBag.SubverseAnonymized = subverse.IsAnonymized;

            //Temp cache user votes for this thread
            ViewBag.VoteCache = UserCommentVotesBySubmission(submissionId);
            ViewBag.SavedCommentCache = UserSavedCommentsBySubmission(submissionId);
            ViewBag.CCP = Karma.CommentKarma(User.Identity.Name);


            var SortingMode = (sort == null ? "top" : sort).ToLower();
            ViewBag.SortingMode = SortingMode;



            var commentTree = DataCache.CommentTree.Retrieve<usp_CommentTree_Result>(submission.ID, null, null);
            var model = new CommentBucketViewModel()
            {
                StartingIndex = startingIndex,
                //NestingThreshold = nestingLevel,
                Subverse = subverse,
                Submission = submission,
                CommentTree = commentTree,
                ParentID = parentId,
                Sort = (CommentSort)Enum.Parse(typeof(CommentSort), SortingMode, true)
            };
            model.CollapseSiblingThreshold = 5;
           
            IQueryable<usp_CommentTree_Result> displayTree = commentTree.AsQueryable();
            displayTree = displayTree.Where(x => x.ParentID == parentId);
            model.TotalInDisplayBranch = displayTree.Count();

            //calculate offsets
            model.EndingIndex = Math.Min(model.StartingIndex + model.CollapseSiblingThreshold, model.TotalInDisplayBranch);


            if (model.Sort == CommentSort.Top){
                displayTree = displayTree.OrderByDescending(x => x.UpCount - x.DownCount);
            } else {
                displayTree = displayTree.OrderByDescending(x => x.CreationDate);
            }

            displayTree = displayTree.Skip(model.StartingIndex).Take(model.Count); 

            model.DisplayTree = displayTree;

            return PartialView("~/Views/Shared/Comments/_CommentBucket.cshtml", model);
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
        [PreventSpam(DelayRequest = 30, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitComment([Bind(Include = "ID, Content, SubmissionID, ParentID")] Comment commentModel)
        {
            commentModel.CreationDate = DateTime.Now;
            commentModel.UserName = User.Identity.Name;
            commentModel.Votes = 0;
            commentModel.UpCount = 0;

            if (ModelState.IsValid)
            {
                // flag the comment as anonymized if it was submitted to a sub which has active anonymized_mode
                var submission = DataCache.Submission.Retrieve(commentModel.SubmissionID.Value);
                var subverse = DataCache.Subverse.Retrieve(submission.Subverse);
                var userCcp = Karma.CommentKarma(User.Identity.Name);
                commentModel.IsAnonymized = submission.IsAnonymized || subverse.IsAnonymized;
   
                // if user CCP is negative and account less than 6 months old, allow only x comment submissions per 24 hours
                var userRegistrationDate = UserHelper.GetUserRegistrationDateTime(User.Identity.Name);
                TimeSpan userMembershipTimeSpam = DateTime.Now - userRegistrationDate;
                if (userMembershipTimeSpam.TotalDays < 180 && userCcp < 1)
                {
                    var quotaUsed = UserHelper.UserDailyCommentPostingQuotaForNegativeScoreUsed(User.Identity.Name);
                    if (quotaUsed)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You have reached your daily comment quota. Your current quota is " + Settings.DailyCommentPostingQuotaForNegativeScore.ToString() + " comment(s) per 24 hours.");
                    }
                }

                // if user CCP is < 50, allow only X comment submissions per 24 hours
                if (userCcp <= -50)
                {
                    var quotaUsed = UserHelper.UserDailyCommentPostingQuotaForNegativeScoreUsed(User.Identity.Name);
                    if (quotaUsed)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You have reached your daily comment quota. Your current quota is " + Settings.DailyCommentPostingQuotaForNegativeScore.ToString() + " comment(s) per 24 hours.");
                    }
                }

                // check if author is banned, don't save the comment or send notifications if true
                if (!UserHelper.IsUserGloballyBanned(User.Identity.Name) && !UserHelper.IsUserBannedFromSubverse(User.Identity.Name, submission.Subverse))
                {
                    bool containsBannedDomain = BanningUtility.ContentContainsBannedDomain(subverse.Name, commentModel.Content);
                    if (containsBannedDomain)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Comment contains links to banned domain(s).");
                    }


                    if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPreSave))
                    {
                        commentModel.Content = ContentProcessor.Instance.Process(commentModel.Content, ProcessingStage.InboundPreSave, commentModel);
                    }

                    //save fully formatted content 
                    var formattedComment = Formatting.FormatMessage(commentModel.Content);
                    commentModel.FormattedContent = formattedComment;
                    
                    _db.Comments.Add(commentModel);

                    await _db.SaveChangesAsync();

                    DataCache.CommentTree.AddCommentToTree(commentModel);

                    if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave))
                    {
                        ContentProcessor.Instance.Process(commentModel.Content, ProcessingStage.InboundPostSave, commentModel);
                    }

                    // send comment reply notification to parent comment author if the comment is not a new root comment
                    await NotificationManager.SendCommentNotification(commentModel, 
                        new Action<string>(recipient => {
                            //get count of unread notifications
                            int unreadNotifications = UserHelper.UnreadTotalNotificationsCount(recipient);
                            // send SignalR realtime notification to recipient
                            var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();
                            hubContext.Clients.User(recipient).setNotificationsPending(unreadNotifications);
                        })
                    );
                }
                if (Request.IsAjaxRequest()) 
                {
                    var comment = commentModel;

                    ViewBag.CommentId = comment.ID; //why?
                    ViewBag.rootComment = comment.ParentID == null; //why?

                    if (submission.IsAnonymized || subverse.IsAnonymized)
                    {
                        comment.UserName = comment.ID.ToString(CultureInfo.InvariantCulture);
                    }

                    var model = new CommentBucketViewModel(comment);

                    return PartialView("~/Views/Shared/Submissions/_SubmissionComment.cshtml", model);
                    //return new HttpStatusCodeResult(HttpStatusCode.OK);
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

            ModelState.AddModelError(String.Empty, "Sorry, you are either banned from this sub or doing that too fast. Please try again in 2 minutes.");
            return View("~/Views/Help/SpeedyGonzales.cshtml");
        }

        // POST: editcomment
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [PreventSpam(DelayRequest = 15, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public async Task<ActionResult> EditComment([Bind(Include = "ID, Content")] Comment commentModel)
        {
            if (ModelState.IsValid)
            {
                var existingComment = _db.Comments.Find(commentModel.ID);

                if (existingComment != null)
                {
                    if (existingComment.UserName.Trim() == User.Identity.Name && !existingComment.IsDeleted)
                    {

                        bool containsBannedDomain = BanningUtility.ContentContainsBannedDomain(existingComment.Submission.Subverse, commentModel.Content);
                        if (containsBannedDomain)
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Comment contains links to banned domain(s).");
                        }

                        existingComment.LastEditDate = DateTime.Now;
                        existingComment.Content = commentModel.Content;

                        if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPreSave))
                        {
                            existingComment.Content = ContentProcessor.Instance.Process(existingComment.Content, ProcessingStage.InboundPreSave, existingComment);
                        }

                        //save fully formatted content 
                        var formattedComment = Formatting.FormatMessage(existingComment.Content);
                        existingComment.FormattedContent = formattedComment;

                        await _db.SaveChangesAsync();

                        if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave))
                        {
                            ContentProcessor.Instance.Process(existingComment.Content, ProcessingStage.InboundPostSave, existingComment);
                        }

                        //return the formatted comment so that it can replace the existing html comment which just got modified
                        return Json(new { response = formattedComment });
                    }
                    return Json("Unauthorized edit.", JsonRequestBehavior.AllowGet);
                }
            }

            if (Request.IsAjaxRequest())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return Json("Unauthorized edit or comment not found - comment ID was.", JsonRequestBehavior.AllowGet);
        }

        // POST: deletecomment
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> DeleteComment(int commentId)
        {
            var commentToDelete = _db.Comments.Find(commentId);

            if (commentToDelete != null && !commentToDelete.IsDeleted)
            {
                var commentSubverse = commentToDelete.Submission.Subverse;

                // delete comment if the comment author is currently logged in user
                if (commentToDelete.UserName == User.Identity.Name)
                {
                    commentToDelete.IsDeleted = true;
                    commentToDelete.Content = "deleted by author at " + DateTime.Now;
                    await _db.SaveChangesAsync();
                }

                // delete comment if delete request is issued by subverse moderator
                else if (UserHelper.IsUserSubverseModerator(User.Identity.Name, commentSubverse))
                {
                    // notify comment author that his comment has been deleted by a moderator
                    MesssagingUtility.SendPrivateMessage(
                        "Voat",
                        commentToDelete.UserName,
                        "Your comment has been deleted by a moderator",
                        "Your [comment](/v/" + commentSubverse + "/comments/" + commentToDelete.SubmissionID + "/" + commentToDelete.ID + ") has been deleted by: " +
                        "/u/" + User.Identity.Name + " on: " + DateTime.Now + "  " + Environment.NewLine +
                        "Original comment content was: " + Environment.NewLine +
                        "---" + Environment.NewLine +
                        commentToDelete.Content
                        );

                    commentToDelete.IsDeleted = true;

                    // move the comment to removal log
                    var removalLog = new CommentRemovalLog
                    {
                        CommentID = commentToDelete.ID,
                        Moderator = User.Identity.Name,
                        Reason = "This feature is not yet implemented",
                        CreationDate = DateTime.Now
                    };

                    _db.CommentRemovalLogs.Add(removalLog);

                    commentToDelete.Content = "deleted by a moderator at " + DateTime.Now;
                    await _db.SaveChangesAsync();
                }
            }
            if (Request.IsAjaxRequest()) 
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            var url = Request.UrlReferrer.AbsolutePath;
            return Redirect(url);
        }

        // POST: comments/distinguish/{commentId}
        [Authorize]
        public JsonResult DistinguishComment(int commentId)
        {
            var commentToDistinguish = _db.Comments.Find(commentId);

            if (commentToDistinguish != null)
            {
                // check to see if request came from comment author
                if (User.Identity.Name == commentToDistinguish.UserName)
                {
                    // check to see if comment author is also sub mod or sub admin for comment sub
                    if (UserHelper.IsUserSubverseModerator(User.Identity.Name, commentToDistinguish.Submission.Subverse))
                    {
                        // mark the comment as distinguished and save to db
                        if (commentToDistinguish.IsDistinguished)
                        {
                            commentToDistinguish.IsDistinguished = false;
                        }
                        else
                        {
                            commentToDistinguish.IsDistinguished = true;
                        }

                        _db.SaveChangesAsync();

                        Response.StatusCode = 200;
                        return Json("Distinguish flag changed.", JsonRequestBehavior.AllowGet);
                    }
                }
            }

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json("Unauthorized distinguish attempt.", JsonRequestBehavior.AllowGet);
        }
    }
}