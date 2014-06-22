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
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whoaverse.Models;
using Whoaverse.Utils;

namespace Whoaverse.Controllers
{
    public class HomeController : Controller
    {
        private whoaverseEntities db = new whoaverseEntities();

        // GET: list of subverses
        public ActionResult Listofsubverses()
        {
            return PartialView("_listofsubverses", db.Defaultsubverses.OrderBy(s => s.position).ToList().AsEnumerable());
        }

        [HttpPost]
        [PreventSpam]
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
        public ActionResult Comments(int? id, string subversetoshow)
        {
            ViewBag.SelectedSubverse = subversetoshow;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
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

        // POST: submitcomment
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [PreventSpam]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Submitcomment([Bind(Include = "Id,CommentContent,MessageId,ParentId")] Comment comment)
        {
            comment.Date = System.DateTime.Now;
            comment.Name = User.Identity.Name;
            comment.Votes = 1;
            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                await db.SaveChangesAsync();

                //get newly generated Comment ID and execute ranking and self upvoting                
                Commentvotingtracker tmpVotingTracker = new Commentvotingtracker();
                tmpVotingTracker.CommentId = comment.Id;
                tmpVotingTracker.UserName = comment.Name;
                tmpVotingTracker.VoteStatus = 1;
                db.Commentvotingtrackers.Add(tmpVotingTracker);
                await db.SaveChangesAsync();

                string url = this.Request.UrlReferrer.AbsolutePath;
                return Redirect(url);
            }
            else
            {
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
                var subverseOwner = db.SubverseAdmins.Where(n => n.SubverseName == commentToDelete.Message.Subverse && n.Power == 1).FirstOrDefault();

                // delete comment if the comment author is currently logged in user
                if (commentToDelete.Name == User.Identity.Name)
                {
                    commentToDelete.Name = "deleted";
                    commentToDelete.CommentContent = "deleted";
                    await db.SaveChangesAsync();
                }
                // delete comment if delete request is issued by subverse moderator
                else if (subverseOwner != null && subverseOwner.Username == User.Identity.Name)
                {
                    commentToDelete.Name = "deleted";
                    commentToDelete.CommentContent = "deleted by a moderator";
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
                var subverseOwner = db.SubverseAdmins.Where(n => n.SubverseName == submissionToDelete.Subverse && n.Power == 1).FirstOrDefault();

                if (submissionToDelete.Name == User.Identity.Name)
                {
                    submissionToDelete.Name = "deleted";

                    if (submissionToDelete.Type == 1)
                    {
                        submissionToDelete.MessageContent = "deleted";
                    }
                    else
                    {
                        submissionToDelete.MessageContent = "http://whoaverse.com";
                    }                       

                    await db.SaveChangesAsync();
                }
                // delete submission if delete request is issued by subverse moderator
                else if (submissionToDelete != null && subverseOwner.Username == User.Identity.Name || submissionToDelete != null && Whoaverse.Utils.User.IsUserSubverseModerator(User.Identity.Name, submissionToDelete.Subverse))
                {
                    submissionToDelete.Name = "deleted";

                    if (submissionToDelete.Type == 1)
                    {
                        submissionToDelete.MessageContent = "deleted by a moderator";
                    }
                    else
                    {
                        submissionToDelete.MessageContent = "http://whoaverse.com";
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
        [PreventSpam(DelayRequest = 300, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public async Task<ActionResult> Submit([Bind(Include = "Id,Votes,Name,Date,Type,Linkdescription,Title,Rank,MessageContent,Subverse")] Message message)
        {           
            if (ModelState.IsValid)
            {
                //check if subverse exists
                if (db.Subverses.Find(message.Subverse) != null && message.Subverse != "all")
                {
                    //check if username is admin and get random username instead
                    if (message.Name == "system")
                    {
                        message.Name = GrowthUtility.GetRandomUsername();
                        Random r = new Random();
                        int rInt = r.Next(6, 17);
                        message.Likes = (short)rInt;
                    }

                    //generate a thumbnail if submission is a link submission and a direct link to image
                    if (message.Type == 2)
                    {
                        try
                        {
                            string domain = Whoaverse.Utils.UrlUtility.GetDomainFromUri(message.MessageContent);

                            //if domain is youtube, try generating a thumbnail for the video
                            if (domain == "youtube.com")
                            {
                                string thumbFileName = ThumbGenerator.GenerateThumbFromYoutubeVideo(message.MessageContent);
                                message.Thumbnail = thumbFileName;
                            }
                            else
                            {
                                string extension = Path.GetExtension(message.MessageContent);

                                if (extension != String.Empty && extension != null)
                                {
                                    if (extension == ".jpg" || extension == ".JPG" || extension == ".png" || extension == ".PNG" || extension == ".gif" || extension == ".GIF")
                                    {
                                        string thumbFileName = ThumbGenerator.GenerateThumbFromUrl(message.MessageContent);
                                        message.Thumbnail = thumbFileName;
                                    }
                                }
                            }

                        }
                        catch (Exception)
                        {
                            //unable to generate a thumbnail, don't use any
                        }
                    }

                    db.Messages.Add(message);
                    await db.SaveChangesAsync();

                    //get newly generated message ID and execute ranking and self upvoting                
                    Votingtracker tmpVotingTracker = new Votingtracker();
                    tmpVotingTracker.MessageId = message.Id;
                    tmpVotingTracker.UserName = message.Name;
                    tmpVotingTracker.VoteStatus = 1;
                    db.Votingtrackers.Add(tmpVotingTracker);
                    await db.SaveChangesAsync();

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
                    return View("usercomments", userComments.ToPagedList(pageNumber, pageSize));
                }

                //show submissions                        
                if (whattodisplay != null && whattodisplay == "submissions")
                {
                    var userSubmissions = from b in db.Messages.OrderByDescending(s => s.Date)
                                          where b.Name.Equals(id)
                                          select b;
                    return View(userSubmissions.ToPagedList(pageNumber, pageSize));
                }

                //default, show overview
                var userDefaultSubmissions = from b in db.Messages.OrderByDescending(s => s.Date)
                                             where b.Name.Equals(id)
                                             select b;
                return View(userDefaultSubmissions.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

        }

        public ViewResult Index(int? page)
        {
            ViewBag.SelectedSubverse = "frontpage";

            int pageSize = 25;
            int pageNumber = (page ?? 1);

            //get only submissions from default subverses, order by rank
            var submissions = (from message in db.Messages
                               join defaultsubverse in db.Defaultsubverses on message.Subverse equals defaultsubverse.name
                               where message.Name != "deleted"
                               select message).OrderByDescending(s => s.Rank).ToList();

            return View(submissions.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult @New(int? page, string sortingmode)
        {
            //sortingmode: new, contraversial, hot, etc
            ViewBag.SortingMode = sortingmode;

            int pageSize = 25;
            int pageNumber = (page ?? 1);

            //get only submissions from default subverses, sort by date
            var submissions = (from message in db.Messages
                               join defaultsubverse in db.Defaultsubverses on message.Subverse equals defaultsubverse.name
                               where message.Name != "deleted"
                               select message).OrderByDescending(s => s.Date).ToList();

            //setup a cookie to find first time visitors and display welcome banner
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
                this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                ViewBag.FirstTimeVisitor = true;
            }

            return View("Index", submissions.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult About(string pagetoshow)
        {
            ViewBag.SelectedSubverse = string.Empty;

            if (pagetoshow == "team")
            {
                return View("~/Views/About/Team.cshtml");
            }
            else if (pagetoshow == "intro")
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

        public ActionResult Cla()
        {
            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.Message = "Whoaverse CLA";
            return View("~/Views/Legal/Cla.cshtml");
        }

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

        public ActionResult Privacy()
        {
            ViewBag.Message = "Privacy Policy";
            return View("~/Views/Help/Privacy.cshtml");
        }

        [Authorize]
        public JsonResult Vote(int messageId, int typeOfVote)
        {
            string loggedInUser = User.Identity.Name;

            if (typeOfVote == 1)
            {
                //perform upvoting or resetting
                Voting.UpvoteSubmission(messageId, loggedInUser);
            }
            else if (typeOfVote == -1)
            {
                //perform downvoting or resetting
                Voting.DownvoteSubmission(messageId, loggedInUser);
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
        
    }
}