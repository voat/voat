using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Voat.Utilities.Components;

namespace Voat.IO.Email
{
    public interface IEmailSender
    {
        Task<bool> SendEmail(string emailAddress, string subject, string message);
    }
    public abstract class EmailSender
    {
        private static IEmailSender _instance = null;

        public static IEmailSender Instance
        {
            get
            {
                if (_instance == null)
                {
                    var configSettings = EmailConfigurationSettings.Instance;
                    var setInstance = new Action<EmailConfigurationSettings>(settings =>
                    {
                        try
                        {
                            var handler = settings.Handler;
                            if (handler != null)
                            {
                                Debug.WriteLine($"EmailSender.Instance.Construct({handler.Type})");
                                _instance = handler.Construct<IEmailSender>();
                            }
                        }
                        catch (Exception ex)
                        {
                            EventLogger.Log(ex);
                        }
                        finally
                        {
                            if (_instance == null)
                            {
                                _instance = new DummyEmailSender();
                            }
                        }
                    });

                    setInstance(EmailConfigurationSettings.Instance);

                    //reset if update
                    configSettings.OnUpdate += (sender, settings) =>
                    {
                        setInstance(settings);
                    };
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        private static void Instance_OnUpdate(object sender, EmailConfigurationSettings e)
        {
            throw new NotImplementedException();
        }
    }
}
