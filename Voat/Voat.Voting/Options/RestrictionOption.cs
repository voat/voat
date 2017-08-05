using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Voat.Common;

namespace Voat.Voting.Options
{
    public abstract class RestrictionOption : Option
    {
        public DateTime EndDate { get; set; }
        public TimeSpan Duration { get; set; }
        [JsonIgnore]
        public DateRange DateRange { get => new DateRange(Duration, DateRangeDirection.Past, EndDate); }
    }
}
