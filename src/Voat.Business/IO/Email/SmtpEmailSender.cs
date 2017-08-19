using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using Voat.Configuration;
using System.Threading.Tasks;

namespace Voat.IO.Email
{
   
    public class SmtpEmailSender : IEmailSender
    {
        private string _host;
        private int _port = 25;

        public SmtpEmailSender(string host, int port)
        {
            _host = host;
            if (port > 0)
            {
                _port = port;
            }
        }

        public async Task<bool> SendEmail(string emailAddress, string subject, string message)
        {
            SmtpClient client = new SmtpClient(_host, _port);
            var mailMessage = new MailMessage();
            mailMessage.To.Add(new MailAddress(emailAddress));
            mailMessage.From = new MailAddress(VoatSettings.Instance.EmailAddress, VoatSettings.Instance.SiteName);
            mailMessage.Subject = subject;
            mailMessage.Body = message;
            try
            {
                throw new NotImplementedException("This swallows errors and kills the process if connection is refused. Needs research, will fix later.");
                await client.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                return false;
;            }
        }
    }
}
