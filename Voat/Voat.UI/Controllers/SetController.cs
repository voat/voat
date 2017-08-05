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
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Voat.Models;
using Voat.Models.ViewModels;

using Voat.Utilities;
using Voat.Data.Models;
using Voat.Configuration;
using Voat.UI.Utilities;
using Voat.Data;
using Voat.Domain.Query;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Voat.Common;
using Voat.Http;

namespace Voat.Controllers
{
    public class SetController : BaseController
    {
        public async Task<ActionResult> Index(string name, string sort)
        {

            var domainReference = DomainReference.Parse(name, DomainType.Set);

            //var domainReference = new DomainReference(DomainType.Set, name, userName);

            var qSet = new QuerySet(domainReference.Name, domainReference.OwnerName);
            var set = await qSet.ExecuteAsync();

            if (set == null)
            {
                return ErrorView(new ErrorViewModel() { Title = "Can't find this set", Description = "Maybe it's gone?", Footer = "Probably yeah" });
            }

            var perms = SetPermission.GetPermissions(set, User.Identity);

            if (!perms.View)
            {
                return ErrorView(new ErrorViewModel() { Title = "Set is Private", Description = "This set doesn't allow viewing. It is private.", Footer = "Sometimes sets are shy around others." });
            }


            var options = new SearchOptions(Request.QueryString.Value);
            //Set sort because it is part of path
            if (!String.IsNullOrEmpty(sort))
            {
                options.Sort = (SortAlgorithm)Enum.Parse(typeof(SortAlgorithm), sort, true);
            }
            else
            {
                //Set Default Set to Relative if no sort is provided
                if (set.Name.IsEqual("Default") && String.IsNullOrEmpty(set.UserName))
                {
                    options.Sort = SortAlgorithm.Relative;
                }
            }
            //set span to day if not specified explicitly 
            if (options.Sort == SortAlgorithm.Top && !Request.Query.ContainsKey("span"))
            {
                options.Span = SortSpan.Day;
            }

            var q = new QuerySubmissions(domainReference, options).SetUserContext(User);
            var result = await q.ExecuteAsync();
            
            var model = new SubmissionListViewModel();
            model.Submissions = new Utilities.PaginatedList<Domain.Models.Submission>(result, options.Page, options.Count);
            model.Submissions.RouteName = "SetIndex";
            model.Context = domainReference;
            model.Sort = options.Sort;// == SortAlgorithm.RelativeRank ? SortAlgorithm.Hot :options.Sort; //UI doesn't want relative rank
            model.Span = options.Span;

            ViewBag.NavigationViewModel = new NavigationViewModel() {
                Description = "Set Description Here",
                Name = name,
                MenuType = MenuType.Set,
                BasePath = model.Context.BasePath(),
                Sort = model.Sort
            };

            return View(ViewPath(model.Context), model);
        }
        public async Task<ActionResult> Sidebar(string name)
        {
            //TODO: Implement Command/Query - Remove direct Repository access
            using (var repo = new Repository(User))
            {
                var domainReference = DomainReference.Parse(name, DomainType.Set);
                var set = repo.GetSet(domainReference.Name, domainReference.OwnerName);

                if (set != null)
                {
                    return PartialView("~/Views/Shared/Sidebars/_SidebarSet.cshtml", set);
                }
                else
                {
                    return new EmptyResult();
                }

            }
        }

        public async Task<ActionResult> Details(string name)
        {
            //TODO: Implement Command/Query - Remove direct Repository access
            using (var repo = new Repository(User))
            {
                var domainReference = DomainReference.Parse(name, DomainType.Set);
                var set = repo.GetSet(domainReference.Name, domainReference.OwnerName);
                if (set == null)
                {
                    //Since system sets are not created until needed we show a slightly better error message for front/blocked.
                    if (name.IsEqual(SetType.Front.ToString()))
                    {
                        ViewBag.NavigationViewModel = new Models.ViewModels.NavigationViewModel()
                        {
                            Description = "Discover Search",
                            Name = "No Idea",
                            MenuType = Models.ViewModels.MenuType.Discovery,
                            BasePath = VoatUrlFormatter.BasePath(domainReference),
                            Sort = null
                        };
                        return ErrorView(new ErrorViewModel() { Title = "No Subscriptions!", Description = "You don't have any subscriptions so we have to show you this instead", Footer = "Subscribe to a subverse you silly goat" });
                    }
                    else if (name.IsEqual(SetType.Blocked.ToString()))
                    {
                        ViewBag.NavigationViewModel = new Models.ViewModels.NavigationViewModel()
                        {
                            Description = "Discover Search",
                            Name = "No Idea",
                            MenuType = Models.ViewModels.MenuType.Discovery,
                            BasePath = VoatUrlFormatter.BasePath(domainReference),
                            Sort = null
                        };
                        return ErrorView(new ErrorViewModel() { Title = "No Blocked Subs!", Description = "You don't have any blocked subs. Golf clap.", Footer = "Block some subs and this page will magically change!" });
                    }
                    else
                    {
                        return ErrorView(new ErrorViewModel() { Title = "Can't find this set", Description = "Maybe it's gone?", Footer = "Probably yeah" });
                    }
                }

                var perms = SetPermission.GetPermissions(set.Map(), User.Identity);

                if (!perms.View)
                {
                    return ErrorView(new ErrorViewModel() { Title = "Set is Private", Description = "This set doesn't allow the viewing of its properties", Footer = "It's ok, I can't see it either" });
                }

                var options = new SearchOptions(Request.QueryString.Value);
                options.Count = 50;

                var setList = await repo.GetSetListDescription(set.ID, options.Page);

                var model = new SetViewModel();
                model.Permissions = perms;
                model.Set = set.Map();
                model.List = new PaginatedList<Domain.Models.SubverseSubscriptionDetail>(setList, options.Page, options.Count);
                model.List.RouteName = "SetDetails";

                ViewBag.NavigationViewModel = new NavigationViewModel() {
                    Name = domainReference.FullName,
                    Description = set.Description,
                    BasePath = VoatUrlFormatter.BasePath(domainReference),
                    MenuType = (set.Type == (int)SetType.Front || set.Type == (int)SetType.Blocked ?  MenuType.Discovery : MenuType.Set)
                };
                return View("Details", model);
            }
        }
        // /s/{set}/{owner}/{subverse}
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ListChange(string name, string subverse, Domain.Models.SubscriptionAction subscribeAction)
        {
            var domainReference = DomainReference.Parse(name, DomainType.Set);
            //Only user sets can be changed, thus userName never needs to be checked here.
            var cmd = new SetSubverseCommand(domainReference, subverse, subscribeAction).SetUserContext(User);
            var result = await cmd.Execute();
            return JsonResult(result);
        }

        //// /s/{set}/{owner}/{subverse}
        //[Authorize]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<JsonResult> Subscribe(string name, string userName, Domain.Models.SubscriptionAction action = SubscriptionAction.Toggle)
        //{

        //    var cmd = new SubscribeCommand(new DomainReference(DomainType.Set, name, userName), action);
        //    var result = await cmd.Execute();
        //    return JsonResult(result);
        //}


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string name)
        {

            if (ModelState.IsValid)
            {
                //TODO: Implement Command/Query - Remove direct Repository access
                using (var repo = new Repository(User))
                {
                    var domainReference = DomainReference.Parse(name, DomainType.Set);
                    var result = await repo.DeleteSet(domainReference);

                    if (result.Success)
                    {
                        if (Request.IsAjaxRequest())
                        {
                            return JsonResult(result);
                        }
                        else
                        {
                            return new RedirectToRouteResult("UserSets", new RouteValueDictionary() { { "pathPrefix", "user" }, { "userName", User.Identity.Name } });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", result.Message);
                    }
                }
            }
            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Update(Set set)
        {
            if (ModelState.IsValid)
            {
                var cmd = new UpdateSetCommand(set).SetUserContext(User);
                var result = await cmd.Execute();
                return JsonResult(result);
            }
            return View(set);           
        }

        [Authorize]
        public async Task<ActionResult> Create()
        {
            if (!VoatSettings.Instance.SetCreationEnabled)
            {
                return ErrorView(new ErrorViewModel() { Title = "Set Creation Disabled", Description = "Sorry, but set creation is currently disabled", Footer = "Someone will be fired for this" });
            }

            return View(new Set());
        }
        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Set set)
        {
            if (!VoatSettings.Instance.SetCreationEnabled)
            {
                return ErrorView(new ErrorViewModel() { Title = "Set Creation Disabled", Description = "Sorry, but set creation is currently disabled", Footer = "Someone will be fired for this" });
            }

            if (ModelState.IsValid)
            {
                //TODO: Implement Command/Query - Remove direct Repository access
                using (var repo = new Repository(User))
                {
                    var result = await repo.CreateOrUpdateSet(set);

                    if (result.Success)
                    {
                        Caching.CacheHandler.Instance.Remove(Caching.CachingKey.UserSubscriptions(User.Identity.Name));
                        if (Request.IsAjaxRequest())
                        {
                            return JsonResult(result);
                        }
                        else
                        {
                            var domainReference = new DomainReference(DomainType.Set, result.Response.Name, result.Response.UserName);
                            return new RedirectToRouteResult("SetDetails", new RouteValueDictionary() { { "name", domainReference.FullName } });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", result.Message);
                    }
                }
            }
            return View(set);       
        }
        #region OLD CODE
        //private readonly voatEntities _db = new VoatUIDataContextAccessor();

        //// GET: /set/setid
        //// show single set frontpage
        //public ActionResult SingleSet(int setId, int? page)
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        return RedirectToAction("UnAuthorized", "Error");
        //    }

        //    const int pageSize = 25;
        //    int recordsToSkip = (page ?? 0);
        //    try
        //    {
        //        // get list of subverses for the set
        //        // for each subverse, get top ranked submissions
        //        var set = _db.UserSets.FirstOrDefault(ds => ds.ID == setId);

        //        if (set == null) return RedirectToAction("NotFound", "Error");

        //        ViewBag.SelectedSubverse = set.Name;
        //        var singleSetResultModel = new SingleSetViewModel();
        //        var submissions = new List<SetSubmission>();

        //        // if subs in set count is < 1, don't display the page, instead, check if the user owns this set and give them a chance to add subs to the set
        //        if (set.UserSetLists.Count < 1)
        //        {
        //            // check if the user owns this sub
        //            if (User.Identity.IsAuthenticated && User.Identity.Name == set.CreatedBy)
        //            {
        //                return RedirectToAction("EditSet", "Sets", new { setId = set.ID });
        //            }
        //        }

        //        int subsInSet = set.UserSetLists.Count();
        //        int submissionsToGet = 5;

        //        // there is at least 1 sub in the set
        //        if (subsInSet == 1)
        //        {
        //            submissionsToGet = 25;
        //        }
        //        // get only one submission from each sub if set contains 25 or more subverses
        //        else if (subsInSet >= 25)
        //        {
        //            submissionsToGet = 1;
        //        }
        //        // try to aim for 25 submissions
        //        else
        //        {
        //            submissionsToGet = (int)Math.Ceiling((double)25 / subsInSet);
        //        }

        //        foreach (var subverse in set.UserSetLists)
        //        {
        //            // get top ranked submissions for current subverse
        //            Subverse currentSubverse = subverse.Subverse1;

        //            if (currentSubverse != null)
        //            {
        //                // skip parameter could be passed here
        //                submissions.AddRange(SetsUtility.TopRankedSubmissionsFromASub(currentSubverse.Name, _db.Submissions, set.Name, submissionsToGet, recordsToSkip * pageSize));
        //            }
        //            singleSetResultModel.Name = set.Name;
        //            singleSetResultModel.Description = set.Description;
        //            singleSetResultModel.Id = set.ID;
        //        }

        //        singleSetResultModel.SubmissionsList = new List<SetSubmission>(submissions.OrderByDescending(s => s.Rank));

        //        return View("~/Views/Sets/Index.cshtml", singleSetResultModel);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //// GET: /set/setId/page
        //// fetch x more items from a set
        //public ActionResult SingleSetPage(int setId, int page)
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        return RedirectToAction("UnAuthorized", "Error");
        //    }
        //    const int pageSize = 2;
        //    try
        //    {
        //        // get list of subverses for the set
        //        // for each subverse, get top ranked submissions
        //        var set = _db.UserSets.FirstOrDefault(ds => ds.ID == setId);

        //        if (set == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);

        //        ViewBag.SelectedSubverse = set.Name;
        //        var singleSetResultModel = new SingleSetViewModel();
        //        var submissions = new List<SetSubmission>();

        //        foreach (var subverse in set.UserSetLists)
        //        {
        //            // get 5 top ranked submissions for current subverse
        //            Subverse currentSubverse = subverse.Subverse1;

        //            if (currentSubverse != null)
        //            {
        //                // skip parameter could be passed here
        //                submissions.AddRange(SetsUtility.TopRankedSubmissionsFromASub(currentSubverse.Name, _db.Submissions, set.Name, pageSize, page * pageSize));
        //            }
        //            singleSetResultModel.Name = set.Name;
        //            singleSetResultModel.Description = set.Description;
        //            singleSetResultModel.Id = set.ID;
        //        }

        //        singleSetResultModel.SubmissionsList = new List<SetSubmission>(submissions.OrderByDescending(s => s.Rank).ThenByDescending(s => s.CreationDate));

        //        if (submissions.Any())
        //        {
        //            ViewBag.Page = page;
        //            return PartialView("~/Views/Sets/_SingleSetPage.cshtml", singleSetResultModel);
        //        }

        //        // no more entries found
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //// GET: /set/setid/edit
        //[Authorize]
        //public ActionResult EditSet(int setId)
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        return RedirectToAction("UnAuthorized", "Error");
        //    }

        //    var setToEdit = _db.UserSets.FirstOrDefault(s => s.ID == setId);

        //    if (setToEdit != null)
        //    {
        //        // check if user owns the set and abort
        //        if (!UserHelper.IsUserSetOwner(User.Identity.Name, setToEdit.ID)) return RedirectToAction("UnAuthorized", "Error");

        //        // get list of subverses for the set
        //        var setSubversesList = _db.UserSetLists.Where(s => s.ID == setToEdit.ID).ToList();

        //        // populate viewmodel for the set
        //        var setViewModel = new SingleSetViewModel()
        //        {
        //            Name = setToEdit.Name,
        //            Description = setToEdit.Description,
        //            SubversesList = setSubversesList,
        //            Id = setToEdit.ID,
        //            Created = setToEdit.CreationDate,
        //            Subscribers = setToEdit.SubscriberCount
        //        };

        //        return View("~/Views/Sets/EditSet.cshtml", setViewModel);
        //    }
        //    return RedirectToAction("NotFound", "Error");
        //}

        //// POST: /s/reorder/setname
        //[Authorize]
        //[HttpPost]
        //[VoatValidateAntiForgeryToken]
        //public ActionResult ReorderSet(string setName, int direction)
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        return RedirectToAction("UnAuthorized", "Error");
        //    }
        //    // check if user is subscribed to given set
        //    if (UserHelper.IsUserSubscribedToSet(User.Identity.Name, setName))
        //    {
        //        // reorder the set for logged in user using given direction
        //        // TODO: reorder
        //        return new HttpStatusCodeResult(HttpStatusCode.OK);
        //    }

        //    // the user is not subscribed to given set
        //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //}

        //// GET: /sets
        //public ActionResult Sets(int? page)
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        return RedirectToAction("UnAuthorized", "Error");
        //    }
        //    ViewBag.SelectedSubverse = "Popular sets";
        //    const int pageSize = 25;
        //    int pageNumber = (page ?? 0);

        //    if (pageNumber < 0)
        //    {
        //        return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
        //    }

        //    try
        //    {
        //        // order by subscriber count (popularity), show only sets which are fully defined by their creators
        //        var sets = _db.UserSets.Where(s => s.UserSetLists.Any()).OrderByDescending(s => s.SubscriberCount);

        //        var paginatedSets = new PaginatedList<UserSet>(sets, page ?? 0, pageSize);

        //        return View(paginatedSets);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //// GET: /sets/recommended
        //public ActionResult RecommendedSets(int? page)
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        return RedirectToAction("UnAuthorized", "Error");
        //    }
        //    ViewBag.SelectedSubverse = "recommended sets";
        //    ViewBag.sortingmode = "recommended";

        //    const int pageSize = 25;
        //    int pageNumber = (page ?? 0);

        //    if (pageNumber < 0)
        //    {
        //        return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
        //    }

        //    try
        //    {
        //        // order by subscriber count (popularity), show only sets which are fully defined
        //        var sets = _db.UserSets.Where(s => s.UserSetLists.Any() && s.IsDefault).OrderByDescending(s => s.SubscriberCount);

        //        var paginatedSets = new PaginatedList<UserSet>(sets, page ?? 0, pageSize);

        //        return View("~/Views/Sets/Sets.cshtml", paginatedSets);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //// GET: /sets/create
        //[Authorize]
        //public ActionResult CreateSet()
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        return RedirectToAction("UnAuthorized", "Error");
        //    }
        //    return View("~/Views/Sets/CreateSet.cshtml");
        //}

        //// POST: /sets/create
        //[Authorize]
        //[HttpPost]
        //[VoatValidateAntiForgeryToken]
        //public async Task<ActionResult> CreateSet([Bind(Include = "Name, Description")] AddSet setTmpModel)
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        return RedirectToAction("UnAuthorized", "Error");
        //    }
        //    if (!User.Identity.IsAuthenticated) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    int maximumOwnedSets = VoatSettings.Instance.MaximumOwnedSets;

        //    // TODO
        //    // ###############################################################################################
        //    try
        //    {
        //        // abort if model is in invalid state
        //        if (!ModelState.IsValid) return View();

        //        // setup default values
        //        var set = new UserSet
        //        {
        //            Name = setTmpModel.Name,
        //            Description = setTmpModel.Description,
        //            CreationDate = Repository.CurrentDate,
        //            CreatedBy = User.Identity.Name,
        //            IsDefault = false,
        //            IsPublic = true,
        //            SubscriberCount = 0
        //        };

        //        // only allow users with less than maximum allowed sets to create a set
        //        var amountOfOwnedSets = _db.UserSets
        //            .Where(s => s.CreatedBy == User.Identity.Name)
        //            .ToList();

        //        if (amountOfOwnedSets.Count <= maximumOwnedSets)
        //        {
        //            _db.UserSets.Add(set);
        //            await _db.SaveChangesAsync();

        //            // subscribe user to the newly created set
        //            UserHelper.SubscribeToSet(User.Identity.Name, set.ID);

        //            // go to newly created Set
        //            return RedirectToAction("EditSet", "Sets", new { setId = set.ID });
        //        }

        //        ModelState.AddModelError(string.Empty, "Sorry, you can not own more than " + maximumOwnedSets + " sets.");
        //        return View();
        //    }
        //    catch (Exception)
        //    {
        //        ModelState.AddModelError(string.Empty, "Something bad happened.");
        //        return View();
        //    }
        //    // ###############################################################################################
        //}

        //// GET: /mysets
        //[Authorize]
        //public ActionResult UserSets(int? page)
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        return RedirectToAction("UnAuthorized", "Error");
        //    }
        //    const int pageSize = 25;
        //    int pageNumber = (page ?? 0);

        //    if (pageNumber < 0)
        //    {
        //        return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
        //    }

        //    // load user sets for logged in user
        //    IQueryable<UserSetSubscription> userSets = _db.UserSetSubscriptions.Where(s => s.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)).OrderBy(s => s.UserSet.Name);

        //    var paginatedUserSetSubscriptions = new PaginatedList<UserSetSubscription>(userSets, page ?? 0, pageSize);

        //    return View("~/Views/Sets/MySets.cshtml", paginatedUserSetSubscriptions);
        //}

        //// GET: /mysets/manage
        //[Authorize]
        //public ActionResult ManageUserSets(int? page)
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        return RedirectToAction("UnAuthorized", "Error");
        //    }
        //    const int pageSize = 25;
        //    int pageNumber = (page ?? 0);

        //    if (pageNumber < 0)
        //    {
        //        return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
        //    }

        //    // load user owned sets for logged in user
        //    IQueryable<UserSet> userSets = _db.UserSets.Where(s => s.CreatedBy.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)).OrderBy(s => s.Name);

        //    var paginatedUserSets = new PaginatedList<UserSet>(userSets, page ?? 0, pageSize);

        //    return View("~/Views/Sets/ManageMySets.cshtml", paginatedUserSets);
        //}

        //// GET: 40 most popular sets by subscribers
        //[ChildActionOnly]
        //public PartialViewResult PopularSets()
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return new PartialViewResult();
        //    }
        //    var popularSets = _db.UserSets.Where(s => s.IsPublic).OrderByDescending(s => s.SubscriberCount).Take(40);

        //    return PartialView("~/Views/Sets/_PopularSets.cshtml", popularSets);
        //}

        //// POST: subscribe to a set
        //[Authorize]
        //[HttpPost]
        //[VoatValidateAntiForgeryToken]
        //public JsonResult Subscribe(int setId)
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return Json("Sets disabled." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //    }
        //    var loggedInUser = User.Identity.Name;

        //    UserHelper.SubscribeToSet(loggedInUser, setId);
        //    return Json("Subscription request was successful." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //}

        //// POST: unsubscribe from a set
        //[Authorize]
        //[HttpPost]
        //[VoatValidateAntiForgeryToken]
        //public JsonResult UnSubscribe(int setId)
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return Json("Sets disabled." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //    }
        //    var loggedInUser = User.Identity.Name;

        //    UserHelper.UnSubscribeFromSet(loggedInUser, setId);
        //    return Json("Unsubscribe request was successful." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //}

        //// POST: add a subverse to set
        //[Authorize]
        //[HttpPost]
        //[VoatValidateAntiForgeryToken]
        //public JsonResult AddSubverseToSet(string subverseName, int setId)
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return Json("Sets disabled." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //    }
        //    // check if set exists
        //    var setToModify = _db.UserSets.Find(setId);
        //    if (setToModify == null)
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return Json("Set doesn't exist." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //    }

        //    // check if user is set owner
        //    if (!UserHelper.IsUserSetOwner(User.Identity.Name, setId))
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return Json("Unauthorized request." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //    }

        //    // check if subverse exists
        //    var subverseToAdd = _db.Subverses.FirstOrDefault(s => s.Name.Equals(subverseName, StringComparison.OrdinalIgnoreCase));
        //    if (subverseToAdd == null)
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return Json("The subverse does not exist." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //    }

        //    // check if subverse is already a part of this set
        //    if (setToModify.UserSetLists.Any(sd => sd.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase)))
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return Json("The subverse is already a part of this set." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //    }

        //    // add subverse to set
        //    UserSetList newUsersetdefinition = new UserSetList
        //    {
        //        ID = setId,
        //        Subverse = subverseToAdd.Name
        //    };

        //    _db.UserSetLists.Add(newUsersetdefinition);
        //    _db.SaveChangesAsync();

        //    return Json("Add subverse to set request sucessful." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //}

        //// POST: remove a subverse from set
        //[Authorize]
        //[HttpPost]
        //[VoatValidateAntiForgeryToken]
        //public JsonResult RemoveSubverseFromSet(string subverseName, int setId)
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return Json("Sets disabled." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //    }
        //    // check if user is set owner
        //    if (!UserHelper.IsUserSetOwner(User.Identity.Name, setId))
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return Json(new List<string> { "Unauthorized request." });
        //    }

        //    // remove subverse from set
        //    var setDefinitionToRemove = _db.UserSetLists.FirstOrDefault(s => s.ID == setId && s.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase));
        //    if (setDefinitionToRemove != null)
        //    {
        //        _db.UserSetLists.Remove(setDefinitionToRemove);
        //        _db.SaveChangesAsync();
        //        return Json("Add subverse to set request sucessful." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //    }

        //    // expected subverse was not found in user set definition
        //    Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //    return Json(new List<string> { "Bad request." });
        //}

        //// POST: change set name and description
        //[Authorize]
        //[HttpPost]
        //[VoatValidateAntiForgeryToken]
        //public JsonResult ChangeSetInfo(int setId, string newSetName)
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return Json("Sets disabled." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //    }
        //    // check if user is set owner
        //    if (!UserHelper.IsUserSetOwner(User.Identity.Name, setId))
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return Json(new List<string> { "Unauthorized request." });
        //    }

        //    // find the set to modify
        //    var setToModify = _db.UserSets.Find(setId);

        //    if (setToModify != null)
        //    {
        //        try
        //        {
        //            setToModify.Name = newSetName;
        //            // TODO setToModify.Description = newSetDescription;

        //            _db.SaveChangesAsync();
        //            return Json("Set info change was sucessful." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //        }
        //        catch (Exception)
        //        {
        //            //
        //        }
        //    }

        //    // something went horribly wrong
        //    Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //    return Json(new List<string> { "Bad request." });
        //}

        //// POST: delete a set
        //[Authorize]
        //[HttpPost]
        //[VoatValidateAntiForgeryToken]
        //public JsonResult DeleteSet(int setId)
        //{
        //    if (VoatSettings.Instance.SetsDisabled)
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return Json("Sets disabled." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //    }
        //    // check if user is set owner
        //    if (!UserHelper.IsUserSetOwner(User.Identity.Name, setId))
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //        return Json(new List<string> { "Unauthorized request." });
        //    }

        //    // delete the set
        //    var setToRemove = _db.UserSets.FirstOrDefault(s => s.ID == setId && s.CreatedBy.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
        //    if (setToRemove != null)
        //    {
        //        _db.UserSets.Remove(setToRemove);
        //        _db.SaveChangesAsync();
        //        return Json("Set has been deleted." /* CORE_PORT: Removed , JsonRequestBehavior.AllowGet */);
        //    }

        //    // expected set was not found in user sets
        //    Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //    return Json(new List<string> { "Bad request." });
        //}
        #endregion
    }
}
