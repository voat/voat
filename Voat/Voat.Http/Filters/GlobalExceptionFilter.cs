using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Configuration;
using Voat.Http;
using Voat.Logging;
using Voat.Utilities.Components;

namespace Voat.Http.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter, IDisposable
    {

        public GlobalExceptionFilter(ILoggerFactory logger)
        {
        }

        public void Dispose()
        {
        }

        public void OnException(ExceptionContext context)
        {
            var logger = EventLogger.Instance;
            var logLevel = LogType.Exception;
            if (logger.IsEnabledFor(logLevel))
            {
                var logInfo = context.HttpContext.GetLogInformation("GlobalExceptionFilter", logLevel, context.Exception);
                logger.Log(logInfo);
            }
        }
    }
}
