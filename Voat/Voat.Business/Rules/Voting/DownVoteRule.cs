using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    [RuleDiscovery("Approved if user has 100 CCP or higher.", "approved = (user.CCP >= 100)")]
    public class DownVoteRule : BaseCCPVote
    {
        public DownVoteRule() : base("Downvote", "2.3", 100, RuleScope.DownVote)
        {
        }
    }
}
