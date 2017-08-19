using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Voat.IO.Email
{
    public interface IEmailSender
    {
        Task<bool> SendEmail(string emailAddress, string subject, string message);
    }
    public abstract class EmailSender
    {
        private static IEmailSender _emailSender = null;

        public static IEmailSender Instance
        {
            get
            {
                if (_emailSender == null)
                {
                    var handlerInfo = EmailConfigurationSettings.Instance.Handler;
                    if (handlerInfo != null)
                    {
                        _emailSender = handlerInfo.Construct<IEmailSender>();
                    }
                }
                return _emailSender;
            }
            set
            {
                _emailSender = value;
            }
        }
    }
}
