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

    public abstract class BaseQueryCommentSegment : Query<CommentSegment>
    {
        protected int _submissionID;
        protected SearchOptions _options;
        protected IEnumerable<usp_CommentTree_Result> fullTree;

        protected abstract IQueryable<usp_CommentTree_Result> FilterSegment(IQueryable<usp_CommentTree_Result> commentTree);
        protected abstract IQueryable<usp_CommentTree_Result> TakeSegment(IQueryable<usp_CommentTree_Result> commentTree);
        protected CommentSegment GetSegment(bool addChildren)
        {
            QueryCommentTree q = new QueryCommentTree(_submissionID);
            IEnumerable<CommentVoteTracker> commentVotes = null;
            IEnumerable<CommentSaveTracker> commentSaves = null;


            if (!String.IsNullOrEmpty(UserName))
            {
                var p = new QueryUserData(UserName).Execute();
                var preference = p.Preferences;
                commentVotes = new QueryUserCommentVotesForSubmission(_submissionID, CachePolicy.None).Execute();
                commentSaves = new QueryUserSavedCommentsForSubmission(_submissionID).Execute();
            }
            //TODO: Set with preferences
            _options.Count = 4;
            int nestLevel = 3;
            //TODO: set IsCollapsed flag in output on every comment below this threshold
            int collapseThreshold = -4;

            fullTree = (q.Execute()).Values;
            switch (_options.Sort)
            {
                case SortAlgorithm.Intensity:
                    //really rough alg for intensity
                    fullTree = fullTree.OrderByDescending(x => (x.UpCount * x.DownCount) / Math.Max(1, Math.Abs((x.UpCount - x.DownCount))));
                    break;
                case SortAlgorithm.New:
                    fullTree = fullTree.OrderByDescending(x => x.CreationDate);
                    break;
                case SortAlgorithm.Bottom:
                    fullTree = fullTree.OrderByDescending(x => x.DownCount).ThenByDescending(x => x.CreationDate);
                    break;
                default:
                    //top
                    fullTree = fullTree.OrderByDescending(x => x.UpCount - x.DownCount).ThenByDescending(x => x.CreationDate);
                    break;
            }

            var queryTree = fullTree.AsQueryable();
            queryTree = FilterSegment(queryTree);
            var queryableTree = TakeSegment(queryTree);

            List<NestedComment> comments = new List<NestedComment>();

            var processor = new Action<NestedComment>(n => {
                if (!String.IsNullOrEmpty(UserName))
                {
                    n.IsSubmitter = n.UserName == UserName;
                    n.Vote = 0;
                    if (commentVotes != null)
                    {
                        var vote = commentVotes.FirstOrDefault(x => x.CommentID == n.ID);
                        if (vote != null)
                        {
                            n.Vote = vote.VoteStatus;
                        }
                    }
                    n.IsSaved = false;
                    if (commentSaves != null)
                    {
                        n.IsSaved = commentSaves.Any(x => x.CommentID == n.ID);
                    }
                }
            });

            foreach (var c in queryableTree)
            {
                var n = c.Map();

                processor(n);
                n.IsCollapsed = (n.Sum <= collapseThreshold);
                if (addChildren)
                {
                    AddComments(fullTree, n, _options.Count, nestLevel, 1, collapseThreshold, processor, _options.Sort);
                }
                comments.Add(n);
            }

            var segment = new Domain.Models.CommentSegment();
            segment.Sort = _options.Sort;
            segment.StartingIndex = 0;
            segment.TotalCount = queryTree.Count();
            if (comments.Any())
            {
                segment.Comments = comments;
                int segmentCount = segment.Comments.Count();
            }

            return segment;
        }
        //recursive addition of child comments
        private void AddComments(IEnumerable<usp_CommentTree_Result> queryTree, NestedComment parent, int count, int nestLevel, int currentNestLevel, int collapseThreshold, Action<NestedComment> processor, SortAlgorithm sort)
        {
            if (currentNestLevel < nestLevel)
            {
                var children = queryTree.Where(x => x.ParentID == parent.ID && !(x.IsDeleted && x.ChildCount == 0)).ToList();
                if (children.Any())
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (children.Count > i)
                        {
                            var c = children[i].Map();

                            c.IsCollapsed = (c.Sum <= collapseThreshold);
                            processor(c);
                            var nextNestLevel = currentNestLevel + 1;
                            AddComments(queryTree, c, count, nestLevel, nextNestLevel, collapseThreshold, processor, sort);
                            parent.AddChildComment(c);
                            parent.Children.TotalCount = children.Count;
                            parent.Children.Sort = sort;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Allows segments of comments to be retrieved out of the comment tree for a submission.
    /// </summary>
    //Currently this class does not cache a segment, but it uses the cached comment tree to format the output per request.
    public class QueryCommentContext : BaseQueryCommentSegment
    {
        protected int? _context;
        protected int _commentID;

        public QueryCommentContext(int submissionID, int commentID, int? context = null, SearchOptions options = null)
        {
            _submissionID = submissionID;
            _commentID = commentID;
            _options = options ?? SearchOptions.Default;
            if (context.HasValue)
            {
                //ensure context isn't negative and less than 20
                _context = Math.Min(20, Math.Max(0, context.Value));
            }
        }

        public override CommentSegment Execute()
        {
            bool addChildren = !_context.HasValue; //logic here is that if context is negative user is viewing permalink
            var segment = base.GetSegment(addChildren);

            if (_context.HasValue)
            {
                //build the context upward
                var primary = fullTree.Where(x => x.ID == _commentID).FirstOrDefault();
                var parent = (primary.ParentID != null ? fullTree.Where(x => x.ID == primary.ParentID).FirstOrDefault() : null);

                int previousChildCount = primary.ChildCount.Value;
                int currentContext = 0;
                var childSegment = segment;
                while (parent != null && (currentContext < _context || _context == 0))
                {
                    var parentNestedComment = parent.Map();
                    parentNestedComment.Children = childSegment;
                    childSegment = new CommentSegment(parentNestedComment);
                    childSegment.Sort = _options.Sort;
                    parent = (parent.ParentID != null ? fullTree.Where(x => x.ID == parent.ParentID).FirstOrDefault() : null);
                    //HACK: So we have a bit of a bug here. If we provide valid counts here the UI will attempt to offer loading capabilities
                    //if this happens the ajax loading will most likely load duplicate comments as sorting doesn't appy to history, if dups get loaded
                    //the UI goes all kinds of cray cray. So, until I can think of a clean solution, we will load context history as having no siblings
                    //childSegment.TotalCount = parent == null ? 0 : parent.ChildCount.Value;
                    childSegment.TotalCount = parent == null ? 0 : 1;
                    currentContext++;
                }
                segment = childSegment;
            }
            
            return segment;
        }
        protected override IQueryable<usp_CommentTree_Result> FilterSegment(IQueryable<usp_CommentTree_Result> commentTree)
        {
            return commentTree.Where(x => x.ID == _commentID);
        }
        protected override IQueryable<usp_CommentTree_Result> TakeSegment(IQueryable<usp_CommentTree_Result> commentTree)
        {
            return commentTree;
        }
    }
}
