using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Domain.Models;
using Voat.Domain.Query;

namespace Voat.UI.ViewComponents
{
    public class SubverseModeratorListViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(DomainReference domainReference)
        {
            var q = new QuerySubverseModerators(domainReference.Name);
            var r = await q.ExecuteAsync();
            return View(r);
        }
    }
}
