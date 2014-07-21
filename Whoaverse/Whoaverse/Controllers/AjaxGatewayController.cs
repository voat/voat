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
using System.Linq;
using Whoaverse.Utils;

namespace Whoaverse.Controllers
{
    public class AjaxGatewayController : Controller
    {
        private whoaverseEntities db = new whoaverseEntities();

        // GET: MessageContent
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

        // GET: subverse link flairs for selected subverse
        [Authorize]
        public ActionResult SubverseLinkFlairs(string subversetoshow, int? messageId)
        {
            // get model for selected subverse
            var subverseModel = db.Subverses.Find(subversetoshow);

            if (subverseModel != null && messageId != null)
            {
                var submissionId = db.Messages.Find(messageId);
                if (submissionId != null && submissionId.Subverses.name == subversetoshow)
                {
                    // check if caller is subverse owner or moderator, if not, deny listing
                    if (Whoaverse.Utils.User.IsUserSubverseModerator(User.Identity.Name, subversetoshow) || Whoaverse.Utils.User.IsUserSubverseAdmin(User.Identity.Name, subversetoshow))
                    {
                        var subverseLinkFlairs = db.Subverseflairsettings.OrderBy(s => s.Id)
                        .Where(n => n.Subversename == subversetoshow)
                        .Take(10)
                        .ToList();

                        ViewBag.SubmissionId = messageId;
                        ViewBag.SubverseName = subversetoshow;

                        return PartialView("~/Views/AjaxViews/_LinkFlairSelectDialog.cshtml", subverseLinkFlairs);
                    }
                    else
                    {
                        return new HttpUnauthorizedResult();
                    }
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }
    }
}