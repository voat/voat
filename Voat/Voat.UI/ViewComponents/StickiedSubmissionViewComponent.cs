using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Utilities;

namespace Voat.UI.ViewComponents
{
    public class StickiedSubmissionViewComponent : ViewComponent
    {
        private object subverse;

        public async Task<IViewComponentResult> InvokeAsync(DomainReference domainReference)
        {

            //Use default stickies for site
            if (domainReference == null)
            {
                domainReference = new DomainReference(DomainType.Subverse, "announcements");
            }

            var q = new QueryStickies(domainReference.Name).SetUserContext(User);
            var stickies = await q.ExecuteAsync();


            if (stickies != null)
            {
                return View(stickies);
            }
            else
            {
                //Empty
                return Content(string.Empty);
            }
        }
    }
}
