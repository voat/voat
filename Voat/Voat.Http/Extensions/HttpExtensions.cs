using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Voat.Configuration;

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

        public static object ToDebugginInformation(this HttpContext context)
        {
            return new {
                url = context.Request.GetUrl().ToString(),
                method = context.Request.Method,
                headers = context.Request.Headers.Select(x => new { key = x.Key, value = x.Value.Any() ? x.Value.Aggregate((z1, z2) => z1 + "|" + z2) : "" }).ToList()
            };
        }

        public static bool IsLocal(this HttpRequest req)
        {
            var connection = req.HttpContext.Connection;
            if (connection.RemoteIpAddress != null)
            {
                if (connection.LocalIpAddress != null)
                {
                    return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
                }
                else
                {
                    return IPAddress.IsLoopback(connection.RemoteIpAddress);
                }
            }

            // for in memory TestServer or when dealing with default connection info
            if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
            {
                return true;
            }

            return false;
        }

        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (request.Headers != null)
            {
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";
            }
            return false;
        }

        public static void Add(this IResponseCookies cookies, HttpCookie cookie)
        {
            cookies.Append(cookie.Name, cookie.Value, new CookieOptions() { Expires = cookie.Expires });
        }
        public static void SetCookie(this HttpResponse response, HttpCookie cookie)
        {
            response.Cookies.Add(cookie);
        }

        public static bool HandleRuntimeState(this HttpContext context)
        {
            var result = false;
            var request = context.Request;
            var isLocal = request.IsLocal();

            if (!isLocal)
            {
                var url = request.GetUrl();
                var path = url.AbsolutePath.ToLower();
                var isSignalR = path.StartsWith("/signalr/");
                if (!isSignalR)
                {
                    //Need to be able to kill connections for certain db tasks... This intercepts calls and redirects
                    if (VoatSettings.Instance.RuntimeState == RuntimeStateSetting.Disabled)
                    {
                        try
                        {
                            var isAjax = request.IsAjaxRequest();
                            var response = context.Response;
                            if (isAjax)
                            {
                                //js calls
                                response.StatusCode = (int)HttpStatusCode.BadRequest;
                                result = true;
                                //response.R = "Website is disabled :( Try again in a moment.";
                                //response.End();
                                //return;
                            }
                            else
                            {
                                //Don't send stylesheet request to an html page
                                if (Regex.IsMatch(path, $"v/[a-z0-9]+/stylesheet", RegexOptions.IgnoreCase))
                                {
                                    response.ContentType = "text/css";
                                    result = true;
                                }
                                else
                                {
                                    //page requests
                                    context.Response.Redirect("~/inactive.min.htm");
                                    result = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //if (!(ex is ThreadAbortException))
                            //{
                            //    //bail with static
                            //    Server.Transfer("~/inactive.min.htm");
                            //    return;
                            //}
                        }
                    }

                    // force single site domain
                    if (VoatSettings.Instance.RedirectToSiteDomain && !VoatSettings.Instance.SiteDomain.Equals(url.Host, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.Redirect(String.Format("http{2}://{0}{1}", VoatSettings.Instance.SiteDomain, url.PathAndQuery, (VoatSettings.Instance.ForceHTTPS ? "s" : "")), true);
                        result = true;
                    }

                    // force SSL for every request if enabled in Web.config
                    if (VoatSettings.Instance.ForceHTTPS && !request.IsHttps)
                    {
                        context.Response.Redirect(String.Format("https://{0}{1}", url.Host, url));
                        result = true;
                    }
                }

                //change formatting culture for .NET
                try
                {
                    var langHeader = request.Headers[""];
                    var lang = (request != null && langHeader.Any()) ? langHeader.First() : null;

                    if (!String.IsNullOrEmpty(lang))
                    {
                        System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
                    }
                }
                catch { }
            }

            return result;

        }


    }
}
