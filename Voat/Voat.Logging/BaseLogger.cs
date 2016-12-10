
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

        public void Log(ILogInformation info)
        {
            Debug.Print(info.ToString());
            ProtectedLog(info);
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
