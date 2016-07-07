using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    /// <summary>
    /// Allows segments of comments to be retrieved out of the comment tree for a submission.
    /// </summary>
    //Currently this class does not cache a segment, but it uses the cached comment tree to format the output per request.
    public class QueryCommentSegment : BaseQueryCommentSegment
    {
        protected int? _index;
        protected int? _parentID;

        public QueryCommentSegment(int submissionID, int? parentID = null, int? index = null, SearchOptions options = null)
        {
            _submissionID = submissionID;
            _parentID = parentID;
            _index = index;
            _options = options ?? SearchOptions.Default;
        }

        protected override IQueryable<usp_CommentTree_Result> FilterSegment(IQueryable<usp_CommentTree_Result> commentTree)
        {
            var filtered = commentTree.Where(x => x.ParentID == _parentID && !(x.IsDeleted && x.ChildCount == 0));
            //filtered = filtered.Skip(_index.HasValue ? _index.Value : 0).Take(_options.Count);
            return filtered;
        }
        protected override IQueryable<usp_CommentTree_Result> TakeSegment(IQueryable<usp_CommentTree_Result> commentTree)
        {
            return commentTree.Skip(_index.HasValue ? _index.Value : 0).Take(_options.Count);
        }
        public override CommentSegment Execute()
        {
            var segment = base.GetSegment(true);
            segment.StartingIndex = _index.HasValue ? _index.Value : 0;
            return segment;
        }

        //public override CommentSegment Execute()
        //{
        //    int startingIndex = _index == null ? 0 : _index.Value;

        //    QueryCommentTree q = new QueryCommentTree(_submissionID);
        //    IEnumerable<CommentVoteTracker> commentVotes = null;
        //    IEnumerable<CommentSaveTracker> commentSaves = null;


        //    if (!String.IsNullOrEmpty(UserName))
        //    {
        //        var p = new QueryUserData(UserName).Execute();
        //        var preference = p.Preferences;
        //        commentVotes = new QueryUserCommentVotesForSubmission(_submissionID, CachePolicy.None).Execute();
        //        commentSaves = new QueryUserSavedCommentsForSubmission(_submissionID).Execute();
        //    }
        //    //TODO: Set with preferences
        //    _options.Count = 4;
        //    int nestLevel = 3;
        //    //TODO: set IsCollapsed flag in output on every comment below this threshold
        //    int collapseThreshold = -4;

        //    IEnumerable<usp_CommentTree_Result> fullTree = (q.Execute()).Values;
        //    switch (_options.Sort)
        //    {
        //        case SortAlgorithm.Intensity:
        //            //really rough alg for intensity
        //            fullTree = fullTree.OrderByDescending(x => (x.UpCount * x.DownCount) / Math.Max(1, Math.Abs((x.UpCount - x.DownCount))));
        //            break;
        //        case SortAlgorithm.New:
        //            fullTree = fullTree.OrderByDescending(x => x.CreationDate);
        //            break;
        //        case SortAlgorithm.Bottom:
        //            fullTree = fullTree.OrderByDescending(x => x.DownCount).ThenByDescending(x => x.CreationDate);
        //            break;
        //        default:
        //            //top
        //            fullTree = fullTree.OrderByDescending(x => x.UpCount - x.DownCount).ThenByDescending(x => x.CreationDate);
        //            break;
        //    }

        //    var queryTree = fullTree.AsQueryable();
        //    queryTree = queryTree.Where(x => x.ParentID == _parentID && !(x.IsDeleted && x.ChildCount == 0));
        //    var queryableTree = queryTree.Skip(startingIndex).Take(_options.Count);

        //    List<NestedComment> comments = new List<NestedComment>();

        //    var processor = new Action<NestedComment>(n => {
        //        if (!String.IsNullOrEmpty(UserName))
        //        {
        //            n.IsSubmitter = n.UserName == UserName;
        //            n.Vote = 0;
        //            if (commentVotes != null)
        //            {
        //                var vote = commentVotes.FirstOrDefault(x => x.CommentID == n.ID);
        //                if (vote != null)
        //                {
        //                    n.Vote = vote.VoteStatus;
        //                }
        //            }
        //            n.IsSaved = false;
        //            if (commentSaves != null)
        //            {
        //                n.IsSaved = commentSaves.Any(x => x.CommentID == n.ID);
        //            }
        //        }
        //    });

        //    foreach (var c in queryableTree)
        //    {
        //        var n = c.Map();

        //        processor(n);
        //        n.IsCollapsed = (n.Sum <= collapseThreshold);

        //        AddComments(fullTree, n, _options.Count, nestLevel, 1, collapseThreshold, processor, _options.Sort);
        //        comments.Add(n);
        //    }

        //    var segment = new Domain.Models.CommentSegment();
        //    segment.Sort = _options.Sort;
        //    segment.StartingIndex = _index ?? 0;
        //    segment.TotalCount = queryTree.Count();
        //    if (comments.Any())
        //    {
        //        segment.Comments = comments;
        //        int segmentCount = segment.Comments.Count();
        //    }

        //    return segment;
        //}

        ////recursive addition of child comments
        //private void AddComments(IEnumerable<usp_CommentTree_Result> queryTree, NestedComment parent, int count, int nestLevel, int currentNestLevel, int collapseThreshold, Action<NestedComment> processor, SortAlgorithm sort)
        //{
        //    if (currentNestLevel < nestLevel)
        //    {
        //        var children = queryTree.Where(x => x.ParentID == parent.ID && !(x.IsDeleted && x.ChildCount == 0)).ToList();
        //        if (children.Any())
        //        {
        //            for (int i = 0; i < count; i++)
        //            {
        //                if (children.Count > i)
        //                {
        //                    var c = children[i].Map();

        //                    c.IsCollapsed = (c.Sum <= collapseThreshold);
        //                    processor(c);
        //                    var nextNestLevel = currentNestLevel + 1;
        //                    AddComments(queryTree, c, count, nestLevel, nextNestLevel, collapseThreshold, processor, sort);
        //                    parent.AddChildComment(c);
        //                    parent.Children.TotalCount = parent.ChildCount;
        //                    parent.Children.Sort = sort;
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
