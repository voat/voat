using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            Submission sticky = null;

            //Use default stickies for site
            if (domainReference == null)
            {
                domainReference = new DomainReference(DomainType.Subverse, "announcements");
            }

            var q = new QueryStickies(domainReference.Name);
            var stickies = await q.ExecuteAsync();


            sticky = stickies.FirstOrDefault();

            if (sticky != null)
            {
                return View(sticky);
            }
            else
            {
                //Empty
                return Content(string.Empty);
            }
        }
    }
}
