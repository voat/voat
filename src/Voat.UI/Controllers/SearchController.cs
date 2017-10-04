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
using Voat.Http.Filters;
using Voat.Models.ViewModels;
using Voat.UI.Utilities;
using Voat.Utilities;


namespace Voat.Controllers
{
    public class SearchController : BaseController
    {
        [PreventSpam]
        [HttpGet]
        public async Task<ActionResult> SearchResults(int? page, string q, string l, string sub)
        {

            if (!VoatSettings.Instance.SearchEnabled)
            {
                return base.ErrorView(new Models.ViewModels.ErrorViewModel() { Title = "Search Disabled", Description = "Sorry, search is currently disabled. :(", Footer = "Tune in for The People vs. Search court case" });
            }
            //sanitize
            q = q.TrimSafe();
            
            if (String.IsNullOrWhiteSpace(q) || q.Length < 3 || q.Length > 50)
            {
                return ErrorView(new ErrorViewModel() { Title = "Your search found one result: An error", Description = "Search phrases are required to be between 3 and 50 characters", Footer = "Got it?" });
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
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
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

            var query = new QuerySubmissions(new Domain.Models.DomainReference(Domain.Models.DomainType.Subverse, subverse), options, new CachePolicy(TimeSpan.FromMinutes(60))).SetUserContext(User);
            var results = await query.ExecuteAsync().ConfigureAwait(false);
            var paginatedResults = new PaginatedList<Domain.Models.Submission>(results, 0, pageSize, 24); //HACK: To turn off paging 

            ViewBag.Title = "search results";
            ViewBag.SearchTerm = q;

            return View("~/Views/Search/Index.cshtml", paginatedResults);
        }
    }
}
