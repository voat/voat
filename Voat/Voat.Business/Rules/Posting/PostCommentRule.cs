using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Configuration;
using Voat.Data;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Posting
{

    public class PostCommentRule : BaseSubverseBanRule
    {

        public PostCommentRule()
            : base("Comment Post Rule", "7.0", RuleScope.PostComment)
        {

        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {

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
        }

    }
}
