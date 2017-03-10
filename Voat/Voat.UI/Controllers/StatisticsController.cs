using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Voat.Data;
using Voat.Domain.Query.Statistics;
using Voat.Utilities;

namespace Voat.Controllers
{
    public class StatisticsController : BaseController
    {
        [HttpGet]
        [Route("statistics/user/votes/received")]
        public async Task<ActionResult> UserVotesReceived()
        {

            var q = new QueryUserVotesReceived();
            var result = await q.ExecuteAsync();

            ViewBag.NavigationViewModel = new Models.ViewModels.NavigationViewModel()
            {
                BasePath = "statistics",
                MenuType = Models.ViewModels.MenuType.Statistics,
                Description = "Voat Statistics",
                Name = "User Votes Received"
            };

            return View(result);
        }

        [HttpGet]
        [Route("statistics/user/votes/given")]
        public async Task<ActionResult> UserVotesGiven()
        {
            var q = new QueryUserVotesGiven();
            var result = await q.ExecuteAsync();

            ViewBag.NavigationViewModel = new Models.ViewModels.NavigationViewModel() {
                    BasePath = "statistics",
                    MenuType = Models.ViewModels.MenuType.Statistics,
                    Description = "Voat Statistics",
                    Name = "User Votes Given"
                };

            return View(result);
        }
        [Route("statistics/content/votes")]
        [Route("statistics")]
        public async Task<ActionResult> HighestVotedContent()
        {

            var q = new QueryHighestVotedContent();
            var result = await q.ExecuteAsync();

            //Hydrate content?
            //Domain.DomainMaps.HydrateUserData(result.Data.Where(x => x.Submission != null).Select(x => x.Submission));
            //Domain.DomainMaps.HydrateUserData(result.Data.Where(x => x.Comment != null).Select(x => x.Comment));

            ViewBag.NavigationViewModel = new Models.ViewModels.NavigationViewModel()
            {
                BasePath = "statistics",
                MenuType = Models.ViewModels.MenuType.Statistics,
                Description = "Voat Statistics",
                Name = "Highest Rated Content"
            };

            return View(result);
        }
    }
}