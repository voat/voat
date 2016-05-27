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
    [RuleDiscovery("Approved if vote hasn't been registered from same device.", "approved = (hasVoted(Submission, AddressHash) == false)")]
    public class VoteSubmissionAddressCheckRule : VoatRule
    {
        public VoteSubmissionAddressCheckRule() 
            : base("Submission Vote Identity", "2.6", RulesEngine.RuleScope.VoteSubmission, 10)
        {

        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            using (var repo = new Repository())
            {
                if (repo.HasAddressVoted(context.PropertyBag.AddressHash, ContentType.Submission, context.CommentID.Value))
                {
                    return CreateOutcome(RuleResult.Denied, "Vote has already been registered for device");
                }
            }
            return Allowed;
        }
    }
}
