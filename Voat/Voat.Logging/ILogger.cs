using System;
using System.Collections.Generic;

namespace Voat.Logging
{

    public interface ILogger
    {
        void Log(ILogInformation info);
        void Log(LogType type, string category, string formatMessage, params object[] formatParameters);
        void Log(LogType type, string category, string message);

        void Log(Exception exception, Guid? activityID = null);


        void Log<T>(LogType type, string category, string message, T data = null) where T : class;
        //void Log<T>(Exception exception, string category = "Exception", string message = null, T value = null) where T : class;
        void Log<T>(Exception exception, T data, Guid? activityID = null) where T : class;
        
    }
}
