using System;
using System.Collections.Generic;
using System.Text;
using Voat.Voting.Options;

namespace Voat.Voting.Outcomes
{
    public abstract class VoteOutcome<T> : OptionHandler<T> where T: OutcomeOption
    {
        public abstract void Execute(T options);
    }
}
