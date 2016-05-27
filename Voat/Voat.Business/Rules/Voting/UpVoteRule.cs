using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    [RuleDiscovery("Approved if user has more than 20 CCP.", "approved = (user.CCP >= 20)")]
    public class UpVoteRule : BaseCCPVote
    {
        public UpVoteRule() : base("UpVote Submission", "2.1", 20, RuleScope.UpVote)
        {
        }
    }
}
