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

namespace Voat.Common
{
    /// <summary>
    /// Turns TimeSpans into friendly ages. 1 day, 12 hours, etc.
    /// </summary>
    public static class Age
    {
        public static string ToRelative(DateTime date)
        {
            return ToRelative(DateTime.UtcNow.Subtract(date));
        }

        public static string ToRelative(TimeSpan span)
        {
            string result = "sometime";

            if (span.TotalDays >= 365)
            {
                //years
                //double years = Math.Round(span.TotalDays / 365f, 1); //Round 
                double years = Math.Round(Math.Floor(span.TotalDays / 365f * 10) / 10, 1); //Round down
                result = years.PluralizeIt("year");
            }
            else if (span.TotalDays > 31)
            {
                //months
                int days = (int)span.TotalDays;
                if (days.Equals(52))
                {
                    result = "1 dog year";
                }
                else
                {
                    int months = (int)(span.TotalDays / 30);
                    result = months.PluralizeIt("month");
                }
            }
            else if (span.TotalHours >= 24)
            {
                //days
                int days = (int)span.TotalDays;
                if (days.Equals(14))
                {
                    result = "1 fortnight";
                }
                else
                {
                    result = Math.Round(span.TotalDays, (span.TotalDays < 2 ? 1 : 0)).PluralizeIt("day");
                }
            }
            else if (span.TotalHours >= 1)
            {
                //hours
                if (span.TotalHours < 3)
                {
                    result = span.TotalHours.PluralizeIt("hour");
                }
                else
                {
                    result = ((int)span.TotalHours).PluralizeIt("hour");
                }
            }
            else if (span.TotalSeconds >= 60)
            {
                //minutes
                int min = (int)span.TotalMinutes;
                if (min.Equals(52))
                {
                    result = "1 microcentury";
                }
                else
                {
                    result = min.PluralizeIt("minute");
                }
            }
            else if (span.TotalSeconds > 0)
            {
                //seconds
                if (Math.Round(span.TotalSeconds, 2).Equals(1.21))
                {
                    result = "1 microfortnight";
                }
                else
                {
                    result = Math.Max(1, Math.Round(span.TotalSeconds, 0)).PluralizeIt("second");
                }
            }

            return result;
        }
    }
}
