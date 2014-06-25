﻿/*
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
using System.Web.Mvc;
using Whoaverse.Models;
using Whoaverse.Models.ViewModels;
using Whoaverse.Utils;

namespace Whoaverse.Controllers
{
    public class SubversesController : Controller
    {
        private whoaverseEntities db = new whoaverseEntities();

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
                return PartialView("_Sidebar", subverse);
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
            ViewBag.SelectedSubverse = subversetoshow;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Message message = db.Messages.Find(id);
            if (message == null)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }
            return View(message);
        }

        // GET: submit
        [Authorize]
        public ActionResult Submit()
        {
            return View();
        }

        // POST: submit a new submission
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [PreventSpam(DelayRequest = 300, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit([Bind(Include = "Id,Votes,Name,Date,Type,Linkdescription,Title,Rank,MessageContent")] Message message)
        {
            if (ModelState.IsValid)
            {
                db.Messages.Add(message);
                await db.SaveChangesAsync();

                //get newly generated message ID and execute ranking and self upvoting                
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
                ModelState.AddModelError(string.Empty, "Sorry, you are doing that too fast. Please try again in a few minutes.");
                return View(message);
            }
        }

        // POST: Create a new Subverse
        // To protect from overposting attacks, enable the specific properties you want to bind to 
        [HttpPost]
        [PreventSpam(DelayRequest = 1800, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateSubverse([Bind(Include = "Name, Title, Description, Type, Sidebar, Creation_date, Owner")] AddSubverse subverseTmpModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Subverse subverse = new Subverse();
                    subverse.name = subverseTmpModel.Name;
                    subverse.title = "/v/" + subverseTmpModel.Name;
                    subverse.description = subverseTmpModel.Description;
                    subverse.type = subverseTmpModel.Type;
                    subverse.sidebar = subverseTmpModel.Sidebar;
                    subverse.creation_date = subverseTmpModel.Creation_date;

                    //check if subverse exists before attempting to create it
                    if (db.Subverses.Find(subverse.name) == null)
                    {
                        db.Subverses.Add(subverse);
                        await db.SaveChangesAsync();

                        //register user as the owner of the newly created subverse
                        SubverseAdmin tmpSubverseAdmin = new SubverseAdmin();

                        tmpSubverseAdmin.SubverseName = subverse.name;
                        tmpSubverseAdmin.Username = subverseTmpModel.Owner;
                        tmpSubverseAdmin.Power = 1;

                        db.SubverseAdmins.Add(tmpSubverseAdmin);
                        await db.SaveChangesAsync();

                        //go to newly created Subverse
                        return RedirectToAction("Index", "Subverses", new { subversetoshow = subverse.name });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Sorry, The subverse you are trying to create already exists.");
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
                        .Where(x => x.SubverseName == subversetoshow && x.Username == User.Identity.Name && x.Power == 1).FirstOrDefault();

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

                return View(viewModel);
            }
            else
            {
                return new EmptyResult();
            }
        }

        // POST: Eddit a Subverse
        // To protect from overposting attacks, enable the specific properties you want to bind to 
        [HttpPost]
        [PreventSpam(DelayRequest = 60, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
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
                                    .Where(x => x.SubverseName == updatedModel.name && x.Username == User.Identity.Name && x.Power == 1).FirstOrDefault();

                        if (subAdmin != null)
                        {
                            existingSubverse.description = updatedModel.description;
                            existingSubverse.sidebar = updatedModel.sidebar;
                            existingSubverse.stylesheet = updatedModel.stylesheet;

                            // these properties are currently not implemented but they can be saved and edited for future use
                            existingSubverse.type = updatedModel.type;
                            existingSubverse.label_submit_new_link = updatedModel.label_submit_new_link;
                            existingSubverse.label_sumit_new_selfpost = updatedModel.label_sumit_new_selfpost;

                            existingSubverse.submission_text = updatedModel.submission_text;
                            existingSubverse.allow_default = updatedModel.allow_default;
                            existingSubverse.rated_adult = updatedModel.rated_adult;



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

        //show a subverse index
        public ActionResult Index(int? page, string subversetoshow)
        {
            int pageSize = 25;
            int pageNumber = (page ?? 1);

            if (subversetoshow == null)
            {
                return View("~/Views/Errors/Subversenotfound.cshtml");
            }

            ViewBag.SelectedSubverse = subversetoshow;

            try
            {
                if (subversetoshow != "all")
                {
                    //check if subverse exists, if not, send to a page not found error
                    Subverse subverse = db.Subverses.Find(subversetoshow);
                    if (subverse != null)
                    {
                        var submissions = db.Messages.Where(x => x.Subverse == subversetoshow && x.Name != "deleted").OrderByDescending(s => s.Rank).ToList();
                        ViewBag.Title = subverse.description;
                        return View(submissions.ToPagedList(pageNumber, pageSize));
                    }
                    else
                    {
                        ViewBag.SelectedSubverse = "404";
                        return View("~/Views/Errors/Subversenotfound.cshtml");
                    }
                }
                else
                {
                    //if selected subverse is ALL, show submissions from all subverses, sorted by rank
                    var submissions = db.Messages
                        .Where(x => x.Name != "deleted")
                        .OrderByDescending(s => s.Rank).ToList();

                    ViewBag.Title = "all subverses";
                    return View(submissions.ToPagedList(pageNumber, pageSize));
                }
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Home");
            }
        }

        public ActionResult Subverses(int? page)
        {
            ViewBag.SelectedSubverse = "subverses";
            int pageSize = 25;
            int pageNumber = (page ?? 1);

            try
            {
                //order by subscriber count (popularity)
                var subverses = db.Subverses.OrderByDescending(s => s.subscribers).ToList();

                return View(subverses.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Home");

            }
        }

        [Authorize]
        public ViewResult SubversesSubscribed(int? page)
        {
            ViewBag.SelectedSubverse = "subverses";
            ViewBag.whattodisplay = "subscribed";
            int pageSize = 25;
            int pageNumber = (page ?? 1);

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
                int subscriberCount = db.Subscriptions.AsEnumerable()
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

            var subverses = db.Subverses.OrderByDescending(s => s.creation_date).ToList();

            return View("~/Views/Subverses/Subverses.cshtml", subverses.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Subversenotfound()
        {
            ViewBag.SelectedSubverse = "404";
            return View("~/Views/Errors/Subversenotfound.cshtml");
        }

        public ActionResult @New(int? page, string subversetoshow, string sortingmode)
        {
            //sortingmode: new, contraversial, hot, etc
            ViewBag.SortingMode = sortingmode;
            ViewBag.SelectedSubverse = subversetoshow;

            int pageSize = 25;
            int pageNumber = (page ?? 1);

            ViewBag.Title = subversetoshow;

            if (subversetoshow != "all")
            {
                //check if subverse exists, if not, send to a page not found error
                Subverse subverse = db.Subverses.Find(subversetoshow);
                if (subverse != null)
                {
                    var submissions = db.Messages.Where(x => x.Subverse == subversetoshow && x.Name != "deleted").OrderByDescending(s => s.Date).ToList();
                    return View("Index", submissions.ToPagedList(pageNumber, pageSize));
                }
                else
                {
                    return View("~/Views/Errors/Subversenotfound.cshtml");
                }
            }
            else
            {
                //if selected subverse is ALL, show submissions from all subverses, sorted by date
                var submissions = db.Messages
                    .Where(x => x.Name != "deleted")
                    .OrderByDescending(s => s.Date).ToList();

                return View("Index", submissions.ToPagedList(pageNumber, pageSize));
            }

        }

        public ActionResult Random()
        {
            try
            {
                var qry = from row in db.Subverses
                          select row;

                int count = qry.Count(); // 1st round-trip
                int index = new Random().Next(count);

                // example subverse to show: pics
                Subverse randomSubverse = qry.OrderBy(s => s.name).Skip(index).FirstOrDefault(); // 2nd round-trip            

                return RedirectToAction("Index", "Subverses", new { subversetoshow = randomSubverse.name });
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Home");
            }
        }

    }
}