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

using System.Threading.Tasks;
using System.Web.Mvc;

using Voat.Utilities;
using Voat.UI.Utilities;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Domain;
using Voat.Domain.Query;
using PagedList;
using Voat.Models.ViewModels;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Web.Routing;

namespace Voat.Controllers
{
   
    [Authorize]
    public class MessagesController : BaseController
    {
        
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.Context = new MessageContextViewModel() { ViewerContext = UserDefinition.Parse(User.Identity.Name) };
            base.OnActionExecuting(filterContext);
        }

        private PagedList.StaticPagedList<T> FakePaging<T>(IEnumerable<T> result, int page, int pageSize)
        {
            
            int currentCount = result.Count();
            int fakeTotal = currentCount;
            if (currentCount < pageSize)
            {
                //no future pages
                fakeTotal = Math.Max((page),0) * pageSize + currentCount;
            }
            else
            {
                fakeTotal = (page + 1) * pageSize + 1;
            }
            return new StaticPagedList<T>(result, page + 1, pageSize, fakeTotal);

        }

        public async Task<ActionResult> Index(int? page = null)
        {

            var unread = new QueryMessageCounts(MessageTypeFlag.All, MessageState.Unread);
            var counts = await unread.ExecuteAsync();

            if (counts.Total > 0)
            {
                if (counts.Counts.Any(x => x.Type == MessageType.Private))
                {
                    return await Private(page);
                }
                else if (counts.Counts.Any(x => x.Type == MessageType.SubmissionMention))
                {
                    return await Mentions(ContentType.Submission, page);
                }
                else if (counts.Counts.Any(x => x.Type == MessageType.CommentMention))
                {
                    return await Mentions(ContentType.Comment, page);
                }
                else if (counts.Counts.Any(x => x.Type == MessageType.SubmissionReply))
                {
                    return await Replies(ContentType.Submission, page);
                }
                else if (counts.Counts.Any(x => x.Type == MessageType.CommentReply))
                {
                    return await Replies(ContentType.Comment, page);
                }
            }
            return await Private(page);
        }


        public async Task<ActionResult> Private(int? page = null)
        {

            ViewBag.PmView = "inbox";
            ViewBag.Title = "Inbox";

            var q = new QueryMessages(MessageTypeFlag.Private, MessageState.All, false);
            q.PageNumber = (page.HasValue && page.Value >= 0 ? page.Value - 1 : 0);

            var result = await q.ExecuteAsync();

            var pagedList = FakePaging(result, q.PageNumber, q.PageCount);

            return View("Index", pagedList);

        }

        public async Task<ActionResult> Sent(int? page = null)
        {

            ViewBag.PmView = "sent";
            ViewBag.Title = "Sent";

            var q = new QueryMessages(MessageTypeFlag.Sent, MessageState.All, true);
            q.PageNumber = (page.HasValue && page.Value >= 0 ? page.Value - 1 : 0);
            q.PageCount = 5;
            var result = await q.ExecuteAsync();

            var pagedList = FakePaging(result, q.PageNumber, q.PageCount);

            return View("Index", pagedList);

        }
        public async Task<ActionResult> Replies(ContentType? type = null, int? page = null)
        {

            ViewBag.PmView = "inbox";
            ViewBag.Title = "Replies";

            var contentType = MessageTypeFlag.CommentReply | MessageTypeFlag.SubmissionReply;
            if (type.HasValue)
            {
                contentType = type.Value == ContentType.Comment ? MessageTypeFlag.CommentReply : MessageTypeFlag.SubmissionReply;
                ViewBag.Title = type.ToString() + " Replies";
            }

            var q = new QueryMessages(contentType, MessageState.All, true);
            q.PageNumber = (page.HasValue && page.Value >= 0 ? page.Value - 1 : 0);
            var result = await q.ExecuteAsync();

            var pagedList = FakePaging(result, q.PageNumber, q.PageCount);

            return View("Index", pagedList);

        }
        public async Task<ActionResult> Mentions(ContentType? type = null, int? page = null)
        {

            ViewBag.PmView = "inbox";
            ViewBag.Title = "Mentions";

            var contentType = MessageTypeFlag.CommentMention | MessageTypeFlag.SubmissionMention;
            if (type.HasValue)
            {
                contentType = type.Value == ContentType.Comment ? MessageTypeFlag.CommentMention : MessageTypeFlag.SubmissionMention;
                ViewBag.Title = type.ToString() + " Mentions";
            }

            var q = new QueryMessages(contentType, MessageState.All, true);
            q.PageNumber = (page.HasValue && page.Value >= 0 ? page.Value - 1 : 0);

            var result = await q.ExecuteAsync();
            var pagedList = FakePaging(result, q.PageNumber, q.PageCount);

            return View("Index", pagedList);
        }
        // GET: Compose
        [System.Web.Mvc.Authorize]
        public ActionResult Compose()
        {
            ViewBag.PmView = "compose";
            ViewBag.Title = "Compose";

            var recipient = Request.Params["recipient"];
            var subverse = (string)RouteData.Values["subverse"];
            var model = new NewMessageViewModel() { Recipient = recipient };

            if (!string.IsNullOrEmpty(subverse))
            {
                if (!ModeratorPermission.HasPermission(User.Identity.Name, subverse, ModeratorAction.SendMail))
                {
                    return RedirectToAction("Home", "Index");
                }
                ViewBag.PmView = "mod";
                model.Sender = UserDefinition.Format(subverse, IdentityType.Subverse);
            }

            // return compose view
            return View(model);
        }

        // POST: Compose
        [HttpPost]
        [PreventSpam(DelayRequest = 30, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> Compose(NewMessageViewModel message)
        {
            if (!ModelState.IsValid)
            {
                return View(message);
            }

            if (message.Recipient == null || message.Subject == null || message.Body == null)
            {
                return RedirectToAction("Sent", "Messages");
            }

            var userData = new UserData(User.Identity.Name, false);
            if (userData.Information.CommentPoints.Sum < 100)
            {
                bool isCaptchaValid = await ReCaptchaUtility.Validate(Request);

                if (!isCaptchaValid)
                {
                    ModelState.AddModelError(string.Empty, "Incorrect recaptcha answer.");
                    return View(message);
                }
            }
            var sendMessage = new SendMessage() {
                Recipient = message.Recipient,
                Message = message.Body,
                Subject = message.Subject,
                Sender = message.Sender
            };
            var cmd = new SendMessageCommand(sendMessage);
            var response = await cmd.Execute();

            if (response.Success)
            {
                var m = response.Response;
                if (m.SenderType == IdentityType.Subverse)
                {
                    return RedirectToAction("SubverseIndex", "Messages", new { subverse = m.Sender, type = MessageTypeFlag.Sent, state = MessageState.All });
                }
                else
                {
                    return RedirectToAction("Sent", "Messages");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, response.Message);
                return View(message);
            }
        }
        // POST: Compose
        [Authorize]
        [HttpGet]
        public ActionResult ReplyForm(MessageReplyViewModel message)
        {
            ModelState.Clear();
            return PartialView("_MessageReply", message);
        }
        // POST: Compose
        [HttpPost]
        [PreventSpam(DelayRequest = 30, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> Reply(MessageReplyViewModel message)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (message.ID <= 0)
            {
                return RedirectToAction("Sent", "Messages");
            }

            var cmd = new SendMessageReplyCommand(message.ID, message.Body);
            var response = await cmd.Execute();

            if (response.Success)
            {
                if (Request.IsAjaxRequest())
                {
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
                else
                {
                    return RedirectToAction("Sent", "Messages");
                }
            }
            else
            {
                if (Request.IsAjaxRequest())
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, response.Message);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response.Message);
                    return View();
                }
            }
        }

        //url: messageRoot + "/mark/{type}/{action}/{id}",
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> Mark(MessageTypeFlag type, MessageState markAction, int? id = null, string subverse = null)
        {

            var ownerName = User.Identity.Name;
            var ownerType = IdentityType.User;
            if (!string.IsNullOrEmpty(subverse))
            {
                ownerName = subverse.TrimSafe();
                ownerType = IdentityType.Subverse;
            }
            var cmd = new MarkMessagesCommand(ownerName, ownerType, type, markAction, id);
            var response = await cmd.Execute();

            if (response.Success)
            {
                
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, response.Message);
            }
        }

        //url: messageRoot + "/delete/{type}/{id}",
        [HttpPost]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(MessageTypeFlag type, int? id = null, string subverse = null)
        {
            var ownerName = User.Identity.Name;
            var ownerType = IdentityType.User;
            if (!string.IsNullOrEmpty(subverse))
            {
                ownerName = subverse.TrimSafe();
                ownerType = IdentityType.Subverse;
            }

            var cmd = new DeleteMessagesCommand(ownerName, ownerType, type, id);
            var response = await cmd.Execute();

            if (response.Success)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, response.Message);
            }
        }

        public async Task<ActionResult> SubverseIndex(string subverse, MessageTypeFlag type, MessageState? state = null)
        {
            if (!(type == MessageTypeFlag.Private || type == MessageTypeFlag.Sent))
            {
                return RedirectToAction("Index", "Home");
            }
            if (string.IsNullOrEmpty(subverse))
            {
                return RedirectToAction("Index", "Home");
            }
            if (!ModeratorPermission.HasPermission(User.Identity.Name, subverse, ModeratorAction.ReadMail))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Context = new MessageContextViewModel() { ViewerContext = new UserDefinition() { Name = subverse, Type = IdentityType.Subverse }};

            var qSub = new QuerySubverse(subverse);
            var sub = await qSub.ExecuteAsync();

            ViewBag.PmView = "mod";
            ViewBag.Title = string.Format("v/{0} {1}", sub.Name, (type == MessageTypeFlag.Sent ? "Sent" : "Inbox"));

            var q = new QueryMessages(sub.Name, IdentityType.Subverse, type, MessageState.All, false);
            var result = await q.ExecuteAsync();

            var pagedList = FakePaging(result, q.PageNumber, q.PageCount);
            return View("Index", pagedList);
        }

    }

    //public class MessagingController : BaseController
    //{
    //    private readonly voatEntities _db = new voatEntities();

    //    private void SetViewBagCounts()
    //    {
    //        // set unread counts
    //        ViewBag.UnreadCommentReplies = Voat.Utilities.UserHelper.UnreadCommentRepliesCount(User.Identity.Name);
    //        ViewBag.UnreadPostReplies = Voat.Utilities.UserHelper.UnreadPostRepliesCount(User.Identity.Name);
    //        ViewBag.UnreadPrivateMessages = Voat.Utilities.UserHelper.UnreadPrivateMessagesCount(User.Identity.Name);

    //        // set total counts
    //        ViewBag.PostRepliesCount = Voat.Utilities.UserHelper.PostRepliesCount(User.Identity.Name);
    //        ViewBag.CommentRepliesCount = Voat.Utilities.UserHelper.CommentRepliesCount(User.Identity.Name);
    //        ViewBag.InboxCount = Voat.Utilities.UserHelper.PrivateMessageCount(User.Identity.Name);
    //    }

    //    // GET: Inbox
    //    [System.Web.Mvc.Authorize]
    //    public ActionResult Inbox(int? page)
    //    {
    //        int unreadCommentCount = Voat.Utilities.UserHelper.UnreadCommentRepliesCount(User.Identity.Name);
    //        int unreadPostCount = Voat.Utilities.UserHelper.UnreadPostRepliesCount(User.Identity.Name);
    //        int unreadPMCount = Voat.Utilities.UserHelper.UnreadPrivateMessagesCount(User.Identity.Name);

    //        if (unreadPMCount > 0)
    //        {
    //            return InboxPrivateMessages(page);
    //        }

    //        if (unreadCommentCount > 0)
    //        {
    //            return InboxCommentReplies(page);
    //        }

    //        // return inbox view if there are no unread comments or post replies
    //        return unreadPostCount > 0 ? InboxPostReplies(page) : InboxPrivateMessages(page);
    //    }

    //    // GET: Inbox
    //    [System.Web.Mvc.Authorize]
    //    public ActionResult InboxPrivateMessages(int? page)
    //    {
    //        ViewBag.PmView = "inbox";

    //        SetViewBagCounts();

    //        const int pageSize = 25;
    //        int pageNumber = (page ?? 1);

    //        if (pageNumber < 1)
    //        {
    //            return View("~/Views/Errors/Error_404.cshtml");
    //        }

    //        // get logged in username and fetch received messages
    //        try
    //        {
    //            IQueryable<PrivateMessage> privateMessages = _db.PrivateMessages
    //                .Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
    //                .OrderByDescending(s => s.CreationDate)
    //                .ThenBy(s => s.Sender);

    //            var unreadCount = privateMessages.Count(pm => pm.IsUnread);

    //            ViewBag.InboxCount = privateMessages.Count();
    //            ViewBag.UnreadCount = unreadCount;
    //            return View("Inbox", privateMessages.ToPagedList(pageNumber, pageSize));
    //        }
    //        catch (Exception)
    //        {
    //            return View("~/Views/Errors/DbNotResponding.cshtml");
    //        }
    //    }

    //    // GET: Inbox/Unread
    //    [System.Web.Mvc.Authorize]
    //    public ActionResult InboxPrivateMessagesUnread(int? page)
    //    {
    //        ViewBag.PmView = "unread";

    //        SetViewBagCounts();

    //        const int pageSize = 25;
    //        int pageNumber = (page ?? 1);

    //        if (pageNumber < 1)
    //        {
    //            return View("~/Views/Errors/Error_404.cshtml");
    //        }

    //        // get logged in username and fetch unread private messages
    //        try
    //        {
    //            IQueryable<PrivateMessage> privateMessages = _db.PrivateMessages
    //                .Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
    //                .OrderByDescending(s => s.CreationDate)
    //                .ThenBy(s => s.Sender);

    //            ViewBag.InboxCount = privateMessages.Count();
    //            var unreadMessages = privateMessages.Where(pm => pm.IsUnread);
    //            ViewBag.UnreadCount = unreadMessages.Count();
    //            return View("InboxUnread", unreadMessages.ToPagedList(pageNumber, pageSize));
    //        }
    //        catch (Exception)
    //        {
    //            return View("~/Views/Errors/DbNotResponding.cshtml");
    //        }
    //    }

    //    // GET: InboxCommentReplies
    //    [System.Web.Mvc.Authorize]
    //    public ActionResult InboxCommentReplies(int? page)
    //    {
    //        ViewBag.PmView = "inbox";
    //        SetViewBagCounts();
    //        const int pageSize = 25;
    //        int pageNumber = (page ?? 0);

    //        if (pageNumber < 0)
    //        {
    //            return View("~/Views/Errors/Error_404.cshtml");
    //        }

    //        // get logged in username and fetch received comment replies
    //        try
    //        {
    //            IQueryable<CommentReplyNotification> commentReplyNotifications = _db.CommentReplyNotifications.Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
    //            IQueryable<Comment> commentReplies = _db.Comments.Where(p => commentReplyNotifications.Any(p2 => p2.CommentID == p.ID)).OrderByDescending(s => s.CreationDate);

    //            // mark all unread messages as read as soon as the inbox is served, except for manually marked as unread
    //            if (commentReplyNotifications.Any())
    //            {
    //                var unreadCommentReplies = commentReplyNotifications.Where(s => s.IsUnread && s.MarkedAsUnread == false);

    //                // todo: implement a delay in the marking of messages as read until the returned inbox view is rendered
    //                if (unreadCommentReplies.Any())
    //                {
    //                    foreach (var singleCommentReply in unreadCommentReplies)
    //                    {
    //                        // status: true = unread, false = read
    //                        singleCommentReply.IsUnread = false;
    //                    }
    //                    _db.SaveChanges();
    //                    EventNotification.Instance.SendMessageNotice(User.Identity.Name, null, Domain.Models.MessageTypeFlag.All, null, null);
    //                    // update notification icon
    //                    //UpdateNotificationCounts();
    //                }
    //            }

    //            ViewBag.CommentRepliesCount = commentReplyNotifications.Count();

    //            PaginatedList<Comment> paginatedComments = new PaginatedList<Comment>(commentReplies, page ?? 0, pageSize);
    //            return View("InboxCommentReplies", paginatedComments);
    //        }
    //        catch (Exception)
    //        {
    //            return View("~/Views/Errors/DbNotResponding.cshtml");
    //        }
    //    }

    //    // GET: InboxPostReplies
    //    [System.Web.Mvc.Authorize]
    //    public ActionResult InboxPostReplies(int? page)
    //    {
    //        ViewBag.PmView = "inbox";
    //        SetViewBagCounts();
    //        const int pageSize = 25;
    //        int pageNumber = (page ?? 0);

    //        if (pageNumber < 0)
    //        {
    //            return View("~/Views/Errors/Error_404.cshtml");
    //        }

    //        // get logged in username and fetch received post replies
    //        try
    //        {
    //            IQueryable<SubmissionReplyNotification> postReplyNotifications = _db.SubmissionReplyNotifications.Where(s => s.Recipient.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
    //            IQueryable<Comment> postReplies = _db.Comments.Where(p => postReplyNotifications.Any(p2 => p2.CommentID == p.ID)).OrderByDescending(s => s.CreationDate);

    //            // mark all unread messages as read as soon as the inbox is served, except for manually marked as unread
    //            if (postReplyNotifications.Any())
    //            {
    //                var unreadPostReplies = postReplyNotifications.Where(s => s.IsUnread && s.MarkedAsUnread == false);

    //                // todo: implement a delay in the marking of messages as read until the returned inbox view is rendered
    //                if (unreadPostReplies.Any())
    //                {
    //                    foreach (var singlePostReply in unreadPostReplies)
    //                    {
    //                        // status: true = unread, false = read
    //                        singlePostReply.IsUnread = false;
    //                    }
    //                    _db.SaveChanges();

    //                    EventNotification.Instance.SendMessageNotice(User.Identity.Name, null, Domain.Models.MessageTypeFlag.All, null, null);
    //                }
    //            }

    //            ViewBag.PostRepliesCount = postReplyNotifications.Count();

    //            PaginatedList<Comment> paginatedComments = new PaginatedList<Comment>(postReplies, page ?? 0, pageSize);
    //            return View("InboxPostReplies", paginatedComments);
    //        }
    //        catch (Exception)
    //        {
    //            return View("~/Views/Errors/DbNotResponding.cshtml");
    //        }
    //    }

    //    // GET: Sent
    //    [System.Web.Mvc.Authorize]
    //    public ActionResult Sent(int? page)
    //    {
    //        ViewBag.PmView = "sent";

    //        const int pageSize = 25;
    //        int pageNumber = (page ?? 1);

    //        if (pageNumber < 1)
    //        {
    //            return View("~/Views/Errors/Error_404.cshtml");
    //        }

    //        // get logged in username and fetch sent messages
    //        try
    //        {
    //            var privateMessages = _db.PrivateMessages
    //                .Where(s => s.Sender.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
    //                .OrderByDescending(s => s.CreationDate)
    //                .ThenBy(s => s.Recipient)
    //                .ToList().AsEnumerable();

    //            var privatemessages = privateMessages as IList<PrivateMessage> ?? privateMessages.ToList();
    //            ViewBag.OutboxCount = privatemessages.Count();
    //            return View(privatemessages.ToPagedList(pageNumber, pageSize));

    //        }
    //        catch (Exception)
    //        {
    //            return View("~/Views/Errors/DbNotResponding.cshtml");
    //        }
    //    }

    //    // GET: Compose
    //    [System.Web.Mvc.Authorize]
    //    public ActionResult Compose()
    //    {
    //        ViewBag.PmView = "compose";

    //        var recipient = Request.Params["recipient"];

    //        if (recipient != null)
    //        {
    //            ViewBag.recipient = recipient;
    //        }

    //        // return compose view
    //        return View();
    //    }

    //    // POST: Compose
    //    [System.Web.Mvc.Authorize]
    //    [HttpPost]
    //    [PreventSpam(DelayRequest = 30, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
    //    [VoatValidateAntiForgeryToken]
    //    public async Task<ActionResult> Compose([Bind(Include = "Recipient,Subject,Body")] PrivateMessageComposeViewModel privateMessage)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return View();
    //        }
    //        if (privateMessage.Recipient == null || privateMessage.Subject == null || privateMessage.Body == null) return RedirectToAction("Sent", "Messaging");

    //        if (Karma.CommentKarma(User.Identity.Name) < 100)
    //        {
    //            bool isCaptchaValid = await ReCaptchaUtility.Validate(Request);

    //            if (!isCaptchaValid)
    //            {
    //                ModelState.AddModelError(string.Empty, "Incorrect recaptcha answer.");
    //                return View();
    //            }
    //        }

    //        var message = new Domain.Models.SendMessage()
    //        {
    //            //Sender = User.Identity.Name,
    //            Recipient = privateMessage.Recipient,
    //            Subject = privateMessage.Subject,
    //            Message = privateMessage.Body
    //        };
    //        var cmd = new SendMessageCommand(message);
    //        var response = await cmd.Execute();

    //        if (response.Success)
    //        {
    //            return RedirectToAction("Sent", "Messaging");
    //        }
    //        else
    //        {
    //            ModelState.AddModelError(string.Empty, response.Message);
    //            return View();
    //        }

    //    }

    //    // POST: Send new private submission or reply
    //    [System.Web.Mvc.Authorize]
    //    [HttpPost]
    //    [PreventSpam(DelayRequest = 30, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
    //    [VoatValidateAntiForgeryToken]
    //    public async Task<ActionResult> SendPrivateMessage([Bind(Include = "ID,Recipient,Subject,Body")] PrivateMessageComposeViewModel privateMessage)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ModelState.GetFirstErrorMessage());
    //        }
    //        if (privateMessage.Recipient == null || privateMessage.Subject == null || privateMessage.Body == null)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.OK);
    //        }

    //        var message = new Domain.Models.SendMessage()
    //        {
    //            //Sender = User.Identity.Name,
    //            Recipient = privateMessage.Recipient,
    //            Subject = privateMessage.Subject,
    //            Message = privateMessage.Body
    //        };
    //        var cmd = new SendMessageCommand(message, true); //allow replies with limited ccp
    //        var response = await cmd.Execute();
    //        if (response.Success)
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.OK);
    //        }
    //        else
    //        {
    //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, response.Message);
    //        }
    //    }

    //    [System.Web.Mvc.Authorize]
    //    [HttpPost]
    //    [VoatValidateAntiForgeryToken]
    //    [PreventSpam(DelayRequest = 3, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
    //    public JsonResult DeletePrivateMessage(int privateMessageId)
    //    {
    //        // check that the submission is owned by logged in user executing delete action
    //        var loggedInUser = User.Identity.Name;

    //        var privateMessageToDelete = _db.PrivateMessages.FirstOrDefault(s => s.Recipient.Equals(loggedInUser, StringComparison.OrdinalIgnoreCase) && s.ID == privateMessageId);

    //        if (privateMessageToDelete != null)
    //        {
    //            // delete the submission
    //            var privateMessage = _db.PrivateMessages.Find(privateMessageId);
    //            _db.PrivateMessages.Remove(privateMessage);
    //            _db.SaveChangesAsync();
    //            Response.StatusCode = 200;
    //            return Json("Message deleted.", JsonRequestBehavior.AllowGet);
    //        }
    //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
    //        return Json("Bad request.", JsonRequestBehavior.AllowGet);
    //    }

    //    [System.Web.Mvc.Authorize]
    //    [HttpPost]
    //    [VoatValidateAntiForgeryToken]
    //    [PreventSpam(DelayRequest = 3, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
    //    public JsonResult DeletePrivateMessageFromSent(int privateMessageId)
    //    {
    //        // check that the submission is owned by logged in user executing delete action
    //        var loggedInUser = User.Identity.Name;

    //        var privateMessageToDelete = _db.PrivateMessages.FirstOrDefault(s => s.Sender.Equals(loggedInUser, StringComparison.OrdinalIgnoreCase) && s.ID == privateMessageId);

    //        if (privateMessageToDelete != null)
    //        {
    //            // delete the submission
    //            var privateMessage = _db.PrivateMessages.Find(privateMessageId);
    //            _db.PrivateMessages.Remove(privateMessage);
    //            _db.SaveChangesAsync();
    //            Response.StatusCode = 200;
    //            return Json("Message deleted.", JsonRequestBehavior.AllowGet);
    //        }
    //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
    //        return Json("Bad request.", JsonRequestBehavior.AllowGet);
    //    }

    //    //// a method which triggers SignalR notification count update on client side for logged-in user
    //    //private void UpdateNotificationCounts()
    //    //{
    //    //    // get count of unread notifications
    //    //    int unreadNotifications = Voat.Utilities.UserHelper.UnreadTotalNotificationsCount(User.Identity.Name);

    //    //    var hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();
    //    //    hubContext.Clients.User(User.Identity.Name).setNotificationsPending(unreadNotifications);
    //    //}

    //    [System.Web.Mvc.Authorize]
    //    [HttpGet]
    //    [PreventSpam(DelayRequest = 3, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
    //    public async Task<JsonResult> MarkAsRead(string itemType, bool? markAll, int? itemId)
    //    {
    //        if (markAll == null)
    //        {
    //            markAll = false;
    //        }

    //        // item status: true = unread, false = read
    //        switch (itemType)
    //        {
    //            case "privateMessage":
    //                if (await MesssagingUtility.MarkPrivateMessagesAsRead((bool)markAll, User.Identity.Name, itemId))
    //                {
    //                    EventNotification.Instance.SendMessageNotice(User.Identity.Name, null, Domain.Models.MessageTypeFlag.All, null, null);
    //                    //UpdateNotificationCounts(); // update notification icon
    //                    Response.StatusCode = 200;
    //                    return Json("Item marked as read.", JsonRequestBehavior.AllowGet);
    //                }
    //                break;
    //        }

    //        Response.StatusCode = (int)HttpStatusCode.BadRequest;
    //        return Json("Bad request.", JsonRequestBehavior.AllowGet);
    //    }
    //}
}
