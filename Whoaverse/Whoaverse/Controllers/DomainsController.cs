/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
*/

using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whoaverse.Models;

namespace Whoaverse.Controllers
{
    public class DomainsController : Controller
    {
        private whoaverseEntities db = new whoaverseEntities();

        // GET: Domains
        // GET: all submissions which link to given domain
        public ActionResult Index(int? page, string domainname, string ext, string sortingmode)
        {
            int pageSize = 25;
            int pageNumber = (page ?? 1);

            ViewBag.SelectedSubverse = "domains";
            ViewBag.SelectedDomain = domainname + "." + ext;

            //check if at least one submission for given domain was found, if not, send to a page not found error
            var submissions = db.Messages
                        .Where(x => x.Name != "deleted" & x.Type == 2 & x.MessageContent.ToLower().Contains(domainname + "." + ext))
                        .OrderByDescending(s => s.Rank)
                        .ThenByDescending(s => s.Date).Take(100).ToList();

            if (submissions != null)
            {
                ViewBag.Title = "Showing all submissions which link to " + domainname;
                return View("Index", submissions.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                ViewBag.SelectedSubverse = "404";
                return View("~/Views/Errors/Subversenotfound.cshtml");
            }
        }

        public ActionResult @New(int? page, string domainname, string ext, string sortingmode)
        {
            //sortingmode: new, contraversial, hot, etc
            ViewBag.SortingMode = sortingmode;

            ViewBag.SelectedSubverse = "domains";
            ViewBag.SelectedDomain = domainname + "." + ext;

            int pageSize = 25;
            int pageNumber = (page ?? 1);

            //check if at least one submission for given domain was found, if not, send to a page not found error
            var submissions = db.Messages
                        .Where(x => x.Name != "deleted" & x.Type == 2 & x.MessageContent.ToLower().Contains(domainname + "." + ext))
                        .OrderByDescending(s => s.Date).Take(100).ToList();

            if (submissions != null)
            {
                ViewBag.Title = "Showing all newest submissions which link to " + domainname;
                return View("Index", submissions.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                ViewBag.SelectedSubverse = "404";
                return View("~/Views/Errors/Subversenotfound.cshtml");
            }
        }
    }
}