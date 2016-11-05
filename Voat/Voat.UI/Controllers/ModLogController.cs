using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Query;
using Voat.Utilities;

namespace Voat.Controllers
{
    public class ModLogController : BaseController
    {

        public async Task<ActionResult> Submissions(string subverse)
        {
            ViewBag.SelectedSubverse = subverse;

            var subverseObject = DataCache.Subverse.Retrieve(subverse);
            if (subverseObject != null)
            {
                //HACK: Disable subverse
                if (subverseObject.IsAdminDisabled.HasValue && subverseObject.IsAdminDisabled.Value)
                {
                    ViewBag.Subverse = subverseObject.Name;
                    return SubverseDisabledErrorView();
                }
            }
            var options = new SearchOptions(Request.Url.Query);
            var q = new QueryModLogRemovedSubmissions(subverse, options);
            var results = await q.ExecuteAsync();

            var list = new PaginatedList<SubmissionRemovalLog>(results, options.Page, options.Count);

            return View(list);
        }
        public async Task<ActionResult> Comments(string subverse)
        {
            ViewBag.SelectedSubverse = subverse;

            var subverseObject = DataCache.Subverse.Retrieve(subverse);
            if (subverseObject != null)
            {
                ViewBag.SubverseAnonymized = subverseObject.IsAnonymized;

                //HACK: Disable subverse
                if (subverseObject.IsAdminDisabled.HasValue && subverseObject.IsAdminDisabled.Value)
                {
                    ViewBag.Subverse = subverseObject.Name;
                    return SubverseDisabledErrorView();
                }
            }

            var options = new SearchOptions(Request.Url.Query);
            var q = new QueryModLogRemovedComments(subverse, options);
            var results = await q.ExecuteAsync();

            var list = new PaginatedList<Domain.Models.CommentRemovalLog>(results, options.Page, options.Count);

            return View(list);
        }
        public async Task<ActionResult> Banned(string subverse)
        {
            ViewBag.SelectedSubverse = subverse;

            var subverseObject = DataCache.Subverse.Retrieve(subverse);
            if (subverseObject != null)
            {
                //HACK: Disable subverse
                if (subverseObject.IsAdminDisabled.HasValue && subverseObject.IsAdminDisabled.Value)
                {
                    ViewBag.Subverse = subverseObject.Name;
                    return SubverseDisabledErrorView();
                }
            }
            var options = new SearchOptions(Request.Url.Query);
            var q = new QueryModLogBannedUsers(subverse, options);
            var results = await q.ExecuteAsync();
            using (var db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
            {
                ViewBag.TotalBannedUsersInSubverse = db.SubverseBans.Where(rl => rl.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase)).Count();
            }

            var list = new PaginatedList<Domain.Models.SubverseBan>(results, options.Page, options.Count);

            return View(list);
        }
    }
}