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

using System.Net;
using System.Web.Mvc;
using Whoaverse.Models;

namespace Whoaverse.Controllers
{
    public class AjaxGatewayController : Controller
    {
        private whoaverseEntities db = new whoaverseEntities();

        // GET: CommentReplyForm
        public ActionResult MessageContent(int? messageId)
        {
            var message = db.Messages.Find(messageId);            

            if (message != null)
            {
                if (message.MessageContent != null)
                {
                    return PartialView("~/Views/AjaxViews/_MessageContent.cshtml", message);
                }
                else
                {
                    message.MessageContent = "This message only has a title.";
                    return PartialView("~/Views/AjaxViews/_MessageContent.cshtml", message);
                }                
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }
    }
}