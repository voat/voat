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
            var logInfo = context.HttpContext.GetLogInformation("GlobalExceptionFilter", LogType.Exception, context.Exception);
            EventLogger.Instance.Log(logInfo);
        }
    }
}
