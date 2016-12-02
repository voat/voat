using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using SendGrid;
using Voat.Configuration;
using Voat.Utilities;

namespace Voat.App_Start
{
    public class IdentityConfig
    {
        public class EmailService : IIdentityMessageService
        {
            public async Task SendAsync(IdentityMessage message)
            {

                var content = new SendGrid.Helpers.Mail.Content();
                content.Type = "text/html";
                content.Value = message.Body;

                var msg = new SendGrid.Helpers.Mail.Mail(
                    new SendGrid.Helpers.Mail.Email("noreply@voat.co", CONSTANTS.SYSTEM_USER_NAME),
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
    }
}