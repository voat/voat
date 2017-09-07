using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Voat.Configuration;
using Voat.Data;
using Voat.Domain.Command;
using Voat.Voting.Attributes;

namespace Voat.Voting.Restrictions
{
    [Restriction(
       Enabled = true,
       Name = "Contribution Vote Restriction",
       Description = "Restriction by the votes given on (comments and/or submissions)"
   )]
    public class ContributionVoteRestriction : ContributionRestriction
    {
        public override CommandResponse<IVoteRestriction> Evaluate(IPrincipal principal)
        {
            using (var repo = new Repository())
            {
                //var score = repo.UserVotingBehavior(principal.Identity.Name, Options.Subverse, Options.ContentType, Options.Duration, Options.CutOffDate);
                //return score.Sum >= Options.MinimumCount;
            }
            return null;
        }

        public override string ToDescription()
        {
            return $"Requires {MinimumCount} votes on {ContentTypeDescription(true)} {WhereDescription()} {DateRangeDescription()}";
        }
    }
}
