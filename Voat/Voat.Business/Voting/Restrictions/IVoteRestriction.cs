using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace Voat.Voting.Restrictions
{
    public interface IVoteRestriction
    {
        bool Evaluate(IPrincipal principal);
    }
}
