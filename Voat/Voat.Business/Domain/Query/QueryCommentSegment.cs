using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    /// <summary>
    /// Allows segments of comments to be retrieved out of the comment tree for a submission.
    /// </summary>
    //Currently this class does not cache a segment, but it uses the cached comment tree to format the output per request.
    public class QueryCommentSegment : Query<CommentSegment>
    {
        protected int? _index;
        protected SearchOptions _options;
        protected int? _parentID;
        protected int _submissionID;

        public QueryCommentSegment(int submissionID, int? parentID = null, int? index = null, SearchOptions options = null)
        {
            _submissionID = submissionID;
            _parentID = parentID;
            _index = index;
            _options = options;
        }

        public override async Task<CommentSegment> Execute()
        {
            int startingIndex = _index == null ? 0 : _index.Value;

            QueryCommentTree q = new QueryCommentTree(_submissionID);

            var p = await new QueryUserData(UserName).Execute();

            UserPreference preference = p.Preferences;
            //This is for testing
            //options.Count = 3;

            var fullTree = (await q.Execute()).Values;
            var queryTree = fullTree.AsQueryable();
            queryTree = queryTree.Where(x => x.ParentID == _parentID);

            switch (_options.Sort)
            {
                case SortAlgorithm.New:
                    queryTree = queryTree.OrderByDescending(x => x.CreationDate);
                    break;

                case SortAlgorithm.Top:
                    queryTree = queryTree.OrderByDescending(x => x.UpCount - x.DownCount);
                    break;
            }
            var queryableTree = queryTree.Skip(startingIndex).Take(_options.Count);

            var commentVotes = await new QueryUserCommentVotesForSubmission(_submissionID).Execute();

            List<NestedComment> comments = new List<NestedComment>();

            int nestLevel = 4;
            int collapseThreshold = -4;

            var processor = new Action<NestedComment>(n => {
                if (!String.IsNullOrEmpty(UserName))
                {
                    n.Vote = 0;
                    if (commentVotes != null)
                    {
                        var vote = commentVotes.FirstOrDefault(x => x.CommentID == n.ID);
                        if (vote != null)
                        {
                            n.Vote = vote.VoteStatus;
                        }
                    }
                }
            });

            foreach (var c in queryableTree)
            {
                var n = c.Map();

                processor(n);
                n.IsCollapsed = (n.Total <= collapseThreshold);

                AddComments(fullTree, n, _options.Count, nestLevel, 0, collapseThreshold, processor);
                comments.Add(n);
            }

            var segment = new Domain.Models.CommentSegment();
            segment.StartingIndex = _index ?? 0;
            segment.TotalCount = queryTree.Count();
            if (comments.Any())
            {
                segment.Comments = comments;
                int segmentCount = segment.Comments.Count();
            }

            return segment;
        }

        //recursive addition of child comments
        private void AddComments(IEnumerable<usp_CommentTree_Result> queryTree, NestedComment parent, int count, int nestLevel, int currentNestLevel, int collapseThreshold, Action<NestedComment> processor)
        {
            if (currentNestLevel <= nestLevel)
            {
                var children = queryTree.Where(x => x.ParentID == parent.ID).ToList();
                if (children.Any())
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (children.Count > i)
                        {
                            var c = children[i].Map();

                            c.IsCollapsed = (c.Total <= collapseThreshold);
                            processor(c);

                            AddComments(queryTree, c, count, nestLevel, currentNestLevel++, collapseThreshold, processor);
                            parent.AddChildComment(c);
                            parent.Children.TotalCount = parent.ChildCount.Value;
                        }
                    }
                }
            }
        }
    }
}
