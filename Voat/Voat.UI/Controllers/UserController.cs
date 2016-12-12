using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Query;
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
        //Ported code requires this because views use EF Context.
        private voatEntities _db = new voatEntities();

        public ActionResult Overview(string userName)
        {
            var originalUserName = UserHelper.OriginalUsername(userName);
            if (String.IsNullOrEmpty(originalUserName))
            {
                return NotFoundErrorView();
            }
            ViewBag.UserName = originalUserName;

            return View();
        }
        public async  Task<ActionResult> Comments(string userName, int? page = null)
        {
            if (page.HasValue && page.Value < 0)
            {
                return NotFoundErrorView();
            }

            var originalUserName = UserHelper.OriginalUsername(userName);
            if (String.IsNullOrEmpty(originalUserName))
            {
                return NotFoundErrorView();
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

            PaginatedList<Domain.Models.SubmissionComment> paginatedUserComments = new PaginatedList<Domain.Models.SubmissionComment>(comments, page ?? 0, PAGE_SIZE, -1);

            return View(paginatedUserComments);
        }
        public async Task<ActionResult> Submissions(string userName, int? page = null)
        {
            if (page.HasValue && page.Value < 0)
            {
                return NotFoundErrorView();
            }


            var originalUserName = UserHelper.OriginalUsername(userName);
            if (String.IsNullOrEmpty(originalUserName))
            {
                return NotFoundErrorView();
            }
            ViewBag.UserName = originalUserName;

            //TODO: Convert this over to query syntax
            ////if user is accesing their own comments, increase max page size to 100, else use default
            //int? maxPages = (originalUserName == User.Identity.Name ? 100 : (int?)null);

            //var q = new QueryUserSubmissions(userName,
            //    new Data.SearchOptions(maxPages)
            //    {
            //        Page = page ?? 0,
            //        Sort = Domain.Models.SortAlgorithm.New,
            //    });

            //var comments = await q.ExecuteAsync();

            //PaginatedList<Domain.Models.SubmissionComment> paginatedUserComments = new PaginatedList<Submission>(comments, page ?? 0, PAGE_SIZE, -1);

            //return View(paginatedUserComments);

            var userSubmissions = from s in _db.Submissions.OrderByDescending(s => s.CreationDate)
                                  where s.UserName.Equals(originalUserName)
                                  && !s.IsAnonymized
                                  && !s.IsDeleted
                                  && !s.Subverse1.IsAnonymized //Don't think we need this condition
                                  select s;

            PaginatedList<Submission> paginatedUserSubmissions = new PaginatedList<Submission>(userSubmissions, page ?? 0, PAGE_SIZE);


            return View(paginatedUserSubmissions);
        }
        public ActionResult Saved(string userName, int? page = null)
        {
            if (page.HasValue && page.Value < 0)
            {
                return NotFoundErrorView();
            }

            var originalUserName = UserHelper.OriginalUsername(userName);
            if (String.IsNullOrEmpty(originalUserName))
            {
                return NotFoundErrorView();
            }
            if (!User.Identity.IsAuthenticated || (User.Identity.IsAuthenticated && !User.Identity.Name.Equals(originalUserName, StringComparison.OrdinalIgnoreCase)))
            {
                return RedirectToAction("Overview");
            }
            ViewBag.UserName = originalUserName;

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

            var paginatedUserSubmissionsAndComments = new PaginatedList<SavedItem>(mergedSubmissionsAndComments, page ?? 0, PAGE_SIZE);
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
            var cmd = new BlockCommand(blockType, name, true);
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
            var cmd = new BlockCommand(Domain.Models.DomainType.User, name, false);
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
        public async Task<ViewResult> Blocked(Domain.Models.DomainType blockType, int? page)
        {

            switch (blockType) {
                case Domain.Models.DomainType.User:
                    var q = new QueryUserBlocks();
                    var blocks = await q.ExecuteAsync();

                    var userBlocks = blocks.Where(x => x.Type == Domain.Models.DomainType.User).OrderBy(x => x.Name);

                    return View("BlockedUsers", userBlocks);

                    break;

                case Domain.Models.DomainType.Subverse:
                default:

                    //Original Code below, leaving as is bc it works
                    ViewBag.SelectedSubverse = "subverses";
                    ViewBag.SubversesView = "blocked";
                    const int pageSize = 25;
                    int pageNumber = (page ?? 0);

                    if (pageNumber < 0)
                    {
                        return NotFoundErrorView();
                    }
                    string userName = User.Identity.Name;
                    // get a list of user blocked subverses with details and order by subverse name, ascending
                    IQueryable<SubverseDetailsViewModel> blockedSubverses = from c in _db.Subverses
                                                                            join a in _db.UserBlockedSubverses
                                                                            on c.Name equals a.Subverse
                                                                            where a.UserName.Equals(userName)
                                                                            orderby a.Subverse ascending
                                                                            select new SubverseDetailsViewModel
                                                                            {
                                                                                Name = c.Name,
                                                                                Title = c.Title,
                                                                                Description = c.Description,
                                                                                Creation_date = c.CreationDate,
                                                                                Subscribers = c.SubscriberCount
                                                                            };

                    var paginatedBlockedSubverses = new PaginatedList<SubverseDetailsViewModel>(blockedSubverses, page ?? 0, pageSize);

                    return View("BlockedSubverses", paginatedBlockedSubverses);

                    break;

            }
            
        }
        #endregion
    }
}