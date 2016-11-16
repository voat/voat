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
using Voat.Configuration;

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

        private int SetPage(int? page = null)
        {
            return (page.HasValue && page.Value >= 0 ? page.Value : 0);
        }
        public async Task<ActionResult> Private(int? page = null)
        {

            ViewBag.PmView = "inbox";
            ViewBag.Title = "Inbox";

            var q = new QueryMessages(MessageTypeFlag.Private, MessageState.All, false);
            q.PageNumber = SetPage(page);

            var result = await q.ExecuteAsync();

            var pagedList = FakePaging(result, q.PageNumber, q.PageCount);

            return View("Index", pagedList);

        }

        public async Task<ActionResult> Sent(int? page = null)
        {

            ViewBag.PmView = "sent";
            ViewBag.Title = "Sent";

            var q = new QueryMessages(MessageTypeFlag.Sent, MessageState.All, true);
            q.PageNumber = SetPage(page);
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
            q.PageNumber = SetPage(page);

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
            q.PageNumber = SetPage(page);

            var result = await q.ExecuteAsync();
            var pagedList = FakePaging(result, q.PageNumber, q.PageCount);

            return View("Index", pagedList);
        }


        [Authorize]
        public async Task<ActionResult> Notifications()
        {
            ViewBag.PmView = "notifications";
            ViewBag.selectedView = "notifications";
            ViewBag.Title = "All Unread Notifications";
            ViewBag.SelectedSubverse = "";
            var q = new QueryAllMessageCounts(Domain.Models.MessageTypeFlag.All, Domain.Models.MessageState.Unread);
            var model = await q.ExecuteAsync();
            return View(model);
        }

        // GET: Compose
        [System.Web.Mvc.Authorize]
        public ActionResult Compose()
        {
            ViewBag.PmView = "compose";
            ViewBag.Title = "Compose";

            var recipient = Request.Params["recipient"];
            var subject = Request.Params["subject"];
            var subverse = (string)RouteData.Values["subverse"];
            var model = new NewMessageViewModel() { Recipient = recipient, Subject = subject };

            var userData = UserData;
            model.RequireCaptcha = userData.Information.CommentPoints.Sum < Settings.MinimumCommentPointsForCaptchaMessaging && !Settings.CaptchaDisabled;

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

            ViewBag.PmView = "compose";
            ViewBag.Title = "Compose";

            //set this incase invalid submittal 
            var userData = UserData;
            message.RequireCaptcha = userData.Information.CommentPoints.Sum < Settings.MinimumCommentPointsForCaptchaMessaging && !Settings.CaptchaDisabled;

            if (!ModelState.IsValid)
            {
                return View(message);
            }

            if (message.Recipient == null || message.Subject == null || message.Body == null)
            {
                return RedirectToAction("Sent", "Messages");
            }

            if (message.RequireCaptcha)
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
                PreventSpamAttribute.Reset();
                if (Request.IsAjaxRequest())
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ModelState.GetFirstErrorMessage());
                }
                else
                {
                    return View();
                }
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

        public async Task<ActionResult> SubverseIndex(string subverse, MessageTypeFlag type, MessageState? state = null, int? page = null)
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
            q.PageNumber = SetPage(page);
            var result = await q.ExecuteAsync();

            var pagedList = FakePaging(result, q.PageNumber, q.PageCount);
            return View("Index", pagedList);
        }

    }
}
