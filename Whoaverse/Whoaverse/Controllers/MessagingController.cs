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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whoaverse.Models;
using Whoaverse.Utils;

namespace Whoaverse.Controllers
{
    public class MessagingController : Controller
    {
        private whoaverseEntities db = new whoaverseEntities();

        // GET: Inbox
        [Authorize]
        public ActionResult Inbox(int? page)
        {
            ViewBag.SelectedSubverse = "inbox";
            ViewBag.UnreadCommentReplies = Whoaverse.Utils.User.UnreadCommentRepliesCount(User.Identity.Name);
            ViewBag.UnreadPostReplies = Whoaverse.Utils.User.UnreadPostRepliesCount(User.Identity.Name);
            ViewBag.UnreadPrivateMessages = Whoaverse.Utils.User.UnreadPrivateMessagesCount(User.Identity.Name);

            int pageSize = 25;
            int pageNumber = (page ?? 1);

            // get logged in username and fetch received messages
            try
            {
                var privateMessages = db.Privatemessages
                    .Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(s => s.Timestamp)
                    .ThenBy(s => s.Sender)
                    .ToList().AsEnumerable();

                if (privateMessages.Count() > 0)
                {
                    var unreadPrivateMessages = privateMessages
                        .Where(s => s.Status == true && s.Markedasunread == false).ToList();

                    // todo: implement a delay in the marking of messages as read until the returned inbox view is rendered
                    if (unreadPrivateMessages.Count > 0)
                    {
                        // mark all unread messages as read as soon as the inbox is served, except for manually marked as unread
                        foreach (var singleMessage in privateMessages)
                        {
                            singleMessage.Status = false;
                            db.SaveChanges();
                        }
                    }
                }

                return View(privateMessages.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Home");
            }
        }

        // GET: InboxCommentReplies
        [Authorize]
        public ActionResult InboxCommentReplies(int? page)
        {
            ViewBag.SelectedSubverse = "inbox";

            ViewBag.UnreadCommentReplies = Whoaverse.Utils.User.UnreadCommentRepliesCount(User.Identity.Name);
            ViewBag.UnreadPostReplies = Whoaverse.Utils.User.UnreadPostRepliesCount(User.Identity.Name);
            ViewBag.UnreadPrivateMessages = Whoaverse.Utils.User.UnreadPrivateMessagesCount(User.Identity.Name);

            int pageSize = 25;
            int pageNumber = (page ?? 1);

            // get logged in username and fetch received comment replies
            try
            {
                var commentReplies = db.Commentreplynotifications
                    .Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(s => s.Timestamp)
                    .ThenBy(s => s.Sender)
                    .ToList().AsEnumerable();

                if (commentReplies.Count() > 0)
                {
                    var unreadCommentReplies = commentReplies
                        .Where(s => s.Status == true && s.Markedasunread == false).ToList();

                    // todo: implement a delay in the marking of messages as read until the returned inbox view is rendered
                    if (unreadCommentReplies.Count > 0)
                    {
                        // mark all unread messages as read as soon as the inbox is served, except for manually marked as unread
                        foreach (var singleCommentReply in commentReplies)
                        {
                            singleCommentReply.Status = false;
                            db.SaveChanges();
                        }
                    }
                }

                return View(commentReplies.ToPagedList(pageNumber, pageSize));

            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Home");
            }
        }

        // GET: InboxPostReplies
        [Authorize]
        public ActionResult InboxPostReplies(int? page)
        {
            ViewBag.SelectedSubverse = "inbox";

            ViewBag.UnreadCommentReplies = Whoaverse.Utils.User.UnreadCommentRepliesCount(User.Identity.Name);
            ViewBag.UnreadPostReplies = Whoaverse.Utils.User.UnreadPostRepliesCount(User.Identity.Name);
            ViewBag.UnreadPrivateMessages = Whoaverse.Utils.User.UnreadPrivateMessagesCount(User.Identity.Name);

            int pageSize = 25;
            int pageNumber = (page ?? 1);

            // get logged in username and fetch received comment replies
            try
            {
                var postReplies = db.Postreplynotifications
                    .Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(s => s.Timestamp)
                    .ThenBy(s => s.Sender)
                    .ToList().AsEnumerable();

                if (postReplies.Count() > 0)
                {
                    var unreadPostReplies = postReplies
                        .Where(s => s.Status == true && s.Markedasunread == false).ToList();

                    // todo: implement a delay in the marking of messages as read until the returned inbox view is rendered
                    if (unreadPostReplies.Count > 0)
                    {
                        // mark all unread messages as read as soon as the inbox is served, except for manually marked as unread
                        foreach (var singlePostReply in postReplies)
                        {
                            singlePostReply.Status = false;
                            db.SaveChanges();
                        }
                    }
                }

                return View(postReplies.ToPagedList(pageNumber, pageSize));

            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Home");
            }
        }        

        // GET: InboxUserMentions
        [Authorize]
        public ActionResult InboxUserMentions()
        {
            ViewBag.SelectedSubverse = "inboxusermentions";
            // get logged in username and fetch received user mentions

            // return user mentions inbox view
            return View();
        }

        // GET: Sent
        [Authorize]
        public ActionResult Sent(int? page)
        {
            ViewBag.SelectedSubverse = "sent";

            int pageSize = 25;
            int pageNumber = (page ?? 1);

            // get logged in username and fetch sent messages
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var privateMessages = db.Privatemessages
                        .Where(s => s.Sender.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(s => s.Timestamp)
                        .ThenBy(s => s.Recipient)
                        .ToList().AsEnumerable();

                    return View(privateMessages.ToPagedList(pageNumber, pageSize));
                }
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Home");
            }

            // return sent messages view
            return View();
        }

        // GET: Compose
        [Authorize]
        public ActionResult Compose()
        {
            ViewBag.SelectedSubverse = "compose";

            // get logged in username and configure compose view

            // return compose view
            return View();
        }

        // POST: Compose
        [Authorize]
        [HttpPost]
        [PreventSpam(DelayRequest = 300, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Compose([Bind(Include = "Id,Recipient,Subject,Body")] Privatemessage privateMessage)
        {
            if (ModelState.IsValid)
            {
                if (privateMessage.Recipient != null && privateMessage.Subject != null && privateMessage.Body != null)
                {
                    // check if recipient exists
                    if (Whoaverse.Utils.User.UserExists(privateMessage.Recipient))
                    {
                        // send the message
                        privateMessage.Timestamp = System.DateTime.Now;
                        privateMessage.Sender = User.Identity.Name;
                        privateMessage.Status = true;
                        db.Privatemessages.Add(privateMessage);

                        try
                        {
                            await db.SaveChangesAsync();
                        }
                        catch (Exception)
                        {
                            return RedirectToAction("HeavyLoad", "Home");
                        }

                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Sorry, there is no recipient with that username.");
                        return View();
                    }
                }

                return RedirectToAction("Sent", "Messaging");

            }
            else
            {
                return View();
            }
        }

        [Authorize]
        [HttpPost]
        [PreventSpam(DelayRequest = 3, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public JsonResult DeletePrivateMessage(int privateMessageId)
        {
            // check that the message is owned by logged in user executing delete action
            string loggedInUser = User.Identity.Name;

            var privateMessageToDelete = db.Privatemessages
                        .Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) && s.Id == privateMessageId).FirstOrDefault();

            if (privateMessageToDelete != null)
            {
                // delete the message
                Privatemessage privateMessage = db.Privatemessages.Find(privateMessageId);
                db.Privatemessages.Remove(privateMessage);
                db.SaveChangesAsync();
                Response.StatusCode = 200;
                return Json("Message deleted.", JsonRequestBehavior.AllowGet);
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Bad request.", JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        [HttpPost]
        [PreventSpam(DelayRequest = 3, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public JsonResult DeletePrivateMessageFromSent(int privateMessageId)
        {
            // check that the message is owned by logged in user executing delete action
            string loggedInUser = User.Identity.Name;

            var privateMessageToDelete = db.Privatemessages
                        .Where(s => s.Sender.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase) && s.Id == privateMessageId).FirstOrDefault();

            if (privateMessageToDelete != null)
            {
                // delete the message
                Privatemessage privateMessage = db.Privatemessages.Find(privateMessageId);
                db.Privatemessages.Remove(privateMessage);
                db.SaveChangesAsync();
                Response.StatusCode = 200;
                return Json("Message deleted.", JsonRequestBehavior.AllowGet);
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Bad request.", JsonRequestBehavior.AllowGet);
            }
        }

    }
}