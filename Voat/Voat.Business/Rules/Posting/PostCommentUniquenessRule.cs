using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a comment if it hasn't been submitted before.", "approved = (comment.Exists(content) == false)")]
    public class PostCommentUniquenessRule : BaseSubverseBanRule
    {

        public PostCommentUniquenessRule()
            : base("Comment Uniqueness Rule", "7.1", RuleScope.PostComment)
        {

        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {

            string content = context.PropertyBag.CommentContent;

            // check for copypasta
            // TODO: use Levenshtein distance algo or similar for better results
            var copyPasta = UserHelper.SimilarCommentSubmittedRecently(context.UserName, content);
            if (copyPasta)
            {
                return base.CreateOutcome(RuleResult.Denied, "You have recently submitted a similar comment. Please try to not use copy/paste so often.");
            }

            return base.Allowed;

        }
    }
}