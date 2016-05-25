using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    [RuleDiscovery("Approved if user has more upvotes than downvotes on comments", "approved = (user.Comment.UpVotes > user.Comment.DownVotes)")]
    public class DownVoteCommentMeanieRule : VoatRule
    {
        public DownVoteCommentMeanieRule() : base("Comment Meanie", "6.2", RuleScope.DownVoteComment)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            if (context.UserData.Information.CommentVoting.Sum < 0)
            {
                return CreateOutcome(RuleResult.Denied, "Can not downvote more than you upvote");
            }
            return base.EvaluateRule(context);
        }
    }
}
