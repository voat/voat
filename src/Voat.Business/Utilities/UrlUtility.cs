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
using System.Text.RegularExpressions;
using Voat.Common;

namespace Voat.Utilities
{
    public static class UrlUtility
    {
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
