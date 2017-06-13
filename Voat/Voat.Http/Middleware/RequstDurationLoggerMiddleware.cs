using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Voat.Logging;
using Voat.Configuration;
using Voat.Utilities.Components;

namespace Voat.Http.Middleware
{
    public class RequestDurationLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TimeSpan _timeSpan = TimeSpan.Zero;

        public RequestDurationLoggerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var logInfo = context.GetLogInformation("RequestDuration", LogType.Information);
            using (var duration = new DurationLogger(EventLogger.Instance, logInfo, _timeSpan))
            {
                await _next.Invoke(context);
            }
        }
    }
}
