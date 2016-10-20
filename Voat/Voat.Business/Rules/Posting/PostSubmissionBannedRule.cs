using Voat.RulesEngine;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a submission post if user is not banned in subverse.", "approved = (user.IsBannedFromSubverse(subverse) == false)")]
    public class PostSubmissionBannedRule : BaseSubverseRule
    {
        public PostSubmissionBannedRule()
            : base("Subverse Submission Ban Rule", "5.6", RuleScope.PostSubmission)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            //base class evaluates this
            return base.EvaluateRule(context);
        }
    }
}
