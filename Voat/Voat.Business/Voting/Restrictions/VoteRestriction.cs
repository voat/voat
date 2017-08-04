using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Voat.Voting.Options;

namespace Voat.Voting.Restrictions
{
    public abstract class VoteRestriction<T> : OptionHandler<T>, IVoteRestriction where T: RestrictionOption
    {
        public abstract bool Evaluate(IPrincipal principal);

    }
}
