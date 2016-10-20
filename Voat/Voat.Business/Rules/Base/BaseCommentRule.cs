using System;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules
{
    public class BaseCommentRule : VoatRule
    {
        public BaseCommentRule(string name, string number, RuleScope scope, int order = 100) : base(name, number, scope, order)
        {
            /*no-op*/
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            string content = context.PropertyBag.CommentContent;
            if (String.IsNullOrWhiteSpace(content))
            {
                return base.CreateOutcome(RuleResult.Denied, "Empty comments not allowed");
            }

            // check for copypasta
            // TODO: use Levenshtein distance algo or similar for better results
            var copyPasta = UserHelper.SimilarCommentSubmittedRecently(context.UserName, content);
            if (copyPasta)
            {
                return base.CreateOutcome(RuleResult.Denied, "You have recently submitted a similar comment. Please try to not use copy/paste so often.");
            }

            return Allowed;
        }
    }
}
