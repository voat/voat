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

using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Http;
using Voat.Models.ViewModels;

namespace Voat.Controllers
{
    public class DashboardController : BaseController
    {
        public async Task<ActionResult> Menu()
        {
            //var showDashboard = !Request.IsCookiePresent("menu", "legacy", this.Response);

            var showDashboard = (Request.Cookies["legacy"] == null);

            //Temp logic, parse querystring, allows showing both menus
            var menuQueryString = Request.Query["menu"];
            if (!String.IsNullOrEmpty(menuQueryString))
            {
                if (menuQueryString.Equals("dashboard"))
                {
                    Response.SetCookie(new HttpCookie("legacy") { Expires = DateTime.UtcNow.AddDays(-7) });
                    showDashboard = true;
                }
                else if (menuQueryString.Equals("legacy"))
                {
                    Response.SetCookie(new HttpCookie("legacy") { Expires = DateTime.UtcNow.AddDays(7) });
                    showDashboard = false;
                }
            }

            if (showDashboard)
            {
                return await Dashboard();
            }
            else
            {
                return await LegacyMenu();
            }
        }
        /// <summary>
        /// Represents the traditional menu
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> LegacyMenu()
        {
            IEnumerable<string> subs = null;
            if (User.Identity.IsAuthenticated && UserData.HasSubscriptions(Domain.Models.DomainType.Subverse) && UserData.Preferences.UseSubscriptionsMenu)
            {

                var q = new QueryUserSubscriptions(User.Identity.Name);
                var results = await q.ExecuteAsync();
                subs = results[Domain.Models.DomainType.Subverse];
                subs = subs.OrderBy(x => x);
            }
            else
            {
                var q = new QueryDefaultSubverses();
                var r = q.Execute();
                subs = r.Select(x => x.Name).ToList();
            }

            return PartialView("_LegacyMenu", subs);
        }

        public async Task<ActionResult> Dashboard()
        {

            var dashboardViewModel = new DashboardViewModel();

            if (User.Identity.IsAuthenticated && UserData.HasSubscriptions(Domain.Models.DomainType.Subverse))
            {
                dashboardViewModel.Subscriptions = UserData.Subscriptions;
                if (UserData.Preferences.UseSubscriptionsMenu)
                {
                    dashboardViewModel.TopBar = dashboardViewModel.Subscriptions[DomainType.Subverse].Select(x => new DomainReference(DomainType.Subverse, x));
                }
                else
                {
                    //use defaults 
                    var q = new QueryDefaultSubverses();
                    var r = await q.ExecuteAsync();
                    dashboardViewModel.TopBar = r.Select(x => new DomainReference(DomainType.Subverse, x.Name));
                }
                
            }
            else
            {
                //defaults 
                Dictionary<DomainType, IEnumerable<string>> dict = new Dictionary<DomainType, IEnumerable<string>>();

                var q = new QueryDefaultSubverses();
                var r = await q.ExecuteAsync();
                //subs = r.Select(x => x.Name).ToList();
                dict[DomainType.Subverse] = r.Select(x => x.Name);

                //hard coded sets
                dict[DomainType.Set] = (new string[] { "Default", "Newsy", "Sports", "Music" }).AsEnumerable();

                dashboardViewModel.Subscriptions = dict;

                dashboardViewModel.TopBar = dict[DomainType.Subverse].Select(x => new DomainReference(DomainType.Subverse, x));
            }

            return PartialView("_Dashboard", dashboardViewModel);


        }

    }
}
