using System;
using Voat.Domain.Models;
using Voat.Rules.Posting.Base;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a submission post if data does not contain banned domains.", "approved = (submission.ContainsBannedDomains() == false)")]
    public class PostSubmissionBannedDomainRule : SubmissionBannedDomainRule
    {
        public PostSubmissionBannedDomainRule()
            : base("Submission Banned Domain", "5.2.1", RuleScope.PostSubmission)
        {
        }
    }
}
