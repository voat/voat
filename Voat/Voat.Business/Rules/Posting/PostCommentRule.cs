using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a comment if it hasn't been submitted before and is valid.", "approved = (comment.Exists(content) == false && comment.IsValid())")]
    public class PostCommentRule : BaseCommentRule
    {

        public PostCommentRule()
            : base("Post Comment", "7.1", RuleScope.PostComment)
        {

        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            //rules checked in base common class
            return base.EvaluateRule(context);
        }
    }
}