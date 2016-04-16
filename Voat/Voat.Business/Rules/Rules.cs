using System;
using System.Linq;
using Voat.Configuration;
using Voat.Data;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules
{
    #region 1.x Rules (Admin/Site Functionality Rules)

    [RuleDiscovery(false, "All posting is disabled.", "approved = (false)")]
    public class DisablePostingRule : VoatRule
    {
        public DisablePostingRule() : base("DisablePosting", "1.2", RuleScope.Post)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            return CreateOutcome(RuleResult.Denied, "All posting is disabled. Website is in READ ONLY mode.");
        }
    }

    [RuleDiscovery(false, "All voting is disabled.", "approved = (false)")]
    public class DisableVotingRule : VoatRule
    {
        public DisableVotingRule() : base("DisableVoting", "1.1", RuleScope.Vote)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            return CreateOutcome(RuleResult.Denied, "All voting is disabled. Website is in READ ONLY mode.");
        }
    }

    [RuleDiscovery(false, "Voat is in Read-Only mode.", "approved = (false)")]
    public class ReadOnlyRule : VoatRule
    {
        public ReadOnlyRule() : base("ReadOnly", "1.0", RuleScope.Global)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            return CreateOutcome(RuleResult.Denied, "All website actions are disabled. Website is in READ ONLY mode.");
        }
    }

    #endregion 1.x Rules (Admin/Site Functionality Rules)

    #region 2.x Basic Voat Rules https://github.com/voat/voat/issues/400

    [RuleDiscovery("Approved if user has 100 CCP or higher.", "approved = (user.CCP >= 100)")]
    public class DownVoteRule : BaseCCPVote
    {
        public DownVoteRule() : base("Downvote", "2.3", 100, RuleScope.DownVote)
        {
        }
    }

    [RuleDiscovery("Approved if user has more than 20 CCP.", "approved = (user.CCP > 20)")]
    public class UpVoteCommentRule : BaseCCPVote
    {
        public UpVoteCommentRule() : base("UpVote Comment", "2.2", 20, RuleScope.UpVoteComment)
        {
        }
    }

    [RuleDiscovery("Approved if user has more than 20 CCP.", "approved = (user.CCP > 20)")]
    public class UpVoteSubmissionRule : BaseCCPVote
    {
        public UpVoteSubmissionRule() : base("UpVote Submission", "2.1", 20, RuleScope.UpVoteSubmission)
        {
        }
    }

    #endregion 2.x Basic Voat Rules https://github.com/voat/voat/issues/400

    #region 4.x Rules - Throttle Rules

    //[RuleDiscovery("Approved if a user who has less than 20 CCP hasn't voted more than 10 times in the last 24 hours.", "approved = !(user.CCP <= 20 and user.TotalVotesInPast24Hours >= 10)")]
    //public class UpvoteLimitRule : VoatRule
    //{
    //    public UpvoteLimitRule()
    //        : base("New User Upvote Limit", "4.1", RuleScope.UpVote)
    //    {
    //    }

    //    protected override RuleOutcome EvaluateRule(VoatRuleContext context)
    //    {
    //        if (context.UserData.Information.CommentPoints.Sum <= 20 && context.UserData.TotalVotesUsedIn24Hours >= 10)
    //        {
    //            return CreateOutcome(RuleResult.Denied, "User has exceeded votes allowed per CCP and/or time");
    //        }

    //        return Allowed;
    //    }
    //}

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
            var userCcp = context.UserData.Information.CommentPoints.Sum;
            var scaledDailyVotingQuota = Math.Max(dailyVotingQuota, userCcp / 2);
            var totalVotesUsedInPast24Hours = context.UserData.TotalVotesUsedIn24Hours;

            if (totalVotesUsedInPast24Hours >= scaledDailyVotingQuota)
            {
                return CreateOutcome(RuleResult.Denied, "User has exceeded vote throttle limit based on CCP. Available votes per 24 hours: {0}", scaledDailyVotingQuota);
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

    //TODO: This rule need refactored, too many db calls.
    [RuleDiscovery("Approved if a user doesn't surpass Voat's submission throttling policy.", "approved = !(throttlePolicy.Surpassed = true)")]
    public class PostSubmissionRule : BaseBannedRule
    {
        public PostSubmissionRule()
            : base("Submission Post Rule", "4.3", RuleScope.PostSubmission)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            //this checks global ban
            var result = base.EvaluateRule(context);

            if (result.IsAllowed)
            {
                bool isMod = context.UserData.Information.Moderates.Any(x => x == context.Subverse.Name);

                // check posting quotas if user is posting to subs they do not moderate
                if (!isMod)
                {
                    // reject if user has reached global daily submission quota
                    if (UserHelper.UserDailyGlobalPostingQuotaUsed(context.UserName))
                    {
                        result = CreateOutcome(RuleResult.Denied, "You have reached your daily global submission quota");
                    }
                    // reject if user has reached global hourly submission quota
                    else if (UserHelper.UserHourlyGlobalPostingQuotaUsed(context.UserName))
                    {
                        result = CreateOutcome(RuleResult.Denied, "You have reached your hourly global submission quota");
                    }
                    // check if user has reached hourly posting quota for target subverse
                    else if (UserHelper.UserHourlyPostingQuotaForSubUsed(context.UserName, context.Subverse.Name))
                    {
                        result = CreateOutcome(RuleResult.Denied, "You have reached your hourly submission quota for this subverse");
                    }
                    // check if user has reached daily posting quota for target subverse
                    else if (UserHelper.UserDailyPostingQuotaForSubUsed(context.UserName, context.Subverse.Name))
                    {
                        result = CreateOutcome(RuleResult.Denied, "You have reached your daily submission quota for this subverse");
                    }
                    else if (context.Subverse.IsAuthorizedOnly)
                    {
                        result = CreateOutcome(RuleResult.Denied, "You are not authorized to submit links or start discussions in this subverse. Please contact subverse moderators for authorization.");
                    }
                }
                
                // if user CCP or SCP is less than -10, allow only X submissions per 24 hours
                if ((context.UserData.Information.CommentPoints.Sum <= -10 || context.UserData.Information.SubmissionPoints.Sum <= -10) 
                    && UserHelper.UserDailyPostingQuotaForNegativeScoreUsed(context.UserName))
                {
                    result = CreateOutcome(RuleResult.Denied, String.Format("You have reached your daily submission quota. Your current quota is {0} submission(s) per 24 hours.", Settings.DailyPostingQuotaForNegativeScore));
                }
            }
            return result;
        }
    }

    public class PostCommentRule : BaseBannedRule
    {

        public PostCommentRule()
            : base("Comment Post Rule", "4.4", RuleScope.PostComment)
        {

        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {

            #region Original Logic
            //// flag the comment as anonymized if it was submitted to a sub which has active anonymized_mode
            //var submission = DataCache.Submission.Retrieve(commentModel.SubmissionID.Value);
            //var subverse = DataCache.Subverse.Retrieve(submission.Subverse);
            //var userCcp = Karma.CommentKarma(User.Identity.Name);
            //commentModel.IsAnonymized = submission.IsAnonymized || subverse.IsAnonymized;

            //// if user CCP is negative and account less than 6 months old, allow only x comment submissions per 24 hours
            //var userRegistrationDate = UserHelper.GetUserRegistrationDateTime(User.Identity.Name);
            //TimeSpan userMembershipTimeSpam = Repository.CurrentDate - userRegistrationDate;
            //if (userMembershipTimeSpam.TotalDays < 180 && userCcp < 1)
            //{
            //    var quotaUsed = UserHelper.UserDailyCommentPostingQuotaForNegativeScoreUsed(User.Identity.Name);
            //    if (quotaUsed)
            //    {
            //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You have reached your daily comment quota. Your current quota is " + Settings.DailyCommentPostingQuotaForNegativeScore.ToString() + " comment(s) per 24 hours.");
            //    }
            //}

            //// if user CCP is < 50, allow only X comment submissions per 24 hours
            //if (userCcp <= -50)
            //{
            //    var quotaUsed = UserHelper.UserDailyCommentPostingQuotaForNegativeScoreUsed(User.Identity.Name);
            //    if (quotaUsed)
            //    {
            //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You have reached your daily comment quota. Your current quota is " + Settings.DailyCommentPostingQuotaForNegativeScore.ToString() + " comment(s) per 24 hours.");
            //    }
            //}

            //// check if author is banned, don't save the comment or send notifications if true
            //if (!UserHelper.IsUserGloballyBanned(User.Identity.Name) && !UserHelper.IsUserBannedFromSubverse(User.Identity.Name, submission.Subverse))
            //{
            //    bool containsBannedDomain = BanningUtility.ContentContainsBannedDomain(subverse.Name, commentModel.Content);
            //    if (containsBannedDomain)
            //    {
            //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Comment contains links to banned domain(s).");
            //    }


            //    if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPreSave))
            //    {
            //        commentModel.Content = ContentProcessor.Instance.Process(commentModel.Content, ProcessingStage.InboundPreSave, commentModel);
            //    }

            //    //save fully formatted content 
            //    var formattedComment = Formatting.FormatMessage(commentModel.Content);
            //    commentModel.FormattedContent = formattedComment;

            //    _db.Comments.Add(commentModel);

            //    await _db.SaveChangesAsync();

            //    DataCache.CommentTree.AddCommentToTree(commentModel);

            //    if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave))
            //    {
            //        ContentProcessor.Instance.Process(commentModel.Content, ProcessingStage.InboundPostSave, commentModel);
            //    }

            //    // send comment reply notification to parent comment author if the comment is not a new root comment
            //    await NotificationManager.SendCommentNotification(commentModel,
            //        new Action<string>(recipient => {
            //                //get count of unread notifications
            //                int unreadNotifications = UserHelper.UnreadTotalNotificationsCount(recipient);
            //                // send SignalR realtime notification to recipient
            //                var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();
            //            hubContext.Clients.User(recipient).setNotificationsPending(unreadNotifications);
            //        })
            //    );
            //}

            #endregion

            var result = base.EvaluateRule(context);
            if (result.IsAllowed)
            {

                // flag the comment as anonymized if it was submitted to a sub which has active anonymized_mode
                var subverse = context.Subverse;
                var userCcp = context.UserData.Information.CommentPoints.Sum;

                if (userCcp <= 0)
                {
                    var userMembershipTimeSpam = Repository.CurrentDate - context.UserData.Information.RegistrationDate;
                    // if user CCP is negative and account less than 6 months old, allow only x comment submissions per 24 hours
                    if ((userMembershipTimeSpam.TotalDays < 180 || userCcp <= -50) && UserHelper.UserDailyCommentPostingQuotaForNegativeScoreUsed(context.UserName))
                    {
                        result = CreateOutcome(RuleResult.Denied, String.Format("You have reached your daily comment quota. Your current quota is {0} comment(s) per 24 hours.", Settings.DailyCommentPostingQuotaForNegativeScore.ToString()));
                    }
                }
            }

            return result;
        }

    }
    #endregion 4.x Rules - Throttle Rules

    #region 5.x Rules - Subverse

    [RuleDiscovery("Approves a comment post if user is not banned in subverse.", "approved = (user.IsBannedFromSubverse(subverse) == false)")]
    public class BannedCommentRule : BaseBannedRule
    {
        public BannedCommentRule()
            : base("Subverse Comment Ban Rule", "5.8", RuleScope.PostComment)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            return base.EvaluateRule(context);
        }
    }

    [RuleDiscovery("Approves a submission post if user is not banned in subverse.", "approved = (user.IsBannedFromSubverse(subverse) == false)")]
    public class BannedSubmissionRule : BaseBannedRule
    {
        public BannedSubmissionRule()
            : base("Subverse Submission Ban Rule", "5.7", RuleScope.PostSubmission)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            return base.EvaluateRule(context);
        }
    }

    [RuleDiscovery("Approves comment downvote if user has more CCP in subverse than what is specified by the subverse minimum for downvoting.", "approved = ((subverse.minCCP > 0 and user.subverseCCP > subverse.minCCP) or subverse.minCCP == 0)")]
    public class SubverseCCPDownVoteCommentRule : BaseSubverseMinCCPRule
    {
        public SubverseCCPDownVoteCommentRule()
            : base("Subverse Min CCP Downvoat", "5.2", RuleScope.DownVoteComment)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            return base.EvaluateRule(context);
        }
    }

    [RuleDiscovery("Approves submission downvote if user has more CCP in subverse than what is specified by the subverse minimum for downvoting.", "approved = ((subverse.minCCP > 0 and user.subverseCCP > subverse.minCCP) or subverse.minCCP == 0)")]
    public class SubverseCCPDownVoteSubmissionRule : BaseSubverseMinCCPRule
    {
        public SubverseCCPDownVoteSubmissionRule()
            : base("Subverse Min CCP Downvoat", "5.1", RuleScope.DownVoteSubmission)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            return base.EvaluateRule(context);
        }
    }

    #endregion 5.x Rules - Subverse

    #region Rule 6.X - Behavior Rules https://voat.co/v/announcements/comments/100313

    [RuleDiscovery("Approved if user who has -50 CCP or less has submitted fewer than 5 comments in a 24 hour sliding window.", "approved = (user.CCP <= -50 and TotalCommentsInPast24Hours < 5 or user.CCP > -50)")]
    public class CommentMeanieCommentRule : MinimumCCPRule
    {
        private int countThreshold = 5;

        public CommentMeanieCommentRule() : base("Comment Meanie Throttle", "6.0", -50, RuleScope.PostComment)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            if (context.UserData.Information.CommentPoints.Sum <= MinimumCommentPoints && context.UserData.TotalVotesUsedIn24Hours >= countThreshold)
            {
                return CreateOutcome(RuleResult.Denied, "User with CCP value of {0} is limited to {1} comment(s) in 24 hours.", context.PropertyBag.CCP, countThreshold);
            }

            return base.EvaluateRule(context);
        }
    }

    [RuleDiscovery("Approved if user who has -50 CCP or less has submitted fewer than 1 submission in a 24 hour sliding window.", "approved = (user.CCP <= -50 and TotalSubmissionsInPast24Hours < 1 or user.CCP > -50)")]
    public class CommentMeanieSubmissionRule : MinimumCCPRule
    {
        private int countThreshold = 1;

        public CommentMeanieSubmissionRule() : base("Comment Meanie Throttle", "6.1", -50, RuleScope.PostSubmission)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            //TODO: Need implementation of this rule
            //if (context.UserInformation.CommentPoints.Sum <= MinimumCommentPoints && context.TotalSubmissionsInPast24Hours >= countThreshold)
            //{
            //    return CreateOutcome(RuleResult.Denied, "User with CCP value of {0} is limited to {1} submission(s) in 24 hours.", context.UserInformation.CommentPoints.Sum, countThreshold);
            //}
            return base.EvaluateRule(context);
        }
    }

    #endregion Rule 6.X - Behavior Rules https://voat.co/v/announcements/comments/100313

    #region Admin / Emergency Rules

    namespace Administrative
    {
        [RuleDiscovery("Approves action if username isn't DerpyGuy", "approved = (user.Name != DerpyGuy)")]
        public class DerpyGuyRule : VoatRule
        {
            public DerpyGuyRule() : base("DerpyGuy", "88.88.89", RuleScope.Global)
            {
            }

            protected override RuleOutcome EvaluateRule(VoatRuleContext context)
            {
                if (context.UserName == "DerpyGuy")
                {
                    return CreateOutcome(RuleResult.Denied, "Your name is DerpyGuy");
                }
                return Allowed;
            }
        }

        [RuleDiscovery(false, "A dummy test rule that denies.", "approved = (false)")]
        public class DummyDenyRule : Rule<VoatRuleContext>
        {
            public DummyDenyRule() : base("DummyDeny", "1.01", RuleScope.Global)
            {
            }

            protected override RuleOutcome EvaluateRule(VoatRuleContext context)
            {
                return CreateOutcome(RuleResult.Denied, "Talk to the hand cause you've been denied.");
            }
        }
    }

    #endregion Admin / Emergency Rules
}
