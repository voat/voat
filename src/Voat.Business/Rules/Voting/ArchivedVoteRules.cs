#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

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
