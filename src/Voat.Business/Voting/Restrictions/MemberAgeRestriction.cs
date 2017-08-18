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
       Name = "Member Age Restriction",
       Description = "Restriction by the age of a user account"
   )]
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
