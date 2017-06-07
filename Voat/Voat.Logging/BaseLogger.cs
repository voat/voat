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

namespace Voat.Logging
{

    public abstract class BaseLogger : ILogger
    {
        protected abstract void ProtectedLog(ILogInformation info);
        public LogType LogLevel { get; set; } = LogType.All;

        public virtual bool IsEnabledFor(LogType logType)
        {
            return (int)logType >= (int)LogLevel || LogLevel == LogType.All;
        }

        public void Log(ILogInformation info)
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
    }
}
