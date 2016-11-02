using System;
using Voat.Data;
using Voat.Domain.Models;
using Voat.RulesEngine;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a submission if the url hasn't been recently submitted", "approved = (submission.Exists(duration: 15 days) == false)")]
    public class PostSubmissionRecentlySubmittedRule : VoatRule
    {
        public PostSubmissionRecentlySubmittedRule()
            : base("Recently Submitted", "5.7", RuleScope.PostSubmission)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            UserSubmission submission = context.PropertyBag.UserSubmission;
            switch (submission.Type)
            {
                case SubmissionType.Link:
                    Data.Models.Submission recentlySubmitted = null;
                    using (var repo = new Repository())
                    {
                        recentlySubmitted = repo.FindSubverseLinkSubmission(context.Subverse.Name, submission.Url, TimeSpan.FromDays(15));
                    }
                    if (recentlySubmitted != null)
                    {
                        return CreateOutcome(RuleResult.Denied, $"Sorry, this link has already been submitted recently. https://voat.co/v/{recentlySubmitted.Subverse}/{recentlySubmitted.ID}");
                    }
                    break;

                case SubmissionType.Text:

                    //containsBannedDomain = BanningUtility.ContentContainsBannedDomain(context.Subverse.Name, submission.Content);
                    break;
            }

            return Allowed;
        }
    }
}
