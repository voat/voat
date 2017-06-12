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

namespace Voat.UI.Runtime
{
    public class GlobalExceptionFilter : IExceptionFilter, IDisposable
    {
        //private readonly ILogger _logger;

        public GlobalExceptionFilter(ILoggerFactory logger)
        {
        //    if (logger == null)
        //    {
        //        throw new ArgumentNullException(nameof(logger));
        //    }

        //    this._logger = logger.CreateLogger("Global Exception Filter");
        }

        public void Dispose()
        {
            //
        }

        public void OnException(ExceptionContext context)
        {
            var logInfo = context.HttpContext.GetLogInformation("GlobalExceptionFilter", context.Exception);
            EventLogger.Instance.Log(logInfo);
        }
    }
}
