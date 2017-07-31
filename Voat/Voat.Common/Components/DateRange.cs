using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Common
{
    public enum DateRangeDirection
    {
        Past,
        Future
    }
    public class DateRange
    {
        public DateRange(DateTime start, DateTime end)
        {
            StartDate = start;
            EndDate = end;
        }
        public DateRange(TimeSpan timeSpan, DateRangeDirection dateRangeDirection = DateRangeDirection.Past, DateTime? baseLineDate = null)
        {
            var refDate = baseLineDate == null ? DateTime.UtcNow : baseLineDate.Value;

            if (dateRangeDirection == DateRangeDirection.Past)
            {
                StartDate = refDate.Subtract(timeSpan);
                EndDate = refDate;
            }
            else
            {
                EndDate = refDate.Add(timeSpan);
                StartDate = refDate;
            }
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}
