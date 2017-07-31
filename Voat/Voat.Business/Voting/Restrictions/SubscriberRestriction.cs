using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Voat.Configuration;
using Voat.Voting.Options;

namespace Voat.Voting.Restrictions
{
    public class SubscriberRestriction : VoteRestriction<SubverseOption>
    {
        public override bool Evaluate(IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            var where = $"to {VoatSettings.Instance.SiteName}";
            if (!String.IsNullOrEmpty(Options.Subverse))
            {
                where = $"to v/{Options.Subverse}";
            }
            return $"Was subscribed {where} before {Options.CutOffDate}";
        }
    }
}
