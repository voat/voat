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
    public class VoteCommentAddressCheckRule : VoatRule
    {
        public VoteCommentAddressCheckRule() : base("Check Comment Vote IP Address", "8.1", RulesEngine.RuleScope.VoteComment, 10) { }
        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            using (var repo = new Repository())
            {
                if (repo.HasAddressVoted(context.PropertyBag.AddressHash, ContentType.Comment, context.CommentID.Value))
                {
                    return CreateOutcome(RuleResult.Denied, "Vote has already been registered");
                }
            }
            return Allowed;
        }
    }
}
