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
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Whoaverse.Utils;

namespace Whoaverse.Models
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
                    return View("~/Views/Legal/ClaSent.cshtml");
                }
                catch (Exception)
                {
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
        [Authorize]
        public ActionResult Submitcomment()
        {
            return View();
        }

        // POST: submitcomment
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [PreventSpam]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Submitcomment([Bind(Include = "Id,Votes,Name,Date,CommentContent,MessageId")] Comment comment)
        {
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

        // GET: submit
        [Authorize]
        public ActionResult Submit()
        {
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
                if (db.Subverses.Find(message.Subverse) != null)
                {
                    //check if username is admin and get random username instead
                    if (message.Name == "system")
                    {
                        message.Name = GrowthUtility.GetRandomUsername();
                        Random r = new Random();
                        int rInt = r.Next(6, 17);
                        message.Likes = (short)rInt;
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
                    ModelState.AddModelError("Subverse", "Sorry, The subverse you are trying to post to does not exist.");
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

        public ViewResult Index(int? page)
        {
            ViewBag.SelectedSubverse = "frontpage";
            int pageSize = 25;
            int pageNumber = (page ?? 1);
       
            //get only submissions from default subverses, order by rank
            var submissions = (from message in db.Messages
                     join defaultsubverse in db.Defaultsubverses on message.Subverse equals defaultsubverse.name                     
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
                               select message).OrderByDescending(s => s.Date).ToList();

            return View("Index", submissions.ToPagedList(pageNumber, pageSize));
        }        

        public ActionResult About(string pagetoshow)
        {
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
            ViewBag.Message = "Whoaverse CLA";
            return View("~/Views/Legal/Cla.cshtml");
        }

        public ActionResult Help(string pagetoshow)
        {
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
            if (User.Identity.IsAuthenticated)
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
            }
            return Json("Voting Ok", JsonRequestBehavior.AllowGet);
        }

    }
}