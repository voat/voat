using System;
using Microsoft.EntityFrameworkCore;

namespace Voat.Data.Models
{
    public abstract partial class VoatEntityContext : DbContext
    {
        public virtual DbSet<ApiClient> ApiClient { get; set; }
        public virtual DbSet<ApiLog> ApiLog { get; set; }
        public virtual DbSet<ApiThrottlePolicy> ApiThrottlePolicy { get; set; }
        public virtual DbSet<ApiCorsPolicy> ApiCorsPolicy { get; set; }
        public virtual DbSet<ApiPermissionPolicy> ApiPermissionPolicy { get; set; }
        public virtual DbSet<Badge> Badge { get; set; }
        public virtual DbSet<BannedDomain> BannedDomain { get; set; }
        public virtual DbSet<BannedUser> BannedUser { get; set; }
        public virtual DbSet<Comment> Comment { get; set; }
        public virtual DbSet<CommentRemovalLog> CommentRemovalLog { get; set; }
        public virtual DbSet<CommentSaveTracker> CommentSaveTracker { get; set; }
        public virtual DbSet<CommentVoteTracker> CommentVoteTracker { get; set; }
        public virtual DbSet<DefaultSubverse> DefaultSubverse { get; set; }
        public virtual DbSet<ModeratorInvitation> ModeratorInvitation { get; set; }
        public virtual DbSet<SessionTracker> SessionTracker { get; set; }
        public virtual DbSet<StickiedSubmission> StickiedSubmission { get; set; }
        public virtual DbSet<Submission> Submission { get; set; }
        public virtual DbSet<SubmissionRemovalLog> SubmissionRemovalLog { get; set; }
        public virtual DbSet<SubmissionSaveTracker> SubmissionSaveTracker { get; set; }
        public virtual DbSet<SubmissionVoteTracker> SubmissionVoteTracker { get; set; }
        public virtual DbSet<Subverse> Subverse { get; set; }
        public virtual DbSet<SubverseBan> SubverseBan { get; set; }
        public virtual DbSet<SubverseFlair> SubverseFlair { get; set; }
        public virtual DbSet<SubverseModerator> SubverseModerator { get; set; }
        public virtual DbSet<UserBadge> UserBadge { get; set; }
        public virtual DbSet<UserPreference> UserPreference { get; set; }
        public virtual DbSet<ViewStatistic> ViewStatistic { get; set; }
        public virtual DbSet<EventLog> EventLog { get; set; }
        public virtual DbSet<UserBlockedUser> UserBlockedUser { get; set; }
        public virtual DbSet<Ad> Ad { get; set; }
        public virtual DbSet<Message> Message { get; set; }
        public virtual DbSet<Filter> Filter { get; set; }
        public virtual DbSet<RuleReport> RuleReport { get; set; }
        public virtual DbSet<RuleSet> RuleSet { get; set; }
        public virtual DbSet<UserContribution> UserContribution { get; set; }
        public virtual DbSet<SubverseSet> SubverseSet { get; set; }
        public virtual DbSet<SubverseSetList> SubverseSetList { get; set; }
        public virtual DbSet<SubverseSetSubscription> SubverseSetSubscription { get; set; }
        public virtual DbSet<Featured> Featured { get; set; }

        //Votes
        public virtual DbSet<Vote> Vote { get; set; }
        public virtual DbSet<VoteOption> VoteOption { get; set; }
        public virtual DbSet<VoteRestriction> VoteRestriction { get; set; }
        public virtual DbSet<VoteOutcome> VoteOutcome { get; set; }
        public virtual DbSet<VoteTracker> VoteTracker { get; set; }


        //CORE_PORT: Code no worky. Future people problems.
        //public virtual ObjectResult<usp_CommentTree_Result> usp_CommentTree(Nullable<int> submissionID, Nullable<int> depth, Nullable<int> parentID)
        //{
        //    var submissionIDParameter = submissionID.HasValue ?
        //        new ObjectParameter("SubmissionID", submissionID) :
        //        new ObjectParameter("SubmissionID", typeof(int));

        //    var depthParameter = depth.HasValue ?
        //        new ObjectParameter("Depth", depth) :
        //        new ObjectParameter("Depth", typeof(int));

        //    var parentIDParameter = parentID.HasValue ?
        //        new ObjectParameter("ParentID", parentID) :
        //        new ObjectParameter("ParentID", typeof(int));

        //    var result = ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<usp_CommentTree_Result>("usp_CommentTree", submissionIDParameter, depthParameter, parentIDParameter);
        //    return result;
        //}

    }
}
