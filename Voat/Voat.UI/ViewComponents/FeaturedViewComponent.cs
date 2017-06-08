using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Data;

namespace Voat.UI.ViewComponents
{
    public class FeaturedViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            //TODO: Move to a query that is cached
            using (var repo = new Repository())
            {
                var featured = await repo.GetFeatured();
                if (featured != null)
                {
                    return View(featured);
                }
                else
                {
                    return Content(string.Empty);
                }
            }
        }
    }
}
