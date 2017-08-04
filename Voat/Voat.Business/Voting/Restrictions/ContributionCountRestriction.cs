using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Voat.Configuration;
using Voat.Data;
using Voat.Voting.Attributes;
using Voat.Voting.Options;

namespace Voat.Voting.Restrictions
{
    [Restriction(
        Enabled = true, 
        Description = "Restriction by the count of posts (comments and/or submissions)", 
        Name = "Contribution Count")]
    public class ContributionCountRestriction : VoteRestriction<ContentOption>
    {
        public override bool Evaluate(IPrincipal principal)
        {
            using (var repo = new Repository())
            {
                var count = repo.UserContributionCount(principal.Identity.Name, Options.ContentType, Options.Subverse, Options.DateRange);
                return count >= Options.MinimumCount;
            }
        }

        public override string ToDescription()
        {
            var where = $"to {VoatSettings.Instance.SiteName}";
            if (!String.IsNullOrEmpty(Options.Subverse))
            {
                where = $"in v/{Options.Subverse}";
            }   
            return $"Has submitted at least {Options.MinimumCount} {Options.ContentType} {where} from {Options.DateRange.ToString()}";
        }
    }
}
