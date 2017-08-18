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
            int dailyVotingQuota = VoatSettings.Instance.DailyVotingQuota;
            int dailyVotingQuotaScaledMinimum = VoatSettings.Instance.DailyVotingQuotaScaledMinimum;

            var userCCP = context.UserData.Information.CommentPoints.Sum;

            //TODO: Configure this scale in configuration file instead of hardcoding
            //if user has 20+ use scaled quota, else use 10
            var scaledDailyVotingQuota = (userCCP >= 20 ? Math.Max(dailyVotingQuota, userCCP / 2) : dailyVotingQuotaScaledMinimum);
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
