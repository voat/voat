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

using Voat.Configuration;
using Voat.RulesEngine;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approved if user who has -50 CCP or less has submitted fewer than 1 submission in a 24 hour sliding window.", "approved = (user.CCP <= -50 and TotalSubmissionsInPast24Hours < 1 or user.CCP > -50)")]
    public class PostSubmissionCCPRule : MinimumCCPRule
    {
        public PostSubmissionCCPRule() : base("Submission CCP Throttle", "4.4", -10, RuleScope.PostSubmission)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            //// if user CCP or SCP is less than -10, allow only X submissions per 24 hours
            //if (result != null && (context.UserData.Information.CommentPoints.Sum <= -10 || context.UserData.Information.SubmissionPoints.Sum <= -10)
            //    && UserHelper.UserDailyPostingQuotaForNegativeScoreUsed(context.UserName))
            //{
            //    result = CreateOutcome(RuleResult.Denied, String.Format("You have reached your daily submission quota. Your current quota is {0} submission(s) per 24 hours", VoatSettings.Instance.DailyPostingQuotaForNegativeScore));
            //}

            int postThreshold = VoatSettings.Instance.DailyPostingQuotaForNegativeScore;

            //var isModerator = context.UserData.Information.Moderates.Any(x => x == context.Subverse.Name);
            var userData = context.UserData;
            var userInfo = userData.Information;

            if (userInfo.CommentPoints.Sum <= base.MinimumCommentPoints && userData.TotalSubmissionsPostedIn24Hours >= postThreshold)
            {
                return CreateOutcome(RuleResult.Denied, "An Account with a CCP value of {0} is limited to {1} posts(s) in 24 hours", userInfo.CommentPoints.Sum, postThreshold);
            }

            if (userInfo.SubmissionPoints.Sum <= base.MinimumCommentPoints && userData.TotalSubmissionsPostedIn24Hours >= postThreshold)
            {
                return CreateOutcome(RuleResult.Denied, "An Account with a SCP value of {0} is limited to {1} posts(s) in 24 hours", userInfo.SubmissionPoints.Sum, postThreshold);
            }

            //less than zero turns off this check
            var minCCPForPost = VoatSettings.Instance.MinimumCommentPointsForSubmissionCreation;
            if (minCCPForPost != -1 && userInfo.CommentPoints.Sum < minCCPForPost)
            {
                return CreateOutcome(RuleResult.Denied, $"An Account must have a minimum of {minCCPForPost} CCP to create a submission");
            }
            
            return base.EvaluateRule(context);
        }
    }
}
