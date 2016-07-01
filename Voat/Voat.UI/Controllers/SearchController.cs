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
using System.Web.Mvc;
using Voat.Caching;
using Voat.Data.Models;
using Voat.UI.Utilities;
using Voat.Utilities;


namespace Voat.Controllers
{
    public class SearchController : Controller
    {
        //IAmAGate: Move queries to read-only mirror
        private readonly voatEntities _db = new voatEntities(CONSTANTS.CONNECTION_READONLY);

        [PreventSpam]
        public ActionResult SearchResults(int? page, string q, string l, string sub)
        {
            
            if (q == null || q.Length < 3) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //sanitize
            q = q.Trim();

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
                
                string cacheKey = DataCache.Keys.Search(sub, q);
                IList<Submission> cacheData = CacheHandler.Instance.Retrieve<IList<Submission>>(cacheKey);
                if (cacheData == null) {
                    cacheData = (IList<Submission>)CacheHandler.Instance.Register(cacheKey, new Func<object>(() =>
                    {
                        using (var db = new voatEntities())
                        {
                            db.Configuration.LazyLoadingEnabled = false;
                            db.Configuration.ProxyCreationEnabled = false;
                            var results = (from m in db.Submissions
                                           join s in db.Subverses on m.Subverse equals s.Name
                                           where
                                            !s.IsAdminDisabled.Value &&
                                            !m.IsDeleted &&
                                            m.Subverse == sub &&
                                            (m.LinkDescription.ToLower().Contains(q) || m.Content.ToLower().Contains(q) || m.Title.ToLower().Contains(q))
                                           orderby m.Rank ascending, m.CreationDate descending
                                           select m).Take(25).ToList();
                            return results;
                        }
                    }), TimeSpan.FromMinutes(10));

                }

                ViewBag.Title = "search results";

                var paginatedResults = new PaginatedList<Submission>(cacheData, 0, pageSize, 24); //HACK: To turn off paging 

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

                string cacheKey = DataCache.Keys.Search(q);
                IList<Submission> cacheData = CacheHandler.Instance.Retrieve<IList<Submission>>(cacheKey);
                if (cacheData == null)
                {
                    cacheData = (IList<Submission>)CacheHandler.Instance.Register(cacheKey, new Func<object>(() =>
                    {
                        using (var db = new voatEntities())
                        {
                            db.Configuration.LazyLoadingEnabled = false;
                            db.Configuration.ProxyCreationEnabled = false;
                            var results = (from m in db.Submissions
                                           join s in db.Subverses on m.Subverse equals s.Name
                                           where
                                            !s.IsAdminDisabled.Value &&
                                            !m.IsDeleted &&
                                            //m.Subverse == sub &&
                                            (m.LinkDescription.ToLower().Contains(q) || m.Content.ToLower().Contains(q) || m.Title.ToLower().Contains(q))
                                           orderby m.Rank ascending, m.CreationDate descending
                                           select m
                                    ).Take(25).ToList();
                            return results;
                        }
                        
                    }), TimeSpan.FromMinutes(10));

                }

                var paginatedResults = new PaginatedList<Submission>(cacheData, 0, pageSize, 24);//HACK to stop paging

                ViewBag.Title = "search results";

                return View("~/Views/Search/Index.cshtml", paginatedResults);
            }
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
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // find a subverse by name and/or description, sort search results by number of subscribers
            var subversesByName = _db.Subverses
                .Where(s => s.Name.ToLower().Contains(q) && (s.IsAdminDisabled != true))
                .OrderByDescending(s => s.SubscriberCount);

            if (d != null)
            {
                var subversesByDescription = _db.Subverses
                    .Where(s => s.Description.ToLower().Contains(q))
                    .OrderByDescending(s => s.SubscriberCount);

                results = subversesByName.Concat(subversesByDescription).OrderByDescending(s=>s.SubscriberCount);
            }
            else
            {
                results = subversesByName.OrderByDescending(s => s.SubscriberCount);
            }

            ViewBag.Title = "Search results";

            var paginatedResults = new PaginatedList<Subverse>(results, page ?? 0, pageSize);

            return View("~/Views/Search/FindSubverseSearchResult.cshtml", paginatedResults);
        }
    }
}