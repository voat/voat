using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Controllers;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Query;
using Voat.Utilities;

namespace Voat.UI.Controllers
{
    public class LogController : BaseController
    {
        public async Task<ActionResult> BannedUsers()
        {
            var options = new SearchOptions(this.Request.QueryString.Value);
            var q = new QueryBannedUsers();
            var results = await q.ExecuteAsync();

            if (!String.IsNullOrEmpty(options.Phrase))
            {
                results = results.Where(x => x.UserName.ToLower().Contains(options.Phrase.ToLower()) || x.Reason.ToLower().Contains(options.Phrase.ToLower()));
            }

            var page = results.Skip(options.Index).Take(options.Count);
            
            var p = new PaginatedList<BannedUser>(page, options.Page, options.Count);
            return View(p);
        }

        public async Task<ActionResult> BannedDomains()
        {
            var options = new SearchOptions(this.Request.QueryString.Value);
            var q = new QueryBannedDomains();
            var results = await q.ExecuteAsync();

            if (!String.IsNullOrEmpty(options.Phrase))
            {
                results = results.Where(x => x.Domain.ToLower().Contains(options.Phrase.ToLower()) || x.Reason.ToLower().Contains(options.Phrase.ToLower()));
            }

            var page = results.Skip(options.Index).Take(options.Count);

            var p = new PaginatedList<BannedDomain>(page, options.Page, options.Count);
            return View(p);
        }

    }
}
