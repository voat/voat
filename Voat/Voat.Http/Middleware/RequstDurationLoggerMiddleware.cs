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
            using (var duration = new DurationLogger(EventLogger.Instance,
                new LogInformation() {
                    Category = "RequestDuration",
                    UserName = context.User.Identity.Name,
                    Origin = Settings.Origin.ToString(),
                    Type = LogType.Information,
                    Message = $"{context.Request.Method} {context.Request.GetUrl().PathAndQuery}",
                    Data = context.ToDebugginInformation()
                }, 
                _timeSpan))
            {
                await _next.Invoke(context);
            }
        }
    }
}
