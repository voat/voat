/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat, Inc.
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

using Voat.Utilities;
using Voat.Data.Models;
using Voat.Configuration;
using Voat.UI.Utilities;

namespace Voat.Controllers
{
    public class SetsController : Controller
    {
        private readonly voatEntities _db = new voatEntities();

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
                var set = _db.UserSets.FirstOrDefault(ds => ds.ID == setId);

                if (set == null) return RedirectToAction("NotFound", "Error");

                ViewBag.SelectedSubverse = set.Name;
                var singleSetResultModel = new SingleSetViewModel();
                var submissions = new List<SetSubmission>();

                // if subs in set count is < 1, don't display the page, instead, check if the user owns this set and give them a chance to add subs to the set
                if (set.UserSetLists.Count < 1)
                {
                    // check if the user owns this sub
                    if (User.Identity.IsAuthenticated && User.Identity.Name == set.CreatedBy)
                    {
                        return RedirectToAction("EditSet", "Sets", new { setId = set.ID });
                    }
                }

                int subsInSet = set.UserSetLists.Count();
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

                foreach (var subverse in set.UserSetLists)
                {
                    // get top ranked submissions for current subverse
                    Subverse currentSubverse = subverse.Subverse1;

                    if (currentSubverse != null)
                    {
                        // skip parameter could be passed here
                        submissions.AddRange(SetsUtility.TopRankedSubmissionsFromASub(currentSubverse.Name, _db.Submissions, set.Name, submissionsToGet, recordsToSkip * pageSize));
                    }
                    singleSetResultModel.Name = set.Name;
                    singleSetResultModel.Description = set.Description;
                    singleSetResultModel.Id = set.ID;
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
                var set = _db.UserSets.FirstOrDefault(ds => ds.ID == setId);

                if (set == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);

                ViewBag.SelectedSubverse = set.Name;
                var singleSetResultModel = new SingleSetViewModel();
                var submissions = new List<SetSubmission>();

                foreach (var subverse in set.UserSetLists)
                {
                    // get 5 top ranked submissions for current subverse
                    Subverse currentSubverse = subverse.Subverse1;

                    if (currentSubverse != null)
                    {
                        // skip parameter could be passed here
                        submissions.AddRange(SetsUtility.TopRankedSubmissionsFromASub(currentSubverse.Name, _db.Submissions, set.Name, pageSize, page * pageSize));
                    }
                    singleSetResultModel.Name = set.Name;
                    singleSetResultModel.Description = set.Description;
                    singleSetResultModel.Id = set.ID;
                }

                singleSetResultModel.SubmissionsList = new List<SetSubmission>(submissions.OrderByDescending(s => s.Rank).ThenByDescending(s => s.CreationDate));

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
            var setToEdit = _db.UserSets.FirstOrDefault(s => s.ID == setId);

            if (setToEdit != null)
            {
                // check if user owns the set and abort
                if (!UserHelper.IsUserSetOwner(User.Identity.Name, setToEdit.ID)) return RedirectToAction("UnAuthorized", "Error");

                // get list of subverses for the set
                var setSubversesList = _db.UserSetLists.Where(s => s.ID == setToEdit.ID).ToList();

                // populate viewmodel for the set
                var setViewModel = new SingleSetViewModel()
                {
                    Name = setToEdit.Name,
                    Description = setToEdit.Description,
                    SubversesList = setSubversesList,
                    Id = setToEdit.ID,
                    Created = setToEdit.CreationDate,
                    Subscribers = setToEdit.SubscriberCount
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
            if (UserHelper.IsUserSubscribedToSet(User.Identity.Name, setName))
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
                var sets = _db.UserSets.Where(s => s.UserSetLists.Any()).OrderByDescending(s => s.SubscriberCount);

                var paginatedSets = new PaginatedList<UserSet>(sets, page ?? 0, pageSize);

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
                var sets = _db.UserSets.Where(s => s.UserSetLists.Any() && s.IsDefault).OrderByDescending(s => s.SubscriberCount);

                var paginatedSets = new PaginatedList<UserSet>(sets, page ?? 0, pageSize);

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
            int maximumOwnedSets = Settings.MaximumOwnedSets;

            // TODO
            // ###############################################################################################
            try
            {
                // abort if model is in invalid state
                if (!ModelState.IsValid) return View();

                // setup default values
                var set = new UserSet
                {
                    Name = setTmpModel.Name,
                    Description = setTmpModel.Description,
                    CreationDate = DateTime.Now,
                    CreatedBy = User.Identity.Name,
                    IsDefault = false,
                    IsPublic = true,
                    SubscriberCount = 0
                };

                // only allow users with less than maximum allowed sets to create a set
                var amountOfOwnedSets = _db.UserSets
                    .Where(s => s.CreatedBy == User.Identity.Name)
                    .ToList();

                if (amountOfOwnedSets.Count <= maximumOwnedSets)
                {
                    _db.UserSets.Add(set);
                    await _db.SaveChangesAsync();

                    // subscribe user to the newly created set
                    UserHelper.SubscribeToSet(User.Identity.Name, set.ID);

                    // go to newly created Set
                    return RedirectToAction("EditSet", "Sets", new { setId = set.ID });
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
            IQueryable<UserSetSubscription> userSets = _db.UserSetSubscriptions.Where(s => s.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)).OrderBy(s => s.UserSet.Name);

            var paginatedUserSetSubscriptions = new PaginatedList<UserSetSubscription>(userSets, page ?? 0, pageSize);

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
            IQueryable<UserSet> userSets = _db.UserSets.Where(s => s.CreatedBy.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)).OrderBy(s => s.Name);

            var paginatedUserSets = new PaginatedList<UserSet>(userSets, page ?? 0, pageSize);

            return View("~/Views/Sets/ManageMySets.cshtml", paginatedUserSets);
        }

        // GET: 40 most popular sets by subscribers
        [ChildActionOnly]
        public PartialViewResult PopularSets()
        {
            var popularSets = _db.UserSets.Where(s => s.IsPublic).OrderByDescending(s => s.SubscriberCount).Take(40);

            return PartialView("~/Views/Sets/_PopularSets.cshtml", popularSets);
        }

        // POST: subscribe to a set
        [Authorize]
        [HttpPost]
        public JsonResult Subscribe(int setId)
        {
            var loggedInUser = User.Identity.Name;

            UserHelper.SubscribeToSet(loggedInUser, setId);
            return Json("Subscription request was successful.", JsonRequestBehavior.AllowGet);
        }

        // POST: unsubscribe from a set
        [Authorize]
        [HttpPost]
        public JsonResult UnSubscribe(int setId)
        {
            var loggedInUser = User.Identity.Name;

            UserHelper.UnSubscribeFromSet(loggedInUser, setId);
            return Json("Unsubscribe request was successful.", JsonRequestBehavior.AllowGet);
        }

        // POST: add a subverse to set
        [Authorize]
        [HttpPost]
        public JsonResult AddSubverseToSet(string subverseName, int setId)
        {
            // check if set exists
            var setToModify = _db.UserSets.Find(setId);
            if (setToModify == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Set doesn't exist.", JsonRequestBehavior.AllowGet);
            }

            // check if user is set owner
            if (!UserHelper.IsUserSetOwner(User.Identity.Name, setId))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Unauthorized request.", JsonRequestBehavior.AllowGet);
            }

            // check if subverse exists
            var subverseToAdd = _db.Subverses.FirstOrDefault(s => s.Name.Equals(subverseName, StringComparison.OrdinalIgnoreCase));
            if (subverseToAdd == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("The subverse does not exist.", JsonRequestBehavior.AllowGet);
            }

            // check if subverse is already a part of this set
            if (setToModify.UserSetLists.Any(sd => sd.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase)))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("The subverse is already a part of this set.", JsonRequestBehavior.AllowGet);
            }

            // add subverse to set
            UserSetList newUsersetdefinition = new UserSetList
            {
                ID = setId,
                Subverse = subverseToAdd.Name
            };
            
            _db.UserSetLists.Add(newUsersetdefinition);
            _db.SaveChangesAsync();

            return Json("Add subverse to set request sucessful.", JsonRequestBehavior.AllowGet);
        }

        // POST: remove a subverse from set
        [Authorize]
        [HttpPost]
        public JsonResult RemoveSubverseFromSet(string subverseName, int setId)
        {
            // check if user is set owner
            if (!UserHelper.IsUserSetOwner(User.Identity.Name, setId))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new List<string> { "Unauthorized request." });
            }

            // remove subverse from set
            var setDefinitionToRemove = _db.UserSetLists.FirstOrDefault(s => s.ID == setId && s.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase));
            if (setDefinitionToRemove != null)
            {
                _db.UserSetLists.Remove(setDefinitionToRemove);
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
            if (!UserHelper.IsUserSetOwner(User.Identity.Name, setId))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new List<string> { "Unauthorized request." });
            }

            // find the set to modify
            var setToModify = _db.UserSets.Find(setId);

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
            if (!UserHelper.IsUserSetOwner(User.Identity.Name, setId))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new List<string> { "Unauthorized request." });
            }

            // delete the set
            var setToRemove = _db.UserSets.FirstOrDefault(s => s.ID == setId && s.CreatedBy.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
            if (setToRemove != null)
            {
                _db.UserSets.Remove(setToRemove);
                _db.SaveChangesAsync();
                return Json("Set has been deleted.", JsonRequestBehavior.AllowGet);
            }

            // expected set was not found in user sets
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(new List<string> { "Bad request." });
        }
    }
}