using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.RulesEngine;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a comment if Submission isn't archived.", "approved = (!submission.IsArchived)")]
    public class PostCommentArchiveRule : VoatRule
    {
        public PostCommentArchiveRule()
            : base("Post Comment", "7.1.1", RuleScope.PostComment)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            var submission = context.PropertyBag.Submission;

            if (submission.ArchiveDate != null)
            {
                return CreateOutcome(RuleResult.Denied, "Archived Submissions do not allow new comments");
            }

            return Allowed;
        }
    }
}
