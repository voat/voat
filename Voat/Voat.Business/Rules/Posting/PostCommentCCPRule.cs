using System;
using Voat.Configuration;
using Voat.Data;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Posting
{
    [RuleDiscovery("Approves a comment post if user CCP passes all checks.", "approved = (user.CCPThrottleExceeded() == false)")]
    public class PostCommentCCPRule : VoatRule
    {
        public PostCommentCCPRule()
            : base("Comment CCP Rule", "7.0", RuleScope.PostComment)
        {
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            var result = base.EvaluateRule(context);
            if (result.IsAllowed)
            {
                var subverse = context.Subverse;
                var userCcp = context.UserData.Information.CommentPoints.Sum;
                var userMembershipTimeSpan = Repository.CurrentDate.Subtract(context.UserData.Information.RegistrationDate);

                //TODO: Port UserHelper methods to Repository.UserCommentCount()

                // throttle comment posting if CCP is low, regardless of account age
                if (userCcp < 1)
                {
                    var quotaUsed = UserDailyCommentPostingQuotaForNegativeScoreUsed(context);
                    if (quotaUsed)
                    {
                        return CreateOutcome(RuleResult.Denied, String.Format("You have reached your daily comment quota. Your current quota is {0} comment(s) per 24 hours.", Settings.DailyCommentPostingQuotaForNegativeScore.ToString()));
                    }
                }

                // if user account is new, allow max X comments per hour
                if (userMembershipTimeSpan.TotalDays < 7 && userCcp < 50)
                {
                    var quotaUsed = UserHourlyCommentPostingQuotaUsed(context);
                    if (quotaUsed)
                    {
                        return CreateOutcome(RuleResult.Denied, String.Format("You have reached your hourly comment quota. Your current quota is {0} comment(s) per hour.", Settings.HourlyCommentPostingQuota.ToString()));
                    }
                }

                // if user CCP is < 10, allow only X comment submissions per 24 hours
                if (userMembershipTimeSpan.TotalDays < 7 && userCcp <= 10)
                {
                    var quotaUsed = UserDailyCommentPostingQuotaUsed(context);
                    if (quotaUsed)
                    {
                        return CreateOutcome(RuleResult.Denied, String.Format("You have reached your daily comment quota. Your current quota is {0} comment(s) per 24 hours.", Settings.DailyCommentPostingQuota.ToString()));
                    }
                }

                //if (userCcp <= 0)
                //{
                //    var userMembershipTimeSpam = Repository.CurrentDate - context.UserData.Information.RegistrationDate;
                //    // if user CCP is negative or account less than 6 months old, allow only x comment submissions per 24 hours
                //    if ((userMembershipTimeSpam.TotalDays < 180 || userCcp <= -50) && UserHelper.UserDailyCommentPostingQuotaForNegativeScoreUsed(context.UserName))
                //    {
                //        result = CreateOutcome(RuleResult.Denied, String.Format("You have reached your daily comment quota. Your current quota is {0} comment(s) per 24 hours.", Settings.DailyCommentPostingQuotaForNegativeScore.ToString()));
                //    }
                //}
            }

            return result;


            #region Original Logic

            //// flag the comment as anonymized if it was submitted to a sub which has active anonymized_mode
            //var submission = DataCache.Submission.Retrieve(commentModel.SubmissionID.Value);
            //var subverse = DataCache.Subverse.Retrieve(submission.Subverse);
            //var userCcp = Karma.CommentKarma(User.Identity.Name);
            //commentModel.IsAnonymized = submission.IsAnonymized || subverse.IsAnonymized;

            //// if user CCP is negative or account less than 6 months old, allow only x comment submissions per 24 hours
            //var userRegistrationDate = UserHelper.GetUserRegistrationDateTime(User.Identity.Name);
            //TimeSpan userMembershipTimeSpan = Repository.CurrentDate - userRegistrationDate;

            //// throttle comment posting if CCP is low, regardless of account age
            //if (userCcp < 1)
            //{
            //    var quotaUsed = UserHelper.UserDailyCommentPostingQuotaForNegativeScoreUsed(User.Identity.Name);
            //    if (quotaUsed)
            //    {
            //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You have reached your daily comment quota. Your current quota is " + Settings.DailyCommentPostingQuotaForNegativeScore.ToString() + " comment(s) per 24 hours.");
            //    }
            //}

            //// if user account is new, allow max X comments per hour
            //if (userMembershipTimeSpan.TotalDays < 7 && userCcp < 50)
            //{
            //    var quotaUsed = UserHelper.UserHourlyCommentPostingQuotaUsed(User.Identity.Name);
            //    if (quotaUsed)
            //    {
            //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You have reached your hourly comment quota. Your current quota is " + Settings.HourlyCommentPostingQuota.ToString() + " comment(s) per hour.");
            //    }
            //}

            //// if user CCP is < 10, allow only X comment submissions per 24 hours
            //if (userMembershipTimeSpan.TotalDays < 7 && userCcp <= 10)
            //{
            //    var quotaUsed = UserHelper.UserDailyCommentPostingQuotaUsed(User.Identity.Name);
            //    if (quotaUsed)
            //    {
            //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You have reached your daily comment quota. Your current quota is " + Settings.DailyCommentPostingQuota.ToString() + " comment(s) per 24 hours.");
            //    }
            //}

            //PORTED
            //// check for copypasta
            //// TODO: use Levenshtein distance algo or similar for better results
            //var copyPasta = UserHelper.SimilarCommentSubmittedRecently(User.Identity.Name, commentModel.Content);
            //if (copyPasta)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You have recently submitted a similar comment. Please try to not use copy/paste so often.");
            //}

            //// check if author is banned, don't save the comment or send notifications if true
            //if (!UserHelper.IsUserGloballyBanned(User.Identity.Name) && !UserHelper.IsUserBannedFromSubverse(User.Identity.Name, submission.Subverse))
            //{
            //PORTED
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
            //    var formattedComment = Voat.Utilities.Formatting.FormatMessage(commentModel.Content);
            //    commentModel.FormattedContent = formattedComment;

            //    _db.Comments.Add(commentModel);

            //    await _db.SaveChangesAsync();

            //    DataCache.CommentTree.AddCommentToTree(commentModel);

            //    if (ContentProcessor.Instance.HasStage(ProcessingStage.InboundPostSave))
            //    {
            //        ContentProcessor.Instance.Process(commentModel.Content, ProcessingStage.InboundPostSave, commentModel);
            //    }

            //    // send comment reply notification to parent comment author if the comment is not a new root comment
            //    await NotificationManager.SendCommentNotification(commentModel);
            //}
            //if (Request.IsAjaxRequest())
            //{
            //    var comment = commentModel;

            //    ViewBag.CommentId = comment.ID; //why?
            //    ViewBag.rootComment = comment.ParentID == null; //why?

            //    if (submission.IsAnonymized || subverse.IsAnonymized)
            //    {
            //        comment.UserName = comment.ID.ToString(CultureInfo.InvariantCulture);
            //    }

            //    var model = new CommentBucketViewModel(comment);

            //    return PartialView("~/Views/Shared/Submissions/_SubmissionComment.cshtml", model);
            //    //return new HttpStatusCodeResult(HttpStatusCode.OK);
            //}
            //if (Request.UrlReferrer != null)
            //{
            //    var url = Request.UrlReferrer.AbsolutePath;
            //    return Redirect(url);
            //}

            #endregion Original Logic
        }
    }
}
