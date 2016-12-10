using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Voat.Configuration;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Logging;
using Voat.Rules;
using Voat.UI.Utilities;
using Voat.Utilities;
using Voat.Utilities.Components;
using Voat.Utils;

namespace Voat
{
    public class MvcApplication : HttpApplication
    {
        Timer timer;
        protected void Application_Start()
        {
            var formatters = GlobalConfiguration.Configuration.Formatters;
            formatters.Remove(formatters.XmlFormatter);

            LiveConfigurationManager.Reload(ConfigurationManager.AppSettings);
            LiveConfigurationManager.Start();

            //forces Rules Engine to load
            var x = VoatRulesEngine.Instance;
            //forces thumbgenerator to initialize
            var p = ThumbGenerator.DestinationPathThumbs;

            if (!Settings.SignalRDisabled)
            {
                Microsoft.AspNet.SignalR.GlobalHost.DependencyResolver.Register(typeof(Microsoft.AspNet.SignalR.Hubs.IJavaScriptMinifier), () => new HubMinifier());
            }
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //register global error handler
            GlobalConfiguration.Configuration.Services.Add(typeof(IExceptionLogger), new VoatExceptionLogger());

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            ModelMetadataProviders.Current = new CachedDataAnnotationsModelMetadataProvider();

            JsonConvert.DefaultSettings = () => { return JsonSettings.GetSerializationSettings(); };

            #region Hook Events

            EventHandler<MessageReceivedEventArgs> updateNotificationCount = delegate (object s, MessageReceivedEventArgs e)
            {
                if (!Settings.SignalRDisabled)
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
                if (!Settings.SignalRDisabled)
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


            //BLOCK: Temp Log ThreadPool Stats
            timer = new Timer(new TimerCallback(o => {
                int workerThreads;
                int completionPortThreads;
                int maxWorkerThreads;
                int maxCompletionPortThreads;

                ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
                ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);

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
                    Origin = Settings.Origin.ToString(),
                    Type = LogType.Debug,
                    UserName = null,
                    Message = "ThreadPool Stats",
                    Category = "Monitor",
                    Data = data
                };

                EventLogger.Instance.Log(logEntry);

            }), null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
            

            // USE ONLY FOR DEBUG: clear all sessions used for online users count
            // SessionTracker.RemoveAllSessions();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            EventLogger.Log(ex, Origin.UI);
            if (ex is HttpException && ((HttpException)ex).GetHttpCode() == 404)
            {
                Response.RedirectToRoute(
                    new {
                        controller = "Error",
                        action = "Generic"
                    });
            }
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            var request = HttpContext.Current.Request;
            var isLocal = request.IsLocal;

            if (!isLocal)
            {
                var path = request.Path.ToLower();
                var isSignalR = path.StartsWith("/signalr/");
                if (!isSignalR)
                {
                    //Need to be able to kill connections for certain db tasks... This intercepts calls and redirects
                    if (RuntimeState.Current == RuntimeStateSetting.Disabled)
                    {
                        try
                        {
                            var isAjax = request.RequestContext.HttpContext.Request.IsAjaxRequest();
                            var response = HttpContext.Current.Response;
                            if (isAjax)
                            {
                                //js calls
                                response.StatusCode = (int)HttpStatusCode.BadRequest;
                                response.StatusDescription = "Website is disabled :( Try again in a moment.";
                                response.End();
                                return;
                            }
                            else
                            {
                                //Don't send stylesheet request to an html page
                                if (Regex.IsMatch(path, $"v/{CONSTANTS.SUBVERSE_REGEX}/stylesheet", RegexOptions.IgnoreCase))
                                {
                                    response.ContentType = "text/css";
                                    response.End();
                                    return;
                                }
                                else
                                {
                                    //page requests
                                    Server.Transfer("~/inactive.min.htm");
                                    return;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!(ex is ThreadAbortException))
                            {
                                //bail with static
                                Server.Transfer("~/inactive.min.htm");
                                return;
                            }
                        }
                    }

                    // force single site domain
                    if (Settings.RedirectToSiteDomain && !Settings.SiteDomain.Equals(request.ServerVariables["HTTP_HOST"], StringComparison.OrdinalIgnoreCase))
                    {
                        Response.RedirectPermanent(String.Format("http{2}://{0}{1}", Settings.SiteDomain, request.RawUrl, (Settings.ForceHTTPS ? "s" : "")), true);
                        return;
                    }

                    // force SSL for every request if enabled in Web.config
                    if (Settings.ForceHTTPS && !request.IsSecureConnection)
                    {
                        Response.Redirect(String.Format("https://{0}{1}", request.ServerVariables["HTTP_HOST"], request.RawUrl), true);
                        return;
                    }
                }

                //change formatting culture for .NET
                try
                {
                    var lang = (request != null && request.UserLanguages != null && request.UserLanguages.Length > 0) ? request.UserLanguages[0] : null;

                    if (!String.IsNullOrEmpty(lang))
                    {
                        System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
                    }
                }
                catch { }
            }
        }
    }
}
