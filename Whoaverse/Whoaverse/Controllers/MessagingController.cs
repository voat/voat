/*
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

using System.Web.Mvc;

namespace Whoaverse.Controllers
{
    public class MessagingController : Controller
    {
        // GET: Inbox
        [Authorize]
        public ActionResult Inbox()
        {
            ViewBag.SelectedSubverse = "inbox";
            // get logged in username and fetch received messages

            // return inbox view
            return View();
        }

        // GET: InboxCommentReplies
        [Authorize]
        public ActionResult InboxCommentReplies()
        {
            ViewBag.SelectedSubverse = "inboxcommentreplies";
            // get logged in username and fetch received comment replies

            // return comment replies inbox view
            return View();
        }

        // GET: InboxSubmissionReplies
        [Authorize]
        public ActionResult InboxSubmissionReplies()
        {
            ViewBag.SelectedSubverse = "inboxsubmissionreplies";
            // get logged in username and fetch received submission replies

            // return submission replies inbox view
            return View();
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
        public ActionResult Sent()
        {
            ViewBag.SelectedSubverse = "sent";
            // get logged in username and fetch sent messages

            // return sent view
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

    }
}