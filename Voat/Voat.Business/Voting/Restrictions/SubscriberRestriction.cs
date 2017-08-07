using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
using System.Text;
using Voat.Configuration;
using Voat.Domain.Command;

namespace Voat.Voting.Restrictions
{
    public class SubscriberRestriction : VoteRestriction
    {
        public override CommandResponse<IVoteRestriction> Evaluate(IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override string ToDescription()
        {
            var where = $"to {VoatSettings.Instance.SiteName}";
            if (!String.IsNullOrEmpty(Subverse))
            {
                where = $"to v/{Subverse}";
            }
            return $"Was subscribed {where} before {EndDate}";
        }
        [Required]
        public string Subverse { get; set; }
    }
}
