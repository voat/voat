using System;
using Voat.Data;

namespace Voat.Common
{
    /// <summary>
    /// Turns TimeSpans into friendly ages. 1 day, 12 hours, etc.
    /// </summary>
    public static class Age
    {
        public static string ToRelative(DateTime date)
        {
            return ToRelative(Repository.CurrentDate.Subtract(date));
        }

        public static string ToRelative(TimeSpan span)
        {
            string result = "sometime";

            if (span.TotalDays >= 365)
            {
                //years
                //double years = Math.Round(span.TotalDays / 365f, 1); //Round 
                double years = Math.Round(Math.Floor(span.TotalDays / 365f * 10) / 10, 1); //Round down
                result = Utilities.Formatting.PluralizeIt(years, "year");
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
                    result = Utilities.Formatting.PluralizeIt(months, "month");
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
                    result = Utilities.Formatting.PluralizeIt(Math.Round(span.TotalDays, (span.TotalDays < 2 ? 1 : 0)), "day");
                }
            }
            else if (span.TotalHours >= 1)
            {
                //hours
                if (span.TotalHours < 3)
                {
                    result = Utilities.Formatting.PluralizeIt(span.TotalHours, "hour");
                }
                else
                {
                    result = Utilities.Formatting.PluralizeIt((int)span.TotalHours, "hour");
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
                    result = Utilities.Formatting.PluralizeIt(min, "minute");
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
                    result = Utilities.Formatting.PluralizeIt(Math.Max(1, Math.Round(span.TotalSeconds, 0)), "second");
                }
            }

            return result;
        }
    }
}
