using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Query;
using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    [RuleDiscovery("Approved if comment is less that 7 days old", "approved = (comment.Age < 7 days)")]
    public class DownVoteCommentAgeRule : VoatRule
    {
        public DownVoteCommentAgeRule() : base("Downvote Comment Age", "6.3", RuleScope.DownVoteComment)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            var q = new QueryComment(context.CommentID.Value);
            var comment = q.Execute();

            // do not execute downvoting if comment is older than 7 days
            var commentPostingDate = comment.CreationDate;
            TimeSpan timeElapsed = Repository.CurrentDate - commentPostingDate;
            if (timeElapsed.TotalDays > 7)
            {
                return CreateOutcome(RuleResult.Denied, "Comment downvotes not registered after 7 days");
            }
            return base.EvaluateRule(context);
        }
    }
}
