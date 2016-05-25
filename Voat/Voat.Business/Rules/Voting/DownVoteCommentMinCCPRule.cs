using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    //The base class handles all logic, this class is just used for rule engine processing
    [RuleDiscovery("Approves comment downvote if user has more CCP in subverse than what is specified by the subverse minimum for downvoting.", "approved = ((subverse.minCCP > 0 and user.subverseCCP > subverse.minCCP) or subverse.minCCP == 0)")]
    public class DownVoteCommentMinCCPRule : BaseSubverseMinimumCCPRule
    {
        public DownVoteCommentMinCCPRule()
            : base("Subverse Min CCP Downvoat", "5.2", RuleScope.DownVoteComment)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            return base.EvaluateRule(context);
        }
    }
}
