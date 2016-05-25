using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Configuration;
using Voat.RulesEngine;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approved if user who has -50 CCP or less has submitted fewer than 5 comments in a 24 hour sliding window.", "approved = (user.CCP <= -50 and TotalCommentsInPast24Hours < 5 or user.CCP > -50)")]
    public class PostCommentCCPRule : MinimumCCPRule
    {
        private int countThreshold = Settings.DailyCommentPostingQuotaForNegativeScore;

        public PostCommentCCPRule() : base("Comment CCP Throttle", "6.0", -50, RuleScope.PostComment)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            if (context.UserData.Information.CommentPoints.Sum <= MinimumCommentPoints && context.UserData.TotalVotesUsedIn24Hours >= countThreshold)
            {
                return CreateOutcome(RuleResult.Denied, "User with CCP value of {0} is limited to {1} comment(s) in 24 hours.", context.PropertyBag.CCP, countThreshold);
            }

            return base.EvaluateRule(context);
        }
    }
}
