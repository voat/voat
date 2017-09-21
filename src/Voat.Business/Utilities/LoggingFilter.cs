using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Voat.Configuration;

namespace Voat.Utilities
{
    /// <summary>
    /// The purpose of this class to to filter log messages becasue I can't figure out how to implement this 
    /// using the default framework. So frustrating. This class ONLY filters the .net core log messages and does not 
    /// filter direct voat logging statements.
    /// </summary>
    public class LoggingFilter
    {
        public Func<string, string, LogLevel, bool> Filter { get => IsAllowed; }

        protected bool IsAllowed(string provider, string category, LogLevel level)
        {
            var logLevels = VoatSettings.Instance.LogLevels;
            var key = logLevels.Keys.FirstOrDefault(x => category.StartsWith(x));
            var setLevel = Microsoft.Extensions.Logging.LogLevel.None;
            if (!String.IsNullOrEmpty(key))
            {
                setLevel = logLevels[key];
            }
            else
            {
                string defaultKey = "Default";
                setLevel = logLevels.ContainsKey(defaultKey) ? logLevels[defaultKey] : setLevel;
            }
            var allowed = setLevel <= level;
            return allowed;
        }

    }
}