using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Logging;
using Voat.Utilities;
using Voat.Utilities.Components;

namespace Voat.Configuration
{
    public static class VoatConfiguration
    {
        public static void ConfigureVoat(this IConfigurationRoot config)
        {
            ConfigureVoat(config, false);
        }
        private static void ConfigureVoat(IConfigurationRoot config, bool reloading)
        {

            VoatSettings.Load(config, "voat:settings", reloading);

            Caching.CacheConfigurationSettings.Load(config, "voat:cache", reloading);
            RulesEngine.RuleConfigurationSettings.Load(config, "voat:rules", reloading);
            Logging.LoggingConfigurationSettings.Load(config, "voat:logging", reloading);
            Data.DataConfigurationSettings.Load(config, "voat:data", reloading);
            IO.FileManagerConfigurationSettings.Load(config, "voat:fileManager", reloading);
            IO.Email.EmailConfigurationSettings.Load(config, "voat:emailSender", reloading);

            if (!reloading)
            {


                //From old global.asax - this more than likely does not belong in this library but wanted to persist code
                /*
                
                if (EventLogger.Instance.LogLevel >= Logging.LogType.Debug)
                {
                    //BLOCK: Temp Log ThreadPool Stats
                    var timer = new Timer(new TimerCallback(o => {

                        ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
                        ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);

                        var data = new
                        {
                            InUse = maxWorkerThreads - workerThreads,
                            InUseIO = maxCompletionPortThreads - completionPortThreads,
                            AvailableWorkerThreads = workerThreads,
                            AvailableIOThreads = completionPortThreads,
                            MaxWorkerThreads = maxWorkerThreads,
                            MaxIOThreads = maxCompletionPortThreads
                        };


                        var logEntry = new LogInformation
                        {
                            Origin = VoatSettings.Instance.Origin.ToString(),
                            Type = LogType.Debug,
                            UserName = null,
                            Message = "ThreadPool Stats",
                            Category = "Monitor",
                            Data = data
                        };

                        EventLogger.Instance.Log(logEntry);

                    }), null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
                }     

                #region Hook Events

                EventHandler<MessageReceivedEventArgs> updateNotificationCount = delegate (object s, MessageReceivedEventArgs e)
                {
                    if (VoatSettings.Instance.SignalrEnabled)
                    {
                        var userDef = UserDefinition.Parse(e.TargetUserName);
                        if (userDef.Type == IdentityType.User)
                        {
                            //get count of unread notifications
                            var q = new QueryMessageCounts(userDef.Name, userDef.Type, MessageTypeFlag.All, MessageState.Unread);
                            var unreadNotifications = q.Execute().Total;
                            // send SignalR realtime notification to recipient
                            var hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();
                            hubContext.Clients.User(e.TargetUserName).setNotificationsPending(unreadNotifications);
                        }

                    }
                };
                EventNotification.Instance.OnMessageReceived += updateNotificationCount;
                EventNotification.Instance.OnMentionReceived += updateNotificationCount;
                EventNotification.Instance.OnCommentReplyReceived += updateNotificationCount;

                EventNotification.Instance.OnVoteReceived += (s, e) =>
                {
                    if (VoatSettings.Instance.SignalrEnabled)
                    {
                        var hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();
                        switch (e.ReferenceType)
                        {
                            case Domain.Models.ContentType.Submission:
                                hubContext.Clients.User(e.TargetUserName).voteChange(1, e.ChangeValue);
                                break;
                            case Domain.Models.ContentType.Comment:
                                hubContext.Clients.User(e.TargetUserName).voteChange(2, e.ChangeValue);
                                break;
                        }
                    }
                };

                //TODO: Fuzzy can't wait for this feature!
                EventNotification.Instance.OnHeadButtReceived += (s, e) => { };
                EventNotification.Instance.OnChatMessageReceived += (s, e) => { };

                #endregion
                */
            }

            //Register Change Callback - I LOVE .NET CORE BTW
            //Update: This seems to fire twice which is a bit weird, I sure hope future people will figure this out.
            //var reloadToken = config.GetReloadToken();
            //reloadToken.RegisterChangeCallback(x =>
            //{
            //    ConfigureVoat((IConfigurationRoot)x, true);
            //}, config);
        }
    }
}
