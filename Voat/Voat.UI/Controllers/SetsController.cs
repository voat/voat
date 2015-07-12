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
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

                // if subs in set count is < 1, don't display the page, instead, check if the user owns this set and give them a chance to add subs to the set
                if (set.Usersetdefinitions.Count < 1)
                {
                    // check if the user owns this sub
                    if (User.Identity.IsAuthenticated && User.Identity.Name == set.Created_by)
                    {
                        return RedirectToAction("EditSet", "Sets", new { setId = set.Set_id });
                    }
                }

                int subsInSet = set.Usersetdefinitions.Count();
                int submissionsToGet = 5;

                // there is at least 1 sub in the set
                if (subsInSet == 1)
                {
                    submissionsToGet = 25;
                }
                // get only one submission from each sub if set contains 25 or more subverses
                else if (subsInSet >= 25)
                {
                    submissionsToGet = 1;
                }
                // try to aim for 25 submissions
                else
                {
                    submissionsToGet = (int)Math.Ceiling((double)25 / subsInSet);
                }

                foreach (var subverse in set.Usersetdefinitions)
                {
                    // get top ranked submissions for current subverse
                    Subverse currentSubverse = subverse.Subverse;

                    if (currentSubverse != null)
                    {
                        // skip parameter could be passed here
                        submissions.AddRange(SetsUtility.TopRankedSubmissionsFromASub(currentSubverse.name, _db.Messages, set.Name, submissionsToGet, recordsToSkip * pageSize));
                    }
                    singleSetResultModel.Name = set.Name;
                    singleSetResultModel.Description = set.Description;
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

        // GET: /set/setId/page
        // fetch x more items from a set
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
                    Subverse currentSubverse = subverse.Subverse;

                    if (currentSubverse != null)
                    {
                        // skip parameter could be passed here
                        submissions.AddRange(SetsUtility.TopRankedSubmissionsFromASub(currentSubverse.name, _db.Messages, set.Name, pageSize, page * pageSize));
                    }
                    singleSetResultModel.Name = set.Name;
                    singleSetResultModel.Description = set.Description;
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
        [Authorize]
        public ActionResult EditSet(int setId)
        {
            var setToEdit = _db.Usersets.FirstOrDefault(s => s.Set_id == setId);

            if (setToEdit != null)
            {
                // check if user owns the set and abort
                if (!Utils.User.IsUserSetOwner(User.Identity.Name, setToEdit.Set_id)) return RedirectToAction("UnAuthorized", "Error");

                // get list of subverses for the set
                var setSubversesList = _db.Usersetdefinitions.Where(s => s.Set_id == setToEdit.Set_id).ToList();

                // populate viewmodel for the set
                var setViewModel = new SingleSetViewModel()
                {
                    Name = setToEdit.Name,
                    Description = setToEdit.Description,
                    SubversesList = setSubversesList,
                    Id = setToEdit.Set_id,
                    Created = setToEdit.Created_on,
                    Subscribers = setToEdit.Subscribers
                };

                return View("~/Views/Sets/EditSet.cshtml", setViewModel);
            }
            return RedirectToAction("NotFound", "Error");
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
            ViewBag.SelectedSubverse = "Popular sets";
            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            try
            {
                // order by subscriber count (popularity), show only sets which are fully defined by their creators
                var sets = _db.Usersets.Where(s => s.Usersetdefinitions.Any()).OrderByDescending(s => s.Subscribers);

                var paginatedSets = new PaginatedList<Userset>(sets, page ?? 0, pageSize);

                return View(paginatedSets);
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        // GET: /sets/recommended
        public ActionResult RecommendedSets(int? page)
        {
            ViewBag.SelectedSubverse = "recommended sets";
            ViewBag.sortingmode = "recommended";

            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            try
            {
                // order by subscriber count (popularity), show only sets which are fully defined
                var sets = _db.Usersets.Where(s => s.Usersetdefinitions.Any() && s.Default).OrderByDescending(s => s.Subscribers);

                var paginatedSets = new PaginatedList<Userset>(sets, page ?? 0, pageSize);

                return View("~/Views/Sets/Sets.cshtml", paginatedSets);
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        // GET: /sets/create
        [Authorize]
        public ActionResult CreateSet()
        {
            return View("~/Views/Sets/CreateSet.cshtml");
        }

        // POST: /sets/create
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateSet([Bind(Include = "Name, Description")] AddSet setTmpModel)
        {
            if (!User.Identity.IsAuthenticated) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            int maximumOwnedSets = MvcApplication.MaximumOwnedSets;

            // TODO
            // ###############################################################################################
            try
            {
                // abort if model is in invalid state
                if (!ModelState.IsValid) return View();

                // setup default values
                var set = new Userset
                {
                    Name = setTmpModel.Name,
                    Description = setTmpModel.Description,
                    Created_on = DateTime.Now,
                    Created_by = User.Identity.Name,
                    Default = false,
                    Public = true,
                    Subscribers = 0
                };

                // only allow users with less than maximum allowed sets to create a set
                var amountOfOwnedSets = _db.Usersets
                    .Where(s => s.Created_by == User.Identity.Name)
                    .ToList();

                if (amountOfOwnedSets.Count <= maximumOwnedSets)
                {
                    _db.Usersets.Add(set);
                    await _db.SaveChangesAsync();

                    // subscribe user to the newly created set
                    Utils.User.SubscribeToSet(User.Identity.Name, set.Set_id);

                    // go to newly created Set
                    return RedirectToAction("EditSet", "Sets", new { setId = set.Set_id });
                }

                ModelState.AddModelError(string.Empty, "Sorry, you can not own more than " + maximumOwnedSets + " sets.");
                return View();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Something bad happened.");
                return View();
            }
            // ###############################################################################################
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

        // GET: /mysets/manage
        [Authorize]
        public ActionResult ManageUserSets(int? page)
        {
            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // load user owned sets for logged in user
            IQueryable<Userset> userSets = _db.Usersets.Where(s => s.Created_by.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)).OrderBy(s => s.Name);

            var paginatedUserSets = new PaginatedList<Userset>(userSets, page ?? 0, pageSize);

            return View("~/Views/Sets/ManageMySets.cshtml", paginatedUserSets);
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
        [HttpPost]
        public JsonResult Subscribe(int setId)
        {
            var loggedInUser = User.Identity.Name;

            Utils.User.SubscribeToSet(loggedInUser, setId);
            return Json("Subscription request was successful.", JsonRequestBehavior.AllowGet);
        }

        // POST: unsubscribe from a set
        [Authorize]
        [HttpPost]
        public JsonResult UnSubscribe(int setId)
        {
            var loggedInUser = User.Identity.Name;

            Utils.User.UnSubscribeFromSet(loggedInUser, setId);
            return Json("Unsubscribe request was successful.", JsonRequestBehavior.AllowGet);
        }

        // POST: add a subverse to set
        [Authorize]
        [HttpPost]
        public JsonResult AddSubverseToSet(string subverseName, int setId)
        {
            // check if set exists
            var setToModify = _db.Usersets.Find(setId);
            if (setToModify == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Set doesn't exist.", JsonRequestBehavior.AllowGet);
            }

            // check if user is set owner
            if (!Utils.User.IsUserSetOwner(User.Identity.Name, setId))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Unauthorized request.", JsonRequestBehavior.AllowGet);
            }

            // check if subverse exists
            var subverseToAdd = _db.Subverses.FirstOrDefault(s => s.name.Equals(subverseName, StringComparison.OrdinalIgnoreCase));
            if (subverseToAdd == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("The subverse does not exist.", JsonRequestBehavior.AllowGet);
            }

            // check if subverse is already a part of this set
            if (setToModify.Usersetdefinitions.Any(sd => sd.Subversename.Equals(subverseName, StringComparison.OrdinalIgnoreCase)))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("The subverse is already a part of this set.", JsonRequestBehavior.AllowGet);
            }

            // add subverse to set
            Usersetdefinition newUsersetdefinition = new Usersetdefinition
            {
                Set_id = setId,
                Subversename = subverseToAdd.name
            };
            
            _db.Usersetdefinitions.Add(newUsersetdefinition);
            _db.SaveChangesAsync();

            return Json("Add subverse to set request sucessful.", JsonRequestBehavior.AllowGet);
        }

        // POST: remove a subverse from set
        [Authorize]
        [HttpPost]
        public JsonResult RemoveSubverseFromSet(string subverseName, int setId)
        {
            // check if user is set owner
            if (!Utils.User.IsUserSetOwner(User.Identity.Name, setId))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new List<string> { "Unauthorized request." });
            }

            // remove subverse from set
            var setDefinitionToRemove = _db.Usersetdefinitions.FirstOrDefault(s => s.Set_id == setId && s.Subversename.Equals(subverseName, StringComparison.OrdinalIgnoreCase));
            if (setDefinitionToRemove != null)
            {
                _db.Usersetdefinitions.Remove(setDefinitionToRemove);
                _db.SaveChangesAsync();
                return Json("Add subverse to set request sucessful.", JsonRequestBehavior.AllowGet);
            }

            // expected subverse was not found in user set definition
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(new List<string> { "Bad request." });
        }

        // POST: change set name and description
        [Authorize]
        [HttpPost]
        public JsonResult ChangeSetInfo(int setId, string newSetName)
        {
            // check if user is set owner
            if (!Utils.User.IsUserSetOwner(User.Identity.Name, setId))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new List<string> { "Unauthorized request." });
            }

            // find the set to modify
            var setToModify = _db.Usersets.Find(setId);

            if (setToModify != null)
            {
                try
                {
                    setToModify.Name = newSetName;
                    // TODO setToModify.Description = newSetDescription;

                    _db.SaveChangesAsync();
                    return Json("Set info change was sucessful.", JsonRequestBehavior.AllowGet);
                }
                catch (Exception)
                {
                    //
                }
            }

            // something went horribly wrong
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(new List<string> { "Bad request." });
        }

        // POST: delete a set
        [Authorize]
        [HttpPost]
        public JsonResult DeleteSet(int setId)
        {
            // check if user is set owner
            if (!Utils.User.IsUserSetOwner(User.Identity.Name, setId))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new List<string> { "Unauthorized request." });
            }

            // delete the set
            var setToRemove = _db.Usersets.FirstOrDefault(s => s.Set_id == setId && s.Created_by.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
            if (setToRemove != null)
            {
                _db.Usersets.Remove(setToRemove);
                _db.SaveChangesAsync();
                return Json("Set has been deleted.", JsonRequestBehavior.AllowGet);
            }

            // expected set was not found in user sets
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(new List<string> { "Bad request." });
        }
    }
}