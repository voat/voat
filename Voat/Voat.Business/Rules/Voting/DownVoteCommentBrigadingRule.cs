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

            using (var repo = new Repository())
            {
                var count = repo.VoteCount(context.UserName, comment.UserName, Domain.Models.ContentType.Comment, Domain.Models.Vote.Down, _timeSpan);
                if (count >= _threshold)
                {
                    return CreateOutcome(RuleResult.Denied, "You need a cooling down period");
                }
            }
            return base.EvaluateRule(context);
        }
    }
}
