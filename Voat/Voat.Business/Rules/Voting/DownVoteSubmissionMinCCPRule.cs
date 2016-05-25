using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    [RuleDiscovery("Approves submission downvote if user has more CCP in subverse than what is specified by the subverse minimum for downvoting.", "approved = ((subverse.minCCP > 0 and user.subverseCCP > subverse.minCCP) or subverse.minCCP == 0)")]
    public class DownVoteSubmissionMinCCPRule : BaseSubverseMinimumCCPRule
    {
        public DownVoteSubmissionMinCCPRule()
            : base("Subverse Min CCP Downvoat", "5.1", RuleScope.DownVoteSubmission)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            return base.EvaluateRule(context);
        }
    }
}
