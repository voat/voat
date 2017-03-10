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
using System.Globalization;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Utilities
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
        /// <summary>
        /// Returns a range based on the span provided. Does not standardize range, simply subtracts offset.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="sortSpan"></param>
        /// <returns></returns>
        public static Tuple<DateTime, DateTime> ToRelativeRange(this DateTime dateTime, SortSpan sortSpan, SortDirection sortDirection = SortDirection.Reverse)
        {
            DateTime start = dateTime;
            DateTime end = dateTime;
            var directionMultiplier = sortDirection == SortDirection.Reverse ? -1 : 1;
            switch (sortSpan)
            {
                case SortSpan.Hour:
                    end = start.ToEndOfHour();
                    start = end.AddHours(1 * directionMultiplier);
                    break;

                case SortSpan.Day:
                    end = start.ToEndOfHour();
                    start = end.AddHours(24 * directionMultiplier);
                    break;

                case SortSpan.Week:
                    end = start.ToEndOfDay();
                    start = end.AddDays(7 * directionMultiplier);
                    break;

                case SortSpan.Month:
                    end = start.ToEndOfDay();
                    start = end.AddDays(30 * directionMultiplier);
                    break;

                case SortSpan.Quarter:
                    end = start.ToEndOfDay();
                    start = end.AddDays(90 * directionMultiplier);
                    break;

                case SortSpan.Year:
                    end = start.ToEndOfDay();
                    start = end.AddDays(365 * directionMultiplier);
                    break;

                default:
                case SortSpan.All:

                    //Date Range shouldn't be processed for this span
                    break;
            }

            return new Tuple<DateTime, DateTime>(start, end);
        }
        /// <summary>
        /// The purpose of this function is to standardize inputs so that we can cache ranged queries. Currently ranges
        /// use the current date which contains diffrent minute, second, and ms with each call. This function converts to common
        /// start and end ranges (beginning and ending of days, hours, etc.)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="sortSpan"></param>
        /// <returns></returns>
        public static Tuple<DateTime, DateTime> ToRange(this DateTime dateTime, SortSpan sortSpan)
        {
            DateTime start = dateTime;
            DateTime end = dateTime;
            switch (sortSpan)
            {
                case SortSpan.Hour:
                    start = start.ToStartOfHour();
                    end = start.Add(TimeSpan.FromHours(-1));
                    break;

                case SortSpan.Day:
                    start = start.ToStartOfDay();
                    end = start.ToEndOfDay();
                    break;

                case SortSpan.Week:
                    start = start.ToStartOfWeek();
                    end = start.ToEndOfWeek();
                    break;

                case SortSpan.Month:
                    start = start.ToStartOfMonth();
                    end = start.ToEndOfMonth();
                    break;

                case SortSpan.Quarter:
                    var range = start.ToQuarterRange();
                    start = range.Item1;
                    end = range.Item2;
                    break;

                case SortSpan.Year:
                    start = start.ToStartOfYear();
                    end = start.ToEndOfYear();
                    break;

                default:
                case SortSpan.All:

                    //Date Range shouldn't be processed for this span
                    break;
            }

            return new Tuple<DateTime, DateTime>(start, end);
        }
    }
}
