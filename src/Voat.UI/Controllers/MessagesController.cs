#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using System.Threading.Tasks;


using Voat.Utilities;
using Voat.UI.Utilities;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Domain.Query;

using Voat.Models.ViewModels;
using System.Net;
using System.Linq;
using System;
using Voat.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Voat.Common;
using Voat.Http;
using Voat.Http.Filters;

namespace Voat.Controllers
{

    [Authorize]
    public class MessagesController : BaseController
    {
        
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.Context = new MessageContextViewModel() { ViewerContext = UserDefinition.Parse(User.Identity.Name) };
            base.OnActionExecuting(filterContext);
        }

       

        public async Task<ActionResult> Index(int? page = null)
        {


            var unread = new QueryAllMessageCounts(User, MessageTypeFlag.All, MessageState.Unread).SetUserContext(User);
            var counts = await unread.ExecuteAsync();

            //var unread = new QueryMessageCounts(User, MessageTypeFlag.All, MessageState.Unread).SetUserContext(User);
            //var counts = await unread.ExecuteAsync();


            if (counts.Any(x => x.UserDefinition.Type == IdentityType.Subverse && x.Total > 0))
            {
                //is admin, send them to notfication page becasue they have smail 
                SetMenuNavigationModel("Notifications", MenuType.UserMessages);
                return View("Notifications", counts);
            }
            else
            {
                var userCounts = counts.FirstOrDefault(x => x.UserDefinition.Type == IdentityType.User);

                if (userCounts.Total > 0)
                {
                    if (userCounts.Counts.Any(x => x.Type == MessageType.Private))
                    {
                        return await Private(page);
                    }
                    else if (userCounts.Counts.Any(x => x.Type == MessageType.SubmissionMention))
                    {
                        return await Mentions(ContentType.Submission, page);
                    }
                    else if (userCounts.Counts.Any(x => x.Type == MessageType.CommentMention))
                    {
                        return await Mentions(ContentType.Comment, page);
                    }
                    else if (userCounts.Counts.Any(x => x.Type == MessageType.SubmissionReply))
                    {
                        return await Replies(ContentType.Submission, page);
                    }
                    else if (userCounts.Counts.Any(x => x.Type == MessageType.CommentReply))
                    {
                        return await Replies(ContentType.Comment, page);
                    }
                }
                return await Private(page);
            }

           
        }

        private int SetPage(int? page = null)
        {
            return (page.HasValue && page.Value >= 0 ? page.Value : 0);
        }

        private void SetMenuNavigationModel(string name, MenuType menuType, string subverse = null)
        {
            string suffix = "/messages";
            ViewBag.NavigationViewModel = new NavigationViewModel()
            {
                Description = (String.IsNullOrEmpty(subverse) ? "Messages" : String.Format("v/{0} Smail", subverse)),
                Name = name,
                MenuType = menuType,
                BasePath = (String.IsNullOrEmpty(subverse) ? suffix : String.Format("{0}/about{1}", VoatUrlFormatter.BasePath(new DomainReference(DomainType.Subverse, subverse)), suffix)),
                Sort = null
            };
        }

        public async Task<ActionResult> Private(int? page = null)
        {

            //ViewBag.PmView = "inbox";
            //ViewBag.Title = "Inbox";

            var q = new QueryMessages(User, MessageTypeFlag.Private, MessageState.All, false).SetUserContext(User);
            q.PageNumber = SetPage(page);

            var result = await q.ExecuteAsync();

            var pagedList = new PaginatedList<Message>(result, q.PageNumber, q.PageCount);

            SetMenuNavigationModel("Inbox", MenuType.UserMessages);

            return View("Index", pagedList);
        }

        public async Task<ActionResult> Sent(int? page = null)
        {

            //ViewBag.PmView = "sent";
            //ViewBag.Title = "Sent";

            var q = new QueryMessages(User, MessageTypeFlag.Sent, MessageState.All, true).SetUserContext(User);
            q.PageNumber = SetPage(page);
            var result = await q.ExecuteAsync();

            var pagedList = new PaginatedList<Message>(result, q.PageNumber, q.PageCount);

            SetMenuNavigationModel("Sent", MenuType.UserMessages);
            ViewBag.Title = "Sent";

            return View("Index", pagedList);

        }
        public async Task<ActionResult> Replies(ContentType? type = null, int? page = null)
        {

            //ViewBag.PmView = "inbox";
            //ViewBag.Title = "Replies";

            var contentType = MessageTypeFlag.CommentReply | MessageTypeFlag.SubmissionReply;
            if (type.HasValue)
            {
                contentType = type.Value == ContentType.Comment ? MessageTypeFlag.CommentReply : MessageTypeFlag.SubmissionReply;
                ViewBag.Title = type.ToString() + " Replies";
            }

            var q = new QueryMessages(User, contentType, MessageState.All, true).SetUserContext(User);
            q.PageNumber = SetPage(page);

            var result = await q.ExecuteAsync();

            var pagedList = new PaginatedList<Message>(result, q.PageNumber, q.PageCount);

            SetMenuNavigationModel("Replies", MenuType.UserMessages);

            return View("Index", pagedList);

        }
        public async Task<ActionResult> Mentions(ContentType? type = null, int? page = null)
        {

            //ViewBag.PmView = "inbox";
            //ViewBag.Title = "Mentions";

            var contentType = MessageTypeFlag.CommentMention | MessageTypeFlag.SubmissionMention;
            if (type.HasValue)
            {
                contentType = type.Value == ContentType.Comment ? MessageTypeFlag.CommentMention : MessageTypeFlag.SubmissionMention;
                ViewBag.Title = type.ToString() + " Mentions";
            }

            var q = new QueryMessages(User, contentType, MessageState.All, true).SetUserContext(User);
            q.PageNumber = SetPage(page);

            var result = await q.ExecuteAsync();
            var pagedList = new PaginatedList<Message>(result, q.PageNumber, q.PageCount);

            SetMenuNavigationModel("Mentions", MenuType.UserMessages);

            return View("Index", pagedList);
        }


        [Authorize]
        public async Task<ActionResult> Notifications()
        {
            //ViewBag.PmView = "notifications";
            //ViewBag.selectedView = "notifications";
            //ViewBag.Title = "All Unread Notifications";
            //ViewBag.SelectedSubverse = "";
            var q = new QueryAllMessageCounts(User, MessageTypeFlag.All, MessageState.Unread).SetUserContext(User);
            var model = await q.ExecuteAsync();

            SetMenuNavigationModel("Notifications", MenuType.UserMessages);

            return View(model);
        }

        // GET: Compose
        [Authorize]
        public ActionResult Compose()
        {

            //CORE_PORT: Ported correctly?
            //var recipient = Request.Params["recipient"];
            //var subject = Request.Params["subject"];
            var recipient = Request.Query["recipient"].FirstOrDefault();
            var subject = Request.Query["subject"].FirstOrDefault();
            var subverse = (string)RouteData.Values["subverse"];
            var model = new NewMessageViewModel() { Recipient = recipient, Subject = subject };

            var userData = UserData;
            model.RequireCaptcha = userData.Information.CommentPoints.Sum < VoatSettings.Instance.MinimumCommentPointsForCaptchaMessaging && VoatSettings.Instance.CaptchaEnabled;

            if (!string.IsNullOrEmpty(subverse))
            {
                if (!ModeratorPermission.HasPermission(User, subverse, ModeratorAction.SendMail))
                {
                    return RedirectToAction("Home", "Index");
                }
                ViewBag.PmView = "mod";
                model.Sender = UserDefinition.Format(subverse, IdentityType.Subverse);
                SetMenuNavigationModel("Compose", MenuType.Smail, subverse);
            }
            else
            {
                SetMenuNavigationModel("Compose", MenuType.UserMessages);

            }


            // return compose view
            return View(model);
        }

        // POST: Compose
        [HttpPost]
        [PreventSpam(30)]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> Compose(NewMessageViewModel message)
        {

            //ViewBag.PmView = "compose";
            //ViewBag.Title = "Compose";

            //set this incase invalid submittal 
            var userData = UserData;
            message.RequireCaptcha = userData.Information.CommentPoints.Sum < VoatSettings.Instance.MinimumCommentPointsForCaptchaMessaging && VoatSettings.Instance.CaptchaEnabled;

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
            var cmd = new SendMessageCommand(sendMessage, false, true).SetUserContext(User);
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
        [PreventSpam(30)]
        [VoatValidateAntiForgeryToken]
        public async Task<ActionResult> Reply(MessageReplyViewModel message)
        {
            if (!ModelState.IsValid)
            {
                PreventSpamAttribute.Reset(HttpContext);
                
                if (Request.IsAjaxRequest())
                {
                    return new JsonResult(CommandResponse.FromStatus(Status.Invalid, ModelState.GetFirstErrorMessage()));
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

            var cmd = new SendMessageReplyCommand(message.ID, message.Body).SetUserContext(User);
            var response = await cmd.Execute();

            return JsonResult(response);
          
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
            var cmd = new MarkMessagesCommand(ownerName, ownerType, type, markAction, id).SetUserContext(User);
            var response = await cmd.Execute();

            return JsonResult(response);
            
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

            var cmd = new DeleteMessagesCommand(ownerName, ownerType, type, id).SetUserContext(User);
            var response = await cmd.Execute();

            return JsonResult(response);
       
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
            if (!ModeratorPermission.HasPermission(User, subverse, ModeratorAction.ReadMail))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Context = new MessageContextViewModel() { ViewerContext = new UserDefinition() { Name = subverse, Type = IdentityType.Subverse }};

            var qSub = new QuerySubverse(subverse);
            var sub = await qSub.ExecuteAsync();

            if (sub == null)
            {
                return ErrorView(ErrorViewModel.GetErrorViewModel(ErrorType.SubverseNotFound));
            }
            //ViewBag.PmView = "mod";
            //ViewBag.Title = string.Format("v/{0} {1}", sub.Name, (type == MessageTypeFlag.Sent ? "Sent" : "Inbox"));

            var q = new QueryMessages(sub.Name, IdentityType.Subverse, type, MessageState.All, false).SetUserContext(User);
            q.PageNumber = SetPage(page);
            var result = await q.ExecuteAsync();

            var pagedList = new PaginatedList<Message>(result, q.PageNumber, q.PageCount);

            //TODO: This needs to be the Smail Menu, right now it shows user menu
            var name = type == MessageTypeFlag.Sent ? "Sent" : "Inbox";
            ViewBag.Title = name;
            SetMenuNavigationModel(name, MenuType.Smail, subverse);

            return View("Index", pagedList);
        }

    }
}
