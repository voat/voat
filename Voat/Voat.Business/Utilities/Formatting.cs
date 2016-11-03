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

using System;
using MarkdownDeep;
using Voat.Utilities.Components;
using System.Text.RegularExpressions;
using System.Linq;

namespace Voat.Utilities
{
    public static class Formatting
    {
        public static string FormatMessage(String originalMessage, bool processContent = true, bool? forceLinksNewWindow = null)
        {
            //Test changes to this code against this markdown thread content:
            //https://voat.co/v/test/comments/53891

            if (processContent && !String.IsNullOrEmpty(originalMessage))
            {
                originalMessage = ContentProcessor.Instance.Process(originalMessage, ProcessingStage.Outbound, null);
            }

            var newWindow = false;

            if (forceLinksNewWindow.HasValue)
            {
                newWindow = forceLinksNewWindow.Value;
            }

            var m = new Markdown
            {
                PrepareLink = new Func<HtmlTag, bool>(x =>
                {
                    //Remove [Title](javascript:alter('hello')) exploit
                    string href = x.attributes["href"];
                    if (!String.IsNullOrEmpty(href))
                    {
                        if (UrlUtility.InjectableJavascriptDetected(href))
                        {
                            x.attributes["href"] = "#";

                            //add it to the output for verification?
                            x.attributes.Add("data-ScriptStrip", String.Format("/* script detected: {0} */", href));
                        }
                    }
                    return true;
                }),
                ExtraMode = true,
                SafeMode = true,
                NewWindowForExternalLinks = newWindow,
                NewWindowForLocalLinks = newWindow
            };

            try
            {
                return m.Transform(originalMessage).Trim();
            }
            catch (Exception ex)
            {
                return "Content contains unsafe or unknown tags.";
            }
        }

        // credits to http://stackoverflow.com/questions/1613896/truncate-string-on-whole-words-in-net-c-sharp
        public static string TruncateAtWord(this string input, int length)
        {
            if (input == null || input.Length < length)
                return input;
            var iNextSpace = input.LastIndexOf(" ", length, StringComparison.Ordinal);
            return string.Format("{0}...", input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
        }

        // check if a string contains unicode characters
        public static bool ContainsUnicode(string stringToTest, bool includeUnprintableChars = true)
        {
            const int maxAnsiCode = 255;

            //Adding constraint for unprintable characters
            int minAnsiCode = (includeUnprintableChars ? 32 : 0);
            return stringToTest.Any(c => (c > maxAnsiCode || c < minAnsiCode));
        }

        public static string StripWhiteSpace(string stringToClean)
        {
            var scrubbed = stringToClean;
            if (!String.IsNullOrEmpty(scrubbed))
            {
                scrubbed = Regex.Replace(scrubbed, @"\s{2,}", " ").Trim();
            }
            return scrubbed;
        }

        // string unicode characters from a string
        public static string StripUnicode(string stringToClean, bool includeUnprintableChars = true, bool includeWhitespace = true)
        {
            var scrubbed = stringToClean;
            if (!String.IsNullOrEmpty(scrubbed))
            {
                scrubbed = Regex.Replace(scrubbed, String.Format(@"[^\u00{0}-\u00FF]", (includeUnprintableChars ? "20" : "00")), string.Empty).Trim();

                //remove sequential whitespace
                if (includeWhitespace)
                {
                    scrubbed = StripWhiteSpace(scrubbed);
                }
            }
            return scrubbed;
        }
    }
}
