using System;


namespace Voat.Logging
{
    public enum LogType
    {
        All = 0,
        Information,
        Warning,
        Exception,
        Audit,
        Trace,
        Debug,
        Critical
    }
}
