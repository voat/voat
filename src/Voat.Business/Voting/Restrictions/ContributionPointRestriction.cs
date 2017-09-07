using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Voat.Configuration;
using Voat.Domain.Command;
using Voat.Voting.Attributes;

namespace Voat.Voting.Restrictions
{
    [Restriction(
        Enabled = true,
        Name = "Contribution Point Restriction",
        Description = "Restriction by the points earned on (comments and/or submissions)"
    )]
    public class ContributionPointRestriction : ContributionRestriction
    {
        public override CommandResponse<IVoteRestriction> Evaluate(IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override string ToDescription()
        {
            return $"Requires {MinimumCount} earned points for {ContentTypeDescription(true)} {WhereDescription()} {DateRangeDescription()}";
        }
    }
}
