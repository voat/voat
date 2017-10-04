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

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Voat.Models;
using Voat.Models.ViewModels;

using System.Collections.Generic;
using Voat.Data.Models;
using Voat.Utilities;
using Voat.UI.Utilities;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Query;
using Voat.Domain.Command;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voat.Common;
using Voat.Http;
using Voat.Http.Filters;
using Microsoft.Extensions.Logging;

namespace Voat.Controllers
{
    public class SubversesController : BaseController
    {
        //IAmAGate: Move queries to read-only mirror
        private readonly VoatOutOfRepositoryDataContextAccessor _db = new VoatOutOfRepositoryDataContextAccessor(CONSTANTS.CONNECTION_LIVE);
        private ILogger _logger;

        public SubversesController(ILogger<SubversesController> logger)
        {
            _logger = logger;
        }

        // POST: Create a new Subverse
        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(300)]
        public async Task<ActionResult> CreateSubverse([Bind("Name, Title, Description, Type, Sidebar, CreationDate, Owner")] AddSubverse subverseTmpModel)
        {
            // abort if model state is invalid
            if (!ModelState.IsValid)
            {
                PreventSpamAttribute.Reset(HttpContext);
                return View(subverseTmpModel);
            }

            var title = subverseTmpModel.Title;
            if (String.IsNullOrEmpty(title))
            {
                title = $"/v/{subverseTmpModel.Name}"; //backwards compatibility, previous code always uses this
            }

            var cmd = new CreateSubverseCommand(subverseTmpModel.Name, title, subverseTmpModel.Description, subverseTmpModel.Sidebar).SetUserContext(User);
            var respones = await cmd.Execute();
            if (respones.Success)
            {
                return RedirectToRoute(Models.ROUTE_NAMES.SUBVERSE_INDEX, new { subverse = subverseTmpModel.Name });
            }
            else
            {
                PreventSpamAttribute.Reset(HttpContext);
                ModelState.AddModelError(string.Empty, respones.DebugMessage());
                return View(subverseTmpModel);
            }
            
        }

        // GET: create
        [Authorize]
        public ActionResult CreateSubverse()
        {
            return View();
        }

       

   
        //// GET: sidebar for selected subverse
        //public ActionResult DetailsForSelectedSubverse(string selectedSubverse)
        //{
        //    var subverse = DataCache.Subverse.Retrieve(selectedSubverse);

        //    if (subverse == null)
        //        return new EmptyResult();

        //    // get subscriber count for selected subverse
        //    //var subscriberCount = _db.SubverseSubscriptions.Count(r => r.Subverse.Equals(selectedSubverse, StringComparison.OrdinalIgnoreCase));

        //    //ViewBag.SubscriberCount = subscriberCount;
        //    ViewBag.SelectedSubverse = selectedSubverse;
        //    return PartialView("_SubverseDetails", subverse);

        //    //don't return a sidebar since subverse doesn't exist or is a system subverse
        //}


        public ActionResult Subversenotfound()
        {
            ViewBag.SelectedSubverse = "404";
            return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
        }

        public ActionResult AdultContentFiltered(string destination)
        {
            ViewBag.SelectedSubverse = destination;
            return View("~/Views/Subverses/AdultContentFiltered.cshtml");
        }

        public ActionResult AdultContentWarning(string destination, bool? nsfwok)
        {
            ViewBag.SelectedSubverse = String.Empty;

            if (destination == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (nsfwok != null && nsfwok == true)
            {
                // setup nswf cookie
                HttpCookie hc = new HttpCookie("NSFWEnabled", "1");
                hc.Expires = Repository.CurrentDate.AddYears(1);
                Response.Cookies.Add(hc);

                // redirect to destination subverse
                return RedirectToRoute(Models.ROUTE_NAMES.SUBVERSE_INDEX, new { subverse = destination });
            }
            ViewBag.Destination = destination;
            return View("~/Views/Subverses/AdultContentWarning.cshtml");
        }

        // GET: fetch a random subbverse with x subscribers and x submissions
        public async Task<ActionResult> Random()
        {
            try
            {
                var q = new QueryRandomSubverse(false);
                var randomSubverse = await q.ExecuteAsync();

                if (!String.IsNullOrEmpty(randomSubverse))
                {
                    return RedirectToRoute(ROUTE_NAMES.SUBVERSE_INDEX, new { subverse = randomSubverse });
                }
                else
                {
                    return RedirectToRoute(ROUTE_NAMES.FRONT_INDEX);
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // GET: fetch a random NSFW subbverse with x subscribers and x submissions
        public async Task<ActionResult> RandomNsfw()
        {
            try
            {
                var q = new QueryRandomSubverse(true);
                var randomSubverse = await q.ExecuteAsync();

                if (!String.IsNullOrEmpty(randomSubverse))
                {
                    return RedirectToRoute(ROUTE_NAMES.SUBVERSE_INDEX, new { subverse = randomSubverse });
                }
                else
                {
                    return RedirectToRoute(ROUTE_NAMES.FRONT_INDEX);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<ContentResult> Stylesheet(string subverse, bool cache = true, bool minimized = true)
        {
            var policy = (cache ? new CachePolicy(TimeSpan.FromMinutes(30)) : CachePolicy.None);
            var q = new QuerySubverseStylesheet(subverse, policy);

            var madStylesYo = await q.ExecuteAsync();

            return new ContentResult()
            {
                Content = (minimized ? madStylesYo.Minimized : madStylesYo.Raw),
                ContentType = "text/css"
            };
        }

        //// POST: subscribe to a subverse
        //[Authorize]
        //public async Task<JsonResult> Subscribe(string subverseName)
        //{
        //    var cmd = new SubscribeCommand(new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, subverseName), Domain.Models.SubscriptionAction.Subscribe);
        //    var r = await cmd.Execute();
        //    if (r.Success)
        //    {
        //        return Json("Subscription request was successful." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //    }
        //    else
        //    {
        //        return Json(r.Message /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //    }
        //}

        // POST: unsubscribe from a subverse
        [Authorize]
        public async Task<JsonResult> UnSubscribe(string subverseName)
        {
            //var loggedInUser = User.Identity.Name;

            //Voat.Utilities.UserHelper.UnSubscribeFromSubverse(loggedInUser, subverseName);
            //return Json("Unsubscribe request was successful." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
            var cmd = new SubscribeCommand(new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, subverseName), Domain.Models.SubscriptionAction.Unsubscribe).SetUserContext(User);
            var r = await cmd.Execute();
            if (r.Success)
            {
                return Json("Unsubscribe request was successful." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
            }
            else
            {
                return Json(r.Message /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
            }

        }
        
        // POST: block a subverse
        [Authorize]
        public async Task<JsonResult> BlockSubverse(string subverseName)
        {
            var loggedInUser = User.Identity.Name;
            var cmd = new BlockCommand(Domain.Models.DomainType.Subverse, subverseName).SetUserContext(User);
            var response = await cmd.Execute();

            if (response.Success)
            {
                return Json("Subverse block request was successful." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
            }
            else
            {
                Response.StatusCode = 400;
                return Json(response.Message /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
            }
        }

        #region Submission Display Methods

        private void SetFirstTimeCookie()
        {
            // setup a cookie to find first time visitors and display welcome banner
            const string cookieName = "NotFirstTime";
            if (ControllerContext.HttpContext.Request.Cookies.ContainsKey(cookieName))
            {
                // not a first time visitor
                ViewBag.FirstTimeVisitor = false;
            }
            else
            {
                // add a cookie for first time visitors
                HttpCookie hc = new HttpCookie("NotFirstTime", "1");
                hc.Expires = Repository.CurrentDate.AddYears(1);
                Response.Cookies.Add(hc);

                ViewBag.FirstTimeVisitor = true;
            }
        }

        // GET: show a subverse index
        public async Task<ActionResult> SubverseIndex(string subverse, string sort = "hot", bool? previewMode = null)
        {
            const string cookieName = "NSFWEnabled";
           
            var viewProperties = new ListViewModel<Domain.Models.Submission>();
            viewProperties.PreviewMode = previewMode ?? false;
            ViewBag.PreviewMode = viewProperties.PreviewMode;

            //Set to DEFAULT if querystring is present
            if (Request.Query["frontpage"] == "guest")
            {
                subverse = AGGREGATE_SUBVERSE.DEFAULT;
            }
            if (String.IsNullOrEmpty(subverse))
            {
                subverse = AGGREGATE_SUBVERSE.FRONT;
            }

            SetFirstTimeCookie();
            var logVisit = new LogVisitCommand(subverse, null, IpHash.CreateHash(Request.RemoteAddress())).SetUserContext(User);
            await logVisit.Execute();

            //Parse query
            var options = new SearchOptions(Request.QueryString.Value);

            //Set sort because it is part of path
            if (!String.IsNullOrEmpty(sort))
            {
                options.Sort = (Domain.Models.SortAlgorithm)Enum.Parse(typeof(Domain.Models.SortAlgorithm), sort, true);
            }

            //set span to day if not specified explicitly 
            if (options.Sort == Domain.Models.SortAlgorithm.Top && !Request.Query.ContainsKey("span"))
            {
                options.Span = Domain.Models.SortSpan.Day;
            }
            //reset count incase they try to change it with querystrings those sneaky snakes
            options.Count = 25;

            viewProperties.Sort = options.Sort;
            viewProperties.Span = options.Span;
            var routeName = ROUTE_NAMES.SUBVERSE_INDEX;
            try
            {
                PaginatedList<Domain.Models.Submission> pageList = null;

                if (AGGREGATE_SUBVERSE.IsAggregate(subverse))
                {
                    if (AGGREGATE_SUBVERSE.FRONT.IsEqual(subverse))
                    {
                        //Check if user is logged in and has subscriptions, if not we convert to default query
                        if (!User.Identity.IsAuthenticated || (User.Identity.IsAuthenticated && !UserData.HasSubscriptions()))
                        {
                            subverse = AGGREGATE_SUBVERSE.DEFAULT;
                        }
                        else
                        {
                            subverse = AGGREGATE_SUBVERSE.FRONT;
                        }
                        //viewProperties.Title = "Front";
                        //ViewBag.SelectedSubverse = "frontpage";
                    }
                    else if (AGGREGATE_SUBVERSE.DEFAULT.IsEqual(subverse))
                    {
                        //viewProperties.Title = "Front";
                        //ViewBag.SelectedSubverse = "frontpage";
                    }
                    else
                    {
                        // selected subverse is ALL, show submissions from all subverses, sorted by rank
                        viewProperties.Title = "all subverses";
                        if (AGGREGATE_SUBVERSE.ANY.IsEqual(subverse))
                        {
                            viewProperties.Context = new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, AGGREGATE_SUBVERSE.ANY);
                            subverse = AGGREGATE_SUBVERSE.ANY;
                        }
                        else
                        {
                            viewProperties.Context = new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, "all");
                            subverse = AGGREGATE_SUBVERSE.ALL;
                        }
                    }
                }
                else
                {
                    // check if subverse exists, if not, send to a page not found error
                    //Can't use cached, view using to query db
                    var subverseObject = _db.Subverse.FirstOrDefault(x => x.Name.ToLower() == subverse.ToLower());

                    if (subverseObject == null)
                    {
                        ViewBag.SelectedSubverse = "404";
                        return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
                    }

                    //Set to proper cased name
                    subverse = subverseObject.Name;

                    //HACK: Disable subverse
                    if (subverseObject.IsAdminDisabled.HasValue && subverseObject.IsAdminDisabled.Value)
                    {
                        //viewProperties.Subverse = subverseObject.Name;
                        ViewBag.Subverse = subverseObject.Name;
                        return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseDisabled));
                    }

                    //Check NSFW Settings
                    if (subverseObject.IsAdult)
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            if (!UserData.Preferences.EnableAdultContent)
                            {
                                // display a view explaining that account preference is set to NO NSFW and why this subverse can not be shown
                                return RedirectToAction("AdultContentFiltered", "Subverses", new { destination = subverseObject.Name });
                            }
                        }
                        // check if user wants to see NSFW content by reading NSFW cookie
                        else if (!ControllerContext.HttpContext.Request.Cookies.ContainsKey(cookieName))
                        {
                            return RedirectToAction("AdultContentWarning", "Subverses", new { destination = subverseObject.Name, nsfwok = false });
                        }
                    }

                    viewProperties.Context = new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse,  subverseObject.Name);
                    viewProperties.Title = subverseObject.Title;
                    routeName = ROUTE_NAMES.SUBVERSE_INDEX;
                }
                //what to query
                var domainReference = new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, subverse);

                var q = new QuerySubmissions(domainReference, options).SetUserContext(User);
                var results = await q.ExecuteAsync().ConfigureAwait(false);

                viewProperties.Context = domainReference;
                pageList = new PaginatedList<Domain.Models.Submission>(results, options.Page, options.Count, -1);
                pageList.RouteName = routeName;
                viewProperties.Items = pageList;

                var navModel = new NavigationViewModel()
                {
                    Description = "Subverse",
                    Name = subverse,
                    MenuType = MenuType.Subverse,
                    BasePath = "/v/" + subverse,
                    Sort = null
                };

                //Backwards compat with Views
                if (subverse == AGGREGATE_SUBVERSE.FRONT || subverse == AGGREGATE_SUBVERSE.DEFAULT)
                {
                    navModel.BasePath = "";
                    navModel.Name = "";
                    //ViewBag.SelectedSubverse = "frontpage";
                    viewProperties.Context.Name = "";
                    pageList.RouteName = Models.ROUTE_NAMES.FRONT_INDEX;
                }
                else if (subverse == AGGREGATE_SUBVERSE.ALL || subverse == AGGREGATE_SUBVERSE.ANY)
                {
                    navModel.BasePath = "/v/all";
                    navModel.Name = "All";
                    ViewBag.SelectedSubverse = "all";
                    viewProperties.Context.Name = "all";
                }
                else 
                {
                    ViewBag.SelectedSubverse = subverse;
                }
                ViewBag.SortingMode = sort;

                ViewBag.NavigationViewModel = navModel;
                var viewPath = ViewPath(domainReference);

                return View(viewPath, viewProperties);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region random subverse
        [Obsolete("Original Random Sub Query", true)]
        public string RandomSubverse(bool sfw)
        {
            throw new Exception("This method does not play nice with others");

            #region Old Logic (Preserved for the Lulz)
            /*
            // fetch a random subverse with minimum number of subscribers where last subverse activity was evident
            IQueryable<Subverse> subverse;
            if (sfw)
            {
                subverse = from subverses in
                               _db.Subverses
                                   .Where(s => s.SubscriberCount > 10 && !s.Name.Equals("all", StringComparison.OrdinalIgnoreCase) && s.LastSubmissionDate != null
                                   && !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(s.Name) select ubs.UserName).Contains(User.Identity.Name)
                                   && !s.IsAdult
                                   && !s.IsAdminDisabled.Value)
                           select subverses;
            }
            else
            {
                subverse = from subverses in
                               _db.Subverses
                                   .Where(s => s.SubscriberCount > 10 && !s.Name.Equals("all", StringComparison.OrdinalIgnoreCase)
                                               && s.LastSubmissionDate != null
                                               && !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(s.Name) select ubs.UserName).Contains(User.Identity.Name)
                                               && s.IsAdult
                                               && !s.IsAdminDisabled.Value)
                           select subverses;
            }

            var submissionCount = 0;
            Subverse randomSubverse;

            do
            {
                var count = subverse.Count(); // 1st round-trip
                var index = new Random().Next(count);

                randomSubverse = subverse.OrderBy(s => s.Name).Skip(index).FirstOrDefault(); // 2nd round-trip

                var submissions = _db.Submissions
                        .Where(x => x.Subverse == randomSubverse.Name && !x.IsDeleted)
                        .OrderByDescending(s => s.Rank)
                        .Take(50)
                        .ToList();

                if (submissions.Count > 9)
                {
                    submissionCount = submissions.Count;
                }
            } while (submissionCount == 0);

            return randomSubverse != null ? randomSubverse.Name : "all";
            */

            #endregion
        }

        #endregion random subverse


        #region Discovery Methods

        //// GET: show subverse search view
        //public ActionResult Search()
        //{
        //    ViewBag.NavigationViewModel = new NavigationViewModel()
        //    {
        //        Description = "Search Subverses",
        //        Name = "Subverses",
        //        MenuType = MenuType.Discovery,
        //        BasePath = null,
        //        Sort = null
        //    };

        //    return View("~/Views/Subverses/SearchForSubverse.cshtml", new SearchSubverseViewModel());
        //}

        //// GET: show a list of subverses by number of subscribers
        //public ActionResult Subverses(int? page)
        //{
        //    //ViewBag.SelectedSubverse = "subverses";
        //    const int pageSize = 25;
        //    int pageNumber = (page ?? 0);

        //    if (pageNumber < 0)
        //    {
        //        return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
        //    }

        //    try
        //    {
        //        // order by subscriber count (popularity)
        //        var subverses = _db.Subverses.OrderByDescending(s => s.SubscriberCount);

        //        var paginatedSubverses = new PaginatedList<Subverse>(subverses, page ?? 0, pageSize);

        //        ViewBag.NavigationViewModel = new NavigationViewModel()
        //        {
        //            Description = "Popular Subverses",
        //            Name = "Subverses",
        //            MenuType = MenuType.Discovery,
        //            BasePath = null,
        //            Sort = null
        //        };

        //        return View(paginatedSubverses);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //// GET: list of subverses user is subscribed to, used in hover menu
        //public async Task<ActionResult> ListOfSubversesUserIsSubscribedTo()
        //{
        //    var q = new QueryUserSubscriptions(User.Identity.Name);
        //    var results = await q.ExecuteAsync();
        //    var subs = results[Domain.Models.DomainType.Subverse];
        //    subs = subs.OrderBy(x => x);

        //    //// show custom list of subverses in top menu
        //    //var listOfSubverses = _db.SubverseSubscriptions
        //    //    .Where(s => s.UserName == User.Identity.Name)
        //    //    .OrderBy(s => s.Subverse);

        //    return PartialView("_ListOfSubverses", subs);
        //}

        // GET: list of default subverses
        //public ActionResult ListOfDefaultSubverses()
        //{
        //    try
        //    {
        //        var q = new QueryDefaultSubverses();
        //        var r = q.Execute();
        //        var names = r.Select(x => x.Name).ToList();

        //        return PartialView("_ListOfSubverses", names);
        //    }
        //    catch (Exception)
        //    {
        //        return new EmptyResult();
        //    }
        //}

        [Authorize]
        public ActionResult SubversesSubscribed(int? page)
        {
            //DISCOVERY METHOD 
            return RedirectToAction("Details", "Set", new { name = Domain.Models.SetType.Front.ToString(), userName = User.Identity.Name });

            //ViewBag.SelectedSubverse = "subverses";
            //ViewBag.SubversesView = "subscribed";
            //const int pageSize = 25;
            //int pageNumber = (page ?? 0);

            //if (pageNumber < 0)
            //{
            //    return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            //}

            //// get a list of subcribed subverses with details and order by subverse names, ascending
            //IQueryable<SubverseDetailsViewModel> subscribedSubverses = from c in _db.Subverses
            //                                                           join a in _db.SubverseSubscriptions
            //                                                           on c.Name equals a.Subverse
            //                                                           where a.UserName.Equals(User.Identity.Name)
            //                                                           orderby a.Subverse ascending
            //                                                           select new SubverseDetailsViewModel
            //                                                           {
            //                                                               Name = c.Name,
            //                                                               Title = c.Title,
            //                                                               Description = c.Description,
            //                                                               CreationDate = c.CreationDate,
            //                                                               Subscribers = c.SubscriberCount
            //                                                           };

            //var paginatedSubscribedSubverses = new PaginatedList<SubverseDetailsViewModel>(subscribedSubverses, page ?? 0, pageSize);

            //return View("SubscribedSubverses", paginatedSubscribedSubverses);
        }

        //// GET: show a list of subverses by creation date
        //public ViewResult NewestSubverses(int? page, string sortingmode)
        //{

        //    //DISCOVERY METHOD 
        //    //ViewBag.SelectedSubverse = "subverses";
        //    ViewBag.SortingMode = sortingmode;

        //    const int pageSize = 25;
        //    int pageNumber = (page ?? 0);

        //    if (pageNumber < 0)
        //    {
        //        return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
        //    }

        //    var subverses = _db.Subverses.Where(s => s.Description != null).OrderByDescending(s => s.CreationDate);

        //    var paginatedNewestSubverses = new PaginatedList<Subverse>(subverses, page ?? 0, pageSize);

        //    ViewBag.NavigationViewModel = new NavigationViewModel()
        //    {
        //        Description = "Newest Subverses",
        //        Name = "Subverses",
        //        MenuType = MenuType.Discovery,
        //        BasePath = null,
        //        Sort = null
        //    };

        //    return View("~/Views/Subverses/Subverses.cshtml", paginatedNewestSubverses);
        //}

        //// show subverses ordered by last received submission
        //public ViewResult ActiveSubverses(int? page)
        //{
        //    //ViewBag.SelectedSubverse = "subverses";
        //    ViewBag.SortingMode = "active";

        //    const int pageSize = 100;
        //    int pageNumber = (page ?? 0);

        //    if (pageNumber < 0)
        //    {
        //        return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
        //    }
        //    var subverses = CacheHandler.Instance.Register("Legacy:ActiveSubverses", new Func<IList<Subverse>>(() => {
        //        using (var db = new VoatUIDataContextAccessor())
        //        {
        //            db.EnableCacheableOutput();

        //            //HACK: I'm either completely <censored> or this is a huge pain in EF (sorting on a joined column and using .Distinct()), what you see below is a total hack that 'kinda' works
        //            return (from subverse in db.Subverses
        //                    join submission in db.Submissions on subverse.Name equals submission.Subverse
        //                    where subverse.Description != null && subverse.SideBar != null
        //                    orderby submission.CreationDate descending
        //                    select subverse).Take(pageSize).ToList().Distinct().ToList();
        //        }
        //    }), TimeSpan.FromMinutes(15));

        //    //Turn off paging and only show the top ~50 most active
        //    var paginatedActiveSubverses = new PaginatedList<Subverse>(subverses, 0, pageSize, pageSize);

        //    ViewBag.NavigationViewModel = new NavigationViewModel()
        //    {
        //        Description = "Active Subverses",
        //        Name = "Subverses",
        //        MenuType = MenuType.Discovery,
        //        BasePath = null,
        //        Sort = null
        //    };

        //    return View("~/Views/Subverses/Subverses.cshtml", paginatedActiveSubverses);
        //}

        #endregion


    }
}
