using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Voting.Attributes;

namespace Voat.Voting.Outcomes
{
    [Outcome(Enabled = false, Name = "Add Subverse Rule", Description = "Add new rule to a subverse")]
    public class AddSubverseRuleOutcome : SubverseVoteOutcome
    {
        public ContentType ContentType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }


        public override Task<CommandResponse> Execute()
        {
            throw new NotImplementedException();
        }

        public override string ToDescription()
        {
            throw new NotImplementedException();
        }
    }
    [Outcome(Enabled = false, Name = "Remove Subverse Rule", Description = "Remove and existing subverse rule")]
    public class RemoveSubverseRuleOutcome : SubverseVoteOutcome
    {
        public int RuleSetID { get; set; }

        public override Task<CommandResponse> Execute()
        {
            throw new NotImplementedException();
        }

        public override string ToDescription()
        {
            throw new NotImplementedException();
        }
    }
    [Outcome(Enabled = false, Name = "Modify Subverse Rule", Description = "Modify an existing subverse rule")]
    public class ModifySubverseRuleOutcome : AddSubverseRuleOutcome
    {
        public int RuleSetID { get; set; }

        public override Task<CommandResponse> Execute()
        {
            throw new NotImplementedException();
        }

        public override string ToDescription()
        {
            throw new NotImplementedException();
        }
    }
}
