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
                var userSubmissions = from b in _db.Messages.OrderByDescending(s => s.Date)
                                      where (b.Name.Equals(id) && b.Anonymized == false) && (b.Name.Equals(id) && b.Subverses.anonymized_mode == false)
                                      select b;

                PaginatedList<Message> paginatedUserSubmissions = new PaginatedList<Message>(userSubmissions, page ?? 0, pageSize);

                return View("~/Views/Home/UserSubmitted.cshtml", paginatedUserSubmissions);
        }

        // GET: show user comments
        // this method is a STUB and not used anywhere
        [ChildActionOnly]
        [OutputCache(Duration = 600, VaryByParam = "*")]
        public ActionResult UserComments(string id, int? page, string whattodisplay)
        {
                var userComments = from c in _db.Comments.OrderByDescending(c => c.Date)
                                   where (c.Name.Equals(id) && c.Message.Anonymized == false) && (c.Name.Equals(id) && c.Message.Subverses.anonymized_mode == false)
                                   select c;

                PaginatedList<Comment> paginatedUserComments = new PaginatedList<Comment>(userComments, page ?? 0, pageSize);

                return View("~/Views/Home/UserComments.cshtml", paginatedUserComments);
        }
    }
}