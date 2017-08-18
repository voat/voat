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
using Voat.Data;
using Voat.Domain.Query;
using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    [RuleDiscovery(false, "Approved if user isn't brigading another users comments", "approved = (!isUserBrigading())")]
    public class DownVoteCommentBrigadeRule : VoatRule
    {
        private TimeSpan _timeSpan = TimeSpan.FromMinutes(30);
        private int _threshold = 10;

        public DownVoteCommentBrigadeRule() : base("Downvote Comment Brigading Rule", "2.9", RuleScope.DownVoteComment)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            var q = new QueryComment(context.CommentID.Value);
            var comment = q.Execute();

            using (var repo = new Repository(context.User))
            {
                var count = repo.VoteCount(context.UserName, comment.UserName, Domain.Models.ContentType.Comment, Domain.Models.VoteValue.Down, _timeSpan);
                if (count >= _threshold)
                {
                    return CreateOutcome(RuleResult.Denied, "You need a cooling down period");
                }
            }
            return base.EvaluateRule(context);
        }
    }
}
