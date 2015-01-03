/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2014 Voat
All Rights Reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Voat.Models;
using Voat.Models.ViewModels;
using Voat.Utils;

namespace Voat.Controllers
{
    public class SetsController : Controller
    {
        private readonly whoaverseEntities _db = new whoaverseEntities();

        // GET: /s/setname
        public ActionResult SingleDefaultSet(int? page, string defaultSetName)
        {
            ViewBag.SelectedSubverse = "single default set";
            var singleSetResultModel = new SingleDefaultSetViewModel();
            var submissions = new List<SetSubmission>();

            try
            {
                // show a single set
                // get list of subverses for the set
                // for each subverse, get top ranked submissions
                var set = _db.Defaultsets.FirstOrDefault(ds => ds.Name == defaultSetName);

                if (set != null)
                    foreach (var subverse in set.Defaultsetsetups)
                    {
                        // get top ranked submissions
                        Subverse currentSubverse = subverse.Subvers;
                        Defaultset currentSet = set;

                        if (currentSubverse != null)
                        {
                            submissions.AddRange(SetsUtility.TopRankedSubmissionsFromASub(currentSubverse.name, _db.Messages, currentSet.Name, 5));
                        }
                    }

                singleSetResultModel.Name = defaultSetName;
                singleSetResultModel.SubmissionsList = new List<SetSubmission>(submissions.OrderByDescending(s => s.Rank));

                return View("~/Views/Sets/Index.cshtml", singleSetResultModel);
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        // POST: /s/reorder/setname
        [Authorize]
        [HttpPost]
        public ActionResult ReorderSet(string setName, int direction)
        {
            // check if user is subscribed to given set
            if (Utils.User.IsUserSubscribedToSet(User.Identity.Name, setName))
            {
                // reorder the set for logged in user using given direction
                // TODO: reorder
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            // the user is not subscribed to given set
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // GET: /mysets
        [Authorize]
        public ActionResult UserSets(int? page)
        {
            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // load user sets for logged in user
            IQueryable<Usersetsubscription> userSets = _db.Usersetsubscriptions.Where(s => s.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)).OrderBy(s=>s.Userset.Name);

            var paginatedUserSetSubscriptions = new PaginatedList<Usersetsubscription>(userSets, page ?? 0, pageSize);

            return View("~/Views/Sets/MySets.cshtml", paginatedUserSetSubscriptions);
        }

        [ChildActionOnly]
        public PartialViewResult PopularSets()
        {
            var popularSets = _db.Usersets.Where(s=>s.Public).OrderByDescending(s => s.Subscribers).Take(40);

            return PartialView("~/Views/Sets/_PopularSets.cshtml", popularSets);
        }
    }
}