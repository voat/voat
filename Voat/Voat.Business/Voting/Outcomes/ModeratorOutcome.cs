using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Voat.Voting.Outcomes
{
    public abstract class ModeratorOutcome : VoteOutcome
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Subverse { get; set; }

    }
}
