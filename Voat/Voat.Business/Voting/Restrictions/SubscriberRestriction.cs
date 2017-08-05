using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Voat.Configuration;
using Voat.Domain.Command;
using Voat.Voting.Options;

namespace Voat.Voting.Restrictions
{
    public class SubscriberRestriction : VoteRestriction<SubverseOption>
    {
        public override CommandResponse<IVoteRestriction> Evaluate(IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override string ToDescription()
        {
            var where = $"to {VoatSettings.Instance.SiteName}";
            if (!String.IsNullOrEmpty(Options.Subverse))
            {
                where = $"to v/{Options.Subverse}";
            }
            return $"Was subscribed {where} before {Options.EndDate}";
        }
    }
}
