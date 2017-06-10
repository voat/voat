using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            EventLogger.Instance.Log(
                new LogInformation() {
                    ActivityID = null,
                    Type = LogType.Critical,
                    Category = "Exception",
                    Message = "GlobalExceptionFilter",
                    UserName = context.HttpContext.User.Identity.Name,
                    Data = context.HttpContext.ToDebugginInformation(),
                    Exception = context.Exception }
                );
        }
    }
}
