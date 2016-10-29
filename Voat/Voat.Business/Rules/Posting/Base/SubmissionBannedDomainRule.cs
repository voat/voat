using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Models;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Posting.Base
{
    //[RuleDiscovery("Approves a submission post if data does not contain banned domains.", "approved = (submission.ContainsBannedDomains() == false)")]
    public class SubmissionBannedDomainRule : VoatRule
    {
        public SubmissionBannedDomainRule(string name, string number, RuleScope scope)
            : base(name, number, scope)
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
                    containsBannedDomain = BanningUtility.ContentContainsBannedDomain(context.Subverse.Name, $"{submission.Title} {submission.Url}");
                    break;

                case SubmissionType.Text:
                    containsBannedDomain = BanningUtility.ContentContainsBannedDomain(context.Subverse.Name, $"{submission.Title} {submission.Content}");
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
