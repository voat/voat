using Voat.Data;
using Voat.Domain.Models;
using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    [RuleDiscovery("Approved if vote hasn't been registered from same device.", "approved = (hasVoted(Submission, AddressHash) == false)")]
    public class VoteSubmissionAddressCheckRule : VoatRule
    {
        public VoteSubmissionAddressCheckRule()
            : base("Submission Vote Identity", "2.7", RulesEngine.RuleScope.VoteSubmission, 10)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            using (var repo = new Repository())
            {
                int? existingVote = context.PropertyBag.CurrentVoteValue;

                if ((existingVote == null || existingVote.Value == 0) && repo.HasAddressVoted(context.PropertyBag.AddressHash, ContentType.Submission, context.SubmissionID.Value))
                {
                    return CreateOutcome(RuleResult.Denied, "Vote has already been registered for device");
                }
            }
            return Allowed;
        }
    }
}
