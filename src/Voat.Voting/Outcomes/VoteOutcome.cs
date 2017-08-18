using System;
using System.Collections.Generic;
using System.Text;
using Voat.Domain.Command;

namespace Voat.Voting.Outcomes
{
    public abstract class VoteOutcome : VoteItem
    {
        public abstract CommandResponse Execute();
    }
}
