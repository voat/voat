using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using SendGrid;
using Voat.Configuration;

namespace Voat.App_Start
{
    public class IdentityConfig
    {
        public class EmailService : IIdentityMessageService
        {
            public Task SendAsync(IdentityMessage message)
            {
                var sgMessage = new SendGridMessage { From = new MailAddress("noreply@voat.co", "Voat") };
                
                sgMessage.AddTo(message.Destination);
                sgMessage.Subject = message.Subject;
                sgMessage.Html = message.Body;
                sgMessage.Text = message.Body;
                
                sgMessage.DisableClickTracking();
                sgMessage.DisableOpenTracking();

                var transportWeb = new Web(Settings.EmailServiceKey);
                return transportWeb.DeliverAsync(sgMessage);
            }
        }
    }
}