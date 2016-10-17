using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a comment post if data does not contain banned domains.", "approved = (comment.ContainsBannedDomains() == false)")]
    public class EditCommentBannedDomainRule : VoatRule
    {
        public EditCommentBannedDomainRule()
            : base("Edit Comment Banned Domain", "7.5", RuleScope.EditComment)
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
