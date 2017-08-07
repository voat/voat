using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Voting.Models;

namespace Voat.Voting.Outcomes
{
    public class ModeratorOutcome : VoteOutcome
    {
        public override CommandResponse Execute()
        {
            throw new NotImplementedException();
        }

        public override string ToDescription()
        {
            if (Action == ModeratorVoteAction.Add)
            {
                return $"Will add @{UserName} to v/{Subverse} as a {Level}";
            }
            else
            {
                return $"Will remove @{UserName} from v/{Subverse} as a {Level}";
            }
        }
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Subverse { get; set; }

        public ModeratorVoteAction Action { get; set; }

        public ModeratorLevel Level { get; set; }
    }
}
