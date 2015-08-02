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

using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Voat.Data.Models;
using Voat.Models;
using Voat.Models.ViewModels;
using Voat.Utilities;


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

        //// GET: latest single submission comment for given user
        ////HACK: The comment process needs redone. 
        //public ActionResult SingleMostRecentCommentByUser(string userName, int? messageId)
        //{
        //    if (userName == null || messageId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        //    var comment = _db.Comments
        //        .Where(c => c.Name == userName && c.MessageId == messageId)
        //        .OrderByDescending(c => c.Id)
        //        .FirstOrDefault();

        //    if (comment == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    ViewBag.CommentId = comment.Id; //why?
        //    ViewBag.rootComment = comment.ParentId == null; //why?

        //    var submission = DataCache.Submission.Retrieve(comment.MessageId.Value);
        //    var subverse = DataCache.Subverse.Retrieve(submission.Subverse);

        //    if (submission.Anonymized || subverse.anonymized_mode)
        //    {
        //        comment.Name = comment.Id.ToString(CultureInfo.InvariantCulture);
        //    }

        //    //COPYPASTA EVERYWHERE!!!!!!!!!!!!!!! Left, right, up down. EVERYWHERE. 

        //    var model = new CommentBucketViewModel(comment);
            
        //    return PartialView("~/Views/Shared/Submissions/_SubmissionComment.cshtml", model);

        //}
    }
}