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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Models;

namespace Voat.Domain.Query.Base
{
    public abstract class BaseQueryCommentSegment : Query<CommentSegment>
    {
        protected int _submissionID;
        protected CommentSortAlgorithm _sort;

        //protected SearchOptions _options;
        protected IEnumerable<usp_CommentTree_Result> fullTree;

        protected int _collapseThreshold = -4;
        protected int _count = 4;
        protected IEnumerable<VotedValue> _commentVotes = null;
        protected IEnumerable<CommentSaveTracker> _commentSaves = null;
        protected IEnumerable<BlockedItem> _userBlocks = null;

        protected abstract IQueryable<usp_CommentTree_Result> FilterSegment(IQueryable<usp_CommentTree_Result> commentTree);

        protected abstract IQueryable<usp_CommentTree_Result> TakeSegment(IQueryable<usp_CommentTree_Result> commentTree);

        private string _submitterName;

        protected string SubmitterName
        {
            get
            {
                if (String.IsNullOrEmpty(_submitterName))
                {
                    using (var repo = new Repository(User))
                    {
                        _submitterName = repo.GetSubmissionOwnerName(_submissionID);
                    }
                }
                return _submitterName;
            }
        }

        protected async Task<CommentSegment> GetSegment(bool addChildren)
        {
            QueryCommentTree q = new QueryCommentTree(_submissionID);
            if (!String.IsNullOrEmpty(UserName))
            {
                //var p = new QueryUserData(UserName).Execute();
                //var preference = p.Preferences;
                var qvotes = new QueryUserCommentVotesForSubmission(_submissionID, CachePolicy.None).SetUserContext(User);
                _commentVotes = await qvotes.ExecuteAsync();

                var qblocks = new QueryUserBlocks().SetUserContext(User);
                _userBlocks = (await qblocks.ExecuteAsync()).Where(x => x.Type == DomainType.User);
                //_commentSaves = new QueryUserSavedCommentsForSubmission(_submissionID).Execute();
            }

            //TODO: Set with preferences

            int nestLevel = 3;

            //TODO: set IsCollapsed flag in output on every comment below this threshold

            fullTree = (await q.ExecuteAsync()).Values;
            switch (_sort)
            {
                case CommentSortAlgorithm.Intensity:
                    fullTree = fullTree.OrderByDescending(x => Math.Max(1, (x.UpCount + x.DownCount)) ^ (Math.Min(x.UpCount, x.DownCount) / Math.Max(1, Math.Max(x.UpCount, x.DownCount))));
                    break;

                case CommentSortAlgorithm.New:
                    fullTree = fullTree.OrderByDescending(x => x.CreationDate);
                    break;

                case CommentSortAlgorithm.Old:
                    fullTree = fullTree.OrderBy(x => x.CreationDate);
                    break;

                case CommentSortAlgorithm.Bottom:
                    fullTree = fullTree.OrderBy(x => x.UpCount - x.DownCount).ThenByDescending(x => x.CreationDate);
                    break;

                default:

                    //top
                    fullTree = fullTree.OrderByDescending(x => x.UpCount - x.DownCount).ThenByDescending(x => x.CreationDate);
                    break;
            }

            var queryTree = fullTree.AsQueryable();
            queryTree = FilterSegment(queryTree);
            var queryableTree = TakeSegment(queryTree);

            //creating this to keep local vars in scope
            Func<usp_CommentTree_Result, NestedComment> mapToNestedCommentFunc = new Func<usp_CommentTree_Result, NestedComment>(commentTree =>
            {
                return commentTree.Map(User, SubmitterName, _commentVotes, _userBlocks);
            });

            List<NestedComment> comments = new List<NestedComment>();
            foreach (var c in queryableTree)
            {
                var n = mapToNestedCommentFunc(c);
                if (!n.IsCollapsed)
                {
                    n.IsCollapsed = (n.Sum <= _collapseThreshold);
                }
                if (addChildren)
                {
                    AddComments(fullTree, n, _count, nestLevel, 1, _collapseThreshold, _sort, mapToNestedCommentFunc);
                }

                //having small issue with comments load links for deleted children, see if this clears up
                n.ChildCount = fullTree.Count(x => x.ParentID == n.ID && !(x.IsDeleted && x.ChildCount == 0));
                n.Children.TotalCount = n.ChildCount;
                comments.Add(n);
            }

            var segment = new Domain.Models.CommentSegment();
            segment.Sort = _sort;
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
        private void AddComments(IEnumerable<usp_CommentTree_Result> queryTree, NestedComment parent, int count, int nestLevel, int currentNestLevel, int collapseThreshold, CommentSortAlgorithm sort, Func<usp_CommentTree_Result, NestedComment> mapToNestedCommentFunc)
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
                            var c = mapToNestedCommentFunc(children[i]);

                            if (!c.IsCollapsed)
                            {
                                c.IsCollapsed = (c.Sum <= _collapseThreshold);
                            }
                            var nextNestLevel = currentNestLevel + 1;
                            AddComments(queryTree, c, count, nestLevel, nextNestLevel, collapseThreshold, sort, mapToNestedCommentFunc);
                            parent.AddChildComment(c);
                            parent.Children.TotalCount = children.Count;
                            parent.Children.Sort = sort;
                        }
                    }
                }
                else
                {
                    parent.ChildCount = 0;
                }
            }
        }
    }
}
