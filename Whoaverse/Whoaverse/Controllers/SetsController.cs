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
using Microsoft.Ajax.Utilities;
using Voat.Models;
using Voat.Models.ViewModels;
using Voat.Utils;

namespace Voat.Controllers
{
    public class SetsController : Controller
    {
        private readonly whoaverseEntities _db = new whoaverseEntities();

        // GET: frontpage for default sets
        public ActionResult DefaultSetsIndex()
        {
            ViewBag.SelectedSubverse = "frontpage";
            var frontPageResultModel = new SetFrontpageViewModel();
            var submissions = new List<SetSubmission>();

            try
            {
                // show default sets
                // get names of default sets
                // for each set name, get list of subverses
                // for each subverse, get top ranked submissions
                var defaultSets = _db.Usersets.ToList();

                foreach (var set in defaultSets)
                {
                    Userset setId = set;
                    var defaultSetDefinition = _db.Usersetdefinitions.Where(st => st.Set_id == setId.Set_id);

                    foreach (var subverse in defaultSetDefinition)
                    {
                        // get top ranked submissions
                        submissions.AddRange(SetsUtility.TopRankedSubmissionsFromASub(subverse.Subversename, _db.Messages, set.Name, 2, 0));
                    }
                }

                frontPageResultModel.HasSetSubscriptions = false;
                frontPageResultModel.SubmissionsList = new List<SetSubmission>(submissions.OrderByDescending(s => s.Rank));

                return View("~/Views/Home/IndexV2.cshtml", frontPageResultModel);
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        // GET: /set/setid
        // show single set frontpage
        public ActionResult SingleSet(int setId, int? page)
        {
            const int pageSize = 25;
            int recordsToSkip = (page ?? 0);
            try
            {
                // get list of subverses for the set
                // for each subverse, get top ranked submissions
                var set = _db.Usersets.FirstOrDefault(ds => ds.Set_id == setId);

                if (set == null) return RedirectToAction("NotFound", "Error");

                ViewBag.SelectedSubverse = set.Name;
                var singleSetResultModel = new SingleSetViewModel();
                var submissions = new List<SetSubmission>();

                foreach (var subverse in set.Usersetdefinitions)
                {
                    // get 5 top ranked submissions for current subverse
                    Subverse currentSubverse = subverse.Subvers;

                    if (currentSubverse != null)
                    {
                        // skip parameter could be passed here
                        submissions.AddRange(SetsUtility.TopRankedSubmissionsFromASub(currentSubverse.name, _db.Messages, set.Name, 5, recordsToSkip * pageSize));
                    }
                    singleSetResultModel.Name = set.Name;
                    singleSetResultModel.Id = set.Set_id;
                }

                singleSetResultModel.SubmissionsList = new List<SetSubmission>(submissions.OrderByDescending(s => s.Rank));

                return View("~/Views/Sets/Index.cshtml", singleSetResultModel);
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        // GET: /set/setid
        // show single set frontpage page
        public ActionResult SingleSetPage(int setId, int page)
        {
            const int pageSize = 2;
            try
            {
                // get list of subverses for the set
                // for each subverse, get top ranked submissions
                var set = _db.Usersets.FirstOrDefault(ds => ds.Set_id == setId);

                if (set == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);

                ViewBag.SelectedSubverse = set.Name;
                var singleSetResultModel = new SingleSetViewModel();
                var submissions = new List<SetSubmission>();

                foreach (var subverse in set.Usersetdefinitions)
                {
                    // get 5 top ranked submissions for current subverse
                    Subverse currentSubverse = subverse.Subvers;

                    if (currentSubverse != null)
                    {
                        // skip parameter could be passed here
                        submissions.AddRange(SetsUtility.TopRankedSubmissionsFromASub(currentSubverse.name, _db.Messages, set.Name, pageSize, page * pageSize));
                    }
                    singleSetResultModel.Name = set.Name;
                    singleSetResultModel.Id = set.Set_id;
                }

                singleSetResultModel.SubmissionsList = new List<SetSubmission>(submissions.OrderByDescending(s => s.Rank).ThenByDescending(s => s.Date));

                if (submissions.Any())
                {
                    ViewBag.Page = page;
                    return PartialView("~/Views/Sets/_SingleSetPage.cshtml", singleSetResultModel);
                }

                // no more entries found
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        // GET: /set/setid/edit
        public ActionResult EditSet(int setId)
        {
            var setToEdit = _db.Usersets.FirstOrDefault(s => s.Set_id == setId);

            if (setToEdit != null)
            {
                // get list of subverses for the set
                var setSubversesList = _db.Usersetdefinitions.Where(s => s.Set_id == setToEdit.Set_id).ToList();

                // populate viewmodel for the set
                var setViewModel = new SingleSetViewModel()
                {
                    Name = setToEdit.Name,
                    SubversesList = setSubversesList,
                    Id = setToEdit.Set_id,
                    Created = setToEdit.Created_on,
                    Subscribers = setToEdit.Subscribers
                };

                return View("~/Views/Sets/SingleSetView.cshtml", setViewModel);
            }
            return RedirectToAction("SetNotFound", "Error");
        }

        // GET: /set/defaultsetid
        public ActionResult SingleDefaultSet(int setId)
        {
            try
            {
                ViewBag.SelectedSubverse = "single default set index";
                var singleSetResultModel = new SingleSetViewModel();
                var submissions = new List<SetSubmission>();

                try
                {
                    // show a single default set
                    // get list of subverses for the set
                    // for each subverse, get top ranked submissions
                    var set = _db.Defaultsets.FirstOrDefault(ds => ds.Set_id == setId);

                    if (set != null)
                        foreach (var subverse in set.Defaultsetsetups)
                        {
                            // get top ranked submissions
                            Subverse currentSubverse = subverse.Subvers;

                            if (currentSubverse != null)
                            {
                                submissions.AddRange(SetsUtility.TopRankedSubmissionsFromASub(currentSubverse.name, _db.Messages, set.Name, 5, 0));
                            }
                            singleSetResultModel.Name = set.Name;
                            singleSetResultModel.Id = set.Set_id;
                        }


                    singleSetResultModel.SubmissionsList = new List<SetSubmission>(submissions.OrderByDescending(s => s.Rank));

                    ViewBag.DefaultSet = true;
                    return View("~/Views/Sets/Index.cshtml", singleSetResultModel);
                }
                catch (Exception)
                {
                    return RedirectToAction("HeavyLoad", "Error");
                }

            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        // GET: /set/defaultsetid/page
        public ActionResult SingleDefaultSetPage(int setId, int page)
        {
            const int pageSize = 2;
            try
            {
                ViewBag.SelectedSubverse = "single default set index";
                var singleSetResultModel = new SingleSetViewModel();
                var submissions = new List<SetSubmission>();

                // show a single default set page
                // get list of subverses for the set
                // for each subverse, get top ranked submissions
                var set = _db.Defaultsets.FirstOrDefault(ds => ds.Set_id == setId);

                if (set != null)
                    foreach (var subverse in set.Defaultsetsetups)
                    {
                        // get top ranked submissions
                        Subverse currentSubverse = subverse.Subvers;

                        if (currentSubverse != null)
                        {
                            submissions.AddRange(SetsUtility.TopRankedSubmissionsFromASub(currentSubverse.name, _db.Messages, set.Name, pageSize, page * pageSize));
                        }
                        singleSetResultModel.Name = set.Name;
                        singleSetResultModel.Id = set.Set_id;
                    }


                singleSetResultModel.SubmissionsList = new List<SetSubmission>(submissions.OrderByDescending(s => s.Rank).ThenByDescending(s => s.Date));

                if (submissions.Any())
                {
                    ViewBag.Page = page;
                    return PartialView("~/Views/Sets/_SingleSetPage.cshtml", singleSetResultModel);
                }

                // no more entries found
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
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

        // GET: /sets
        public ActionResult Sets(int? page)
        {
            ViewBag.SelectedSubverse = "sets";
            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            try
            {
                // order by subscriber count (popularity)
                var sets = _db.Usersets.Where(s => s.Usersetdefinitions.Any()).OrderByDescending(s => s.Subscribers);

                var paginatedSets = new PaginatedList<Userset>(sets, page ?? 0, pageSize);

                return View(paginatedSets);
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
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
            IQueryable<Usersetsubscription> userSets = _db.Usersetsubscriptions.Where(s => s.Username.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)).OrderBy(s => s.Userset.Name);

            var paginatedUserSetSubscriptions = new PaginatedList<Usersetsubscription>(userSets, page ?? 0, pageSize);

            return View("~/Views/Sets/MySets.cshtml", paginatedUserSetSubscriptions);
        }

        // GET: 40 most popular sets by subscribers
        [ChildActionOnly]
        public PartialViewResult PopularSets()
        {
            var popularSets = _db.Usersets.Where(s => s.Public).OrderByDescending(s => s.Subscribers).Take(40);

            return PartialView("~/Views/Sets/_PopularSets.cshtml", popularSets);
        }

        // POST: subscribe to a set
        [Authorize]
        public JsonResult Subscribe(int setId)
        {
            var loggedInUser = User.Identity.Name;

            Utils.User.SubscribeToSet(loggedInUser, setId);
            return Json("Subscription request was successful.", JsonRequestBehavior.AllowGet);
        }

        // POST: unsubscribe from a set
        [Authorize]
        public JsonResult UnSubscribe(int setId)
        {
            var loggedInUser = User.Identity.Name;

            Utils.User.UnSubscribeFromSet(loggedInUser, setId);
            return Json("Unsubscribe request was successful.", JsonRequestBehavior.AllowGet);
        }
    }
}