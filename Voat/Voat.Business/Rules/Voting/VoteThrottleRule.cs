using System;
using Voat.Configuration;
using Voat.RulesEngine;

namespace Voat.Rules.Voting
{
    [RuleDiscovery("Approves action if quota not exceeded for votes in a 24 sliding window.", "approved = (user.TotalVotesInPast24Hours < max(quota, user.CCP / 2))")]
    public class VoteThrottleRule : VoatRule
    {
        public VoteThrottleRule()
            : base("Vote Throttle", "4.0", RuleScope.Vote)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            int dailyVotingQuota = Settings.DailyVotingQuota;
            var userCCP = context.UserData.Information.CommentPoints.Sum;

            //if user has 20+ use scaled quota, else use 10
            var scaledDailyVotingQuota = (userCCP >= 20 ? Math.Max(dailyVotingQuota, userCCP / 2) : 10);
            var totalVotesUsedInPast24Hours = context.UserData.TotalVotesUsedIn24Hours;

            //see if they have a current vote on this item and only evaluate if they don't
            int? existingVote = context.PropertyBag.CurrentVoteValue;

            if ((existingVote == null || existingVote.Value == 0) && totalVotesUsedInPast24Hours >= scaledDailyVotingQuota)
            {
                return CreateOutcome(RuleResult.Denied, "Vote limit exceeded based on CCP. Available votes per 24 hours: {0}", scaledDailyVotingQuota);
            }

            //switch (context)
            //{
            //    case 1:
            //        if (userCcp >= 20)
            //        {
            //            if (totalVotesUsedInPast24Hours < scaledDailyVotingQuota)
            //            {
            //                // perform upvoting or resetting
            //                VotingComments.UpvoteComment(commentId, loggedInUser, IpHash.CreateHash(UserHelper.UserIpAddress(Request)));
            //            }
            //        }
            //        else if (totalVotesUsedInPast24Hours < 11)
            //        {
            //            // perform upvoting or resetting even if user has no CCP but only allow 10 votes per 24 hours
            //            VotingComments.UpvoteComment(commentId, loggedInUser, IpHash.CreateHash(UserHelper.UserIpAddress(Request)));
            //        }
            //        break;
            //    case -1:
            //        if (userCcp >= 100)
            //        {
            //            if (totalVotesUsedInPast24Hours < scaledDailyVotingQuota)
            //            {
            //                // perform downvoting or resetting
            //                VotingComments.DownvoteComment(commentId, loggedInUser, IpHash.CreateHash(UserHelper.UserIpAddress(Request)));
            //            }
            //        }
            //        break;
            //}

            return base.EvaluateRule(context);
        }
    }
}
