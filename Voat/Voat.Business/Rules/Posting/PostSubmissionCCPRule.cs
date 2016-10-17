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
            //    result = CreateOutcome(RuleResult.Denied, String.Format("You have reached your daily submission quota. Your current quota is {0} submission(s) per 24 hours", Settings.DailyPostingQuotaForNegativeScore));
            //}

            int postThreshold = Settings.DailyPostingQuotaForNegativeScore;

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

            return base.EvaluateRule(context);
        }
    }
}
