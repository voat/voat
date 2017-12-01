using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Voat.Common;
using Voat.Configuration;
using Voat.Logging;

namespace Voat.Http
{
    public static class HttpExtensions
    {

        public static ILogInformation GetLogInformation(this HttpContext context, string category, LogType logLevel, Exception exception = null)
        {
            
            var logInfo = new LogInformation()
            {
                ActivityID = null,
                Origin = VoatSettings.Instance.Origin.ToString(),
                Type = logLevel,
                Message = $"{context.Request.Method} {context.Request.GetUrl().PathAndQuery}",
                Category = category,
                UserName = context.User.Identity.Name,
                Data = context.ToDebugginInformation(),
                Exception = exception
            };

            return logInfo;
        }
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
            if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
            {
                return true; // This is a test host scenario
            }
            else if (connection.RemoteIpAddress.Equals(connection.LocalIpAddress))
            {
                return true;
            }
            else if (IPAddress.IsLoopback(connection.RemoteIpAddress))
            {
                return true;
            }
            // for in memory TestServer or when dealing with default connection info
            else if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
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
        //This is very hacky, will need to fix later.
        public static bool HandleRuntimeState(this HttpContext context)
        {
            var result = false;
            var request = context.Request;
            var isLocal = request.IsLocal();

            if (!isLocal)
            {
                var currentUri = request.GetUrl();
                var path = currentUri.AbsolutePath.ToLower();
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
                                    context.Response.Redirect("/static/inactive.min.htm");
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
                    if (VoatSettings.Instance.RedirectToSiteDomain && !VoatSettings.Instance.SiteDomain.IsEqual(currentUri.Host))
                    {
                        context.Response.Redirect(String.Format("http{2}://{0}{1}", VoatSettings.Instance.SiteDomain, currentUri.PathAndQuery, (VoatSettings.Instance.ForceHTTPS ? "s" : "")), true);
                        result = true;
                    }

                    // force SSL for every request if enabled in Web.config
                    if (VoatSettings.Instance.ForceHTTPS && !request.IsHttps)
                    {
                        var sslUrl = String.Format("https://{0}/{1}", currentUri.Authority, currentUri.PathAndQuery.TrimStart('/'));
                        context.Response.Redirect(sslUrl);
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

        public static string RemoteAddress(this HttpRequest request, string headerKeys = "CF-Connecting-IP, X-Forwarded-For, X-Original-For")
        {
            var keys = headerKeys.Split(',', ';').Select(x => x.TrimSafe());
            string clientIpAddress = String.Empty;

            foreach (var key in keys)
            {
                if (request.Headers.ContainsKey(key))
                {
                    clientIpAddress = request.Headers[key];
                    if (!String.IsNullOrEmpty(clientIpAddress))
                    {
                        return clientIpAddress.StripIPAddressPort();
                    }
                }
            }
            return clientIpAddress;
        }
        public static string StripIPAddressPort(this string ipAddress)
        {
            //TODO: We need to strip ports on v4 or else rules will not work in development
            if (!String.IsNullOrEmpty(ipAddress) && Regex.IsMatch(ipAddress, @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)"))
            {
                if (ipAddress.Contains(":"))
                {
                    return ipAddress.Substring(0, ipAddress.IndexOf(":"));
                }
            }
            return ipAddress;
        }
        public static string Signature(this HttpRequest request, bool full = true)
        {
            var originationInfo = request.RemoteAddress();
            if (full)
            {
                originationInfo += request.Headers["User-Agent"].ToString();
                var url = request.GetUrl();
                //Need to filter out querystrings from signature
                originationInfo += url.GetLeftPart(UriPartial.Path);
            }

            var hashValue = string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(originationInfo)).Select(s => s.ToString("x2")));
            return hashValue;
        }

        public static string SiteDomain(this HttpRequest request, string siteDomain, bool preferRequestDomain = false)
        {
            var url = request.GetUrl();
            var host = url.Host;
            var port = "";

            if (url.Port != 80 && url.Port != 443)
            {
                port = ":" + url.Port.ToString();
            }

            if (!(String.IsNullOrEmpty(siteDomain) || !String.IsNullOrEmpty(siteDomain) && preferRequestDomain))
            {
                host = siteDomain;
            }

            return $"{host}{port}";
        }

        public static void WriteJsonResponse(this HttpResponse response, object content, HttpStatusCode? statusCode = null, string contentType = "application/json", JsonSerializerSettings jsonSerializerSettings = null)
        {
            response.StatusCode = statusCode.HasValue ? (int)statusCode.Value : response.StatusCode;
            using (var writer = new StreamWriter(response.Body))
            {
                writer.Write(content.ToJson(jsonSerializerSettings));
            }
            response.ContentType = contentType;
        }
        public static void WriteJsonResponse(this HttpResponseMessage response, object content, HttpStatusCode? statusCode = null, string contentType = "application/json", JsonSerializerSettings jsonSerializerSettings = null)
        {
            response.StatusCode = statusCode.HasValue ? statusCode.Value : response.StatusCode;
            response.Content = new StringContent(content.ToJson(jsonSerializerSettings), Encoding.UTF8, contentType);
        }
    }
}
