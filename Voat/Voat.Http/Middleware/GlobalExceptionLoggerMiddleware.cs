using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Voat.Configuration;
using Voat.Logging;
using Voat.Utilities.Components;

namespace Voat.Http.Middleware
{
    public class GlobalExceptionLoggerMiddleware : BaseMiddleware
    {

        public GlobalExceptionLoggerMiddleware(RequestDelegate next) : base(next) {}

        public override async Task Invoke(HttpContext context)
        {
            try
            {
                await base.Invoke(context);
            }
            catch (Exception ex)
            {
                var logInfo = context.GetLogInformation("GlobalExceptionLoggerMiddleware", LogType.Exception, ex);
                EventLogger.Instance.Log(logInfo);
                throw;
            }
        }
    }
}
