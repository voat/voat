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

namespace Voat.Logging
{

    public interface ILogger : Microsoft.Extensions.Logging.ILogger
    {

        LogType LogLevel { get; set; }

        bool IsEnabledFor(LogType logType);
        void Log(ILogInformation info);
        void Log(LogType type, string category, string formatMessage, params object[] formatParameters);
        void Log(LogType type, string category, string message);
        void Log(Exception exception, Guid? activityID = null);
        void Log<T>(LogType type, string category, string message, T data = null) where T : class;
        //void Log<T>(Exception exception, string category = "Exception", string message = null, T value = null) where T : class;
        void Log<T>(Exception exception, T data, Guid? activityID = null) where T : class;
        
    }
}
