/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2014 Voat
All Rights Reserved.
*/

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Voat.Utils
{
    public static class Submissions
    {

        // calculate submission age in days, hours or minutes for use in views
        public static string CalcSubmissionAge(DateTime inPostingDateTime)
        {
            var currentDateTime = DateTime.Now;
            var duration = currentDateTime - inPostingDateTime;
            return CalcSubmissionAge(duration);
        }
        
        private static string IsPlural(int amount)
        {
            return (amount == 1 ? "" : "s");
        }
        
        public static string CalcSubmissionAge(TimeSpan span)
        {

            string result = "No idea when this was posted...";

            if (span.TotalDays > 365)
            {
                //years
                double years = Math.Round(span.TotalDays / 365, 1);
                result = String.Format("{0} year{1}", years, (years > 1.0 ? "s" : ""));
            }
            else if (span.TotalDays > 31)
            {
                //months
                int months = (int)(span.TotalDays / 31);
                result = String.Format("{0} month{1}", months, IsPlural(months));
            }
            else if (span.TotalHours >= 24)
            {
                //days 
                result = String.Format("{0} day{1}", (int)span.TotalDays, IsPlural((int)span.TotalDays));
            }
            else if (span.TotalMinutes >= 60)
            {
                //hours
                result = String.Format("{0} hour{1}", (int)span.TotalHours, IsPlural((int)span.TotalHours));
            }
            else if (span.TotalSeconds >= 60)
            {
                //minutes
                result = String.Format("{0} minute{1}", (int)span.TotalMinutes, IsPlural((int)span.TotalMinutes));
            }
            else
            {
                //seconds
                result = String.Format("{0} second{1}", (int)span.TotalSeconds, IsPlural((int)span.TotalSeconds));
            }

            return result;
        }

        // calculate submission age in hours from posting date for ranking purposes
        public static double CalcSubmissionAgeDouble(DateTime inPostingDateTime)
        {
            var currentDateTime = DateTime.Now;
            var duration = currentDateTime - inPostingDateTime;

            return duration.TotalHours;
        }

        // check if a string contains unicode characters
        public static bool ContainsUnicode(string stringToTest)
        {
            const int maxAnsiCode = 255;
            return stringToTest.Any(c => c > maxAnsiCode);
        }

        // string unicode characters from a string
        public static string StripUnicode(string stringToClean)
        {
            return Regex.Replace(stringToClean, @"[^\u0000-\u007F]", string.Empty);
        }

    }
}
