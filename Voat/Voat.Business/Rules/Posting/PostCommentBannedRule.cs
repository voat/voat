using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.RulesEngine;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a comment post if user is not banned in subverse.", "approved = (user.IsBannedFromSubverse(subverse) == false)")]
    public class PostCommentBannedRule : BaseSubverseBanRule
    {
        public PostCommentBannedRule()
            : base("Subverse Comment Ban Rule", "7.2", RuleScope.PostComment)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            return base.EvaluateRule(context);
        }
    }
}
