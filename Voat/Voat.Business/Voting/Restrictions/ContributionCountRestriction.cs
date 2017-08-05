using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Voat.Configuration;
using Voat.Data;
using Voat.Domain.Command;
using Voat.Voting.Attributes;
using Voat.Voting.Options;

namespace Voat.Voting.Restrictions
{
    [Restriction(
        Enabled = true, 
        Description = "Restriction by the count of posts (comments and/or submissions)", 
        Name = "Contribution Count Restriction")]
    public class ContributionCountRestriction : VoteRestriction<ContentOption>
    {
        public override CommandResponse<IVoteRestriction> Evaluate(IPrincipal principal)
        {
            var evaluation = CommandResponse.FromStatus<IVoteRestriction>(null, Status.Success);
            using (var repo = new Repository())
            {
                var count = repo.UserContributionCount(principal.Identity.Name, Options.ContentType, Options.Subverse, Options.DateRange);
                if (count < Options.MinimumCount)
                {
                    evaluation = CommandResponse.FromStatus<IVoteRestriction>(this, Status.Denied, $"User only has {count} and needs {Options.MinimumCount}");
                }
            }
            return evaluation;
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
