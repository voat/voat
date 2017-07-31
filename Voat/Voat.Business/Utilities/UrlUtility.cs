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
            try
            {
                var tmpUri = new Uri(completeUri);
                return tmpUri.GetLeftPart(UriPartial.Authority).Replace("/www.", "/").Replace("http://", "").Replace("https://", "");
            }
            catch (Exception)
            {
                return null;
            }
        }

        // check if a URI is valid HTTP or HTTPS URI
        public static bool IsUriValid(string completeUri, bool evaluateRegex = true)
        {
            Uri uriResult;
            bool result = false;

            if (Uri.TryCreate(completeUri, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                if (evaluateRegex)
                {
                    result = Regex.IsMatch(completeUri, String.Concat("^", CONSTANTS.HTTP_LINK_REGEX, "$"), RegexOptions.IgnoreCase);
                }
                else
                {
                    result = true;
                }
            }
            return result;
        }
    }
}
