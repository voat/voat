using System;
using Voat.RulesEngine;

namespace Voat.Rules
{

    #region 1.x Rules (Admin/Site Functionality Rules)

    //[RuleLoadable(false, "Voat is in Read-Only mode.", "approved = (false)")]
    //public class ReadOnlyRule : Rule {

    //    public ReadOnlyRule() : base("ReadOnly", "1.0", RuleScope.Global) { }

    //    public override RuleOutcome Evaluate() {

    //        return CreateOutcome(RuleResult.Denied, "All website actions are disabled. Website is in READ ONLY mode.");

    //    }
    //}

    [RuleLoadable(false, "Voat is in Read-Only mode.", "approved = (false)")]
    public class ReadOnlyRule : Rule
    {

        public ReadOnlyRule() : base("ReadOnly", "1.0", RuleScope.Global) { }

        public override RuleOutcome Evaluate()
        {

            return CreateOutcome(RuleResult.Denied, "All website actions are disabled. Website is in READ ONLY mode.");

        }
    }


    [RuleLoadable(false, "All voting is disabled.", "approved = (false)")]
    public class DisableVotingRule : Rule
    {

        public DisableVotingRule() : base("DisableVoting", "1.1", RuleScope.Vote) { }

        public override RuleOutcome Evaluate()
        {

            return CreateOutcome(RuleResult.Denied, "All voting is disabled. Website is in READ ONLY mode.");

        }
    }

    [RuleLoadable(false, "All posting is disabled.", "approved = (false)")]
    public class DisablePostingRule : Rule
    {

        public DisablePostingRule() : base("DisablePosting", "1.2", RuleScope.Post) { }

        public override RuleOutcome Evaluate()
        {

            return CreateOutcome(RuleResult.Denied, "All posting is disabled. Website is in READ ONLY mode.");

        }
    }
    #endregion

    #region 2.x Basic Voat Rules https://github.com/voat/voat/issues/400

    [RuleLoadable(true,
          "Approved if user has more than 20 CCP.",
          "approved = (user.CCP > 20)")]
    public class UpVoteSubmissionRule : BaseCCPVote
    {

        public UpVoteSubmissionRule()
            : base("UpVote Submission", "2.1", 20, RuleScope.UpVoteSubmission)
        {
        }
    }


    [RuleLoadable(true,
        "Approved if user has more than 20 CCP.",
        "approved = (user.CCP > 20)")]
    public class UpVoteCommentRule : BaseCCPVote
    {

        public UpVoteCommentRule()
            : base("UpVote Comment", "2.2", 20, RuleScope.UpVoteComment)
        {

        }
    }

    [RuleLoadable(true,
      "Approved if user has 100 CCP or higher.",
      "approved = (user.CCP >= 100)")]
    public class DownVoteRule : BaseCCPVote
    {

        public DownVoteRule()
            : base("Downvote", "2.3", 100, RuleScope.DownVote)
        {
        }
    }


    #endregion

    #region 4.x Rules - Throttle Rules

    [RuleLoadable(true,
        "Approves action if quota not exceeded for votes in a 24 sliding window.",
        "approved = (user.TotalVotesInPast24Hours < max(10, user.CCP / 2))")]
    public class VoteThrottleRule : BaseVoatRule
    {

        public VoteThrottleRule()
            : base("Vote Throttle", "4.0", RuleScope.Vote)
        {
        }

        public override RuleOutcome Evaluate()
        {
            //using (DataGateway db = new DataGateway())
            //{
            //    if (Context.PropertyBag.TotalVotesUsedInPast24Hours == null)
            //    {
            //        Context.PropertyBag.TotalVotesUsedInPast24Hours = db.UserVotingBehavior(Context.UserName, ContentType.Comment | ContentType.Submission, TimeSpan.FromHours(24)).Total;
            //    }
            //    if (Context.PropertyBag.CCP == null)
            //    {
            //        Context.PropertyBag.CCP = db.UserContributionPoints(Context.UserName, ContentType.Comment).Sum;
            //    }
            //}

            int votesPer24HourWindow = Math.Max(10, (Context.PropertyBag.CCP / 2));

            if (Context.PropertyBag.TotalVotesUsedInPast24Hours > votesPer24HourWindow)
            {
                return CreateOutcome(RuleResult.Denied, "User has exceeded vote throttle limit based on CCP. Available votes per 24 hours: {0}", votesPer24HourWindow);
            }
            return Allowed;
        }
    }

    [RuleLoadable(true,
       "Approved if a user who has less than 20 CCP hasn't voted more than 10 times in the last 24 hours.",
       "approved = !(user.CCP <= 20 and user.TotalVotesInPast24Hours >= 10)")]
    public class UpvoteLimitRule : BaseVoatRule
    {

        public UpvoteLimitRule()
            : base("New User Upvote Limit", "4.1", RuleScope.UpVote)
        {

        }

        public override RuleOutcome Evaluate()
        {

            //using (DataGateway db = new DataGateway())
            //{
            //    if (Context.PropertyBag.CCP == null)
            //    {
            //        Context.PropertyBag.CCP = db.UserContributionPoints(Context.UserName, ContentType.Comment).Sum;
            //    }
            //    if (Context.PropertyBag.TotalVotesUsedInPast24Hours == null)
            //    {
            //        Context.PropertyBag.TotalVotesUsedInPast24Hours = db.UserVotingBehavior(Context.UserName, ContentType.Comment | ContentType.Submission, TimeSpan.FromHours(24)).Total;
            //    }
            //}

            if (Context.PropertyBag.CCP <= 20 && Context.PropertyBag.TotalVotesUsedInPast24Hours >= 10)
            {
                return CreateOutcome(RuleResult.Denied, "User has exceeded votes allowed per CCP and/or time");
            }

            return Allowed;
        }

    }
    #endregion

    #region 5.x Rules - Subverse

    [RuleLoadable(true,
      "Approves submission downvote if user has more CCP in subverse than what is specified by the subverse minimum for downvoting.",
      "approved = ((subverse.minCCP > 0 and user.subverseCCP > subverse.minCCP) or subverse.minCCP == 0)")]
    public class SubverseCCPDownVoteSubmissionRule : BaseSubverseMinCCPRule
    {

        public SubverseCCPDownVoteSubmissionRule()
            : base("Subverse Min CCP Downvoat", "5.1", RuleScope.DownVoteSubmission)
        {
        }

        public override RuleOutcome Evaluate()
        {

            int? submissionID = Context.SubmissionID;

            if (String.IsNullOrEmpty(Context.SubverseName) && (submissionID.HasValue && submissionID > 0))
            {
                //using (DataGateway db = new DataGateway())
                //{
                //    Context.SubverseName = db.SubverseForSubmission(submissionID.Value);
                //}
            }

            return base.Evaluate();
        }

    }

    [RuleLoadable(true,
        "Approves comment downvote if user has more CCP in subverse than what is specified by the subverse minimum for downvoting.",
        "approved = ((subverse.minCCP > 0 and user.subverseCCP > subverse.minCCP) or subverse.minCCP == 0)")]
    public class SubverseCCPDownVoteCommentRule : BaseSubverseMinCCPRule
    {

        public SubverseCCPDownVoteCommentRule()
            : base("Subverse Min CCP Downvoat", "5.2", RuleScope.DownVoteComment)
        {
        }

        public override RuleOutcome Evaluate()
        {

            int? commentID = Context.CommentID;

            if (String.IsNullOrEmpty(Context.SubverseName) && (commentID.HasValue && commentID.Value > 0))
            {
                //using (DataGateway db = new DataGateway())
                //{
                //    Context.SubverseName = db.SubverseForComment(commentID.Value);
                //}
            }

            return base.Evaluate();
        }

    }
    #endregion

    #region Rule 6.X - Behavior Rules https://voat.co/v/announcements/comments/100313
    [RuleLoadable(true,
          "Approved if user who has -50 CCP or less has submitted fewer than 5 comments in a 24 hour sliding window.",
          "approved = (user.CCP <= -50 and TotalCommentsInPast24Hours < 5 or user.CCP > -50)")]
    public class CommentMeanieCommentRule : MinCCPRule
    {

        private int countThreshold = 5;

        public CommentMeanieCommentRule() : base("Comment Meanie Throttle", "6.0", -50, RuleScope.PostComment) { }

        public override RuleOutcome Evaluate()
        {

            //if (Context.PropertyBag.CCP == null)
            //{
            //    Context.PropertyBag.CCP = Karma.CommentKarma(Context.UserName);
            //}

            //if (Context.PropertyBag.TotalCommentsInPast24Hours == null)
            //{
            //    using (DataGateway db = new DataGateway())
            //    {
            //        Context.PropertyBag.TotalCommentsInPast24Hours = db.UserCommentCount(Context.UserName, TimeSpan.FromHours(24));
            //    }
            //}

            if (Context.PropertyBag.CCP <= MinCCP && Context.PropertyBag.TotalCommentsInPast24Hours >= countThreshold)
            {
                return CreateOutcome(RuleResult.Denied, "User with CCP value of {0} is limited to {1} comment(s) in 24 hours.", Context.PropertyBag.CCP, countThreshold);
            }

            return Allowed;
        }
    }

    [RuleLoadable(true,
         "Approved if user who has -50 CCP or less has submitted fewer than 1 submission in a 24 hour sliding window.",
         "approved = (user.CCP <= -50 and TotalSubmissionsInPast24Hours < 1 or user.CCP > -50)")]
    public class CommentMeanieSubmissionRule : MinCCPRule
    {

        private int countThreshold = 1;

        public CommentMeanieSubmissionRule() : base("Comment Meanie Throttle", "6.1", -50, RuleScope.PostSubmission) { }

        public override RuleOutcome Evaluate()
        {

            if (Context.PropertyBag.CCP == null)
            {
                //Context.PropertyBag.CCP = Karma.CommentKarma(Context.UserName);
            }

            if (Context.PropertyBag.TotalSubmissionsInPast24Hours == null)
            {
                //using (DataGateway db = new DataGateway())
                //{
                //    Context.PropertyBag.TotalSubmissionsInPast24Hours = db.UserSubmissionCount(Context.UserName, TimeSpan.FromHours(24));
                //}
            }

            if (Context.PropertyBag.CCP <= MinCCP && Context.PropertyBag.TotalSubmissionsInPast24Hours >= countThreshold)
            {
                return CreateOutcome(RuleResult.Denied, "User with CCP value of {0} is limited to {1} submission(s) in 24 hours.", Context.PropertyBag.CCP, countThreshold);
            }

            return Allowed;
        }
    }

    #endregion





    #region Admin / Emergency Rules

    namespace Administrative
    {






        #endregion

        [RuleLoadable(true, "A dummy test rule that denies.", "approved = (false)")]
        public class DummyDenyRule : Rule
        {

            public DummyDenyRule() : base("DummyDeny", "1.01", RuleScope.Edit) { }

            public override RuleOutcome Evaluate()
            {

                return CreateOutcome(RuleResult.Denied, "Talk to the hand cause you've been denied.");

            }
        }

        [RuleLoadable(true, "Approves action if username isn't DerpyGuy", "approved = (user.Name != DerpyGuy)")]
        public class DerpyGuyRule : BaseVoatRule
        {

            public DerpyGuyRule() : base("DerpyGuy", "88.88.89", RuleScope.Global) { }

            public override RuleOutcome Evaluate()
            {

                if (Context.UserName == "DerpyGuy")
                {
                    return CreateOutcome(RuleResult.Denied, "Your name is DerpyGuy");
                }
                return Allowed;
            }

        }

    }
}