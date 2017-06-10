using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.UI.Utilities;

namespace Voat.UI.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(DomainReference domainReference) //, ContentReference contentReference = null
        {
            IViewComponentResult result = View("Default", domainReference);

            switch (domainReference.Type)
            {
                case DomainType.Subverse:
                    Subverse subverse = null;

                    if (!String.IsNullOrEmpty(domainReference.Name))
                    {
                        var q = new QuerySubverse(domainReference.Name);
                        subverse = await q.ExecuteAsync();
                    }
                    if (subverse != null)
                    {
                        var qa = new QueryActiveSessionCount(domainReference);
                        ViewBag.OnlineUsers = await qa.ExecuteAsync();

                        var view = "Subverse";// contentReference != null && contentReference.Type == ContentType.Submission ? "Submission" : "Subverse";

                        result = View(view, subverse);
                    }
                    else
                    {
                        result = View("Default");
                    }
                    break;
                case DomainType.User:
                    result = View("User", domainReference);
                    break;
                case DomainType.Set:
                    result = View("Set", domainReference);
                    break;
            }

            return result;
            return View("Chat", domainReference);
        }
    }
}
