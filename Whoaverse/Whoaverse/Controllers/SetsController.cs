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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Whoaverse.Models;
using Whoaverse.Models.ViewModels;
using Whoaverse.Utils;

namespace Whoaverse.Controllers
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
                var set = _db.Defaultsets.FirstOrDefault(ds => ds.Name.Equals(defaultSetName, StringComparison.OrdinalIgnoreCase));

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
    }
}