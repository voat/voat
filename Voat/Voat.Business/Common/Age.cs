﻿using System;
using Voat.Data;

namespace Voat.Common
{


    /// <summary>
    /// Turns TimeSpans into friendly ages. 1 day, 12 hours, etc.
    /// </summary>
    public static class Age
    {

        private static string PluralizeIt(int amount, string unit)
        {
            return String.Format("{0} {1}{2}", amount, unit, (amount == 1 ? "" : "s"));
        }

        private static string PluralizeIt(double amount, string unit)
        {
            return String.Format("{0} {1}{2}", (Math.Round(amount, 1)), unit, (Math.Round(amount, 1) == 1.0 ? "" : "s"));
        }

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
                double years = Math.Round(span.TotalDays / 365f, 1);
                result = PluralizeIt(years, "year");

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
                    result = PluralizeIt(months, "month");
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
                    result = PluralizeIt(Math.Round(span.TotalDays, (span.TotalDays < 2 ? 1 : 0)), "day");
                }
            }
            else if (span.TotalHours >= 1)
            {
                //hours
                if (span.TotalHours < 3)
                {
                    result = PluralizeIt(span.TotalHours, "hour");
                }
                else
                {
                    result = PluralizeIt((int)span.TotalHours, "hour");
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
                    result = PluralizeIt(min, "minute");
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
                    result = PluralizeIt(Math.Max(1, Math.Round(span.TotalSeconds, 0)), "second");
                }
            }

            return result;
        }

    }
}
