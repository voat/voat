using System;
using Voat.Data;
using System.Threading;

namespace Voat.Utilities.Components
{
    //TODO: This code needs to not block on exception logging

    /// <summary>
    /// Global event/exception logger for Voat
    /// </summary>
    //This might need replacement down the road with a commericial logger but should be satisfactory right now
    public static class EventLogger
    {
        private static Repository _db = new Repository();

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
            string userName = null;

            //log user info if possible
            if (Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.Identity != null && Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                userName = Thread.CurrentPrincipal.Identity.Name;
            }

            string sDetails = null;
            if (exception.Data.Count > 0)
            {
                sDetails = Newtonsoft.Json.JsonConvert.SerializeObject(exception.Data);
            }

            try
            {
                var result = _db.Log(new Data.Models.EventLog
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
                    Log((result != null ? result.ID : (int?)null), exception.InnerException);
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
        }
    }
}
