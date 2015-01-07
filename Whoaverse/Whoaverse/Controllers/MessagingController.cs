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

using System.Collections.Generic;
using PagedList;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Voat.Models;
using Voat.Utils;

namespace Voat.Controllers
{
    public class MessagingController : Controller
    {
        private readonly whoaverseEntities _db = new whoaverseEntities();

        // GET: Inbox
        [Authorize]
        public ActionResult Inbox(int? page)
        {
            ViewBag.PmView = "inbox";
            ViewBag.UnreadCommentReplies = Utils.User.UnreadCommentRepliesCount(User.Identity.Name);
            ViewBag.UnreadPostReplies = Utils.User.UnreadPostRepliesCount(User.Identity.Name);
            ViewBag.UnreadPrivateMessages = Utils.User.UnreadPrivateMessagesCount(User.Identity.Name);

            const int pageSize = 25;
            int pageNumber = (page ?? 1);

            if (pageNumber < 1)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // get logged in username and fetch received messages
            try
            {
                var privateMessages = _db.Privatemessages
                    .Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(s => s.Timestamp)
                    .ThenBy(s => s.Sender)
                    .ToList().AsEnumerable();

                var singleMessages = privateMessages as IList<Privatemessage> ?? privateMessages.ToList();
                if (singleMessages.Any())
                {
                    var unreadPrivateMessages = singleMessages
                        .Where(s => s.Status && s.Markedasunread == false).ToList();

                    // todo: implement a delay in the marking of messages as read until the returned inbox view is rendered
                    if (unreadPrivateMessages.Count > 0)
                    {
                        // mark all unread messages as read as soon as the inbox is served, except for manually marked as unread
                        foreach (var singleMessage in singleMessages)
                        {
                            singleMessage.Status = false;
                            _db.SaveChanges();
                        }
                    }
                }

                ViewBag.InboxCount = singleMessages.Count();
                return View(singleMessages.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        // GET: InboxCommentReplies
        [Authorize]
        public ActionResult InboxCommentReplies(int? page)
        {
            ViewBag.PmView = "inbox";

            ViewBag.UnreadCommentReplies = Utils.User.UnreadCommentRepliesCount(User.Identity.Name);
            ViewBag.UnreadPostReplies = Utils.User.UnreadPostRepliesCount(User.Identity.Name);
            ViewBag.UnreadPrivateMessages = Utils.User.UnreadPrivateMessagesCount(User.Identity.Name);

            const int pageSize = 25;
            int pageNumber = (page ?? 1);

            if (pageNumber < 1)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // get logged in username and fetch received comment replies
            try
            {
                var commentReplies = _db.Commentreplynotifications
                    .Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(s => s.Timestamp)
                    .ThenBy(s => s.Sender)
                    .ToList().AsEnumerable();

                var singleCommentReplies = commentReplies as IList<Commentreplynotification> ?? commentReplies.ToList();
                if (singleCommentReplies.Any())
                {
                    var unreadCommentReplies = singleCommentReplies
                        .Where(s => s.Status && s.Markedasunread == false).ToList();

                    // todo: implement a delay in the marking of messages as read until the returned inbox view is rendered
                    if (unreadCommentReplies.Count > 0)
                    {
                        // mark all unread messages as read as soon as the inbox is served, except for manually marked as unread
                        foreach (var singleCommentReply in singleCommentReplies)
                        {
                            singleCommentReply.Status = false;
                            _db.SaveChanges();
                        }
                    }
                }

                ViewBag.CommentRepliesCount = singleCommentReplies.Count();
                return View(singleCommentReplies.ToPagedList(pageNumber, pageSize));

            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        // GET: InboxPostReplies
        [Authorize]
        public ActionResult InboxPostReplies(int? page)
        {
            ViewBag.PmView = "inbox";

            ViewBag.UnreadCommentReplies = Utils.User.UnreadCommentRepliesCount(User.Identity.Name);
            ViewBag.UnreadPostReplies = Utils.User.UnreadPostRepliesCount(User.Identity.Name);
            ViewBag.UnreadPrivateMessages = Utils.User.UnreadPrivateMessagesCount(User.Identity.Name);

            const int pageSize = 25;
            int pageNumber = (page ?? 1);

            if (pageNumber < 1)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // get logged in username and fetch received comment replies
            try
            {
                var postReplies = _db.Postreplynotifications
                    .Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(s => s.Timestamp)
                    .ThenBy(s => s.Sender)
                    .ToList().AsEnumerable();

                var singlePostReplies = postReplies as IList<Postreplynotification> ?? postReplies.ToList();
                if (singlePostReplies.Any())
                {
                    var unreadPostReplies = singlePostReplies
                        .Where(s => s.Status && s.Markedasunread == false).ToList();

                    // todo: implement a delay in the marking of messages as read until the returned inbox view is rendered
                    if (unreadPostReplies.Count > 0)
                    {
                        // mark all unread messages as read as soon as the inbox is served, except for manually marked as unread
                        foreach (var singlePostReply in singlePostReplies)
                        {
                            singlePostReply.Status = false;
                            _db.SaveChanges();
                        }
                    }
                }

                ViewBag.PostRepliesCount = singlePostReplies.Count();
                return View(singlePostReplies.ToPagedList(pageNumber, pageSize));

            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        // GET: InboxUserMentions
        [Authorize]
        public ActionResult InboxUserMentions()
        {
            ViewBag.PmView = "inboxusermentions";
            // get logged in username and fetch received user mentions

            // return user mentions inbox view
            return View();
        }

        // GET: Sent
        [Authorize]
        public ActionResult Sent(int? page)
        {
            ViewBag.PmView = "sent";

            const int pageSize = 25;
            int pageNumber = (page ?? 1);

            if (pageNumber < 1)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // get logged in username and fetch sent messages
            try
            {
                var privateMessages = _db.Privatemessages
                    .Where(s => s.Sender.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(s => s.Timestamp)
                    .ThenBy(s => s.Recipient)
                    .ToList().AsEnumerable();

                var privatemessages = privateMessages as IList<Privatemessage> ?? privateMessages.ToList();
                ViewBag.OutboxCount = privatemessages.Count();
                return View(privatemessages.ToPagedList(pageNumber, pageSize));

            }
            catch (Exception)
            {
                return RedirectToAction("HeavyLoad", "Error");
            }
        }

        // GET: Compose
        [Authorize]
        public ActionResult Compose()
        {
            ViewBag.PmView = "compose";

            var recipient = Request.Params["recipient"];

            if (recipient != null)
            {
                ViewBag.recipient = recipient;
            }

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
            if (!ModelState.IsValid) return View();
            if (privateMessage.Recipient == null || privateMessage.Subject == null || privateMessage.Body == null) return RedirectToAction("Sent", "Messaging");
            
            // check if recipient exists
            if (Utils.User.UserExists(privateMessage.Recipient) && !Utils.User.IsUserBanned(User.Identity.Name))
            {
                // send the message
                privateMessage.Timestamp = DateTime.Now;
                privateMessage.Sender = User.Identity.Name;
                privateMessage.Status = true;
                if (Utils.User.IsUserBanned(User.Identity.Name)) return RedirectToAction("Sent", "Messaging");
                _db.Privatemessages.Add(privateMessage);
                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return RedirectToAction("HeavyLoad", "Error");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Sorry, there is no recipient with that username.");
                return View();
            }
            return RedirectToAction("Sent", "Messaging");
        }

        // POST: Send new private message or reply
        [Authorize]
        [HttpPost]
        [PreventSpam(DelayRequest = 300, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendPrivateMessage([Bind(Include = "Id,Recipient,Subject,Body")] Privatemessage privateMessage)
        {
            if (!ModelState.IsValid) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (privateMessage.Recipient == null || privateMessage.Subject == null || privateMessage.Body == null)
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            // check if recipient exists
            if (Utils.User.UserExists(privateMessage.Recipient))
            {
                // send the message
                privateMessage.Timestamp = DateTime.Now;
                privateMessage.Sender = User.Identity.Name;
                privateMessage.Status = true;
                if (Utils.User.IsUserBanned(User.Identity.Name)) return new HttpStatusCodeResult(HttpStatusCode.OK);
                _db.Privatemessages.Add(privateMessage);

                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return RedirectToAction("HeavyLoad", "Error");
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [Authorize]
        [HttpPost]
        [PreventSpam(DelayRequest = 3, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public JsonResult DeletePrivateMessage(int privateMessageId)
        {
            // check that the message is owned by logged in user executing delete action
            var loggedInUser = User.Identity.Name;

            var privateMessageToDelete = _db.Privatemessages.FirstOrDefault(s => s.Recipient.Equals(loggedInUser, StringComparison.OrdinalIgnoreCase) && s.Id == privateMessageId);

            if (privateMessageToDelete != null)
            {
                // delete the message
                var privateMessage = _db.Privatemessages.Find(privateMessageId);
                _db.Privatemessages.Remove(privateMessage);
                _db.SaveChangesAsync();
                Response.StatusCode = 200;
                return Json("Message deleted.", JsonRequestBehavior.AllowGet);
            }
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json("Bad request.", JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        [PreventSpam(DelayRequest = 3, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public JsonResult DeletePrivateMessageFromSent(int privateMessageId)
        {
            // check that the message is owned by logged in user executing delete action
            var loggedInUser = User.Identity.Name;

            var privateMessageToDelete = _db.Privatemessages.FirstOrDefault(s => s.Sender.Equals(loggedInUser, StringComparison.OrdinalIgnoreCase) && s.Id == privateMessageId);

            if (privateMessageToDelete != null)
            {
                // delete the message
                var privateMessage = _db.Privatemessages.Find(privateMessageId);
                _db.Privatemessages.Remove(privateMessage);
                _db.SaveChangesAsync();
                Response.StatusCode = 200;
                return Json("Message deleted.", JsonRequestBehavior.AllowGet);
            }
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json("Bad request.", JsonRequestBehavior.AllowGet);
        }

    }
}