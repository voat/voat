using System;
using System.Collections.Generic;
using System.Text;
using Voat.Domain.Command;
using Voat.Voting.Options;

namespace Voat.Voting.Outcomes
{
    public abstract class VoteOutcome<T> : OptionHandler<T> where T: OutcomeOption
    {
        public abstract CommandResponse Execute(T options);
    }
}
