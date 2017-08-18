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
        public DateRange()
        {

        }
        public DateRange(DateTime? start, DateTime? end)
        {
            StartDate = start;
            EndDate = end;
        }
        public DateRange(TimeSpan timeSpan, DateRangeDirection dateRangeDirection = DateRangeDirection.Past, DateTime? baseLineDate = null)
        {
            var refDate = baseLineDate == null ? DateTime.UtcNow : baseLineDate.Value;

            if (dateRangeDirection == DateRangeDirection.Past)
            {
                StartDate = refDate.Subtract(timeSpan.Duration());
                EndDate = refDate;
            }
            else
            {
                EndDate = refDate.Add(timeSpan.Duration());
                StartDate = refDate;
            }
        }
        public static DateRange StartFrom(TimeSpan timeSpan, DateRangeDirection dateRangeDirection = DateRangeDirection.Past)
        {
            var d = new DateRange();

            if (dateRangeDirection == DateRangeDirection.Past)
            {
                d.StartDate = DateTime.UtcNow.Subtract(timeSpan.Duration());
            }
            else
            {
                d.StartDate = DateTime.UtcNow.Add(timeSpan.Duration());
            }
            return d;
        }
        //This needs to be an extension method
        public override string ToString()
        {
            if (StartDate.HasValue && EndDate.HasValue)
            {
                return $"{StartDate.Value.ToShortDateString()} to {EndDate.Value.ToShortDateString()}";
            }
            else if (StartDate.HasValue)
            {
                return $"{StartDate.Value.ToShortDateString()} to now";
            }
            else if (EndDate.HasValue)
            {
                return $"until {EndDate.Value.ToShortDateString()}";
            }
            else 
            {
                return $"all time";
            }

        }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
