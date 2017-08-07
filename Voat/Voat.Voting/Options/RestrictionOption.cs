using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Voat.Common;

namespace Voat.Voting.Options
{
    public abstract class RestrictionOption : Option
    {
        [DisplayName("Group")]
        public string Group { get; set; }
        [DisplayName("End Date")]
        public DateTime EndDate { get; set; }
        [DisplayName("Duration")]
        public TimeSpan Duration { get; set; }
        [JsonIgnore]
        public DateRange DateRange { get => new DateRange(Duration, DateRangeDirection.Past, EndDate); }
    }
}
