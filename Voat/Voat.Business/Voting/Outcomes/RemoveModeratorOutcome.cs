using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Voting.Models;

namespace Voat.Voting.Outcomes
{
    public class RemoveModeratorOutcome : ModeratorOutcome
    {
        public override CommandResponse Execute()
        {
            throw new NotImplementedException();
        }

        public override string ToDescription()
        {
            return $"Will remove @{UserName} from v/{Subverse}";
        }
    }
}
