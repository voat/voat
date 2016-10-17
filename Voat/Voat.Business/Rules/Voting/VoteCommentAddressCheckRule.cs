using Voat.Data;
using Voat.Domain.Models;
using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    [RuleDiscovery("Approved if vote hasn't been registered from same device.", "approved = (hasVoted(Comment, AddressHash) == false)")]
    public class VoteCommentAddressCheckRule : VoatRule
    {
        public VoteCommentAddressCheckRule()
            : base("Comment Vote Identity", "2.8", RulesEngine.RuleScope.VoteComment, 10)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            using (var repo = new Repository())
            {
                int? existingVote = context.PropertyBag.CurrentVoteValue;

                if ((existingVote == null || existingVote.Value == 0) && repo.HasAddressVoted(context.PropertyBag.AddressHash, ContentType.Comment, context.CommentID.Value))
                {
                    return CreateOutcome(RuleResult.Denied, "Vote has already been registered");
                }
            }
            return Allowed;
        }
    }
}
