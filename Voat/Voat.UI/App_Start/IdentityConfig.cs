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

using System;
using System.Threading.Tasks;
using Voat.Configuration;

namespace Voat.App_Start
{
    public class IdentityConfig
    {
        //CORE_PORT: Unsupported
        public class EmailService 
        {
            public EmailService()
            {
                throw new NotImplementedException("Core port not ported");
            }
        }
        /*
        public class EmailService : IIdentityMessageService
        {
            public async Task SendAsync(IdentityMessage message)
            {

                var content = new SendGrid.Helpers.Mail.Content();
                content.Type = "text/html";
                content.Value = message.Body;

                var msg = new SendGrid.Helpers.Mail.Mail(
                    new SendGrid.Helpers.Mail.Email(Settings.EmailAddress, Settings.SiteName),
                    message.Subject,
                    new SendGrid.Helpers.Mail.Email(message.Destination),
                    content
                    );

                var trackingSettings = new SendGrid.Helpers.Mail.TrackingSettings();
                trackingSettings.ClickTracking = new SendGrid.Helpers.Mail.ClickTracking();
                trackingSettings.OpenTracking = new SendGrid.Helpers.Mail.OpenTracking();
                trackingSettings.ClickTracking.Enable = false;
                trackingSettings.OpenTracking.Enable = false;
                msg.TrackingSettings = trackingSettings;

                dynamic sendGridClient = new SendGridAPIClient(Settings.EmailServiceKey);

                var response = await sendGridClient.client.mail.send.post(requestBody: msg.Get());

            }
        }
        */
    }
}
