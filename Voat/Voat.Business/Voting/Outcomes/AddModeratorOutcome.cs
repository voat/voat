using System;
using System.Collections.Generic;
using System.Text;
using Voat.Domain.Command;
using Voat.Domain.Models;

namespace Voat.Voting.Outcomes
{
    public class AddModeratorOutcome : ModeratorOutcome
    {
        public override CommandResponse Execute()
        {
            throw new NotImplementedException();
        }

        public override string ToDescription()
        {
            return $"Will add @{UserName} to v/{Subverse} as a {Level.ToString()}";
        }
        public ModeratorLevel Level { get; set; }

    }
}
