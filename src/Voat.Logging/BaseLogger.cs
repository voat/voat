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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Configuration;

namespace Voat.Logging
{

    public abstract class BaseLogger : ILogger
    {
        public BaseLogger() { }
        public BaseLogger(LogType logLevel)
        {
            this.LogLevel = logLevel;
        }

        protected abstract void ProtectedLog(ILogInformation info);
        public LogType LogLevel { get; set; } = LogType.All;

        public virtual bool IsEnabledFor(LogType logType)
        {
            var enabled = (LogLevel != LogType.Off && logType != LogType.Off) && ((int)logType >= (int)LogLevel || (logType == LogType.All || LogLevel == LogType.All));
            return enabled;
        }
        
        public virtual void Log(ILogInformation info)
        {
            Debug.WriteLine(info.ToString());
            if (IsEnabledFor(info.Type))
            {
                ProtectedLog(info);
            }
        }

        public void Log(LogType type, string category, string formatMessage, params object[] formatParameters)
        {
            Log(new LogInformation() { Type = type, Category = category, Message = String.Format(formatMessage, formatParameters) });
        }

        public void Log(LogType type, string category, string message)
        {
            Log(new LogInformation() { Type = type, Category = category, Message = message });
        }

        public void Log(Exception exception, Guid? activityID = null)
        {
            Log(new LogInformation() { ActivityID = activityID, Type = LogType.Critical, Category = "Exception", Message = exception.Message, Exception = exception});
        }

        public void Log<T>(LogType type, string category, string message, T data = null) where T : class
        {
            Log(new LogInformation() { Type = type, Category = category, Message = message, Data = data });
        }

        //public void Log<T>(Exception exception, string category = "Exception", string message = null, T data = null) where T : class
        //{
        //    Log(new LogInformation() { Type = LogType.Critical, Category = category, Message = message, Data = data, Exception = exception });
        //}

        public void Log<T>(Exception exception, T data, Guid? activityID = null) where T : class
        {
            Log(new LogInformation() { ActivityID = activityID, Type = LogType.Critical, Category = "Exception", Data = data, Exception = exception });
        }

        #region Microsoft.Extensions.Logging.ILogger 

        bool Microsoft.Extensions.Logging.ILogger.IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            LogType logType = MapLogLevel(logLevel);
            return IsEnabledFor(logType);
        }

        void Microsoft.Extensions.Logging.ILogger.Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            
            Log(new LogInformation() {
                Data = new { State = state, Event = eventId } ,
                Exception = exception,
                Type = MapLogLevel(logLevel),
                Origin = VoatSettings.Instance.Origin.ToString(),
                Message = formatter(state, exception)
            });
        }

        IDisposable Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState state)
        {
            return new DummyScope();
        }

        private class DummyScope : IDisposable
        {
            public void Dispose()
            {
                
            }
        }

        private LogType MapLogLevel(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            var logType = LogType.All;
            switch (logLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Critical:
                    logType = LogType.Critical;
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    logType = LogType.Debug;
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    logType = LogType.Exception;
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    logType = LogType.Information;
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    logType = LogType.Trace;
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    logType = LogType.Warning;
                    break;
                case Microsoft.Extensions.Logging.LogLevel.None:
                default:
                    logType = LogType.Off;
                    break;
            }

            return logType;
        }
        #endregion
    }
}
