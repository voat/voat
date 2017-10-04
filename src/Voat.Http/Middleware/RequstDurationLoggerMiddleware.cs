using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Voat.Logging;
using Voat.Configuration;
using Voat.Utilities.Components;

namespace Voat.Http.Middleware
{
    public class RequestDurationLoggerMiddleware : BaseMiddleware
    {
        private readonly TimeSpan _timeSpan = TimeSpan.Zero;

        public RequestDurationLoggerMiddleware(RequestDelegate next) : base(next) {}

        public override async Task Invoke(HttpContext context)
        {
            
            var logger = EventLogger.Instance;
            var logLevel = LogType.Information;
            var logInfo = context.GetLogInformation("RequestDuration", logLevel);
            using (var duration = new DurationLogger(logger, logInfo, _timeSpan))
            {
                await base.Invoke(context);
            }

        }
    }
}
