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

namespace Voat.Controllers
{
    public class SubversesController : Controller
    {
        //IAmAGate: Move queries to read-only mirror
        private readonly voatEntities _db = new voatEntities(true);
        private int subverseCacheTimeInSeconds = 240;

        // GET: sidebar for selected subverse
        public ActionResult SidebarForSelectedSubverseComments(string selectedSubverse, bool showingComments,
            string name, DateTime? date, DateTime? lastEditDate, int? submissionId, int? likes, int? dislikes,
            bool anonymized, int? views, bool isDeleted)
        {

            //Can't cache as view is using model to query
            var subverse = _db.Subverses.Find(selectedSubverse);

            //don't return a sidebar since subverse doesn't exist or is a system subverse
            if (subverse == null) return new EmptyResult();

            // get subscriber count for selected subverse
            var subscriberCount = _db.SubverseSubscriptions.Count(r => r.Subverse.Equals(selectedSubverse, StringComparison.OrdinalIgnoreCase));

            ViewBag.SubscriberCount = subscriberCount;
            ViewBag.SelectedSubverse = selectedSubverse;

            if (!showingComments) return new EmptyResult();

            if (anonymized || subverse.IsAnonymized)
            {
                ViewBag.name = submissionId.ToString();
                ViewBag.anonymized = true;
            }
            else
            {
                if (isDeleted)
                {
                    ViewBag.name = "deleted";
                }
                else
                {
                    ViewBag.name = name;
                }
            }

            ViewBag.date = date;
            ViewBag.lastEditDate = lastEditDate;
            ViewBag.likes = likes;
            ViewBag.dislikes = dislikes;
            ViewBag.anonymized_mode = subverse.IsAnonymized;
            ViewBag.views = views;

            try
            {
                ViewBag.OnlineUsers = SessionHelper.ActiveSessionsForSubverse(selectedSubverse);
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
            if (subverse == null) return new EmptyResult();

            // get subscriber count for selected subverse
            var subscriberCount = _db.SubverseSubscriptions.Count(r => r.Subverse.Equals(selectedSubverse, StringComparison.OrdinalIgnoreCase));

            ViewBag.SubscriberCount = subscriberCount;
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

        // GET: stylesheet for selected subverse
        public ActionResult StylesheetForSelectedSubverse(string selectedSubverse)
        {
            var subverse = DataCache.Subverse.Retrieve(selectedSubverse);

            return Content(subverse != null ? subverse.Stylesheet : string.Empty);
        }

        // POST: Create a new Subverse
        // To protect from overposting attacks, enable the specific properties you want to bind to 
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [PreventSpam(DelayRequest = 300, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public async Task<ActionResult> CreateSubverse([Bind(Include = "Name, Title, Description, Type, Sidebar, CreationDate, Owner")] AddSubverse subverseTmpModel)
        {
            // abort if model state is invalid
            if (!ModelState.IsValid) return View();

            int minimumCcp = Settings.MinimumCcp;
            int maximumOwnedSubs = Settings.MaximumOwnedSubs;

            // verify recaptcha if user has less than minimum required CCP
            if (Karma.CommentKarma(User.Identity.Name) < minimumCcp)
            {
                // begin recaptcha check
                bool isCaptchaCodeValid = await ReCaptchaUtility.Validate(Request);

                if (!isCaptchaCodeValid)
                {
                    ModelState.AddModelError("", "Incorrect recaptcha answer.");

                    // TODO 
                    // SET PREVENT SPAM DELAY TO 0

                    return View();
                }
            }

            // only allow users with less than maximum allowed subverses to create a subverse
            var amountOfOwnedSubverses = _db.SubverseModerators
                .Where(s => s.UserName == User.Identity.Name && s.Power == 1)
                .ToList();
            if (amountOfOwnedSubverses.Count >= maximumOwnedSubs)
            {
                ModelState.AddModelError(string.Empty, "Sorry, you can not own more than " + maximumOwnedSubs + " subverses.");
                return View();
            }

            // check if subverse already exists
            if (DataCache.Subverse.Retrieve(subverseTmpModel.Name) != null)
            {
                ModelState.AddModelError(string.Empty, "Sorry, The subverse you are trying to create already exists, but you can try to claim it by submitting a takeover request to /v/subverserequest.");
                return View();
            }

            try
            {
                // setup default values and create the subverse
                var subverse = new Subverse
                {
                    Name = subverseTmpModel.Name,
                    Title = "/v/" + subverseTmpModel.Name,
                    Description = subverseTmpModel.Description,
                    SideBar = subverseTmpModel.Sidebar,
                    CreationDate = DateTime.Now,
                    Type = "link",
                    IsThumbnailEnabled = true,
                    IsAdult = false,
                    IsPrivate = false,
                    MinCCPForDownvote = 0,
                    IsAdminDisabled = false,
                    CreatedBy = User.Identity.Name
                };

                _db.Subverses.Add(subverse);
                await _db.SaveChangesAsync();

                // subscribe user to the newly created subverse
                UserHelper.SubscribeToSubverse(subverseTmpModel.Owner, subverse.Name);

                // register user as the owner of the newly created subverse
                var tmpSubverseAdmin = new SubverseModerator
                {
                    Subverse = subverseTmpModel.Name,
                    UserName = User.Identity.Name,
                    Power = 1
                };
                _db.SubverseModerators.Add(tmpSubverseAdmin);
                await _db.SaveChangesAsync();

                // go to newly created Subverse
                return RedirectToAction("SubverseIndex", "Subverses", new { subversetoshow = subverseTmpModel.Name });
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Something bad happened, please report this to /v/voatdev. Thank you.");
                return View();
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
                return View("~/Views/Errors/Subversenotfound.cshtml");
            }

            // check that the user requesting to edit subverse settings is subverse owner!
            var subAdmin =
                _db.SubverseModerators.FirstOrDefault(
                    x => x.Subverse == subversetoshow && x.UserName == User.Identity.Name && x.Power <= 2);

            if (subAdmin == null) return RedirectToAction("Index", "Home");
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
        // To protect from overposting attacks, enable the specific properties you want to bind to 
        [HttpPost]
        [PreventSpam(DelayRequest = 30, ErrorMessage = "Sorry, you are doing that too fast. Please try again in 30 seconds.")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubverseSettings(Subverse updatedModel)
        {
            try
            {
                if (!ModelState.IsValid) return View("~/Views/Subverses/Admin/SubverseSettings.cshtml");
                var existingSubverse = _db.Subverses.Find(updatedModel.Name);

                // check if subverse exists before attempting to edit it
                if (existingSubverse != null)
                {
                    // check if user requesting edit is authorized to do so for current subverse
                    if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, updatedModel.Name))
                    {
                        return new EmptyResult();
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
                    if (UserHelper.IsUserSubverseAdmin(User.Identity.Name, updatedModel.Name))
                    {
                        existingSubverse.IsAnonymized = updatedModel.IsAnonymized;
                    }

                    await _db.SaveChangesAsync();
                    DataCache.Subverse.Remove(existingSubverse.Name);

                    // go back to this subverse
                    return RedirectToAction("SubverseIndex", "Subverses", new { subversetoshow = updatedModel.Name });

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
                return View("~/Views/Errors/Subversenotfound.cshtml");
            }

            // check that the user requesting to edit subverse settings is subverse owner!
            var subAdmin = _db.SubverseModerators.FirstOrDefault(x => x.Subverse == subversetoshow && x.UserName == User.Identity.Name && x.Power <= 2);

            if (subAdmin == null) return RedirectToAction("Index", "Home");

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
        [PreventSpam(DelayRequest = 30, ErrorMessage = "Sorry, you are doing that too fast. Please try again in 30 seconds.")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubverseStylesheetEditor(Subverse updatedModel)
        {
            try
            {
                if (!ModelState.IsValid) return View("~/Views/Subverses/Admin/SubverseSettings.cshtml");
                var existingSubverse = _db.Subverses.Find(updatedModel.Name);

                // check if subverse exists before attempting to edit it
                if (existingSubverse != null)
                {
                    // check if user requesting edit is authorized to do so for current subverse
                    // check that the user requesting to edit subverse settings is subverse owner!
                    var subAdmin = _db.SubverseModerators.FirstOrDefault(x => x.Subverse == updatedModel.Name && x.UserName == User.Identity.Name && x.Power <= 2);

                    // user was not authorized to commit the changes, drop attempt
                    if (subAdmin == null) return new EmptyResult();

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

                    await _db.SaveChangesAsync();
                    DataCache.Subverse.Remove(existingSubverse.Name);

                    // go back to this subverse
                    return RedirectToAction("SubverseIndex", "Subverses", new { subversetoshow = updatedModel.Name });
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

        // GET: show a subverse index
        public ActionResult SubverseIndex(int? page, string subversetoshow, bool? previewMode)
        {
            ViewBag.previewMode = previewMode ?? false;

            const string cookieName = "NSFWEnabled";
            int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            if (subversetoshow == null)
            {
                return View("~/Views/Errors/Subversenotfound.cshtml");
            }

            // register a new session for this subverse
            try
            {
                var currentSubverse = (string)RouteData.Values["subversetoshow"];

                // register a new session for this subverse
                string clientIpAddress = String.Empty;
                if (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
                {
                    clientIpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                }
                else if (Request.UserHostAddress.Length != 0)
                {
                    clientIpAddress = Request.UserHostAddress;
                }
                string ipHash = IpHash.CreateHash(clientIpAddress);
                SessionHelper.Add(currentSubverse, ipHash);

                ViewBag.OnlineUsers = SessionHelper.ActiveSessionsForSubverse(currentSubverse);
            }
            catch (Exception)
            {
                ViewBag.OnlineUsers = -1;
            }

            try
            {
                if (!subversetoshow.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    // check if subverse exists, if not, send to a page not found error

                    //Can't use cached, view using to query db
                    var subverse = _db.Subverses.Find(subversetoshow);

                    if (subverse == null)
                    {
                        ViewBag.SelectedSubverse = "404";
                        return View("~/Views/Errors/Subversenotfound.cshtml");
                    }
                    //HACK: Disable subverse
                    if (subverse.IsAdminDisabled.HasValue && subverse.IsAdminDisabled.Value)
                    {
                        ViewBag.Subverse = subverse.Name;
                        return View("~/Views/Errors/SubverseDisabled.cshtml");
                    }

                    ViewBag.SelectedSubverse = subverse.Name;
                    ViewBag.Title = subverse.Description;

                    //IAmAGate: Perf mods for caching
                    string cacheKey = String.Format("subverse.{0}.page.{1}.sort.{2}", subversetoshow, pageNumber, "rank").ToLower();
                    Tuple<IList<Submission>, int> cacheData = (Tuple<IList<Submission>, int>)CacheHandler.Retrieve(cacheKey);

                    if (cacheData == null)
                    {
                        var getDataFunc = new Func<object>(() =>
                        {
                            using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                            {
                                var x = SubmissionsFromASubverseByRank(subversetoshow, db);
                                int count = x.Count();
                                List<Submission> content = x.Skip(pageNumber * pageSize).Take(pageSize).ToList();
                                return new Tuple<IList<Submission>, int>(content, count);
                            }
                        });

                        cacheData = (Tuple<IList<Submission>, int>)CacheHandler.Register(cacheKey, getDataFunc, TimeSpan.FromSeconds(subverseCacheTimeInSeconds), (pageNumber < 3 ? 10 : 1));
                    }

                    PaginatedList<Submission> paginatedSubmissions = new PaginatedList<Submission>(cacheData.Item1, pageNumber, pageSize, cacheData.Item2);

                    // check if subverse is rated adult, show a NSFW warning page before entering
                    if (!subverse.IsAdult) return View(paginatedSubmissions);

                    // check if user wants to see NSFW content by reading user preference
                    if (User.Identity.IsAuthenticated)
                    {
                        if (UserHelper.AdultContentEnabled(User.Identity.Name))
                        {
                            return View(paginatedSubmissions);
                        }

                        // display a view explaining that account preference is set to NO NSFW and why this subverse can not be shown
                        return RedirectToAction("AdultContentFiltered", "Subverses", new { destination = subverse.Name });
                    }

                    // check if user wants to see NSFW content by reading NSFW cookie
                    if (!ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
                    {
                        return RedirectToAction("AdultContentWarning", "Subverses", new { destination = subverse.Name, nsfwok = false });
                    }
                    return View(paginatedSubmissions);
                }

                // selected subverse is ALL, show submissions from all subverses, sorted by rank
                ViewBag.SelectedSubverse = "all";
                ViewBag.Title = "all subverses";

                PaginatedList<Submission> paginatedSfwSubmissions;

                // check if user wants to see NSFW content by reading user preference
                if (User.Identity.IsAuthenticated)
                {
                    if (UserHelper.AdultContentEnabled(User.Identity.Name))
                    {
                        var paginatedSubmissionsFromAllSubverses = new PaginatedList<Submission>(SubmissionsFromAllSubversesByRank(), page ?? 0, pageSize);
                        return View(paginatedSubmissionsFromAllSubverses);
                    }

                    // return only sfw submissions 
                    paginatedSfwSubmissions = new PaginatedList<Submission>(SfwSubmissionsFromAllSubversesByRank(_db), page ?? 0, pageSize);
                    return View(paginatedSfwSubmissions);
                }

                // check if user wants to see NSFW content by reading NSFW cookie
                if (ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
                {
                    var paginatedSubmissionsFromAllSubverses = new PaginatedList<Submission>(SubmissionsFromAllSubversesByRank(), page ?? 0, pageSize);
                    return View(paginatedSubmissionsFromAllSubverses);
                }


                //NEW LOGIC
                //IAmAGate: Perf mods for caching
                string cacheKeyAll = String.Format("subverse.{0}.page.{1}.sort.{2}.sfw", "all", pageNumber, "rank").ToLower();
                Tuple<IList<Submission>, int> cacheDataAll = (Tuple<IList<Submission>, int>)CacheHandler.Retrieve(cacheKeyAll);

                if (cacheDataAll == null)
                {
                    var getDataFunc = new Func<object>(() =>
                    {
                        using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                        {
                            var x = SfwSubmissionsFromAllSubversesByRank(db);
                            int count = 50000;
                            List<Submission> content = x.Skip(pageNumber * pageSize).Take(pageSize).ToList();
                            return new Tuple<IList<Submission>, int>(content, count);
                        }
                    });
                    cacheDataAll = (Tuple<IList<Submission>, int>)CacheHandler.Register(cacheKeyAll, getDataFunc, TimeSpan.FromSeconds(subverseCacheTimeInSeconds), (pageNumber > 2) ? 5 : 0);
                }
                paginatedSfwSubmissions = new PaginatedList<Submission>(cacheDataAll.Item1, pageNumber, pageSize, cacheDataAll.Item2);
                return View(paginatedSfwSubmissions);
            }
            catch (Exception)
            {
                return View("~/Views/Errors/DbNotResponding.cshtml");
            }
        }

        public ActionResult SortedSubverseFrontpage(int? page, string subversetoshow, string sortingmode, string time)
        {
            // sortingmode: new, contraversial, hot, etc
            ViewBag.SortingMode = sortingmode;
            ViewBag.Time = time;
            ViewBag.SelectedSubverse = subversetoshow;
            ViewBag.Title = subversetoshow;

            if (!sortingmode.Equals("new") && !sortingmode.Equals("top")) return RedirectToAction("Index", "Home");

            int pageNumber = (page ?? 0);
            if (pageNumber < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // register a new session for this subverse                
            try
            {
                var currentSubverse = (string)RouteData.Values["subversetoshow"];

                // register a new session for this subverse
                string clientIpAddress = String.Empty;
                if (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
                {
                    clientIpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                }
                else if (Request.UserHostAddress.Length != 0)
                {
                    clientIpAddress = Request.UserHostAddress;
                }
                string ipHash = IpHash.CreateHash(clientIpAddress);
                SessionHelper.Add(currentSubverse, ipHash);

                ViewBag.OnlineUsers = SessionHelper.ActiveSessionsForSubverse(currentSubverse);
            }
            catch (Exception)
            {
                ViewBag.OnlineUsers = -1;
            }

            if (!subversetoshow.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                return HandleSortedSubverse(page, subversetoshow, sortingmode, time);
            }

            // selected subverse is ALL, show submissions from all subverses, sorted by date
            return HandleSortedSubverseAll(page, sortingmode, time);
        }

        // GET: show a list of subverses by number of subscribers
        public ActionResult Subverses(int? page)
        {
            ViewBag.SelectedSubverse = "subverses";
            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            try
            {
                // order by subscriber count (popularity)
                var subverses = _db.Subverses.OrderByDescending(s => s.SubscriberCount);

                var paginatedSubverses = new PaginatedList<Subverse>(subverses, page ?? 0, pageSize);

                return View(paginatedSubverses);
            }
            catch (Exception)
            {
                return View("~/Views/Errors/DbNotResponding.cshtml");
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
                return View("~/Views/Errors/Error_404.cshtml");
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

            if (subverse == null) return new EmptyResult();
            // get subscriber count for selected subverse
            var subscriberCount = _db.SubverseSubscriptions.Count(r => r.Subverse.Equals(selectedSubverse, StringComparison.OrdinalIgnoreCase));

            ViewBag.SubscriberCount = subscriberCount;
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
                return View("~/Views/Errors/Error_404.cshtml");
            }

            var subverses = _db.Subverses.Where(s => s.Description != null && s.SideBar != null).OrderByDescending(s => s.CreationDate);

            var paginatedNewestSubverses = new PaginatedList<Subverse>(subverses, page ?? 0, pageSize);

            return View("~/Views/Subverses/Subverses.cshtml", paginatedNewestSubverses);
        }

        // show subverses ordered by last received submission
        public ViewResult ActiveSubverses(int? page)
        {
            ViewBag.SelectedSubverse = "subverses";
            ViewBag.SortingMode = "active";

            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            var subverses = _db.Subverses
                .Where(s => s.Description != null && s.SideBar != null && s.LastSubmissionDate != null)
                .OrderByDescending(s => s.LastSubmissionDate);

            var paginatedActiveSubverses = new PaginatedList<Subverse>(subverses, page ?? 0, pageSize);

            return View("~/Views/Subverses/Subverses.cshtml", paginatedActiveSubverses);
        }

        [OutputCache(Duration = 3600, VaryByParam = "none")]
        public ActionResult Subversenotfound()
        {
            ViewBag.SelectedSubverse = "404";
            return View("~/Views/Errors/Subversenotfound.cshtml");
        }

        public ActionResult AdultContentFiltered(string destination)
        {
            ViewBag.SelectedSubverse = destination;
            return View("~/Views/Subverses/AdultContentFiltered.cshtml");
        }

        public ActionResult AdultContentWarning(string destination, bool? nsfwok)
        {
            ViewBag.SelectedSubverse = String.Empty;

            if (destination == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (nsfwok != null && nsfwok == true)
            {
                // setup nswf cookie
                HttpCookie hc = new HttpCookie("NSFWEnabled", "1");
                hc.Expires = DateTime.Now.AddYears(1);
                System.Web.HttpContext.Current.Response.Cookies.Add(hc);

                // redirect to destination subverse
                return RedirectToAction("SubverseIndex", "Subverses", new { subversetoshow = destination });
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
                return RedirectToAction("SubverseIndex", "Subverses", new { subversetoshow = randomSubverse });
            }
            catch (Exception)
            {
                return View("~/Views/Errors/DbNotResponding.cshtml");
            }
        }

        // GET: fetch a random NSFW subbverse with x subscribers and x submissions
        public ActionResult RandomNsfw()
        {
            try
            {
                string randomSubverse = RandomSubverse(false);
                return RedirectToAction("SubverseIndex", "Subverses", new { subversetoshow = randomSubverse });
            }
            catch (Exception)
            {
                return View("~/Views/Errors/DbNotResponding.cshtml");
            }
        }

        // GET: subverse moderators for selected subverse
        [Authorize]
        public ActionResult SubverseModerators(string subversetoshow)
        {
            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subversetoshow);
            if (subverseModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // check if caller is subverse owner, if not, deny listing
            if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, subversetoshow))
                return RedirectToAction("Index", "Home");
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
            if (subverseModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // check if caller is subverse owner, if not, deny listing
            if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, subversetoshow)) return RedirectToAction("Index", "Home");

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
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subversetoshow);

            if (subverseModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // check if caller is authorized, if not, deny listing
            if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, subversetoshow)) return RedirectToAction("Index", "Home");

            var subverseBans = _db.SubverseBans.Where(n => n.Subverse == subversetoshow).OrderByDescending(s => s.CreationDate);
            var paginatedSubverseBans = new PaginatedList<SubverseBan>(subverseBans, page ?? 0, pageSize);

            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subversetoshow;

            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/SubverseBans.cshtml", paginatedSubverseBans);
        }

        // GET: show add moderators view for selected subverse
        [Authorize]
        public ActionResult AddModerator(string subversetoshow)
        {
            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subversetoshow);
            if (subverseModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // check if caller is subverse owner, if not, deny listing
            if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, subversetoshow)) return RedirectToAction("Index", "Home");

            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subversetoshow;
            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/AddModerator.cshtml");
        }

        // GET: show add ban view for selected subverse
        [Authorize]
        public ActionResult AddBan(string subversetoshow)
        {
            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subversetoshow);

            if (subverseModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // check if caller is subverse owner, if not, deny listing
            if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, subversetoshow)) return RedirectToAction("Index", "Home");

            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subversetoshow;
            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/AddBan.cshtml");
        }

        // POST: add a moderator to given subverse
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddModerator([Bind(Include = "ID,Subverse,Username,Power")] SubverseModerator subverseAdmin)
        {
            if (!ModelState.IsValid) return View(subverseAdmin);

            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subverseAdmin.Subverse);
            if (subverseModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            int maximumOwnedSubs = Settings.MaximumOwnedSubs;

            // check if the user being added is not already a moderator of 10 subverses
            var currentlyModerating = _db.SubverseModerators.Where(a => a.UserName == subverseAdmin.UserName).ToList();

            SubverseModeratorViewModel tmpModel;
            if (currentlyModerating.Count <= maximumOwnedSubs)
            {
                // check if caller is subverse owner, if not, deny posting
                if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, subverseAdmin.Subverse)) return RedirectToAction("Index", "Home");

                // check that user is not already moderating given subverse
                var isAlreadyModerator = _db.SubverseModerators.FirstOrDefault(a => a.UserName == subverseAdmin.UserName && a.Subverse == subverseAdmin.Subverse);

                if (isAlreadyModerator == null)
                {
                    // check if this user is already invited
                    var userModeratorInvitations = _db.ModeratorInvitations.Where(i => i.Recipient.Equals(subverseAdmin.UserName, StringComparison.OrdinalIgnoreCase) && i.Subverse.Equals(subverseModel.Name, StringComparison.OrdinalIgnoreCase));
                    if (userModeratorInvitations.Any())
                    {
                        ModelState.AddModelError(string.Empty, "Sorry, the user is already invited to moderate this subverse.");
                        ViewBag.subversetoshow = subverseAdmin.Subverse;
                        return View("Admin/AddModerator");
                    }

                    // send a new moderator invitation
                    ModeratorInvitation modInv = new ModeratorInvitation
                    {
                        CreatedBy = User.Identity.Name,
                        CreationDate = DateTime.Now,
                        Recipient = subverseAdmin.UserName,
                        Subverse = subverseAdmin.Subverse,
                        Power = subverseAdmin.Power
                    };

                    _db.ModeratorInvitations.Add(modInv);
                    _db.SaveChanges();

                    int invitationId = modInv.ID;
                    var invitationBody = new StringBuilder();
                    invitationBody.Append("Hello,");
                    invitationBody.Append(Environment.NewLine);
                    invitationBody.Append("You are invited to moderate /v/" + subverseAdmin.Subverse + ".");
                    invitationBody.Append(Environment.NewLine);
                    invitationBody.Append(Environment.NewLine);
                    invitationBody.Append("Please visit the following link if you want to accept this invitation: " + "https://" + Request.ServerVariables["HTTP_HOST"] + "/acceptmodinvitation/" + invitationId);
                    invitationBody.Append(Environment.NewLine);
                    invitationBody.Append(Environment.NewLine);
                    invitationBody.Append("Thank you.");

                    MesssagingUtility.SendPrivateMessage(User.Identity.Name, subverseAdmin.UserName, "/v/" + subverseAdmin.Subverse + " moderator invitation", invitationBody.ToString());

                    return RedirectToAction("SubverseModerators");
                }

                ModelState.AddModelError(string.Empty, "Sorry, the user is already moderating this subverse.");
                tmpModel = new SubverseModeratorViewModel
                {
                    UserName = subverseAdmin.UserName,
                    Power = subverseAdmin.Power
                };

                ViewBag.SubverseModel = subverseModel;
                ViewBag.SubverseName = subverseAdmin.Subverse;
                ViewBag.SelectedSubverse = string.Empty;
                return View("~/Views/Subverses/Admin/AddModerator.cshtml", tmpModel);
            }

            ModelState.AddModelError(string.Empty, "Sorry, the user is already moderating a maximum of " + maximumOwnedSubs + " subverses.");
            tmpModel = new SubverseModeratorViewModel
            {
                UserName = subverseAdmin.UserName,
                Power = subverseAdmin.Power
            };

            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subverseAdmin.Subverse;
            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/AddModerator.cshtml", tmpModel);
        }

        [HttpGet]
        [Authorize]
        public ActionResult AcceptModInvitation(int invitationId)
        {
            int maximumOwnedSubs = Settings.MaximumOwnedSubs;

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
                Subverse = userInvitation.Subverse,
                UserName = userInvitation.Recipient,
                Power = userInvitation.Power,
                CreatedBy = userInvitation.CreatedBy,
                CreationDate = DateTime.Now
            };

            _db.SubverseModerators.Add(subAdm);

            // notify sender that user has accepted the invitation
            StringBuilder confirmation = new StringBuilder();
            confirmation.Append("User " + User.Identity.Name + " has accepted your invitation to moderate subverse /v/" + userInvitation.Subverse + ".");
            confirmation.AppendLine();
            MesssagingUtility.SendPrivateMessage("Voat", userInvitation.CreatedBy, "Moderator invitation for " + userInvitation.Subverse + " accepted", confirmation.ToString());

            // delete the invitation from database
            _db.ModeratorInvitations.Remove(userInvitation);
            _db.SaveChanges();

            return RedirectToAction("SubverseSettings", "Subverses", new { subversetoshow = userInvitation.Subverse });
        }

        // POST: add a user ban to given subverse
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddBan([Bind(Include = "Id,Subverse,UserName,Reason")] SubverseBan subverseBan)
        {
            if (!ModelState.IsValid) return View(subverseBan);

            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subverseBan.Subverse);

            if (subverseModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // check if caller is subverse owner, if not, deny posting
            if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, subverseBan.Subverse)) return RedirectToAction("Index", "Home");

            // check that user is not already banned in given subverse
            var isAlreadyBanned = _db.SubverseBans.FirstOrDefault(a => a.UserName == subverseBan.UserName && a.Subverse == subverseBan.Subverse);

            if (isAlreadyBanned == null)
            {
                subverseBan.Subverse = subverseModel.Name;
                subverseBan.CreatedBy = User.Identity.Name;
                subverseBan.CreationDate = DateTime.Now;
                _db.SubverseBans.Add(subverseBan);
                _db.SaveChanges();

                return RedirectToAction("SubverseBans");
            }

            ModelState.AddModelError(string.Empty, "Sorry, the user is already banned from this subverse.");
            var tmpModel = new SubverseBanViewModel
            {
                UserName = subverseBan.UserName
            };

            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subverseBan.Subverse;
            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/AddBan.cshtml", tmpModel);
        }

        // GET: show remove moderators view for selected subverse
        [Authorize]
        public ActionResult RemoveModerator(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var subverseAdmin = _db.SubverseModerators.Find(id);

            if (subverseAdmin == null)
            {
                return HttpNotFound();
            }

            // check if caller has clearance to access this area
            if (!UserHelper.IsUserSubverseAdmin(User.Identity.Name, subverseAdmin.Subverse)) return RedirectToAction("Index", "Home");

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SubverseName = subverseAdmin.Subverse;
            return View("~/Views/Subverses/Admin/RemoveModerator.cshtml", subverseAdmin);
        }

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

            ViewBag.SubverseName = moderatorInvitation.Subverse;
            return View("~/Views/Subverses/Admin/RecallModeratorInvitation.cshtml", moderatorInvitation);
        }

        // POST: remove a moderator invitation from given subverse
        [Authorize]
        [HttpPost, ActionName("RecallModeratorInvitation")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RecallModeratorInvitation(int invitationId)
        {
            // get invitation to remove
            var invitationToBeRemoved = await _db.ModeratorInvitations.FindAsync(invitationId);
            if (invitationToBeRemoved == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // check if subverse exists
            var subverse = DataCache.Subverse.Retrieve(invitationToBeRemoved.Subverse);
            if (subverse == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // check if caller has clearance to remove a moderator invitation
            if (!UserHelper.IsUserSubverseAdmin(User.Identity.Name, subverse.Name) || invitationToBeRemoved.Recipient == User.Identity.Name) return RedirectToAction("Index", "Home");

            // execute invitation removal
            _db.ModeratorInvitations.Remove(invitationToBeRemoved);
            await _db.SaveChangesAsync();
            return RedirectToAction("SubverseModerators");
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
            if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, subversetoshow)) return RedirectToAction("Index", "Home");

            var subverseBan = _db.SubverseBans.Find(id);

            if (subverseBan == null)
            {
                return HttpNotFound();
            }

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SubverseName = subverseBan.Subverse;
            return View("~/Views/Subverses/Admin/RemoveBan.cshtml", subverseBan);
        }

        // GET: show resign as moderator view for selected subverse
        [Authorize]
        public ActionResult ResignAsModerator(string subversetoresignfrom)
        {
            if (subversetoresignfrom == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var subverseAdmin = _db.SubverseModerators.FirstOrDefault(s => s.Subverse == subversetoresignfrom && s.UserName == User.Identity.Name && s.Power > 1);

            if (subverseAdmin == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SubverseName = subverseAdmin.Subverse;

            return View("~/Views/Subverses/Admin/ResignAsModerator.cshtml", subverseAdmin);
        }

        // POST: resign as moderator from given subverse
        [Authorize]
        [HttpPost]
        [ActionName("ResignAsModerator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResignAsModeratorPost(string subversetoresignfrom)
        {
            // get moderator name for selected subverse
            var moderatorToBeRemoved = _db.SubverseModerators.FirstOrDefault(s => s.Subverse == subversetoresignfrom && s.UserName == User.Identity.Name && s.Power != 1);

            if (moderatorToBeRemoved == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var subverse = DataCache.Subverse.Retrieve(moderatorToBeRemoved.Subverse);
            if (subverse == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // execute removal                    
            _db.SubverseModerators.Remove(moderatorToBeRemoved);
            await _db.SaveChangesAsync();
            return RedirectToAction("SubverseIndex", "Subverses", new { subversetoshow = subversetoresignfrom });
        }

        // POST: remove a moderator from given subverse
        [Authorize]
        [HttpPost, ActionName("RemoveModerator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveModerator(int id)
        {
            // get moderator name for selected subverse
            var moderatorToBeRemoved = await _db.SubverseModerators.FindAsync(id);
            if (moderatorToBeRemoved == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var subverse = DataCache.Subverse.Retrieve(moderatorToBeRemoved.Subverse);
            if (subverse == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // check if caller has clearance to remove a moderator
            if (!UserHelper.IsUserSubverseAdmin(User.Identity.Name, subverse.Name) || moderatorToBeRemoved.UserName == User.Identity.Name) return RedirectToAction("Index", "Home");

            // execute removal
            _db.SubverseModerators.Remove(moderatorToBeRemoved);
            await _db.SaveChangesAsync();
            return RedirectToAction("SubverseModerators");
        }

        // POST: remove a ban from given subverse
        [Authorize]
        [HttpPost, ActionName("RemoveBan")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveBan(int id)
        {
            // get ban name for selected subverse
            var banToBeRemoved = await _db.SubverseBans.FindAsync(id);

            if (banToBeRemoved == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var subverse = DataCache.Subverse.Retrieve(banToBeRemoved.Subverse);
            if (subverse == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // check if caller has clearance to remove a ban
            if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, subverse.Name)) return RedirectToAction("Index", "Home");

            // execute removal
            _db.SubverseBans.Remove(banToBeRemoved);
            await _db.SaveChangesAsync();
            return RedirectToAction("SubverseBans");
        }

        // GET: show subverse flair settings view for selected subverse
        [Authorize]
        public ActionResult SubverseFlairSettings(string subversetoshow)
        {
            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subversetoshow);

            if (subverseModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // check if caller is authorized for this sub, if not, deny listing
            if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, subversetoshow)) return RedirectToAction("Index", "Home");
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

            if (subverseModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // check if caller is authorized for this sub, if not, deny listing
            if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, subversetoshow)) return RedirectToAction("Index", "Home");
            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subversetoshow;
            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/Flair/AddLinkFlair.cshtml");
        }

        // POST: add a link flair to given subverse
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddLinkFlair([Bind(Include = "Id,Subverse,Label,CssClass")] SubverseFlair subverseFlairSetting)
        {
            if (!ModelState.IsValid) return View(subverseFlairSetting);
            // get model for selected subverse
            var subverseModel = DataCache.Subverse.Retrieve(subverseFlairSetting.Subverse);
            if (subverseModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // check if caller is subverse owner, if not, deny posting
            if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, subverseFlairSetting.Subverse)) return RedirectToAction("Index", "Home");
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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLinkFlair(int id)
        {
            // get link flair for selected subverse
            var linkFlairToRemove = await _db.SubverseFlairs.FindAsync(id);
            if (linkFlairToRemove == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var subverse = DataCache.Subverse.Retrieve(linkFlairToRemove.Subverse);
            if (subverse == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // check if caller has clearance to remove a link flair
            if (!UserHelper.IsUserSubverseModerator(User.Identity.Name, subverse.Name)) return RedirectToAction("Index", "Home");
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
            // get 10 administration members for a subverse
            var subverseAdministration =
                _db.SubverseModerators
                .Where(n => n.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .ToList()
                .OrderBy(s => s.UserName);

            ViewBag.subverseModerators = subverseAdministration;

            return PartialView("~/Views/Subverses/_SubverseModerators.cshtml", subverseAdministration);
        }

        // GET: stickied submission
        [ChildActionOnly]
        public ActionResult StickiedSubmission(string subverseName)
        {
            var stickiedSubmissions = _db.StickiedSubmissions.FirstOrDefault(s => s.Subverse == subverseName);

            if (stickiedSubmissions == null) return new EmptyResult();

            var stickiedSubmission = DataCache.Submission.Retrieve(stickiedSubmissions.SubmissionID);

            if (stickiedSubmission != null)
            {
                var subverse = DataCache.Subverse.Retrieve(subverseName);
                if (subverse.IsAnonymized)
                {
                    ViewBag.SubverseAnonymized = true;
                }
                return PartialView("_Stickied", stickiedSubmission);
            }
            return new EmptyResult();
        }

        // GET: list of default subverses
        public ActionResult ListOfDefaultSubverses()
        {
            try
            {
                var listOfSubverses = _db.DefaultSubverses.OrderBy(s => s.Order).ToList();
                return PartialView("_ListOfDefaultSubverses", listOfSubverses);
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
        public JsonResult Subscribe(string subverseName)
        {
            var loggedInUser = User.Identity.Name;

            Voat.Utilities.UserHelper.SubscribeToSubverse(loggedInUser, subverseName);
            return Json("Subscription request was successful.", JsonRequestBehavior.AllowGet);
        }

        // POST: unsubscribe from a subverse
        [Authorize]
        public JsonResult UnSubscribe(string subverseName)
        {
            var loggedInUser = User.Identity.Name;

            Voat.Utilities.UserHelper.UnSubscribeFromSubverse(loggedInUser, subverseName);
            return Json("Unsubscribe request was successful.", JsonRequestBehavior.AllowGet);
        }

        // GET: show submission removal log
        public ActionResult SubmissionRemovalLog(int? page, string subversetoshow)
        {
            ViewBag.SelectedSubverse = subversetoshow;

            try
            {
                var subverse = DataCache.Subverse.Retrieve(subversetoshow);
                if (subverse != null)
                {
                    //HACK: Disable subverse
                    if (subverse.IsAdminDisabled.HasValue && subverse.IsAdminDisabled.Value)
                    {
                        ViewBag.Subverse = subverse.Name;
                        return View("~/Views/Errors/SubverseDisabled.cshtml");
                    }
                }
                var listOfRemovedSubmissions = new PaginatedList<SubmissionRemovalLog>(_db.SubmissionRemovalLogs.Where(rl => rl.Submission.Subverse.Equals(subversetoshow, StringComparison.OrdinalIgnoreCase)).OrderByDescending(rl => rl.CreationDate), page ?? 0, 20);
                return View("SubmissionRemovalLog", listOfRemovedSubmissions);
            }
            catch (Exception)
            {
                return new EmptyResult();
            }
        }

        // GET: show comment removal log
        public ActionResult CommentRemovalLog(int? page, string subversetoshow)
        {
            ViewBag.SelectedSubverse = subversetoshow;

            try
            {
                var subverse = DataCache.Subverse.Retrieve(subversetoshow);
                if (subverse != null)
                {
                    //HACK: Disable subverse
                    if (subverse.IsAdminDisabled.HasValue && subverse.IsAdminDisabled.Value)
                    {
                        ViewBag.Subverse = subverse.Name;
                        return View("~/Views/Errors/SubverseDisabled.cshtml");
                    }
                }
                var listOfRemovedComments = new PaginatedList<CommentRemovalLog>(_db.CommentRemovalLogs.Where(rl => rl.Comment.Submission.Subverse.Equals(subversetoshow, StringComparison.OrdinalIgnoreCase)).OrderByDescending(rl => rl.CreationDate), page ?? 0, 20);
                return View("CommentRemovalLog", listOfRemovedComments);
            }
            catch (Exception)
            {
                return new EmptyResult();
            }
        }

        // GET: show banned users log
        public ActionResult BannedUsersLog(int? page, string subversetoshow)
        {
            ViewBag.SelectedSubverse = subversetoshow;

            try
            {
                var subverse = DataCache.Subverse.Retrieve(subversetoshow);
                if (subverse != null)
                {
                    //HACK: Disable subverse
                    if (subverse.IsAdminDisabled.HasValue && subverse.IsAdminDisabled.Value)
                    {
                        ViewBag.Subverse = subverse.Name;
                        return View("~/Views/Errors/SubverseDisabled.cshtml");
                    }
                }
                ViewBag.TotalBannedUsersInSubverse = _db.SubverseBans.Where(rl => rl.Subverse.Equals(subversetoshow, StringComparison.OrdinalIgnoreCase)).Count();
                var listOfBannedUsers = new PaginatedList<SubverseBan>(_db.SubverseBans.Where(rl => rl.Subverse.Equals(subversetoshow, StringComparison.OrdinalIgnoreCase)).OrderByDescending(rl => rl.CreationDate), page ?? 0, 20);
                return View("BannedUsersLog", listOfBannedUsers);
            }
            catch (Exception)
            {
                return new EmptyResult();
            }
        }

        // POST: block a subverse
        [Authorize]
        public JsonResult BlockSubverse(string subverseName)
        {
            var loggedInUser = User.Identity.Name;

            Voat.Utilities.UserHelper.BlockSubverse(loggedInUser, subverseName);
            return Json("Subverse block request was successful.", JsonRequestBehavior.AllowGet);
        }

        [ChildActionOnly]
        private ActionResult HandleSortedSubverse(int? page, string subversetoshow, string sortingmode, string daterange)
        {
            ViewBag.SortingMode = sortingmode;
            ViewBag.SelectedSubverse = subversetoshow;
            const string cookieName = "NSFWEnabled";
            DateTime startDate = DateTimeUtility.DateRangeToDateTime(daterange);

            if (!sortingmode.Equals("new") && !sortingmode.Equals("top")) return RedirectToAction("Index", "Home");

            const int pageSize = 25;
            int pageNumber = (page ?? 0);
            if (pageNumber < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // check if subverse exists, if not, send to a page not found error
            var subverse = DataCache.Subverse.Retrieve(subversetoshow);
            if (subverse == null) return View("~/Views/Errors/Subversenotfound.cshtml");

            ViewBag.Title = subverse.Description;

            //HACK: Disable subverse
            if (subverse.IsAdminDisabled.HasValue && subverse.IsAdminDisabled.Value)
            {
                ViewBag.Subverse = subverse.Name;
                return View("~/Views/Errors/SubverseDisabled.cshtml");
            }

            // subverse is adult rated, check if user wants to see NSFW content
            PaginatedList<Submission> paginatedSubmissionsByRank;

            if (subverse.IsAdult)
            {
                if (User.Identity.IsAuthenticated)
                {
                    // check if user wants to see NSFW content by reading user preference
                    if (Voat.Utilities.UserHelper.AdultContentEnabled(User.Identity.Name))
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
                string cacheKey = String.Format("subverse.{0}.page.{1}.sort.{2}", subversetoshow, pageNumber, sortingmode).ToLower();
                Tuple<IList<Submission>, int> cacheData = (Tuple<IList<Submission>, int>)CacheHandler.Retrieve(cacheKey);

                if (cacheData == null)
                {
                    var getDataFunc = new Func<object>(() =>
                    {
                        using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                        {
                            var x = SubmissionsFromASubverseByDate(subversetoshow, db);
                            int count = x.Count();
                            List<Submission> content = x.Skip(pageNumber * pageSize).Take(pageSize).ToList();
                            return new Tuple<IList<Submission>, int>(content, count);
                        }
                    });

                    cacheData = (Tuple<IList<Submission>, int>)CacheHandler.Register(cacheKey, getDataFunc, TimeSpan.FromSeconds(subverseCacheTimeInSeconds), (pageNumber < 3 ? 10 : 1));
                }

                ////IAmAGate: Perf mods for caching
                //string cacheKey = String.Format("subverse.{0}.page.{1}.sort.{2}", subversetoshow, pageNumber, sortingmode).ToLower();
                //Tuple<IList<Message>, int> cacheData = (Tuple<IList<Message>, int>)System.Web.HttpContext.Current.Cache[cacheKey];

                //if (cacheData == null)
                //{
                //    var x = SubmissionsFromASubverseByDate(subversetoshow);
                //    int count = x.Count();
                //    List<Message> content = x.Skip(pageNumber * pageSize).Take(pageSize).ToList();
                //    cacheData = new Tuple<IList<Message>, int>(content, count);
                //    System.Web.HttpContext.Current.Cache.Insert(cacheKey, cacheData, null, DateTime.Now.AddSeconds(subverseCacheTimeInSeconds), System.Web.Caching.Cache.NoSlidingExpiration);
                //}

                PaginatedList<Submission> paginatedSubmissionsByDate = new PaginatedList<Submission>(cacheData.Item1, pageNumber, pageSize, cacheData.Item2);

                return View("SubverseIndex", paginatedSubmissionsByDate);

                //var paginatedSubmissionsByDate = new PaginatedList<Message>(SubmissionsFromASubverseByDate(subversetoshow), page ?? 0, pageSize);
                //return View("SubverseIndex", paginatedSubmissionsByDate);
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

        [ChildActionOnly]
        private ActionResult HandleSortedSubverseAll(int? page, string sortingmode, string daterange)
        {
            const string cookieName = "NSFWEnabled";
            const int pageSize = 25;
            DateTime startDate = DateTimeUtility.DateRangeToDateTime(daterange);
            PaginatedList<Submission> paginatedSubmissions;

            ViewBag.SelectedSubverse = "all";

            if (User.Identity.IsAuthenticated)
            {
                var blockedSubverses = _db.UserBlockedSubverses.Where(x => x.UserName.Equals(User.Identity.Name)).Select(x => x.Subverse);
                IQueryable<Submission> submissionsExcludingBlockedSubverses;

                // check if user wants to see NSFW content by reading user preference and exclude submissions from blocked subverses
                if (Voat.Utilities.UserHelper.AdultContentEnabled(User.Identity.Name))
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
            }

            // guest users: check if user wants to see NSFW content by reading NSFW cookie
            if (!HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
            {
                if (sortingmode.Equals("new"))
                {
                    //IAmAGate: Perf mods for caching
                    int pageNumber = page.HasValue ? page.Value : 0;
                    int size = pageSize;
                    string cacheKeyAll = String.Format("subverse.{0}.page.{1}.sort.{2}.sfw", "all", pageNumber, "new").ToLower();
                    Tuple<IList<Submission>, int> cacheDataAll = (Tuple<IList<Submission>, int>)CacheHandler.Retrieve(cacheKeyAll);

                    if (cacheDataAll == null)
                    {
                        var getDataFunc = new Func<object>(() =>
                        {
                            using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                            {
                                var x = SfwSubmissionsFromAllSubversesByDate(db);
                                int count = 50000;
                                List<Submission> content = x.Skip(pageNumber * size).Take(size).ToList();
                                return new Tuple<IList<Submission>, int>(content, count);
                            }
                        });

                        cacheDataAll = (Tuple<IList<Submission>, int>)CacheHandler.Register(cacheKeyAll, getDataFunc, TimeSpan.FromSeconds(subverseCacheTimeInSeconds), (pageNumber < 3 ? 10 : 1));
                    }
                    paginatedSubmissions = new PaginatedList<Submission>(cacheDataAll.Item1, pageNumber, pageSize, cacheDataAll.Item2);

                    //paginatedSubmissions = new PaginatedList<Message>(SfwSubmissionsFromAllSubversesByDate(), page ?? 0, pageSize);
                    return View("SubverseIndex", paginatedSubmissions);
                }
                if (sortingmode.Equals("top"))
                {
                    paginatedSubmissions = new PaginatedList<Submission>(SfwSubmissionsFromAllSubversesByTop(startDate), page ?? 0, pageSize);
                    return View("SubverseIndex", paginatedSubmissions);
                }

                //QUE: I don't think this code is reachable
                // default sorting mode by rank
                paginatedSubmissions = new PaginatedList<Submission>(SfwSubmissionsFromAllSubversesByRank(_db), page ?? 0, pageSize);
                return View("SubverseIndex", paginatedSubmissions);
            }

            if (sortingmode.Equals("new"))
            {

                //IAmAGate: Perf mods for caching
                int pageNumber = page.HasValue ? page.Value : 0;
                string cacheKeyAll = String.Format("subverse.{0}.page.{1}.sort.{2}.nsfw", "all", pageNumber, "new").ToLower();
                Tuple<IList<Submission>, int> cacheDataAll = (Tuple<IList<Submission>, int>)CacheHandler.Retrieve(cacheKeyAll);

                if (cacheDataAll == null)
                {
                    var getDataFunc = new Func<object>(() =>
                    {
                        using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_LIVE))
                        {
                            var x = SubmissionsFromAllSubversesByDate(db);
                            int count = 50000;
                            List<Submission> content = x.Skip(pageNumber * pageSize).Take(pageSize).ToList();
                            return new Tuple<IList<Submission>, int>(content, count);
                        }
                    });

                    cacheDataAll = (Tuple<IList<Submission>, int>)CacheHandler.Register(cacheKeyAll, getDataFunc, TimeSpan.FromSeconds(subverseCacheTimeInSeconds), (pageNumber < 3 ? 10 : 1));
                }
                paginatedSubmissions = new PaginatedList<Submission>(cacheDataAll.Item1, pageNumber, pageSize, cacheDataAll.Item2);

                //paginatedSubmissions = new PaginatedList<Message>(SubmissionsFromAllSubversesByDate(), page ?? 0, pageSize);
                return View("SubverseIndex", paginatedSubmissions);
            }
            if (sortingmode.Equals("top"))
            {
                paginatedSubmissions = new PaginatedList<Submission>(SubmissionsFromAllSubversesByTop(startDate), page ?? 0, pageSize);
                return View("SubverseIndex", paginatedSubmissions);
            }

            // default sorting mode by rank
            paginatedSubmissions = new PaginatedList<Submission>(SubmissionsFromAllSubversesByRank(), page ?? 0, pageSize);
            return View("SubverseIndex", paginatedSubmissions);
        }

        [ChildActionOnly]
        [OutputCache(Duration = 600, VaryByParam = "none")]
        public ActionResult TopViewedSubmissions24Hours()
        {
            //var submissions = 
            var cacheData = CacheHandler.Register("TopViewedSubmissions24Hours", new Func<object>(() =>
            {
                using (voatEntities db = new voatEntities(CONSTANTS.CONNECTION_READONLY))
                {
                    return SfwSubmissionsFromAllSubversesByViews24Hours(db).ToList();
                }

            }), TimeSpan.FromMinutes(60), 5);


            return PartialView("_MostViewedSubmissions", cacheData);
        }

        #region sfw submissions from all subverses
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
                                                                        where !message.IsArchived && !message.IsDeleted && subverse.IsPrivate != true && subverse.IsAdminPrivate != true  && subverse.IsAdult == false && subverse.MinCCPForDownvote == 0 && message.Rank > 0.00009
                                                                        where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                        where !subverse.IsAdminDisabled.Value
                                                                        where !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(subverse.Name) select ubs.UserName).Contains(userName)
                                                                        select message).OrderByDescending(s => s.Rank).ThenByDescending(s => s.CreationDate).AsNoTracking();

            return sfwSubmissionsFromAllSubversesByRank;
        }

        private IQueryable<Submission> SfwSubmissionsFromAllSubversesByTop(DateTime startDate)
        {
            IQueryable<Submission> sfwSubmissionsFromAllSubversesByTop = (from message in _db.Submissions
                                                                          join subverse in _db.Subverses on message.Subverse equals subverse.Name
                                                                       where !message.IsDeleted && subverse.IsPrivate != true && subverse.IsAdult == false && subverse.MinCCPForDownvote == 0
                                                                       where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                       where !subverse.IsAdminDisabled.Value
                                                                       where !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(subverse.Name) select ubs.UserName).Contains(User.Identity.Name)
                                                                       select message).OrderByDescending(s => s.UpCount - s.DownCount).Where(s => s.CreationDate >= startDate && s.CreationDate <= DateTime.Now)
                                                                       .AsNoTracking();

            return sfwSubmissionsFromAllSubversesByTop;
        }

        private IQueryable<Submission> SfwSubmissionsFromAllSubversesByViews24Hours(voatEntities _db)
        {
            if (_db == null)
            {
                _db = this._db;
            }
            var startDate = DateTime.Now.Add(new TimeSpan(0, -24, 0, 0, 0));
            IQueryable<Submission> sfwSubmissionsFromAllSubversesByViews24Hours = (from message in _db.Submissions
                                                                                   join subverse in _db.Subverses on message.Subverse equals subverse.Name
                                                                                where !message.IsArchived && !message.IsDeleted && subverse.IsPrivate != true && subverse.IsAdminPrivate != true && subverse.IsAdult == false && message.CreationDate >= startDate && message.CreationDate <= DateTime.Now
                                                                                where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                                where !subverse.IsAdminDisabled.Value
                                                                                where !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(subverse.Name) select ubs.UserName).Contains(User.Identity.Name)
                                                                                select message).OrderByDescending(s => s.Views).DistinctBy(m => m.Subverse).Take(5).AsQueryable().AsNoTracking();

            return sfwSubmissionsFromAllSubversesByViews24Hours;
        }
        #endregion

        #region unfiltered submissions from all subverses
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

        private IQueryable<Submission> SubmissionsFromAllSubversesByRank()
        {
            IQueryable<Submission> submissionsFromAllSubversesByRank = (from message in _db.Submissions
                                                                        join subverse in _db.Subverses on message.Subverse equals subverse.Name
                                                                     where !message.IsArchived && !message.IsDeleted && subverse.IsPrivate != true && subverse.IsAdminPrivate != true && subverse.MinCCPForDownvote == 0 && message.Rank > 0.00009
                                                                     where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                     where !subverse.IsAdminDisabled.Value
                                                                     where !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(subverse.Name) select ubs.UserName).Contains(User.Identity.Name)
                                                                     select message).OrderByDescending(s => s.Rank).ThenByDescending(s => s.CreationDate).AsNoTracking();

            return submissionsFromAllSubversesByRank;
        }

        private IQueryable<Submission> SubmissionsFromAllSubversesByTop(DateTime startDate)
        {
            IQueryable<Submission> submissionsFromAllSubversesByTop = (from message in _db.Submissions
                                                                       join subverse in _db.Subverses on message.Subverse equals subverse.Name
                                                                    where !message.IsDeleted && subverse.IsPrivate != true && subverse.IsAdminPrivate != true && subverse.MinCCPForDownvote  == 0
                                                                    where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                    where !subverse.IsAdminDisabled.Value
                                                                    where !(from ubs in _db.UserBlockedSubverses where ubs.Subverse.Equals(subverse.Name) select ubs.UserName).Contains(User.Identity.Name)
                                                                    select message).OrderByDescending(s => s.UpCount - s.DownCount).Where(s => s.CreationDate >= startDate && s.CreationDate <= DateTime.Now)
                                                                    .AsNoTracking();

            return submissionsFromAllSubversesByTop;
        }
        #endregion

        #region submissions from a single subverse
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

        private IQueryable<Submission> SubmissionsFromASubverseByTop(string subverseName, DateTime startDate)
        {
            var subverseStickie = _db.StickiedSubmissions.FirstOrDefault(ss => ss.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase));
            IQueryable<Submission> submissionsFromASubverseByTop = (from message in _db.Submissions
                                                                    join subverse in _db.Subverses on message.Subverse equals subverse.Name
                                                                 where !message.IsDeleted && message.Subverse == subverseName
                                                                 where !(from bu in _db.BannedUsers select bu.UserName).Contains(message.UserName)
                                                                 select message).OrderByDescending(s => s.UpCount - s.DownCount).Where(s => s.CreationDate >= startDate && s.CreationDate <= DateTime.Now)
                                                                 .AsNoTracking();
            if (subverseStickie != null)
            {
                return submissionsFromASubverseByTop.Where(s => s.ID != subverseStickie.SubmissionID);
            }
            return submissionsFromASubverseByTop;
        }
        #endregion

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
        #endregion
    }
}