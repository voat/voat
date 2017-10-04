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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Models.Input;
using Voat.Http.Filters;
using Voat.Models.ViewModels;
using Voat.UI.Utilities;
using Voat.Utilities;
using Voat.Utilities.Components;

namespace Voat.Controllers
{
    [Authorize]
    public class SubverseModerationController : BaseController
    {
        private readonly VoatOutOfRepositoryDataContextAccessor _db = new VoatOutOfRepositoryDataContextAccessor(CONSTANTS.CONNECTION_LIVE);

        private void SetNavigationViewModel(string subverseName)
        {
            ViewBag.NavigationViewModel = new Models.ViewModels.NavigationViewModel()
            {
                Description = "Moderation",
                Name = subverseName,
                MenuType = Models.ViewModels.MenuType.Moderator,
                BasePath = null,
                Sort = null
            };
        }

        // GET: settings
        [Authorize]
        public ActionResult Update(string subverse)
        {
            var subverseObject = DataCache.Subverse.Retrieve(subverse);

            if (subverseObject == null)
            {
                ViewBag.SelectedSubverse = "404";
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
            }

            if (!ModeratorPermission.HasPermission(User, subverse, Domain.Models.ModeratorAction.ModifySettings))
            {
                return RedirectToRoute(Models.ROUTE_NAMES.SUBVERSE_INDEX, new { subverse = subverse });
            }

            // map existing data to view model for editing and pass it to frontend
            // NOTE: we should look into a mapper which automatically maps these properties to corresponding fields to avoid tedious manual mapping
            var viewModel = new SubverseSettingsViewModel
            {
                Name = subverseObject.Name,
                Title = subverseObject.Title,
                Description = subverseObject.Description,
                SideBar = subverseObject.SideBar,
                //Stylesheet = subverseObject.Stylesheet,
                IsAdult = subverseObject.IsAdult,
                IsPrivate = subverseObject.IsPrivate,
                IsThumbnailEnabled = subverseObject.IsThumbnailEnabled,
                ExcludeSitewideBans = subverseObject.ExcludeSitewideBans,
                IsAuthorizedOnly = subverseObject.IsAuthorizedOnly,
                IsAnonymized = subverseObject.IsAnonymized,
                MinCCPForDownvote = subverseObject.MinCCPForDownvote,
                LastUpdateDate = subverseObject.LastUpdateDate
            };

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SubverseName = subverseObject.Name;
            SetNavigationViewModel(subverseObject.Name);

            return View("~/Views/Subverses/Admin/SubverseSettings.cshtml", viewModel);
        }

        // POST: Eddit a Subverse
        [HttpPost]
        [PreventSpam(30)]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> Update(SubverseSettingsViewModel updatedModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    SetNavigationViewModel(updatedModel.Name);
                    return View("~/Views/Subverses/Admin/SubverseSettings.cshtml", updatedModel);
                }
                var existingSubverse = _db.Subverse.FirstOrDefault(x => x.Name.ToUpper() == updatedModel.Name.ToUpper());

                // check if subverse exists before attempting to edit it
                if (existingSubverse != null)
                {
                    SetNavigationViewModel(existingSubverse.Name);

                    // check if user requesting edit is authorized to do so for current subverse
                    if (!ModeratorPermission.HasPermission(User, updatedModel.Name, Domain.Models.ModeratorAction.ModifySettings))
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

                    existingSubverse.Title = updatedModel.Title;
                    existingSubverse.Description = updatedModel.Description;
                    existingSubverse.SideBar = updatedModel.SideBar;

                    //if (updatedModel.Stylesheet != null)
                    //{
                    //    if (updatedModel.Stylesheet.Length < 50001)
                    //    {
                    //        existingSubverse.Stylesheet = updatedModel.Stylesheet;
                    //    }
                    //    else
                    //    {
                    //        ModelState.AddModelError(string.Empty, "Sorry, custom CSS limit is set to 50000 characters.");
                    //        return View("~/Views/Subverses/Admin/SubverseSettings.cshtml", updatedModel);
                    //    }
                    //}
                    //else
                    //{
                    //    existingSubverse.Stylesheet = updatedModel.Stylesheet;
                    //}

                    existingSubverse.IsAdult = updatedModel.IsAdult;
                  
                    existingSubverse.IsThumbnailEnabled = updatedModel.IsThumbnailEnabled;
                    existingSubverse.IsAuthorizedOnly = updatedModel.IsAuthorizedOnly;
                    existingSubverse.ExcludeSitewideBans = updatedModel.ExcludeSitewideBans;

                    //Only update if time lock has expired
                    if (existingSubverse.LastUpdateDate == null || (Repository.CurrentDate.Subtract(existingSubverse.LastUpdateDate.Value) > TimeSpan.FromHours(VoatSettings.Instance.SubverseUpdateTimeLockInHours)))
                    {
                        existingSubverse.MinCCPForDownvote = updatedModel.MinCCPForDownvote;
                        existingSubverse.IsPrivate = updatedModel.IsPrivate;
                    }

                    // these properties are currently not implemented but they can be saved and edited for future use
                    //existingSubverse.Type = updatedModel.Type;
                    //existingSubverse.SubmitLinkLabel = updatedModel.SubmitLinkLabel;
                    //existingSubverse.SubmitPostLabel = updatedModel.SubmitPostLabel;
                    //existingSubverse.SubmissionText = updatedModel.SubmissionText;
                    //existingSubverse.IsDefaultAllowed = updatedModel.IsDefaultAllowed;

                    //if (existingSubverse.IsAnonymized == true && updatedModel.IsAnonymized == false)
                    //{
                    //    ModelState.AddModelError(string.Empty, "Sorry, this subverse is permanently locked to anonymized mode.");
                    //    return View("~/Views/Subverses/Admin/SubverseSettings.cshtml", updatedModel);
                    //}

                    // only subverse owners should be able to convert a sub to anonymized mode
                    if (ModeratorPermission.IsLevel(User, updatedModel.Name, Domain.Models.ModeratorLevel.Owner))
                    {
                        existingSubverse.IsAnonymized = updatedModel.IsAnonymized;
                    }

                    existingSubverse.LastUpdateDate = Repository.CurrentDate;
                    await _db.SaveChangesAsync();

                    //purge new minified CSS
                    CacheHandler.Instance.Remove(CachingKey.SubverseStylesheet(existingSubverse.Name));

                    //purge subvere
                    CacheHandler.Instance.Remove(CachingKey.Subverse(existingSubverse.Name));

                    // go back to this subverse
                    return RedirectToRoute(Models.ROUTE_NAMES.SUBVERSE_INDEX, new { subverse = updatedModel.Name });

                    // user was not authorized to commit the changes, drop attempt
                }
                ModelState.AddModelError(string.Empty, "Sorry, The subverse you are trying to edit does not exist.");
                return View("~/Views/Subverses/Admin/SubverseSettings.cshtml", updatedModel);
            }
            catch (Exception ex)
            {
                EventLogger.Instance.Log(ex);
                ModelState.AddModelError(string.Empty, "Something bad happened.");
                return View("~/Views/Subverses/Admin/SubverseSettings.cshtml", updatedModel);
            }
        }

        // GET: subverse stylesheet editor
        [Authorize]
        public ActionResult SubverseStylesheetEditor(string subverse)
        {
            var subverseObject = DataCache.Subverse.Retrieve(subverse);

            if (subverseObject == null)
            {
                ViewBag.SelectedSubverse = "404";
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
            }
            if (!ModeratorPermission.HasPermission(User, subverse, Domain.Models.ModeratorAction.ModifyCSS))
            {
                return RedirectToAction("Index", "Home");
            }

            // map existing data to view model for editing and pass it to frontend
            var viewModel = new SubverseStylesheetViewModel
            {
                Name = subverseObject.Name,
                Stylesheet = subverseObject.Stylesheet
            };

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SubverseName = subverseObject.Name;
            SetNavigationViewModel(subverseObject.Name);

            return View("~/Views/Subverses/Admin/SubverseStylesheetEditor.cshtml", viewModel);
        }

        [HttpPost]
        //CORE_PORT: Not supported
        //[ValidateInput(false)]
        [PreventSpam(30)]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> SubverseStylesheetEditor(SubverseStylesheetViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    SetNavigationViewModel(model.Name);
                    return View("~/Views/Subverses/Admin/SubverseStylesheetEditor.cshtml");
                }
                var existingSubverse = _db.Subverse.FirstOrDefault(x => x.Name.ToUpper() == model.Name.ToUpper());

                // check if subverse exists before attempting to edit it
                if (existingSubverse != null)
                {
                    SetNavigationViewModel(model.Name);
                    // check if user requesting edit is authorized to do so for current subverse
                    // check that the user requesting to edit subverse settings is subverse owner!
                    if (!ModeratorPermission.HasPermission(User, existingSubverse.Name, Domain.Models.ModeratorAction.ModifyCSS))
                    {
                        return new EmptyResult();
                    }

                    if (!String.IsNullOrEmpty(model.Stylesheet))
                    {
                        if (model.Stylesheet.Length < 50001)
                        {
                            existingSubverse.Stylesheet = model.Stylesheet;
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Sorry, custom CSS limit is set to 50000 characters.");
                            return View("~/Views/Subverses/Admin/SubverseStylesheetEditor.cshtml");
                        }
                    }
                    else
                    {
                        existingSubverse.Stylesheet = model.Stylesheet;
                    }

                    await _db.SaveChangesAsync();

                    //purge new minified CSS
                    CacheHandler.Instance.Remove(CachingKey.SubverseStylesheet(existingSubverse.Name));
                    CacheHandler.Instance.Remove(CachingKey.Subverse(existingSubverse.Name));

                    // go back to this subverse
                    return RedirectToRoute(Models.ROUTE_NAMES.SUBVERSE_INDEX, new { subverse = model.Name });
                }

                ModelState.AddModelError(string.Empty, "Sorry, The subverse you are trying to edit does not exist.");
                return View("~/Views/Subverses/Admin/SubverseStylesheetEditor.cshtml", model);
            }
            catch (Exception ex)
            {
                EventLogger.Instance.Log(ex);
                ModelState.AddModelError(string.Empty, "Something bad happened.");
                return View("~/Views/Subverses/Admin/SubverseStylesheetEditor.cshtml", model);
            }
        }
        // GET: subverse moderators for selected subverse
        [Authorize]
        public ActionResult SubverseModerators(string subverse)
        {
            // get model for selected subverse
            var subverseObject = DataCache.Subverse.Retrieve(subverse);
            if (subverseObject == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!ModeratorPermission.HasPermission(User, subverse, Domain.Models.ModeratorAction.InviteMods))
            {
                return RedirectToAction("Index", "Home");
            }

            var subverseModerators = _db.SubverseModerator
                .Where(n => n.Subverse == subverse)
                .Take(20)
                .OrderBy(s => s.Power)
                .ToList();
            var moderatorInvitations = _db.ModeratorInvitation
                .Where(mi => mi.Subverse == subverse)
                .Take(20)
                .OrderBy(s => s.Power)
                .ToList();

            ViewBag.SubverseModel = subverseObject;
            ViewBag.SubverseName = subverse;
            ViewBag.SelectedSubverse = string.Empty;
            SetNavigationViewModel(subverseObject.Name);

            var model = new SubverseModeratorsViewModel() { Moderators = subverseModerators, Invitations = moderatorInvitations };

            return View("~/Views/Subverses/Admin/SubverseModerators.cshtml", model);
        }

        // GET: banned users for selected subverse
        [Authorize]
        public ActionResult SubverseBans(string subverse, int? page)
        {
            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }

            // get model for selected subverse
            var subverseObject = DataCache.Subverse.Retrieve(subverse);

            if (subverseObject == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!ModeratorPermission.HasPermission(User, subverse, Domain.Models.ModeratorAction.Banning))
            {
                return RedirectToAction("Index", "Home");
            }

            var subverseBans = _db.SubverseBan.Where(n => n.Subverse == subverse).OrderByDescending(s => s.CreationDate);
            var paginatedSubverseBans = new PaginatedList<SubverseBan>(subverseBans, page ?? 0, pageSize);

            ViewBag.SubverseModel = subverseObject;
            ViewBag.SubverseName = subverse;
            ViewBag.SelectedSubverse = string.Empty;
            SetNavigationViewModel(subverseObject.Name);

            return View("~/Views/Subverses/Admin/SubverseBans.cshtml", paginatedSubverseBans);
        }
        #region Banning
        // GET: show add ban view for selected subverse
        [Authorize]
        public ActionResult AddBan(string subverse)
        {
            // get model for selected subverse
            var subverseObject = DataCache.Subverse.Retrieve(subverse);

            if (subverseObject == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!ModeratorPermission.HasPermission(User, subverse, Domain.Models.ModeratorAction.Banning))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.SubverseModel = subverseObject;
            ViewBag.SubverseName = subverse;
            ViewBag.SelectedSubverse = string.Empty;
            SetNavigationViewModel(subverseObject.Name);

            return View("~/Views/Subverses/Admin/AddBan.cshtml");
        }

        // POST: add a user ban to given subverse
        [Authorize]
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> AddBan([Bind("Id,Subverse,UserName,Reason")] SubverseBan subverseBan)
        {
            if (!ModelState.IsValid)
            {
                return View(subverseBan);
            }
            //check perms
            if (!ModeratorPermission.HasPermission(User, subverseBan.Subverse, Domain.Models.ModeratorAction.Banning))
            {
                return RedirectToAction("Index", "Home");
            }

            var cmd = new SubverseBanCommand(subverseBan.UserName, subverseBan.Subverse, subverseBan.Reason, true).SetUserContext(User);
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
                SetNavigationViewModel(subverseBan.Subverse);

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
        public ActionResult RemoveBan(string subverse, int? id)
        {
            if (id == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
            }

            // check if caller is subverse owner, if not, deny listing
            if (!ModeratorPermission.HasPermission(User, subverse, Domain.Models.ModeratorAction.Banning))
            {
                return RedirectToAction("Index", "Home");
            }
            var subverseBan = _db.SubverseBan.Find(id);

            if (subverseBan == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SubverseName = subverseBan.Subverse;
            SetNavigationViewModel(subverseBan.Subverse);

            return View("~/Views/Subverses/Admin/RemoveBan.cshtml", subverseBan);
        }

        // POST: remove a ban from given subverse
        [Authorize]
        [HttpPost, ActionName("RemoveBan")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveBan(int id)
        {
            // get ban name for selected subverse
            var banToBeRemoved = await _db.SubverseBan.FindAsync(id);

            if (banToBeRemoved == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var cmd = new SubverseBanCommand(banToBeRemoved.UserName, banToBeRemoved.Subverse, null, false).SetUserContext(User);
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
                    SetNavigationViewModel(banToBeRemoved.Subverse);

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
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var moderatorInvitation = _db.ModeratorInvitation.Find(invitationId);

            if (moderatorInvitation == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (!ModeratorPermission.HasPermission(User, moderatorInvitation.Subverse, Domain.Models.ModeratorAction.InviteMods))
            {
                return RedirectToAction("SubverseModerators");
            }
            //make sure mods can't remove invites 
            var currentModLevel = ModeratorPermission.Level(User, moderatorInvitation.Subverse);
            if (moderatorInvitation.Power <= (int)currentModLevel && currentModLevel != Domain.Models.ModeratorLevel.Owner)
            {
                return RedirectToAction("SubverseModerators");
            }

            ViewBag.SubverseName = moderatorInvitation.Subverse;
            SetNavigationViewModel(moderatorInvitation.Subverse);

            return View("~/Views/Subverses/Admin/RecallModeratorInvitation.cshtml", moderatorInvitation);
        }

        // POST: remove a moderator invitation from given subverse
        [Authorize]
        [HttpPost, ActionName("RecallModeratorInvitation")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> RecallModeratorInvitation(int invitationId)
        {
            // get invitation to remove
            var invitationToBeRemoved = await _db.ModeratorInvitation.FindAsync(invitationId);
            if (invitationToBeRemoved == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // check if subverse exists
            var subverse = DataCache.Subverse.Retrieve(invitationToBeRemoved.Subverse);
            if (subverse == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // check if caller has clearance to remove a moderator invitation
            //if (!UserHelper.IsUserSubverseAdmin(User.Identity.Name, subverse.Name) || invitationToBeRemoved.Recipient == User.Identity.Name) return RedirectToAction("Index", "Home");
            if (!ModeratorPermission.HasPermission(User, subverse.Name, Domain.Models.ModeratorAction.InviteMods))
            {
                return RedirectToAction("Index", "Home");
            }
            //make sure mods can't remove invites 
            var currentModLevel = ModeratorPermission.Level(User, subverse.Name);
            if (invitationToBeRemoved.Power <= (int)currentModLevel && currentModLevel != Domain.Models.ModeratorLevel.Owner)
            {
                return RedirectToAction("SubverseModerators");
            }

            // execute invitation removal
            _db.ModeratorInvitation.Remove(invitationToBeRemoved);
            await _db.SaveChangesAsync();

            return RedirectToAction("SubverseModerators");
        }



        // GET: show resign as moderator view for selected subverse
        [Authorize]
        public ActionResult ResignAsModerator(string subverse)
        {
            if (subverse == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var subModerator = _db.SubverseModerator.FirstOrDefault(s => s.Subverse == subverse && s.UserName == User.Identity.Name);

            if (subModerator == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SubverseName = subModerator.Subverse;
            SetNavigationViewModel(subModerator.Subverse);

            return View("~/Views/Subverses/Admin/ResignAsModerator.cshtml", subModerator);
        }

        // POST: resign as moderator from given subverse
        [Authorize]
        [HttpPost]
        [ActionName("ResignAsModerator")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> ResignAsModeratorPost(string subverse)
        {
            // get moderator name for selected subverse
            var subModerator = _db.SubverseModerator.FirstOrDefault(s => s.Subverse == subverse && s.UserName == User.Identity.Name);

            if (subModerator == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var subverseObject = DataCache.Subverse.Retrieve(subModerator.Subverse);
            if (subverseObject == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // execute removal
            _db.SubverseModerator.Remove(subModerator);
            await _db.SaveChangesAsync();

            //clear mod cache
            CacheHandler.Instance.Remove(CachingKey.SubverseModerators(subverseObject.Name));

            return RedirectToRoute(Models.ROUTE_NAMES.SUBVERSE_INDEX, new { subverse = subverse });
        }



        // GET: show subverse flair settings view for selected subverse
        [Authorize]
        public ActionResult SubverseFlairSettings(string subverse)
        {
            // get model for selected subverse
            var subverseObject = DataCache.Subverse.Retrieve(subverse);

            if (subverseObject == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // check if caller is authorized for this sub, if not, deny listing
            if (!ModeratorPermission.HasPermission(User, subverse, Domain.Models.ModeratorAction.ModifyFlair))
            {
                return RedirectToAction("Index", "Home");
            }

            var subverseFlairsettings = _db.SubverseFlair
                .Where(n => n.Subverse == subverse)
                .Take(20)
                .OrderBy(s => s.ID)
                .ToList();

            ViewBag.SubverseModel = subverseObject;
            ViewBag.SubverseName = subverse;
            ViewBag.SelectedSubverse = string.Empty;
            SetNavigationViewModel(subverseObject.Name);

            return View("~/Views/Subverses/Admin/Flair/FlairSettings.cshtml", subverseFlairsettings);
        }
        
        #region ADD/REMOVE SUB FLAIR 

        // GET: show add link flair view for selected subverse
        [Authorize]
        public ActionResult AddLinkFlair(string subverse)
        {
            // get model for selected subverse
            var subverseObject = DataCache.Subverse.Retrieve(subverse);

            if (subverseObject == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //check perms
            if (!ModeratorPermission.HasPermission(User, subverse, Domain.Models.ModeratorAction.ModifyFlair))
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.SubverseModel = subverseObject;
            ViewBag.SubverseName = subverse;
            ViewBag.SelectedSubverse = string.Empty;
            SetNavigationViewModel(subverseObject.Name);

            return View("~/Views/Subverses/Admin/Flair/AddLinkFlair.cshtml");
        }

        // POST: add a link flair to given subverse
        [Authorize]
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(5)]
        public ActionResult AddLinkFlair(SubverseFlairInput subverseFlairSetting)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Subverses/Admin/Flair/AddLinkFlair.cshtml", subverseFlairSetting);
            }

            //check perms
            if (!ModeratorPermission.HasPermission(User, subverseFlairSetting.Subverse, Domain.Models.ModeratorAction.ModifyFlair))
            {
                return RedirectToAction("Index", "Home");
            }

            // get model for selected subverse
            var subverse = DataCache.Subverse.Retrieve(subverseFlairSetting.Subverse);
            if (subverse == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
            }
            var count = _db.SubverseFlair.Count(x => x.Subverse == subverseFlairSetting.Subverse);
            if (count >= 20)
            {
                ViewBag.SubverseModel = subverse;
                ViewBag.SubverseName = subverse.Name;
                ViewBag.SelectedSubverse = string.Empty;
                SetNavigationViewModel(subverse.Name);
                ModelState.AddModelError("", "Subverses are limited to 20 flairs");
                return View("~/Views/Subverses/Admin/Flair/AddLinkFlair.cshtml", subverseFlairSetting);
            }

            subverseFlairSetting.Subverse = subverse.Name;
            _db.SubverseFlair.Add(new SubverseFlair() {
                Label = subverseFlairSetting.Label,
                CssClass = subverseFlairSetting.CssClass,
                Subverse = subverseFlairSetting.Subverse
            });
            _db.SaveChanges();

            //clear cache
            CacheHandler.Instance.Remove(CachingKey.SubverseFlair(subverse.Name));

            return RedirectToAction("SubverseFlairSettings");
        }

        // GET: show remove link flair view for selected subverse
        [Authorize]
        public ActionResult RemoveLinkFlair(int? id)
        {
            if (id == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var subverseFlairSetting = _db.SubverseFlair.Find(id);

            if (subverseFlairSetting == null)
            {
                return RedirectToAction("NotFound", "Error");

            }

            ViewBag.SubverseName = subverseFlairSetting.Subverse;
            ViewBag.SelectedSubverse = string.Empty;
            SetNavigationViewModel(subverseFlairSetting.Subverse);

            return View("~/Views/Subverses/Admin/Flair/RemoveLinkFlair.cshtml", subverseFlairSetting);
        }

        // POST: remove a link flair from given subverse
        [Authorize]
        [HttpPost, ActionName("RemoveLinkFlair")]
        [VoatValidateAntiForgeryToken]
        [PreventSpam(5)]
        public async Task<ActionResult> RemoveLinkFlair(int id)
        {
            // get link flair for selected subverse
            var linkFlairToRemove = await _db.SubverseFlair.FindAsync(id);
            if (linkFlairToRemove == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var subverse = DataCache.Subverse.Retrieve(linkFlairToRemove.Subverse);
            if (subverse == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // check if caller has clearance to remove a link flair
            if (!ModeratorPermission.HasPermission(User, subverse.Name, Domain.Models.ModeratorAction.ModifyFlair))
            {
                return RedirectToAction("Index", "Home");
            }

            // execute removal
            var subverseFlairSetting = await _db.SubverseFlair.FindAsync(id);
            _db.SubverseFlair.Remove(subverseFlairSetting);
            await _db.SaveChangesAsync();
            //clear cache
            CacheHandler.Instance.Remove(CachingKey.SubverseFlair(subverse.Name));

            return RedirectToAction("SubverseFlairSettings");
        }
        #endregion ADD/REMOVE SUB FLAIR 

        #region ADD/REMOVE MODERATORS LOGIC

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> AcceptModeratorInvitation(int invitationId)
        {
            int maximumOwnedSubs = VoatSettings.Instance.MaximumOwnedSubs;

            //TODO: These errors are not friendly - please update to redirect or something
            // check if there is an invitation for this user with this id
            var userInvitation = _db.ModeratorInvitation.Find(invitationId);
            if (userInvitation == null)
            {
                return ErrorView(new ErrorViewModel() { Title = "Moderator Invite Not Found", Description = "The moderator invite is no longer valid", Footer = "Where did it go?" });
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // check if logged in user is actually the invited user
            if (!User.Identity.Name.IsEqual(userInvitation.Recipient))
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(HttpStatusCode.Unauthorized));
                //return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            // check if user is over modding limits
            var amountOfSubsUserModerates = _db.SubverseModerator.Where(s => s.UserName.ToLower() == User.Identity.Name.ToLower());
            if (amountOfSubsUserModerates.Any())
            {
                if (amountOfSubsUserModerates.Count() >= maximumOwnedSubs)
                {
                    return ErrorView(new ErrorViewModel() { Title = "Maximum Moderation Level Exceeded", Description = $"Sorry, you can not own or moderate more than {maximumOwnedSubs} subverses.", Footer = "That's too bad" });
                }
            }

            // check if subverse exists
            var subverse = _db.Subverse.FirstOrDefault(s => s.Name.ToLower() == userInvitation.Subverse.ToLower());
            if (subverse == null)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // check if user is already a moderator of this sub
            var userModerating = _db.SubverseModerator.Where(s => s.Subverse.ToLower() == userInvitation.Subverse.ToLower() && s.UserName.ToLower() == User.Identity.Name.ToLower());
            if (userModerating.Any())
            {
                _db.ModeratorInvitation.Remove(userInvitation);
                _db.SaveChanges();
                return ErrorView(new ErrorViewModel(){ Title = "You = Moderator * 2?",  Description = "You are currently already a moderator of this subverse", Footer = "How much power do you want?" });
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // add user as moderator as specified in invitation
            var subAdm = new SubverseModerator
            {
                Subverse = subverse.Name,
                UserName = UserHelper.OriginalUsername(userInvitation.Recipient),
                Power = userInvitation.Power,
                CreatedBy = UserHelper.OriginalUsername(userInvitation.CreatedBy),
                CreationDate = Repository.CurrentDate
            };

            _db.SubverseModerator.Add(subAdm);

            // notify sender that user has accepted the invitation
            var message = new Domain.Models.SendMessage()
            {
                Sender = $"v/{subverse.Name}",
                Subject = $"Moderator invitation for v/{subverse.Name} accepted",
                Recipient = userInvitation.CreatedBy,
                Message = $"User {User.Identity.Name} has accepted your invitation to moderate subverse v/{subverse.Name}."
            };
            var cmd = new SendMessageCommand(message).SetUserContext(User);
            await cmd.Execute();

            //clear mod cache
            CacheHandler.Instance.Remove(CachingKey.SubverseModerators(subverse.Name));

            // delete the invitation from database
            _db.ModeratorInvitation.Remove(userInvitation);
            _db.SaveChanges();

            return RedirectToAction("Update", "SubverseModeration", new { subverse = subverse.Name });
            //return Update(subverse.Name);
        }

        // GET: show add moderators view for selected subverse
        [Authorize]
        public ActionResult AddModerator(string subverse)
        {
            // get model for selected subverse
            var subverseObject = DataCache.Subverse.Retrieve(subverse);
            if (subverseObject == null)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!ModeratorPermission.HasPermission(User, subverse, Domain.Models.ModeratorAction.InviteMods))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.SubverseModel = subverseObject;
            ViewBag.SubverseName = subverse;
            ViewBag.SelectedSubverse = string.Empty;
            SetNavigationViewModel(subverseObject.Name);

            return View("~/Views/Subverses/Admin/AddModerator.cshtml");
        }

        // POST: add a moderator to given subverse
        [Authorize]
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> AddModerator([Bind("ID,Subverse,UserName,Power")] SubverseModerator subverseAdmin)
        {
            if (!ModelState.IsValid)
            {
                return View(subverseAdmin);
            }

            // check if caller can add mods, if not, deny posting
            if (!ModeratorPermission.HasPermission(User, subverseAdmin.Subverse, Domain.Models.ModeratorAction.InviteMods))
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
                SetNavigationViewModel(subverseAdmin.Subverse);

                return View("~/Views/Subverses/Admin/AddModerator.cshtml",
                new SubverseModeratorViewModel
                {
                    UserName = subverseAdmin.UserName,
                    Power = subverseAdmin.Power
                }
                );
            });

            // prevent invites to the current moderator
            if (User.Identity.Name.IsEqual(subverseAdmin.UserName))
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
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
            }

            if ((subverseAdmin.Power < 1 || subverseAdmin.Power > 4) && subverseAdmin.Power != 99)
            {
                return sendFailureResult("Only powers levels 1 - 4 and 99 are supported currently");
            }

            //check current mod level and invite level and ensure they are a lower level
            var currentModLevel = ModeratorPermission.Level(User, subverseModel.Name);
            if (subverseAdmin.Power <= (int)currentModLevel && currentModLevel != Domain.Models.ModeratorLevel.Owner)
            {
                return sendFailureResult("Sorry, but you can only add moderators that are a lower level than yourself");
            }

            int maximumOwnedSubs = VoatSettings.Instance.MaximumOwnedSubs;

            // check if the user being added is not already a moderator of 10 subverses
            var currentlyModerating = _db.SubverseModerator.Where(a => a.UserName == originalRecipientUserName).ToList();

            SubverseModeratorViewModel tmpModel;
            if (currentlyModerating.Count <= maximumOwnedSubs)
            {
                // check that user is not already moderating given subverse
                var isAlreadyModerator = _db.SubverseModerator.FirstOrDefault(a => a.UserName == originalRecipientUserName && a.Subverse == subverseAdmin.Subverse);

                if (isAlreadyModerator == null)
                {
                    // check if this user is already invited
                    var userModeratorInvitations = _db.ModeratorInvitation.Where(i => i.Recipient.ToLower() == originalRecipientUserName.ToLower() && i.Subverse.ToLower() == subverseModel.Name.ToLower());
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

                    _db.ModeratorInvitation.Add(modInv);
                    _db.SaveChanges();
                    int invitationId = modInv.ID;
                    var invitationBody = new StringBuilder();

                    //v/{subverse}/about/moderatorinvitations/accept/{invitationId}

                    string acceptInviteUrl = VoatUrlFormatter.BuildUrlPath(this.HttpContext, new PathOptions(true, true), $"/v/{subverseModel.Name}/about/moderatorinvitations/accept/{invitationId}");

                    invitationBody.Append("Hello,");
                    invitationBody.Append(Environment.NewLine);
                    invitationBody.Append($"@{User.Identity.Name} invited you to moderate v/" + subverseAdmin.Subverse + ".");
                    invitationBody.Append(Environment.NewLine);
                    invitationBody.Append(Environment.NewLine);
                    invitationBody.Append($"Please visit the following link if you want to accept this invitation: {acceptInviteUrl}");
                    invitationBody.Append(Environment.NewLine);
                    invitationBody.Append(Environment.NewLine);
                    invitationBody.Append("Thank you.");

                    var cmd = new SendMessageCommand(new Domain.Models.SendMessage()
                    {
                        Sender = $"v/{subverseAdmin.Subverse}",
                        Recipient = originalRecipientUserName,
                        Subject = $"v/{subverseAdmin.Subverse} moderator invitation",
                        Message = invitationBody.ToString()
                    }, true).SetUserContext(User);
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
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var subModerator = _db.SubverseModerator.Find(id);

            if (subModerator == null)
            {
                return RedirectToAction("NotFound","Error");
            }

            if (!ModeratorPermission.HasPermission(User, subModerator.Subverse, Domain.Models.ModeratorAction.RemoveMods))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SubverseName = subModerator.Subverse;
            SetNavigationViewModel(subModerator.Subverse);

            return View("~/Views/Subverses/Admin/RemoveModerator.cshtml", subModerator);
        }

        // POST: remove a moderator from given subverse
        [Authorize]
        [HttpPost, ActionName("RemoveModerator")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveModerator(int id)
        {

            var cmd = new RemoveModeratorByRecordIDCommand(id, true).SetUserContext(User);
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
                    SetNavigationViewModel(model.Subverse);
                    return View("~/Views/Subverses/Admin/RemoveModerator.cshtml", model);
                }
                else
                {
                    //bail
                    return RedirectToAction("SubverseModerators");
                }
            }
        }

        #endregion ADD/REMOVE MODERATORS LOGIC

       
    }
}
