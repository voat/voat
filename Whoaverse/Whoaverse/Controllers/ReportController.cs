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

using System;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using Whoaverse.Models;
using Whoaverse.Utils;

namespace Whoaverse.Controllers
{
    public class ReportController : Controller
    {
        private readonly whoaverseEntities _db = new whoaverseEntities();

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
                    var smtp = new SmtpClient();
                    var from = new MailAddress("abuse@whoaverse.com");
                    var to = new MailAddress("legal@whoaverse.com");
                    var sb = new StringBuilder();
                    var msg = new MailMessage(from, to)
                    {
                        Subject = "New comment report from " + User.Identity.Name,
                        IsBodyHtml = false
                    };

                    smtp.Host = "whoaverse.com";
                    smtp.Port = 25;

                    // format Partner Intent Email
                    sb.Append("Comment Id: " + id);
                    sb.Append(Environment.NewLine);
                    sb.Append("Subverse: " + commentSubverse);
                    sb.Append(Environment.NewLine);
                    sb.Append("Report timestamp: " + reportTimeStamp);
                    sb.Append(Environment.NewLine);
                    sb.Append("Comment permalink: " + "http://whoaverse.com/v/" + commentSubverse + "/comments/" + commentToReport.MessageId + "/" + id);
                    sb.Append(Environment.NewLine);

                    msg.Body = sb.ToString();

                    // send the email with Partner Intent data
                    smtp.Send(msg);
                    msg.Dispose();
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