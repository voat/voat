using System;
using Voat.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Voat.Utilities.Components
{
    /// <summary>
    /// Global event/exception logger for Voat
    /// </summary>
    //This might need replacement down the road with a commericial logger but should be satisfactory right now
    public static class EventLogger
    {
        /// <summary>
        /// Log an exception to the database
        /// </summary>
        /// <param name="exception">The System.Exception to log</param>
        public static void Log(Exception exception)
        {
            Log(null, exception);
        }

        private static void Log(int? parentID, Exception exception)
        {
            if (exception != null)
            {
                string userName = null;

                //log user info if possible
                if (Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.Identity != null && Thread.CurrentPrincipal.Identity.IsAuthenticated)
                {
                    userName = Thread.CurrentPrincipal.Identity.Name;
                }
                //Execute this without blocking
                Task t = Task.Factory.StartNew(() => {
                    try
                    {
                        using (var repo = new Repository())
                        {
                            while (exception != null)
                            {
                                string sDetails = null;
                                if (exception.Data.Count > 0)
                                {
                                    sDetails = Newtonsoft.Json.JsonConvert.SerializeObject(exception.Data);
                                }
                                var result = repo.Log(new Data.Models.EventLog
                                {
                                    ParentID = parentID,
                                    Type = exception.GetType().Name,
                                    UserName = userName,
                                    Message = exception.Message,
                                    Source = (!String.IsNullOrEmpty(exception.Source) ? exception.Source : "N/A"),
                                    CallStack = (!String.IsNullOrEmpty(exception.StackTrace) ? exception.StackTrace : "N/A"),
                                    IsBase = (exception.InnerException == null),
                                    CreationDate = Repository.CurrentDate,
                                    Data = sDetails
                                });
                                if (exception.InnerException != null)
                                {
                                    parentID = result.ID;
                                    exception = exception.InnerException;
                                }
                                else
                                {
                                    exception = null;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //the real question of a lifetime is:
                        //what do you do when you catch an exception in the exception logger?
#if DEBUG
                        //I've decided to live dangerously: (at least while in dev)
                        throw ex;
#endif
                    }
                });
            }
        }
    }
}
