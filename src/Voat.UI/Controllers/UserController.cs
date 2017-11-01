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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Query;
using Voat.Http;
using Voat.Http.Filters;
using Voat.Models;
using Voat.Models.ViewModels;
using Voat.Utilities;

namespace Voat.Controllers
{
    public class UserController : BaseController
    {

        public UserController()
        {
            //HACK: required to get _Layout to render the user sub menu
            ViewBag.SelectedSubverse = "user";
        }

        private const int PAGE_SIZE = 20;
        private VoatOutOfRepositoryDataContextAccessor _db = new VoatOutOfRepositoryDataContextAccessor();

        public ActionResult Overview(string userName)
        {
            var originalUserName = UserHelper.OriginalUsername(userName);
            if (String.IsNullOrEmpty(originalUserName))
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }
            ViewBag.UserName = originalUserName;
            ViewBag.NavigationViewModel = new NavigationViewModel()
            {
                MenuType = MenuType.UserProfile,
                Name = originalUserName,
                BasePath = "/user/" + originalUserName,
                Description = originalUserName + "'s Overview",
            };
            return View();
        }

        [Authorize]
        public async  Task<ActionResult> Comments(string userName, int? page = null)
        {
            if (page.HasValue && page.Value < 0)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }

            var originalUserName = UserHelper.OriginalUsername(userName);
            if (String.IsNullOrEmpty(originalUserName))
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }
            ViewBag.UserName = originalUserName;

            //if user is accesing their own comments, increase max page size to 100, else use default
            int? maxPages = (originalUserName == User.Identity.Name ? 100 : (int?)null);

            var q = new QueryUserComments(userName, 
                new Data.SearchOptions(maxPages) {
                    Page = page ?? 0,
                    Sort = Domain.Models.SortAlgorithm.New,
                });

            var comments = await q.ExecuteAsync();

            //var userComments = from c in _db.Comments.OrderByDescending(c => c.CreationDate)
            //                    where c.UserName.Equals(originalUserName)
            //                    && !c.IsAnonymized
            //                    && !c.IsDeleted
            //                    //&& !c.Submission.Subverse1.IsAnonymized //Don't think we need this condition
            //                    select c;

            var paged = new PaginatedList<Domain.Models.SubmissionComment>(comments, page ?? 0, PAGE_SIZE, -1);
            ViewBag.NavigationViewModel = new NavigationViewModel()
            {
                MenuType = MenuType.UserProfile,
                Name = originalUserName,
                BasePath = "/user/" + originalUserName,
                Description = originalUserName + "'s Comments",
            };

            ViewBag.RobotIndexing = Domain.Models.RobotIndexing.None;
            return View(paged);
        }
        [Authorize]
        public async Task<ActionResult> Submissions(string userName, int? page = null)
        {
            if (page.HasValue && page.Value < 0)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }


            var originalUserName = UserHelper.OriginalUsername(userName);
            if (String.IsNullOrEmpty(originalUserName))
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }
            ViewBag.UserName = originalUserName;

            ////if user is accesing their own comments, increase max page size to 100, else use default
            int? maxPages = (originalUserName == User.Identity.Name ? 100 : (int?)null);

            var q = new QueryUserSubmissions(userName,
                new Data.SearchOptions(maxPages)
                {
                    Page = page ?? 0,
                    Sort = Domain.Models.SortAlgorithm.New,
                });

            var data = await q.ExecuteAsync();

            var paged = new PaginatedList<Domain.Models.Submission>(data, page ?? 0, PAGE_SIZE, -1);

            ViewBag.NavigationViewModel = new NavigationViewModel()
            {
                MenuType = MenuType.UserProfile,
                Name = originalUserName,
                BasePath = "/user/" + originalUserName,
                Description = originalUserName + "'s Submissions",
            };

            ViewBag.RobotIndexing = Domain.Models.RobotIndexing.None;
            return View(paged);
        }

        //TODO: Rewrite this
        public ActionResult Saved(string userName, int? page = null)
        {
            if (page.HasValue && page.Value < 0)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }

            var originalUserName = UserHelper.OriginalUsername(userName);
            if (String.IsNullOrEmpty(originalUserName))
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }
            if (!User.Identity.IsAuthenticated || (User.Identity.IsAuthenticated && !User.Identity.Name.IsEqual(originalUserName)))
            {
                return RedirectToAction("Overview");
            }
            ViewBag.UserName = originalUserName;

            

            IQueryable<SavedItem> savedSubmissions = (from m in _db.Submission
                                                          join s in _db.SubmissionSaveTracker on m.ID equals s.SubmissionID
                                                          where !m.IsDeleted && s.UserName == User.Identity.Name
                                                          select new SavedItem()
                                                          {
                                                              SaveDateTime = s.CreationDate,
                                                              SavedSubmission = m,
                                                              SavedComment = null,
                                                              Subverse = m.Subverse
                                                          });

            IQueryable<SavedItem> savedComments = (from c in _db.Comment
                                                    join sub in _db.Submission on c.SubmissionID equals sub.ID
                                                    join s in _db.CommentSaveTracker on c.ID equals s.CommentID
                                                    where !c.IsDeleted && s.UserName == User.Identity.Name
                                                    select new SavedItem()
                                                    {
                                                        SaveDateTime = s.CreationDate,
                                                        SavedSubmission = null,
                                                        SavedComment = c,
                                                        Subverse = sub.Subverse
                                                    });

            // merge submissions and comments into one list sorted by date
            var mergedSubmissionsAndComments = savedSubmissions.Concat(savedComments).OrderByDescending(s => s.SaveDateTime).AsQueryable();

            var paginatedUserSubmissionsAndComments = new PaginatedList<SavedItem>(mergedSubmissionsAndComments, page ?? 0, PAGE_SIZE);

            ViewBag.NavigationViewModel = new NavigationViewModel() {
                MenuType = MenuType.UserProfile,
                Name = originalUserName,
                BasePath = "/user/" + originalUserName,
                Description = originalUserName + "'s Saved",
            };

            return View(paginatedUserSubmissionsAndComments);
        }

        #region ACCOUNT BASED
        //This code really belongs in an Account controller but didn't want to add it to the existing Account controller


        [Authorize]
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> Block(Domain.Models.DomainType blockType, string name)
        {
            //Used by voat.js
            var cmd = new BlockCommand(blockType, name, true).SetUserContext(User);
            var result = await cmd.Execute();

            if (Request.IsAjaxRequest())
            {
                return Json(result);
            }
            else
            {
                return await Blocked(blockType, null);
            }
        }
        [Authorize]
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> BlockUser(string name)
        {
            var cmd = new BlockCommand(Domain.Models.DomainType.User, name, false).SetUserContext(User);
            var result = await cmd.Execute();

            if (Request.IsAjaxRequest())
            {
                return Json(result);
            }
            else
            {
                if (!result.Success)
                {
                    ModelState.AddModelError("", result.Message);
                    return await Blocked(Domain.Models.DomainType.User, null);
                }
                else
                {
                    return Redirect("/user/blocked/user");
                }
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Blocked(Domain.Models.DomainType blockType, int? page)
        {

            switch (blockType) {
                case Domain.Models.DomainType.User:
                    var q = new QueryUserBlocks().SetUserContext(User);
                    var blocks = await q.ExecuteAsync();

                    var userBlocks = blocks.Where(x => x.Type == Domain.Models.DomainType.User).OrderBy(x => x.Name);

                    var originalUserName = User.Identity.Name;
                    ViewBag.NavigationViewModel = new NavigationViewModel()
                    {
                        MenuType = MenuType.UserProfile,
                        Name = originalUserName,
                        BasePath = "/user/" + originalUserName,
                        Description = originalUserName + "'s Blocked Users",
                    };

                    return View("BlockedUsers", userBlocks);

                    break;

                case Domain.Models.DomainType.Subverse:
                default:

                    var domainReference = new Domain.Models.DomainReference(Domain.Models.DomainType.Set, Domain.Models.SetType.Blocked.ToString(), User.Identity.Name);
                    return RedirectToAction("Details", "Set", new { name = domainReference.FullName });

                    ////Original Code below, leaving as is bc it works
                    //ViewBag.SelectedSubverse = "subverses";
                    //ViewBag.SubversesView = "blocked";
                    //const int pageSize = 25;
                    //int pageNumber = (page ?? 0);

                    //if (pageNumber < 0)
                    //{
                    //    return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
                    //}
                    //string userName = User.Identity.Name;
                    //// get a list of user blocked subverses with details and order by subverse name, ascending
                    //IQueryable<SubverseDetailsViewModel> blockedSubverses = from c in _db.Subverses
                    //                                                        join a in _db.UserBlockedSubverses
                    //                                                        on c.Name equals a.Subverse
                    //                                                        where a.UserName.Equals(userName)
                    //                                                        orderby a.Subverse ascending
                    //                                                        select new SubverseDetailsViewModel
                    //                                                        {
                    //                                                            Name = c.Name,
                    //                                                            Title = c.Title,
                    //                                                            Description = c.Description,
                    //                                                            CreationDate = c.CreationDate,
                    //                                                            Subscribers = c.SubscriberCount
                    //                                                        };

                    //var paginatedBlockedSubverses = new PaginatedList<SubverseDetailsViewModel>(blockedSubverses, page ?? 0, pageSize);

                    //return View("BlockedSubverses", paginatedBlockedSubverses);

                    break;

            }
            
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Sets(string userName)
        {

            var originalUserName = UserHelper.OriginalUsername(userName);

            if (String.IsNullOrEmpty(originalUserName))
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }

            ViewBag.UserName = originalUserName;

            ViewBag.NavigationViewModel = new NavigationViewModel()
            {
                MenuType = MenuType.UserProfile,
                Name = originalUserName,
                BasePath = "/user/" + originalUserName,
                Description = originalUserName + "'s Sets",
            };

            var q = new QueryUserSets(userName).SetUserContext(User);
            var results = await q.ExecuteAsync();

            return View(results);
            
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Subscribed(Domain.Models.DomainType domainType)
        {
            switch (domainType)
            {
                case Domain.Models.DomainType.Subverse:
                    var domainReference = new Domain.Models.DomainReference(Domain.Models.DomainType.Set, Domain.Models.SetType.Front.ToString(), User.Identity.Name);
                    return RedirectToAction("Details", "Set", new { name = domainReference.FullName });
                    break;
                case Domain.Models.DomainType.Set:


                    var options = new SearchOptions(Request.QueryString.Value);

                    var q = new QueryUserSubscribedSets(options).SetUserContext(User);
                   
                    var results = await q.ExecuteAsync();

                    var paged = new Utilities.PaginatedList<Domain.Models.DomainReferenceDetails>(results, options.Page, options.Count);

                    ViewBag.NavigationViewModel = new Models.ViewModels.NavigationViewModel()
                    {
                        Description = "Subscribed Sets",
                        Name = "No Idea",
                        MenuType = Models.ViewModels.MenuType.Discovery,
                        BasePath = null,
                        Sort = null
                    };
                    ViewBag.DomainType = Voat.Domain.Models.DomainType.Set;
                    return View(paged);
                    break;
                default:
                case Domain.Models.DomainType.User:
                    throw new NotImplementedException("This isn't done yet!");
                    break;
            }
        }

        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        public async Task<JsonResult> Subscribe(Domain.Models.DomainType domainType, string name, Domain.Models.SubscriptionAction subscribeAction)
        {
            var domainReference = Domain.Models.DomainReference.Parse(name, domainType);

            var cmd = new SubscribeCommand(domainReference, subscribeAction).SetUserContext(User);
            var result = await cmd.Execute();
            return JsonResult(result); 

        }


        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(5)]
        public async Task<ActionResult> Save(Domain.Models.ContentType contentType, int id)
        {
            var cmd = new SaveCommand(contentType, id).SetUserContext(User);
            var response = await cmd.Execute();

            return JsonResult(response);
            //if (response.Success)
            //{
            //    return Json(new { success = true });
            //}
            //else
            //{
            //    return JsonError(response.Message);
            //}
        }

        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        public async Task<JsonResult> Vote(Domain.Models.ContentType contentType, int id, int voteStatus)
        {
            VoteResponse result = null; 
            switch (contentType) {
                case Domain.Models.ContentType.Submission:
                    var cmdV = new SubmissionVoteCommand(id, voteStatus, IpHash.CreateHash(Request.RemoteAddress())).SetUserContext(User);
                    result = await cmdV.Execute();
                    break;
                case Domain.Models.ContentType.Comment:
                    var cmdC = new CommentVoteCommand(id, voteStatus, IpHash.CreateHash(Request.RemoteAddress())).SetUserContext(User);
                    result = await cmdC.Execute();
                    break;
            }
            return Json(result);
        }

        #endregion
    }
}
