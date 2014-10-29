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

using OpenGraph_Net;
using PagedList;
using System;
using System.IO;
using System.Linq;
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
        private readonly whoaverseEntities _db = new whoaverseEntities();

        [HttpPost]
        [PreventSpam(DelayRequest = 300, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public ActionResult ClaSubmit(Cla claModel)
        {
            if (!ModelState.IsValid) return View();

            var from = new MailAddress(claModel.Email);
            var to = new MailAddress("legal@whoaverse.com");
            var sb = new StringBuilder();
            var msg = new MailMessage(@from, to) {Subject = "New CLA Submission from " + claModel.FullName};

            // format CLA email
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

            // send the email with CLA data
            if (EmailUtility.SendEmail(msg))
            {
                msg.Dispose();
                ViewBag.SelectedSubverse = string.Empty;
                return View("~/Views/Legal/ClaSent.cshtml");
            }
            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Legal/ClaFailed.cshtml");
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

            if (selectedsubverse != null)
            {
                if (!selectedsubverse.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    ViewBag.selectedSubverse = selectedsubverse;
                }
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
            // check if user is banned
            if (Utils.User.IsUserBanned(message.Name))
            {
                ViewBag.SelectedSubverse = message.Subverse;
                return View("~/Views/Home/Comments.cshtml", message);
            }

            // verify recaptcha if user has less than 25 CCP
            if (Karma.CommentKarma(User.Identity.Name) < 25)
            {
                // begin recaptcha check
                const string captchaMessage = "";
                var isCaptchaCodeValid = ReCaptchaUtility.GetCaptchaResponse(captchaMessage, Request);

                if (!isCaptchaCodeValid)
                {
                    ModelState.AddModelError("", "Incorrect recaptcha answer.");
                    return View();
                }
                // end recaptcha check
            }

            if (!ModelState.IsValid) return View();
            // check if subverse exists
            var targetSubverse = _db.Subverses.Find(message.Subverse.Trim());
            if (targetSubverse != null && !message.Subverse.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                // check if subverse has "authorized_submitters_only" set and dissalow submission if user is not allowed submitter
                if (targetSubverse.authorized_submitters_only)
                {
                    if (!Utils.User.IsUserSubverseModerator(User.Identity.Name, targetSubverse.name))
                    {
                        // user is not a moderator, check if user is an administrator
                        if (!Utils.User.IsUserSubverseAdmin(User.Identity.Name, targetSubverse.name))
                        {
                            ModelState.AddModelError("", "You are not authorized to submit links or start discussions in this subverse. Please contact subverse moderators for authorization.");
                            return View();
                        }
                    }
                }

                // submission is a link post
                // generate a thumbnail if submission is a direct link to image or video
                if (message.Type == 2 && message.MessageContent != null && message.Linkdescription != null)
                {
                    var domain = UrlUtility.GetDomainFromUri(message.MessageContent);

                    // check if hostname is banned before accepting submission
                    if (BanningUtility.IsHostnameBanned(domain))
                    {
                        ModelState.AddModelError(string.Empty, "Sorry, the hostname you are trying to submit is banned.");
                        return View();
                    }

                    // check if target subverse has thumbnails setting enabled before generating a thumbnail
                    if (targetSubverse.enable_thumbnails)
                    {
                        var extension = Path.GetExtension(message.MessageContent);

                        // this is a direct link to image
                        if (extension != String.Empty)
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
                            else
                            {
                                // try generating a thumbnail by using the Open Graph Protocol
                                try
                                {
                                    OpenGraph graph = OpenGraph.ParseUrl(message.MessageContent);
                                    if (graph.Image != null)
                                    {
                                        string thumbFileName = ThumbGenerator.GenerateThumbFromUrl(graph.Image.ToString());
                                        message.Thumbnail = thumbFileName;
                                    }
                                }
                                catch (Exception)
                                {
                                    // thumnail generation failed, skip adding thumbnail
                                }
                            }
                        }
                        else
                        {
                            // try generating a thumbnail by using the Open Graph Protocol
                            try
                            {
                                var graph = OpenGraph.ParseUrl(message.MessageContent);
                                if (graph.Image != null)
                                {
                                    string thumbFileName = ThumbGenerator.GenerateThumbFromUrl(graph.Image.ToString());
                                    message.Thumbnail = thumbFileName;
                                }
                            }
                            catch (Exception)
                            {
                                // thumnail generation failed, skip adding thumbnail
                            }
                        }
                    }

                    // flag the submission as anonymized if it was submitted to a subverse with active anonymized_mode
                    if (targetSubverse.anonymized_mode)
                    {
                        message.Anonymized = true;
                    }
                    else
                    {
                        message.Name = User.Identity.Name;
                    }

                    // accept submission and save it to the database
                    message.Subverse = targetSubverse.name;
                    // grab server timestamp and modify submission timestamp to have posting time instead of "started writing submission" time
                    message.Date = DateTime.Now;
                    message.Likes = 1;
                    _db.Messages.Add(message);
                    await _db.SaveChangesAsync();

                }
                else if (message.Type == 1 && message.Title != null)
                {
                    // submission is a self post
                    // accept submission and save it to the database
                    // trim trailing blanks from subverse name if a user mistakenly types them
                    message.Subverse = targetSubverse.name;
                    // flag the submission as anonymized if it was submitted to a subverse with active anonymized_mode
                    if (targetSubverse.anonymized_mode)
                    {
                        message.Anonymized = true;
                    }
                    else
                    {
                        message.Name = User.Identity.Name;
                    }
                    // grab server timestamp and modify submission timestamp to have posting time instead of "started writing submission" time
                    message.Date = DateTime.Now;
                    message.Likes = 1;
                    _db.Messages.Add(message);
                    await _db.SaveChangesAsync();
                }

                return RedirectToRoute(
                    "SubverseComments",
                    new
                    {
                        controller = "Comment",
                        action = "Comments",
                        id = message.Id,
                        subversetoshow = message.Subverse
                    }
                    );
            }
            ModelState.AddModelError(string.Empty, "Sorry, The subverse you are trying to post to does not exist.");
            return View();
        }

        // GET: user/id
        public ActionResult UserProfile(string id, int? page, string whattodisplay)
        {
            ViewBag.SelectedSubverse = "user";
            ViewBag.whattodisplay = whattodisplay;
            ViewBag.userid = id;
            const int pageSize = 25;
            int pageNumber = (page ?? 1);

            if (pageNumber < 1)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            if (!Utils.User.UserExists(id) || id == "deleted") return View("~/Views/Errors/Error_404.cshtml");

            // show comments
            if (whattodisplay != null && whattodisplay == "comments")
            {
                var userComments = from c in _db.Comments.OrderByDescending(c => c.Date)
                    where (c.Name.Equals(id) && c.Message.Anonymized == false) && (c.Name.Equals(id) && c.Message.Subverses.anonymized_mode == false)
                    select c;
                return View("UserComments", userComments.ToPagedList(pageNumber, pageSize));
            }

            // show submissions                        
            if (whattodisplay != null && whattodisplay == "submissions")
            {
                var userSubmissions = from b in _db.Messages.OrderByDescending(s => s.Date)
                    where (b.Name.Equals(id) && b.Anonymized == false) && (b.Name.Equals(id) && b.Subverses.anonymized_mode == false)
                    select b;
                return View("UserSubmitted", userSubmissions.ToPagedList(pageNumber, pageSize));
            }

            // default, show overview
            ViewBag.whattodisplay = "overview";

            var userDefaultSubmissions = from b in _db.Messages.OrderByDescending(s => s.Date)
                where b.Name.Equals(id) && b.Anonymized == false
                select b;
            return View("UserProfile", userDefaultSubmissions.ToPagedList(pageNumber, pageSize));
        }

        // GET: /
        public ActionResult Index(int? page)
        {
            ViewBag.SelectedSubverse = "frontpage";

            const int pageSize = 25;
            int pageNumber = (page ?? 1);

            if (pageNumber < 1)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            try
            {
                // show only submissions from subverses that user is subscribed to if user is logged in
                // also do a check so that user actually has subscriptions
                if (User.Identity.IsAuthenticated && Utils.User.SubscriptionCount(User.Identity.Name) > 0)
                {
                    var submissions = (from m in _db.Messages
                                       join s in _db.Subscriptions on m.Subverse equals s.SubverseName
                                       where m.Name != "deleted" && s.Username == User.Identity.Name
                                       select m)
                                       .OrderByDescending(s => s.Rank);

                    return View(submissions.ToPagedList(pageNumber, pageSize));
                }
                else
                {
                    // get only submissions from default subverses, order by rank
                    var submissions = (from message in _db.Messages
                                       where message.Name != "deleted"
                                       join defaultsubverse in _db.Defaultsubverses on message.Subverse equals defaultsubverse.name
                                       select message)
                                       .OrderByDescending(s => s.Rank);

                    return View(submissions.ToPagedList(pageNumber, pageSize));
                }
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        // GET: /new
        public ActionResult @New(int? page, string sortingmode)
        {
            // sortingmode: new, contraversial, hot, etc
            ViewBag.SortingMode = sortingmode;

            if (!sortingmode.Equals("new")) return RedirectToAction("Index", "Home");

            const int pageSize = 25;
            int pageNumber = (page ?? 1);

            if (pageNumber < 1)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // setup a cookie to find first time visitors and display welcome banner
            const string cookieName = "NotFirstTime";
            if (ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
            {
                // not a first time visitor
                ViewBag.FirstTimeVisitor = false;
            }
            else
            {
                // add a cookie for first time visitors
                var cookie = new HttpCookie(cookieName)
                {
                    Value = "whoaverse first time visitor identifier",
                    Expires = DateTime.Now.AddMonths(6)
                };
                ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                ViewBag.FirstTimeVisitor = true;
            }

            try
            {
                // show only submissions from subverses that user is subscribed to if user is logged in
                // also do a check so that user actually has subscriptions
                if (User.Identity.IsAuthenticated && Utils.User.SubscriptionCount(User.Identity.Name) > 0)
                {
                    var submissions = (from m in _db.Messages
                        join s in _db.Subscriptions on m.Subverse equals s.SubverseName
                        where m.Name != "deleted" && s.Username == User.Identity.Name
                        select m)
                        .OrderByDescending(s => s.Date);

                    return View("Index", submissions.ToPagedList(pageNumber, pageSize));
                }
                else
                {
                    // get only submissions from default subverses, sort by date
                    var submissions = (from message in _db.Messages
                        where message.Name != "deleted"
                        join defaultsubverse in _db.Defaultsubverses on message.Subverse equals defaultsubverse.name
                        select message)
                        .OrderByDescending(s => s.Date);

                    return View("Index", submissions.ToPagedList(pageNumber, pageSize));
                }

            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        // GET: /about
        public ActionResult About(string pagetoshow)
        {
            ViewBag.SelectedSubverse = string.Empty;

            switch (pagetoshow)
            {
                case "intro":
                    return View("~/Views/About/Intro.cshtml");
                case "contact":
                    return View("~/Views/About/Contact.cshtml");
                default:
                    return View("~/Views/About/About.cshtml");
            }
        }

        // GET: /cla
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
        public ActionResult Help(string pagetoshow)
        {
            ViewBag.SelectedSubverse = string.Empty;

            switch (pagetoshow)
            {
                case "privacy":
                    return View("~/Views/Help/Privacy.cshtml");
                case "useragreement":
                    return View("~/Views/Help/UserAgreement.cshtml");
                case "markdown":
                    return View("~/Views/Help/Markdown.cshtml");
                case "faq":
                    return View("~/Views/Help/Faq.cshtml");
                default:
                    return View("~/Views/Help/Index.cshtml");
            }
        }

        // GET: /help/privacy
        public ActionResult Privacy()
        {
            ViewBag.Message = "Privacy Policy";
            return View("~/Views/Help/Privacy.cshtml");
        }

        // GET: stickied submission from /v/announcements for display on frontpage
        [ChildActionOnly]
        public ActionResult StickiedSubmission()
        {
            var stickiedSubmissions = _db.Stickiedsubmissions.FirstOrDefault(s => s.Subversename == "announcements");

            if (stickiedSubmissions == null) return new EmptyResult();

            var stickiedSubmission = _db.Messages.Find(stickiedSubmissions.Submission_id);

            if (stickiedSubmission != null)
            {
                return PartialView("~/Views/Subverses/_Stickied.cshtml", stickiedSubmission);
            }
            return new EmptyResult();
        }

        // GET: list of subverses user moderates
        public ActionResult SubversesUserModerates(string userName)
        {
            if (userName != null)
            {
                return PartialView("~/Views/Shared/Userprofile/_SidebarSubsUserModerates.cshtml", _db.SubverseAdmins
                .Where(x => x.Username == userName)
                .Select(s => new SelectListItem { Value = s.SubverseName })
                .OrderBy(s => s.Value)
                .ToList()
                .AsEnumerable());
            }
            return new EmptyResult();
        }

        // GET: list of subverses user is subscribed to
        [ChildActionOnly]
        public ActionResult SubversesUserIsSubscribedTo(string userName)
        {
            if (userName != null)
            {
                return PartialView("~/Views/Shared/Userprofile/_SidebarSubsUserIsSubscribedTo.cshtml", _db.Subscriptions
                .Where(x => x.Username == userName)
                .Select(s => new SelectListItem { Value = s.SubverseName })
                .OrderBy(s => s.Value)
                .ToList()
                .AsEnumerable());
            }
            return new EmptyResult();
        }
    }
}