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

using Voat.Data;
using Voat.Domain.Models;
using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    [RuleDiscovery("Approved if vote hasn't been registered from same device.", "approved = (hasVoted(Comment, AddressHash) == false)")]
    public class VoteCommentAddressCheckRule : VoatRule
    {
        public VoteCommentAddressCheckRule()
            : base("Comment Vote Identity", "2.8", RulesEngine.RuleScope.VoteComment, 10)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            using (var repo = new Repository())
            {
                int? existingVote = context.PropertyBag.CurrentVoteValue;

                if ((existingVote == null || existingVote.Value == 0) && repo.HasAddressVoted(context.PropertyBag.AddressHash, ContentType.Comment, context.CommentID.Value))
                {
                    return CreateOutcome(RuleResult.Denied, "Vote has already been registered");
                }
            }
            return Allowed;
        }
    }
}
