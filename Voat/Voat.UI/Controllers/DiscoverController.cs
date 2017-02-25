using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
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
            var options = new SearchOptions(Request.QueryString, 20);

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