using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Web;
using Voat.Data;
using Voat.Models;

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

            //log user info if possible
            if (System.Threading.Thread.CurrentPrincipal != null && System.Threading.Thread.CurrentPrincipal.Identity != null && System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated && !exception.Data.Contains("USER"))
            {
                exception.Data.Add("USER", System.Threading.Thread.CurrentPrincipal.Identity.Name);
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

        //private static string IDictionaryToString(IDictionary o)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    string seperator = "";
        //    foreach (var property in o.Keys)
        //    {
        //        sb.Append(seperator);
        //        sb.AppendFormat("\"{0}\":\"{1}\"", property, o[property]);
        //        seperator = ", ";
        //    }
        //    return "{" + sb.ToString() + "}";

        //}

    }
}