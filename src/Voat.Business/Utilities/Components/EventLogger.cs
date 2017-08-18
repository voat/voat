#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using System;
using Voat.Data;
using System.Threading;
using System.Threading.Tasks;
using Voat.Domain.Models;
using Voat.Logging;
using Voat.Common;
using System.Diagnostics;

namespace Voat.Utilities.Components
{
    /// <summary>
    /// Global event/exception logger for Voat
    /// </summary>
    //This might need replacement down the road with a commericial logger but should be satisfactory right now
    public static class EventLogger
    {

        private static ILogger _logger;

        public static ILogger Instance
        {
            get
            {
                if (_logger == null)
                {
                    var configInstance = LoggingConfigurationSettings.Instance;

                    configInstance.OnUpdate += (sender, args) => {
                        var currentLogger = _logger;
                        //Create new logger based on config
                        _logger = args.GetDefault();
                        Debug.WriteLine("Constructing new ILogger");

                        //Dispose current logger
                        var disposable = currentLogger as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    };
                    _logger = configInstance.GetDefault();
                }
                return _logger;
            }
            set
            {
                _logger = value;
            }
        }
        

        /// <summary>
        /// Log an exception to the database
        /// </summary>
        /// <param name="exception">The System.Exception to log</param>
        public static void Log(Exception exception, string origin = "Unknown")
        {
            Log(null, exception, origin);
        }

        private static void Log(int? parentID, Exception exception, string origin = "Unknown")
        {
            if (exception != null)
            {
                string userName = null;

                ////log user info if possible
                //if (User.IsAuthenticated)
                //{
                //    userName = UserIdentity.UserName;
                //}

                //Execute this without blocking
                Task t = Task.Factory.StartNew(async () =>
                {
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
                                var result = await repo.Log(new Data.Models.EventLog
                                {
                                    ParentID = parentID,
                                    UserName = userName,
                                    Origin = origin.ToString(),
                                    Type = "Exception",
                                    Category = "Exception",
                                    Message = exception.Message,
                                    Exception = exception.ToString(),
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
