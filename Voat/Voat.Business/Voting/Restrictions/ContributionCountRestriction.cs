using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Voat.Configuration;
using Voat.Voting.Options;

namespace Voat.Voting.Restrictions
{
    public class ContributionCountRestriction : VoteRestriction<ContentOption>
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
                where = $"in v/{Options.Subverse}";
            }   
            return $"Have at submitted at least {Options.MinimumCount} {Options.ContentType} {where} from {Options.StartDate} to {Options.CutOffDate}";
        }
    }
}
