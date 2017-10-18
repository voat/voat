using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Voat.Configuration;

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
            if (String.IsNullOrEmpty(Voat.Configuration.VoatSettings.Instance.SiteDomain))
            {
                var siteDomain = context.Request.SiteDomain(Voat.Configuration.VoatSettings.Instance.SiteDomain);
                Voat.Configuration.VoatSettings.Instance.SiteDomain = siteDomain;
            }

            await Next.Invoke(context);
        }
        public static async Task WriteJsonResponse(HttpContext context, HttpStatusCode statusCode, object responseObject)
        {
            var responseJson = JsonConvert.SerializeObject(responseObject, JsonSettings.APISerializationSettings);
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(responseJson);
        }
    }
}
