using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Domain.Models
{
    public class RateLimits
    {
        public int PerSecond { get; set; }
        public int PerMinute { get; set; }
        public int PerHour { get; set; }
        public int PerDay { get; set; }
        public int PerWeek { get; set; }

    }
}
