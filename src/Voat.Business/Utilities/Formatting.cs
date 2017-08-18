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

using System;
using MarkdownDeep;
using Voat.Utilities.Components;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using Voat.Common;

namespace Voat.Utilities
{
    public static class Formatting
    {
        private static Dictionary<string, string> replacements = new Dictionary<string, string>() {
            //{ @"\n", "" },
            //{ @"\r", "" },
            { @"([\>](\s+)?){4,}", ">" }, // https://voat.co/v/voatdev/2050918
            { @"(\-\s+){2,}(\-(\s+)?)", "-" }, //three or more - followed by spaces
            { @"(\*\s+){2,}(\*(\s+)?)", "*" } //three or more * followed by spaces
        };

        public static string SanitizeInput(string message)
        {
            message = message.TrimSafe();
            if (!String.IsNullOrEmpty(message))
            {
                //message = message.SubstringMax(500);
                message = replacements.Aggregate(message, (value, keyPair) => Regex.Replace(value, keyPair.Key, keyPair.Value));
                message = message.TrimSafe();
            }

            return message;
        }

        public static string FormatMessage(String originalMessage, bool processContent = true, bool? forceLinksNewWindow = null)
        {
            //Test changes to this code against this markdown thread content:
            //https://voat.co/v/test/comments/53891

            if (processContent && !String.IsNullOrEmpty(originalMessage))
            {
                originalMessage = ContentProcessor.Instance.Process(originalMessage, ProcessingStage.Outbound, null);
            }

            //Sanitize Markdown
            var sanitizedMessage = SanitizeInput(originalMessage);

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
                return m.Transform(sanitizedMessage).Trim();
            }
            catch (Exception ex)
            {
                return "Content contains unsafe or unknown tags.";
            }
        }

        
       
    }
}
