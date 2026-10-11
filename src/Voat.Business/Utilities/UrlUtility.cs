#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using System.Web;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voat.Common;

namespace Voat.Utilities
{
    public static class UrlUtility
    {
        //Schemes allowed in rendered markdown links (href) and images (src).
        //Everything else (javascript:, data:, vbscript:, ...) is stripped.
        //mailto: is inert (opens a mail client) and was rendered by the legacy site, so it is kept.
        private static readonly string[] AllowedUrlSchemes = { "http", "https", "ftp", "mailto" };

        public static bool InjectableJavascriptDetected(string url)
        {
            if (!String.IsNullOrEmpty(url))
            {
                string htmlUrl = HttpUtility.HtmlDecode(url);
                return Regex.IsMatch(htmlUrl, @"javascript\s{0,}:", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            }
            else
            {
                return false;
            }
        }

        //SSRF guard for server-side fetches of user-supplied urls (thumbnails, og titles).
        //The host must resolve to public addresses only — loopback, RFC1918, link-local
        //(169.254/16 incl. cloud metadata 169.254.169.254), CGNAT, multicast and unspecified
        //addresses are all rejected. Every resolved address must pass, not just the first.
        public static async Task<bool> IsUriSafeForServerFetch(Uri uri)
        {
            if (uri == null || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return false;
            }
            IPAddress[] addresses;
            try
            {
                addresses = await Dns.GetHostAddressesAsync(uri.Host);
            }
            catch
            {
                return false;
            }
            if (addresses == null || addresses.Length == 0)
            {
                return false;
            }
            return addresses.All(x => IsPublicAddress(x));
        }

        private static bool IsPublicAddress(IPAddress address)
        {
            if (address == null)
            {
                return false;
            }
            if (IPAddress.IsLoopback(address))
            {
                return false;
            }
            if (address.IsIPv4MappedToIPv6)
            {
                address = address.MapToIPv4();
            }
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                var b = address.GetAddressBytes();
                if (b[0] == 10) return false; //10.0.0.0/8
                if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return false; //172.16.0.0/12
                if (b[0] == 192 && b[1] == 168) return false; //192.168.0.0/16
                if (b[0] == 169 && b[1] == 254) return false; //169.254.0.0/16 link-local + cloud metadata
                if (b[0] == 100 && b[1] >= 64 && b[1] <= 127) return false; //100.64.0.0/10 CGNAT
                if (b[0] == 0) return false; //0.0.0.0/8
                if (b[0] >= 224) return false; //224+ multicast/reserved/broadcast
            }
            else if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var b = address.GetAddressBytes();
                if ((b[0] & 0xFE) == 0xFC) return false; //fc00::/7 unique local
                if (b[0] == 0xFE && (b[1] & 0xC0) != 0x00) return false; //fe80::/10 + fec0::/10
                if (b[0] == 0xFF) return false; //ff00::/8 multicast
                if (address.Equals(IPAddress.IPv6Any)) return false;
            }
            return true;
        }

        //Allow-list counterpart of InjectableJavascriptDetected: true when the url is safe to
        //place in href/src (relative urls and the allowed schemes above).
        public static bool IsUrlSchemeAllowed(string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                return false;
            }
            string decoded = HttpUtility.HtmlDecode(url).Trim();

            //Protocol-relative (//example.com) and site-relative/fragment/query urls carry no scheme.
            if (decoded.StartsWith("//") || decoded.StartsWith("/") || decoded.StartsWith("#") || decoded.StartsWith("?"))
            {
                return true;
            }

            int schemeEnd = decoded.IndexOf(':');
            if (schemeEnd < 0)
            {
                return true; //no scheme at all -> relative url
            }
            int firstSlash = decoded.IndexOf('/');
            if (firstSlash >= 0 && firstSlash < schemeEnd)
            {
                return true; //colon lives inside a path segment -> relative url
            }

            string scheme = decoded.Substring(0, schemeEnd).Trim().ToLowerInvariant();
            return AllowedUrlSchemes.Contains(scheme);
        }

        // return domain from URI
        public static string GetDomainFromUri(string completeUri)
        {
            Uri uriResult;

            if (Uri.TryCreate(completeUri, UriKind.Absolute, out uriResult))
            {
                return uriResult.GetLeftPart(UriPartial.Authority).Replace("/www.", "/").Replace("http://", "").Replace("https://", "");
            }

            return null; 
        }

        // check if a URI is valid HTTP or HTTPS URI
        public static bool IsUriValid(string completeUri, bool evaluateRegex = true, bool dnsNamesOnly = false)
        {
            Uri uriResult;
            bool result = false;

            if (Uri.TryCreate(completeUri, UriKind.Absolute, out uriResult))
            {
                if (
                        (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps) 
                        && !uriResult.IsLoopback
                        && (!dnsNamesOnly || dnsNamesOnly && uriResult.HostNameType == UriHostNameType.Dns)
                        )
                {
                    if (evaluateRegex)
                    {
                        //we had blocking on this when we used ^ and $ so now we just match for a link and ensure the match equals the url
                        var match = Regex.Match(completeUri, CONSTANTS.HTTP_LINK_REGEX, RegexOptions.IgnoreCase);
                        result = match.Success && match.Value == completeUri; 
                    }
                    else
                    {
                        result = true;
                    }
                }
            }
            return result;
        }
    }
}
