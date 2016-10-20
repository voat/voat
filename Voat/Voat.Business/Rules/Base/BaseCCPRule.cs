using System;
using Voat.RulesEngine;

namespace Voat.Rules
{
    /// <summary>
    /// Base class for any simple rules concerning only CCP and a user action.
    /// </summary>
    public abstract class BaseCCPVote : MinimumCCPRule
    {
        public BaseCCPVote(string name, string number, int minCCP, RuleScope scope)
            : base(name, number, minCCP, scope)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            if (context.UserData.Information.CommentPoints.Sum < MinimumCommentPoints)
            {
                return CreateOutcome(RuleResult.Denied, (String.Format("CCP of {0} is below minimum of {1} required for action {2}", context.UserData.Information.CommentPoints.Sum, MinimumCommentPoints, Scope.ToString())));
            }
            return base.EvaluateRule(context);
        }
    }
}
