﻿/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
*/

using System;
using MarkdownDeep;

namespace Whoaverse.Utils
{
    public static class Formatting
    {
        
        public static string FormatMessage (String originalMessage){
            var m = new Markdown
            {
                ExtraMode = true, 
                SafeMode = true
            };

            m.NewWindowForExternalLinks = Whoaverse.Utils.User.LinksInNewWindow(System.Web.HttpContext.Current.User.Identity.Name);

            return m.Transform(originalMessage);
        }

        // credits to http://stackoverflow.com/questions/1613896/truncate-string-on-whole-words-in-net-c-sharp
        public static string TruncateAtWord(this string input, int length)
        {
            if (input == null || input.Length < length)
                return input;
            var iNextSpace = input.LastIndexOf(" ", length, StringComparison.Ordinal);
            return string.Format("{0}...", input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
        }
        
    }
}