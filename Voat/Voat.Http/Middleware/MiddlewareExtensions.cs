using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Http.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionLogger(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionLoggerMiddleware>();
        }
        public static IApplicationBuilder UseRequestDurationLogger(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestDurationLoggerMiddleware>();
        }
    }
}
