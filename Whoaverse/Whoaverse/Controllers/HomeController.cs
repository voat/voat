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
using System.Web;
using System.Web.Mvc;
using Whoaverse.Utils;
using PagedList;
using System.Threading.Tasks;
using System.Net;

namespace Whoaverse.Models
{
    public class HomeController : Controller
    {
        private whoaverseEntities db = new whoaverseEntities();

        // GET: list of subverses
        public ActionResult listofsubverses()
        {
            return PartialView("_listofsubverses", db.Defaultsubverses.OrderBy(s => s.position).ToList().AsEnumerable());
        }

        //public ActionResult userProfile()
        //{
        //    dbusers dbusers = new dbusers();
        //    return PartialView("_userkarma", Message message = db.Messages.Find(id););
        //}

        // GET: Messages/Details/5
        public ActionResult comments(int? id, string subversetoshow)
        {
            ViewBag.SelectedSubverse = subversetoshow;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Message message = db.Messages.Find(id);
            if (message == null)
            {
                return View("~/Views/Shared/Error_404.cshtml");
            }
            return View(message);
        }

        // GET: submitcomment
        [Authorize]
        public ActionResult submitcomment()
        {
            return View();
        }

        // POST: submitcomment
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> submitcomment([Bind(Include = "Id,Votes,Name,Date,CommentContent,MessageId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                await db.SaveChangesAsync();

                //get newly generated Comment ID and execute ranking and self upvoting                
                Commentvotingtracker tmpVotingTracker = new Commentvotingtracker();
                tmpVotingTracker.CommentId = comment.Id;
                tmpVotingTracker.UserName = comment.Name;
                tmpVotingTracker.VoteStatus = 1;
                db.Commentvotingtrackers.Add(tmpVotingTracker);
                await db.SaveChangesAsync();

                string url = this.Request.UrlReferrer.AbsolutePath;
                return Redirect(url);
            }

            return View(comment);
        }

        // GET: submit
        [Authorize]
        public ActionResult submit()
        {
            return View();
        }

        // POST: submit
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> submit([Bind(Include = "Id,Votes,Name,Date,Type,Linkdescription,Title,Rank,MessageContent,Subverse")] Message message)
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

                return RedirectToRoute(
                "SubverseComments",
                new
                {
                    controller = "Home",
                    action = "comments",
                    id = message.Id,
                    subversetoshow = message.Subverse
                }
            );

            }

            return View(message);
        }

        public ActionResult user(string id, int? page, string whattodisplay)
        {
            ViewBag.SelectedSubverse = "user";
            ViewBag.whattodisplay = whattodisplay;
            ViewBag.userid = id;
            int pageSize = 25;
            int pageNumber = (page ?? 1);

            //show comments
            if (whattodisplay != null && whattodisplay == "comments")
            {
                var userComments = from c in db.Comments.OrderByDescending(c => c.Date)
                                   where c.Name.Equals(id)
                                   select c;
                //return View(viewnameasstring, model)
                return View("usercomments",userComments.ToPagedList(pageNumber, pageSize));
            }

            //show submissions                        
            if (whattodisplay != null && whattodisplay == "submissions")
            {
                var userSubmissions = from b in db.Messages.OrderByDescending(s => s.Date)
                                      where b.Name.Equals(id)
                                      select b;
                return View(userSubmissions.ToPagedList(pageNumber, pageSize));
            }

            //default, show overview
            var userDefaultSubmissions = from b in db.Messages.OrderByDescending(s => s.Date)
                                  where b.Name.Equals(id)
                                  select b;
            return View(userDefaultSubmissions.ToPagedList(pageNumber, pageSize));


        }

        public ViewResult Index(int? page)
        {
            ViewBag.SelectedSubverse = "frontpage";
            int pageSize = 25;
            int pageNumber = (page ?? 1);

            var submissions = db.Messages.OrderByDescending(s => s.Rank).ToList();

            return View(submissions.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult @new(int? page, string sortingmode)
        {
            //sortingmode: new, contraversial, hot, etc
            ViewBag.SortingMode = sortingmode;

            int pageSize = 25;
            int pageNumber = (page ?? 1);

            var submissions = db.Messages.OrderByDescending(s => s.Date).ToList();
            return View("Index", submissions.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult about()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult intro()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult help(string pagetoshow)
        {
            if (pagetoshow == "privacy")
            {
                return View("~/Views/Help/privacy.cshtml");
            }
            if (pagetoshow == "markdown")
            {
                return View("~/Views/Help/markdown.cshtml");
            }
            else
            {
                return View("~/Views/Help/index.cshtml");
            }
        }

        public ActionResult contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        public ActionResult privacy()
        {
            ViewBag.Message = "Privacy Policy";
            return View("~/Views/Help/privacy.cshtml");
        }

        public ActionResult Vote(string userWhichVoted, int messageId, int typeOfVote)
        {
            var checkResult = db.Votingtrackers
                                .Where(b => b.MessageId == messageId && b.UserName == userWhichVoted)
                                .FirstOrDefault();

            if (checkResult != null)
            {
                Votingtracker votingtracker = db.Votingtrackers.Find(checkResult.Id);
                votingtracker.UserName = userWhichVoted;
                votingtracker.VoteStatus = typeOfVote;
                db.SaveChangesAsync();
                return new ContentResult { Content = "Glasanje uspjesno!" };
            }
            else
            {
                return new ContentResult { Content = "Glasanje nije uspjesno!" };
            }

        }

        public PartialViewResult VoteTest(string pacient_Name = "")
        {
            return PartialView("_VotingIconsMessage");
        }

    }
}