using System;
using System.Linq;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Domain.Models;
using Voat.Domain.Query.Base;

namespace Voat.Domain.Query
{
    /// <summary>
    /// Allows segments of comments to be retrieved out of the comment tree for a submission.
    /// </summary>
    //Currently this class does not cache a segment, but it uses the cached comment tree to format the output per request.
    public class QueryCommentContext : BaseQueryCommentSegment
    {
        protected int? _context;
        protected int _commentID;

        public QueryCommentContext(int submissionID, int commentID, int? context = null, CommentSortAlgorithm? sort = null)
        {
            _submissionID = submissionID;
            _commentID = commentID;
            _sort = sort.HasValue ? sort.Value : CommentSortAlgorithm.Top;

            //_options = options ?? SearchOptions.Default;
            if (context.HasValue)
            {
                //ensure context isn't negative and less than 20
                _context = Math.Min(20, Math.Max(0, context.Value));
            }

            //bringing up context, don't collapse
            _collapseThreshold = -10000;
        }

        public override async Task<CommentSegment> ExecuteAsync()
        {
            bool addChildren = !_context.HasValue; //logic here is that if context is negative user is viewing permalink
            var segment = await base.GetSegment(addChildren);

            if (_context.HasValue)
            {
                //build the context upward
                var primary = fullTree.Where(x => x.ID == _commentID).FirstOrDefault();
                if (primary != null)
                {
                    var parent = (primary.ParentID != null ? fullTree.Where(x => x.ID == primary.ParentID).FirstOrDefault() : null);

                    int previousChildCount = primary.ChildCount.Value;
                    int currentContext = 0;
                    var childSegment = segment;
                    while (parent != null && (currentContext < _context || _context == 0))
                    {
                        var parentNestedComment = parent.Map(SubmitterName, _commentVotes);
                        parentNestedComment.Children = childSegment;
                        childSegment = new CommentSegment(parentNestedComment);
                        childSegment.Sort = _sort;
                        parent = (parent.ParentID != null ? fullTree.Where(x => x.ID == parent.ParentID).FirstOrDefault() : null);

                        //HACK: So we have a bit of a bug here. If we provide valid counts here the UI will attempt to offer loading capabilities
                        //if this happens the ajax loading will most likely load duplicate comments as sorting doesn't appy to history, if dups get loaded
                        //the UI goes all kinds of cray cray. So, until I can think of a clean solution, we will load context history as having no siblings
                        //childSegment.TotalCount = parent == null ? 0 : parent.ChildCount.Value;
                        childSegment.TotalCount = 1;
                        currentContext++;
                    }
                    segment = childSegment;
                }
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
