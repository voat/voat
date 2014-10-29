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

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Whoaverse.Models;
using Whoaverse.Utils;

namespace Whoaverse.Controllers
{
    public class AjaxGatewayController : Controller
    {
        private readonly whoaverseEntities _db = new whoaverseEntities();

        // GET: MessageContent
        public ActionResult MessageContent(int? messageId)
        {
            var message = _db.Messages.Find(messageId);

            if (message != null)
            {
                if (message.MessageContent != null)
                {
                    return PartialView("~/Views/AjaxViews/_MessageContent.cshtml", message);
                }
                message.MessageContent = "This message only has a title.";
                return PartialView("~/Views/AjaxViews/_MessageContent.cshtml", message);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // GET: subverse link flairs for selected subverse
        [Authorize]
        public ActionResult SubverseLinkFlairs(string subversetoshow, int? messageId)
        {
            // get model for selected subverse
            var subverseModel = _db.Subverses.Find(subversetoshow);

            if (subverseModel == null || messageId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var submissionId = _db.Messages.Find(messageId);
            if (submissionId == null || submissionId.Subverses.name != subversetoshow)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            // check if caller is subverse owner or moderator, if not, deny listing
            if (!Utils.User.IsUserSubverseModerator(User.Identity.Name, subversetoshow) &&
                !Utils.User.IsUserSubverseAdmin(User.Identity.Name, subversetoshow))
                return new HttpUnauthorizedResult();
            var subverseLinkFlairs = _db.Subverseflairsettings
                .Where(n => n.Subversename == subversetoshow)
                .Take(10)
                .ToList()
                .OrderBy(s => s.Id);

            ViewBag.SubmissionId = messageId;
            ViewBag.SubverseName = subversetoshow;

            return PartialView("~/Views/AjaxViews/_LinkFlairSelectDialog.cshtml", subverseLinkFlairs);
        }

        // GET: title from Uri
        [Authorize]
        public string TitleFromUri()
        {
            var uri = Request.Params["uri"];
            return UrlUtility.GetTitleFromUri(uri);
        }

        // GET: subverse names containing search term (used for autocomplete on new submission views)
        public JsonResult AutocompleteSubverseName(string term)
        {
            var resultList = new List<string>();

            var subverseNameSuggestions = _db.Subverses
                .Where(s => s.name.ToLower().StartsWith(term))
                .Take(10).ToArray();

            // jquery UI doesn't play nice with key value pairs so we have to build a simple string array
            if (!subverseNameSuggestions.Any()) return Json(resultList, JsonRequestBehavior.AllowGet);
            resultList.AddRange(subverseNameSuggestions.Select(item => item.name));

            return Json(resultList, JsonRequestBehavior.AllowGet);
        }
    }
}