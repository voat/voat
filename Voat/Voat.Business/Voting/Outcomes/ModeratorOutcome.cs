using System;
using System.Collections.Generic;
using System.Text;
using Voat.Voting.Options;

namespace Voat.Voting.Outcomes
{
    public class ModeratorOutcome : VoteOutcome<ModeratorOption>
    {
        public override void Execute(ModeratorOption options)
        {
            throw new NotImplementedException();
        }

        public override string ToDescription()
        {
            if (Options.Action == ModeratorAction.Add)
            {
                return $"Will add @{Options.UserName} to v/{Options.Subverse} as a {Options.Level}";
            }
            else
            {
                return $"Will remove @{Options.UserName} from v/{Options.Subverse} as a {Options.Level}";
            }
        }
    }
}
