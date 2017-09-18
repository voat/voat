using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Voat.Common;
using Voat.Configuration;
using Voat.Data;
using Voat.Domain.Command;
using Voat.Voting.Attributes;

namespace Voat.Voting.Restrictions
{
    [Restriction(
        Enabled = true,
        Name = "Contribution Count Restriction",
        Description = "Restriction by the count of posts (comments and/or submissions)" 
        )]
    public class ContributionCountRestriction : ContributionRestriction
    {
        public override CommandResponse<IVoteRestriction> Evaluate(IPrincipal principal)
        {
            var evaluation = CommandResponse.FromStatus<IVoteRestriction>(this, Status.Success);
            using (var repo = new Repository())
            {
                var count = repo.UserContributionCount(principal.Identity.Name, (Voat.Domain.Models.ContentType)ContentType, Subverse, DateRange);
                if (count < MinimumCount)
                {
                    evaluation = CommandResponse.FromStatus<IVoteRestriction>(this, Status.Denied, $"User only has {count} and needs {MinimumCount}");
                }
            }
            return evaluation;
        }

        public override string ToDescription()
        {
            return $"Requires {MinimumCount} {ContentTypeDescription()} posts {WhereDescription()} {DateRangeDescription()}";
        }
    }
}
