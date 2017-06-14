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

using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    [RuleDiscovery("Approved if user has more upvotes than downvotes on comments", "approved = (user.Comment.UpVotes > user.Comment.DownVotes)")]
    public class DownVoteCommentMeanieRule : VoatRule
    {
        public DownVoteCommentMeanieRule() : base("Comment Voting Meanie", "2.3.2", RuleScope.DownVoteComment)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            var sum = context.UserData.Information.CommentVoting.Sum;
            if (sum < 0)
            {
                return CreateOutcome(RuleResult.Denied, "Cannot downvote more than you upvote");
            }
            return base.EvaluateRule(context);
        }
    }
}
