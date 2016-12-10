/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat, Inc.
All Rights Reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Query;
using Voat.UI.Utilities;
using Voat.Utilities;


namespace Voat.Controllers
{
    public class SearchController : BaseController
    {
        //IAmAGate: Move queries to read-only mirror
        private readonly voatEntities _db = new voatEntities(CONSTANTS.CONNECTION_READONLY);
        
        //TODO: Port to submission search alg
        [PreventSpam]
        public async Task<ActionResult> SearchResults(int? page, string q, string l, string sub)
        {
            //sanitize
            q = q.TrimSafe();

            if (String.IsNullOrWhiteSpace(q) || q.Length < 3)
            {
                return View("~/Views/Search/Index.cshtml", new PaginatedList<Submission>(new List<Submission>(), 0, 25, 24));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            if (q == "rick roll")
            {
                return new RedirectResult("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            }

            if (q == "spoon")
            {
                return View("Jaje");
            }

            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return NotFoundErrorView();
            }

            var subverse = AGGREGATE_SUBVERSE.ANY;

            // limit the search to selected subverse
            if (l != null && sub != null)
            {
                subverse = sub;
            }
              
            var options = new SearchOptions();
            options.Phrase = q;
            options.Sort = Domain.Models.SortAlgorithm.Top;

            var query = new QuerySubmissionsLegacy(subverse, options, new CachePolicy(TimeSpan.FromMinutes(60)));
            var results = await query.ExecuteAsync().ConfigureAwait(false);
            var paginatedResults = new PaginatedList<Submission>(results, 0, pageSize, 24); //HACK: To turn off paging 

            ViewBag.Title = "search results";
            ViewBag.SearchTerm = q;

            return View("~/Views/Search/Index.cshtml", paginatedResults);
        }

        [PreventSpam]
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
                return NotFoundErrorView();
            }

            // find a subverse by name and/or description, sort search results by number of subscribers
            results = _db.Subverses.Where(s => s.IsAdminDisabled != true);
            if (d != null)
            {
                results = results.Where(x => x.Name.ToLower().Contains(q) || x.Description.ToLower().Contains(q));
            }
            else
            {
                results = results.Where(x => x.Name.ToLower().Contains(q));
            }
            results = results.OrderByDescending(s => s.SubscriberCount);

            ViewBag.Title = "Search results";

            var paginatedResults = new PaginatedList<Subverse>(results, page ?? 0, pageSize);

            return View("~/Views/Search/FindSubverseSearchResult.cshtml", paginatedResults);
        }
    }
}