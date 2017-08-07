using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Voat.Configuration;
using Voat.Domain.Command;
using Voat.Voting.Options;

namespace Voat.Voting.Restrictions
{
    public class MemberAgeRestriction : VoteRestriction<RestrictionOption>
    {
        public override CommandResponse<IVoteRestriction> Evaluate(IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override string ToDescription()
        {
            return $"Account created before {Options.EndDate}";
        }
    }
}
