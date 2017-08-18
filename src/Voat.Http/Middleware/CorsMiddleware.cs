using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;

namespace Voat.Http.Middleware
{
    public abstract class CorsMiddleware : BaseMiddleware
    {
        public enum CorEvaluation
        {
            NotProcessed,
            Allowed,
            Denied
        }

        public CorsMiddleware(RequestDelegate next) : base(next) {}

        public override async Task Invoke(HttpContext context)
        {
            var optionsRequest = context.Request.Method.IsEqual("OPTIONS");

            //If options request we handle and do not go into voat
            if (optionsRequest)
            {
                var result = await ProcessCorsRequest(context);
                switch (result)
                {
                    case CorEvaluation.Denied:
                        context.Response.StatusCode = 400; //??? Have no idea
                        break;
                    case CorEvaluation.Allowed:
                        context.Response.StatusCode = 200; //??? Have no idea
                        break;
                }
            }
            else
            {
                //we process request and append cors outgoing response headers
                await base.Invoke(context);
                //process outgoing headers
                await ProcessCorsRequest(context);
            }
        }
        protected abstract Task<CorEvaluation> ProcessCorsRequest(HttpContext context);
    }
}
