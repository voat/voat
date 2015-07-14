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

using System;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using Voat.Models;
using Voat.Utils;

namespace Voat.Controllers
{
    public class ReportController : Controller
    {
        private readonly voatEntities _db = new voatEntities();

        // POST: reportcomment
        [HttpPost]
        [Authorize]
        [PreventSpam(DelayRequest = 30, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        public ActionResult ReportComment(int id)
        {
            var commentToReport = _db.Comments.Find(id);

            if (commentToReport != null)
            {
                // prepare report headers
                var commentSubverse = commentToReport.Message.Subverse;
                var reportTimeStamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);

                // send the report
                try
                {
                    var from = new MailAddress("abuse@whoaverse.com");
                    var to = new MailAddress("legal@voat.co");
                    var msg = new MailMessage(from, to)
                    {
                        Subject = "New comment report from " + User.Identity.Name,
                        IsBodyHtml = false
                    };

                    // format report email
                    var sb = new StringBuilder();
                    sb.Append("Comment Id: " + id);
                    sb.Append(Environment.NewLine);
                    sb.Append("Subverse: " + commentSubverse);
                    sb.Append(Environment.NewLine);
                    sb.Append("Report timestamp: " + reportTimeStamp);
                    sb.Append(Environment.NewLine);
                    sb.Append("Comment permalink: " + "http://voat.co/v/" + commentSubverse + "/comments/" + commentToReport.MessageId + "/" + id);
                    sb.Append(Environment.NewLine);

                    msg.Body = sb.ToString();

                    EmailUtility.SendEmail(msg);
                }
                catch (Exception)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.ServiceUnavailable, "Service Unavailable");
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK, "OK");
        }
    }
}