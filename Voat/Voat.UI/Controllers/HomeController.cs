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
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Voat.Caching;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Query;
using Voat.Models;
using Voat.Models.ViewModels;
using Voat.Rules;
using Voat.RulesEngine;
using Voat.UI.Utilities;
using Voat.Utilities;

namespace Voat.Controllers
{
    public class HomeController : BaseController
    {
        //IAmAGate: Move queries to read-only mirror
        private readonly voatEntities _db = new voatEntities(true);

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

            if (selectedsubverse == null)
                return View("~/Views/Home/Submit.cshtml");
            if (!selectedsubverse.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.selectedSubverse = selectedsubverse;
            }

            return View("~/Views/Home/Submit.cshtml");
        }
        // GET: submit
        [Authorize]
        public ActionResult Submit(string selectedsubverse)
        {
            ViewBag.selectedSubverse = selectedsubverse;

            string linkPost = Request.Params["linkpost"];
            string title = Request.Params["title"];
            string url = Request.QueryString["url"];

            CreateSubmissionViewModel model = new CreateSubmissionViewModel();
            model.Title = title;

            if (linkPost != null && linkPost == "true")
            {
                model.Type = Domain.Models.SubmissionType.Link;
                model.Url = url;
            }
            else
            {
                model.Type = Domain.Models.SubmissionType.Text;
                model.Url = "https://voat.co"; //dummy for validation (Do not look at me like that)
            }

            if (!String.IsNullOrWhiteSpace(selectedsubverse))
            {
                model.Subverse = selectedsubverse;
            }

            var userData = UserData;
            model.RequireCaptcha = userData.Information.CommentPoints.Sum < 25 && !Settings.CaptchaDisabled;

            return View("Submit", model);
        }
       
        // POST: submit
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(DelayRequest = 60, ErrorMessage = "Sorry, you are doing that too fast. Please try again in 60 seconds.")]
        public async Task<ActionResult> Submit(CreateSubmissionViewModel model)
        {
            //set this incase invalid submittal 
            var userData = UserData;
            model.RequireCaptcha = userData.Information.CommentPoints.Sum < 25 && !Settings.CaptchaDisabled;

            // abort if model state is invalid
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //Check Captcha
            if (model.RequireCaptcha)
            {
                var captchaSuccess = await ReCaptchaUtility.Validate(Request);
                if (!captchaSuccess)
                {
                    ModelState.AddModelError(string.Empty, "Incorrect recaptcha answer");
                    return View("Submit");
                }
            }

            //new pipeline
            var userSubmission = new Domain.Models.UserSubmission();
            userSubmission.Subverse = model.Subverse;
            userSubmission.Title = model.Title;
            userSubmission.Content = (model.Type == Domain.Models.SubmissionType.Text ? model.Content : null);
            userSubmission.Url = (model.Type == Domain.Models.SubmissionType.Link ? model.Url : null);

            var q = new CreateSubmissionCommand(userSubmission);
            var result = await q.Execute();

            if (result.Success)
            {
                // redirect to comments section of newly posted submission
                return RedirectToRoute(
                    "SubverseCommentsWithSort_Short",
                    new
                    {
                        submissionID = result.Response.ID,
                        subverseName = result.Response.Subverse
                    }
                    );
            }
            else
            {
                //Help formatting issues with unicode.
                if (Formatting.ContainsUnicode(model.Title))
                {
                    ModelState.AddModelError(string.Empty, "Voat has strip searched your title and removed it's unicode. Please verify you approve of what you see.");
                    model.Title = Formatting.StripUnicode(model.Title);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                }
                PreventSpamAttribute.Reset();
                return View("Submit", model);
            }
        }

        // GET: /
        public ActionResult Index(int? page)
        {
            ViewBag.SelectedSubverse = "frontpage";

            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return NotFoundErrorView();
            }

            // show only submissions from subverses that user is subscribed to if user is logged in
            // also do a check so that user actually has subscriptions
            if (User.Identity.IsAuthenticated && UserData.HasSubscriptions() && Request.QueryString["frontpage"] != "guest")
            {
                //IAmAGate: Perf mods for caching
                int pagesToTake = 2;
                int subset = pageNumber / pagesToTake;
                string cacheKey = String.Format("legacy:front:{0}:block.{1}.sort.rank", User.Identity.Name, subset);
                object cacheData = CacheHandler.Instance.Retrieve<object>(cacheKey);

                if (cacheData == null)
                {
                    int recordsToTake = pageSize * pagesToTake; //4 pages worth
                    var userName = User.Identity.Name;
                    var getDataFunc = new Func<object>(() =>
                    {
                        using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                        {
                            db.EnableCacheableOutput();

                            var blockedSubverses = db.UserBlockedSubverses.Where(x => x.UserName.Equals(userName)).Select(x => x.Subverse);

                            // TODO: check if user wants to exclude downvoted submissions from frontpage
                            var downvotedSubmissionIds = db.SubmissionVoteTrackers.AsNoTracking().Where(vt => vt.UserName.Equals(userName) && vt.VoteStatus == -1).Select(s => s.SubmissionID);

                            IQueryable<Submission> submissions = (from m in db.Submissions.Include("Subverse").AsNoTracking()
                                                                  join s in db.SubverseSubscriptions on m.Subverse equals s.Subverse
                                                                  where !m.IsArchived && !m.IsDeleted && s.UserName == userName
                                                                  where !(from bu in db.BannedUsers select bu.UserName).Contains(m.UserName)
                                                                  select m).OrderByDescending(s => s.Rank);

                            var submissionsWithoutStickies = submissions.Where(s => s.StickiedSubmission.SubmissionID != s.ID);

                            // exclude downvoted submissions
                            var submissionsWithoutDownvotedSubmissions = submissionsWithoutStickies.Where(x => !downvotedSubmissionIds.Contains(x.ID));

                            return submissionsWithoutDownvotedSubmissions.Where(x => !blockedSubverses.Contains(x.Subverse)).Skip(subset * recordsToTake).Take(recordsToTake).ToList();
                        }
                    });

                    //now with new and improved locking
                    cacheData = CacheHandler.Instance.Register(cacheKey, getDataFunc, TimeSpan.FromMinutes(5));
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

        public static IList<Submission> GetGuestFrontPage(int pageSize, int pageNumber)
        {
            //IAmAGate: Perf mods for caching
            string cacheKey = String.Format("legacy:front:guest:page.{0}.sort.rank", pageNumber);
            IList<Submission> cacheData = CacheHandler.Instance.Retrieve<IList<Submission>>(cacheKey);
            if (cacheData == null)
            {
                var getDataFunc = new Func<IList<Submission>>(() =>
                {
                    using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                    {
                        db.EnableCacheableOutput();

                        // get only submissions from default subverses not older than 24 hours, order by relative rank
                        var startDate = Repository.CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));

                        IQueryable<Submission> submissions = (from message in db.Submissions.AsNoTracking()
                                                              where !message.IsArchived && !message.IsDeleted && (message.UpCount - message.DownCount >= 20) && message.CreationDate >= startDate && message.CreationDate <= Repository.CurrentDate
                                                              where !(from bu in db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                              join defaultsubverse in db.DefaultSubverses on message.Subverse equals defaultsubverse.Subverse
                                                              select message).OrderByDescending(s => s.RelativeRank);

                        return submissions.Where(s => s.StickiedSubmission.SubmissionID != s.ID).Skip(pageNumber * pageSize).Take(pageSize).ToList();
                    }
                });

                //Now with it's own locking!
                cacheData = CacheHandler.Instance.Register(cacheKey, getDataFunc, TimeSpan.FromMinutes(CONSTANTS.DEFAULT_GUEST_PAGE_CACHE_MINUTES), (pageNumber < 3 ? 0 : 3));
            }

            return cacheData;
        }

        // GET: /v2
        public ActionResult IndexV2(int? page)
        {
            if (Settings.SetsDisabled)
            {
                return RedirectToAction("UnAuthorized", "Error");
            }

            ViewBag.SelectedSubverse = "frontpage";
            var submissions = new List<SetSubmission>();

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

        // GET: /new
        public ActionResult @New(int? page, string sortingmode)
        {
            // sortingmode: new, contraversial, hot, etc
            ViewBag.SortingMode = sortingmode;

            if (!sortingmode.Equals("new"))
                return RedirectToAction("Index", "Home");

            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return NotFoundErrorView();
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
                hc.Expires = Repository.CurrentDate.AddYears(1);
                System.Web.HttpContext.Current.Response.Cookies.Add(hc);

                ViewBag.FirstTimeVisitor = true;
            }
            var userData = Voat.Domain.UserData.GetContextUserData();
            // show only submissions from subverses that user is subscribed to if user is logged in
            // also do a check so that user actually has subscriptions
            if (User.Identity.IsAuthenticated && userData.HasSubscriptions())
            {
                //IAmAGate: Perf mods for caching
                int pagesToTake = 2;
                int subset = pageNumber / pagesToTake;
                string cacheKey = String.Format("legacy:front:{0}:block.{1}.sort.new", User.Identity.Name, subset);
                object cacheData = CacheHandler.Instance.Retrieve<object>(cacheKey);

                if (cacheData == null)
                {
                    int recordsToTake = 25 * pagesToTake; //pages worth
                    var userName = User.Identity.Name;
                    var getDataFunc = new Func<object>(() =>
                    {
                        using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                        {
                            db.EnableCacheableOutput();

                            var blockedSubverses = db.UserBlockedSubverses.Where(x => x.UserName.Equals(userName)).Select(x => x.Subverse);
                            IQueryable<Submission> submissions = (from m in db.Submissions
                                                                  join s in db.SubverseSubscriptions on m.Subverse equals s.Subverse
                                                                  where !m.IsArchived && !m.IsDeleted && s.UserName == userName
                                                                  where !(from bu in db.BannedUsers select bu.UserName).Contains(m.UserName)
                                                                  select m).OrderByDescending(s => s.CreationDate);
                            return submissions.Where(x => !blockedSubverses.Contains(x.Subverse)).Skip(subset * recordsToTake).Take(recordsToTake).ToList();
                        }
                    });

                    //now with new and improved locking
                    cacheData = CacheHandler.Instance.Register(cacheKey, getDataFunc, TimeSpan.FromMinutes(5), 1);
                }
                var set = ((IList<Submission>)cacheData).Skip((pageNumber - (subset * pagesToTake)) * pageSize).Take(pageSize).ToList();

                PaginatedList<Submission> paginatedSubmissions = new PaginatedList<Submission>(set, pageNumber, pageSize, 50000);
                return View("Index", paginatedSubmissions);
            }
            else
            {
                //IAmAGate: Perf mods for caching
                string cacheKey = String.Format("legacy:front:guest:page.{0}.sort.new", pageNumber);
                object cacheData = CacheHandler.Instance.Retrieve<object>(cacheKey);

                if (cacheData == null)
                {
                    var getDataFunc = new Func<object>(() =>
                    {
                        using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                        {
                            db.EnableCacheableOutput();

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
                    cacheData = CacheHandler.Instance.Register(cacheKey, getDataFunc, TimeSpan.FromMinutes(CONSTANTS.DEFAULT_GUEST_PAGE_CACHE_MINUTES), (pageNumber < 3 ? 0 : 3));
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

        // GET: /advertize
        public ActionResult Advertize()
        {
            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Help/Advertize.cshtml");
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

        //[OutputCache(Duration = 600)]
        public ActionResult StickiedSubmission()
        {
            Submission sticky = StickyHelper.GetSticky("announcements");

            if (sticky != null)
            {
                return PartialView("~/Views/Subverses/_Stickied.cshtml", sticky);
            }
            else
            {
                return new EmptyResult();
            }
        }

        // GET: list of subverses user moderates
        //[OutputCache(Duration = 600, VaryByParam = "*")]
        public ActionResult SubversesUserModerates(string userName)
        {
            if (userName != null)
            {
                //This is expensive to hydrate the userData.Information for the moderation list
                var userData = new Domain.UserData(userName);
                return PartialView("~/Views/Shared/Userprofile/_SidebarSubsUserModerates.cshtml", userData.Information.Moderates);
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

            if (featuredSub == null)
                return new EmptyResult();

            return PartialView("~/Views/Subverses/_FeaturedSub.cshtml", featuredSub);
        }

        [HttpGet]
        public ActionResult Rules()
        {
            var ruleinfo = RuleDiscoveryProvider.GetDescriptions(Assembly.GetAssembly(typeof(VoatRuleContext))).Where(x => x.Enabled).Select(x => new RuleInformationWithOutcome(x)).OrderBy(x => x.Info.Rule.Number).ToList();
            RuleOutcome unevaluated = new RuleOutcome(RuleResult.Unevaluated, "UnevaluatedRule", "0.0", "This rule is unevaluated.");

            if (User.Identity.IsAuthenticated)
            {
                var context = new VoatRuleContext();
                context.UserName = User.Identity.Name;

                //run every rule we can for the current user
                foreach (var rule in VoatRulesEngine.Instance.Rules)
                {
                    var info = ruleinfo.FirstOrDefault(x => x.Info.Rule.Name == rule.Name && x.Info.Rule.Number == rule.Number);
                    if (info != null)
                    {
                        RuleOutcome outcome = null;
                        if (((Rule<VoatRuleContext>)rule).TryEvaluate(context, out outcome))
                        {
                            info.Outcome = outcome;
                        }
                        else
                        {
                            info.Outcome = unevaluated;
                        }
                    }
                }
            }
            return View("Rules", ruleinfo);
        }
    }
}
