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

using System.Net;
using System.Web.Mvc;
using Voat.Data.Models;

namespace Voat.Controllers
{
    public class HtmlElementsController : Controller
    {
        private readonly voatEntities _db = new voatEntities();

        // GET: CommentReplyForm
        public ActionResult CommentReplyForm(int? parentCommentId, int? messageId)
        {
            if (parentCommentId == null || messageId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ViewBag.MessageId = messageId;
            ViewBag.ParentCommentId = parentCommentId;

            return PartialView("~/Views/AjaxViews/_CommentReplyForm.cshtml");
        }

        // GET: PrivateMessageReplyForm
        public ActionResult PrivateMessageReplyForm(int? parentPrivateMessageId, string recipient, string subject)
        {
            if (parentPrivateMessageId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            ViewBag.ParentPrivateMessageId = parentPrivateMessageId;
            ViewBag.Recipient = recipient;
            ViewBag.Subject = subject;

            return PartialView("~/Views/AjaxViews/_PrivateMessageReplyForm.cshtml");
        }
    }
}