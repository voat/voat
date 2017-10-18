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

using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Query;
using Voat.Models.ViewModels;
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
                    return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseDisabled));
                }
            }
            var options = new SearchOptions(Request.QueryString.Value);
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
                    return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseDisabled));
                }
            }

            var options = new SearchOptions(Request.QueryString.Value);
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
                    return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseDisabled));
                }
            }

            var options = new SearchOptions(Request.QueryString.Value);
            var q = new QueryModLogBannedUsers(subverse, options);
            var results = await q.ExecuteAsync();

            //CORE_PORT: This is bad mmkay
            ViewBag.TotalBannedUsersInSubverse = results.Item1;
            //using (var db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
            //{
            //    ViewBag.TotalBannedUsersInSubverse = db.SubverseBans.Where(rl => rl.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase)).Count();
            //}

            var list = new PaginatedList<Domain.Models.SubverseBan>(results.Item2, options.Page, options.Count);

            return View(list);
        }
    }
}
