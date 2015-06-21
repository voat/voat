/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2014 Voat
All Rights Reserved.
*/

using System.Linq;
using System.Net;
using System.Web.Mvc;
using Voat.Models;
using Voat.Utils;

namespace Voat.Controllers
{
    public class SearchController : Controller
    {
        private readonly whoaverseEntities _db = new whoaverseEntities();

        [PreventSpam]
        [ValidateInput(false)]
        [OutputCache(Duration = 600, VaryByParam = "*")]
        public ActionResult SearchResults(int? page, string q, string l, string sub)
        {
            if (q == null || q.Length < 3) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (q == "rick roll")
            {
                return new RedirectResult("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            }

            if (q == "spoon")
            {
                return View("Jaje");
            }
            
            // limit the search to selected subverse
            if (l != null && sub != null)
            {
                // ViewBag.SelectedSubverse = string.Empty;
                ViewBag.SearchTerm = q;

                const int pageSize = 25;
                int pageNumber = (page ?? 0);

                if (pageNumber < 0)
                {
                    return View("~/Views/Errors/Error_404.cshtml");
                }

                // show search results, default sorting by rank and date
                var linkSubmissions = _db.Messages
                    .Where(x => x.Name != "deleted" & x.Subverse == sub & x.Linkdescription.ToLower().Contains(q))
                    .OrderByDescending(s => s.Rank)
                    .ThenByDescending(s => s.Date).Take(100);

                var selfSubmissionsTitle = _db.Messages
                    .Where(x => x.Name != "deleted" & x.Subverse == sub & x.Title.ToLower().Contains(q))
                    .OrderByDescending(s => s.Rank)
                    .ThenByDescending(s => s.Date).Take(100);

                var selfSubmissionsMessageContent = _db.Messages
                    .Where(x => x.Name != "deleted" & x.Subverse == sub & x.MessageContent.ToLower().Contains(q))
                    .OrderByDescending(s => s.Rank)
                    .ThenByDescending(s => s.Date).Take(100);

                var results = linkSubmissions.Concat(selfSubmissionsTitle);
                results = results.Concat(selfSubmissionsMessageContent).Distinct().OrderByDescending(s => s.Date);

                ViewBag.Title = "search results";

                var paginatedResults = new PaginatedList<Message>(results, page ?? 0, pageSize);

                return View("~/Views/Search/Index.cshtml", paginatedResults);
            }
            else
            {
                ViewBag.SelectedSubverse = string.Empty;
                ViewBag.SearchTerm = q;

                const int pageSize = 25;
                int pageNumber = (page ?? 0);

                if (pageNumber < 0)
                {
                    return View("~/Views/Errors/Error_404.cshtml");
                }

                // show search results, default sorting by rank and date
                var linkSubmissions = _db.Messages
                    .Where(x => x.Name != "deleted" & x.Linkdescription.ToLower().Contains(q))
                    .OrderByDescending(s => s.Rank)
                    .ThenByDescending(s => s.Date).Take(100);

                var selfSubmissionsTitle = _db.Messages
                    .Where(x => x.Name != "deleted" & x.Title.ToLower().Contains(q))
                    .OrderByDescending(s => s.Rank)
                    .ThenByDescending(s => s.Date).Take(100);

                var selfSubmissionsMessageContent = _db.Messages
                    .Where(x => x.Name != "deleted" & x.MessageContent.ToLower().Contains(q))
                    .OrderByDescending(s => s.Rank)
                    .ThenByDescending(s => s.Date).Take(100);

                var results = linkSubmissions.Concat(selfSubmissionsTitle);
                results = results.Concat(selfSubmissionsMessageContent).Distinct().OrderByDescending(s => s.Date);

                var paginatedResults = new PaginatedList<Message>(results, page ?? 0, pageSize);

                ViewBag.Title = "search results";

                return View("~/Views/Search/Index.cshtml", paginatedResults);
            }
        }

        [PreventSpam]
        [OutputCache(Duration = 600, VaryByParam = "*")]
        public ActionResult FindSubverse(int? page, string d, string q)
        {
            if (q == null || q.Length < 3) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            IQueryable<Subverse> results;
            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SearchTerm = q;

            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // find a subverse by name and/or description, sort search results by number of subscribers
            var subversesByName = _db.Subverses
                .Where(s => s.name.ToLower().Contains(q))
                .OrderByDescending(s => s.subscribers);

            if (d != null)
            {
                var subversesByDescription = _db.Subverses
                    .Where(s => s.description.ToLower().Contains(q))
                    .OrderByDescending(s => s.subscribers);

                results = subversesByName.Concat(subversesByDescription).OrderByDescending(s=>s.subscribers);
            }
            else
            {
                results = subversesByName.OrderByDescending(s => s.subscribers);
            }

            ViewBag.Title = "Search results";

            var paginatedResults = new PaginatedList<Subverse>(results, page ?? 0, pageSize);

            return View("~/Views/Search/FindSubverseSearchResult.cshtml", paginatedResults);
        }
    }
}