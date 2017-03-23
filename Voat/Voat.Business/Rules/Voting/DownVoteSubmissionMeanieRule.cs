using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    [RuleDiscovery("Approved if user has more upvotes than downvotes on submissions", "approved = (user.Submission.UpVotes > user.Submission.DownVotes)")]
    public class DownVoteSubmissionMeanieRule : VoatRule
    {
        public DownVoteSubmissionMeanieRule() : base("Submission Voting Meanie", "2.3.1", RuleScope.DownVoteSubmission)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            if (context.UserData.Information.SubmissionVoting.Sum < 0)
            {
                return CreateOutcome(RuleResult.Denied, "Cannot downvote more than you upvote");
            }
            return base.EvaluateRule(context);
        }
    }
}
