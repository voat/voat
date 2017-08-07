using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Voat.Domain.Command;

namespace Voat.Voting.Restrictions
{
    public interface IVoteRestriction
    {
        CommandResponse<IVoteRestriction> Evaluate(IPrincipal principal);
        
        string Group { get; }
    }
}
