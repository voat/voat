/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat, Inc.
All Rights Reserved.
*/

using System.Web;
using OpenGraph_Net;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Voat.Utilities
{
    public static class UrlUtility
    {

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
        public static bool IsUriValid(string completeUri)
        {
            Uri uriResult;
            return Uri.TryCreate(completeUri, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        // return remote page title from URI
        public static string GetTitleFromUri(string @remoteUri)
        {
            try
            {
                // try using Open Graph to get target page title
                var graph = OpenGraph.ParseUrl(@remoteUri, "Voat.co OpenGraph Parser");
                if (!string.IsNullOrEmpty(graph.Title))
                {
                    var tmpStringWriter = new StringWriter();
                    HttpUtility.HtmlDecode(graph.Title, tmpStringWriter);
                    return tmpStringWriter.ToString();
                }

                // Open Graph parsing failed, try getting HTML TITLE tag instead
                HtmlWeb htmlWeb = new HtmlWeb();
                HtmlDocument htmlDocument = htmlWeb.Load(@remoteUri);

                if (htmlDocument != null)
                {
                    var titleNode = htmlDocument.DocumentNode.Descendants("title").SingleOrDefault();
                    if (titleNode != null)
                    {
                        return HttpUtility.HtmlDecode(titleNode.InnerText);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // return youtube video id from url
        public static string GetVideoIdFromUrl(string completeUri)
        {
            Regex youtubeRegexPattern = new Regex(@"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)", RegexOptions.IgnoreCase);
            Regex vimeoRegexPattern = new Regex(@"vimeo\.com/(?:.*#|.*/)?([0-9]+)", RegexOptions.IgnoreCase);

            Match youtubeRegexMatch = youtubeRegexPattern.Match(completeUri);
            Match vimeoRegexMatch = vimeoRegexPattern.Match(completeUri);

            if (youtubeRegexMatch.Success)
            {
                return youtubeRegexMatch.Groups[1].Value;
            }

            if (vimeoRegexMatch.Success)
            {
                return vimeoRegexMatch.Groups[1].Value;
            }

            // match not found
            return "Error: regex video ID match failed.";
        }
    }


}