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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Voat.Data.Models;
using Voat.Models;
using Voat.Models.ViewModels;
using Voat.UI.Utilities;
using Voat.Utilities;


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
        public async Task<ActionResult> Submit([Bind(Include = "ID,Votes,Name,CreationDate,Type,LinkDescription,Title,Rank,Content,Subverse")] Submission submission)
        {
            // abort if model state is invalid
            if (!ModelState.IsValid) return View();

            // save temp values for the view in case submission fails
            ViewBag.selectedSubverse = submission.Subverse;
            ViewBag.message = submission.Content;
            ViewBag.title = submission.Title;
            ViewBag.linkDescription = submission.LinkDescription;

            // grab server timestamp and modify submission timestamp to have posting time instead of "started writing submission" time
            submission.CreationDate = DateTime.Now;

            // check if user is banned
            if (UserHelper.IsUserGloballyBanned(User.Identity.Name) || UserHelper.IsUserBannedFromSubverse(User.Identity.Name, submission.Subverse))
            {
                ViewBag.SelectedSubverse = submission.Subverse;
                return View("~/Views/Home/Comments.cshtml", submission);
            }
            if (String.IsNullOrEmpty(submission.Subverse))
            {
                ModelState.AddModelError(string.Empty, "Please enter a subverse.");
                return View("Submit");
            }
            // check if subverse exists
            var targetSubverse = _db.Subverses.Find(submission.Subverse.Trim());

            if (targetSubverse == null || targetSubverse.Name.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Sorry, The subverse you are trying to post to does not exist.");
                return View("Submit");
            }

            //wrap captcha check in anon method as following method is in non UI dll
            var captchaCheck = new Func<HttpRequestBase, Task<bool>>(request => {
                return ReCaptchaUtility.Validate(request);
            });

            // check if this submission is valid and good to go
            var preProcessCheckResult = await Submissions.PreAddSubmissionCheck(submission, Request, User.Identity.Name, targetSubverse, captchaCheck);
            if (preProcessCheckResult != null)
            {
                ModelState.AddModelError(string.Empty, preProcessCheckResult);
                return View("Submit");
            }

            // submission is a link post
            if (submission.Type == 2 && submission.Content != null && submission.LinkDescription != null)
            {
                // check if same link was submitted before and deny submission
                var existingSubmission = _db.Submissions.FirstOrDefault(s => s.Content.Equals(submission.Content, StringComparison.OrdinalIgnoreCase) && s.Subverse.Equals(submission.Subverse, StringComparison.OrdinalIgnoreCase));

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
                            id = existingSubmission.ID,
                            subversetoshow = existingSubmission.Subverse
                        }
                        );
                }

                // process new link submission
                var addLinkSubmissionResult = await Submissions.AddNewSubmission(submission, targetSubverse, User.Identity.Name);
                if (addLinkSubmissionResult != null)
                {
                    ModelState.AddModelError(string.Empty, addLinkSubmissionResult);
                    return View("Submit");
                }
                // update last submission received date for target subverse
                targetSubverse.LastSubmissionDate = DateTime.Now;
                await _db.SaveChangesAsync();
            }
            // submission is a message type submission
            else if (submission.Type == 1 && submission.Title != null)
            {
                var containsBannedDomain = BanningUtility.ContentContainsBannedDomain(targetSubverse.Name, submission.Content);
                if (containsBannedDomain)
                {
                    ModelState.AddModelError(string.Empty, "Sorry, this post contains links to banned domains.");
                    return View("Submit");
                }

                // process new message type submission
                var addMessageSubmissionResult = await Submissions.AddNewSubmission(submission, targetSubverse, User.Identity.Name);
                if (addMessageSubmissionResult != null)
                {
                    ModelState.AddModelError(string.Empty, addMessageSubmissionResult);
                    return View("Submit");
                }
                // update last submission received date for target subverse
                targetSubverse.LastSubmissionDate = DateTime.Now;
                await _db.SaveChangesAsync();
            }

            // redirect to comments section of newly posted submission
            return RedirectToRoute(
                "SubverseComments",
                new
                {
                    controller = "Comment",
                    action = "Comments",
                    id = submission.ID,
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

            if (!UserHelper.UserExists(id) || id == "deleted") return View("~/Views/Errors/Error_404.cshtml");

            ViewBag.userid = UserHelper.OriginalUsername(id);

            // show comments
            if (whattodisplay != null && whattodisplay == "comments")
            {
                var userComments = from c in _db.Comments.OrderByDescending(c => c.CreationDate)
                                   where (c.UserName.Equals(id) && c.Submission.IsAnonymized == false && !c.IsDeleted) && (c.UserName.Equals(id) && c.Submission.Subverse1.IsAnonymized == false)
                                   select c;

                PaginatedList<Comment> paginatedUserComments = new PaginatedList<Comment>(userComments, page ?? 0, pageSize);

                return View("UserComments", paginatedUserComments);
            }

            // show submissions                        
            if (whattodisplay != null && whattodisplay == "submissions")
            {
                var userSubmissions = from b in _db.Submissions.OrderByDescending(s => s.CreationDate)
                                      where (b.UserName.Equals(id) && b.IsAnonymized == false && !b.IsDeleted) && (b.UserName.Equals(id) && b.Subverse1.IsAnonymized == false)
                                      select b;

                PaginatedList<Submission> paginatedUserSubmissions = new PaginatedList<Submission>(userSubmissions, page ?? 0, pageSize);


                return View("UserSubmitted", paginatedUserSubmissions);
            }

            // show saved                        
            if (whattodisplay != null && whattodisplay == "saved" && User.Identity.IsAuthenticated && User.Identity.Name.Equals(id, StringComparison.OrdinalIgnoreCase))
            {
                IQueryable<SavedItem> savedSubmissions = (from m in _db.Submissions
                                                          join s in _db.SubmissionSaveTrackers on m.ID equals s.SubmissionID
                                                          where !m.IsDeleted && s.UserName == User.Identity.Name
                                                          select new SavedItem()
                                                          {
                                                              SaveDateTime = s.CreationDate,
                                                              SavedSubmission = m,
                                                              SavedComment = null
                                                          });

                IQueryable<SavedItem> savedComments = (from c in _db.Comments
                                                       join s in _db.CommentSaveTrackers on c.ID equals s.CommentID
                                                       where !c.IsDeleted && s.UserName == User.Identity.Name
                                                       select new SavedItem()
                                                       {
                                                           SaveDateTime = s.CreationDate,
                                                           SavedSubmission = null,
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
                if (User.Identity.IsAuthenticated && UserHelper.SubscriptionCount(User.Identity.Name) > 0 && Request.QueryString["frontpage"] != "guest")
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

                                var blockedSubverses = db.UserBlockedSubverses.Where(x => x.UserName.Equals(User.Identity.Name)).Select(x => x.Subverse);
                                
                                // TODO: 
                                // check if user wants to exclude downvoted submissions from frontpage
                                var downvotedSubmissionIds = db.SubmissionVoteTrackers.AsNoTracking().Where(vt => vt.UserName.Equals(User.Identity.Name) && vt.VoteStatus == -1).Select(s=>s.SubmissionID);

                                IQueryable<Submission> submissions = (from m in db.Submissions.Include("Subverse").AsNoTracking()
                                                                   join s in db.SubverseSubscriptions on m.Subverse equals s.Subverse
                                                                   where !m.IsArchived && !m.IsDeleted && s.UserName == User.Identity.Name
                                                                   where !(from bu in db.BannedUsers select bu.UserName).Contains(m.UserName)
                                                                   select m).OrderByDescending(s => s.Rank);

                                var submissionsWithoutStickies = submissions.Where(s => s.StickiedSubmission.SubmissionID != s.ID);

                                // exclude downvoted submissions
                                var submissionsWithoutDownvotedSubmissions = submissionsWithoutStickies.Where(x => !downvotedSubmissionIds.Contains(x.ID));

                                return submissionsWithoutDownvotedSubmissions.Where(x => !blockedSubverses.Contains(x.Subverse)).Skip(subset * recordsToTake).Take(recordsToTake).ToList();
                            }

                        });
                        //now with new and improved locking
                        cacheData = CacheHandler.Register(cacheKey, getDataFunc, TimeSpan.FromMinutes(5));
                    }
                    var set = ((IList<Submission>)cacheData).Skip((pageNumber - (subset * pagesToTake)) * pageSize).Take(pageSize).ToList();
                    PaginatedList<Submission> paginatedSubmissions = new PaginatedList<Submission>(set, pageNumber, pageSize, 50000);

                    return View(paginatedSubmissions);
                }
                else
                {
                     IList<Submission> cacheData = GetGuestFrontPage(pageSize, pageNumber);

                    PaginatedList<Submission> paginatedSubmissions = new PaginatedList<Submission>((IList<Submission>)cacheData, pageNumber, pageSize, 50000);

                    return View(paginatedSubmissions);
                }
            }
            catch (Exception)
            {
                return View("~/Views/Errors/DbNotResponding.cshtml");
            }
        }

        public static IList<Submission> GetGuestFrontPage(int pageSize, int pageNumber)
        {
            //IAmAGate: Perf mods for caching
            string cacheKey = String.Format("front.guest.page.{0}.sort.rank", pageNumber);
            IList<Submission> cacheData = CacheHandler.Retrieve<IList<Submission>>(cacheKey);
            if (cacheData == null)
            {

                var getDataFunc = new Func<IList<Submission>>(() =>
                {
                    using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                    {
                        // get only submissions from default subverses not older than 24 hours, order by relative rank
                        var startDate = DateTime.Now.Add(new TimeSpan(0, -24, 0, 0, 0));

                        IQueryable<Submission> submissions = (from message in db.Submissions.AsNoTracking()
                                                              where !message.IsArchived && !message.IsDeleted && message.UpCount >= 3 && message.CreationDate >= startDate && message.CreationDate <= DateTime.Now
                                                              where !(from bu in db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                              join defaultsubverse in db.DefaultSubverses on message.Subverse equals defaultsubverse.Subverse
                                                              select message).OrderByDescending(s => s.RelativeRank);

                        return submissions.Where(s => s.StickiedSubmission.SubmissionID != s.ID).Skip(pageNumber * pageSize).Take(pageSize).ToList();

                    }
                });
                //Now with it's own locking!
                cacheData = CacheHandler.Register(cacheKey, getDataFunc, TimeSpan.FromMinutes(CONSTANTS.DEFAULT_GUEST_PAGE_CACHE_MINUTES), (pageNumber < 3 ? 0 : 3));
            }

            return cacheData;
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
                if (User.Identity.IsAuthenticated && UserHelper.SetsSubscriptionCount(User.Identity.Name) > 0)
                {
                    var userSetSubscriptions = _db.UserSetSubscriptions.Where(usd => usd.UserName == User.Identity.Name);
                    var blockedSubverses = _db.UserBlockedSubverses.Where(x => x.UserName.Equals(User.Identity.Name)).Select(x => x.Subverse);

                    foreach (var set in userSetSubscriptions)
                    {
                        UserSetSubscription setId = set;
                        var userSetDefinition = _db.UserSetLists.Where(st => st.UserSetID == setId.UserSetID);

                        foreach (var subverse in userSetDefinition)
                        {
                            // get top ranked submissions
                            var submissionsExcludingBlockedSubverses = SetsUtility.TopRankedSubmissionsFromASub(subverse.Subverse, _db.Submissions, subverse.UserSet.Name, 2, 0)
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
                var defaultSets = _db.UserSets.Where(ds => ds.IsDefault && ds.UserSetLists.Any());
                var defaultFrontPageResultModel = new SetFrontpageViewModel();

                foreach (var set in defaultSets)
                {
                    UserSet setId = set;
                    var defaultSetDefinition = _db.UserSetLists.Where(st => st.UserSetID == setId.ID);

                    foreach (var subverse in defaultSetDefinition)
                    {
                        // get top ranked submissions
                        submissions.AddRange(SetsUtility.TopRankedSubmissionsFromASub(subverse.Subverse, _db.Submissions, set.Name, 2, 0));
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
                if (User.Identity.IsAuthenticated && UserHelper.SubscriptionCount(User.Identity.Name) > 0)
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
                                var blockedSubverses = db.UserBlockedSubverses.Where(x => x.UserName.Equals(User.Identity.Name)).Select(x => x.Subverse);
                                IQueryable<Submission> submissions = (from m in db.Submissions
                                                                   join s in db.SubverseSubscriptions on m.Subverse equals s.Subverse
                                                                   where !m.IsArchived && !m.IsDeleted && s.UserName == User.Identity.Name
                                                                   where !(from bu in db.BannedUsers select bu.UserName).Contains(m.UserName)
                                                                   select m).OrderByDescending(s => s.CreationDate);
                                return submissions.Where(x => !blockedSubverses.Contains(x.Subverse)).Skip(subset * recordsToTake).Take(recordsToTake).ToList();
                            }
                        });
                        //now with new and improved locking
                        cacheData = CacheHandler.Register(cacheKey, getDataFunc, TimeSpan.FromMinutes(5), 1);
                    }
                    var set = ((IList<Submission>)cacheData).Skip((pageNumber - (subset * pagesToTake)) * pageSize).Take(pageSize).ToList();

                    PaginatedList<Submission> paginatedSubmissions = new PaginatedList<Submission>(set, pageNumber, pageSize, 50000);
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
                                IQueryable<Submission> submissions = (from message in db.Submissions
                                                                      where !message.IsArchived && !message.IsDeleted
                                                                   where !(from bu in db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                   join defaultsubverse in db.DefaultSubverses on message.Subverse equals defaultsubverse.Subverse
                                                                   select message).OrderByDescending(s => s.CreationDate);
                                return submissions.Where(s => s.StickiedSubmission.SubmissionID != s.ID).Skip(pageNumber * pageSize).Take(pageSize).ToList();
                            }
                        });

                        //now with new and improved locking
                        cacheData = CacheHandler.Register(cacheKey, getDataFunc, TimeSpan.FromMinutes(CONSTANTS.DEFAULT_GUEST_PAGE_CACHE_MINUTES), (pageNumber < 3 ? 0 : 3));
                    }
                    PaginatedList<Submission> paginatedSubmissions = new PaginatedList<Submission>((IList<Submission>)cacheData, pageNumber, pageSize, 50000);

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
            var stickiedSubmissions = _db.StickiedSubmissions.FirstOrDefault(s => s.Subverse == "announcements");

            if (stickiedSubmissions == null) return new EmptyResult();

            var stickiedSubmission = DataCache.Submission.Retrieve(stickiedSubmissions.SubmissionID);

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
                return PartialView("~/Views/Shared/Userprofile/_SidebarSubsUserModerates.cshtml", _db.SubverseModerators
                .Where(x => x.UserName == userName)
                .Select(s => new SelectListItem { Value = s.Subverse })
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
                return PartialView("~/Views/Shared/Userprofile/_SidebarSubsUserIsSubscribedTo.cshtml", _db.SubverseSubscriptions
                .Where(x => x.UserName == userName)
                .Select(s => new SelectListItem { Value = s.Subverse })
                .OrderBy(s => s.Value)
                .ToList()
                .AsEnumerable());
            }
            return new EmptyResult();
        }

        [OutputCache(Duration = 600, VaryByParam = "none")]
        public ActionResult FeaturedSub()
        {
            var featuredSub = _db.FeaturedSubverses.OrderByDescending(s => s.CreationDate).FirstOrDefault();

            if (featuredSub == null) return new EmptyResult();

            return PartialView("~/Views/Subverses/_FeaturedSub.cshtml", featuredSub);
        }
    }
}
