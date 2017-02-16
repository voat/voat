using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    [RuleDiscovery("Approved if submission is not archived", "approved = (!submission.IsArchived)")]
    public class ArchivedVoteCommentRule : VoatRule
    {
        public ArchivedVoteCommentRule() : base("Archived Vote Rule", "2.9.1", RuleScope.Vote)
        {
            RequiredContext.Add("Submission", typeof(Domain.Models.Submission));
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {

            Domain.Models.Submission submission = context.PropertyBag.Submission;

            if (submission.ArchiveDate != null)
            {
                return CreateOutcome(RuleResult.Denied, "Archived Submissions do not allow voting");
            }
            if (submission.IsDeleted)
            {
                return CreateOutcome(RuleResult.Denied, "Deleted Submissions do not allow voting");
            }
            return base.EvaluateRule(context);
        }
    }
}
