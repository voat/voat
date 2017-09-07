using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
using System.Text;
using Voat.Common;
using Voat.Domain.Command;

namespace Voat.Voting.Restrictions
{
    public abstract class VoteRestriction : VoteItem, IVoteRestriction
    {
        //[Required]
        [DisplayName("Group")]
        public string Group { get; set; }
        //[Required]
        [DisplayName("End Date")]
        public DateTime? EndDate { get; set; }
        [DisplayName("Duration")]
        public TimeSpan Duration { get; set; }
        [JsonIgnore]
        public DateRange DateRange { get => new DateRange(Duration, DateRangeDirection.Past, EndDate); }

        public abstract CommandResponse<IVoteRestriction> Evaluate(IPrincipal principal);
       
    }
}
