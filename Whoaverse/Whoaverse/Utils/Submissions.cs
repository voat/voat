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

namespace Voat.Utils
{
    public static class Submissions
    {
        //private const double Tolerance = 0.01;

        // calculate submission age in days, hours or minutes for use in views
        public static string CalcSubmissionAge(DateTime inPostingDateTime)
        {
            var currentDateTime = DateTime.Now;
            var duration = currentDateTime - inPostingDateTime;
            return CalcSubmissionAge(duration);
        }
        private static string PluralizeIt(int amount, string unit) {
            return String.Format("{0} {1}{2}", amount, unit, (amount == 1 ? "" : "s"));
        }
        private static string PluralizeIt(double amount, string unit) {
            return String.Format("{0} {1}{2}", amount, unit, (Math.Round(amount, 1) == 1.0 ? "" : "s"));
        }

        public static string CalcSubmissionAge(TimeSpan span) {
            
            string result = "sometime";

            if (span.TotalDays >= 365) { 
                //years
                double years = Math.Round(span.TotalDays / 365, 1);
                result = PluralizeIt(years, "year"); 
            } else if (span.TotalDays > 31) { 
                //months
                int days = (int)span.TotalDays;
                if (days.Equals(14)) {
                    result = "1 fortnight";
                } else if (days.Equals(52)) {
                    result = "1 dog year";
                } else {
                    int months = (int)(span.TotalDays / 31);
                    result = PluralizeIt(months, "month");
                }
            } else if (span.TotalHours >= 24) { 
                //days 
                result = PluralizeIt((int)span.TotalDays, "day"); 
            } else if (span.TotalHours > 1) {
                //hours
                if (span.TotalHours < 2) {
                    result = PluralizeIt(span.TotalHours, "hour");
                } else {
                    result = PluralizeIt((int)span.TotalHours, "hour");
                }
            } else if (span.TotalSeconds >= 60) {
                //minutes
                int min = (int)span.TotalMinutes;
                if (min.Equals(52)) {
                    result = "1 microcentury";
                } else {
                    result = PluralizeIt(min, "minute");
                } 
            } else {
                //seconds
                if (Math.Round(span.TotalSeconds, 2).Equals(1.21)) {
                    result = "1 microfortnight";
                } else {
                    result = PluralizeIt((int)span.TotalSeconds, "second");
                }
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


        
    }
}