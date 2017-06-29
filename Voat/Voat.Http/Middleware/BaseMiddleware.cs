using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Http.Middleware
{
    public abstract class BaseMiddleware
    {
        private readonly RequestDelegate _next;

        public BaseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public RequestDelegate Next { get => _next; }


        public virtual async Task Invoke(HttpContext context)
        {
            await Next.Invoke(context);
        }

    }
}
