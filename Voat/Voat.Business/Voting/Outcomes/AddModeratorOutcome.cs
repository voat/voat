using System;
using System.Collections.Generic;
using System.Text;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Voting.Attributes;

namespace Voat.Voting.Outcomes
{
    [Outcome(Enabled = true, Name = "Add Moderator Outcome", Description = "Adds a moderator to a subverse")]
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
