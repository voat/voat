using System;
using System.Collections.Generic;
using System.Text;
using Voat.Domain.Command;

namespace Voat.Voting.Restrictions
{
    public class RestrictionViolation
    {
        public RestrictionViolation(IVoteRestriction restriction)
        {

        }

    }
    public class RestrictionEvaluation
    {
        public List<CommandResponse<IVoteRestriction>> Violations = new List<CommandResponse<IVoteRestriction>>();
        public bool IsValid { get => Violations.Count == 0; }
    }
}
