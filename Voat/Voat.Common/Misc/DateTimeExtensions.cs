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
using System.Globalization;

namespace Voat.Common
{
    public static class DateTimeExtensions
    {
        private static JulianCalendar julianCalendar = new JulianCalendar();

        public static DateTime ToStartOfDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0);
        }

        public static DateTime ToEndOfDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999);
        }

        public static DateTime ToStartOfHour(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, 0);
        }

        public static DateTime ToEndOfHour(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 59, 59, 999);
        }

        public static DateTime ToStartOfYear(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, 1, 1, 0, 0, 0, 0);
        }

        public static DateTime ToEndOfYear(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, 12, 31, 23, 59, 59, 999);
        }

        public static DateTime ToStartOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, 0);
        }

        public static DateTime ToEndOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, julianCalendar.GetDaysInMonth(dateTime.Year, dateTime.Month)).ToEndOfDay();
        }

        public static DateTime ToStartOfWeek(this DateTime dateTime)
        {
            var dayOfWeek = julianCalendar.GetDayOfWeek(dateTime);
            var start = dateTime.Subtract(TimeSpan.FromDays((int)dayOfWeek));
            return start.ToStartOfDay();
        }

        public static DateTime ToEndOfWeek(this DateTime dateTime)
        {
            var dayOfWeek = julianCalendar.GetDayOfWeek(dateTime);
            var start = dateTime.Add(TimeSpan.FromDays(6 - (int)dayOfWeek));
            return start.ToStartOfDay();
        }

        public static Tuple<DateTime, DateTime> ToWeekRange(this DateTime dateTime)
        {
            return new Tuple<DateTime, DateTime>(dateTime.ToStartOfWeek(), dateTime.ToEndOfWeek());
        }

        public static Tuple<DateTime, DateTime> ToMonthRange(this DateTime dateTime)
        {
            return new Tuple<DateTime, DateTime>(dateTime.ToStartOfMonth(), dateTime.ToEndOfMonth());
        }

        public static Tuple<DateTime, DateTime> ToQuarterRange(this DateTime dateTime)
        {
            var j = new JulianCalendar();
            int currentDay = dateTime.DayOfYear;
            int indexDay = 0;

            for (int i = 1; i <= 12; i++)
            {
                indexDay += j.GetDaysInMonth(dateTime.Year, i);
                if (indexDay >= currentDay)
                {
                    if (i >= 0 && i <= 3)
                    {
                        var endMonthNum = 3;
                        return new Tuple<DateTime, DateTime>(dateTime.ToStartOfYear(), new DateTime(dateTime.Year, endMonthNum, j.GetDaysInMonth(dateTime.Year, endMonthNum)).ToEndOfDay());
                    }
                    else if (i >= 4 && i <= 6)
                    {
                        var endMonthNum = 6;
                        return new Tuple<DateTime, DateTime>(new DateTime(dateTime.Year, 4, 1).ToStartOfDay(), new DateTime(dateTime.Year, endMonthNum, j.GetDaysInMonth(dateTime.Year, endMonthNum)).ToEndOfDay());
                    }
                    else if (i >= 7 && i <= 9)
                    {
                        var endMonthNum = 9;
                        return new Tuple<DateTime, DateTime>(new DateTime(dateTime.Year, 7, 1).ToStartOfDay(), new DateTime(dateTime.Year, endMonthNum, j.GetDaysInMonth(dateTime.Year, endMonthNum)).ToEndOfDay());
                    }
                    else
                    {
                        var endMonthNum = 12;
                        return new Tuple<DateTime, DateTime>(new DateTime(dateTime.Year, 10, 1).ToStartOfDay(), new DateTime(dateTime.Year, endMonthNum, j.GetDaysInMonth(dateTime.Year, endMonthNum)).ToEndOfDay());
                    }
                }
            }
            return null;
        }
    }
}
