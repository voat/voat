using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Models;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a submission post if user is not banned in subverse.", "approved = (user.IsBannedFromSubverse(subverse) == false)")]
    public class PostSubmissionBannedUserRule : BaseSubverseBanRule
    {
        public PostSubmissionBannedUserRule()
            : base("Submission Banned User", "5.6", RuleScope.PostSubmission)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            //base class evaluates this
            return base.EvaluateRule(context);
        }
    }
}
