using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Voat.Configuration;
using Voat.Domain.Command;

namespace Voat.Voting.Restrictions
{
    public class MemberAgeRestriction : VoteRestriction
    {
        public override CommandResponse<IVoteRestriction> Evaluate(IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public override string ToDescription()
        {
            return $"Account created before {EndDate}";
        }
    }
}
