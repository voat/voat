using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Http
{
    public static class HttpExtensions
    {
        public static Uri GetUrl(this HttpRequest request)
        {
            var builder = new UriBuilder();
            builder.Scheme = request.Scheme;
            builder.Host = request.Host.Host;
            if (request.Host.Port.HasValue)
            {
                builder.Port = request.Host.Port.Value;
            }
            builder.Path = request.Path;
            builder.Query = request.QueryString.ToUriComponent();
            return builder.Uri;
        }

        public static object ToErrorInformation(this HttpContext context)
        {
            return new {
                url = context.Request.GetUrl().ToString()               
            };
        }
    }
}
