using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.RulesEngine;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a submission post if user is not banned in subverse.", "approved = (user.IsBannedFromSubverse(subverse) == false)")]
    public class PostSubmissionBannedRule : BaseSubverseBanRule
    {
        public PostSubmissionBannedRule()
            : base("Subverse Submission Ban Rule", "5.2", RuleScope.PostSubmission)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            return base.EvaluateRule(context);
        }
    }

}
