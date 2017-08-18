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


namespace Voat.Controllers
{
    public class NavController : BaseController
    {
        // GET: TabMenu
        
        public PartialViewResult TabMenu(string selectedSubverse, string sortingMode, string action, bool? frontpage, string selectedSubverseName)
        {
            ViewBag.selectedSubverse = selectedSubverse;
            ViewBag.sortingMode = sortingMode;
            ViewBag.action = action;
            ViewBag.frontpage = frontpage;
            ViewBag.selectedSubverseName = selectedSubverseName;

            return PartialView("~/Views/Shared/Navigation/_SubverseMenu.cshtml");
        }

        // GET: PmMenu
        
        public PartialViewResult PmMenu(string selectedView)
        {
            ViewBag.selectedView = selectedView;

            return PartialView("~/Views/Shared/Navigation/_PmMenu.cshtml");
        }

        // GET: UserProfileMenu
        
        public PartialViewResult UserProfileMenu(string whattodisplay, string selectedUser)
        {
            ViewBag.whattodisplay = whattodisplay;           
            ViewBag.selectedUser = selectedUser;

            return PartialView("~/Views/Shared/Navigation/_UserProfileMenu.cshtml");
        }

        // GET: SubversesMenu
        
        public PartialViewResult SubversesMenu(string sortingMode, string subversesView)
        {
            ViewBag.sortingMode = sortingMode;
            ViewBag.SubversesView = subversesView;

            return PartialView("~/Views/Shared/Navigation/_SubversesMenu.cshtml");
        }

        // GET: DomainsMenu
        
        public PartialViewResult DomainsMenu(string sortingMode, string selectedSubverse, string selectedDomain)
        {
            ViewBag.sortingMode = sortingMode;
            ViewBag.selectedSubverse = selectedSubverse;
            ViewBag.selectedDomain = selectedDomain;

            return PartialView("~/Views/Shared/Navigation/_DomainsMenu.cshtml");
        }
    }
}
