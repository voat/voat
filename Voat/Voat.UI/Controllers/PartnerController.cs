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

using System;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using Voat.Models;
using Voat.UI.Utilities;
using Voat.Utilities;

namespace Voat.Controllers
{
    public class PartnerController : Controller
    {
        // GET: PartnerIntentRegistration
        public ActionResult PartnerProgramInformation()
        {
            return View();
        }

        // GET: PartnerIntentRegistration
        [RequireHttps]
        [Authorize]
        public ActionResult PartnerIntentRegistration()
        {
            var model = new PartnerIntent {UserName = User.Identity.Name};
            return View(model);
        }

        [Authorize]
        [RequireHttps]
        [HttpPost]
        [PreventSpam(DelayRequest = 300, ErrorMessage = "Sorry, you are doing that too fast. Please try again later.")]
        [ValidateCaptcha]
        public ActionResult PartnerIntentRegistration(PartnerIntent partnerModel)
        {
            if (!ModelState.IsValid) return View();

            var from = new MailAddress(partnerModel.Email);
            var to = new MailAddress("legal@voat.co");
            var sb = new StringBuilder();
            var msg = new MailMessage(@from, to)
            {
                Subject = "New Partner Intent registration from " + partnerModel.FullName,
                IsBodyHtml = false
            };

            // format Partner Intent Email
            sb.Append("Full name: " + partnerModel.FullName);
            sb.Append(Environment.NewLine);
            sb.Append("Email: " + partnerModel.Email);
            sb.Append(Environment.NewLine);
            sb.Append("Mailing address: " + partnerModel.MailingAddress);
            sb.Append(Environment.NewLine);
            sb.Append("City: " + partnerModel.City);
            sb.Append(Environment.NewLine);
            sb.Append("Country: " + partnerModel.Country);
            sb.Append(Environment.NewLine);
            sb.Append("Phone number: " + partnerModel.PhoneNumber);
            sb.Append(Environment.NewLine);
            sb.Append("Username: " + partnerModel.UserName);
            sb.Append(Environment.NewLine);

            msg.Body = sb.ToString();

            // send the email with Partner Intent data
            //if (EmailUtility.SendEmail(msg))
            //{
            //    msg.Dispose();
            //    ViewBag.SelectedSubverse = string.Empty;
            //    return View("~/Views/Partner/PartnerProgramIntentSent.cshtml");
            //}

            ViewBag.SelectedSubverse = string.Empty;
            return View("~/Views/Errors/Error.cshtml");
        }
    }
}