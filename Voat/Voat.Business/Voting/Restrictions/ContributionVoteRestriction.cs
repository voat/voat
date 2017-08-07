using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Voat.Configuration;
using Voat.Data;
using Voat.Domain.Command;

namespace Voat.Voting.Restrictions
{
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
            var where = $"on {VoatSettings.Instance.SiteName}";
            if (!String.IsNullOrEmpty(Subverse))
            {
                where = $"in v/{Subverse}";
            }
            return $"Has voted at least {MinimumCount} times on {ContentType} {where} from {DateRange.ToString()}";

        }
    }
}
