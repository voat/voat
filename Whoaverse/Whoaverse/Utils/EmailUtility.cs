using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;

namespace Whoaverse.Utils
{
    public static class EmailUtility
    {
        public static void sendPasswordRecoveryMail(PasswordRecoveryMessage message)
        {
            #region formatter
            string text = string.Format("Please click on this link to {0}: {1}", message.Subject, message.Link);
            string html = string.Format("Please confirm your account by clicking this link: <a href=\"{0}{1}{2}",
                message.Link, "\">link</a><br/>", HttpUtility.HtmlEncode(
                string.Format(@"Or click on the copy the following link on the browser: {0}", message.Link)));
            #endregion

            MailMessage msg = new MailMessage();
            msg.From = new MailAddress("");
            msg.To.Add(new MailAddress(message.Recipient));
            msg.Subject = message.Subject;
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

            SmtpClient smtpClient = new SmtpClient(ConstantUtility.SMTPGateWay, Convert.ToInt32(587));
            var credentials = new NetworkCredential(ConstantUtility.ServerEmailAddress, ConstantUtility.EmailPassword);
            smtpClient.Credentials = credentials;
            smtpClient.EnableSsl = true;
            smtpClient.Send(msg);
        }

        public class PasswordRecoveryMessage
        {
            public string Recipient { get; set; }
            public string Subject { get; set; }
            public string Link { get; set; }

            public PasswordRecoveryMessage(string recipient, string subject, string link)
            {
                Recipient = recipient;
                Subject = subject;
                Link = link;
            }
        }
    }
}