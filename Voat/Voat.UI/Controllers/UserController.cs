using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Voat.Data.Models;
using Voat.Models.ViewModels;
using Voat.Utilities;

namespace Voat.Controllers
{
    public class UserController : Controller
    {
        private readonly voatEntities _db = new voatEntities();
        const int pageSize = 25;

        // GET: show user submissions
        // this method is a STUB and not used anywhere
        [ChildActionOnly]
        [OutputCache(Duration = 600, VaryByParam = "*")]
        public ActionResult UserSubmissions(string id, int? page, string whattodisplay)
        {
                var userSubmissions = from b in _db.Submissions.OrderByDescending(s => s.CreationDate)
                                      where (b.UserName.Equals(id) && b.IsAnonymized == false) && (b.UserName.Equals(id) && b.Subverse1.IsAnonymized == false)
                                      select b;

                PaginatedList<Submission> paginatedUserSubmissions = new PaginatedList<Submission>(userSubmissions, page ?? 0, pageSize);

                return View("~/Views/Home/UserSubmitted.cshtml", paginatedUserSubmissions);
        }

        // GET: show user comments
        // this method is a STUB and not used anywhere
        [ChildActionOnly]
        [OutputCache(Duration = 600, VaryByParam = "*")]
        public ActionResult UserComments(string id, int? page, string whattodisplay)
        {
                var userComments = from c in _db.Comments.OrderByDescending(c => c.CreationDate)
                                   where (c.UserName.Equals(id) && c.Submission.IsAnonymized == false) && (c.UserName.Equals(id) && c.Submission.Subverse1.IsAnonymized == false)
                                   select c;

                PaginatedList<Comment> paginatedUserComments = new PaginatedList<Comment>(userComments, page ?? 0, pageSize);

                return View("~/Views/Home/UserComments.cshtml", paginatedUserComments);
        }
    }
}