using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Utilities;

namespace Voat.UI.ViewComponents
{
    public class TopViewedSubmissionsViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(DomainReference domainReference)
        {
            var cacheData = CacheHandler.Instance.Register("legacy:TopViewedSubmissions24Hours", new Func<object>(() =>
            {
                using (var db = new VoatOutOfRepositoryDataContextAccessor(CONSTANTS.CONNECTION_READONLY))
                {
                    var startDate = Repository.CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));
                    IQueryable<Data.Models.Submission> submissions =
                        (from message in db.Submission
                         join subverse in db.Subverse on message.Subverse equals subverse.Name
                         where message.ArchiveDate == null && !message.IsDeleted && subverse.IsPrivate != true && subverse.IsAdminPrivate != true && subverse.IsAdult == false && message.CreationDate >= startDate && message.CreationDate <= Repository.CurrentDate
                         where !(from bu in db.BannedUser select bu.UserName).Contains(message.UserName)
                         where !subverse.IsAdminDisabled.Value
                         //where !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(subverse.Name) select ubs.UserName).Contains(User.Identity.Name)
                         select message).OrderByDescending(s => s.Views).Take(5).AsQueryable();
                    //select message).OrderByDescending(s => s.Views).DistinctBy(m => m.Subverse).Take(5).AsQueryable().AsNoTracking();

                    return submissions.ToList();
                    
                }
            }), TimeSpan.FromMinutes(60), 5);

            return View(cacheData);
        }
    }
}
