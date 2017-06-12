using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Voat.Configuration;
using Voat.Logging;
using Voat.Utilities.Components;

namespace Voat.Http.Middleware
{
    public class GlobalExceptionLoggerMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionLoggerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                var logInfo = context.GetLogInformation("GlobalExceptionLoggerMiddleware", ex);
                EventLogger.Instance.Log(logInfo);
                throw;
            }
        }
    }
}
