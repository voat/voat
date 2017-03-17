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
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Query;
using Voat.Models.ViewModels;
using Voat.Utilities;

namespace Voat.Controllers
{
    public class DomainsController : BaseController
    {
       
        public async Task<ActionResult> Index(int? page, string domainName, string sortingMode)
        {
            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0 || String.IsNullOrWhiteSpace(domainName) || pageNumber > 9)
            {
                return NotFoundErrorView();
            }
            if (domainName.Length < 4)
            {
                return RedirectToAction("UnAuthorized", "Error");
            }

            sortingMode = (sortingMode == "new" ? "new" : "hot");

            ViewBag.SelectedSubverse = "domains";
            ViewBag.SelectedDomain = domainName;
            domainName = domainName.Trim().ToLower();

            //TODO: This needs to moved to Query/Repository
            var options = new SearchOptions();
            options.Page = pageNumber;
            options.Sort = sortingMode == "new" ? Domain.Models.SortAlgorithm.New : Domain.Models.SortAlgorithm.Hot;

            var q = new QuerySubmissionsByDomain(domainName, options);
            var results = await q.ExecuteAsync();
            //var results = CacheHandler.Instance.Register(CachingKey.DomainSearch(domainname, pageNumber, sortingmode), () => {
            //    using (var db = new voatEntities())
            //    {
            //        db.EnableCacheableOutput();

            //        //restrict disabled subs from result list
            //        IQueryable<Submission> q = (from m in db.Submissions
            //                                              join s in db.Subverses on m.Subverse equals s.Name
            //                                              where
            //                                              !s.IsAdminDisabled.Value
            //                                              && !m.IsDeleted
            //                                              && m.Type == 2
            //                                              && m.Url.ToLower().Contains(domainname)
            //                                              select m);

            //        if (sortingmode == "new")
            //        {
            //            ViewBag.SortingMode = sortingmode;
            //            q = q.OrderByDescending(x => x.CreationDate);
            //        }
            //        else
            //        {
            //            ViewBag.SortingMode = "hot";
            //            q = q.OrderByDescending(x => x.Rank).ThenByDescending(x => x.CreationDate);
            //        }

            //        var result = q.Skip(pageNumber * pageSize).Take(pageSize).ToList();

            //        return result;
            //    }
            //}, TimeSpan.FromMinutes(60));

            var paginatedSubmissions = new PaginatedList<Domain.Models.Submission>(results, page ?? 0, pageSize, -1);

            var viewProperties = new SubmissionListViewModel();
            viewProperties.Submissions = paginatedSubmissions;
            viewProperties.Submissions.RouteName = "DomainIndex";

            viewProperties.Title = "Domain: " + domainName;

            ViewBag.NavigationViewModel = new NavigationViewModel()
            {
                Description = "Domain " + domainName,
                Name = domainName,
                MenuType = MenuType.Domain,
                BasePath = $"/domains/{domainName}",
                Sort = options.Sort
            };
            return View(VIEW_PATH.SUBMISSION_LIST, viewProperties);
        }
    }
}
