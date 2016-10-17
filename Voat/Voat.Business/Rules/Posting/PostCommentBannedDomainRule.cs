using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a comment post if data does not contain banned domains.", "approved = (comment.ContainsBannedDomains() == false)")]
    public class PostCommentBannedDomainRule : VoatRule
    {
        public PostCommentBannedDomainRule()
            : base("Comment Banned Domain", "7.4", RuleScope.PostComment)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            string content = context.PropertyBag.CommentContent;

            //Check banned domains in submission content
            var containsBannedDomain = BanningUtility.ContentContainsBannedDomain(context.Subverse.Name, content);
            if (containsBannedDomain)
            {
                return CreateOutcome(RuleResult.Denied, "Comment contains banned domains");
            }

            return Allowed;
        }
    }
}
