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

using PagedList;
using Recaptcha.Web;
using Recaptcha.Web.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Whoaverse.Models;
using Whoaverse.Utils;

namespace Whoaverse.Controllers
{
    public class HomeController : Controller
    {
        private whoaverseEntities db = new whoaverseEntities();

        // GET: list of default subverses
        public ActionResult Listofsubverses()
        {
            try
            {
                var listOfSubverses = db.Defaultsubverses.OrderBy(s => s.position).ToList().AsEnumerable();
                return PartialView("_listofsubverses", listOfSubverses);
            }
            catch (Exception)
            {
                return PartialView("_ListofsubversesHeavyLoad");
            }
        }

        public ActionResult HeavyLoad()
        {
            return View("~/Views/Errors/DbNotResponding.cshtml");
        }

        // GET: list of subverses user moderates
        public ActionResult SubversesUserModerates(string userName)
        {
            if (userName != null)
            {
                return PartialView("~/Views/Shared/Userprofile/_SidebarSubsUserModerates.cshtml", db.SubverseAdmins
                .Where(x => x.Username == userName)
                .Select(s => new SelectListItem { Value = s.SubverseName })
                .OrderBy(s => s.Value).ToList().AsEnumerable());
            }
            else
            {
                return new EmptyResult();
            }
        }

        [HttpPost]
        [PreventSpam(DelayRequest = 300, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public ActionResult ClaSubmit(Cla claModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SmtpClient smtp = new SmtpClient();
                    MailAddress from = new MailAddress(claModel.Email);
                    MailAddress to = new MailAddress("legal@whoaverse.com");
                    StringBuilder sb = new StringBuilder();
                    MailMessage msg = new MailMessage(from, to);

                    msg.Subject = "New CLA Submission from " + claModel.FullName;
                    msg.IsBodyHtml = false;
                    smtp.Host = "whoaverse.com";
                    smtp.Port = 25;

                    //format CLA email
                    sb.Append("Full name: " + claModel.FullName);
                    sb.Append(Environment.NewLine);
                    sb.Append("Email: " + claModel.Email);
                    sb.Append(Environment.NewLine);
                    sb.Append("Mailing address: " + claModel.MailingAddress);
                    sb.Append(Environment.NewLine);
                    sb.Append("City: " + claModel.City);
                    sb.Append(Environment.NewLine);
                    sb.Append("Country: " + claModel.Country);
                    sb.Append(Environment.NewLine);
                    sb.Append("Phone number: " + claModel.PhoneNumber);
                    sb.Append(Environment.NewLine);
                    sb.Append("Corporate contributor information: " + claModel.CorpContrInfo);
                    sb.Append(Environment.NewLine);
                    sb.Append("Electronic signature: " + claModel.ElectronicSignature);
                    sb.Append(Environment.NewLine);

                    msg.Body = sb.ToString();

                    //send the email with CLA data
                    smtp.Send(msg);
                    msg.Dispose();
                    ViewBag.SelectedSubverse = string.Empty;
                    return View("~/Views/Legal/ClaSent.cshtml");
                }
                catch (Exception)
                {
                    ViewBag.SelectedSubverse = string.Empty;
                    return View("~/Views/Legal/ClaFailed.cshtml");
                }
            }
            else
            {
                return View();
            }
        }

        // GET: comments for a given submission
        public ActionResult Comments(int? id, string subversetoshow, int? startingcommentid, string sort)
        {
            string queryString = Request.QueryString["subversetoshow"];

            if (startingcommentid != null)
            {
                ViewBag.StartingCommentId = startingcommentid;
            }

            if (sort != null)
            {
                ViewBag.SortingMode = sort;
            }

            if (queryString != null)
            {
                ViewBag.SelectedSubverse = queryString;
            }
            else if (subversetoshow != null)
            {
                ViewBag.SelectedSubverse = subversetoshow;
            }
            else
            {
                return View("~/Views/Errors/Error.cshtml");
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

            return View(message);
        }

        // GET: submitcomment
        public ActionResult Submitcomment()
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
        public async Task<ActionResult> Submitcomment([Bind(Include = "Id,CommentContent,MessageId,ParentId")] Comment comment)
        {
            comment.Date = System.DateTime.Now;
            comment.Name = User.Identity.Name;
            comment.Votes = 0;
            comment.Likes = 0;

            if (ModelState.IsValid)
            {
                //TODO: check if user is shadowbanned and flag the comment
                db.Comments.Add(comment);
                await db.SaveChangesAsync();

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
                                    commentReplyNotification.Sender = User.Identity.Name;
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
                                postReplyNotification.Sender = User.Identity.Name;
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
                if (Request.IsAjaxRequest()){
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                ModelState.AddModelError(string.Empty, "Sorry, The subverse you are trying to post to does not exist.");
                return View("~/Views/Help/SpeedyGonzales.cshtml");
            }
        }

        // POST: editcomment
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        public ActionResult Editcomment(EditComment model)
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
                        "Your [comment](/v/"+commentSubverse+"/comments/"+commentToDelete.MessageId+"/"+commentToDelete.Id+") has been deleted by: " +
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

                    //parse the new submission through markdown formatter and then return the formatted submission so that it can replace the existing html submission which just got modified
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

        // GET: submit
        [Authorize]
        public ActionResult Submit(string selectedsubverse)
        {
            string linkPost = Request.Params["linkpost"];
            string linkDescription = Request.Params["linkdescription"];
            string linkUrl = Request.Params["linkurl"];

            if (linkPost != null)
            {
                if (linkPost == "true")
                {
                    ViewBag.action = "link";
                    ViewBag.linkDescription = linkDescription;
                    ViewBag.linkUrl = linkUrl;
                }
            }
            else
            {
                ViewBag.action = "discussion";
            }

            if (selectedsubverse != "all")
            {
                ViewBag.selectedSubverse = selectedsubverse;
            }

            return View();
        }

        // POST: submit
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [PreventSpam(DelayRequest = 60, ErrorMessage = "Sorry, you are doing that too fast. Please try again in 60 seconds.")]
        public async Task<ActionResult> Submit([Bind(Include = "Id,Votes,Name,Date,Type,Linkdescription,Title,Rank,MessageContent,Subverse")] Message message)
        {
            // verify recaptcha if user has less than 25 CCP
            if (Whoaverse.Utils.Karma.CommentKarma(User.Identity.Name) < 25)
            {
                RecaptchaVerificationHelper recaptchaHelper = this.GetRecaptchaVerificationHelper();

                if (String.IsNullOrEmpty(recaptchaHelper.Response))
                {
                    ModelState.AddModelError("", "Captcha answer cannot be empty.");
                    return View();
                }

                RecaptchaVerificationResult recaptchaResult = recaptchaHelper.VerifyRecaptchaResponse();

                if (recaptchaResult != RecaptchaVerificationResult.Success)
                {
                    ModelState.AddModelError("", "Incorrect captcha answer.");
                    return View();
                }
            }

            if (ModelState.IsValid)
            {
                // check if subverse exists
                var targetSubverse = db.Subverses.Find(message.Subverse.Trim());
                if (targetSubverse != null && message.Subverse != "all")
                {

                    // submission is a link post
                    // generate a thumbnail if submission is a direct link to image or video
                    if (message.Type == 2 && message.MessageContent != null && message.Linkdescription != null)
                    {

                        string domain = Whoaverse.Utils.UrlUtility.GetDomainFromUri(message.MessageContent);

                        // check if hostname is banned before accepting submission
                        if (Utils.BanningUtility.IsHostnameBanned(domain))
                        {
                            ModelState.AddModelError(string.Empty, "Sorry, the hostname you are trying to submit is banned.");
                            return View();
                        }

                        // if domain is youtube, try generating a thumbnail for the video
                        if (domain == "youtube.com")
                        {
                            try
                            {
                                string thumbFileName = ThumbGenerator.GenerateThumbFromYoutubeVideo(message.MessageContent);
                                message.Thumbnail = thumbFileName;
                            }
                            catch (Exception)
                            {
                                // thumnail generation failed, skip adding thumbnail
                            }
                        }
                        else
                        {
                            string extension = Path.GetExtension(message.MessageContent);

                            // this is a direct link to image
                            if (extension != String.Empty && extension != null)
                            {
                                if (extension == ".jpg" || extension == ".JPG" || extension == ".png" || extension == ".PNG" || extension == ".gif" || extension == ".GIF")
                                {
                                    try
                                    {
                                        string thumbFileName = ThumbGenerator.GenerateThumbFromUrl(message.MessageContent);
                                        message.Thumbnail = thumbFileName;
                                    }
                                    catch (Exception)
                                    {
                                        // thumnail generation failed, skip adding thumbnail
                                    }
                                }
                            }
                        }

                        message.Name = User.Identity.Name;
                        message.Subverse = targetSubverse.name;

                        // grab server timestamp and modify submission timestamp to have posting time instead of "started writing submission" time
                        message.Date = System.DateTime.Now;

                        message.Likes = 1;

                        // restrict incoming submissions to announcements subverse (temporary hard-code solution
                        // TODO: add global administrators table with different access levels
                        if (message.Subverse.Equals("announcements", StringComparison.OrdinalIgnoreCase) && User.Identity.Name == "Atko")
                        {
                            db.Messages.Add(message);
                            await db.SaveChangesAsync();
                        }
                        else if (!message.Subverse.Equals("announcements", StringComparison.OrdinalIgnoreCase))
                        {
                            db.Messages.Add(message);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Sorry, The subverse you are trying to post to is restricted.");
                            return View();
                        }

                    }
                    else if (message.Type == 1 && message.Title != null)
                    {
                        // submission is a self post
                        // trim trailing blanks from subverse name if a user mistakenly types them
                        message.Subverse = targetSubverse.name;
                        message.Name = User.Identity.Name;

                        // grab server timestamp and modify submission timestamp to have posting time instead of "started writing submission" time
                        message.Date = System.DateTime.Now;

                        message.Likes = 1;

                        // restrict incoming submissions to announcements subverse (temporary hard-code solution
                        // TODO: add global administrators table with different access levels
                        if (message.Subverse.Equals("announcements", StringComparison.OrdinalIgnoreCase) && User.Identity.Name == "Atko")
                        {
                            db.Messages.Add(message);
                            await db.SaveChangesAsync();
                        }
                        else if (!message.Subverse.Equals("announcements", StringComparison.OrdinalIgnoreCase))
                        {
                            db.Messages.Add(message);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Sorry, The subverse you are trying to post to is restricted.");
                            return View();
                        }
                    }

                    return RedirectToRoute(
                        "SubverseComments",
                        new
                        {
                            controller = "Home",
                            action = "Comments",
                            id = message.Id,
                            subversetoshow = message.Subverse
                        }
                    );
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Sorry, The subverse you are trying to post to does not exist.");
                    return View();
                }
            }
            else
            {
                return View();
            }

        }

        // GET: user/id
        public ActionResult UserProfile(string id, int? page, string whattodisplay)
        {
            ViewBag.SelectedSubverse = "user";
            ViewBag.whattodisplay = whattodisplay;
            ViewBag.userid = id;
            int pageSize = 25;
            int pageNumber = (page ?? 1);

            if (Whoaverse.Utils.User.UserExists(id) && id != "deleted")
            {
                //show comments
                if (whattodisplay != null && whattodisplay == "comments")
                {
                    var userComments = from c in db.Comments.OrderByDescending(c => c.Date)
                                       where c.Name.Equals(id)
                                       select c;
                    return View("UserComments", userComments.Take(200).ToPagedList(pageNumber, pageSize));
                }

                //show submissions                        
                if (whattodisplay != null && whattodisplay == "submissions")
                {
                    var userSubmissions = from b in db.Messages.OrderByDescending(s => s.Date)
                                          where b.Name.Equals(id)
                                          select b;
                    return View("UserSubmitted", userSubmissions.Take(200).ToPagedList(pageNumber, pageSize));
                }

                //default, show overview
                ViewBag.whattodisplay = "overview";

                var userDefaultSubmissions = from b in db.Messages.OrderByDescending(s => s.Date)
                                             where b.Name.Equals(id)
                                             select b;
                return View("UserProfile", userDefaultSubmissions.Take(200).ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }
        }

        // GET: /
        public ActionResult Index(int? page)
        {
            ViewBag.SelectedSubverse = "frontpage";

            int pageSize = 25;
            int pageNumber = (page ?? 1);

            try
            {
                // show only submissions from subverses that user is subscribed to if user is logged in
                // also do a check so that user actually has subscriptions
                if (User.Identity.IsAuthenticated && Whoaverse.Utils.User.SubscriptionCount(User.Identity.Name) > 0)
                {
                    var submissions = (from message in db.Messages
                                       join subscribedsubverses in db.Subscriptions on message.Subverse equals subscribedsubverses.SubverseName
                                       join ownsubscriptions in db.Subscriptions on subscribedsubverses.Username equals User.Identity.Name
                                       where message.Name != "deleted"
                                       select message)
                                       .Distinct()
                                       .OrderByDescending(s => s.Rank).Take(1000).ToList();

                    return View(submissions.ToPagedList(pageNumber, pageSize));
                }
                else
                {
                    //get only submissions from default subverses, order by rank
                    var submissions = (from message in db.Messages
                                       join defaultsubverse in db.Defaultsubverses on message.Subverse equals defaultsubverse.name
                                       where message.Name != "deleted"
                                       select message)
                                       .Distinct()
                                       .OrderByDescending(s => s.Rank).Take(1000).ToList();

                    return View(submissions.ToPagedList(pageNumber, pageSize));
                }
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Home");
            }
        }

        // GET: /new
        public ActionResult @New(int? page, string sortingmode)
        {
            // sortingmode: new, contraversial, hot, etc
            ViewBag.SortingMode = sortingmode;

            int pageSize = 25;
            int pageNumber = (page ?? 1);

            // setup a cookie to find first time visitors and display welcome banner
            string cookieName = "NotFirstTime";
            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
            {
                // not a first time visitor
                ViewBag.FirstTimeVisitor = false;
            }
            else
            {
                // add a cookie for first time visitors
                HttpCookie cookie = new HttpCookie(cookieName);
                cookie.Value = "whoaverse first time visitor identifier";
                cookie.Expires = DateTime.Now.AddMonths(6);
                this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                ViewBag.FirstTimeVisitor = true;
            }

            try
            {
                // show only submissions from subverses that user is subscribed to if user is logged in
                // also do a check so that user actually has subscriptions
                if (User.Identity.IsAuthenticated && Whoaverse.Utils.User.SubscriptionCount(User.Identity.Name) > 0)
                {
                    var submissions = (from message in db.Messages
                                       join subscribedsubverses in db.Subscriptions on message.Subverse equals subscribedsubverses.SubverseName
                                       join ownsubscriptions in db.Subscriptions on subscribedsubverses.Username equals User.Identity.Name
                                       where message.Name != "deleted"
                                       select message)
                                       .Distinct()
                                       .OrderByDescending(s => s.Date).Take(1000).ToList();

                    return View("Index", submissions.ToPagedList(pageNumber, pageSize));
                }
                else
                {
                    // get only submissions from default subverses, sort by date
                    var submissions = (from message in db.Messages
                                       join defaultsubverse in db.Defaultsubverses on message.Subverse equals defaultsubverse.name
                                       where message.Name != "deleted"
                                       select message)
                                       .Distinct()
                                       .OrderByDescending(s => s.Date).Take(1000).ToList();

                    return View("Index", submissions.ToPagedList(pageNumber, pageSize));
                }

            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Home");
            }

        }

        // GET: /about
        [OutputCache(VaryByParam = "none", Duration = 3600)]
        public ActionResult About(string pagetoshow)
        {
            ViewBag.SelectedSubverse = string.Empty;

            if (pagetoshow == "intro")
            {
                return View("~/Views/About/Intro.cshtml");
            }
            else if (pagetoshow == "contact")
            {
                return View("~/Views/About/Contact.cshtml");
            }
            else
            {
                return View("~/Views/About/About.cshtml");
            }
        }

        // GET: /cla
        [OutputCache(VaryByParam = "none", Duration = 3600)]
        public ActionResult Cla()
        {
            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.Message = "Whoaverse CLA";
            return View("~/Views/Legal/Cla.cshtml");
        }

        public ActionResult Welcome()
        {
            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Welcome/Welcome.cshtml");
        }

        // GET: /help
        [OutputCache(VaryByParam = "none", Duration = 3600)]
        public ActionResult Help(string pagetoshow)
        {
            ViewBag.SelectedSubverse = string.Empty;

            if (pagetoshow == "privacy")
            {
                return View("~/Views/Help/Privacy.cshtml");
            }
            if (pagetoshow == "useragreement")
            {
                return View("~/Views/Help/UserAgreement.cshtml");
            }
            if (pagetoshow == "markdown")
            {
                return View("~/Views/Help/Markdown.cshtml");
            }
            if (pagetoshow == "faq")
            {
                return View("~/Views/Help/Faq.cshtml");
            }
            else
            {
                return View("~/Views/Help/Index.cshtml");
            }
        }

        // GET: /help/privacy
        [OutputCache(VaryByParam = "none", Duration = 3600)]
        public ActionResult Privacy()
        {
            ViewBag.Message = "Privacy Policy";
            return View("~/Views/Help/Privacy.cshtml");
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

        [Authorize]
        public JsonResult Subscribe(string subverseName)
        {
            string loggedInUser = User.Identity.Name;

            Whoaverse.Utils.User.SubscribeToSubverse(loggedInUser, subverseName);
            return Json("Subscription request was successful.", JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult UnSubscribe(string subverseName)
        {
            string loggedInUser = User.Identity.Name;

            Whoaverse.Utils.User.UnSubscribeFromSubverse(loggedInUser, subverseName);
            return Json("Unsubscribe request was successful.", JsonRequestBehavior.AllowGet);
        }

        // GET: promoted submission
        public ActionResult PromotedSubmission()
        {
            var submissionId = db.Promotedsubmissions.FirstOrDefault();

            if (submissionId == null) return new EmptyResult();

            Message promotedSubmission = db.Messages.Find(submissionId.promoted_submission_id);

            if (promotedSubmission != null)
            {
                return PartialView("_Promoted", promotedSubmission);
            }
            else
            {
                //don't return a sidebar since subverse doesn't exist or is a system subverse
                return new EmptyResult();
            }
        }


        //EXPERIMENTAL FRONTPAGE RSS
        public ActionResult Rss()
        {
            var submissions = (from message in db.Messages
                               join defaultsubverse in db.Defaultsubverses on message.Subverse equals defaultsubverse.name
                               where message.Name != "deleted"
                               select message)
                                       .Distinct()
                                       .OrderByDescending(s => s.Rank).Take(25).ToList();

            SyndicationFeed feed = new SyndicationFeed("WhoaVerse", "The frontpage of the Universe", new Uri("http://www.whoaverse.com"));
            feed.Language = "en-US";
            feed.ImageUrl = new Uri("http://" + System.Web.HttpContext.Current.Request.Url.Authority + "/Graphics/whoaverse_padded.png");

            List<SyndicationItem> feedItems = new List<SyndicationItem>();

            foreach (var submission in submissions)
            {
                var commentsUrl = new Uri("http://" + System.Web.HttpContext.Current.Request.Url.Authority + "/v/" + submission.Subverse + "/comments/" + submission.Id);
                var subverseUrl = new Uri("http://" + System.Web.HttpContext.Current.Request.Url.Authority + "/v/" + submission.Subverse);
                
                string thumbnailUrl = "";

                if (submission.Type == 1)
                {
                    // message type submission
                    SyndicationItem item = new SyndicationItem(
                        submission.Title,
                        submission.MessageContent + "</br>" + "Submitted by " + "<a href='u/" + submission.Name + "'>" + submission.Name + "</a> to <a href='" + subverseUrl + "'>" + submission.Subverse + "</a> | <a href='" + commentsUrl + "'>" + submission.Comments.Count() + " comments",
                        commentsUrl,
                        "Item ID",
                        submission.Date);

                    feedItems.Add(item);
                }
                else
                {
                    // link type submission
                    var linkUrl = new Uri(submission.MessageContent);

                    // add a thumbnail if submission has one
                    if (submission.Thumbnail != null)
                    {
                        thumbnailUrl = new Uri("http://" + System.Web.HttpContext.Current.Request.Url.Authority + "/Thumbs/" + submission.Thumbnail).ToString();
                        SyndicationItem item = new SyndicationItem(
                                                submission.Linkdescription,
                                                "<a xmlns='http://www.w3.org/1999/xhtml' href='" + commentsUrl + "'><img title='"+submission.Linkdescription+"' alt='"+submission.Linkdescription+"' src='" + thumbnailUrl + "' /></a>"+
                                                "</br>" +
                                                "Submitted by " + "<a href='u/" + submission.Name + "'>" + submission.Name + "</a> to <a href='" + subverseUrl + "'>" + submission.Subverse + "</a> | <a href='" + commentsUrl + "'>" + submission.Comments.Count() + " comments</a>"+
                                                " | <a href='" + linkUrl + "'>link</a>",
                                                commentsUrl,
                                                "Item ID",
                                                submission.Date);                        

                        feedItems.Add(item);
                    }
                    else
                    {
                        SyndicationItem item = new SyndicationItem(
                                                submission.Linkdescription,
                                                "Submitted by " + "<a href='u/" + submission.Name + "'>" + submission.Name + "</a> to <a href='" + subverseUrl + "'>" + submission.Subverse + "</a> | <a href='" + commentsUrl + "'>" + submission.Comments.Count() + " comments",
                                                commentsUrl,
                                                "Item ID",
                                                submission.Date);
                        feedItems.Add(item);
                    }
                }
            }

            feed.Items = feedItems;
            return new FeedResult(new Rss20FeedFormatter(feed));
        }
    }
}