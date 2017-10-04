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
using Voat.Http.Filters;

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
            
            var model = new ListViewModel<Domain.Models.Submission>();
            model.Items = new Utilities.PaginatedList<Domain.Models.Submission>(result, options.Page, options.Count);
            model.Items.RouteName = "SetIndex";
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
            return await Index(name, "");
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
            else
            {
                return JsonResult(CommandResponse.FromStatus(Status.Invalid, ModelState.GetFirstErrorMessage()));
            }
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
        [PreventSpam(60)]
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
    }
}
