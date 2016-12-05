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
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Voat.Models;
using Voat.Models.ViewModels;

using System.Collections.Generic;
using Voat.Data.Models;
using Voat.Utilities;
using Voat.UI.Utilities;
using Voat.Configuration;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Query;
using Voat.Domain.Command;

namespace Voat.Controllers
{
    public class SubversesController : BaseController
    {
        //IAmAGate: Move queries to read-only mirror
        private readonly voatEntities _db = new voatEntities(true);

        private int subverseCacheTimeInSeconds = 240;

        // GET: sidebar for selected subverse
        public ActionResult SidebarForSelectedSubverseComments(Submission submission)
        {
            //Can't cache as view is using model to query
            //var subverse = _db.Subverses.Find(submission.Subverse);
            var subverse = DataCache.Subverse.Retrieve(submission.Subverse);

            //don't return a sidebar since subverse doesn't exist or is a system subverse
            if (subverse == null)
            {
                return new EmptyResult();
            }

            // get subscriber count for selected subverse
            //var subscriberCount = _db.SubverseSubscriptions.Count(r => r.Subverse.Equals(submission.Subverse, StringComparison.OrdinalIgnoreCase));

            //ViewBag.SubscriberCount = subscriberCount;
            ViewBag.SelectedSubverse = submission.Subverse;

            //if (!showingComments) return new EmptyResult();

            if (submission.IsAnonymized || subverse.IsAnonymized)
            {
                ViewBag.name = submission.ID.ToString();
                ViewBag.anonymized = true;
            }
            else
            {
                if (submission.IsDeleted)
                {
                    ViewBag.name = "deleted";
                }
                else
                {
                    ViewBag.name = submission.UserName;
                }
            }

            ViewBag.date = submission.CreationDate;
            ViewBag.lastEditDate = submission.LastEditDate;
            ViewBag.likes = submission.UpCount;
            ViewBag.dislikes = submission.DownCount;
            ViewBag.anonymized_mode = subverse.IsAnonymized;
            ViewBag.views = submission.Views;

            try
            {
                ViewBag.OnlineUsers = SessionHelper.ActiveSessionsForSubverse(submission.Subverse);
            }
            catch (Exception)
            {
                ViewBag.OnlineUsers = -1;
            }

            return PartialView("~/Views/Shared/Sidebars/_SidebarComments.cshtml", subverse);
        }

        // GET: sidebar for selected subverse
        public ActionResult SidebarForSelectedSubverse(string selectedSubverse)
        {
            //Can't cache as view is using Model to query
            var subverse = _db.Subverses.Find(selectedSubverse);

            // don't return a sidebar since subverse doesn't exist or is a system subverse
            if (subverse == null)
            {
                return new EmptyResult();
            }
            
            ViewBag.SelectedSubverse = selectedSubverse;

            try
            {
                ViewBag.OnlineUsers = SessionHelper.ActiveSessionsForSubverse(selectedSubverse);
            }
            catch (Exception)
            {
                ViewBag.OnlineUsers = -1;
            }

            return PartialView("~/Views/Shared/Sidebars/_Sidebar.cshtml", subverse);
        }

        // POST: Create a new Subverse
        [HttpPost]
        [Authorize]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(DelayRequest = 300, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public async Task<ActionResult> CreateSubverse([Bind(Include = "Name, Title, Description, Type, Sidebar, CreationDate, Owner")] AddSubverse subverseTmpModel)
        {
            // abort if model state is invalid
            if (!ModelState.IsValid)
            {
                return View(subverseTmpModel);
            }

            var title = $"/v/{subverseTmpModel.Name}"; //backwards compatibility, previous code always uses this
            var cmd = new CreateSubverseCommand(subverseTmpModel.Name, title, subverseTmpModel.Description, subverseTmpModel.Sidebar);
            var respones = await cmd.Execute();
            if (respones.Success)
            {
                return RedirectToAction("SubverseIndex", "Subverses", new { subverse = subverseTmpModel.Name });
            }
            else
            {
                ModelState.AddModelError(string.Empty, respones.Message);
                return View(subverseTmpModel);
            }
            
        }

        // GET: create
        [Authorize]
        public ActionResult CreateSubverse()
        {
            return View();
        }

        // GET: settings
        [Authorize]
        public ActionResult SubverseSettings(string subversetoshow)
        {
            var subverse = DataCache.Subverse.Retrieve(subversetoshow);

            if (subverse == null)
            {
                ViewBag.SelectedSubverse = "404";
                return SubverseNotFoundErrorView();
            }

            // check that the user requesting to edit subverse settings is subverse owner!
            var subAdmin =
                _db.SubverseModerators.FirstOrDefault(
                    x => x.Subverse == subversetoshow && x.UserName == User.Identity.Name && x.Power <= 2);

            if (subAdmin == null)
                return RedirectToAction("Index", "Home");

            // map existing data to view model for editing and pass it to frontend
            // NOTE: we should look into a mapper which automatically maps these properties to corresponding fields to avoid tedious manual mapping
            var viewModel = new SubverseSettingsViewModel
            {
                Name = subverse.Name,
                Type = subverse.Type,
                SubmissionText = subverse.SubmissionText,
                Description = subverse.Description,
                SideBar = subverse.SideBar,
                Stylesheet = subverse.Stylesheet,
                IsDefaultAllowed = subverse.IsDefaultAllowed,
                SubmitLinkLabel = subverse.SubmitLinkLabel,
                SubmitPostLabel = subverse.SubmitPostLabel,
                IsAdult = subverse.IsAdult,
                IsPrivate = subverse.IsPrivate,
                IsThumbnailEnabled = subverse.IsThumbnailEnabled,
                ExcludeSitewideBans = subverse.ExcludeSitewideBans,
                IsAuthorizedOnly = subverse.IsAuthorizedOnly,
                IsAnonymized = subverse.IsAnonymized,
                MinCCPForDownvote = subverse.MinCCPForDownvote
            };

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SubverseName = subverse.Name;
            return View("~/Views/Subverses/Admin/SubverseSettings.cshtml", viewModel);
        }

        // POST: Eddit a Subverse
        [HttpPost]
        [PreventSpam(DelayRequest = 30, ErrorMessage = "Sorry, you are doing that too fast. Please try again in 30 seconds.")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> SubverseSettings(SubverseSettingsViewModel updatedModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("~/Views/Subverses/Admin/SubverseSettings.cshtml");
                }
                var existingSubverse = _db.Subverses.Find(updatedModel.Name);

                // check if subverse exists before attempting to edit it
                if (existingSubverse != null)
                {
                    // check if user requesting edit is authorized to do so for current subverse
                    if (!ModeratorPermission.HasPermission(User.Identity.Name, updatedModel.Name, Domain.Models.ModeratorAction.ModifySettings))
                    {
                        return new EmptyResult();
                    }
                    //check description for banned domains
                    if (BanningUtility.ContentContainsBannedDomain(existingSubverse.Name, updatedModel.Description))
                    {
                        ModelState.AddModelError(string.Empty, "Sorry, description text contains banned domains.");
                        return View("~/Views/Subverses/Admin/SubverseSettings.cshtml", updatedModel);
                    }
                    //check sidebar for banned domains
                    if (BanningUtility.ContentContainsBannedDomain(existingSubverse.Name, updatedModel.SideBar))
                    {
                        ModelState.AddModelError(string.Empty, "Sorry, sidebar text contains banned domains.");
                        return View("~/Views/Subverses/Admin/SubverseSettings.cshtml", updatedModel);
                    }

                    // TODO investigate if EntityState is applicable here and use that instead
                    // db.Entry(updatedModel).State = EntityState.Modified;

                    existingSubverse.Description = updatedModel.Description;
                    existingSubverse.SideBar = updatedModel.SideBar;

                    if (updatedModel.Stylesheet != null)
                    {
                        if (updatedModel.Stylesheet.Length < 50001)
                        {
                            existingSubverse.Stylesheet = updatedModel.Stylesheet;
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Sorry, custom CSS limit is set to 50000 characters.");
                            return View("~/Views/Subverses/Admin/SubverseSettings.cshtml");
                        }
                    }
                    else
                    {
                        existingSubverse.Stylesheet = updatedModel.Stylesheet;
                    }

                    existingSubverse.IsAdult = updatedModel.IsAdult;
                    existingSubverse.IsPrivate = updatedModel.IsPrivate;
                    existingSubverse.IsThumbnailEnabled = updatedModel.IsThumbnailEnabled;
                    existingSubverse.IsAuthorizedOnly = updatedModel.IsAuthorizedOnly;
                    existingSubverse.ExcludeSitewideBans = updatedModel.ExcludeSitewideBans;
                    existingSubverse.MinCCPForDownvote = updatedModel.MinCCPForDownvote;

                    // these properties are currently not implemented but they can be saved and edited for future use
                    existingSubverse.Type = updatedModel.Type;
                    existingSubverse.SubmitLinkLabel = updatedModel.SubmitLinkLabel;
                    existingSubverse.SubmitPostLabel = updatedModel.SubmitPostLabel;
                    existingSubverse.SubmissionText = updatedModel.SubmissionText;
                    existingSubverse.IsDefaultAllowed = updatedModel.IsDefaultAllowed;

                    if (existingSubverse.IsAnonymized && updatedModel.IsAnonymized == false)
                    {
                        ModelState.AddModelError(string.Empty, "Sorry, this subverse is permanently locked to anonymized mode.");
                        return View("~/Views/Subverses/Admin/SubverseSettings.cshtml");
                    }

                    // only subverse owners should be able to convert a sub to anonymized mode
                    if (ModeratorPermission.IsLevel(User.Identity.Name, updatedModel.Name, Domain.Models.ModeratorLevel.Owner))
                    {
                        existingSubverse.IsAnonymized = updatedModel.IsAnonymized;
                    }

                    await _db.SaveChangesAsync();

                    //purge new minified CSS
                    CacheHandler.Instance.Remove(CachingKey.SubverseStylesheet(existingSubverse.Name));

                    //purge subvere
                    CacheHandler.Instance.Remove(CachingKey.Subverse(existingSubverse.Name));

                    // go back to this subverse
                    return RedirectToAction("SubverseIndex", "Subverses", new { subverse = updatedModel.Name });

                    // user was not authorized to commit the changes, drop attempt
                }
                ModelState.AddModelError(string.Empty, "Sorry, The subverse you are trying to edit does not exist.");
                return View("~/Views/Subverses/Admin/SubverseSettings.cshtml");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Something bad happened.");
                return View("~/Views/Subverses/Admin/SubverseSettings.cshtml");
            }
        }

        // GET: subverse stylesheet editor
        [Authorize]
        public ActionResult SubverseStylesheetEditor(string subversetoshow)
        {
            var subverse = DataCache.Subverse.Retrieve(subversetoshow);

            if (subverse == null)
            {
                ViewBag.SelectedSubverse = "404";
                return SubverseNotFoundErrorView();
            }
            if (!ModeratorPermission.HasPermission(User.Identity.Name, subversetoshow, Domain.Models.ModeratorAction.ModifyCSS))
            {
                return RedirectToAction("Index", "Home");
            }

            // map existing data to view model for editing and pass it to frontend
            var viewModel = new SubverseStylesheetViewModel
            {
                Name = subverse.Name,
                Stylesheet = subverse.Stylesheet
            };

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SubverseName = subverse.Name;
            return View("~/Views/Subverses/Admin/SubverseStylesheetEditor.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        [PreventSpam(DelayRequest = 30, ErrorMessage = "Sorry, you are doing that too fast. Please try again in 30 seconds.")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> SubverseStylesheetEditor(Subverse updatedModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("~/Views/Subverses/Admin/SubverseSettings.cshtml");
                }
                var existingSubverse = _db.Subverses.Find(updatedModel.Name);

                // check if subverse exists before attempting to edit it
                if (existingSubverse != null)
                {
                    // check if user requesting edit is authorized to do so for current subverse
                    // check that the user requesting to edit subverse settings is subverse owner!
                    if (!ModeratorPermission.HasPermission(User.Identity.Name, existingSubverse.Name, Domain.Models.ModeratorAction.ModifyCSS))
                    {
                        return new EmptyResult();
                    }

                    if (!String.IsNullOrEmpty(updatedModel.Stylesheet))
                    {
                        if (updatedModel.Stylesheet.Length < 50001)
                        {
                            existingSubverse.Stylesheet = updatedModel.Stylesheet;
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Sorry, custom CSS limit is set to 50000 characters.");
                            return View("~/Views/Subverses/Admin/SubverseStylesheetEditor.cshtml");
                        }
                    }
                    else
                    {
                        existingSubverse.Stylesheet = updatedModel.Stylesheet;
                    }

                    await _db.SaveChangesAsync();

                    //purge new minified CSS
                    CacheHandler.Instance.Remove(CachingKey.SubverseStylesheet(existingSubverse.Name));
                    CacheHandler.Instance.Remove(CachingKey.Subverse(existingSubverse.Name));

                    // go back to this subverse
                    return RedirectToAction("SubverseIndex", "Subverses", new { subverse = updatedModel.Name });
                }

                ModelState.AddModelError(string.Empty, "Sorry, The subverse you are trying to edit does not exist.");
                return View("~/Views/Subverses/Admin/SubverseStylesheetEditor.cshtml");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Something bad happened.");
                return View("~/Views/Subverses/Admin/SubverseStylesheetEditor.cshtml");
            }
        }

        // GET: show a list of subverses by number of subscribers
        public ActionResult Subverses(int? page)
        {
            ViewBag.SelectedSubverse = "subverses";
            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return NotFoundErrorView();
            }

            try
            {
                // order by subscriber count (popularity)
                var subverses = _db.Subverses.OrderByDescending(s => s.SubscriberCount);

                var paginatedSubverses = new PaginatedList<Subverse>(subverses, page ?? 0, pageSize);

                return View(paginatedSubverses);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // GET: show subverse search view
        public ActionResult Search()
        {
            ViewBag.SelectedSubverse = "subverses";
            ViewBag.SubversesView = "search";

            return View("~/Views/Subverses/SearchForSubverse.cshtml", new SearchSubverseViewModel());
        }

        [Authorize]
        public ViewResult SubversesSubscribed(int? page)
        {
            ViewBag.SelectedSubverse = "subverses";
            ViewBag.SubversesView = "subscribed";
            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return NotFoundErrorView();
            }

            // get a list of subcribed subverses with details and order by subverse names, ascending
            IQueryable<SubverseDetailsViewModel> subscribedSubverses = from c in _db.Subverses
                                                                       join a in _db.SubverseSubscriptions
                                                                       on c.Name equals a.Subverse
                                                                       where a.UserName.Equals(User.Identity.Name)
                                                                       orderby a.Subverse ascending
                                                                       select new SubverseDetailsViewModel
                                                                       {
                                                                           Name = c.Name,
                                                                           Title = c.Title,
                                                                           Description = c.Description,
                                                                           Creation_date = c.CreationDate,
                                                                           Subscribers = c.SubscriberCount
                                                                       };

            var paginatedSubscribedSubverses = new PaginatedList<SubverseDetailsViewModel>(subscribedSubverses, page ?? 0, pageSize);

            return View("SubscribedSubverses", paginatedSubscribedSubverses);
        }

        // GET: sidebar for selected subverse
        public ActionResult DetailsForSelectedSubverse(string selectedSubverse)
        {
            var subverse = DataCache.Subverse.Retrieve(selectedSubverse);

            if (subverse == null)
                return new EmptyResult();

            // get subscriber count for selected subverse
            //var subscriberCount = _db.SubverseSubscriptions.Count(r => r.Subverse.Equals(selectedSubverse, StringComparison.OrdinalIgnoreCase));

            //ViewBag.SubscriberCount = subscriberCount;
            ViewBag.SelectedSubverse = selectedSubverse;
            return PartialView("_SubverseDetails", subverse);

            //don't return a sidebar since subverse doesn't exist or is a system subverse
        }

        // GET: show a list of subverses by creation date
        public ViewResult NewestSubverses(int? page, string sortingmode)
        {
            ViewBag.SelectedSubverse = "subverses";
            ViewBag.SortingMode = sortingmode;

            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return NotFoundErrorView();
            }

            var subverses = _db.Subverses.Where(s => s.Description != null).OrderByDescending(s => s.CreationDate);

            var paginatedNewestSubverses = new PaginatedList<Subverse>(subverses, page ?? 0, pageSize);

            return View("~/Views/Subverses/Subverses.cshtml", paginatedNewestSubverses);
        }

        // show subverses ordered by last received submission
        public ViewResult ActiveSubverses(int? page)
        {
            ViewBag.SelectedSubverse = "subverses";
            ViewBag.SortingMode = "active";

            const int pageSize = 100;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return NotFoundErrorView();
            }
            var subverses = CacheHandler.Instance.Register("Legacy:ActiveSubverses", new Func<IList<Subverse>>(() => {
                using (var db = new voatEntities())
                {
                    db.EnableCacheableOutput();

                    //HACK: I'm either completely <censored> or this is a huge pain in EF (sorting on a joined column and using .Distinct()), what you see below is a total hack that 'kinda' works
                    return (from subverse in db.Subverses
                            join submission in db.Submissions on subverse.Name equals submission.Subverse
                            where subverse.Description != null && subverse.SideBar != null
                            orderby submission.CreationDate descending
                            select subverse).Take(pageSize).ToList().Distinct().ToList();
                }
            }), TimeSpan.FromMinutes(15));

            //Turn off paging and only show the top ~50 most active
            var paginatedActiveSubverses = new PaginatedList<Subverse>(subverses, 0, pageSize, pageSize);

            return View("~/Views/Subverses/Subverses.cshtml", paginatedActiveSubverses);
        }

        public ActionResult Subversenotfound()
        {
            ViewBag.SelectedSubverse = "404";
            return SubverseNotFoundErrorView();
        }

        public ActionResult AdultContentFiltered(string destination)
        {
            ViewBag.SelectedSubverse = destination;
            return View("~/Views/Subverses/AdultContentFiltered.cshtml");
        }

        public ActionResult AdultContentWarning(string destination, bool? nsfwok)
        {
            ViewBag.SelectedSubverse = String.Empty;

            if (destination == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (nsfwok != null && nsfwok == true)
            {
                // setup nswf cookie
                HttpCookie hc = new HttpCookie("NSFWEnabled", "1");
                hc.Expires = Repository.CurrentDate.AddYears(1);
                System.Web.HttpContext.Current.Response.Cookies.Add(hc);

                // redirect to destination subverse
                return RedirectToAction("SubverseIndex", "Subverses", new { subverse = destination });
            }
            ViewBag.Destination = destination;
            return View("~/Views/Subverses/AdultContentWarning.cshtml");
        }

        // GET: fetch a random subbverse with x subscribers and x submissions
        public ActionResult Random()
        {
            try
            {
                string randomSubverse = RandomSubverse(true);
                return RedirectToAction("SubverseIndex", "Subverses", new { subverse = randomSubverse });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // GET: fetch a random NSFW subbverse with x subscribers and x submissions
        public ActionResult RandomNsfw()
        {
            try
            {
                string randomSubverse = RandomSubverse(false);
                return RedirectToAction("SubverseIndex", "Subverses", new { subverse = randomSubverse });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // GET: subverse moderators for selected subverse
        [Authorize]
        public ActionResult SubverseModerators(string subversetoshow)
        {
            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subversetoshow);
            if (subverseModel == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (!ModeratorPermission.HasPermission(User.Identity.Name, subversetoshow, Domain.Models.ModeratorAction.InviteMods))
            {
                return RedirectToAction("Index", "Home");
            }

            var subverseModerators = _db.SubverseModerators
                .Where(n => n.Subverse == subversetoshow)
                .Take(20)
                .OrderBy(s => s.Power)
                .ToList();

            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subversetoshow;

            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/SubverseModerators.cshtml", subverseModerators);
        }

        // GET: subverse moderator invitations for selected subverse
        [Authorize]
        public ActionResult ModeratorInvitations(string subversetoshow)
        {
            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subversetoshow);
            if (subverseModel == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (!ModeratorPermission.HasPermission(User.Identity.Name, subversetoshow, Domain.Models.ModeratorAction.InviteMods))
            {
                return RedirectToAction("Index", "Home");
            }

            var moderatorInvitations = _db.ModeratorInvitations
                .Where(mi => mi.Subverse == subversetoshow)
                .Take(20)
                .OrderBy(s => s.Power)
                .ToList();

            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subversetoshow;

            return PartialView("~/Views/Subverses/Admin/_ModeratorInvitations.cshtml", moderatorInvitations);
        }

        // GET: banned users for selected subverse
        [Authorize]
        public ActionResult SubverseBans(string subversetoshow, int? page)
        {
            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return NotFoundErrorView();
            }

            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subversetoshow);

            if (subverseModel == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (!ModeratorPermission.HasPermission(User.Identity.Name, subversetoshow, Domain.Models.ModeratorAction.Banning))
            {
                return RedirectToAction("Index", "Home");
            }

            var subverseBans = _db.SubverseBans.Where(n => n.Subverse == subversetoshow).OrderByDescending(s => s.CreationDate);
            var paginatedSubverseBans = new PaginatedList<SubverseBan>(subverseBans, page ?? 0, pageSize);

            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subversetoshow;

            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/SubverseBans.cshtml", paginatedSubverseBans);
        }
        #region Banning
        // GET: show add ban view for selected subverse
        [Authorize]
        public ActionResult AddBan(string subversetoshow)
        {
            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subversetoshow);

            if (subverseModel == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!ModeratorPermission.HasPermission(User.Identity.Name, subversetoshow, Domain.Models.ModeratorAction.Banning))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subversetoshow;
            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/AddBan.cshtml");
        }

        // POST: add a user ban to given subverse
        [Authorize]
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> AddBan([Bind(Include = "Id,Subverse,UserName,Reason")] SubverseBan subverseBan)
        {
            if (!ModelState.IsValid)
            {
                return View(subverseBan);
            }
            //check perms
            if (!ModeratorPermission.HasPermission(User.Identity.Name, subverseBan.Subverse, Domain.Models.ModeratorAction.Banning))
            {
                return RedirectToAction("Index", "Home");
            }

            var cmd = new SubverseBanCommand(subverseBan.UserName, subverseBan.Subverse, subverseBan.Reason, true);
            var result = await cmd.Execute();

            if (result.Success)
            {
                return RedirectToAction("SubverseBans");
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
                ViewBag.SubverseName = subverseBan.Subverse;
                ViewBag.SelectedSubverse = string.Empty;
                return View("~/Views/Subverses/Admin/AddBan.cshtml",
                new SubverseBanViewModel
                {
                    UserName = subverseBan.UserName,
                    Reason = subverseBan.Reason
                });
            }
        }
        // GET: show remove ban view for selected subverse
        [Authorize]
        public ActionResult RemoveBan(string subversetoshow, int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // check if caller is subverse owner, if not, deny listing
            if (!ModeratorPermission.HasPermission(User.Identity.Name, subversetoshow, Domain.Models.ModeratorAction.Banning))
            {
                return RedirectToAction("Index", "Home");
            }
            var subverseBan = _db.SubverseBans.Find(id);

            if (subverseBan == null)
            {
                return HttpNotFound();
            }

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SubverseName = subverseBan.Subverse;
            return View("~/Views/Subverses/Admin/RemoveBan.cshtml", subverseBan);
        }

        // POST: remove a ban from given subverse
        [Authorize]
        [HttpPost, ActionName("RemoveBan")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveBan(int id)
        {
            // get ban name for selected subverse
            var banToBeRemoved = await _db.SubverseBans.FindAsync(id);

            if (banToBeRemoved == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var cmd = new SubverseBanCommand(banToBeRemoved.UserName, banToBeRemoved.Subverse, null, false);
                var response = await cmd.Execute();
                if (response.Success)
                {
                    return RedirectToAction("SubverseBans");
                }
                else
                {
                    ModelState.AddModelError(String.Empty, response.Message);
                    ViewBag.SelectedSubverse = string.Empty;
                    ViewBag.SubverseName = banToBeRemoved.Subverse;
                    return View("~/Views/Subverses/Admin/RemoveBan.cshtml", banToBeRemoved);
                }
            }
        }

        
#endregion
        // GET: show remove moderator invitation view for selected subverse
        [Authorize]
        public ActionResult RecallModeratorInvitation(int? invitationId)
        {
            if (invitationId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var moderatorInvitation = _db.ModeratorInvitations.Find(invitationId);

            if (moderatorInvitation == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (!ModeratorPermission.HasPermission(User.Identity.Name, moderatorInvitation.Subverse, Domain.Models.ModeratorAction.InviteMods))
            {
                return RedirectToAction("SubverseModerators");
            }
            //make sure mods can't remove invites 
            var currentModLevel = ModeratorPermission.Level(User.Identity.Name, moderatorInvitation.Subverse);
            if (moderatorInvitation.Power <= (int)currentModLevel && currentModLevel != Domain.Models.ModeratorLevel.Owner)
            {
                return RedirectToAction("SubverseModerators");
            }

            ViewBag.SubverseName = moderatorInvitation.Subverse;
            return View("~/Views/Subverses/Admin/RecallModeratorInvitation.cshtml", moderatorInvitation);
        }

        // POST: remove a moderator invitation from given subverse
        [Authorize]
        [HttpPost, ActionName("RecallModeratorInvitation")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> RecallModeratorInvitation(int invitationId)
        {
            // get invitation to remove
            var invitationToBeRemoved = await _db.ModeratorInvitations.FindAsync(invitationId);
            if (invitationToBeRemoved == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // check if subverse exists
            var subverse = DataCache.Subverse.Retrieve(invitationToBeRemoved.Subverse);
            if (subverse == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            }

            // check if caller has clearance to remove a moderator invitation
            //if (!UserHelper.IsUserSubverseAdmin(User.Identity.Name, subverse.Name) || invitationToBeRemoved.Recipient == User.Identity.Name) return RedirectToAction("Index", "Home");
            if (!ModeratorPermission.HasPermission(User.Identity.Name, subverse.Name, Domain.Models.ModeratorAction.InviteMods))
            {
                return RedirectToAction("Index", "Home");
            }
            //make sure mods can't remove invites 
            var currentModLevel = ModeratorPermission.Level(User.Identity.Name, subverse.Name);
            if (invitationToBeRemoved.Power <= (int)currentModLevel && currentModLevel != Domain.Models.ModeratorLevel.Owner)
            {
                return RedirectToAction("SubverseModerators");
            }

            // execute invitation removal
            _db.ModeratorInvitations.Remove(invitationToBeRemoved);
            await _db.SaveChangesAsync();
            return RedirectToAction("SubverseModerators");
        }

       

        // GET: show resign as moderator view for selected subverse
        [Authorize]
        public ActionResult ResignAsModerator(string subversetoresignfrom)
        {
            if (subversetoresignfrom == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var subModerator = _db.SubverseModerators.FirstOrDefault(s => s.Subverse == subversetoresignfrom && s.UserName == User.Identity.Name);

            if (subModerator == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SubverseName = subModerator.Subverse;

            return View("~/Views/Subverses/Admin/ResignAsModerator.cshtml", subModerator);
        }

        // POST: resign as moderator from given subverse
        [Authorize]
        [HttpPost]
        [ActionName("ResignAsModerator")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> ResignAsModeratorPost(string subversetoresignfrom)
        {
            // get moderator name for selected subverse
            var subModerator = _db.SubverseModerators.FirstOrDefault(s => s.Subverse == subversetoresignfrom && s.UserName == User.Identity.Name);

            if (subModerator == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var subverse = DataCache.Subverse.Retrieve(subModerator.Subverse);
            if (subverse == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // execute removal
            _db.SubverseModerators.Remove(subModerator);
            await _db.SaveChangesAsync();

            //clear mod cache
            CacheHandler.Instance.Remove(CachingKey.SubverseModerators(subverse.Name));

            return RedirectToAction("SubverseIndex", "Subverses", new { subversetoshow = subversetoresignfrom });
        }

        

        // GET: show subverse flair settings view for selected subverse
        [Authorize]
        public ActionResult SubverseFlairSettings(string subversetoshow)
        {
            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subversetoshow);

            if (subverseModel == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // check if caller is authorized for this sub, if not, deny listing
            if (!ModeratorPermission.HasPermission(User.Identity.Name, subversetoshow, Domain.Models.ModeratorAction.ModifyFlair))
            {
                return RedirectToAction("Index", "Home");
            }

            var subverseFlairsettings = _db.SubverseFlairs
                .Where(n => n.Subverse == subversetoshow)
                .Take(20)
                .OrderBy(s => s.ID)
                .ToList();

            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subversetoshow;

            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/Flair/FlairSettings.cshtml", subverseFlairsettings);
        }

        // GET: show add link flair view for selected subverse
        [Authorize]
        public ActionResult AddLinkFlair(string subversetoshow)
        {
            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subversetoshow);

            if (subverseModel == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //check perms
            if (!ModeratorPermission.HasPermission(User.Identity.Name, subversetoshow, Domain.Models.ModeratorAction.ModifyFlair))
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subversetoshow;
            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/Flair/AddLinkFlair.cshtml");
        }

        // POST: add a link flair to given subverse
        [Authorize]
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        public ActionResult AddLinkFlair([Bind(Include = "Id,Subverse,Label,CssClass")] SubverseFlair subverseFlairSetting)
        {
            if (!ModelState.IsValid)
            {
                return View(subverseFlairSetting);
            }

            //check perms
            if (!ModeratorPermission.HasPermission(User.Identity.Name, subverseFlairSetting.Subverse, Domain.Models.ModeratorAction.ModifyFlair))
            {
                return RedirectToAction("Index", "Home");
            }

            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subverseFlairSetting.Subverse);
            if (subverseModel == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            subverseFlairSetting.Subverse = subverseModel.Name;
            _db.SubverseFlairs.Add(subverseFlairSetting);
            _db.SaveChanges();
            return RedirectToAction("SubverseFlairSettings");
        }

        // GET: show remove link flair view for selected subverse
        [Authorize]
        public ActionResult RemoveLinkFlair(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var subverseFlairSetting = _db.SubverseFlairs.Find(id);

            if (subverseFlairSetting == null)
            {
                return HttpNotFound();
            }

            ViewBag.SubverseName = subverseFlairSetting.Subverse;
            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/Flair/RemoveLinkFlair.cshtml", subverseFlairSetting);
        }

        // POST: remove a link flair from given subverse
        [Authorize]
        [HttpPost, ActionName("RemoveLinkFlair")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLinkFlair(int id)
        {
            // get link flair for selected subverse
            var linkFlairToRemove = await _db.SubverseFlairs.FindAsync(id);
            if (linkFlairToRemove == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var subverse = DataCache.Subverse.Retrieve(linkFlairToRemove.Subverse);
            if (subverse == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // check if caller has clearance to remove a link flair
            if (!ModeratorPermission.HasPermission(User.Identity.Name, subverse.Name, Domain.Models.ModeratorAction.ModifyFlair))
            {
                return RedirectToAction("Index", "Home");
            }

            // execute removal
            var subverseFlairSetting = await _db.SubverseFlairs.FindAsync(id);
            _db.SubverseFlairs.Remove(subverseFlairSetting);
            await _db.SaveChangesAsync();
            return RedirectToAction("SubverseFlairSettings");
        }

        // GET: render a partial view with list of moderators for a given subverse, if no moderators are found, return subverse owner
        [ChildActionOnly]
        public ActionResult SubverseModeratorsList(string subverseName)
        {
            var q = new QuerySubverseModerators(subverseName);
            var r = q.Execute();
            return PartialView("~/Views/Subverses/_SubverseModerators.cshtml", r);
        }

        // GET: stickied submission
        [ChildActionOnly]
        public ActionResult StickiedSubmission(string subverseName)
        {
            var stickiedSubmission = StickyHelper.GetSticky(subverseName);

            if (stickiedSubmission != null)
            {
                return PartialView("_Stickied", stickiedSubmission);
            }
            else
            {
                return new EmptyResult();
            }
        }

        // GET: list of default subverses
        public ActionResult ListOfDefaultSubverses()
        {
            try
            {
                var q = new QueryDefaultSubverses();
                var r = q.Execute();

                //var listOfSubverses = _db.DefaultSubverses.OrderBy(s => s.Order).ToList();
                return PartialView("_ListOfDefaultSubverses", r);
            }
            catch (Exception)
            {
                return new EmptyResult();
            }
        }

        [Authorize]

        // GET: list of subverses user is subscribed to, used in hover menu
        public ActionResult ListOfSubversesUserIsSubscribedTo()
        {
            // show custom list of subverses in top menu
            var listOfSubverses = _db.SubverseSubscriptions
                .Where(s => s.UserName == User.Identity.Name)
                .OrderBy(s => s.Subverse);

            return PartialView("_ListOfSubscribedToSubverses", listOfSubverses);
        }

        // POST: subscribe to a subverse
        [Authorize]
        public async Task<JsonResult> Subscribe(string subverseName)
        {
            var cmd = new SubscriptionCommand(Domain.Models.DomainType.Subverse, Domain.Models.SubscriptionAction.Subscribe, subverseName);
            var r = await cmd.Execute();
            if (r.Success)
            {
                return Json("Subscription request was successful.", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(r.Message, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: unsubscribe from a subverse
        [Authorize]
        public async Task<JsonResult> UnSubscribe(string subverseName)
        {
            //var loggedInUser = User.Identity.Name;

            //Voat.Utilities.UserHelper.UnSubscribeFromSubverse(loggedInUser, subverseName);
            //return Json("Unsubscribe request was successful.", JsonRequestBehavior.AllowGet);
            var cmd = new SubscriptionCommand(Domain.Models.DomainType.Subverse, Domain.Models.SubscriptionAction.Unsubscribe, subverseName);
            var r = await cmd.Execute();
            if (r.Success)
            {
                return Json("Unsubscribe request was successful.", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(r.Message, JsonRequestBehavior.AllowGet);
            }

        }
        
        // POST: block a subverse
        [Authorize]
        public async Task<JsonResult> BlockSubverse(string subverseName)
        {
            var loggedInUser = User.Identity.Name;
            var cmd = new BlockCommand(Domain.Models.DomainType.Subverse, subverseName);
            var response = await cmd.Execute();

            if (response.Success)
            {
                return Json("Subverse block request was successful.", JsonRequestBehavior.AllowGet);
            }
            else
            {
                Response.StatusCode = 400;
                return Json(response.Message, JsonRequestBehavior.AllowGet);
            }
        }

        #region Submission Display Methods

        private void RecordSession(string subverse)
        {
            //TODO: Relocate this to a command object
            // register a new session for this subverse
            try
            {
                // register a new session for this subverse
                string clientIpAddress = UserHelper.UserIpAddress(Request);
                string ipHash = IpHash.CreateHash(clientIpAddress);
                SessionHelper.Add(subverse, ipHash);

                ViewBag.OnlineUsers = SessionHelper.ActiveSessionsForSubverse(subverse);
            }
            catch (Exception)
            {
                ViewBag.OnlineUsers = -1;
            }
        }
        private void SetFirstTimeCookie()
        {
            // setup a cookie to find first time visitors and display welcome banner
            const string cookieName = "NotFirstTime";
            if (ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
            {
                // not a first time visitor
                ViewBag.FirstTimeVisitor = false;
            }
            else
            {
                // add a cookie for first time visitors
                HttpCookie hc = new HttpCookie("NotFirstTime", "1");
                hc.Expires = Repository.CurrentDate.AddYears(1);
                System.Web.HttpContext.Current.Response.Cookies.Add(hc);

                ViewBag.FirstTimeVisitor = true;
            }
        }
        // GET: show a subverse index
        public async Task<ActionResult> SubverseIndex(int? page, string subverse, string sort = "hot", string time = "day", bool? previewMode = null)
        {
            ViewBag.previewMode = previewMode ?? false;

            const string cookieName = "NSFWEnabled";
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return NotFoundErrorView();
            }
            //Set to DEFAULT if querystring is present
            if (Request.QueryString["frontpage"] == "guest")
            {
                subverse = AGGREGATE_SUBVERSE.DEFAULT;
            }
            if (String.IsNullOrEmpty(subverse))
            {
                return SubverseNotFoundErrorView();
            }

            SetFirstTimeCookie();
            RecordSession(subverse);

            var options = new SearchOptions();
            options.Page = pageNumber;
            options.Count = 25;

            switch (sort.ToLower())
            {
                case "new":
                    options.Sort = Domain.Models.SortAlgorithm.New;
                    break;
                case "hot":
                    options.Sort = Domain.Models.SortAlgorithm.Rank;
                    break;
                case "top":
                    //set defaults
                    if (String.IsNullOrEmpty(time))
                    {
                        time = "day";
                    }
                    options.Sort = Domain.Models.SortAlgorithm.Top;
                    switch (time.ToLower())
                    {
                        case "day":
                            options.Span = Domain.Models.SortSpan.Day;
                            break;
                        case "week":
                            options.Span = Domain.Models.SortSpan.Week;
                            break;
                        case "month":
                            options.Span = Domain.Models.SortSpan.Month;
                            break;
                        case "year":
                            options.Span = Domain.Models.SortSpan.Year;
                            break;
                        case "all":
                            options.Span = Domain.Models.SortSpan.All;
                            break;
                        default:
                            throw new NotImplementedException("span " + time + " is unknown");
                    }
                    break;
                default:
                    throw new NotImplementedException("sort " + sort + " is unknown");
            }

            try
            {
                if (AGGREGATE_SUBVERSE.IsAggregate(subverse))
                {
                    if (subverse == AGGREGATE_SUBVERSE.FRONT)
                    {
                        //Check if user is logged in and has subscriptions, if not we convert to default query
                        if (User.Identity.IsAuthenticated && !UserData.HasSubscriptions())
                        {
                            subverse = AGGREGATE_SUBVERSE.DEFAULT;
                        }
                        ViewBag.SelectedSubverse = "frontpage";
                    }
                    else if (subverse == AGGREGATE_SUBVERSE.DEFAULT)
                    {
                        ViewBag.SelectedSubverse = "frontpage";
                    }
                    else
                    {
                        // selected subverse is ALL, show submissions from all subverses, sorted by rank
                        ViewBag.SelectedSubverse = "all";
                        ViewBag.Title = "all subverses";
                    }

                    //This should filter out NSFW if User is not logged in or has adult content disabled
                    var q = new QuerySubmissionsLegacy(subverse, options);
                    var results = await q.ExecuteAsync().ConfigureAwait(false);

                    PaginatedList<Submission> pageList = new PaginatedList<Submission>(results, options.Page, options.Count, -1);
                    return View("AggregateIndex", pageList);

                    //// check if user wants to see NSFW content by reading user preference
                    //if (User.Identity.IsAuthenticated)
                    //{
                    //    if (UserData.Preferences.EnableAdultContent)
                    //    {
                    //        var paginatedSubmissionsFromAllSubverses = new PaginatedList<Submission>(SubmissionsFromAllSubversesByRank(), page ?? 0, pageSize);
                    //        return View(paginatedSubmissionsFromAllSubverses);
                    //    }

                    //    // return only sfw submissions
                    //    paginatedSfwSubmissions = new PaginatedList<Submission>(SfwSubmissionsFromAllSubversesByRank(_db), page ?? 0, pageSize);
                    //    return View(paginatedSfwSubmissions);
                    //}

                    //// check if user wants to see NSFW content by reading NSFW cookie
                    //if (ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
                    //{
                    //    var paginatedSubmissionsFromAllSubverses = new PaginatedList<Submission>(SubmissionsFromAllSubversesByRank(), page ?? 0, pageSize);
                    //    return View(paginatedSubmissionsFromAllSubverses);
                    //}

                    //NEW LOGIC
                    //IAmAGate: Perf mods for caching
                    //string cacheKeyAll = String.Format("legacy:subverse:{0}:page.{1}.sort.{2}.sfw", "all", pageNumber, "rank").ToLower();
                    //Tuple<IList<Submission>, int> cacheDataAll = CacheHandler.Instance.Retrieve<Tuple<IList<Submission>, int>>(cacheKeyAll);

                    //if (cacheDataAll == null)
                    //{
                    //    var getDataFunc = new Func<object>(() =>
                    //    {
                    //        using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                    //        {
                    //            db.EnableCacheableOutput();

                    //            var x = SfwSubmissionsFromAllSubversesByRank(db);
                    //            int count = 50000;
                    //            List<Submission> content = x.Skip(pageNumber * pageSize).Take(pageSize).ToList();
                    //            return new Tuple<IList<Submission>, int>(content, count);
                    //        }
                    //    });
                    //    cacheDataAll = (Tuple<IList<Submission>, int>)CacheHandler.Instance.Register(cacheKeyAll, getDataFunc, TimeSpan.FromSeconds(subverseCacheTimeInSeconds), (pageNumber > 2) ? 5 : 0);
                    //}
                    //paginatedSfwSubmissions = new PaginatedList<Submission>(cacheDataAll.Item1, pageNumber, pageSize, cacheDataAll.Item2);
                    //return View(paginatedSfwSubmissions);
                }
                else
                {
                    // check if subverse exists, if not, send to a page not found error

                    //Can't use cached, view using to query db
                    var subverseObject = _db.Subverses.Find(subverse);

                    if (subverseObject == null)
                    {
                        ViewBag.SelectedSubverse = "404";
                        return SubverseNotFoundErrorView();
                    }

                    //HACK: Disable subverse
                    if (subverseObject.IsAdminDisabled.HasValue && subverseObject.IsAdminDisabled.Value)
                    {
                        ViewBag.Subverse = subverseObject.Name;
                        return SubverseDisabledErrorView();
                    }
                    //Check NSFW Settings
                    #region Original Logic Located after query takes place
                    //// check if subverse is rated adult, show a NSFW warning page before entering
                    //if (!subverse.IsAdult)
                    //{
                    //    return View(paginatedSubmissions);
                    //}

                    //// check if user wants to see NSFW content by reading user preference
                    //if (User.Identity.IsAuthenticated)
                    //{
                    //    if (UserData.Preferences.EnableAdultContent)
                    //    {
                    //        return View(paginatedSubmissions);
                    //    }

                    //    // display a view explaining that account preference is set to NO NSFW and why this subverse can not be shown
                    //    return RedirectToAction("AdultContentFiltered", "Subverses", new { destination = subverse.Name });
                    //}

                    //// check if user wants to see NSFW content by reading NSFW cookie
                    //if (!ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
                    //{
                    //    return RedirectToAction("AdultContentWarning", "Subverses", new { destination = subverse.Name, nsfwok = false });
                    //}
                    #endregion
                    if (subverseObject.IsAdult)
                    {
                        if (User.Identity.IsAuthenticated)
                        {
                            if (!UserData.Preferences.EnableAdultContent)
                            {
                                // display a view explaining that account preference is set to NO NSFW and why this subverse can not be shown
                                return RedirectToAction("AdultContentFiltered", "Subverses", new { destination = subverseObject.Name });
                            }
                        }
                        // check if user wants to see NSFW content by reading NSFW cookie
                        else if (!ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
                        {
                            return RedirectToAction("AdultContentWarning", "Subverses", new { destination = subverseObject.Name, nsfwok = false });
                        }
                    }

                    ViewBag.SelectedSubverse = subverseObject.Name;
                    ViewBag.Title = subverseObject.Description;

                    var q = new QuerySubmissionsLegacy(subverse, options);
                    var results = await q.ExecuteAsync().ConfigureAwait(false);

                    ////IAmAGate: Perf mods for caching
                    //string cacheKey = String.Format("legacy:subverse:{0}:page.{1}.sort.{2}", subversetoshow, pageNumber, "rank").ToLower();
                    //Tuple<IList<Submission>, int> cacheData = CacheHandler.Instance.Retrieve<Tuple<IList<Submission>, int>>(cacheKey);

                    //if (cacheData == null)
                    //{
                    //    var getDataFunc = new Func<object>(() =>
                    //    {
                    //        using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                    //        {
                    //            db.EnableCacheableOutput();

                    //            var x = SubmissionsFromASubverseByRank(subversetoshow, db);
                    //            int count = -1;
                    //            List<Submission> content = x.Skip(pageNumber * pageSize).Take(pageSize).ToList();
                    //            return new Tuple<IList<Submission>, int>(content, count);
                    //        }
                    //    });

                    //    cacheData = (Tuple<IList<Submission>, int>)CacheHandler.Instance.Register(cacheKey, getDataFunc, TimeSpan.FromSeconds(subverseCacheTimeInSeconds), (pageNumber < 3 ? 10 : 1));
                    //}

                    PaginatedList<Submission> paginatedSubmissions = new PaginatedList<Submission>(results, options.Page, options.Count, -1);
                    return View(paginatedSubmissions);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Obsolete("Arg Matie, you shipwrecked upon t'is Dead Code", true)]
        public async Task<ActionResult> SortedSubverseFrontpage(int? page, string subversetoshow, string sortingmode, string time)
        {
            // sortingmode: new, contraversial, hot, etc
            ViewBag.SortingMode = sortingmode;
            ViewBag.Time = time;
            ViewBag.SelectedSubverse = subversetoshow;
            ViewBag.Title = subversetoshow;

            if (!sortingmode.Equals("new") && !sortingmode.Equals("top"))
            {
                return RedirectToAction("Index", "Home");
            }

            int pageNumber = (page ?? 0);
            if (pageNumber < 0)
            {
                return NotFoundErrorView();
            }

            RecordSession(subversetoshow);

            var options = new SearchOptions();
            options.Page = pageNumber;
            options.Count = 25;

            switch (sortingmode.ToLower())
            {
                case "new":
                    options.Sort = Domain.Models.SortAlgorithm.New;
                    break;
                case "top":
                    //set defaults
                    if (String.IsNullOrEmpty(time))
                    {
                        time = "day";
                    }
                    options.Sort = Domain.Models.SortAlgorithm.Top;
                    switch (time.ToLower())
                    {
                        case "day":
                            options.Span = Domain.Models.SortSpan.Day;
                            break;
                        case "week":
                            options.Span = Domain.Models.SortSpan.Week;
                            break;
                        case "month":
                            options.Span = Domain.Models.SortSpan.Month;
                            break;
                        case "year":
                            options.Span = Domain.Models.SortSpan.Year;
                            break;
                        case "all":
                            options.Span = Domain.Models.SortSpan.All;
                            break;
                        default:
                            throw new NotImplementedException("span " + time + " is unknown");
                    }
                    break;
                default:
                    throw new NotImplementedException("sort " + sortingmode + " is unknown");
            }
            
            QuerySubmissionsLegacy q = null;
            IEnumerable<Data.Models.Submission> results = null;

            if (subversetoshow.Equals("all", StringComparison.OrdinalIgnoreCase))
            {

                //This should filter out NSFW if User is not logged in or has adult content disabled
                q = new QuerySubmissionsLegacy(AGGREGATE_SUBVERSE.ALL, options);
                results = await q.ExecuteAsync().ConfigureAwait(false);

                // selected subverse is ALL, show submissions from all subverses, sorted by date
                //return HandleSortedSubverseAll(page, sortingmode, time);
            }
            else
            {
                q = new QuerySubmissionsLegacy(subversetoshow, options);
                results = await q.ExecuteAsync().ConfigureAwait(false);
                //return HandleSortedSubverse(page, subversetoshow, sortingmode, time);

            }

            PaginatedList<Submission> paginatedSubmissions = new PaginatedList<Submission>(results, options.Page, options.Count, -1);
            return View("SubverseIndex", paginatedSubmissions);
        }

        [Obsolete("Arg Matie, you shipwrecked upon t'is Dead Code", true)]
        [ChildActionOnly]
        private ActionResult HandleSortedSubverse(int? page, string subversetoshow, string sortingmode, string daterange)
        {
            ViewBag.SortingMode = sortingmode;
            ViewBag.SelectedSubverse = subversetoshow;
            const string cookieName = "NSFWEnabled";
            DateTime startDate = DateTimeUtility.DateRangeToDateTime(daterange);

            if (!sortingmode.Equals("new") && !sortingmode.Equals("top"))
                return RedirectToAction("Index", "Home");

            const int pageSize = 25;
            int pageNumber = (page ?? 0);
            if (pageNumber < 0)
            {
                return NotFoundErrorView();
            }

            // check if subverse exists, if not, send to a page not found error
            var subverse = DataCache.Subverse.Retrieve(subversetoshow);
            if (subverse == null)
                return SubverseNotFoundErrorView();

            ViewBag.Title = subverse.Description;

            //HACK: Disable subverse
            if (subverse.IsAdminDisabled.HasValue && subverse.IsAdminDisabled.Value)
            {
                ViewBag.Subverse = subverse.Name;
                return SubverseDisabledErrorView();
            }

            // subverse is adult rated, check if user wants to see NSFW content
            PaginatedList<Submission> paginatedSubmissionsByRank;

            if (subverse.IsAdult)
            {
                if (User.Identity.IsAuthenticated)
                {
                    // check if user wants to see NSFW content by reading user preference
                    if (UserData.Preferences.EnableAdultContent)
                    {
                        if (sortingmode.Equals("new"))
                        {
                            var paginatedSubmissionsByDate = new PaginatedList<Submission>(SubmissionsFromASubverseByDate(subversetoshow), page ?? 0, pageSize);
                            return View("SubverseIndex", paginatedSubmissionsByDate);
                        }

                        if (sortingmode.Equals("top"))
                        {
                            var paginatedSubmissionsByDate = new PaginatedList<Submission>(SubmissionsFromASubverseByTop(subversetoshow, startDate), page ?? 0, pageSize);
                            return View("SubverseIndex", paginatedSubmissionsByDate);
                        }

                        // default sorting mode by rank
                        paginatedSubmissionsByRank = new PaginatedList<Submission>(SubmissionsFromASubverseByRank(subversetoshow, _db), page ?? 0, pageSize);
                        return View("SubverseIndex", paginatedSubmissionsByRank);
                    }
                    return RedirectToAction("AdultContentFiltered", "Subverses", new { destination = subverse.Name });
                }

                // check if user wants to see NSFW content by reading NSFW cookie
                if (!HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
                {
                    return RedirectToAction("AdultContentWarning", "Subverses",
                        new { destination = subverse.Name, nsfwok = false });
                }

                if (sortingmode.Equals("new"))
                {
                    var paginatedSubmissionsByDate = new PaginatedList<Submission>(SubmissionsFromASubverseByDate(subversetoshow), page ?? 0, pageSize);
                    return View("SubverseIndex", paginatedSubmissionsByDate);
                }

                if (sortingmode.Equals("top"))
                {
                    var paginatedSubmissionsByDate = new PaginatedList<Submission>(SubmissionsFromASubverseByTop(subversetoshow, startDate), page ?? 0, pageSize);
                    return View("SubverseIndex", paginatedSubmissionsByDate);
                }

                // default sorting mode by rank
                paginatedSubmissionsByRank = new PaginatedList<Submission>(SubmissionsFromASubverseByRank(subversetoshow), page ?? 0, pageSize);
                return View("SubverseIndex", paginatedSubmissionsByRank);
            }

            // subverse is safe for work
            if (sortingmode.Equals("new"))
            {
                //IAmAGate: Perf mods for caching
                string cacheKey = String.Format("legacy:subverse:{0}:page.{1}.sort.{2}", subversetoshow, pageNumber, sortingmode).ToLower();
                Tuple<IList<Submission>, int> cacheData = CacheHandler.Instance.Retrieve<Tuple<IList<Submission>, int>>(cacheKey);

                if (cacheData == null)
                {
                    var getDataFunc = new Func<object>(() =>
                    {
                        using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                        {
                            db.EnableCacheableOutput();

                            var x = SubmissionsFromASubverseByDate(subversetoshow, db);
                            int count = -1;
                            List<Submission> content = x.Skip(pageNumber * pageSize).Take(pageSize).ToList();
                            return new Tuple<IList<Submission>, int>(content, count);
                        }
                    });

                    cacheData = (Tuple<IList<Submission>, int>)CacheHandler.Instance.Register(cacheKey, getDataFunc, TimeSpan.FromSeconds(subverseCacheTimeInSeconds), (pageNumber < 3 ? 10 : 1));
                }

                PaginatedList<Submission> paginatedSubmissionsByDate = new PaginatedList<Submission>(cacheData.Item1, pageNumber, pageSize, cacheData.Item2);

                return View("SubverseIndex", paginatedSubmissionsByDate);

            }

            if (sortingmode.Equals("top"))
            {
                var paginatedSubmissionsByDate = new PaginatedList<Submission>(SubmissionsFromASubverseByTop(subversetoshow, startDate), page ?? 0, pageSize);
                return View("SubverseIndex", paginatedSubmissionsByDate);
            }

            // default sorting mode by rank
            paginatedSubmissionsByRank = new PaginatedList<Submission>(SubmissionsFromASubverseByRank(subversetoshow), page ?? 0, pageSize);
            return View("SubverseIndex", paginatedSubmissionsByRank);
        }
        [Obsolete("Arg Matie, you shipwrecked upon t'is Dead Code", true)]
        [ChildActionOnly]
        private ActionResult HandleSortedSubverseAll(int? page, string sortingmode, string daterange)
        {
            const string cookieName = "NSFWEnabled";
            const int pageSize = 25;
            DateTime startDate = DateTimeUtility.DateRangeToDateTime(daterange);
            PaginatedList<Submission> paginatedSubmissions;
            int pageNumber = page ?? 0;

            ViewBag.SelectedSubverse = "all";

            #region authenticated users

            if (User.Identity.IsAuthenticated)
            {
                var blockedSubverses = _db.UserBlockedSubverses.Where(x => x.UserName.Equals(User.Identity.Name)).Select(x => x.Subverse);
                IQueryable<Submission> submissionsExcludingBlockedSubverses;

                #region authenticated users NSFW content

                // check if user wants to see NSFW content by reading user preference and exclude submissions from blocked subverses
                if (UserData.Preferences.EnableAdultContent)
                {
                    if (sortingmode.Equals("new"))
                    {
                        submissionsExcludingBlockedSubverses = SubmissionsFromAllSubversesByDate().Where(x => !blockedSubverses.Contains(x.Subverse));
                        paginatedSubmissions = new PaginatedList<Submission>(submissionsExcludingBlockedSubverses, page ?? 0, pageSize);
                        return View("SubverseIndex", paginatedSubmissions);
                    }

                    if (sortingmode.Equals("top"))
                    {
                        submissionsExcludingBlockedSubverses = SubmissionsFromAllSubversesByTop(startDate).Where(x => !blockedSubverses.Contains(x.Subverse));
                        paginatedSubmissions = new PaginatedList<Submission>(submissionsExcludingBlockedSubverses, page ?? 0, pageSize);
                        return View("SubverseIndex", paginatedSubmissions);
                    }

                    // default sorting mode by rank
                    submissionsExcludingBlockedSubverses = SubmissionsFromAllSubversesByRank().Where(x => !blockedSubverses.Contains(x.Subverse));
                    paginatedSubmissions = new PaginatedList<Submission>(submissionsExcludingBlockedSubverses, page ?? 0, pageSize);
                    return View("SubverseIndex", paginatedSubmissions);
                }

                #endregion authenticated users NSFW content

                #region authenticated users SFW content

                // user does not want to see NSFW content
                if (sortingmode.Equals("new"))
                {
                    submissionsExcludingBlockedSubverses = SfwSubmissionsFromAllSubversesByDate().Where(x => !blockedSubverses.Contains(x.Subverse));
                    paginatedSubmissions = new PaginatedList<Submission>(submissionsExcludingBlockedSubverses, page ?? 0, pageSize);
                    return View("SubverseIndex", paginatedSubmissions);
                }
                if (sortingmode.Equals("top"))
                {
                    submissionsExcludingBlockedSubverses = SfwSubmissionsFromAllSubversesByTop(startDate).Where(x => !blockedSubverses.Contains(x.Subverse));
                    paginatedSubmissions = new PaginatedList<Submission>(submissionsExcludingBlockedSubverses, page ?? 0, pageSize);
                    return View("SubverseIndex", paginatedSubmissions);
                }

                // default sorting mode by rank
                submissionsExcludingBlockedSubverses = SfwSubmissionsFromAllSubversesByRank(_db).Where(x => !blockedSubverses.Contains(x.Subverse));
                paginatedSubmissions = new PaginatedList<Submission>(submissionsExcludingBlockedSubverses, page ?? 0, pageSize);
                return View("SubverseIndex", paginatedSubmissions);

                #endregion authenticated users SFW content
            }

            #endregion authenticated users

            #region guest users: SFW submissions

            // guest users: check if user wants to see NSFW content by reading NSFW cookie
            if (!HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
            {
                if (sortingmode.Equals("new"))
                {
                    //IAmAGate: Perf mods for caching
                    int size = pageSize;
                    string cacheKeyAll = String.Format("legacy:subverse:{0}:page.{1}.sort.{2}.sfw", "all", pageNumber, "new").ToLower();
                    Tuple<IList<Submission>, int> cacheDataAll = CacheHandler.Instance.Retrieve<Tuple<IList<Submission>, int>>(cacheKeyAll);

                    if (cacheDataAll == null)
                    {
                        var getDataFunc = new Func<object>(() =>
                        {
                            using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                            {
                                db.EnableCacheableOutput();

                                var x = SfwSubmissionsFromAllSubversesByDate(db);
                                int count = 50000;
                                List<Submission> content = x.Skip(pageNumber * size).Take(size).ToList();
                                return new Tuple<IList<Submission>, int>(content, count);
                            }
                        });

                        cacheDataAll = (Tuple<IList<Submission>, int>)CacheHandler.Instance.Register(cacheKeyAll, getDataFunc, TimeSpan.FromSeconds(subverseCacheTimeInSeconds), (pageNumber < 3 ? 10 : 1));
                    }
                    paginatedSubmissions = new PaginatedList<Submission>(cacheDataAll.Item1, pageNumber, pageSize, cacheDataAll.Item2);

                    //paginatedSubmissions = new PaginatedList<Message>(SfwSubmissionsFromAllSubversesByDate(), page ?? 0, pageSize);
                    return View("SubverseIndex", paginatedSubmissions);
                }
                if (sortingmode.Equals("top"))
                {
                    string cacheKeyTopSfw = $"legacy:subverse.all.guest.page.{pageNumber}.sort.top.{daterange}.sfw"; // daterange may be null, no biggie
                    Tuple<IList<Submission>, int> cacheDataTopSfw = CacheHandler.Instance.Retrieve<Tuple<IList<Submission>, int>>(cacheKeyTopSfw);
                    if (cacheDataTopSfw == null)
                    {
                        var getDataFunc = new Func<object>(() =>
                        {
                            using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                            {
                                db.EnableCacheableOutput();

                                var dataToCache = SfwSubmissionsFromAllSubversesByTop(startDate, db);
                                int count = 50000;
                                List<Submission> content = dataToCache.Skip(pageNumber * pageSize).Take(pageSize).ToList();
                                return new Tuple<IList<Submission>, int>(content, count);
                            }
                        });

                        cacheDataTopSfw = (Tuple<IList<Submission>, int>)CacheHandler.Instance.Register(cacheKeyTopSfw, getDataFunc, TimeSpan.FromSeconds(subverseCacheTimeInSeconds), (pageNumber < 3 ? 10 : 1));
                    }

                    // paginatedSubmissions = new PaginatedList<Submission>(SfwSubmissionsFromAllSubversesByTop(startDate), page ?? 0, pageSize);
                    paginatedSubmissions = new PaginatedList<Submission>(cacheDataTopSfw.Item1, pageNumber, pageSize, cacheDataTopSfw.Item2);
                    return View("SubverseIndex", paginatedSubmissions);
                }

                // default sorting mode by rank
                // TODO: cache this
                string cacheKeyRankNsfw = $"legacy:subverse.all.guest.page.{pageNumber}.sort.rank.sfw";
                Tuple<IList<Submission>, int> cacheDataRankNsfw = CacheHandler.Instance.Retrieve<Tuple<IList<Submission>, int>>(cacheKeyRankNsfw);
                if (cacheDataRankNsfw == null)
                {
                    var getDataFunc = new Func<object>(() =>
                    {
                        using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                        {
                            db.EnableCacheableOutput();

                            var dataToCache = SfwSubmissionsFromAllSubversesByRank(db);
                            int count = 50000;
                            List<Submission> content = dataToCache.Skip(pageNumber * pageSize).Take(pageSize).ToList();
                            return new Tuple<IList<Submission>, int>(content, count);
                        }
                    });

                    cacheDataRankNsfw = (Tuple<IList<Submission>, int>)CacheHandler.Instance.Register(cacheKeyRankNsfw, getDataFunc, TimeSpan.FromSeconds(subverseCacheTimeInSeconds), (pageNumber < 3 ? 10 : 1));
                }

                // paginatedSubmissions = new PaginatedList<Submission>(SfwSubmissionsFromAllSubversesByRank(_db), page ?? 0, pageSize);
                paginatedSubmissions = new PaginatedList<Submission>(cacheDataRankNsfw.Item1, pageNumber, pageSize, cacheDataRankNsfw.Item2);
                return View("SubverseIndex", paginatedSubmissions);
            }

            #endregion guest users: SFW submissions

            #region guest users: submissions including NSFW

            if (sortingmode.Equals("new"))
            {
                //IAmAGate: Perf mods for caching
                string cacheKeyAll = String.Format("legacy:subverse:{0}:page.{1}.sort.{2}.nsfw", "all", pageNumber, "new").ToLower();
                Tuple<IList<Submission>, int> cacheDataAll = CacheHandler.Instance.Retrieve<Tuple<IList<Submission>, int>>(cacheKeyAll);

                if (cacheDataAll == null)
                {
                    var getDataFunc = new Func<object>(() =>
                    {
                        using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                        {
                            db.EnableCacheableOutput();

                            var x = SubmissionsFromAllSubversesByDate(db);
                            int count = 50000;
                            List<Submission> content = x.Skip(pageNumber * pageSize).Take(pageSize).ToList();
                            return new Tuple<IList<Submission>, int>(content, count);
                        }
                    });

                    cacheDataAll = (Tuple<IList<Submission>, int>)CacheHandler.Instance.Register(cacheKeyAll, getDataFunc, TimeSpan.FromSeconds(subverseCacheTimeInSeconds), (pageNumber < 3 ? 10 : 1));
                }

                //paginatedSubmissions = new PaginatedList<Message>(SubmissionsFromAllSubversesByDate(), page ?? 0, pageSize);
                paginatedSubmissions = new PaginatedList<Submission>(cacheDataAll.Item1, pageNumber, pageSize, cacheDataAll.Item2);
                return View("SubverseIndex", paginatedSubmissions);
            }

            if (sortingmode.Equals("top"))
            {
                string cacheKeyTop = $"legacy:subverse.all.guest.page.{pageNumber}.sort.top.{daterange}.nsfw";
                Tuple<IList<Submission>, int> cacheDataTop = CacheHandler.Instance.Retrieve<Tuple<IList<Submission>, int>>(cacheKeyTop);
                if (cacheDataTop == null)
                {
                    var getDataFunc = new Func<object>(() =>
                    {
                        using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                        {
                            db.EnableCacheableOutput();

                            var dataToCache = SubmissionsFromAllSubversesByTop(startDate, db);
                            int count = 50000;
                            List<Submission> content = dataToCache.Skip(pageNumber * pageSize).Take(pageSize).ToList();
                            return new Tuple<IList<Submission>, int>(content, count);
                        }
                    });

                    cacheDataTop = (Tuple<IList<Submission>, int>)CacheHandler.Instance.Register(cacheKeyTop, getDataFunc, TimeSpan.FromSeconds(subverseCacheTimeInSeconds), (pageNumber < 3 ? 10 : 1));
                }

                // paginatedSubmissions = new PaginatedList<Submission>(SubmissionsFromAllSubversesByTop(startDate), page ?? 0, pageSize);
                paginatedSubmissions = new PaginatedList<Submission>(cacheDataTop.Item1, pageNumber, pageSize, cacheDataTop.Item2);
                return View("SubverseIndex", paginatedSubmissions);
            }

            // default sorting mode by rank
            string cacheKeyRank = $"legacy:subverse.all.guest.page.{pageNumber}.sort.rank.nsfw";
            Tuple<IList<Submission>, int> cacheDataRank = CacheHandler.Instance.Retrieve<Tuple<IList<Submission>, int>>(cacheKeyRank);
            if (cacheDataRank == null)
            {
                var getDataFunc = new Func<object>(() =>
                {
                    using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                    {
                        db.EnableCacheableOutput();

                        var dataToCache = SubmissionsFromAllSubversesByRank(db);
                        int count = 50000;
                        List<Submission> content = dataToCache.Skip(pageNumber * pageSize).Take(pageSize).ToList();
                        return new Tuple<IList<Submission>, int>(content, count);
                    }
                });

                cacheDataRank = (Tuple<IList<Submission>, int>)CacheHandler.Instance.Register(cacheKeyRank, getDataFunc, TimeSpan.FromSeconds(subverseCacheTimeInSeconds), (pageNumber < 3 ? 10 : 1));
            }

            // paginatedSubmissions = new PaginatedList<Submission>(SubmissionsFromAllSubversesByRank(), page ?? 0, pageSize);
            paginatedSubmissions = new PaginatedList<Submission>(cacheDataRank.Item1, pageNumber, pageSize, cacheDataRank.Item2);
            return View("SubverseIndex", paginatedSubmissions);

            #endregion guest users: submissions including NSFW
        }


        #region sfw submissions from all subverses
        [Obsolete("Arg Matie, you shipwrecked upon t'is Dead Code", true)]
        private IQueryable<Submission> SfwSubmissionsFromAllSubversesByDate(voatEntities _db = null)
        {
            if (_db == null)
            {
                _db = this._db;
            }
            string userName = "";
            if (User != null)
            {
                userName = User.Identity.Name;
            }
            IQueryable<Submission> sfwSubmissionsFromAllSubversesByDate = (from message in _db.Submissions
                                                                           join subverse in _db.Subverses on message.Subverse equals subverse.Name
                                                                           where !message.IsArchived && !message.IsDeleted && subverse.IsPrivate != true && subverse.IsAdminPrivate != true && subverse.IsAdult == false && subverse.MinCCPForDownvote == 0
                                                                           where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                           where !(from bu in _db.SubverseBans where bu.Subverse == subverse.Name select bu.UserName).Contains(message.UserName)
                                                                           where !subverse.IsAdminDisabled.Value
                                                                           where !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(subverse.Name) select ubs.UserName).Contains(userName)
                                                                           select message
                                                                        ).OrderByDescending(s => s.CreationDate).AsNoTracking();

            return sfwSubmissionsFromAllSubversesByDate;
        }
        [Obsolete("Arg Matie, you shipwrecked upon t'is Dead Code", true)]
        private IQueryable<Submission> SfwSubmissionsFromAllSubversesByRank(voatEntities _db)
        {
            if (_db == null)
            {
                _db = this._db;
            }
            string userName = "";
            if (User != null)
            {
                userName = User.Identity.Name;
            }
            IQueryable<Submission> sfwSubmissionsFromAllSubversesByRank = (from message in _db.Submissions
                                                                           join subverse in _db.Subverses on message.Subverse equals subverse.Name
                                                                           where !message.IsArchived && !message.IsDeleted && subverse.IsPrivate != true && subverse.IsAdminPrivate != true && subverse.IsAdult == false && subverse.MinCCPForDownvote == 0 && message.Rank > 0.00009
                                                                           where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                           where !subverse.IsAdminDisabled.Value
                                                                           where !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(subverse.Name) select ubs.UserName).Contains(userName)
                                                                           select message).OrderByDescending(s => s.Rank).ThenByDescending(s => s.CreationDate).AsNoTracking();

            return sfwSubmissionsFromAllSubversesByRank;
        }
        [Obsolete("Arg Matie, you shipwrecked upon t'is Dead Code", true)]
        private IQueryable<Submission> SfwSubmissionsFromAllSubversesByTop(DateTime startDate, voatEntities _db = null)
        {
            if (_db == null)
            {
                _db = this._db;
            }
            IQueryable<Submission> sfwSubmissionsFromAllSubversesByTop = (from message in _db.Submissions
                                                                          join subverse in _db.Subverses on message.Subverse equals subverse.Name
                                                                          where !message.IsDeleted && subverse.IsPrivate != true && subverse.IsAdult == false && subverse.MinCCPForDownvote == 0
                                                                          where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                          where !subverse.IsAdminDisabled.Value
                                                                          where !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(subverse.Name) select ubs.UserName).Contains(User.Identity.Name)
                                                                          select message).OrderByDescending(s => s.UpCount - s.DownCount).Where(s => s.CreationDate >= startDate && s.CreationDate <= Repository.CurrentDate)
                                                                       .AsNoTracking();

            return sfwSubmissionsFromAllSubversesByTop;
        }
        //TODO: Move to dedicated query object
        //[Obsolete("Arg Matie, you shipwrecked upon t'is Dead Code", true)]
        private IQueryable<Submission> SfwSubmissionsFromAllSubversesByViews24Hours(voatEntities _db)
        {
            if (_db == null)
            {
                _db = this._db;
            }
            var startDate = Repository.CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));
            IQueryable<Submission> sfwSubmissionsFromAllSubversesByViews24Hours = (from message in _db.Submissions
                                                                                   join subverse in _db.Subverses on message.Subverse equals subverse.Name
                                                                                   where !message.IsArchived && !message.IsDeleted && subverse.IsPrivate != true && subverse.IsAdminPrivate != true && subverse.IsAdult == false && message.CreationDate >= startDate && message.CreationDate <= Repository.CurrentDate
                                                                                   where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                                   where !subverse.IsAdminDisabled.Value
                                                                                   where !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(subverse.Name) select ubs.UserName).Contains(User.Identity.Name)
                                                                                   select message).OrderByDescending(s => s.Views).DistinctBy(m => m.Subverse).Take(5).AsQueryable().AsNoTracking();

            return sfwSubmissionsFromAllSubversesByViews24Hours;
        }

        #endregion sfw submissions from all subverses

        #region unfiltered submissions from all subverses
        [Obsolete("Arg Matie, you shipwrecked upon t'is Dead Code", true)]
        private IQueryable<Submission> SubmissionsFromAllSubversesByDate(voatEntities _db = null)
        {
            if (_db == null)
            {
                _db = this._db;
            }
            string userName = "";
            if (User != null)
            {
                userName = User.Identity.Name;
            }
            IQueryable<Submission> submissionsFromAllSubversesByDate = (from message in _db.Submissions
                                                                        join subverse in _db.Subverses on message.Subverse equals subverse.Name
                                                                        where !message.IsArchived && !message.IsDeleted && subverse.IsPrivate != true && subverse.IsAdminPrivate != true && subverse.MinCCPForDownvote == 0
                                                                        where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                        where !subverse.IsAdminDisabled.Value
                                                                        where !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(subverse.Name) select ubs.UserName).Contains(userName)
                                                                        select message).OrderByDescending(s => s.CreationDate).AsNoTracking();

            return submissionsFromAllSubversesByDate;
        }
        [Obsolete("Arg Matie, you shipwrecked upon t'is Dead Code", true)]
        private IQueryable<Submission> SubmissionsFromAllSubversesByRank(voatEntities _db = null)
        {
            if (_db == null)
            {
                _db = this._db;
            }
            IQueryable<Submission> submissionsFromAllSubversesByRank = (from message in _db.Submissions
                                                                        join subverse in _db.Subverses on message.Subverse equals subverse.Name
                                                                        where !message.IsArchived && !message.IsDeleted && subverse.IsPrivate != true && subverse.IsAdminPrivate != true && subverse.MinCCPForDownvote == 0 && message.Rank > 0.00009
                                                                        where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                        where !subverse.IsAdminDisabled.Value
                                                                        where !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(subverse.Name) select ubs.UserName).Contains(User.Identity.Name)
                                                                        select message).OrderByDescending(s => s.Rank).ThenByDescending(s => s.CreationDate).AsNoTracking();

            return submissionsFromAllSubversesByRank;
        }
        [Obsolete("Arg Matie, you shipwrecked upon t'is Dead Code", true)]
        private IQueryable<Submission> SubmissionsFromAllSubversesByTop(DateTime startDate, voatEntities _db = null)
        {
            if (_db == null)
            {
                _db = this._db;
            }

            IQueryable<Submission> submissionsFromAllSubversesByTop = (from message in _db.Submissions
                                                                       join subverse in _db.Subverses on message.Subverse equals subverse.Name
                                                                       where !message.IsDeleted && subverse.IsPrivate != true && subverse.IsAdminPrivate != true && subverse.MinCCPForDownvote == 0
                                                                       where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                       where !subverse.IsAdminDisabled.Value
                                                                       where !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(subverse.Name) select ubs.UserName).Contains(User.Identity.Name)
                                                                       select message).OrderByDescending(s => s.UpCount - s.DownCount).Where(s => s.CreationDate >= startDate && s.CreationDate <= Repository.CurrentDate)
                                                                    .AsNoTracking();

            return submissionsFromAllSubversesByTop;
        }

        #endregion unfiltered submissions from all subverses

        #region submissions from a single subverse
        [Obsolete("Arg Matie, you shipwrecked upon t'is Dead Code", true)]
        private IQueryable<Submission> SubmissionsFromASubverseByDate(string subverseName, voatEntities _db = null)
        {
            if (_db == null)
            {
                _db = this._db;
            }
            var subverseStickie = _db.StickiedSubmissions.FirstOrDefault(ss => ss.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase));
            IQueryable<Submission> submissionsFromASubverseByDate = (from message in _db.Submissions
                                                                     join subverse in _db.Subverses on message.Subverse equals subverse.Name
                                                                     where !message.IsDeleted && message.Subverse == subverseName
                                                                     where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                     where !(from bu in _db.SubverseBans where bu.Subverse == subverse.Name select bu.UserName).Contains(message.UserName)
                                                                     select message).OrderByDescending(s => s.CreationDate).AsNoTracking();

            if (subverseStickie != null)
            {
                return submissionsFromASubverseByDate.Where(s => s.ID != subverseStickie.SubmissionID);
            }
            return submissionsFromASubverseByDate;
        }
        [Obsolete("Arg Matie, you shipwrecked upon t'is Dead Code", true)]
        private IQueryable<Submission> SubmissionsFromASubverseByRank(string subverseName, voatEntities _db = null)
        {
            if (_db == null)
            {
                _db = this._db;
            }
            var subverseStickie = _db.StickiedSubmissions.FirstOrDefault(ss => ss.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase));
            IQueryable<Submission> submissionsFromASubverseByRank = (from message in _db.Submissions
                                                                     join subverse in _db.Subverses on message.Subverse equals subverse.Name
                                                                     where !message.IsDeleted && message.Subverse == subverseName
                                                                     where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                     select message).OrderByDescending(s => s.Rank).ThenByDescending(s => s.CreationDate).AsNoTracking();

            if (subverseStickie != null)
            {
                return submissionsFromASubverseByRank.Where(s => s.ID != subverseStickie.SubmissionID);
            }
            return submissionsFromASubverseByRank;
        }
        [Obsolete("Arg Matie, you shipwrecked upon t'is Dead Code", true)]
        private IQueryable<Submission> SubmissionsFromASubverseByTop(string subverseName, DateTime startDate)
        {
            var subverseStickie = _db.StickiedSubmissions.FirstOrDefault(ss => ss.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase));
            IQueryable<Submission> submissionsFromASubverseByTop = (from message in _db.Submissions
                                                                    join subverse in _db.Subverses on message.Subverse equals subverse.Name
                                                                    where !message.IsDeleted && message.Subverse == subverseName
                                                                    where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                    select message).OrderByDescending(s => s.UpCount - s.DownCount).Where(s => s.CreationDate >= startDate && s.CreationDate <= Repository.CurrentDate)
                                                                 .AsNoTracking();
            if (subverseStickie != null)
            {
                return submissionsFromASubverseByTop.Where(s => s.ID != subverseStickie.SubmissionID);
            }
            return submissionsFromASubverseByTop;
        }

        #endregion submissions from a single subverse

        #endregion



        [ChildActionOnly]
        [OutputCache(Duration = 600, VaryByParam = "none")]
        public ActionResult TopViewedSubmissions24Hours()
        {
            //var submissions =
            var cacheData = CacheHandler.Instance.Register("legacy:TopViewedSubmissions24Hours", new Func<object>(() =>
            {
                using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                {
                    db.EnableCacheableOutput();

                    return SfwSubmissionsFromAllSubversesByViews24Hours(db).ToList();
                }
            }), TimeSpan.FromMinutes(60), 5);

            return PartialView("_MostViewedSubmissions", cacheData);
        }

        #region ADD/REMOVE MODERATORS LOGIC

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> AcceptModInvitation(int invitationId)
        {
            int maximumOwnedSubs = Settings.MaximumOwnedSubs;

            //TODO: These errors are not friendly - please update to redirect or something
            // check if there is an invitation for this user with this id
            var userInvitation = _db.ModeratorInvitations.Find(invitationId);
            if (userInvitation == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // check if logged in user is actually the invited user
            if (!User.Identity.Name.Equals(userInvitation.Recipient, StringComparison.OrdinalIgnoreCase))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            // check if user is over modding limits
            var amountOfSubsUserModerates = _db.SubverseModerators.Where(s => s.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
            if (amountOfSubsUserModerates.Any())
            {
                if (amountOfSubsUserModerates.Count() >= maximumOwnedSubs)
                {
                    ModelState.AddModelError(string.Empty, "Sorry, you can not own or moderate more than " + maximumOwnedSubs + " subverses.");
                    return RedirectToAction("Index", "Home");
                }
            }

            // check if subverse exists
            var subverseToAddModTo = _db.Subverses.FirstOrDefault(s => s.Name.Equals(userInvitation.Subverse, StringComparison.OrdinalIgnoreCase));
            if (subverseToAddModTo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // check if user is already a moderator of this sub
            var userModerating = _db.SubverseModerators.Where(s => s.Subverse.Equals(userInvitation.Subverse, StringComparison.OrdinalIgnoreCase) && s.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
            if (userModerating.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // add user as moderator as specified in invitation
            var subAdm = new SubverseModerator
            {
                Subverse = subverseToAddModTo.Name,
                UserName = UserHelper.OriginalUsername(userInvitation.Recipient),
                Power = userInvitation.Power,
                CreatedBy = UserHelper.OriginalUsername(userInvitation.CreatedBy),
                CreationDate = Repository.CurrentDate
            };

            _db.SubverseModerators.Add(subAdm);

            // notify sender that user has accepted the invitation
            var message = new Domain.Models.SendMessage()
            {
                Sender = $"v/{subverseToAddModTo}",
                Subject = $"Moderator invitation for v/{userInvitation.Subverse} accepted",
                Recipient = userInvitation.CreatedBy,
                Message = $"User {User.Identity.Name} has accepted your invitation to moderate subverse v/{userInvitation.Subverse}."
            };
            var cmd = new SendMessageCommand(message);
            await cmd.Execute();

            //clear mod cache
            CacheHandler.Instance.Remove(CachingKey.SubverseModerators(userInvitation.Subverse));

            // delete the invitation from database
            _db.ModeratorInvitations.Remove(userInvitation);
            _db.SaveChanges();

            return RedirectToAction("SubverseSettings", "Subverses", new { subversetoshow = userInvitation.Subverse });
        }

        // GET: show add moderators view for selected subverse
        [Authorize]
        public ActionResult AddModerator(string subversetoshow)
        {
            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subversetoshow);
            if (subverseModel == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (!ModeratorPermission.HasPermission(User.Identity.Name, subversetoshow, Domain.Models.ModeratorAction.InviteMods))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subversetoshow;
            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/AddModerator.cshtml");
        }

        // POST: add a moderator to given subverse
        [Authorize]
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> AddModerator([Bind(Include = "ID,Subverse,Username,Power")] SubverseModerator subverseAdmin)
        {
            if (!ModelState.IsValid)
            {
                return View(subverseAdmin);
            }

            // check if caller can add mods, if not, deny posting
            if (!ModeratorPermission.HasPermission(User.Identity.Name, subverseAdmin.Subverse, Domain.Models.ModeratorAction.InviteMods))
            {
                return RedirectToAction("Index", "Home");
            }

            subverseAdmin.UserName = subverseAdmin.UserName.TrimSafe();
            Subverse subverseModel = null;

            //lots of premature retuns so wrap the common code
            var sendFailureResult = new Func<string, ActionResult>(errorMessage =>
            {
                ViewBag.SubverseModel = subverseModel;
                ViewBag.SubverseName = subverseAdmin.Subverse;
                ViewBag.SelectedSubverse = string.Empty;
                ModelState.AddModelError(string.Empty, errorMessage);
                return View("~/Views/Subverses/Admin/AddModerator.cshtml",
                new SubverseModeratorViewModel
                {
                    UserName = subverseAdmin.UserName,
                    Power = subverseAdmin.Power
                });
            });

            // prevent invites to the current moderator
            if (User.Identity.Name.Equals(subverseAdmin.UserName, StringComparison.OrdinalIgnoreCase))
            {
                return sendFailureResult("Can not add yourself as a moderator");
            }

            string originalRecipientUserName = UserHelper.OriginalUsername(subverseAdmin.UserName);
            // prevent invites to the current moderator
            if (String.IsNullOrEmpty(originalRecipientUserName))
            {
                return sendFailureResult("User can not be found");
            }

            // get model for selected subverse
            subverseModel = DataCache.Subverse.Retrieve(subverseAdmin.Subverse);
            if (subverseModel == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if ((subverseAdmin.Power < 1 || subverseAdmin.Power > 4) && subverseAdmin.Power != 99)
            {
                return sendFailureResult("Only powers levels 1 - 4 and 99 are supported currently");
            }

            //check current mod level and invite level and ensure they are a lower level
            var currentModLevel = ModeratorPermission.Level(User.Identity.Name, subverseModel.Name);
            if (subverseAdmin.Power <= (int)currentModLevel && currentModLevel != Domain.Models.ModeratorLevel.Owner)
            {
                return sendFailureResult("Sorry, but you can only add moderators that are a lower level than yourself");
            }

            int maximumOwnedSubs = Settings.MaximumOwnedSubs;

            // check if the user being added is not already a moderator of 10 subverses
            var currentlyModerating = _db.SubverseModerators.Where(a => a.UserName == originalRecipientUserName).ToList();

            SubverseModeratorViewModel tmpModel;
            if (currentlyModerating.Count <= maximumOwnedSubs)
            {
                // check that user is not already moderating given subverse
                var isAlreadyModerator = _db.SubverseModerators.FirstOrDefault(a => a.UserName == originalRecipientUserName && a.Subverse == subverseAdmin.Subverse);

                if (isAlreadyModerator == null)
                {
                    // check if this user is already invited
                    var userModeratorInvitations = _db.ModeratorInvitations.Where(i => i.Recipient.Equals(originalRecipientUserName, StringComparison.OrdinalIgnoreCase) && i.Subverse.Equals(subverseModel.Name, StringComparison.OrdinalIgnoreCase));
                    if (userModeratorInvitations.Any())
                    {
                        return sendFailureResult("Sorry, the user is already invited to moderate this subverse");
                    }

                    // send a new moderator invitation
                    ModeratorInvitation modInv = new ModeratorInvitation
                    {
                        CreatedBy = User.Identity.Name,
                        CreationDate = Repository.CurrentDate,
                        Recipient = originalRecipientUserName,
                        Subverse = subverseAdmin.Subverse,
                        Power = subverseAdmin.Power
                    };

                    _db.ModeratorInvitations.Add(modInv);
                    _db.SaveChanges();

                    int invitationId = modInv.ID;
                    var invitationBody = new StringBuilder();
                    invitationBody.Append("Hello,");
                    invitationBody.Append(Environment.NewLine);
                    invitationBody.Append($"@{User.Identity.Name} invited you to moderate v/" + subverseAdmin.Subverse + ".");
                    invitationBody.Append(Environment.NewLine);
                    invitationBody.Append(Environment.NewLine);
                    invitationBody.Append("Please visit the following link if you want to accept this invitation: " + "https://" + Request.ServerVariables["HTTP_HOST"] + "/acceptmodinvitation/" + invitationId);
                    invitationBody.Append(Environment.NewLine);
                    invitationBody.Append(Environment.NewLine);
                    invitationBody.Append("Thank you.");

                    var cmd = new SendMessageCommand(new Domain.Models.SendMessage()
                    {
                        Sender = $"v/{subverseAdmin.Subverse}",
                        Recipient = originalRecipientUserName,
                        Subject = $"v/{subverseAdmin.Subverse} moderator invitation",
                        Message = invitationBody.ToString()
                    }, true);
                    await cmd.Execute();

                    return RedirectToAction("SubverseModerators");
                }
                else
                {
                    return sendFailureResult("Sorry, the user is already moderating this subverse");
                }
            }
            else
            {
                return sendFailureResult("Sorry, the user is already moderating a maximum of " + maximumOwnedSubs + " subverses");
            }
        }
        // GET: show remove moderators view for selected subverse
        [Authorize]
        public ActionResult RemoveModerator(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var subModerator = _db.SubverseModerators.Find(id);

            if (subModerator == null)
            {
                return HttpNotFound();
            }

            if (!ModeratorPermission.HasPermission(User.Identity.Name, subModerator.Subverse, Domain.Models.ModeratorAction.RemoveMods))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SubverseName = subModerator.Subverse;
            return View("~/Views/Subverses/Admin/RemoveModerator.cshtml", subModerator);
        }

        // POST: remove a moderator from given subverse
        [Authorize]
        [HttpPost, ActionName("RemoveModerator")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveModerator(int id)
        {

            var cmd = new RemoveModeratorCommand(id, true);
            var response = await cmd.Execute();

            if (response.Success)
            {
                return RedirectToAction("SubverseModerators");
            }
            else
            {
                ModelState.AddModelError("", response.Message);
                if (response.Response.SubverseModerator != null)
                {
                    var model = new SubverseModerator()
                    {
                        ID = response.Response.SubverseModerator.ID,
                        Subverse = response.Response.SubverseModerator.Subverse,
                        UserName = response.Response.SubverseModerator.UserName,
                        Power = response.Response.SubverseModerator.Power
                    };
                    return View("~/Views/Subverses/Admin/RemoveModerator.cshtml", model);
                }
                else
                {
                    //bail
                    return RedirectToAction("SubverseModerators");
                }
            }
        }

        public async Task<ContentResult> Stylesheet(string subverse, bool cache = true, bool minimized = true)
        {
            var policy = (cache ? new CachePolicy(TimeSpan.FromMinutes(30)) : CachePolicy.None);
            var q = new QuerySubverseStylesheet(subverse, policy);

            var madStylesYo = await q.ExecuteAsync();

            return new ContentResult() {
                Content = (minimized ? madStylesYo.Minimized : madStylesYo.Raw),
                ContentType = "text/css"
            };
        }

        #endregion ADD/REMOVE MODERATORS LOGIC

        #region random subverse

        public string RandomSubverse(bool sfw)
        {
            // fetch a random subverse with minimum number of subscribers where last subverse activity was evident
            IQueryable<Subverse> subverse;
            if (sfw)
            {
                subverse = from subverses in
                               _db.Subverses
                                   .Where(s => s.SubscriberCount > 10 && !s.Name.Equals("all", StringComparison.OrdinalIgnoreCase) && s.LastSubmissionDate != null
                                   && !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(s.Name) select ubs.UserName).Contains(User.Identity.Name)
                                   && !s.IsAdult
                                   && !s.IsAdminDisabled.Value)
                           select subverses;
            }
            else
            {
                subverse = from subverses in
                               _db.Subverses
                                   .Where(s => s.SubscriberCount > 10 && !s.Name.Equals("all", StringComparison.OrdinalIgnoreCase)
                                               && s.LastSubmissionDate != null
                                               && !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(s.Name) select ubs.UserName).Contains(User.Identity.Name)
                                               && s.IsAdult
                                               && !s.IsAdminDisabled.Value)
                           select subverses;
            }

            var submissionCount = 0;
            Subverse randomSubverse;

            do
            {
                var count = subverse.Count(); // 1st round-trip
                var index = new Random().Next(count);

                randomSubverse = subverse.OrderBy(s => s.Name).Skip(index).FirstOrDefault(); // 2nd round-trip

                var submissions = _db.Submissions
                        .Where(x => x.Subverse == randomSubverse.Name && !x.IsDeleted)
                        .OrderByDescending(s => s.Rank)
                        .Take(50)
                        .ToList();

                if (submissions.Count > 9)
                {
                    submissionCount = submissions.Count;
                }
            } while (submissionCount == 0);

            return randomSubverse != null ? randomSubverse.Name : "all";
        }

        #endregion random subverse
    }
}
