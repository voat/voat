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
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Voat.Models;
using Voat.Models.ViewModels;
using Voat.Utils;
using Voat.Utils.Components;

namespace Voat.Controllers
{
    public class HomeController : Controller
    {
        private readonly whoaverseEntities _db = new whoaverseEntities();

        [HttpPost]
        [PreventSpam(DelayRequest = 300, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public ActionResult ClaSubmit(Cla claModel)
        {
            if (!ModelState.IsValid) return View("~/Views/Legal/Cla.cshtml");

            var from = new MailAddress(claModel.Email);
            var to = new MailAddress("legal@voat.co");
            var sb = new StringBuilder();
            var msg = new MailMessage(@from, to) { Subject = "New CLA Submission from " + claModel.FullName };

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
            ViewBag.selectedSubverse = selectedsubverse;

            string linkPost = Request.Params["linkpost"];
            string linkDescription = Request.Params["title"];
            string linkUrl = Request.QueryString["url"];

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

            if (selectedsubverse == null) return View();

            if (!selectedsubverse.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.selectedSubverse = selectedsubverse;
            }

            return View("Submit");
        }

        // GET: submitlink
        [Authorize]
        public ActionResult SubmitLinkService(string selectedsubverse)
        {
            string linkDescription = Request.Params["title"];
            string linkUrl = Request.QueryString["url"];

            ViewBag.linkPost = "true";
            ViewBag.action = "link";
            ViewBag.linkDescription = linkDescription;
            ViewBag.linkUrl = linkUrl;

            if (selectedsubverse == null) return View("~/Views/Home/Submit.cshtml");
            if (!selectedsubverse.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.selectedSubverse = selectedsubverse;
            }

            return View("~/Views/Home/Submit.cshtml");
        }

        // POST: submit
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [PreventSpam(DelayRequest = 60, ErrorMessage = "Sorry, you are doing that too fast. Please try again in 60 seconds.")]
        public ActionResult Submit([Bind(Include = "Id,Votes,Name,Date,Type,Linkdescription,Title,Rank,MessageContent,Subverse")] Message message)
        {
            // abort if model state is invalid
            if (!ModelState.IsValid) return View();

            // save temp values for the view in case submission fails
            ViewBag.selectedSubverse = message.Subverse;
            ViewBag.message = message.MessageContent;
            ViewBag.title = message.Title;
            ViewBag.linkDescription = message.Linkdescription;

            // check if user is banned
            if (Utils.User.IsUserGloballyBanned(message.Name) || Utils.User.IsUserBannedFromSubverse(User.Identity.Name, message.Subverse))
            {
                ViewBag.SelectedSubverse = message.Subverse;
                return View("~/Views/Home/Comments.cshtml", message);
            }

            // check if user has reached daily posting quota for target subverse
            if (Utils.User.UserDailyPostingQuotaForSubUsed(User.Identity.Name,message.Subverse))
            {
                ModelState.AddModelError("", "You have reached your daily submission quota for this subverse.");
                return View();
            }

            // verify recaptcha if user has less than 25 CCP
            if (Karma.CommentKarma(User.Identity.Name) < 25)
            {
                const string captchaMessage = "";
                var isCaptchaCodeValid = ReCaptchaUtility.GetCaptchaResponse(captchaMessage, Request);

                if (!isCaptchaCodeValid)
                {
                    ModelState.AddModelError("", "Incorrect recaptcha answer.");

                    // TODO 
                    // SET PREVENT SPAM DELAY TO 0

                    return View();
                }
            }

            // everything was okay, process incoming submission
            // abort if model state is invalid
            if (!ModelState.IsValid) return View("Submit");

            // check if subverse exists
            var targetSubverse = _db.Subverses.Find(message.Subverse.Trim());
            if (targetSubverse == null || message.Subverse.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Sorry, The subverse you are trying to post to does not exist.");
                return View("Submit");
            }

            // check if subverse has "authorized_submitters_only" set and dissalow submission if user is not allowed submitter
            if (targetSubverse.authorized_submitters_only)
            {
                if (!Utils.User.IsUserSubverseModerator(User.Identity.Name, targetSubverse.name))
                {
                    // user is not a moderator, check if user is an administrator
                    if (!Utils.User.IsUserSubverseAdmin(User.Identity.Name, targetSubverse.name))
                    {
                        ModelState.AddModelError("", "You are not authorized to submit links or start discussions in this subverse. Please contact subverse moderators for authorization.");
                        return View("Submit");
                    }
                }
            }

            // submission is a link post
            // generate a thumbnail if submission is a direct link to image or video
            if (message.Type == 2 && message.MessageContent != null && message.Linkdescription != null)
            {
                // abort if title contains unicode
                if (Submissions.ContainsUnicode(message.Linkdescription))
                {
                    ModelState.AddModelError(string.Empty, "Sorry, the link description may not contain unicode characters.");
                    return View("Submit");
                }
                // abort if title less than 10 characters
                if (message.Linkdescription.Length < 10)
                {
                    ModelState.AddModelError(string.Empty, "Sorry, the link description may not be less than 10 characters.");
                    return View("Submit");
                }

                var domain = UrlUtility.GetDomainFromUri(message.MessageContent);

                // check if target subvere allows submissions from globally banned hostnames
                if (!targetSubverse.exclude_sitewide_bans)
                {
                    // check if hostname is banned before accepting submission
                    if (BanningUtility.IsHostnameBanned(domain))
                    {
                        ModelState.AddModelError(string.Empty, "Sorry, the hostname you are trying to submit is banned.");
                        return View("Submit");
                    }
                }

                // check if same link was submitted before and deny submission
                var existingSubmission = _db.Messages.FirstOrDefault(s => s.MessageContent.Equals(message.MessageContent, StringComparison.OrdinalIgnoreCase) && s.Subverse.Equals(message.Subverse, StringComparison.OrdinalIgnoreCase));

                // submission is a repost, discard it and inform the user
                if (existingSubmission != null)
                {
                    ModelState.AddModelError(string.Empty, "Sorry, this link has already been submitted by someone else.");

                    // todo: offer the option to repost after informing the user about it
                    return RedirectToRoute(
                        "SubverseComments",
                        new
                        {
                            controller = "Comment",
                            action = "Comments",
                            id = existingSubmission.Id,
                            subversetoshow = existingSubmission.Subverse
                        }
                        );
                }

                // check if target subverse has thumbnails setting enabled before generating a thumbnail
                if (targetSubverse.enable_thumbnails)
                {
                    // try to generate and assign a thumbnail to submission model
                    message.Thumbnail = ThumbGenerator.ThumbnailFromSubmissionModel(message);
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

                // update last submission received date for target subverse
                targetSubverse.last_submission_received = DateTime.Now;
                _db.SaveChanges();
            }
            else if (message.Type == 1 && message.Title != null)
            {
                // submission is a self post

                // abort if title contains unicode
                if (Submissions.ContainsUnicode(message.Title))
                {
                    ModelState.AddModelError(string.Empty, "Sorry, the message title may not contain unicode characters.");
                    return View("Submit");
                }
                // abort if title less than 10 characters
                if (message.Title.Length < 10)
                {
                    ModelState.AddModelError(string.Empty, "Sorry, the the message title may not be less than 10 characters.");
                    return View("Submit");
                }

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
                // update last submission received date for target subverse
                targetSubverse.last_submission_received = DateTime.Now;

                if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPreSave))
                {
                    message.MessageContent = ContentProcessor.Instance.Process(message.MessageContent, ProcessingStage.InboundPreSave, message);
                }

                _db.SaveChanges();

                if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave))
                {
                    ContentProcessor.Instance.Process(message.MessageContent, ProcessingStage.InboundPostSave, message);
                }
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

        // GET: user/id
        public ActionResult UserProfile(string id, int? page, string whattodisplay)
        {
            ViewBag.SelectedSubverse = "user";
            ViewBag.whattodisplay = whattodisplay;
            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            if (!Utils.User.UserExists(id) || id == "deleted") return View("~/Views/Errors/Error_404.cshtml");

            ViewBag.userid = Utils.User.OriginalUsername(id);

            // show comments
            if (whattodisplay != null && whattodisplay == "comments")
            {
                var userComments = from c in _db.Comments.OrderByDescending(c => c.Date)
                                   where (c.Name.Equals(id) && c.Message.Anonymized == false) && (c.Name.Equals(id) && c.Message.Subverses.anonymized_mode == false)
                                   select c;

                PaginatedList<Comment> paginatedUserComments = new PaginatedList<Comment>(userComments, page ?? 0, pageSize);

                return View("UserComments", paginatedUserComments);
            }

            // show submissions                        
            if (whattodisplay != null && whattodisplay == "submissions")
            {
                var userSubmissions = from b in _db.Messages.OrderByDescending(s => s.Date)
                                      where (b.Name.Equals(id) && b.Anonymized == false) && (b.Name.Equals(id) && b.Subverses.anonymized_mode == false)
                                      select b;

                PaginatedList<Message> paginatedUserSubmissions = new PaginatedList<Message>(userSubmissions, page ?? 0, pageSize);


                return View("UserSubmitted", paginatedUserSubmissions);
            }

            // default, show overview
            ViewBag.whattodisplay = "overview";

            var userDefaultSubmissions = from b in _db.Messages.OrderByDescending(s => s.Date)
                                         where b.Name.Equals(id) && b.Anonymized == false
                                         select b;

            PaginatedList<Message> paginatedUserDefaultSubmissions = new PaginatedList<Message>(userDefaultSubmissions, page ?? 0, pageSize);


            return View("UserProfile", paginatedUserDefaultSubmissions);
        }

        // GET: /
        public ActionResult Index(int? page)
        {
            ViewBag.SelectedSubverse = "frontpage";

            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            try
            {
                // show only submissions from subverses that user is subscribed to if user is logged in
                // also do a check so that user actually has subscriptions
                if (User.Identity.IsAuthenticated && Utils.User.SubscriptionCount(User.Identity.Name) > 0)
                {
                    IQueryable<Message> submissions = (from m in _db.Messages
                                                       join s in _db.Subscriptions on m.Subverse equals s.SubverseName
                                                       where m.Name != "deleted" && s.Username == User.Identity.Name
                                                       select m)
                        .OrderByDescending(s => s.Rank);

                    var submissionsWithoutStickies = submissions.Where(s => s.Stickiedsubmission.Submission_id != s.Id);

                    PaginatedList<Message> paginatedSubmissions = new PaginatedList<Message>(submissionsWithoutStickies, page ?? 0, pageSize);

                    return View(paginatedSubmissions);
                }
                else
                {
                    // get only submissions from default subverses, order by rank
                    IQueryable<Message> submissions = (from message in _db.Messages
                                                       where message.Name != "deleted"
                                                       join defaultsubverse in _db.Defaultsubverses on message.Subverse equals defaultsubverse.name
                                                       select message)
                        .OrderByDescending(s => s.Rank);

                    var submissionsWithoutStickies = submissions.Where(s => s.Stickiedsubmission.Submission_id != s.Id);

                    PaginatedList<Message> paginatedSubmissions = new PaginatedList<Message>(submissionsWithoutStickies, page ?? 0, pageSize);

                    return View(paginatedSubmissions);
                }
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        // GET: /v2
        public ActionResult IndexV2(int? page)
        {
            ViewBag.SelectedSubverse = "frontpage";
            var submissions = new List<SetSubmission>();

            try
            {
                var frontPageResultModel = new SetFrontpageViewModel();

                // show user sets
                // get names of each set that user is subscribed to
                // for each set name, get list of subverses that define the set
                // for each subverse, get top ranked submissions
                if (User.Identity.IsAuthenticated && Utils.User.SetsSubscriptionCount(User.Identity.Name) > 0)
                {
                    var userSetSubscriptions = _db.Usersetsubscriptions.Where(usd => usd.Username == User.Identity.Name);

                    foreach (var set in userSetSubscriptions)
                    {
                        Usersetsubscription setId = set;
                        var userSetDefinition = _db.Usersetdefinitions.Where(st => st.Set_id == setId.Set_id);

                        foreach (var subverse in userSetDefinition)
                        {
                            // get top ranked submissions
                            submissions.AddRange(SetsUtility.TopRankedSubmissionsFromASub(subverse.Subversename, _db.Messages, subverse.Userset.Name, 2, 0));
                        }
                    }

                    frontPageResultModel.HasSetSubscriptions = true;
                    frontPageResultModel.UserSets = userSetSubscriptions;
                    frontPageResultModel.SubmissionsList = new List<SetSubmission>(submissions.OrderByDescending(s => s.Rank));

                    return View("~/Views/Home/IndexV2.cshtml", frontPageResultModel);
                }

                // show default sets since user is not logged in or has no set subscriptions
                // get names of default sets
                // for each set name, get list of subverses
                // for each subverse, get top ranked submissions
                var defaultSets = _db.Usersets.Where(ds => ds.Default && ds.Usersetdefinitions.Any());
                var defaultFrontPageResultModel = new SetFrontpageViewModel();

                foreach (var set in defaultSets)
                {
                    Userset setId = set;
                    var defaultSetDefinition = _db.Usersetdefinitions.Where(st => st.Set_id == setId.Set_id);

                    foreach (var subverse in defaultSetDefinition)
                    {
                        // get top ranked submissions
                        submissions.AddRange(SetsUtility.TopRankedSubmissionsFromASub(subverse.Subversename, _db.Messages, set.Name, 2, 0));
                    }
                }

                defaultFrontPageResultModel.DefaultSets = defaultSets;
                defaultFrontPageResultModel.HasSetSubscriptions = false;
                defaultFrontPageResultModel.SubmissionsList = new List<SetSubmission>(submissions.OrderByDescending(s => s.Rank));


                return View("~/Views/Home/IndexV2.cshtml", defaultFrontPageResultModel);
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
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
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
                HttpCookie hc = new HttpCookie("NotFirstTime", "1");
                hc.Expires = DateTime.Now.AddYears(1);
                System.Web.HttpContext.Current.Response.Cookies.Add(hc);

                ViewBag.FirstTimeVisitor = true;
            }

            try
            {
                // show only submissions from subverses that user is subscribed to if user is logged in
                // also do a check so that user actually has subscriptions
                if (User.Identity.IsAuthenticated && Utils.User.SubscriptionCount(User.Identity.Name) > 0)
                {
                    IQueryable<Message> submissions = (from m in _db.Messages
                                                       join s in _db.Subscriptions on m.Subverse equals s.SubverseName
                                                       where m.Name != "deleted" && s.Username == User.Identity.Name
                                                       select m)
                        .OrderByDescending(s => s.Date);

                    PaginatedList<Message> paginatedSubmissions = new PaginatedList<Message>(submissions, page ?? 0, pageSize);

                    return View("Index", paginatedSubmissions);
                }
                else
                {
                    // get only submissions from default subverses, sort by date
                    IQueryable<Message> submissions = (from message in _db.Messages
                                                       where message.Name != "deleted"
                                                       join defaultsubverse in _db.Defaultsubverses on message.Subverse equals defaultsubverse.name
                                                       select message)
                        .OrderByDescending(s => s.Date);

                    PaginatedList<Message> paginatedSubmissions = new PaginatedList<Message>(submissions, page ?? 0, pageSize);

                    return View("Index", paginatedSubmissions);
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
                default:
                    return View("~/Views/About/About.cshtml");
            }
        }

        // GET: /cla
        public ActionResult Cla()
        {
            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.Message = "Voat CLA";
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

        public ActionResult FeaturedSub()
        {
            var featuredSub = _db.Featuredsubs.OrderByDescending(s => s.Featured_on).FirstOrDefault();

            if (featuredSub == null) return new EmptyResult();

            return PartialView("~/Views/Subverses/_FeaturedSub.cshtml", featuredSub);

        }
    }
}
