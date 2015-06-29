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
using Microsoft.AspNet.SignalR;
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

        private void SetViewBagCounts()
        {
            // set unread counts
            ViewBag.UnreadCommentReplies = Utils.User.UnreadCommentRepliesCount(User.Identity.Name);
            ViewBag.UnreadPostReplies = Utils.User.UnreadPostRepliesCount(User.Identity.Name);
            ViewBag.UnreadPrivateMessages = Utils.User.UnreadPrivateMessagesCount(User.Identity.Name);

            // set total counts
            ViewBag.PostRepliesCount = Utils.User.PostRepliesCount(User.Identity.Name);
            ViewBag.CommentRepliesCount = Utils.User.CommentRepliesCount(User.Identity.Name);
            ViewBag.InboxCount = Utils.User.PrivateMessageCount(User.Identity.Name);
        }

        // GET: Inbox
        [System.Web.Mvc.Authorize]
        public ActionResult Inbox(int? page)
        {
            int unreadCommentCount = Utils.User.UnreadCommentRepliesCount(User.Identity.Name);
            int unreadPostCount = Utils.User.UnreadPostRepliesCount(User.Identity.Name);
            int unreadPMCount = Utils.User.UnreadPrivateMessagesCount(User.Identity.Name);

            if (unreadPMCount > 0)
            {
                return InboxPrivateMessages(page);
            }
            if (unreadCommentCount > 0)
            {
                return InboxCommentReplies(page);
            }
            if (unreadPostCount > 0)
            {
                return InboxPostReplies(page);
            }
            return InboxPrivateMessages(page);
        }

        // GET: Inbox
        [System.Web.Mvc.Authorize]
        public ActionResult InboxPrivateMessages(int? page)
        {
            ViewBag.PmView = "inbox";

            SetViewBagCounts();

            const int pageSize = 25;
            int pageNumber = (page ?? 1);

            if (pageNumber < 1)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // get logged in username and fetch received messages
            try
            {
                IQueryable<Privatemessage> privateMessages = _db.Privatemessages
                    .Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(s => s.Timestamp)
                    .ThenBy(s => s.Sender);

                if (privateMessages.Any())
                {
                    var unreadPrivateMessages = privateMessages.Where(s => s.Status && s.Markedasunread == false).ToList();

                    // todo: implement a delay in the marking of messages as read until the returned inbox view is rendered
                    if (unreadPrivateMessages.Count > 0)
                    {
                        // mark all unread messages as read as soon as the inbox is served, except for manually marked as unread
                        foreach (var singleMessage in unreadPrivateMessages.ToList())
                        {
                            // status: true = unread, false = read
                            singleMessage.Status = false;
                        }
                        _db.SaveChanges();
                        // update notification icon
                        UpdateNotificationCounts();
                    }
                }

                ViewBag.InboxCount = privateMessages.Count();
                return View("Inbox", privateMessages.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception)
            {
                return View("~/Views/Errors/DbNotResponding.cshtml");
            }
        }

        // GET: InboxCommentReplies
        [System.Web.Mvc.Authorize]
        public ActionResult InboxCommentReplies(int? page)
        {
            ViewBag.PmView = "inbox";
            SetViewBagCounts();
            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // get logged in username and fetch received comment replies
            try
            {
                IQueryable<Commentreplynotification> commentReplyNotifications = _db.Commentreplynotifications.Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
                IQueryable<Comment> commentReplies = _db.Comments.Where(p => commentReplyNotifications.Any(p2 => p2.CommentId == p.Id)).OrderByDescending(s => s.Date);

                if (commentReplyNotifications.Any())
                {
                    var unreadCommentReplies = commentReplyNotifications.Where(s => s.Status && s.Markedasunread == false);

                    // todo: implement a delay in the marking of messages as read until the returned inbox view is rendered
                    if (unreadCommentReplies.Any())
                    {
                        // mark all unread messages as read as soon as the inbox is served, except for manually marked as unread
                        foreach (var singleCommentReply in unreadCommentReplies)
                        {
                            // status: true = unread, false = read
                            singleCommentReply.Status = false;
                        }
                        _db.SaveChanges();
                        // update notification icon
                        UpdateNotificationCounts();
                    }
                }
                ViewBag.CommentRepliesCount = commentReplyNotifications.Count();

                PaginatedList<Comment> paginatedComments = new PaginatedList<Comment>(commentReplies, page ?? 0, pageSize);
                return View("InboxCommentReplies", paginatedComments);
            }
            catch (Exception)
            {
                return View("~/Views/Errors/DbNotResponding.cshtml");
            }
        }

        // GET: InboxPostReplies
        [System.Web.Mvc.Authorize]
        public ActionResult InboxPostReplies(int? page)
        {
            ViewBag.PmView = "inbox";
            SetViewBagCounts();
            const int pageSize = 25;
            int pageNumber = (page ?? 0);

            if (pageNumber < 0)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // get logged in username and fetch received post replies
            try
            {
                IQueryable<Postreplynotification> postReplyNotifications = _db.Postreplynotifications.Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
                IQueryable<Comment> postReplies = _db.Comments.Where(p => postReplyNotifications.Any(p2 => p2.CommentId == p.Id)).OrderByDescending(s => s.Date);

                if (postReplyNotifications.Any())
                {
                    var unreadPostReplies = postReplyNotifications.Where(s => s.Status && s.Markedasunread == false);

                    // todo: implement a delay in the marking of messages as read until the returned inbox view is rendered
                    if (unreadPostReplies.Any())
                    {
                        // mark all unread messages as read as soon as the inbox is served, except for manually marked as unread
                        foreach (var singlePostReply in unreadPostReplies)
                        {
                            // status: true = unread, false = read
                            singlePostReply.Status = false;
                        }
                        _db.SaveChanges();
                        // update notification icon
                        UpdateNotificationCounts();
                    }
                }
                ViewBag.PostRepliesCount = postReplyNotifications.Count();

                PaginatedList<Comment> paginatedComments = new PaginatedList<Comment>(postReplies, page ?? 0, pageSize);
                return View("InboxPostReplies", paginatedComments);
            }
            catch (Exception)
            {
                return View("~/Views/Errors/DbNotResponding.cshtml");
            }
        }

        // GET: Sent
        [System.Web.Mvc.Authorize]
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
                return View("~/Views/Errors/DbNotResponding.cshtml");
            }
        }

        // GET: Compose
        [System.Web.Mvc.Authorize]
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
        [System.Web.Mvc.Authorize]
        [HttpPost]
        [PreventSpam(DelayRequest = 30, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Compose([Bind(Include = "Id,Recipient,Subject,Body")] Privatemessage privateMessage)
        {
            if (!ModelState.IsValid) return View();
            if (privateMessage.Recipient == null || privateMessage.Subject == null || privateMessage.Body == null) return RedirectToAction("Sent", "Messaging");

            if (Karma.CommentKarma(User.Identity.Name) < 100)
            {
                bool isCaptchaValid = await ReCaptchaUtility.Validate(Request);

                if (!isCaptchaValid)
                {
                    ModelState.AddModelError(string.Empty, "Incorrect recaptcha answer.");
                    return View();
                }
            }

            // check if recipient exists
            if (Utils.User.UserExists(privateMessage.Recipient) && !Utils.User.IsUserGloballyBanned(User.Identity.Name))
            {
                // send the message
                privateMessage.Timestamp = DateTime.Now;
                privateMessage.Sender = User.Identity.Name;
                privateMessage.Status = true;
                if (Utils.User.IsUserGloballyBanned(User.Identity.Name)) return RedirectToAction("Sent", "Messaging");
                _db.Privatemessages.Add(privateMessage);
                try
                {
                    await _db.SaveChangesAsync();

                    // get count of unread notifications
                    int unreadNotifications = Utils.User.UnreadTotalNotificationsCount(privateMessage.Recipient);

                    // send SignalR realtime notification to recipient
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();
                    hubContext.Clients.User(privateMessage.Recipient).setNotificationsPending(unreadNotifications);
                }
                catch (Exception)
                {
                    return View("~/Views/Errors/DbNotResponding.cshtml");
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
        [System.Web.Mvc.Authorize]
        [HttpPost]
        [PreventSpam(DelayRequest = 30, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
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
                if (Utils.User.IsUserGloballyBanned(User.Identity.Name)) return new HttpStatusCodeResult(HttpStatusCode.OK);
                _db.Privatemessages.Add(privateMessage);

                try
                {
                    await _db.SaveChangesAsync();

                    // get count of unread notifications
                    int unreadNotifications = Utils.User.UnreadTotalNotificationsCount(privateMessage.Recipient);

                    // send SignalR realtime notification to recipient
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();
                    hubContext.Clients.User(privateMessage.Recipient).setNotificationsPending(unreadNotifications);
                }
                catch (Exception)
                {
                    return View("~/Views/Errors/DbNotResponding.cshtml");
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [System.Web.Mvc.Authorize]
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

        [System.Web.Mvc.Authorize]
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

        // a method which triggers SignalR notification count update on client side for logged-in user
        private void UpdateNotificationCounts()
        {
            // get count of unread notifications
            int unreadNotifications = Utils.User.UnreadTotalNotificationsCount(User.Identity.Name);

            var hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();
            hubContext.Clients.User(User.Identity.Name).setNotificationsPending(unreadNotifications);
        }
    }
}