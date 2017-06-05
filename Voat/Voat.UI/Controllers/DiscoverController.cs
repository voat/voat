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
using System.Threading.Tasks;
using System.Web;

using Voat.Data;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Models.ViewModels;
using Voat.Utilities;

namespace Voat.Controllers
{
    public class DiscoverController : Controller
    {

        // GET: Discover
        public ActionResult Search(DomainType? domainType = null, SortAlgorithm? sort = null)
        {

            var type = domainType.HasValue ? domainType.Value : DomainType.Subverse;
            var sortValue = sort.HasValue ? sort.Value : SortAlgorithm.Hot;

            ViewBag.DomainType = type;

            ViewBag.NavigationViewModel = new Models.ViewModels.NavigationViewModel()
            {
                Description = "Discover Search",
                Name = "No Idea",
                MenuType = Models.ViewModels.MenuType.Discovery,
                BasePath = null,
                Sort = sortValue
            };

            var page = new PaginatedList<DomainReferenceDetails>(Enumerable.Empty<DomainReferenceDetails>(), 0, 30);

            return View("Index", page);
        }

        // GET: Discover
        public async Task<ActionResult> Index(DomainType? domainType = null, SortAlgorithm? sort = null)
        {
            var options = new SearchOptions(Request.QueryString.Value, 20);

            var type = domainType.HasValue ? domainType.Value : DomainType.Subverse;
            var sortValue = sort.HasValue ? sort.Value : SortAlgorithm.Hot;

            //Lets makes sure we don't get crazy inputs
            options.Sort = sortValue;
            options.Count = 30;

            var q = new QueryDomainObject(type, options);
            var results = await q.ExecuteAsync();

            var page = new PaginatedList<DomainReferenceDetails>(results, options.Page, options.Count);

            ViewBag.NavigationViewModel = new Models.ViewModels.NavigationViewModel()
            {
                Description = "Discover ",
                Name = "No Idea",
                MenuType = Models.ViewModels.MenuType.Discovery,
                BasePath = null,
                Sort = sortValue
            };
            ViewBag.DomainType = type;
            return View(page);
        }
    }
}
