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

using Microsoft.AspNetCore.Mvc;
using System.Net;
using Voat.Models.ViewModels;

namespace Voat.Controllers
{
    public class HtmlElementsController : BaseController
    {
        //CORE_PORT: No direct access
        //private readonly voatEntities _db = new VoatUIDataContextAccessor();

        // GET: CommentReplyForm
        public ActionResult CommentReplyForm(int? parentCommentId, int? messageId)
        {
            if (parentCommentId == null || messageId == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            ViewBag.MessageId = messageId;
            ViewBag.ParentCommentId = parentCommentId;

            return PartialView("~/Views/AjaxViews/_CommentReplyForm.cshtml");
        }

        // GET: PrivateMessageReplyForm
        public ActionResult PrivateMessageReplyForm(int? parentPrivateMessageId, string recipient, string subject)
        {
            if (parentPrivateMessageId == null)
            {
                return HybridError(ErrorViewModel.GetErrorViewModel(ErrorType.NotFound));
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.ParentPrivateMessageId = parentPrivateMessageId;
            ViewBag.Recipient = recipient;
            ViewBag.Subject = subject;

            return PartialView("~/Views/AjaxViews/_PrivateMessageReplyForm.cshtml");
        }
    }
}
