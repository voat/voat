using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Voat.Configuration;
using Voat.Domain.Command;

namespace Voat.Voting.Restrictions
{
    public class ContributionPointRestriction : ContributionRestriction
    {
        public override CommandResponse<IVoteRestriction> Evaluate(IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override string ToDescription()
        {
            var where = $"on {VoatSettings.Instance.SiteName}";
            if (!String.IsNullOrEmpty(Subverse))
            {
                where = $"in v/{Subverse}";
            }
            return $"Have at least {MinimumCount} points for {ContentType} {where} from {DateRange.ToString()}";

        }
    }
}
