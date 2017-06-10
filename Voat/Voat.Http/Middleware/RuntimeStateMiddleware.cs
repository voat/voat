using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Voat.Configuration;

namespace Voat.Http
{
    public class RuntimeStateMiddleware
    {
        private readonly RequestDelegate _next;

        public RuntimeStateMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var redirected = context.HandleRuntimeState();
            if (!redirected)
            {
                await _next.Invoke(context);
            }
        }
    }
}
