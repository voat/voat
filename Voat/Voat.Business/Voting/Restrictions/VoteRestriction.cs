using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Voat.Voting.Options;

namespace Voat.Voting.Restrictions
{
    public abstract class VoteRestriction<T> : VoteOptionItem<T> where T: RestrictionOption
    {
        public abstract bool Evaluate(IPrincipal principal);

    }
}
