using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Configuration;
using Voat.Utilities.Components;

namespace Voat.IO.Email
{
    public class SendGridEmailSender : IEmailSender
    {
        private string _connectionString = "";

        public SendGridEmailSender(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<bool> SendEmail(string emailAddress, string subject, string message)
        {
            try
            {
                var msg = SendGrid.Helpers.Mail.MailHelper.CreateSingleEmail(
                    new SendGrid.Helpers.Mail.EmailAddress(VoatSettings.Instance.EmailAddress, VoatSettings.Instance.SiteName),
                    new SendGrid.Helpers.Mail.EmailAddress(emailAddress),
                    subject,
                    null,
                    message);

                var trackingSettings = new SendGrid.Helpers.Mail.TrackingSettings();
                trackingSettings.ClickTracking = new SendGrid.Helpers.Mail.ClickTracking();
                trackingSettings.OpenTracking = new SendGrid.Helpers.Mail.OpenTracking();

                trackingSettings.ClickTracking.Enable = false;
                trackingSettings.OpenTracking.Enable = false;

                msg.TrackingSettings = trackingSettings;

                var sendGridClient = new SendGrid.SendGridClient(_connectionString);

                var response = await sendGridClient.SendEmailAsync(msg);

                return response.StatusCode == System.Net.HttpStatusCode.Accepted;

            }
            catch (Exception ex)
            {
                EventLogger.Log(ex, VoatSettings.Instance.Origin);
                return false;
            }
        }
    }
}
