using Voat.Domain.Models;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a submission post if data does not contain banned domains.", "approved = (submission.ContainsBannedDomains() == false)")]
    public class PostSubmissionBannedDomainRule : VoatRule
    {
        public PostSubmissionBannedDomainRule()
            : base("Submission Banned Domain", "5.2", RuleScope.PostSubmission)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            UserSubmission submission = context.PropertyBag.UserSubmission;

            //Check banned domains in submission content
            var containsBannedDomain = false;
            switch (submission.Type)
            {
                case SubmissionType.Link:
                    containsBannedDomain = BanningUtility.ContentContainsBannedDomain(context.Subverse.Name, submission.Url);
                    break;

                case SubmissionType.Text:
                    containsBannedDomain = BanningUtility.ContentContainsBannedDomain(context.Subverse.Name, submission.Content);
                    break;
            }
            if (containsBannedDomain)
            {
                return CreateOutcome(RuleResult.Denied, "Submission contains banned domains");
            }

            return Allowed;
        }
    }
}
