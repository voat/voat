using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Http.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseVoatGlobalExceptionLogger(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionLoggerMiddleware>();
        }
        public static IApplicationBuilder UseVoatRequestDurationLogger(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestDurationLoggerMiddleware>();
        }
        public static IApplicationBuilder UseVoatRuntimeState(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RuntimeStateMiddleware>();
        }
    }
}
