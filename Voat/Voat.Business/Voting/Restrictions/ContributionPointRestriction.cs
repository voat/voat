using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Voat.Configuration;
using Voat.Voting.Options;

namespace Voat.Voting.Restrictions
{
    public class ContributionPointRestriction : VoteRestriction<ContentOption>
    {
        public override bool Evaluate(IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override string ToDescription()
        {
            var where = $"on {VoatSettings.Instance.SiteName}";
            if (!String.IsNullOrEmpty(Options.Subverse))
            {
                where = $"in v/{Options.Subverse}";
            }
            return $"Have at least {Options.MinimumCount} points for {Options.ContentType} {where} from {Options.DateRange.ToString()}";

        }
    }
}
