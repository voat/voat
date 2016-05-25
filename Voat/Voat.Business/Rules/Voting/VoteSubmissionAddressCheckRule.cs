using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;
using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    public class VoteSubmissionAddressCheckRule : VoatRule
    {
        public VoteSubmissionAddressCheckRule() : base("Check Comment Vote IP Address", "8.2", RulesEngine.RuleScope.VoteSubmission, 10) { }
        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            using (var repo = new Repository())
            {
                if (repo.HasAddressVoted(context.PropertyBag.AddressHash, ContentType.Submission, context.CommentID.Value))
                {
                    return CreateOutcome(RuleResult.Denied, "Vote has already been registered");
                }
            }
            return Allowed;
        }
    }
}
