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

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Voat.Caching;
using Voat.Common;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.UI.Utilities;
using Voat.Utilities;


namespace Voat.Controllers
{
    public class SearchController : BaseController
    {
        //TODO: Port to submission search alg
        [PreventSpam]
        public async Task<ActionResult> SearchResults(int? page, string q, string l, string sub)
        {
            if (VoatSettings.Instance.SearchDisabled)
            {
                return base.GenericErrorView(new Models.ViewModels.ErrorViewModel() { Title = "Search Disabled", Description = "Sorry, search is currently disabled. :(", FooterMessage = "Tune in for The People vs. Search court case" });
            }
            //sanitize
            q = q.TrimSafe();

            if (String.IsNullOrWhiteSpace(q) || q.Length < 3)
            {
                return View("~/Views/Search/Index.cshtml", new PaginatedList<Data.Models.Submission>(new List<Data.Models.Submission>(), 0, 25, 24));
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

            var query = new QuerySubmissions(new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, subverse), options, new CachePolicy(TimeSpan.FromMinutes(60)));
            var results = await query.ExecuteAsync().ConfigureAwait(false);
            var paginatedResults = new PaginatedList<Domain.Models.Submission>(results, 0, pageSize, 24); //HACK: To turn off paging 

            ViewBag.Title = "search results";
            ViewBag.SearchTerm = q;

            return View("~/Views/Search/Index.cshtml", paginatedResults);
        }

        ////THIS IS A DISCOVERY METHOD REDIRECTED FROM /subverses/search
        //[PreventSpam]
        //public async Task<ActionResult> FindSubverse(int? page, string d, string q)
        //{
        //    ViewBag.SearchTerm = q;
        //    ViewBag.Title = "Search results";
        //    page = page.HasValue ? page.Value : 0;

        //    var options = new SearchOptions() { Phrase = q, Page = page.Value, Count = 30 };
        //    var query = new QueryDomainObject(Domain.Models.DomainType.Subverse, options);
        //    var results = await query.ExecuteAsync();

        //    var paginatedResults = new PaginatedList<DomainReferenceDetails>(results, options.Page, options.Count);

        //    ViewBag.NavigationViewModel = new Models.ViewModels.NavigationViewModel()
        //    {
        //        Description = "Search Subverses",
        //        Name = "Subverses",
        //        MenuType = Models.ViewModels.MenuType.Discovery,
        //        BasePath = null,
        //        Sort = null
        //    };
        //    return View("~/Views/Search/DomainSearchResults.cshtml", paginatedResults);
        //}
    }
}
