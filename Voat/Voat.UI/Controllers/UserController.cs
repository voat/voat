using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Voat.Data.Models;
using Voat.Models;
using Voat.Models.ViewModels;
using Voat.Utilities;

namespace Voat.Controllers
{
    public class UserController : Controller
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
                return View("~/Views/Errors/Error_404.cshtml");
            }
            ViewBag.userid = originalUserName;

            return View();
        }
        public ActionResult Comments(string userName, int? page = null)
        {
            if (page.HasValue && page.Value < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            var originalUserName = UserHelper.OriginalUsername(userName);
            if (String.IsNullOrEmpty(originalUserName))
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }
            ViewBag.userid = originalUserName;

            var userComments = from c in _db.Comments.OrderByDescending(c => c.CreationDate)
                                where c.UserName.Equals(originalUserName)
                                && !c.IsAnonymized
                                && !c.IsDeleted
                                //&& !c.Submission.Subverse1.IsAnonymized //Don't think we need this condition
                                select c;

            PaginatedList<Comment> paginatedUserComments = new PaginatedList<Comment>(userComments, page ?? 0, PAGE_SIZE);

            return View(paginatedUserComments);
        }
        public ActionResult Submissions(string userName, int? page = null)
        {
            if (page.HasValue && page.Value < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }


            var originalUserName = UserHelper.OriginalUsername(userName);
            if (String.IsNullOrEmpty(originalUserName))
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }
            ViewBag.userid = originalUserName;

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
                return View("~/Views/Errors/Error_404.cshtml");
            }

            var originalUserName = UserHelper.OriginalUsername(userName);
            if (String.IsNullOrEmpty(originalUserName))
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }
            if (!User.Identity.IsAuthenticated || (User.Identity.IsAuthenticated && !User.Identity.Name.Equals(originalUserName, StringComparison.OrdinalIgnoreCase)))
            {
                return RedirectToAction("Overview");
            }
            ViewBag.userid = originalUserName;

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
    }
}