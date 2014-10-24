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
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whoaverse.Models;
using Whoaverse.Models.ViewModels;
using Whoaverse.Utils;

namespace Whoaverse.Controllers
{
    public class SubversesController : Controller
    {
        private whoaverseEntities db = new whoaverseEntities();
        Random rnd = new Random();

        // GET: sidebar for selected subverse
        public ActionResult SidebarForSelectedSubverseComments(string selectedSubverse, bool showingComments, string name, DateTime? date, DateTime? lastEditDate, int? submissionId, int? likes, int? dislikes, bool anonymized)
        {
            Subverse subverse = db.Subverses.Find(selectedSubverse);            

            if (subverse != null)
            {
                // get subscriber count for selected subverse
                int subscriberCount = db.Subscriptions.AsEnumerable()
                                    .Where(r => r.SubverseName.Equals(selectedSubverse, StringComparison.OrdinalIgnoreCase))
                                    .Count();

                ViewBag.SubscriberCount = subscriberCount;
                ViewBag.SelectedSubverse = selectedSubverse;

                if (showingComments)
                {
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

                    try
                    {
                        ViewBag.OnlineUsers = SessionTracker.ActiveSessionsForSubverse(selectedSubverse);
                    }
                    catch (Exception)
                    {
                        ViewBag.OnlineUsers = -1;
                    }

                    return PartialView("~/Views/Shared/Sidebars/_SidebarComments.cshtml", subverse);
                }
                else
                {
                    return new EmptyResult();
                }

            }
            else
            {
                //don't return a sidebar since subverse doesn't exist or is a system subverse
                return new EmptyResult();
            }
        }

        // GET: sidebar for selected subverse
        public ActionResult SidebarForSelectedSubverse(string selectedSubverse)
        {
            Subverse subverse = db.Subverses.Find(selectedSubverse);

            if (subverse != null)
            {
                // get subscriber count for selected subverse
                int subscriberCount = db.Subscriptions.AsEnumerable()
                                    .Where(r => r.SubverseName.Equals(selectedSubverse, StringComparison.OrdinalIgnoreCase))
                                    .Count();

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
            }
            else
            {
                //don't return a sidebar since subverse doesn't exist or is a system subverse
                return new EmptyResult();
            }
        }

        // GET: stylesheet for selected subverse
        public ActionResult StylesheetForSelectedSubverse(string selectedSubverse)
        {
            var subverse = db.Subverses.FirstOrDefault(i => i.name == selectedSubverse);

            if (subverse != null)
            {
                return Content(subverse.stylesheet);
            }
            else
            {
                return Content(string.Empty);
            }
        }

        // GET: comments for a given submission
        public ActionResult Comments(int? id, string subversetoshow)
        {
            var subverse = db.Subverses.Find(subversetoshow);

            if (subverse != null)
            {
                ViewBag.SelectedSubverse = subverse.name;

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Message message = db.Messages.Find(id);
                if (message == null)
                {
                    return View("~/Views/Errors/Error_404.cshtml");
                }
                // make sure that the combination of selected subverse and message subverse are linked
                if (!message.Subverse.Equals(subversetoshow, StringComparison.OrdinalIgnoreCase))
                {
                    return View("~/Views/Errors/Error_404.cshtml");
                }
                return View(message);
            }
            else
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }
        }

        // GET: submit
        [Authorize]
        public ActionResult Submit()
        {
            return View();
        }

        // POST: submit a new submission
        [Authorize]
        [HttpPost]
        [PreventSpam(DelayRequest = 60, ErrorMessage = "Sorry, you are doing that too fast. Please try again in 60 seconds.")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit([Bind(Include = "Id,Votes,Name,Date,Type,Linkdescription,Title,Rank,MessageContent")] Message message)
        {
            // check if user is banned
            if (Utils.User.IsUserBanned(message.Name))
            {
                ViewBag.SelectedSubverse = message.Subverse;
                return View("~/Views/Home/Comments.cshtml", message);
            }

            // verify recaptcha if user has less than 25 CCP
            if (Whoaverse.Utils.Karma.CommentKarma(User.Identity.Name) < 25)
            {
                // begin recaptcha check
                bool isCaptchaCodeValid = false;
                string CaptchaMessage = "";
                isCaptchaCodeValid = Whoaverse.Utils.ReCaptchaUtility.GetCaptchaResponse(CaptchaMessage, Request);

                if (!isCaptchaCodeValid)
                {
                    ModelState.AddModelError("", "Incorrect recaptcha answer.");
                    return View();
                }
                // end recaptcha check
            }

            if (ModelState.IsValid)
            {
                var targetSubverse = db.Subverses.Find(message.Subverse.Trim());

                if (targetSubverse != null)
                {
                    // check if subverse has "authorized_submitters_only" set and dissalow submission if user is not allowed submitter
                    if (targetSubverse.authorized_submitters_only)
                    {
                        if (!Whoaverse.Utils.User.IsUserSubverseModerator(User.Identity.Name, targetSubverse.name))
                        {
                            // user is not a moderator, check if user is an administrator
                            if (!Whoaverse.Utils.User.IsUserSubverseAdmin(User.Identity.Name, targetSubverse.name))
                            {
                                ModelState.AddModelError("", "You are not authorized to submit links or start discussions in this subverse. Please contact subverse moderators for authorization.");
                                return View();
                            }
                        }
                    }

                    // accept submission and save it to the database
                    message.Subverse = targetSubverse.name;
                    message.Date = System.DateTime.Now;
                    // flag the submission as anonymized if it was submitted to a subverse with active anonymized_mode
                    if (targetSubverse.anonymized_mode)
                    {
                        message.Anonymized = true;
                        message.Name = User.Identity.Name;
                    }
                    else
                    {
                        message.Name = User.Identity.Name;
                    }
                    
                    db.Messages.Add(message);
                    await db.SaveChangesAsync();

                    // get newly generated submission ID and execute ranking and self upvoting                
                    Votingtracker tmpVotingTracker = new Votingtracker();
                    tmpVotingTracker.MessageId = message.Id;
                    tmpVotingTracker.UserName = message.Name;
                    tmpVotingTracker.VoteStatus = 1;
                    db.Votingtrackers.Add(tmpVotingTracker);
                    await db.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Sorry, you are doing that too fast. Please try again in a few minutes.");
                return View(message);
            }
        }

        // POST: Create a new Subverse
        // To protect from overposting attacks, enable the specific properties you want to bind to 
        [HttpPost]
        [PreventSpam(DelayRequest = 300, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateSubverse([Bind(Include = "Name, Title, Description, Type, Sidebar, Creation_date, Owner")] AddSubverse subverseTmpModel)
        {
            if (User.Identity.IsAuthenticated)
            {
                // verify recaptcha if user has less than 25 CCP
                if (Whoaverse.Utils.Karma.CommentKarma(User.Identity.Name) < 25)
                {
                    // begin recaptcha check
                    bool isCaptchaCodeValid = false;
                    string CaptchaMessage = "";
                    isCaptchaCodeValid = Whoaverse.Utils.ReCaptchaUtility.GetCaptchaResponse(CaptchaMessage, Request);

                    if (!isCaptchaCodeValid)
                    {
                        ModelState.AddModelError("", "Incorrect recaptcha answer.");
                        return View();
                    }
                    // end recaptcha check
                }

                try
                {
                    if (ModelState.IsValid)
                    {
                        Subverse subverse = new Subverse();
                        subverse.name = subverseTmpModel.Name;
                        subverse.title = "/v/" + subverseTmpModel.Name;
                        subverse.description = subverseTmpModel.Description;
                        subverse.sidebar = subverseTmpModel.Sidebar;
                        subverse.creation_date = System.DateTime.Now;

                        // setup default values
                        subverse.type = "link";
                        subverse.enable_thumbnails = true;
                        subverse.rated_adult = false;
                        subverse.private_subverse = false;

                        // check if subverse exists before attempting to create it
                        if (db.Subverses.Find(subverse.name) == null)
                        {
                            // only allow users with less than 10 subverses to create a subverse
                            var amountOfOwnedSubverses = db.SubverseAdmins
                                .Where(s => s.Username == User.Identity.Name && s.Power == 1)
                                .ToList();

                            if (amountOfOwnedSubverses != null)
                            {
                                if (amountOfOwnedSubverses.Count < 11)
                                {
                                    db.Subverses.Add(subverse);
                                    await db.SaveChangesAsync();

                                    //register user as the owner of the newly created subverse
                                    SubverseAdmin tmpSubverseAdmin = new SubverseAdmin();
                                    tmpSubverseAdmin.SubverseName = subverse.name;
                                    tmpSubverseAdmin.Username = User.Identity.Name;
                                    tmpSubverseAdmin.Power = 1;
                                    db.SubverseAdmins.Add(tmpSubverseAdmin);
                                    await db.SaveChangesAsync();

                                    //subscribe user to the newly created subverse
                                    Whoaverse.Utils.User.SubscribeToSubverse(subverseTmpModel.Owner, subverse.name);

                                    //go to newly created Subverse
                                    return RedirectToAction("Index", "Subverses", new { subversetoshow = subverse.name });
                                }
                                else
                                {
                                    ModelState.AddModelError(string.Empty, "Sorry, you can not own more than 10 subverses.");
                                    return View();
                                }
                            }
                            else
                            {
                                db.Subverses.Add(subverse);
                                await db.SaveChangesAsync();

                                //register user as the owner of the newly created subverse
                                SubverseAdmin tmpSubverseAdmin = new SubverseAdmin();
                                tmpSubverseAdmin.SubverseName = subverse.name;
                                tmpSubverseAdmin.Username = User.Identity.Name;
                                tmpSubverseAdmin.Power = 1;
                                db.SubverseAdmins.Add(tmpSubverseAdmin);
                                await db.SaveChangesAsync();

                                //subscribe user to the newly created subverse
                                Whoaverse.Utils.User.SubscribeToSubverse(subverseTmpModel.Owner, subverse.name);

                                //go to newly created Subverse
                                return RedirectToAction("Index", "Subverses", new { subversetoshow = subverse.name });
                            }

                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Sorry, The subverse you are trying to create already exists, but you can try to claim it by submitting a takeover request to /v/subverserequest.");
                            return View();
                        }
                    }
                    else
                    {
                        return View();
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "Something bad happened.");
                    return View();
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
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
            Subverse subverse = db.Subverses.Find(subversetoshow);
            if (subverse == null)
            {
                ViewBag.SelectedSubverse = "404";
                return View("~/Views/Errors/Subversenotfound.cshtml");
            }

            // check that the user requesting to edit subverse settings is subverse owner!
            SubverseAdmin subAdmin = db.SubverseAdmins
                        .Where(x => x.SubverseName == subversetoshow && x.Username == User.Identity.Name && x.Power <= 2).FirstOrDefault();

            if (subAdmin != null)
            {
                // map existing data to view model for editing and pass it to frontend
                // NOTE: we should look into a mapper which automatically maps these properties to corresponding fields to avoid tedious manual mapping
                SubverseSettingsViewModel viewModel = new SubverseSettingsViewModel();

                viewModel.Name = subverse.name;
                viewModel.Type = subverse.type;
                viewModel.Submission_text = subverse.submission_text;
                viewModel.Description = subverse.description;
                viewModel.Sidebar = subverse.sidebar;
                viewModel.Stylesheet = subverse.stylesheet;
                viewModel.Allow_default = subverse.allow_default;
                viewModel.Label_submit_new_link = subverse.label_submit_new_link;
                viewModel.Label_sumit_new_selfpost = subverse.label_sumit_new_selfpost;
                viewModel.Rated_adult = subverse.rated_adult;
                viewModel.Private_subverse = subverse.private_subverse;
                viewModel.Enable_thumbnails = subverse.enable_thumbnails;
                viewModel.Authorized_submitters_only = subverse.authorized_submitters_only;
                viewModel.Anonymized_mode = subverse.anonymized_mode;

                ViewBag.SelectedSubverse = string.Empty;
                ViewBag.SubverseName = subverse.name;
                return View("~/Views/Subverses/Admin/SubverseSettings.cshtml", viewModel);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
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
                if (ModelState.IsValid)
                {
                    var existingSubverse = db.Subverses.Find(updatedModel.name);

                    // check if subverse exists before attempting to edit it
                    if (existingSubverse != null)
                    {
                        // check if user requesting edit is authorized to do so for current subverse
                        // check that the user requesting to edit subverse settings is subverse owner!
                        SubverseAdmin subAdmin = db.SubverseAdmins
                                    .Where(x => x.SubverseName == updatedModel.name && x.Username == User.Identity.Name && x.Power <= 2).FirstOrDefault();

                        if (subAdmin != null)
                        {
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

                            if (existingSubverse.anonymized_mode == true && updatedModel.anonymized_mode == false)
                            {
                                ModelState.AddModelError(string.Empty, "Sorry, this subverse is permanently locked to anonymized mode.");
                                return View("~/Views/Subverses/Admin/SubverseSettings.cshtml");
                            }

                            existingSubverse.anonymized_mode = updatedModel.anonymized_mode;

                            await db.SaveChangesAsync();

                            // go back to this subverse
                            return RedirectToAction("Index", "Subverses", new { subversetoshow = updatedModel.name });
                        }
                        else
                        {
                            // user was not authorized to commit the changes, drop attempt
                            return new EmptyResult();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Sorry, The subverse you are trying to edit does not exist.");
                        return View("~/Views/Subverses/Admin/SubverseSettings.cshtml");
                    }
                }
                else
                {
                    return View("~/Views/Subverses/Admin/SubverseSettings.cshtml");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Something bad happened.");
                return View("~/Views/Subverses/Admin/SubverseSettings.cshtml");
            }
        }

        // GET: show a subverse index
        public ActionResult Index(int? page, string subversetoshow)
        {
            int pageSize = 25;
            int pageNumber = (page ?? 1);

            if (pageNumber < 1)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            string cookieName = "NSFWEnabled";

            if (subversetoshow == null)
            {
                return View("~/Views/Errors/Subversenotfound.cshtml");
            }

            // register a new session for this subverse
            try
            {
                string currentSubverse = (string)this.RouteData.Values["subversetoshow"];
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
                    Subverse subverse = db.Subverses.Find(subversetoshow);
                    if (subverse != null)
                    {
                        ViewBag.SelectedSubverse = subverse.name;
                        ViewBag.Title = subverse.description;

                        var submissions = db.Messages
                                            .Where(x => x.Subverse == subversetoshow && x.Name != "deleted")
                                            .OrderByDescending(s => s.Rank);

                        // check if subverse is rated adult, show a NSFW warning page before entering
                        if (subverse.rated_adult == true)
                        {
                            // check if user wants to see NSFW content by reading user preference
                            if (User.Identity.IsAuthenticated)
                            {
                                if (Whoaverse.Utils.User.AdultContentEnabled(User.Identity.Name))
                                {
                                    return View(submissions.ToPagedList(pageNumber, pageSize));
                                }
                                else
                                {
                                    // display a view explaining that account preference is set to NO NSFW and why this subverse can not be shown
                                    return RedirectToAction("AdultContentFiltered", "Subverses", new { destination = subverse.name });
                                }
                            }
                            else
                            {
                                // check if user wants to see NSFW content by reading NSFW cookie
                                if (!this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
                                {
                                    return RedirectToAction("AdultContentWarning", "Subverses", new { destination = subverse.name, nsfwok = false });
                                }
                                else
                                {
                                    return View(submissions.ToPagedList(pageNumber, pageSize));
                                }
                            }
                        }
                        else
                        {
                            return View(submissions.ToPagedList(pageNumber, pageSize));
                        }
                    }
                    else
                    {
                        ViewBag.SelectedSubverse = "404";
                        return View("~/Views/Errors/Subversenotfound.cshtml");
                    }
                }
                else
                {
                    // selected subverse is ALL, show submissions from all subverses, sorted by rank
                    ViewBag.SelectedSubverse = "all";
                    ViewBag.Title = "all subverses";

                    var submissions = db.Messages
                                        .Where(x => x.Name != "deleted")
                                        .OrderByDescending(s => s.Rank);

                    // check if user wants to see NSFW content by reading user preference
                    if (User.Identity.IsAuthenticated)
                    {
                        if (Whoaverse.Utils.User.AdultContentEnabled(User.Identity.Name))
                        {
                            // display adult content
                            return View(submissions.ToPagedList(pageNumber, pageSize));
                        }
                        else
                        {
                            // filter adult content
                            var sfwsubmissions = db.Messages
                                                .Where(x => x.Name != "deleted" && x.Subverses.rated_adult == false)
                                                .OrderByDescending(s => s.Rank);

                            return View(sfwsubmissions.ToPagedList(pageNumber, pageSize));
                        }
                    }
                    else
                    {
                        // check if user wants to see NSFW content by reading NSFW cookie
                        if (!this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
                        {
                            // filter adult content
                            var sfwsubmissions = db.Messages
                                                .Where(x => x.Name != "deleted" && x.Subverses.rated_adult == false)
                                                .OrderByDescending(s => s.Rank);

                            return View(sfwsubmissions.ToPagedList(pageNumber, pageSize));
                        }
                        else
                        {
                            // display adult content
                            return View(submissions.ToPagedList(pageNumber, pageSize));
                        }
                    }
                }
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        public ActionResult Subverses(int? page)
        {
            ViewBag.SelectedSubverse = "subverses";
            int pageSize = 25;
            int pageNumber = (page ?? 1);

            if (pageNumber < 1)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            try
            {
                // order by subscriber count (popularity)
                var subverses = db.Subverses
                    .OrderByDescending(s => s.subscribers);

                return View(subverses.ToPagedList(pageNumber, pageSize));
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
            int pageSize = 25;
            int pageNumber = (page ?? 1);

            if (pageNumber < 1)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // get a list of subcribed subverses with details and order by subverse names, ascending
            var subscribedSubverses = from c in db.Subverses
                                      join a in db.Subscriptions
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

            return View("SubscribedSubverses", subscribedSubverses.ToPagedList(pageNumber, pageSize));
        }

        // GET: sidebar for selected subverse
        public ActionResult DetailsForSelectedSubverse(string selectedSubverse)
        {
            Subverse subverse = db.Subverses.Find(selectedSubverse);

            if (subverse != null)
            {
                // get subscriber count for selected subverse
                int subscriberCount = db.Subscriptions
                                    .Where(r => r.SubverseName.Equals(selectedSubverse, StringComparison.OrdinalIgnoreCase))
                                    .Count();

                ViewBag.SubscriberCount = subscriberCount;
                ViewBag.SelectedSubverse = selectedSubverse;
                return PartialView("_SubverseDetails", subverse);
            }
            else
            {
                //don't return a sidebar since subverse doesn't exist or is a system subverse
                return new EmptyResult();
            }
        }

        public ViewResult NewestSubverses(int? page, string sortingmode)
        {
            ViewBag.SelectedSubverse = "subverses";
            ViewBag.SortingMode = sortingmode;

            int pageSize = 25;
            int pageNumber = (page ?? 1);

            if (pageNumber < 1)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            var subverses = db.Subverses
                .Where(s => s.description != null && s.sidebar != null)
                .OrderByDescending(s => s.creation_date);

            return View("~/Views/Subverses/Subverses.cshtml", subverses.ToPagedList(pageNumber, pageSize));
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

            if (destination != null)
            {
                if (nsfwok != null && nsfwok == true)
                {
                    // setup nswf cookie
                    HttpCookie cookie = new HttpCookie("NSFWEnabled");
                    cookie.Value = "whoaverse nsfw warning cookie";
                    cookie.Expires = DateTime.Now.AddDays(1);
                    this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);

                    // redirect to destination subverse
                    return RedirectToAction("Index", "Subverses", new { subversetoshow = destination });
                }
                else
                {
                    ViewBag.Destination = destination;
                    return View("~/Views/Subverses/AdultContentWarning.cshtml");
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        public ActionResult SortedSubverseFrontpage(int? page, string subversetoshow, string sortingmode)
        {
            //sortingmode: new, contraversial, hot, etc
            ViewBag.SortingMode = sortingmode;
            ViewBag.SelectedSubverse = subversetoshow;

            string cookieName = "NSFWEnabled";

            if (sortingmode.Equals("new") || sortingmode.Equals("top"))
            {
                int pageSize = 25;
                int pageNumber = (page ?? 1);

                if (pageNumber < 1)
                {
                    return View("~/Views/Errors/Error_404.cshtml");
                }

                ViewBag.Title = subversetoshow;

                // register a new session for this subverse                
                try
                {
                    string currentSubverse = (string)this.RouteData.Values["subversetoshow"];
                    SessionTracker.Add(currentSubverse, Session.SessionID);
                    ViewBag.OnlineUsers = SessionTracker.ActiveSessionsForSubverse(currentSubverse);
                }
                catch (Exception)
                {
                    ViewBag.OnlineUsers = -1;
                }

                if (!subversetoshow.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    // check if subverse exists, if not, send to a page not found error
                    Subverse subverse = db.Subverses.Find(subversetoshow);
                    if (subverse != null)
                    {
                        ViewBag.Title = subverse.description;

                        IQueryable<Message> submissions;

                        if (sortingmode.Equals("new"))
                        {
                            submissions = db.Messages
                                            .Where(x => x.Subverse == subversetoshow && x.Name != "deleted")
                                            .OrderByDescending(s => s.Date);
                        }
                        else
                        {
                            submissions = db.Messages
                                            .Where(x => x.Subverse == subversetoshow && x.Name != "deleted")
                                            .OrderByDescending(s => s.Likes - s.Dislikes);
                        }

                        if (subverse.rated_adult == true)
                        {
                            if (User.Identity.IsAuthenticated)
                            {
                                // check if user wants to see NSFW content by reading user preference
                                if (Whoaverse.Utils.User.AdultContentEnabled(User.Identity.Name))
                                {
                                    return View("Index", submissions.ToPagedList(pageNumber, pageSize));
                                }
                                else
                                {
                                    return RedirectToAction("AdultContentFiltered", "Subverses", new { destination = subverse.name });
                                }
                            }
                            else
                            {
                                // check if user wants to see NSFW content by reading NSFW cookie
                                if (!this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
                                {
                                    return RedirectToAction("AdultContentWarning", "Subverses", new { destination = subverse.name, nsfwok = false });
                                }
                                else
                                {
                                    return View("Index", submissions.ToPagedList(pageNumber, pageSize));
                                }
                            }
                        }
                        else
                        {
                            return View("Index", submissions.ToPagedList(pageNumber, pageSize));
                        }
                    }
                    else
                    {
                        return View("~/Views/Errors/Subversenotfound.cshtml");
                    }
                }
                else
                {
                    // selected subverse is ALL, show submissions from all subverses, sorted by date
                    IQueryable<Message> submissions;

                    if (sortingmode.Equals("new"))
                    {
                        submissions = db.Messages
                                        .Where(x => x.Name != "deleted" && x.Subverses.private_subverse != true)
                                        .OrderByDescending(s => s.Date);
                    }
                    else
                    {
                        submissions = db.Messages
                                        .Where(x => x.Name != "deleted" && x.Subverses.private_subverse != true)
                                        .OrderByDescending(s => s.Likes - s.Dislikes);
                    }

                    // check if user wants to see NSFW content by reading user preference
                    if (User.Identity.IsAuthenticated)
                    {
                        if (Whoaverse.Utils.User.AdultContentEnabled(User.Identity.Name))
                        {
                            return View("Index", submissions.ToPagedList(pageNumber, pageSize));
                        }
                        else
                        {
                            IQueryable<Message> sfwsubmissions;

                            if (sortingmode.Equals("new"))
                            {
                                sfwsubmissions = db.Messages
                                        .Where(x => x.Name != "deleted" && x.Subverses.private_subverse != true && x.Subverses.rated_adult == false)
                                        .OrderByDescending(s => s.Date);
                            }
                            else
                            {
                                sfwsubmissions = db.Messages
                                        .Where(x => x.Name != "deleted" && x.Subverses.private_subverse != true && x.Subverses.rated_adult == false)
                                        .OrderByDescending(s => s.Likes - s.Dislikes);
                            }

                            return View("Index", sfwsubmissions.ToPagedList(pageNumber, pageSize));
                        }
                    }
                    else
                    {
                        // check if user wants to see NSFW content by reading NSFW cookie
                        if (!this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(cookieName))
                        {
                            IQueryable<Message> sfwsubmissions;

                            if (sortingmode.Equals("new"))
                            {
                                sfwsubmissions = db.Messages
                                    .Where(x => x.Name != "deleted" && x.Subverses.private_subverse != true && x.Subverses.rated_adult == false)
                                    .OrderByDescending(s => s.Date);
                            }
                            else
                            {
                                sfwsubmissions = db.Messages
                                    .Where(x => x.Name != "deleted" && x.Subverses.private_subverse != true && x.Subverses.rated_adult == false)
                                    .OrderByDescending(s => s.Likes - s.Dislikes);
                            }

                            return View("Index", sfwsubmissions.ToPagedList(pageNumber, pageSize));
                        }
                        else
                        {
                            return View("Index", submissions.ToPagedList(pageNumber, pageSize));
                        }
                    }
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // fetch a random subbverse with x subscribers and x submissions
        public ActionResult Random()
        {
            try
            {
                // fetch a random subverse with minimum number of subscribers
                var subverse = from subverses in db.Subverses
                          .Where(s => s.subscribers > 10 && !s.name.Equals("all", StringComparison.OrdinalIgnoreCase))
                               select subverses;

                int submissionCount = 0;
                Subverse randomSubverse;

                do
                {
                    int count = subverse.Count(); // 1st round-trip
                    int index = new Random().Next(count);

                    randomSubverse = subverse.OrderBy(s => s.name).Skip(index).FirstOrDefault(); // 2nd round-trip

                    var submissions = db.Messages
                            .Where(x => x.Subverse == randomSubverse.name && x.Name != "deleted")
                            .OrderByDescending(s => s.Rank)
                            .Take(50)
                            .ToList();

                    if (submissions != null)
                    {
                        if (submissions.Count > 9)
                        {
                            submissionCount = submissions.Count;
                        }
                    }

                } while (submissionCount == 0);

                return RedirectToAction("Index", "Subverses", new { subversetoshow = randomSubverse.name });
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
            var subverseModel = db.Subverses.Find(subversetoshow);

            if (subverseModel != null)
            {
                // check if caller is subverse owner, if not, deny listing
                if (Whoaverse.Utils.User.IsUserSubverseAdmin(User.Identity.Name, subversetoshow))
                {
                    var subverseModerators = db.SubverseAdmins
                    .Where(n => n.SubverseName == subversetoshow)
                    .Take(20)
                    .OrderBy(s => s.Power)
                    .ToList();

                    ViewBag.SubverseModel = subverseModel;
                    ViewBag.SubverseName = subversetoshow;

                    ViewBag.SelectedSubverse = string.Empty;
                    return View("~/Views/Subverses/Admin/SubverseModerators.cshtml", subverseModerators);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

        }

        // GET: show add moderators view for selected subverse
        [Authorize]
        public ActionResult AddModerator(string subversetoshow)
        {
            // get model for selected subverse
            var subverseModel = db.Subverses.Find(subversetoshow);

            if (subverseModel != null)
            {
                // check if caller is subverse owner, if not, deny listing
                if (Whoaverse.Utils.User.IsUserSubverseAdmin(User.Identity.Name, subversetoshow))
                {
                    ViewBag.SubverseModel = subverseModel;
                    ViewBag.SubverseName = subversetoshow;
                    ViewBag.SelectedSubverse = string.Empty;
                    return View("~/Views/Subverses/Admin/AddModerator.cshtml");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // POST: add a moderator to given subverse
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddModerator([Bind(Include = "Id,SubverseName,Username,Power")] SubverseAdmin subverseAdmin)
        {
            if (ModelState.IsValid)
            {
                // get model for selected subverse
                var subverseModel = db.Subverses.Find(subverseAdmin.SubverseName);

                if (subverseModel != null)
                {
                    // check if the user being added is not already a moderator of 10 subverses
                    var currentlyModerating = db.SubverseAdmins
                        .Where(a => a.Username == subverseAdmin.Username).ToList();

                    if (currentlyModerating != null)
                    {
                        if (currentlyModerating.Count < 11)
                        {
                            // check if caller is subverse owner, if not, deny posting
                            if (Whoaverse.Utils.User.IsUserSubverseAdmin(User.Identity.Name, subverseAdmin.SubverseName))
                            {
                                // check that user is not already moderating given subverse
                                var isAlreadyModerator = db.SubverseAdmins
                                    .Where(a => a.Username == subverseAdmin.Username && a.SubverseName == subverseAdmin.SubverseName).FirstOrDefault();

                                if (isAlreadyModerator == null)
                                {
                                    subverseAdmin.SubverseName = subverseModel.name;
                                    db.SubverseAdmins.Add(subverseAdmin);
                                    db.SaveChanges();
                                    return RedirectToAction("SubverseModerators");
                                }
                                else
                                {
                                    ModelState.AddModelError(string.Empty, "Sorry, the user is already moderating this subverse.");
                                    SubverseModeratorViewModel tmpModel = new SubverseModeratorViewModel();
                                    tmpModel.Username = subverseAdmin.Username;
                                    tmpModel.Power = subverseAdmin.Power;
                                    ViewBag.SubverseModel = subverseModel;
                                    ViewBag.SubverseName = subverseAdmin.SubverseName;
                                    ViewBag.SelectedSubverse = string.Empty;
                                    return View("~/Views/Subverses/Admin/AddModerator.cshtml", tmpModel);
                                }
                            }
                            else
                            {
                                return RedirectToAction("Index", "Home");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Sorry, the user is already moderating a maximum of 10 subverses.");
                            SubverseModeratorViewModel tmpModel = new SubverseModeratorViewModel();
                            tmpModel.Username = subverseAdmin.Username;
                            tmpModel.Power = subverseAdmin.Power;
                            ViewBag.SubverseModel = subverseModel;
                            ViewBag.SubverseName = subverseAdmin.SubverseName;
                            ViewBag.SelectedSubverse = string.Empty;
                            return View("~/Views/Subverses/Admin/AddModerator.cshtml", tmpModel);
                        }
                    }
                    else
                    {
                        // check if caller is subverse owner, if not, deny posting
                        if (Whoaverse.Utils.User.IsUserSubverseAdmin(User.Identity.Name, subverseAdmin.SubverseName))
                        {
                            subverseAdmin.SubverseName = subverseModel.name;
                            db.SubverseAdmins.Add(subverseAdmin);
                            db.SaveChanges();
                            return RedirectToAction("SubverseModerators");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return View(subverseAdmin);
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

            SubverseAdmin subverseAdmin = db.SubverseAdmins.Find(id);

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

            SubverseAdmin subverseAdmin = db.SubverseAdmins
                .Where(s => s.SubverseName == subversetoresignfrom && s.Username == User.Identity.Name && s.Power > 1)
                .FirstOrDefault();

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
            var moderatorToBeRemoved = db.SubverseAdmins
                .Where(s => s.SubverseName == subversetoresignfrom && s.Username == User.Identity.Name && s.Power != 1)
                .FirstOrDefault();

            if (moderatorToBeRemoved != null)
            {
                var subverse = db.Subverses.Find(moderatorToBeRemoved.SubverseName);
                if (subverse != null)
                {
                    // execute removal                    
                    db.SubverseAdmins.Remove(moderatorToBeRemoved);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index", "Subverses", new { subversetoshow = subversetoresignfrom });
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // POST: remove a moderator from given subverse
        [Authorize]
        [HttpPost, ActionName("RemoveModerator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveModerator(int id)
        {
            // get moderator name for selected subverse
            var moderatorToBeRemoved = await db.SubverseAdmins.FindAsync(id);
            if (moderatorToBeRemoved != null)
            {
                var subverse = db.Subverses.Find(moderatorToBeRemoved.SubverseName);
                if (subverse != null)
                {
                    // check if caller has clearance to remove a moderator
                    if (Whoaverse.Utils.User.IsUserSubverseAdmin(User.Identity.Name, subverse.name) && moderatorToBeRemoved.Username != User.Identity.Name)
                    {
                        // execute removal
                        SubverseAdmin subverseAdmin = await db.SubverseAdmins.FindAsync(id);
                        db.SubverseAdmins.Remove(subverseAdmin);
                        await db.SaveChangesAsync();
                        return RedirectToAction("SubverseModerators");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // GET: show subverse flair settings view for selected subverse
        [Authorize]
        public ActionResult SubverseFlairSettings(string subversetoshow)
        {
            // get model for selected subverse
            var subverseModel = db.Subverses.Find(subversetoshow);

            if (subverseModel != null)
            {
                // check if caller is subverse owner, if not, deny listing
                if (Whoaverse.Utils.User.IsUserSubverseAdmin(User.Identity.Name, subversetoshow) || Whoaverse.Utils.User.IsUserSubverseModerator(User.Identity.Name, subversetoshow))
                {
                    var subverseFlairsettings = db.Subverseflairsettings
                    .Where(n => n.Subversename == subversetoshow)
                    .Take(20)
                    .OrderBy(s => s.Id)
                    .ToList();

                    ViewBag.SubverseModel = subverseModel;
                    ViewBag.SubverseName = subversetoshow;

                    ViewBag.SelectedSubverse = string.Empty;
                    return View("~/Views/Subverses/Admin/Flair/FlairSettings.cshtml", subverseFlairsettings);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // GET: show add link flair view for selected subverse
        [Authorize]
        public ActionResult AddLinkFlair(string subversetoshow)
        {
            // get model for selected subverse
            var subverseModel = db.Subverses.Find(subversetoshow);

            if (subverseModel != null)
            {
                // check if caller is subverse owner, if not, deny listing
                if (Whoaverse.Utils.User.IsUserSubverseAdmin(User.Identity.Name, subversetoshow) || Whoaverse.Utils.User.IsUserSubverseModerator(User.Identity.Name, subversetoshow))
                {
                    ViewBag.SubverseModel = subverseModel;
                    ViewBag.SubverseName = subversetoshow;
                    ViewBag.SelectedSubverse = string.Empty;
                    return View("~/Views/Subverses/Admin/Flair/AddLinkFlair.cshtml");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // POST: add a link flair to given subverse
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddLinkFlair([Bind(Include = "Id,Subversename,Label,CssClass")] Subverseflairsetting subverseFlairSetting)
        {
            if (ModelState.IsValid)
            {
                // get model for selected subverse
                var subverseModel = db.Subverses.Find(subverseFlairSetting.Subversename);

                if (subverseModel != null)
                {
                    // check if caller is subverse owner, if not, deny posting
                    if (Whoaverse.Utils.User.IsUserSubverseAdmin(User.Identity.Name, subverseFlairSetting.Subversename) || Whoaverse.Utils.User.IsUserSubverseModerator(User.Identity.Name, subverseFlairSetting.Subversename))
                    {
                        subverseFlairSetting.Subversename = subverseModel.name;
                        db.Subverseflairsettings.Add(subverseFlairSetting);
                        db.SaveChanges();
                        return RedirectToAction("SubverseFlairSettings");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return View(subverseFlairSetting);
            }
        }

        // GET: show remove link flair view for selected subverse
        [Authorize]
        public ActionResult RemoveLinkFlair(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Subverseflairsetting subverseFlairSetting = db.Subverseflairsettings.Find(id);

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
            var linkFlairToRemove = await db.Subverseflairsettings.FindAsync(id);
            if (linkFlairToRemove != null)
            {
                var subverse = db.Subverses.Find(linkFlairToRemove.Subversename);
                if (subverse != null)
                {
                    // check if caller has clearance to remove a link flair
                    if (Whoaverse.Utils.User.IsUserSubverseAdmin(User.Identity.Name, subverse.name) || Whoaverse.Utils.User.IsUserSubverseModerator(User.Identity.Name, subverse.name))
                    {
                        // execute removal
                        Subverseflairsetting subverseFlairSetting = await db.Subverseflairsettings.FindAsync(id);
                        db.Subverseflairsettings.Remove(subverseFlairSetting);
                        await db.SaveChangesAsync();
                        return RedirectToAction("SubverseFlairSettings");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // GET: render a partial view with list of moderators for a given subverse, if no moderators are found, return subverse owner
        [ChildActionOnly]
        public ActionResult SubverseModeratorsList(string subverseName)
        {
            // get 10 administration members for a subverse
            var subverseAdministration =
                db.SubverseAdmins
                .Where(n => n.SubverseName.Equals(subverseName, StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .ToList()
                .OrderBy(s => s.Username);

            // find all moderators with power = 2
            var subverseModerators = subverseAdministration
                .Where(m => m.Power == 2);

            // if there are no moderators with power = 2, find subverse owner with power = 1 and return him instead
            if (subverseModerators.Count() == 0)
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
            var stickiedSubmissions = db.Stickiedsubmissions
                .Where(s => s.Subversename == subverseName)
                .FirstOrDefault();

            if (stickiedSubmissions == null) return new EmptyResult();

            Message stickiedSubmission = db.Messages.Find(stickiedSubmissions.Submission_id);

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
                var listOfSubverses = db.Defaultsubverses.OrderBy(s => s.position).ToList().AsEnumerable();
                return PartialView("_ListOfDefaultSubverses", listOfSubverses);
            }
            catch (Exception)
            {
                return new EmptyResult();
            }
        }
    }
}