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
        //IAmAGate: Move queries to read-only mirror
        private readonly voatEntities _db = new voatEntities(true);

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
        public async Task<ActionResult> Submit([Bind(Include = "Id,Votes,Name,Date,Type,Linkdescription,Title,Rank,MessageContent,Subverse")] Message submission)
        {
            // abort if model state is invalid
            if (!ModelState.IsValid) return View();

            // save temp values for the view in case submission fails
            ViewBag.selectedSubverse = submission.Subverse;
            ViewBag.message = submission.MessageContent;
            ViewBag.title = submission.Title;
            ViewBag.linkDescription = submission.Linkdescription;

            // grab server timestamp and modify submission timestamp to have posting time instead of "started writing submission" time
            submission.Date = DateTime.Now;

            // check if user is banned
            if (Utils.User.IsUserGloballyBanned(User.Identity.Name) || Utils.User.IsUserBannedFromSubverse(User.Identity.Name, submission.Subverse))
            {
                ViewBag.SelectedSubverse = submission.Subverse;
                return View("~/Views/Home/Comments.cshtml", submission);
            }

            // check if user has reached hourly posting quota for target subverse
            if (Utils.User.UserHourlyPostingQuotaForSubUsed(User.Identity.Name, submission.Subverse))
            {
                ModelState.AddModelError("", "You have reached your hourly submission quota for this subverse.");
                return View();
            }

            // check if user has reached daily posting quota for target subverse
            if (Utils.User.UserDailyPostingQuotaForSubUsed(User.Identity.Name, submission.Subverse))
            {
                ModelState.AddModelError("", "You have reached your daily submission quota for this subverse.");
                return View();
            }

            // verify recaptcha if user has less than 25 CCP
            var userCcp = Karma.CommentKarma(User.Identity.Name);
            if (userCcp < 25)
            {
                bool isCaptchaCodeValid = await ReCaptchaUtility.Validate(Request);

                if (!isCaptchaCodeValid)
                {
                    ModelState.AddModelError("", "Incorrect recaptcha answer.");

                    // TODO 
                    // SET PREVENT SPAM DELAY TO 0

                    return View();
                }
            }

            // if user CCP or SCP is less than -50, allow only X submissions per 24 hours
            var userScp = Karma.LinkKarma(User.Identity.Name);
            if (userCcp <= -50 || userScp <= -50)
            {
                var quotaUsed = Utils.User.UserDailyPostingQuotaForNegativeScoreUsed(User.Identity.Name);
                if (quotaUsed)
                {
                    ModelState.AddModelError("", "You have reached your daily submission quota. Your current quota is " + MvcApplication.DailyPostingQuotaForNegativeScore + " submission(s) per 24 hours.");
                    return View();
                }
            }

            // abort if model state is invalid
            if (!ModelState.IsValid) return View("Submit");

            // check if subverse exists
            var targetSubverse = _db.Subverses.Find(submission.Subverse.Trim());

            if (targetSubverse == null || submission.Subverse.Equals("all", StringComparison.OrdinalIgnoreCase))
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

            // everything was okay so far, try to process incoming submission

            // submission is a link post
            // generate a thumbnail if submission is a direct link to image or video
            if (submission.Type == 2 && submission.MessageContent != null && submission.Linkdescription != null)
            {
                // check if same link was submitted before and deny submission
                var existingSubmission = _db.Messages.FirstOrDefault(s => s.MessageContent.Equals(submission.MessageContent, StringComparison.OrdinalIgnoreCase) && s.Subverse.Equals(submission.Subverse, StringComparison.OrdinalIgnoreCase));

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

                // process new link submission
                var addLinkSubmissionResult = await Submissions.AddNewLinkSubmission(submission,targetSubverse,User.Identity.Name);
                if (addLinkSubmissionResult != null)
                {
                    ModelState.AddModelError(string.Empty, addLinkSubmissionResult);
                    return View("Submit");
                }
            }
            else if (submission.Type == 1 && submission.Title != null)
            {
                // submission is a self post

                // strip unicode if submission contains unicode
                if (Submissions.ContainsUnicode(submission.Title))
                {
                    submission.Title = Submissions.StripUnicode(submission.Title);
                }
                // abort if title less than 10 characters
                if (submission.Title.Length < 10)
                {
                    ModelState.AddModelError(string.Empty, "Sorry, the the submission title may not be less than 10 characters.");
                    return View("Submit");
                }

                // use subverse name from subverses table
                submission.Subverse = targetSubverse.name;

                // flag the submission as anonymized if it was submitted to a subverse with active anonymized_mode
                if (targetSubverse.anonymized_mode)
                {
                    submission.Anonymized = true;
                }
                else
                {
                    submission.Name = User.Identity.Name;
                }

                // grab server timestamp and modify submission timestamp to have posting time instead of "started writing submission" time
                submission.Date = DateTime.Now;
                submission.Likes = 1;
                _db.Messages.Add(submission);

                // update last submission received date for target subverse
                targetSubverse.last_submission_received = DateTime.Now;

                if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPreSave))
                {
                    submission.MessageContent = ContentProcessor.Instance.Process(submission.MessageContent, ProcessingStage.InboundPreSave, submission);
                }

                _db.SaveChanges();

                if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave))
                {
                    ContentProcessor.Instance.Process(submission.MessageContent, ProcessingStage.InboundPostSave, submission);
                }
            }

            return RedirectToRoute(
                "SubverseComments",
                new
                {
                    controller = "Comment",
                    action = "Comments",
                    id = submission.Id,
                    subversetoshow = submission.Subverse
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

            // show saved                        
            if (whattodisplay != null && whattodisplay == "saved" && User.Identity.IsAuthenticated && User.Identity.Name == id)
            {
                IQueryable<SavedItem> savedSubmissions = (from m in _db.Messages
                                                          join s in _db.Savingtrackers on m.Id equals s.MessageId
                                                          where m.Name != "deleted" && s.UserName == User.Identity.Name
                                                          select new SavedItem()
                                                          {
                                                              SaveDateTime = s.Timestamp,
                                                              SavedMessage = m,
                                                              SavedComment = null
                                                          });

                IQueryable<SavedItem> savedComments = (from c in _db.Comments
                                                       join s in _db.Commentsavingtrackers on c.Id equals s.CommentId
                                                       where c.Name != "deleted" && s.UserName == User.Identity.Name
                                                       select new SavedItem()
                                                       {
                                                           SaveDateTime = s.Timestamp,
                                                           SavedMessage = null,
                                                           SavedComment = c
                                                       });

                // merge submissions and comments into one list sorted by date
                var mergedSubmissionsAndComments = savedSubmissions.Concat(savedComments).OrderByDescending(s => s.SaveDateTime).AsQueryable();

                var paginatedUserSubmissionsAndComments = new PaginatedList<SavedItem>(mergedSubmissionsAndComments, page ?? 0, pageSize);
                return View("UserSaved", paginatedUserSubmissionsAndComments);
            }

            // default, show overview
            ViewBag.whattodisplay = "overview";

            return View("UserProfile");
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




                    //IAmAGate: Perf mods for caching
                    int pagesToTake = 2;
                    int subset = pageNumber / pagesToTake;
                    string cacheKey = String.Format("front.{0}.block.{1}.sort.rank", User.Identity.Name, subset);
                    object cacheData = CacheHandler.Retrieve(cacheKey);

                    if (cacheData == null)
                    {
                        int recordsToTake = pageSize * pagesToTake; //4 pages worth

                        var getDataFunc = new Func<object>(() =>
                        {
                            using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                            {

                                var blockedSubverses = db.UserBlockedSubverses.Where(x => x.Username.Equals(User.Identity.Name)).Select(x => x.SubverseName);

                                IQueryable<Message> submissions = (from m in db.Messages.Include("Subverse").AsNoTracking()
                                                                   join s in db.Subscriptions on m.Subverse equals s.SubverseName
                                                                   where !m.IsArchived && m.Name != "deleted" && s.Username == User.Identity.Name
                                                                   where !(from bu in db.Bannedusers select bu.Username).Contains(m.Name)
                                                                   select m).OrderByDescending(s => s.Rank);

                                var submissionsWithoutStickies = submissions.Where(s => s.Stickiedsubmission.Submission_id != s.Id);

                                return submissionsWithoutStickies.Where(x => !blockedSubverses.Contains(x.Subverse)).Skip(subset * recordsToTake).Take(recordsToTake).ToList();

                            }

                        });
                        //now with new and improved locking
                        cacheData = CacheHandler.Register(cacheKey, getDataFunc, TimeSpan.FromMinutes(5));
                    }
                    var set = ((IList<Message>)cacheData).Skip((pageNumber - (subset * pagesToTake)) * pageSize).Take(pageSize).ToList();
                    PaginatedList<Message> paginatedSubmissions = new PaginatedList<Message>(set, pageNumber, pageSize, 50000);

                    return View(paginatedSubmissions);
                }
                else
                {


                    //IAmAGate: Perf mods for caching
                    string cacheKey = String.Format("front.guest.page.{0}.sort.rank", pageNumber);
                    object cacheData = CacheHandler.Retrieve(cacheKey);
                    if (cacheData == null)
                    {

                        var getDataFunc = new Func<object>(() =>
                        {
                            using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                            {

                                // get only submissions from default subverses, order by rank
                                IQueryable<Message> submissions = (from message in db.Messages.AsNoTracking()
                                                                   where !message.IsArchived && message.Name != "deleted"
                                                                   where !(from bu in db.Bannedusers select bu.Username).Contains(message.Name)
                                                                   join defaultsubverse in db.Defaultsubverses on message.Subverse equals defaultsubverse.name
                                                                   select message).OrderByDescending(s => s.Rank);

                                return submissions.Where(s => s.Stickiedsubmission.Submission_id != s.Id).Skip(pageNumber * pageSize).Take(pageSize).ToList();

                            }
                        });
                        //Now with it's own locking!
                        cacheData = CacheHandler.Register(cacheKey, getDataFunc, TimeSpan.FromMinutes(CONSTANTS.DEFAULT_GUEST_PAGE_CACHE_MINUTES), (pageNumber < 3 ? 0 : 3));
                    }

                    PaginatedList<Message> paginatedSubmissions = new PaginatedList<Message>((IList<Message>)cacheData, pageNumber, pageSize, 50000);

                    return View(paginatedSubmissions);
                }
            }
            catch (Exception)
            {
                return View("~/Views/Errors/DbNotResponding.cshtml");
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
                    var blockedSubverses = _db.UserBlockedSubverses.Where(x => x.Username.Equals(User.Identity.Name)).Select(x => x.SubverseName);

                    foreach (var set in userSetSubscriptions)
                    {
                        Usersetsubscription setId = set;
                        var userSetDefinition = _db.Usersetdefinitions.Where(st => st.Set_id == setId.Set_id);

                        foreach (var subverse in userSetDefinition)
                        {
                            // get top ranked submissions
                            var submissionsExcludingBlockedSubverses = SetsUtility.TopRankedSubmissionsFromASub(subverse.Subversename, _db.Messages, subverse.Userset.Name, 2, 0)
                                .Where(x => !blockedSubverses.Contains(x.Subverse));
                            submissions.AddRange(submissionsExcludingBlockedSubverses);
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
                return View("~/Views/Errors/DbNotResponding.cshtml");
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

                    //IAmAGate: Perf mods for caching
                    int pagesToTake = 2;
                    int subset = pageNumber / pagesToTake;
                    string cacheKey = String.Format("front.{0}.block.{1}.sort.new", User.Identity.Name, subset);
                    object cacheData = CacheHandler.Retrieve(cacheKey);

                    if (cacheData == null)
                    {
                        int recordsToTake = 25 * pagesToTake; //pages worth

                        var getDataFunc = new Func<object>(() =>
                        {
                            using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                            {
                                var blockedSubverses = db.UserBlockedSubverses.Where(x => x.Username.Equals(User.Identity.Name)).Select(x => x.SubverseName);
                                IQueryable<Message> submissions = (from m in db.Messages
                                                                   join s in db.Subscriptions on m.Subverse equals s.SubverseName
                                                                   where !m.IsArchived && m.Name != "deleted" && s.Username == User.Identity.Name
                                                                   where !(from bu in db.Bannedusers select bu.Username).Contains(m.Name)
                                                                   select m).OrderByDescending(s => s.Date);
                                return submissions.Where(x => !blockedSubverses.Contains(x.Subverse)).Skip(subset * recordsToTake).Take(recordsToTake).ToList();
                            }
                        });
                        //now with new and improved locking
                        cacheData = CacheHandler.Register(cacheKey, getDataFunc, TimeSpan.FromMinutes(5), 1);
                    }
                    var set = ((IList<Message>)cacheData).Skip((pageNumber - (subset * pagesToTake)) * pageSize).Take(pageSize).ToList();

                    PaginatedList<Message> paginatedSubmissions = new PaginatedList<Message>(set, pageNumber, pageSize, 50000);
                    return View("Index", paginatedSubmissions);
                }
                else
                {

                    //IAmAGate: Perf mods for caching
                    string cacheKey = String.Format("front.guest.page.{0}.sort.new", pageNumber);
                    object cacheData = CacheHandler.Retrieve(cacheKey);

                    if (cacheData == null)
                    {
                        var getDataFunc = new Func<object>(() =>
                        {
                            using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                            {
                                // get only submissions from default subverses, order by rank
                                IQueryable<Message> submissions = (from message in db.Messages
                                                                   where !message.IsArchived && message.Name != "deleted"
                                                                   where !(from bu in db.Bannedusers select bu.Username).Contains(message.Name)
                                                                   join defaultsubverse in db.Defaultsubverses on message.Subverse equals defaultsubverse.name
                                                                   select message).OrderByDescending(s => s.Date);
                                return submissions.Where(s => s.Stickiedsubmission.Submission_id != s.Id).Skip(pageNumber * pageSize).Take(pageSize).ToList();
                            }
                        });

                        //now with new and improved locking
                        cacheData = CacheHandler.Register(cacheKey, getDataFunc, TimeSpan.FromMinutes(CONSTANTS.DEFAULT_GUEST_PAGE_CACHE_MINUTES), (pageNumber < 3 ? 0 : 3));
                    }
                    PaginatedList<Message> paginatedSubmissions = new PaginatedList<Message>((IList<Message>)cacheData, pageNumber, pageSize, 50000);

                    //// get only submissions from default subverses, sort by date
                    //IQueryable<Message> submissions = (from submission in _db.Messages
                    //                              where submission.Name != "deleted"
                    //                              where !(from bu in _db.Bannedusers select bu.Username).Contains(submission.Name)
                    //                              join defaultsubverse in _db.Defaultsubverses on submission.Subverse equals defaultsubverse.name
                    //                              select submission).OrderByDescending(s => s.Date);

                    //PaginatedList<Message> paginatedSubmissions = new PaginatedList<Message>(submissions, page ?? 0, pageSize);

                    return View("Index", paginatedSubmissions);
                }
            }
            catch (Exception)
            {
                return View("~/Views/Errors/DbNotResponding.cshtml");
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

        // GET: stickied submission from /v/announcements for the frontpage
        [ChildActionOnly]
        [OutputCache(Duration = 600)]
        public ActionResult StickiedSubmission()
        {
            var stickiedSubmissions = _db.Stickiedsubmissions.FirstOrDefault(s => s.Subversename == "announcements");

            if (stickiedSubmissions == null) return new EmptyResult();

            var stickiedSubmission = DataCache.Submission.Retrieve(stickiedSubmissions.Submission_id);

            if (stickiedSubmission != null)
            {
                return PartialView("~/Views/Subverses/_Stickied.cshtml", stickiedSubmission);
            }
            return new EmptyResult();
        }

        // GET: list of subverses user moderates
        [OutputCache(Duration = 600, VaryByParam = "*")]
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

        // GET: list of subverses user is subscribed to for sidebar
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

        [OutputCache(Duration = 600, VaryByParam = "none")]
        public ActionResult FeaturedSub()
        {
            var featuredSub = _db.Featuredsubs.OrderByDescending(s => s.Featured_on).FirstOrDefault();

            if (featuredSub == null) return new EmptyResult();

            return PartialView("~/Views/Subverses/_FeaturedSub.cshtml", featuredSub);
        }
    }
}
