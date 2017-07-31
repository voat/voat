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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using Voat.Utilities;
using Voat.Tests.Infrastructure;
using Voat.Common;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class DateTimeExtensionTests : BaseUnitTest
    {
        private Calendar calendar = CultureInfo.InvariantCulture.Calendar;

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.DateTime")]
        public void DateTimeExt_TestEndOfDay()
        {
            var current = DateTime.UtcNow;
            var processed = current.ToEndOfDay();

            Assert.AreEqual(current.Year, processed.Year);
            Assert.AreEqual(current.Month, processed.Month);
            Assert.AreEqual(current.Day, processed.Day);
            Assert.AreEqual(23, processed.Hour);
            Assert.AreEqual(59, processed.Minute);
            Assert.AreEqual(59, processed.Second);
            Assert.AreEqual(999, processed.Millisecond);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.DateTime")]
        public void DateTimeExt_TestEndOfHour()
        {
            var current = DateTime.UtcNow;
            var processed = current.ToEndOfHour();

            Assert.AreEqual(current.Year, processed.Year);
            Assert.AreEqual(current.Month, processed.Month);
            Assert.AreEqual(current.Day, processed.Day);
            Assert.AreEqual(current.Hour, processed.Hour);
            Assert.AreEqual(59, processed.Minute);
            Assert.AreEqual(59, processed.Second);
            Assert.AreEqual(999, processed.Millisecond);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.DateTime")]
        public void DateTimeExt_TestEndOfMonth()
        {
            var current = DateTime.UtcNow;
            var processed = current.ToEndOfMonth();

            Assert.AreEqual(current.Year, processed.Year);
            Assert.AreEqual(current.Month, processed.Month);
            Assert.AreEqual(calendar.GetDaysInMonth(current.Year, current.Month), processed.Day);
            Assert.AreEqual(23, processed.Hour);
            Assert.AreEqual(59, processed.Minute);
            Assert.AreEqual(59, processed.Second);
            Assert.AreEqual(999, processed.Millisecond);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.DateTime")]
        public void DateTimeExt_TestEndOfWeek()
        {
            var current = DateTime.UtcNow;
            var processed = current.ToEndOfWeek();

            Assert.AreEqual(DayOfWeek.Saturday, calendar.GetDayOfWeek(processed));
            Assert.IsTrue(calendar.GetDayOfYear(current) <= calendar.GetDayOfYear(processed));
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.DateTime")]
        public void DateTimeExt_TestMonthRangeEquality()
        {
            //Quarter 1
            var current = new DateTime(2015, 12, 1, 13, 3, 21);
            var range = current.ToMonthRange();
            var start = range.Item1;
            var end = range.Item2;

            var current2 = new DateTime(2015, 12, 31, 2, 50, 21);
            var range2 = current2.ToMonthRange();
            var start2 = range.Item1;
            var end2 = range.Item2;

            Assert.AreEqual(start, start2);
            Assert.AreEqual(end, end2);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.DateTime")]
        public void DateTimeExt_TestQuarterRange()
        {
            //Quarter 1
            string desc = "Quarter 1";
            var current = new DateTime(2015, 2, 14, 13, 3, 21);
            var range = current.ToQuarterRange();
            var start = range.Item1;
            var end = range.Item2;

            //year
            Assert.AreEqual(start.Year, end.Year, desc);
            //month
            Assert.AreEqual(1, start.Month, desc);
            Assert.AreEqual(3, end.Month, desc);
            //day
            Assert.AreEqual(1, start.Day, desc);
            Assert.AreEqual(calendar.GetDaysInMonth(current.Year, end.Month), end.Day, desc);

            Assert.AreEqual(0, start.Minute, desc);
            Assert.AreEqual(0, start.Second, desc);
            Assert.AreEqual(0, start.Millisecond, desc);
            Assert.AreEqual(59, end.Minute, desc);
            Assert.AreEqual(59, end.Second, desc);
            Assert.AreEqual(999, end.Millisecond, desc);

            desc = "Quarter 2";
            current = new DateTime(2015, 4, 1, 13, 3, 21);
            range = current.ToQuarterRange();
            start = range.Item1;
            end = range.Item2;

            //year
            Assert.AreEqual(start.Year, end.Year, desc);
            //month
            Assert.AreEqual(4, start.Month, desc);
            Assert.AreEqual(6, end.Month, desc);
            //day
            Assert.AreEqual(1, start.Day, desc);
            Assert.AreEqual(calendar.GetDaysInMonth(current.Year, end.Month), end.Day, desc);

            Assert.AreEqual(0, start.Minute, desc);
            Assert.AreEqual(0, start.Second, desc);
            Assert.AreEqual(0, start.Millisecond, desc);
            Assert.AreEqual(59, end.Minute, desc);
            Assert.AreEqual(59, end.Second, desc);
            Assert.AreEqual(999, end.Millisecond, desc);

            desc = "Quarter 3";
            current = new DateTime(2015, 7, 30, 13, 3, 21);
            range = current.ToQuarterRange();
            start = range.Item1;
            end = range.Item2;

            //year
            Assert.AreEqual(start.Year, end.Year, desc);
            //month
            Assert.AreEqual(7, start.Month, desc);
            Assert.AreEqual(9, end.Month, desc);
            //day
            Assert.AreEqual(1, start.Day, desc);
            Assert.AreEqual(calendar.GetDaysInMonth(current.Year, end.Month), end.Day, desc);

            Assert.AreEqual(0, start.Minute, desc);
            Assert.AreEqual(0, start.Second, desc);
            Assert.AreEqual(0, start.Millisecond, desc);
            Assert.AreEqual(59, end.Minute, desc);
            Assert.AreEqual(59, end.Second, desc);
            Assert.AreEqual(999, end.Millisecond, desc);

            desc = "Quarter 4";
            current = new DateTime(2015, 12, 31, 13, 3, 21);
            range = current.ToQuarterRange();
            start = range.Item1;
            end = range.Item2;

            //year
            Assert.AreEqual(start.Year, end.Year, desc);
            //month
            Assert.AreEqual(10, start.Month, desc);
            Assert.AreEqual(12, end.Month, desc);
            //day
            Assert.AreEqual(1, start.Day, desc);
            Assert.AreEqual(calendar.GetDaysInMonth(current.Year, end.Month), end.Day, desc);

            Assert.AreEqual(0, start.Minute, desc);
            Assert.AreEqual(0, start.Second, desc);
            Assert.AreEqual(0, start.Millisecond, desc);
            Assert.AreEqual(59, end.Minute, desc);
            Assert.AreEqual(59, end.Second, desc);
            Assert.AreEqual(999, end.Millisecond, desc);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.DateTime")]
        public void DateTimeExt_TestQuarterRangeEquality()
        {
            //Quarter 1
            var current = new DateTime(2015, 2, 14, 13, 3, 21);
            var range = current.ToQuarterRange();
            var start = range.Item1;
            var end = range.Item2;

            var current2 = new DateTime(2015, 3, 1, 2, 50, 21);
            var range2 = current2.ToQuarterRange();
            var start2 = range.Item1;
            var end2 = range.Item2;

            Assert.AreEqual(start, start2);
            Assert.AreEqual(end, end2);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.DateTime")]
        public void DateTimeExt_TestQuarterRangeEquality2()
        {
            //Quarter 1
            var current = new DateTime(2015, 12, 14, 13, 3, 21);
            var range = current.ToQuarterRange();
            var start = range.Item1;
            var end = range.Item2;

            var current2 = new DateTime(2015, 10, 1, 2, 50, 21);
            var range2 = current2.ToQuarterRange();
            var start2 = range.Item1;
            var end2 = range.Item2;

            Assert.AreEqual(start, start2);
            Assert.AreEqual(end, end2);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.DateTime")]
        public void DateTimeExt_TestStartOfDay()
        {
            var current = DateTime.UtcNow;
            var processed = current.ToStartOfDay();

            Assert.AreEqual(current.Year, processed.Year);
            Assert.AreEqual(current.Month, processed.Month);
            Assert.AreEqual(current.Day, processed.Day);
            Assert.AreEqual(0, processed.Hour);
            Assert.AreEqual(0, processed.Minute);
            Assert.AreEqual(0, processed.Second);
            Assert.AreEqual(0, processed.Millisecond);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.DateTime")]
        public void DateTimeExt_TestStartOfHour()
        {
            var current = DateTime.UtcNow;
            var processed = current.ToStartOfHour();

            Assert.AreEqual(current.Year, processed.Year);
            Assert.AreEqual(current.Month, processed.Month);
            Assert.AreEqual(current.Day, processed.Day);
            Assert.AreEqual(current.Hour, processed.Hour);
            Assert.AreEqual(0, processed.Minute);
            Assert.AreEqual(0, processed.Second);
            Assert.AreEqual(0, processed.Millisecond);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.DateTime")]
        public void DateTimeExt_TestStartOfMonth()
        {
            var current = DateTime.UtcNow;
            var processed = current.ToStartOfMonth();

            Assert.AreEqual(current.Year, processed.Year);
            Assert.AreEqual(current.Month, processed.Month);
            Assert.AreEqual(1, processed.Day);
            Assert.AreEqual(0, processed.Hour);
            Assert.AreEqual(0, processed.Minute);
            Assert.AreEqual(0, processed.Second);
            Assert.AreEqual(0, processed.Millisecond);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.DateTime")]
        public void DateTimeExt_TestStartOfWeek()
        {
            var current = DateTime.UtcNow;
            var processed = current.ToStartOfWeek();

            Assert.AreEqual(DayOfWeek.Sunday, calendar.GetDayOfWeek(processed));
            Assert.IsTrue(calendar.GetDayOfYear(current) >= calendar.GetDayOfYear(processed));
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.DateTime")]
        public void DateTimeExt_TestWeekRange()
        {
            var current = new DateTime(2015, 12, 14, 13, 3, 21);
            var processed = current.ToWeekRange();
            var start = processed.Item1;
            var end = processed.Item2;

            Assert.AreEqual(DayOfWeek.Sunday, calendar.GetDayOfWeek(start));
            Assert.AreEqual(DayOfWeek.Saturday, calendar.GetDayOfWeek(end));

            Assert.AreEqual(13, start.Day);
            Assert.AreEqual(19, end.Day);

            Assert.AreEqual(end.Month, start.Month);
            Assert.AreEqual(end.Year, start.Year);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.DateTime")]
        public void DateTimeExt_TestWeekRangeEquality()
        {
            //Quarter 1
            var current = new DateTime(2015, 12, 14, 13, 3, 21);
            var range = current.ToWeekRange();
            var start = range.Item1;
            var end = range.Item2;

            var current2 = new DateTime(2015, 12, 17, 2, 50, 21);
            var range2 = current2.ToWeekRange();
            var start2 = range.Item1;
            var end2 = range.Item2;

            Assert.AreEqual(start, start2);
            Assert.AreEqual(end, end2);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.DateTime")]
        public void DateTimeExt_RelavtiveRanges()
        {
            var current = DateTime.UtcNow;
            var range = current.ToRelativeRange(Domain.Models.SortSpan.Hour);
            Assert.AreEqual(TimeSpan.FromHours(1), range.Item2 - range.Item1);

            range = current.ToRelativeRange(Domain.Models.SortSpan.Day);
            Assert.AreEqual(TimeSpan.FromHours(24), range.Item2 - range.Item1);

            range = current.ToRelativeRange(Domain.Models.SortSpan.Week);
            Assert.AreEqual(TimeSpan.FromDays(7), range.Item2 - range.Item1);

            range = current.ToRelativeRange(Domain.Models.SortSpan.Month);
            Assert.AreEqual(TimeSpan.FromDays(30), range.Item2 - range.Item1);

            range = current.ToRelativeRange(Domain.Models.SortSpan.Quarter);
            Assert.AreEqual(TimeSpan.FromDays(90), range.Item2 - range.Item1);

            range = current.ToRelativeRange(Domain.Models.SortSpan.Year);
            Assert.AreEqual(TimeSpan.FromDays(365), range.Item2 - range.Item1);

        }
    }
}
