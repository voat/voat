using Voat.RulesEngine;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a comment post if user is not banned in subverse.", "approved = (user.IsBannedFromSubverse(subverse) == false)")]
    public class PostCommentBannedRule : BaseSubverseRule
    {
        public PostCommentBannedRule()
            : base("Subverse Comment Ban Rule", "5.3", RuleScope.PostComment)
        {
            this.Order = 1;
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            //base class will check for subverse and global bans here
            return base.EvaluateRule(context);
        }
    }
}
