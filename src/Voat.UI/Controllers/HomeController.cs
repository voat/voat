#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Common.Configuration;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Query;
using Voat.Http.Filters;
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
        private readonly VoatOutOfRepositoryDataContextAccessor _db = new VoatOutOfRepositoryDataContextAccessor(CONSTANTS.CONNECTION_READONLY);

        // GET: submitlink
        [Authorize]
        public ActionResult SubmitLinkService(string selectedsubverse)
        {
            //CORE_PORT: Not Ported
            throw new NotImplementedException("Core port not implemented");
            /*
            string linkDescription = Request.Params["title"];
            string linkUrl = Request.Query["url"];

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
            */
        }
        // GET: submit
        [Authorize]
        public async Task<ActionResult> Submit(string selectedsubverse)
        {
            ViewBag.selectedSubverse = selectedsubverse;
            //CORE_PORT: Ported correctly?
            //string linkPost = Request.Params["linkpost"];
            //string title = Request.Params["title"];
            string linkPost = Request.Query["linkpost"];
            string title = Request.Query["title"];
            string url = Request.Query["url"];

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
                var q = new QuerySubverse(selectedsubverse);
                var subverse = await q.ExecuteAsync();
                if (subverse != null)
                {
                    model.Subverse = subverse.Name;
                    model.IsAdult = subverse.IsAdult;
                    model.IsAnonymized = subverse.IsAnonymized.HasValue ? subverse.IsAnonymized.Value : false;
                    model.AllowAnonymized = subverse.IsAnonymized == null || subverse.IsAnonymized.Value;
                }
            }

            var userData = UserData;
            model.RequireCaptcha = userData.Information.CommentPoints.Sum < VoatSettings.Instance.MinimumCommentPointsForCaptchaSubmission && VoatSettings.Instance.CaptchaEnabled;

            return View(model);
        }
       
        // POST: submit
        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(60)]
        public async Task<ActionResult> Submit(CreateSubmissionViewModel model)
        {
            //set this incase invalid submittal 
            var userData = UserData;
            model.RequireCaptcha = userData.Information.CommentPoints.Sum < VoatSettings.Instance.MinimumCommentPointsForCaptchaSubmission && VoatSettings.Instance.CaptchaEnabled;

            // abort if model state is invalid
            if (!ModelState.IsValid)
            {
                PreventSpamAttribute.Reset(this.HttpContext);
                return View(model);
            }

            //Check Captcha
            if (model.RequireCaptcha)
            {
                var captchaSuccess = await ReCaptchaUtility.Validate(Request);
                if (!captchaSuccess)
                {
                    ModelState.AddModelError(string.Empty, "Incorrect recaptcha answer");
                    return View(model);
                }
            }

            //new pipeline
            var userSubmission = new Domain.Models.UserSubmission();
            userSubmission.IsAdult = model.IsAdult;
            userSubmission.IsAnonymized = model.IsAnonymized;
            userSubmission.Subverse = model.Subverse.TrimSafe();

            userSubmission.Title = model.Title.StripWhiteSpace();
            userSubmission.Content = (model.Type == Domain.Models.SubmissionType.Text ? model.Content.TrimSafe() : null);
            userSubmission.Url = (model.Type == Domain.Models.SubmissionType.Link ? model.Url.TrimSafe() : null);

            var q = new CreateSubmissionCommand(userSubmission).SetUserContext(User);
            var result = await q.Execute();

            if (result.Success)
            {
                // redirect to comments section of newly posted submission
                return RedirectToRoute(
                    "SubverseCommentsWithSort_Short",
                    new
                    {
                        submissionID = result.Response.ID,
                        subverseName = result.Response.Subverse,
                        sort = (string)null

                    }
                    );
            }
            else
            {
                //Help formatting issues with unicode.
                if (model.Title.ContainsUnicode())
                {
                    model.Title = model.Title.StripUnicode();
                    ModelState.AddModelError(String.Empty, "Voat has strip searched your title and removed it's unicode. Please verify you approve of what you see.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.DebugMessage());
                }
                PreventSpamAttribute.Reset(HttpContext);
                return View(model);
            }
        }

        // GET: /v2
        //public ActionResult IndexV2(int? page)
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        return RedirectToAction("UnAuthorized", "Error");
        //    }

        //    ViewBag.SelectedSubverse = "frontpage";
        //    var submissions = new List<SetSubmission>();

        //    var frontPageResultModel = new SetFrontpageViewModel();

        //    // show user sets
        //    // get names of each set that user is subscribed to
        //    // for each set name, get list of subverses that define the set
        //    // for each subverse, get top ranked submissions
        //    if (User.Identity.IsAuthenticated && UserHelper.SetsSubscriptionCount(User.Identity.Name) > 0)
        //    {
        //        var userSetSubscriptions = _db.UserSetSubscriptions.Where(usd => usd.UserName == User.Identity.Name);
        //        var blockedSubverses = _db.UserBlockedSubverses.Where(x => x.UserName.Equals(User.Identity.Name)).Select(x => x.Subverse);

        //        foreach (var set in userSetSubscriptions)
        //        {
        //            UserSetSubscription setId = set;
        //            var userSetDefinition = _db.UserSetLists.Where(st => st.UserSetID == setId.UserSetID);

        //            foreach (var subverse in userSetDefinition)
        //            {
        //                // get top ranked submissions
        //                var submissionsExcludingBlockedSubverses = SetsUtility.TopRankedSubmissionsFromASub(subverse.Subverse, _db.Submissions, subverse.UserSet.Name, 2, 0)
        //                    .Where(x => !blockedSubverses.Contains(x.Subverse));
        //                submissions.AddRange(submissionsExcludingBlockedSubverses);
        //            }
        //        }

        //        frontPageResultModel.HasSetSubscriptions = true;
        //        frontPageResultModel.UserSets = userSetSubscriptions;
        //        frontPageResultModel.SubmissionsList = new List<SetSubmission>(submissions.OrderByDescending(s => s.Rank));

        //        return View("~/Views/Home/IndexV2.cshtml", frontPageResultModel);
        //    }

        //    // show default sets since user is not logged in or has no set subscriptions
        //    // get names of default sets
        //    // for each set name, get list of subverses
        //    // for each subverse, get top ranked submissions
        //    var defaultSets = _db.UserSets.Where(ds => ds.IsDefault && ds.UserSetLists.Any());
        //    var defaultFrontPageResultModel = new SetFrontpageViewModel();

        //    foreach (var set in defaultSets)
        //    {
        //        UserSet setId = set;
        //        var defaultSetDefinition = _db.UserSetLists.Where(st => st.UserSetID == setId.ID);

        //        foreach (var subverse in defaultSetDefinition)
        //        {
        //            // get top ranked submissions
        //            submissions.AddRange(SetsUtility.TopRankedSubmissionsFromASub(subverse.Subverse, _db.Submissions, set.Name, 2, 0));
        //        }
        //    }

        //    defaultFrontPageResultModel.DefaultSets = defaultSets;
        //    defaultFrontPageResultModel.HasSetSubscriptions = false;
        //    defaultFrontPageResultModel.SubmissionsList = new List<SetSubmission>(submissions.OrderByDescending(s => s.Rank));

        //    return View("~/Views/Home/IndexV2.cshtml", defaultFrontPageResultModel);
        //}

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

        // GET: list of subverses user is subscribed to for sidebar
        
        public async Task<ActionResult> SubversesUserIsSubscribedTo(string userName)
        {
            if (userName != null)
            {
                var q = new QueryUserSubscriptions(userName);
                var result = await q.ExecuteAsync();
                var subs = result[Domain.Models.DomainType.Subverse];
                var selectList = subs.OrderBy(x => x).Select(x => new SelectListItem() { Value = x }).ToList();

                return PartialView("~/Views/Shared/Userprofile/_SidebarSubsUserIsSubscribedTo.cshtml",
                    selectList
                );

                //return PartialView("~/Views/Shared/Userprofile/_SidebarSubsUserIsSubscribedTo.cshtml", 
                //    _db.SubverseSubscriptions
                //.Where(x => x.UserName == userName)
                //.Select(s => new SelectListItem { Value = s.Subverse })
                //.OrderBy(s => s.Value)
                //.ToList()
                //.AsEnumerable());
            }
            return new EmptyResult();
        }
        
        [HttpGet]
        public ActionResult Rules()
        {
            var ruleinfo = RuleDiscoveryProvider.GetDescriptions(Assembly.GetAssembly(typeof(VoatRuleContext))).Where(x => x.Enabled).Select(x => new RuleInformationWithOutcome(x)).OrderBy(x => x.Info.Rule.Number).ToList();
            RuleOutcome unevaluated = new RuleOutcome(RuleResult.Unevaluated, "UnevaluatedRule", "0.0", "This rule is unevaluated.");

            if (User.Identity.IsAuthenticated)
            {
                var context = new VoatRuleContext(User);

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
