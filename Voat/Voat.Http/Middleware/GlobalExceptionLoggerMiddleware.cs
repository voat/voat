using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
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
                EventLogger.Instance.Log(
                   new LogInformation()
                   {
                       ActivityID = null,
                       Type = LogType.Critical,
                       Message = "GlobalExceptionLoggerMiddleware",
                       Category = "Exception",
                       UserName = context.User.Identity.Name,
                       Data = context.ToDebugginInformation(),
                       Exception = ex
                   });
            }
        }
    }
}
