using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Voat.Voting.Outcomes
{
    public abstract class SubverseVoteOutcome : VoteOutcome, ISubverse
    {
        [Required]
        public string Subverse { get; set; }
    }
}
