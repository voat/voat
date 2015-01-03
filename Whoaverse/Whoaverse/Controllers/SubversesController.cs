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
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Voat.Models;
using Voat.Models.ViewModels;
using Voat.Utils;

namespace Voat.Controllers
{
    public class SubversesController : Controller
    {
        private readonly whoaverseEntities _db = new whoaverseEntities();

        // GET: sidebar for selected subverse
        public ActionResult SidebarForSelectedSubverseComments(string selectedSubverse, bool showingComments,
            string name, DateTime? date, DateTime? lastEditDate, int? submissionId, int? likes, int? dislikes,
            bool anonymized, int? views)
        {
            var subverse = _db.Subverses.Find(selectedSubverse);

            if (subverse == null) return new EmptyResult();
            // get subscriber count for selected subverse
            var subscriberCount =
                _db.Subscriptions.Count(r => r.SubverseName.Equals(selectedSubverse, StringComparison.OrdinalIgnoreCase));

            ViewBag.SubscriberCount = subscriberCount;
            ViewBag.SelectedSubverse = selectedSubverse;

            if (!showingComments) return new EmptyResult();
            if (anonymized || subverse.anonymized_mode)
            {
                ViewBag.name = submissionId.ToString();
                ViewBag.anonymized = true;
            }
            else
            {
                ViewBag.name = name;
            }

            ViewBag.date = date;
            ViewBag.lastEditDate = lastEditDate;
            ViewBag.likes = likes;
            ViewBag.dislikes = dislikes;
            ViewBag.anonymized_mode = subverse.anonymized_mode;
            ViewBag.views = views;

            try
            {
                ViewBag.OnlineUsers = SessionTracker.ActiveSessionsForSubverse(selectedSubverse);
            }
            catch (Exception)
            {
                ViewBag.OnlineUsers = -1;
            }

            return PartialView("~/Views/Shared/Sidebars/_SidebarComments.cshtml", subverse);
            //don't return a sidebar since subverse doesn't exist or is a system subverse
        }

        // GET: sidebar for selected subverse
        public ActionResult SidebarForSelectedSubverse(string selectedSubverse)
        {
            var subverse = _db.Subverses.Find(selectedSubverse);

            if (subverse == null) return new EmptyResult();
            // get subscriber count for selected subverse
            var subscriberCount =
                _db.Subscriptions.Count(r => r.SubverseName.Equals(selectedSubverse, StringComparison.OrdinalIgnoreCase));

            ViewBag.SubscriberCount = subscriberCount;
            ViewBag.SelectedSubverse = selectedSubverse;

            try
            {
                ViewBag.OnlineUsers = SessionTracker.ActiveSessionsForSubverse(selectedSubverse);
            }
            catch (Exception)
            {
                ViewBag.OnlineUsers = -1;
            }

            return PartialView("~/Views/Shared/Sidebars/_Sidebar.cshtml", subverse);
            //don't return a sidebar since subverse doesn't exist or is a system subverse
        }

        // GET: stylesheet for selected subverse
        public ActionResult StylesheetForSelectedSubverse(string selectedSubverse)
        {
            var subverse = _db.Subverses.FirstOrDefault(i => i.name == selectedSubverse);

            return Content(subverse != null ? subverse.stylesheet : string.Empty);
        }

        // POST: Create a new Subverse
        // To protect from overposting attacks, enable the specific properties you want to bind to 
        [HttpPost]
        [PreventSpam(DelayRequest = 300, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateSubverse([Bind(Include = "Name, Title, Description, Type, Sidebar, Creation_date, Owner")] AddSubverse subverseTmpModel)
        {
            if (!User.Identity.IsAuthenticated) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            int minimumCcp = Convert.ToInt32(ConfigurationManager.AppSettings["minimumCcp"]);
            int maximumOwnedSubs = Convert.ToInt32(ConfigurationManager.AppSettings["maximumOwnedSubs"]);

            // verify recaptcha if user has less than minimum required CCP
            if (Karma.CommentKarma(User.Identity.Name) < minimumCcp)
            {
                // begin recaptcha check
                const string captchaMessage = "";
                var isCaptchaCodeValid = ReCaptchaUtility.GetCaptchaResponse(captchaMessage, Request);

                if (!isCaptchaCodeValid)
                {
                    ModelState.AddModelError("", "Incorrect recaptcha answer.");
                    return View();
                }
                // end recaptcha check
            }

            try
            {
                if (!ModelState.IsValid) return View();
                // setup default values
                var subverse = new Subverse
                {
                    name = subverseTmpModel.Name,
                    title = "/v/" + subverseTmpModel.Name,
                    description = subverseTmpModel.Description,
                    sidebar = subverseTmpModel.Sidebar,
                    creation_date = DateTime.Now,
                    type = "link",
                    enable_thumbnails = true,
                    rated_adult = false,
                    private_subverse = false
                };

                // check if subverse exists before attempting to create it
                if (_db.Subverses.Find(subverse.name) == null)
                {
                    // only allow users with less than maximum allowed subverses to create a subverse
                    var amountOfOwnedSubverses = _db.SubverseAdmins
                        .Where(s => s.Username == User.Identity.Name && s.Power == 1)
                        .ToList();

                    if (amountOfOwnedSubverses.Count <= maximumOwnedSubs)
                    {
                        _db.Subverses.Add(subverse);
                        await _db.SaveChangesAsync();

                        // register user as the owner of the newly created subverse
                        var tmpSubverseAdmin = new SubverseAdmin
                        {
                            SubverseName = subverse.name,
                            Username = User.Identity.Name,
                            Power = 1
                        };
                        _db.SubverseAdmins.Add(tmpSubverseAdmin);
                        await _db.SaveChangesAsync();

                        // subscribe user to the newly created subverse
                        Utils.User.SubscribeToSubverse(subverseTmpModel.Owner, subverse.name);

                        // go to newly created Subverse
                        return RedirectToAction("SubverseIndex", "Subverses", new { subversetoshow = subverse.name });
                    }
                    ModelState.AddModelError(string.Empty, "Sorry, you can not own more than " + maximumOwnedSubs + " subverses.");
                    return View();
                }
                ModelState.AddModelError(string.Empty,
                    "Sorry, The subverse you are trying to create already exists, but you can try to claim it by submitting a takeover request to /v/subverserequest.");
                return View();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Something bad happened.");
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
            var subverse = _db.Subverses.Find(subversetoshow);
            if (subverse == null)
            {
                ViewBag.SelectedSubverse = "404";
                return View("~/Views/Errors/Subversenotfound.cshtml");
            }

            // check that the user requesting to edit subverse settings is subverse owner!
            var subAdmin =
                _db.SubverseAdmins.FirstOrDefault(
                    x => x.SubverseName == subversetoshow && x.Username == User.Identity.Name && x.Power <= 2);

            if (subAdmin == null) return RedirectToAction("Index", "Home");
            // map existing data to view model for editing and pass it to frontend
            // NOTE: we should look into a mapper which automatically maps these properties to corresponding fields to avoid tedious manual mapping
            var viewModel = new SubverseSettingsViewModel
            {
                Name = subverse.name,
                Type = subverse.type,
                Submission_text = subverse.submission_text,
                Description = subverse.description,
                Sidebar = subverse.sidebar,
                Stylesheet = subverse.stylesheet,
                Allow_default = subverse.allow_default,
                Label_submit_new_link = subverse.label_submit_new_link,
                Label_sumit_new_selfpost = subverse.label_sumit_new_selfpost,
                Rated_adult = subverse.rated_adult,
                Private_subverse = subverse.private_subverse,
                Enable_thumbnails = subverse.enable_thumbnails,
                Authorized_submitters_only = subverse.authorized_submitters_only,
                Anonymized_mode = subverse.anonymized_mode
            };

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SubverseName = subverse.name;
            return View("~/Views/Subverses/Admin/SubverseSettings.cshtml", viewModel);
        }

        // POST: Eddit a Subverse
        // To protect from overposting attacks, enable the specific properties you want to bind to 
        [HttpPost]
        [PreventSpam(DelayRequest = 30,
        ErrorMessage = "Sorry, you are doing that too fast. Please try again in 30 seconds.")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubverseSettings(Subverse updatedModel)
        {
            try
            {
                if (!ModelState.IsValid) return View("~/Views/Subverses/Admin/SubverseSettings.cshtml");
                var existingSubverse = _db.Subverses.Find(updatedModel.name);

                // check if subverse exists before attempting to edit it
                if (existingSubverse != null)
                {
                    // check if user requesting edit is authorized to do so for current subverse
                    // check that the user requesting to edit subverse settings is subverse owner!
                    var subAdmin =
                        _db.SubverseAdmins.FirstOrDefault(
                            x => x.SubverseName == updatedModel.name && x.Username == User.Identity.Name && x.Power <= 2);

                    if (subAdmin == null) return new EmptyResult();
                    // TODO investigate if EntityState is applicable here and use that instead
                    // db.Entry(updatedModel).State = EntityState.Modified;

                    existingSubverse.description = updatedModel.description;
                    existingSubverse.sidebar = updatedModel.sidebar;

                    if (updatedModel.stylesheet != null)
                    {
                        if (updatedModel.stylesheet.Length < 50001)
                        {
                            existingSubverse.stylesheet = updatedModel.stylesheet;
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Sorry, custom CSS limit is set to 50000 characters.");
                            return View("~/Views/Subverses/Admin/SubverseSettings.cshtml");
                        }
                    }
                    else
                    {
                        existingSubverse.stylesheet = updatedModel.stylesheet;
                    }

                    existingSubverse.rated_adult = updatedModel.rated_adult;
                    existingSubverse.private_subverse = updatedModel.private_subverse;
                    existingSubverse.enable_thumbnails = updatedModel.enable_thumbnails;
                    existingSubverse.authorized_submitters_only = updatedModel.authorized_submitters_only;

                    // these properties are currently not implemented but they can be saved and edited for future use
                    existingSubverse.type = updatedModel.type;
                    existingSubverse.label_submit_new_link = updatedModel.label_submit_new_link;
                    existingSubverse.label_sumit_new_selfpost = updatedModel.label_sumit_new_selfpost;
                    existingSubverse.submission_text = updatedModel.submission_text;
                    existingSubverse.allow_default = updatedModel.allow_default;

                    if (existingSubverse.anonymized_mode && updatedModel.anonymized_mode == false)
                    {
                        ModelState.AddModelError(string.Empty,
                            "Sorry, this subverse is permanently locked to anonymized mode.");
                        return View("~/Views/Subverses/Admin/SubverseSettings.cshtml");
                    }

                    existingSubverse.anonymized_mode = updatedModel.anonymized_mode;

                    await _db.SaveChangesAsync();

                    // go back to this subverse
                    return RedirectToAction("SubverseIndex", "Subverses", new { subversetoshow = updatedModel.name });
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

        // GET: show a subverse index
        public ActionResult SubverseIndex(int? page, string subversetoshow)
        {
            const string cookieName = "NSFWEnabled";
            const int pageSize = 25;
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
                SessionTracker.Add(currentSubverse, Session.SessionID);
                ViewBag.OnlineUsers = SessionTracker.ActiveSessionsForSubverse(currentSubverse);
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
                    var subverse = _db.Subverses.Find(subversetoshow);
                    if (subverse != null)
                    {
                        ViewBag.SelectedSubverse = subverse.name;
                        ViewBag.Title = subverse.description;

                        var paginatedSubmissions = new PaginatedList<Message>(SubmissionsFromASubverseByRank(subversetoshow), page ?? 0, pageSize);

                        // check if subverse is rated adult, show a NSFW warning page before entering
                        if (!subverse.rated_adult) return View(paginatedSubmissions);

                        // check if user wants to see NSFW content by reading user preference
                        if (User.Identity.IsAuthenticated)
                        {
                            if (Utils.User.AdultContentEnabled(User.Identity.Name))
                            {
                                return View(paginatedSubmissions);
                            }

                            // display a view explaining that account preference is set to NO NSFW and why this subverse can not be shown
                            return RedirectToAction("AdultContentFiltered", "Subverses", new { destination = subverse.name });
                        }

                        // check if user wants to see NSFW content by reading NSFW cookie
                        if (!ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
                        {
                            return RedirectToAction("AdultContentWarning", "Subverses", new { destination = subverse.name, nsfwok = false });
                        }
                        return View(paginatedSubmissions);
                    }
                    ViewBag.SelectedSubverse = "404";
                    return View("~/Views/Errors/Subversenotfound.cshtml");
                }

                // selected subverse is ALL, show submissions from all subverses, sorted by rank
                ViewBag.SelectedSubverse = "all";
                ViewBag.Title = "all subverses";

                // check if user wants to see NSFW content by reading user preference
                PaginatedList<Message> paginatedSfwSubmissions;

                if (User.Identity.IsAuthenticated)
                {
                    if (Utils.User.AdultContentEnabled(User.Identity.Name))
                    {
                        var paginatedSubmissionsFromAllSubverses = new PaginatedList<Message>(SubmissionsFromAllSubversesByRank(), page ?? 0, pageSize);
                        return View(paginatedSubmissionsFromAllSubverses);
                    }

                    // return only sfw submissions
                    paginatedSfwSubmissions = new PaginatedList<Message>(SfwSubmissionsFromAllSubversesByRank(), page ?? 0, pageSize);
                    return View(paginatedSfwSubmissions);
                }

                // check if user wants to see NSFW content by reading NSFW cookie
                if (ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
                {
                    var paginatedSubmissionsFromAllSubverses = new PaginatedList<Message>(SubmissionsFromAllSubversesByRank(), page ?? 0, pageSize);
                    return View(paginatedSubmissionsFromAllSubverses);
                }

                // return only sfw submissions
                paginatedSfwSubmissions = new PaginatedList<Message>(SfwSubmissionsFromAllSubversesByRank(), page ?? 0, pageSize);
                return View(paginatedSfwSubmissions);
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        public ActionResult SortedSubverseFrontpage(int? page, string subversetoshow, string sortingmode)
        {
            // sortingmode: new, contraversial, hot, etc
            ViewBag.SortingMode = sortingmode;
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
                SessionTracker.Add(currentSubverse, Session.SessionID);
                ViewBag.OnlineUsers = SessionTracker.ActiveSessionsForSubverse(currentSubverse);
            }
            catch (Exception)
            {
                ViewBag.OnlineUsers = -1;
            }

            if (!subversetoshow.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                return HandleSortedSubverse(page, subversetoshow, sortingmode);
            }

            // selected subverse is ALL, show submissions from all subverses, sorted by date
            return HandleSortedSubverseAll(page, sortingmode);
        }

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
                var subverses = _db.Subverses
                    .OrderByDescending(s => s.subscribers);

                var paginatedSubverses = new PaginatedList<Subverse>(subverses, page ?? 0, pageSize);

                return View(paginatedSubverses);
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
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
                                                                       join a in _db.Subscriptions
                                                                       on c.name equals a.SubverseName
                                                                       where a.Username.Equals(User.Identity.Name)
                                                                       orderby a.SubverseName ascending
                                                                       select new SubverseDetailsViewModel
                                                                       {
                                                                           Name = c.name,
                                                                           Title = c.title,
                                                                           Description = c.description,
                                                                           Creation_date = c.creation_date,
                                                                           Subscribers = c.subscribers
                                                                       };

            var paginatedSubscribedSubverses = new PaginatedList<SubverseDetailsViewModel>(subscribedSubverses, page ?? 0, pageSize);

            return View("SubscribedSubverses", paginatedSubscribedSubverses);
        }

        // GET: sidebar for selected subverse
        public ActionResult DetailsForSelectedSubverse(string selectedSubverse)
        {
            var subverse = _db.Subverses.Find(selectedSubverse);

            if (subverse == null) return new EmptyResult();
            // get subscriber count for selected subverse
            var subscriberCount = _db.Subscriptions.Count(r => r.SubverseName.Equals(selectedSubverse, StringComparison.OrdinalIgnoreCase));

            ViewBag.SubscriberCount = subscriberCount;
            ViewBag.SelectedSubverse = selectedSubverse;
            return PartialView("_SubverseDetails", subverse);
            //don't return a sidebar since subverse doesn't exist or is a system subverse
        }

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

            var subverses = _db.Subverses
                .Where(s => s.description != null && s.sidebar != null)
                .OrderByDescending(s => s.creation_date);

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
                .Where(s => s.description != null && s.sidebar != null && s.last_submission_received != null)
                .OrderByDescending(s => s.last_submission_received);

            var paginatedActiveSubverses = new PaginatedList<Subverse>(subverses, page ?? 0, pageSize);

            return View("~/Views/Subverses/Subverses.cshtml", paginatedActiveSubverses);
        }

        [OutputCache(VaryByParam = "none", Duration = 3600)]
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
                var cookie = new HttpCookie("NSFWEnabled")
                {
                    Value = "whoaverse nsfw warning cookie",
                    Expires = DateTime.Now.AddDays(1)
                };
                ControllerContext.HttpContext.Response.Cookies.Add(cookie);

                // redirect to destination subverse
                return RedirectToAction("SubverseIndex", "Subverses", new { subversetoshow = destination });
            }
            ViewBag.Destination = destination;
            return View("~/Views/Subverses/AdultContentWarning.cshtml");
        }

        // fetch a random subbverse with x subscribers and x submissions
        public ActionResult Random()
        {
            try
            {
                // fetch a random subverse with minimum number of subscribers
                var subverse = from subverses in _db.Subverses
                          .Where(s => s.subscribers > 10 && !s.name.Equals("all", StringComparison.OrdinalIgnoreCase))
                               select subverses;

                var submissionCount = 0;
                Subverse randomSubverse;

                do
                {
                    var count = subverse.Count(); // 1st round-trip
                    var index = new Random().Next(count);

                    randomSubverse = subverse.OrderBy(s => s.name).Skip(index).FirstOrDefault(); // 2nd round-trip

                    var submissions = _db.Messages
                            .Where(x => x.Subverse == randomSubverse.name && x.Name != "deleted")
                            .OrderByDescending(s => s.Rank)
                            .Take(50)
                            .ToList();

                    if (submissions.Count > 9)
                    {
                        submissionCount = submissions.Count;
                    }
                } while (submissionCount == 0);

                return RedirectToAction("SubverseIndex", "Subverses", new { subversetoshow = randomSubverse.name });
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        // GET: subverse moderators for selected subverse
        [Authorize]
        public ActionResult SubverseModerators(string subversetoshow)
        {
            // get model for selected subverse
            var subverseModel = _db.Subverses.Find(subversetoshow);

            if (subverseModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // check if caller is subverse owner, if not, deny listing
            if (!Utils.User.IsUserSubverseAdmin(User.Identity.Name, subversetoshow))
                return RedirectToAction("Index", "Home");
            var subverseModerators = _db.SubverseAdmins
                .Where(n => n.SubverseName == subversetoshow)
                .Take(20)
                .OrderBy(s => s.Power)
                .ToList();

            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subversetoshow;

            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/SubverseModerators.cshtml", subverseModerators);
        }

        // GET: show add moderators view for selected subverse
        [Authorize]
        public ActionResult AddModerator(string subversetoshow)
        {
            // get model for selected subverse
            var subverseModel = _db.Subverses.Find(subversetoshow);

            if (subverseModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // check if caller is subverse owner, if not, deny listing
            if (!Utils.User.IsUserSubverseAdmin(User.Identity.Name, subversetoshow))
                return RedirectToAction("Index", "Home");
            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subversetoshow;
            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/AddModerator.cshtml");
        }

        // POST: add a moderator to given subverse
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddModerator([Bind(Include = "Id,SubverseName,Username,Power")] SubverseAdmin subverseAdmin)
        {
            if (!ModelState.IsValid) return View(subverseAdmin);

            // get model for selected subverse
            var subverseModel = _db.Subverses.Find(subverseAdmin.SubverseName);

            if (subverseModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            int maximumOwnedSubs = Convert.ToInt32(ConfigurationManager.AppSettings["maximumOwnedSubs"]);

            // check if the user being added is not already a moderator of 10 subverses
            var currentlyModerating = _db.SubverseAdmins
                .Where(a => a.Username == subverseAdmin.Username).ToList();

            SubverseModeratorViewModel tmpModel;
            if (currentlyModerating.Count <= maximumOwnedSubs)
            {
                // check if caller is subverse owner, if not, deny posting
                if (!Utils.User.IsUserSubverseAdmin(User.Identity.Name, subverseAdmin.SubverseName))
                    return RedirectToAction("Index", "Home");

                // check that user is not already moderating given subverse
                var isAlreadyModerator = _db.SubverseAdmins.FirstOrDefault(a => a.Username == subverseAdmin.Username && a.SubverseName == subverseAdmin.SubverseName);

                if (isAlreadyModerator == null)
                {
                    subverseAdmin.SubverseName = subverseModel.name;
                    _db.SubverseAdmins.Add(subverseAdmin);
                    _db.SaveChanges();
                    return RedirectToAction("SubverseModerators");
                }
                ModelState.AddModelError(string.Empty, "Sorry, the user is already moderating this subverse.");
                tmpModel = new SubverseModeratorViewModel
                {
                    Username = subverseAdmin.Username,
                    Power = subverseAdmin.Power
                };
                ViewBag.SubverseModel = subverseModel;
                ViewBag.SubverseName = subverseAdmin.SubverseName;
                ViewBag.SelectedSubverse = string.Empty;
                return View("~/Views/Subverses/Admin/AddModerator.cshtml", tmpModel);
            }
            ModelState.AddModelError(string.Empty, "Sorry, the user is already moderating a maximum of " + maximumOwnedSubs + " subverses.");
            tmpModel = new SubverseModeratorViewModel
            {
                Username = subverseAdmin.Username,
                Power = subverseAdmin.Power
            };
            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subverseAdmin.SubverseName;
            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/AddModerator.cshtml", tmpModel);
        }

        // GET: show remove moderators view for selected subverse
        [Authorize]
        public ActionResult RemoveModerator(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var subverseAdmin = _db.SubverseAdmins.Find(id);

            if (subverseAdmin == null)
            {
                return HttpNotFound();
            }

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SubverseName = subverseAdmin.SubverseName;
            return View("~/Views/Subverses/Admin/RemoveModerator.cshtml", subverseAdmin);
        }

        // GET: show resign as moderator view for selected subverse
        [Authorize]
        public ActionResult ResignAsModerator(string subversetoresignfrom)
        {
            if (subversetoresignfrom == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var subverseAdmin = _db.SubverseAdmins.FirstOrDefault(s => s.SubverseName == subversetoresignfrom && s.Username == User.Identity.Name && s.Power > 1);

            if (subverseAdmin == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.SelectedSubverse = string.Empty;
            ViewBag.SubverseName = subverseAdmin.SubverseName;

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
            var moderatorToBeRemoved = _db.SubverseAdmins.FirstOrDefault(s => s.SubverseName == subversetoresignfrom && s.Username == User.Identity.Name && s.Power != 1);

            if (moderatorToBeRemoved == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var subverse = _db.Subverses.Find(moderatorToBeRemoved.SubverseName);
            if (subverse == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // execute removal                    
            _db.SubverseAdmins.Remove(moderatorToBeRemoved);
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
            var moderatorToBeRemoved = await _db.SubverseAdmins.FindAsync(id);
            if (moderatorToBeRemoved == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var subverse = _db.Subverses.Find(moderatorToBeRemoved.SubverseName);
            if (subverse == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // check if caller has clearance to remove a moderator
            if (!Utils.User.IsUserSubverseAdmin(User.Identity.Name, subverse.name) ||
                moderatorToBeRemoved.Username == User.Identity.Name) return RedirectToAction("Index", "Home");
            // execute removal
            var subverseAdmin = await _db.SubverseAdmins.FindAsync(id);
            _db.SubverseAdmins.Remove(subverseAdmin);
            await _db.SaveChangesAsync();
            return RedirectToAction("SubverseModerators");
        }

        // GET: show subverse flair settings view for selected subverse
        [Authorize]
        public ActionResult SubverseFlairSettings(string subversetoshow)
        {
            // get model for selected subverse
            var subverseModel = _db.Subverses.Find(subversetoshow);

            if (subverseModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // check if caller is subverse owner, if not, deny listing
            if (!Utils.User.IsUserSubverseAdmin(User.Identity.Name, subversetoshow) &&
                !Utils.User.IsUserSubverseModerator(User.Identity.Name, subversetoshow))
                return RedirectToAction("Index", "Home");
            var subverseFlairsettings = _db.Subverseflairsettings
                .Where(n => n.Subversename == subversetoshow)
                .Take(20)
                .OrderBy(s => s.Id)
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
            var subverseModel = _db.Subverses.Find(subversetoshow);

            if (subverseModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // check if caller is subverse owner, if not, deny listing
            if (!Utils.User.IsUserSubverseAdmin(User.Identity.Name, subversetoshow) &&
                !Utils.User.IsUserSubverseModerator(User.Identity.Name, subversetoshow))
                return RedirectToAction("Index", "Home");
            ViewBag.SubverseModel = subverseModel;
            ViewBag.SubverseName = subversetoshow;
            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Subverses/Admin/Flair/AddLinkFlair.cshtml");
        }

        // POST: add a link flair to given subverse
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddLinkFlair([Bind(Include = "Id,Subversename,Label,CssClass")] Subverseflairsetting subverseFlairSetting)
        {
            if (!ModelState.IsValid) return View(subverseFlairSetting);
            // get model for selected subverse
            var subverseModel = _db.Subverses.Find(subverseFlairSetting.Subversename);

            if (subverseModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // check if caller is subverse owner, if not, deny posting
            if (!Utils.User.IsUserSubverseAdmin(User.Identity.Name, subverseFlairSetting.Subversename) &&
                !Utils.User.IsUserSubverseModerator(User.Identity.Name, subverseFlairSetting.Subversename))
                return RedirectToAction("Index", "Home");
            subverseFlairSetting.Subversename = subverseModel.name;
            _db.Subverseflairsettings.Add(subverseFlairSetting);
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

            var subverseFlairSetting = _db.Subverseflairsettings.Find(id);

            if (subverseFlairSetting == null)
            {
                return HttpNotFound();
            }

            ViewBag.SubverseName = subverseFlairSetting.Subversename;
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
            var linkFlairToRemove = await _db.Subverseflairsettings.FindAsync(id);
            if (linkFlairToRemove == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var subverse = _db.Subverses.Find(linkFlairToRemove.Subversename);
            if (subverse == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // check if caller has clearance to remove a link flair
            if (!Utils.User.IsUserSubverseAdmin(User.Identity.Name, subverse.name) &&
                !Utils.User.IsUserSubverseModerator(User.Identity.Name, subverse.name))
                return RedirectToAction("Index", "Home");
            // execute removal
            var subverseFlairSetting = await _db.Subverseflairsettings.FindAsync(id);
            _db.Subverseflairsettings.Remove(subverseFlairSetting);
            await _db.SaveChangesAsync();
            return RedirectToAction("SubverseFlairSettings");
        }

        // GET: render a partial view with list of moderators for a given subverse, if no moderators are found, return subverse owner
        [ChildActionOnly]
        public ActionResult SubverseModeratorsList(string subverseName)
        {
            // get 10 administration members for a subverse
            var subverseAdministration =
                _db.SubverseAdmins
                .Where(n => n.SubverseName.Equals(subverseName, StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .ToList()
                .OrderBy(s => s.Username);

            // find all moderators with power = 2
            var subverseModerators = subverseAdministration
                .Where(m => m.Power == 2);

            // if there are no moderators with power = 2, find subverse owner with power = 1 and return him instead
            if (!subverseModerators.Any())
            {
                subverseModerators =
                subverseAdministration
                .Where(n => n.SubverseName.Equals(subverseName, StringComparison.OrdinalIgnoreCase) && n.Power == 1)
                .Take(1)
                .ToList();
            }

            ViewBag.subverseModerators = subverseModerators;

            return PartialView("~/Views/Subverses/_SubverseModerators.cshtml", subverseModerators);
        }

        // GET: stickied submission
        [ChildActionOnly]
        public ActionResult StickiedSubmission(string subverseName)
        {
            var stickiedSubmissions = _db.Stickiedsubmissions.FirstOrDefault(s => s.Subversename == subverseName);

            if (stickiedSubmissions == null) return new EmptyResult();

            var stickiedSubmission = _db.Messages.Find(stickiedSubmissions.Submission_id);

            if (stickiedSubmission != null)
            {
                return PartialView("_Stickied", stickiedSubmission);
            }
            return new EmptyResult();
        }

        // GET: list of default subverses
        public ActionResult ListOfDefaultSubverses()
        {
            try
            {
                var listOfSubverses = _db.Defaultsubverses.OrderBy(s => s.position).ToList();
                return PartialView("_ListOfDefaultSubverses", listOfSubverses);
            }
            catch (Exception)
            {
                return new EmptyResult();
            }
        }

        [Authorize]
        public ActionResult ListOfSubversesUserIsSubscribedTo()
        {
            // show custom list of subverses in top menu
            var listOfSubverses = _db.Subscriptions
                .Where(s => s.Username == User.Identity.Name)
                .OrderBy(s => s.SubverseName);

            return PartialView("_ListOfSubscribedToSubverses", listOfSubverses);
        }

        // POST: subscribe to a subverse
        [Authorize]
        public JsonResult Subscribe(string subverseName)
        {
            var loggedInUser = User.Identity.Name;

            Utils.User.SubscribeToSubverse(loggedInUser, subverseName);
            return Json("Subscription request was successful.", JsonRequestBehavior.AllowGet);
        }

        // POST: unsubscribe from a subverse
        [Authorize]
        public JsonResult UnSubscribe(string subverseName)
        {
            var loggedInUser = User.Identity.Name;

            Utils.User.UnSubscribeFromSubverse(loggedInUser, subverseName);
            return Json("Unsubscribe request was successful.", JsonRequestBehavior.AllowGet);
        }

        [ChildActionOnly]
        private ActionResult HandleSortedSubverse(int? page, string subversetoshow, string sortingmode)
        {
            ViewBag.SortingMode = sortingmode;
            ViewBag.SelectedSubverse = subversetoshow;
            const string cookieName = "NSFWEnabled";

            if (!sortingmode.Equals("new") && !sortingmode.Equals("top")) return RedirectToAction("Index", "Home");
            const int pageSize = 25;
            int pageNumber = (page ?? 0);
            if (pageNumber < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            ViewBag.Title = subversetoshow;

            // check if subverse exists, if not, send to a page not found error
            var subverse = _db.Subverses.Find(subversetoshow);
            if (subverse == null) return View("~/Views/Errors/Subversenotfound.cshtml");
            ViewBag.Title = subverse.description;

            // subverse is adult rated, check if user wants to see NSFW content
            PaginatedList<Message> paginatedSubmissionsByRank;
            if (subverse.rated_adult)
            {
                if (User.Identity.IsAuthenticated)
                {
                    // check if user wants to see NSFW content by reading user preference
                    if (Utils.User.AdultContentEnabled(User.Identity.Name))
                    {
                        if (sortingmode.Equals("new"))
                        {
                            var paginatedSubmissionsByDate =
                                new PaginatedList<Message>(SubmissionsFromASubverseByDate(subversetoshow), page ?? 0,
                                    pageSize);
                            return View("SubverseIndex", paginatedSubmissionsByDate);
                        }

                        if (sortingmode.Equals("top"))
                        {
                            var paginatedSubmissionsByDate =
                                new PaginatedList<Message>(SubmissionsFromASubverseByTop(subversetoshow), page ?? 0,
                                    pageSize);
                            return View("SubverseIndex", paginatedSubmissionsByDate);
                        }

                        // default sorting mode by rank
                        paginatedSubmissionsByRank = new PaginatedList<Message>(SubmissionsFromASubverseByRank(subversetoshow), page ?? 0,
                            pageSize);
                        return View("SubverseIndex", paginatedSubmissionsByRank);
                    }
                    return RedirectToAction("AdultContentFiltered", "Subverses", new { destination = subverse.name });
                }
                else
                {
                    // check if user wants to see NSFW content by reading NSFW cookie
                    if (!HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
                    {
                        return RedirectToAction("AdultContentWarning", "Subverses",
                            new { destination = subverse.name, nsfwok = false });
                    }
                    else
                    {
                        if (sortingmode.Equals("new"))
                        {
                            var paginatedSubmissionsByDate =
                                new PaginatedList<Message>(SubmissionsFromASubverseByDate(subversetoshow), page ?? 0,
                                    pageSize);
                            return View("SubverseIndex", paginatedSubmissionsByDate);
                        }

                        if (sortingmode.Equals("top"))
                        {
                            var paginatedSubmissionsByDate =
                                new PaginatedList<Message>(SubmissionsFromASubverseByTop(subversetoshow), page ?? 0,
                                    pageSize);
                            return View("SubverseIndex", paginatedSubmissionsByDate);
                        }

                        // default sorting mode by rank
                        paginatedSubmissionsByRank = new PaginatedList<Message>(SubmissionsFromASubverseByRank(subversetoshow), page ?? 0, pageSize);
                        return View("SubverseIndex", paginatedSubmissionsByRank);
                    }
                }
            }

            // subverse is safe for work
            if (sortingmode.Equals("new"))
            {
                var paginatedSubmissionsByDate =
                    new PaginatedList<Message>(SubmissionsFromASubverseByDate(subversetoshow), page ?? 0, pageSize);
                return View("SubverseIndex", paginatedSubmissionsByDate);
            }

            if (sortingmode.Equals("top"))
            {
                var paginatedSubmissionsByDate =
                    new PaginatedList<Message>(SubmissionsFromASubverseByTop(subversetoshow), page ?? 0, pageSize);
                return View("SubverseIndex", paginatedSubmissionsByDate);
            }

            // default sorting mode by rank
            paginatedSubmissionsByRank = new PaginatedList<Message>(SubmissionsFromASubverseByRank(subversetoshow), page ?? 0, pageSize);
            return View("SubverseIndex", paginatedSubmissionsByRank);
        }

        [ChildActionOnly]
        private ActionResult HandleSortedSubverseAll(int? page, string sortingmode)
        {
            const string cookieName = "NSFWEnabled";
            const int pageSize = 25;

            PaginatedList<Message> paginatedSubmissions;
            if (User.Identity.IsAuthenticated)
            {
                // check if user wants to see NSFW content by reading user preference
                if (Utils.User.AdultContentEnabled(User.Identity.Name))
                {
                    if (sortingmode.Equals("new"))
                    {
                        paginatedSubmissions = new PaginatedList<Message>(SubmissionsFromAllSubversesByDate(), page ?? 0, pageSize);
                        return View("SubverseIndex", paginatedSubmissions);
                    }

                    if (sortingmode.Equals("top"))
                    {
                        paginatedSubmissions = new PaginatedList<Message>(SubmissionsFromAllSubversesByTop(), page ?? 0, pageSize);
                        return View("SubverseIndex", paginatedSubmissions);
                    }

                    // default sorting mode by rank
                    paginatedSubmissions = new PaginatedList<Message>(SubmissionsFromAllSubversesByRank(), page ?? 0, pageSize);
                    return View("SubverseIndex", paginatedSubmissions);

                }

                // user does not want to see NSFW content
                if (sortingmode.Equals("new"))
                {
                    paginatedSubmissions = new PaginatedList<Message>(SfwSubmissionsFromAllSubversesByDate(), page ?? 0, pageSize);
                    return View("SubverseIndex", paginatedSubmissions);
                }
                if (sortingmode.Equals("top"))
                {
                    paginatedSubmissions = new PaginatedList<Message>(SfwSubmissionsFromAllSubversesByTop(), page ?? 0, pageSize);
                    return View("SubverseIndex", paginatedSubmissions);
                }
                // default sorting mode by rank
                paginatedSubmissions = new PaginatedList<Message>(SfwSubmissionsFromAllSubversesByRank(), page ?? 0, pageSize);
                return View("SubverseIndex", paginatedSubmissions);

            }

            // guest users: check if user wants to see NSFW content by reading NSFW cookie
            if (!HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
            {
                if (sortingmode.Equals("new"))
                {
                    paginatedSubmissions = new PaginatedList<Message>(SfwSubmissionsFromAllSubversesByDate(), page ?? 0, pageSize);
                    return View("SubverseIndex", paginatedSubmissions);
                }
                if (sortingmode.Equals("top"))
                {
                    paginatedSubmissions = new PaginatedList<Message>(SfwSubmissionsFromAllSubversesByTop(), page ?? 0, pageSize);
                    return View("SubverseIndex", paginatedSubmissions);
                }

                // default sorting mode by rank
                paginatedSubmissions = new PaginatedList<Message>(SfwSubmissionsFromAllSubversesByRank(), page ?? 0, pageSize);
                return View("SubverseIndex", paginatedSubmissions);
            }

            if (sortingmode.Equals("new"))
            {
                paginatedSubmissions = new PaginatedList<Message>(SubmissionsFromAllSubversesByDate(), page ?? 0, pageSize);
                return View("SubverseIndex", paginatedSubmissions);
            }
            if (sortingmode.Equals("top"))
            {
                paginatedSubmissions = new PaginatedList<Message>(SubmissionsFromAllSubversesByTop(), page ?? 0, pageSize);
                return View("SubverseIndex", paginatedSubmissions);
            }

            // default sorting mode by rank
            paginatedSubmissions = new PaginatedList<Message>(SubmissionsFromAllSubversesByRank(), page ?? 0, pageSize);
            return View("SubverseIndex", paginatedSubmissions);

        }


        #region sfw submissions from all subverses
        private IQueryable<Message> SfwSubmissionsFromAllSubversesByDate()
        {
            IQueryable<Message> sfwSubmissionsFromAllSubversesByDate = (from message in _db.Messages
                                                                        join subverse in _db.Subverses on message.Subverse equals subverse.name
                                                                        where message.Name != "deleted" && subverse.private_subverse != true && subverse.rated_adult == false
                                                                        select message
                                                                        ).OrderByDescending(s => s.Date).AsNoTracking();

            return sfwSubmissionsFromAllSubversesByDate;
        }

        private IQueryable<Message> SfwSubmissionsFromAllSubversesByRank()
        {
            IQueryable<Message> sfwSubmissionsFromAllSubversesByRank = (from message in _db.Messages
                                                                        join subverse in _db.Subverses on message.Subverse equals subverse.name
                                                                        where message.Name != "deleted" && subverse.private_subverse != true && subverse.rated_adult == false && message.Rank > 0.00009
                                                                        select message).OrderByDescending(s => s.Rank).AsNoTracking();

            return sfwSubmissionsFromAllSubversesByRank;
        }

        private IQueryable<Message> SfwSubmissionsFromAllSubversesByTop()
        {
            IQueryable<Message> sfwSubmissionsFromAllSubversesByTop = (from message in _db.Messages
                                                                       join subverse in _db.Subverses on message.Subverse equals subverse.name
                                                                       where message.Name != "deleted" && subverse.private_subverse != true && subverse.rated_adult == false
                                                                       select message).OrderByDescending(s => s.Likes - s.Dislikes).AsNoTracking();


            return sfwSubmissionsFromAllSubversesByTop;
        }
        #endregion

        #region unfiltered submissions from all subverses
        private IQueryable<Message> SubmissionsFromAllSubversesByDate()
        {
            IQueryable<Message> submissionsFromAllSubversesByDate = (from message in _db.Messages
                                                                     join subverse in _db.Subverses on message.Subverse equals subverse.name
                                                                     where message.Name != "deleted" && subverse.private_subverse != true
                                                                     select message).OrderByDescending(s => s.Date).AsNoTracking();

            return submissionsFromAllSubversesByDate;
        }

        private IQueryable<Message> SubmissionsFromAllSubversesByRank()
        {
            IQueryable<Message> submissionsFromAllSubversesByRank = (from message in _db.Messages
                                                                     join subverse in _db.Subverses on message.Subverse equals subverse.name
                                                                     where message.Name != "deleted" && subverse.private_subverse != true && message.Rank > 0.00009
                                                                     select message).OrderByDescending(s => s.Rank).AsNoTracking();

            return submissionsFromAllSubversesByRank;
        }

        private IQueryable<Message> SubmissionsFromAllSubversesByTop()
        {
            IQueryable<Message> submissionsFromAllSubversesByTop = (from message in _db.Messages
                                                                    join subverse in _db.Subverses on message.Subverse equals subverse.name
                                                                    where message.Name != "deleted" && subverse.private_subverse != true
                                                                    select message).OrderByDescending(s => s.Likes - s.Dislikes).AsNoTracking();

            return submissionsFromAllSubversesByTop;
        }
        #endregion

        #region submissions from a single subverse
        private IQueryable<Message> SubmissionsFromASubverseByDate(string subverseName)
        {
            IQueryable<Message> submissionsFromASubverseByDate = (from message in _db.Messages
                                                                  join subverse in _db.Subverses on message.Subverse equals subverse.name
                                                                  where message.Name != "deleted" && message.Subverse == subverseName
                                                                  select message).OrderByDescending(s => s.Date).AsNoTracking();

            return submissionsFromASubverseByDate;
        }

        private IQueryable<Message> SubmissionsFromASubverseByRank(string subverseName)
        {
            IQueryable<Message> submissionsFromASubverseByRank = (from message in _db.Messages
                                                                  join subverse in _db.Subverses on message.Subverse equals subverse.name
                                                                  where message.Name != "deleted" && message.Subverse == subverseName
                                                                  select message).OrderByDescending(s => s.Rank).AsNoTracking();

            return submissionsFromASubverseByRank;
        }

        private IQueryable<Message> SubmissionsFromASubverseByTop(string subverseName)
        {
            IQueryable<Message> submissionsFromASubverseByTop = (from message in _db.Messages
                                                                 join subverse in _db.Subverses on message.Subverse equals subverse.name
                                                                 where message.Name != "deleted" && message.Subverse == subverseName
                                                                 select message).OrderByDescending(s => s.Likes - s.Dislikes).AsNoTracking();

            return submissionsFromASubverseByTop;
        }
        #endregion
    }

}