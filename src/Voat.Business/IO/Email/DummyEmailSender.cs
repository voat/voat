using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using Voat.Configuration;
using System.Threading.Tasks;

namespace Voat.IO.Email
{
   
    public class DummyEmailSender : IEmailSender
    {

        public DummyEmailSender()
        {
            
        }

        public Task<bool> SendEmail(string emailAddress, string subject, string message)
        {
            return Task.FromResult(false);
        }
    }
}
