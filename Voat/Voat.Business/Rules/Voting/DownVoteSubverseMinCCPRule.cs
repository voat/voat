using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    [RuleDiscovery("Approves submission downvote if user has more CCP in subverse than what is specified by the subverse minimum for downvoting.", "approved = ((subverse.minCCP > 0 and user.subverseCCP > subverse.minCCP) or subverse.minCCP == 0)")]
    public class DownVoteSubverseMinCCPRule : BaseSubverseMinimumCCPRule
    {
        public DownVoteSubverseMinCCPRule()
            : base("Subverse Min CCP Downvote", "2.4", RuleScope.DownVote)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            return base.EvaluateRule(context);
        }
    }
}
