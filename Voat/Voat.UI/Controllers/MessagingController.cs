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

using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using PagedList;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Voat.Models;

using Voat.Utilities;
using Voat.Data.Models;
using Voat.UI.Utilities;

namespace Voat.Controllers
{
    public class MessagingController : Controller
    {
        private readonly voatEntities _db = new voatEntities();

        private void SetViewBagCounts()
        {
            // set unread counts
            ViewBag.UnreadCommentReplies = Voat.Utilities.UserHelper.UnreadCommentRepliesCount(User.Identity.Name);
            ViewBag.UnreadPostReplies = Voat.Utilities.UserHelper.UnreadPostRepliesCount(User.Identity.Name);
            ViewBag.UnreadPrivateMessages = Voat.Utilities.UserHelper.UnreadPrivateMessagesCount(User.Identity.Name);

            // set total counts
            ViewBag.PostRepliesCount = Voat.Utilities.UserHelper.PostRepliesCount(User.Identity.Name);
            ViewBag.CommentRepliesCount = Voat.Utilities.UserHelper.CommentRepliesCount(User.Identity.Name);
            ViewBag.InboxCount = Voat.Utilities.UserHelper.PrivateMessageCount(User.Identity.Name);
        }

        // GET: Inbox
        [System.Web.Mvc.Authorize]
        public ActionResult Inbox(int? page)
        {
            int unreadCommentCount = Voat.Utilities.UserHelper.UnreadCommentRepliesCount(User.Identity.Name);
            int unreadPostCount = Voat.Utilities.UserHelper.UnreadPostRepliesCount(User.Identity.Name);
            int unreadPMCount = Voat.Utilities.UserHelper.UnreadPrivateMessagesCount(User.Identity.Name);

            if (unreadPMCount > 0)
            {
                return InboxPrivateMessages(page);
            }

            if (unreadCommentCount > 0)
            {
                return InboxCommentReplies(page);
            }
            
            // return inbox view if there are no unread comments or post replies
            return unreadPostCount > 0 ? InboxPostReplies(page) : InboxPrivateMessages(page);
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
                IQueryable<PrivateMessage> privateMessages = _db.PrivateMessages
                    .Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(s => s.CreationDate)
                    .ThenBy(s => s.Sender);

                var unreadCount = privateMessages.Count(pm => pm.IsUnread);

                ViewBag.InboxCount = privateMessages.Count();
                ViewBag.UnreadCount = unreadCount;
                return View("Inbox", privateMessages.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception)
            {
                return View("~/Views/Errors/DbNotResponding.cshtml");
            }
        }

        // GET: Inbox/Unread
        [System.Web.Mvc.Authorize]
        public ActionResult InboxPrivateMessagesUnread(int? page)
        {
            ViewBag.PmView = "unread";

            SetViewBagCounts();

            const int pageSize = 25;
            int pageNumber = (page ?? 1);

            if (pageNumber < 1)
            {
                return View("~/Views/Errors/Error_404.cshtml");
            }

            // get logged in username and fetch unread private messages
            try
            {
                IQueryable<PrivateMessage> privateMessages = _db.PrivateMessages
                    .Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(s => s.CreationDate)
                    .ThenBy(s => s.Sender);

                ViewBag.InboxCount = privateMessages.Count();
                var unreadMessages = privateMessages.Where(pm => pm.IsUnread);
                ViewBag.UnreadCount = unreadMessages.Count();
                return View("InboxUnread", unreadMessages.ToPagedList(pageNumber, pageSize));
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
                IQueryable<CommentReplyNotification> commentReplyNotifications = _db.CommentReplyNotifications.Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
                IQueryable<Comment> commentReplies = _db.Comments.Where(p => commentReplyNotifications.Any(p2 => p2.CommentID == p.ID)).OrderByDescending(s => s.CreationDate);

                // mark all unread messages as read as soon as the inbox is served, except for manually marked as unread
                if (commentReplyNotifications.Any())
                {
                    var unreadCommentReplies = commentReplyNotifications.Where(s => s.IsUnread && s.MarkedAsUnread == false);

                    // todo: implement a delay in the marking of messages as read until the returned inbox view is rendered
                    if (unreadCommentReplies.Any())
                    {
                        foreach (var singleCommentReply in unreadCommentReplies)
                        {
                            // status: true = unread, false = read
                            singleCommentReply.IsUnread = false;
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
                IQueryable<SubmissionReplyNotification> postReplyNotifications = _db.SubmissionReplyNotifications.Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
                IQueryable<Comment> postReplies = _db.Comments.Where(p => postReplyNotifications.Any(p2 => p2.CommentID == p.ID)).OrderByDescending(s => s.CreationDate);

                // mark all unread messages as read as soon as the inbox is served, except for manually marked as unread
                if (postReplyNotifications.Any())
                {
                    var unreadPostReplies = postReplyNotifications.Where(s => s.IsUnread && s.MarkedAsUnread == false);

                    // todo: implement a delay in the marking of messages as read until the returned inbox view is rendered
                    if (unreadPostReplies.Any())
                    {
                        foreach (var singlePostReply in unreadPostReplies)
                        {
                            // status: true = unread, false = read
                            singlePostReply.IsUnread = false;
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
                var privateMessages = _db.PrivateMessages
                    .Where(s => s.Sender.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(s => s.CreationDate)
                    .ThenBy(s => s.Recipient)
                    .ToList().AsEnumerable();

                var privatemessages = privateMessages as IList<PrivateMessage> ?? privateMessages.ToList();
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
        public async Task<ActionResult> Compose([Bind(Include = "ID,Recipient,Subject,Body")] PrivateMessage privateMessage)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
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

            var response = MesssagingUtility.SendPrivateMessage(User.Identity.Name, privateMessage.Recipient, privateMessage.Subject, privateMessage.Body);

            return RedirectToAction("Sent", "Messaging");
        }

        // POST: Send new private submission or reply
        [System.Web.Mvc.Authorize]
        [HttpPost]
        [PreventSpam(DelayRequest = 30, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendPrivateMessage([Bind(Include = "ID,Recipient,Subject,Body")] PrivateMessage privateMessage)
        {
            if (!ModelState.IsValid) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (privateMessage.Recipient == null || privateMessage.Subject == null || privateMessage.Body == null)
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            // check if recipient exists
            if (UserHelper.UserExists(privateMessage.Recipient))
            {
                // send the submission
                privateMessage.CreationDate = DateTime.Now;
                privateMessage.Sender = User.Identity.Name;
                privateMessage.IsUnread = true;
                if (Voat.Utilities.UserHelper.IsUserGloballyBanned(User.Identity.Name)) return new HttpStatusCodeResult(HttpStatusCode.OK);
                _db.PrivateMessages.Add(privateMessage);

                try
                {
                    await _db.SaveChangesAsync();

                    // get count of unread notifications
                    int unreadNotifications = Voat.Utilities.UserHelper.UnreadTotalNotificationsCount(privateMessage.Recipient);

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
            // check that the submission is owned by logged in user executing delete action
            var loggedInUser = User.Identity.Name;

            var privateMessageToDelete = _db.PrivateMessages.FirstOrDefault(s => s.Recipient.Equals(loggedInUser, StringComparison.OrdinalIgnoreCase) && s.ID == privateMessageId);

            if (privateMessageToDelete != null)
            {
                // delete the submission
                var privateMessage = _db.PrivateMessages.Find(privateMessageId);
                _db.PrivateMessages.Remove(privateMessage);
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
            // check that the submission is owned by logged in user executing delete action
            var loggedInUser = User.Identity.Name;

            var privateMessageToDelete = _db.PrivateMessages.FirstOrDefault(s => s.Sender.Equals(loggedInUser, StringComparison.OrdinalIgnoreCase) && s.ID == privateMessageId);

            if (privateMessageToDelete != null)
            {
                // delete the submission
                var privateMessage = _db.PrivateMessages.Find(privateMessageId);
                _db.PrivateMessages.Remove(privateMessage);
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
            int unreadNotifications = Voat.Utilities.UserHelper.UnreadTotalNotificationsCount(User.Identity.Name);

            var hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();
            hubContext.Clients.User(User.Identity.Name).setNotificationsPending(unreadNotifications);
        }

        [System.Web.Mvc.Authorize]
        [HttpGet]
        [PreventSpam(DelayRequest = 3, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public async Task<JsonResult> MarkAsRead(string itemType, bool? markAll, int? itemId)
        {
            if (markAll == null)
            {
                markAll = false;
            }

            // item status: true = unread, false = read
            switch (itemType)
            {
                case "privateMessage":
                    if (await MesssagingUtility.MarkPrivateMessagesAsRead((bool)markAll, User.Identity.Name, itemId))
                    {
                        UpdateNotificationCounts(); // update notification icon
                        Response.StatusCode = 200;
                        return Json("Item marked as read.", JsonRequestBehavior.AllowGet);
                    }
                    break;
            }

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json("Bad request.", JsonRequestBehavior.AllowGet);
        }
    }
}