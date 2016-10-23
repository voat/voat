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
    public class QueryCommentSegment : BaseQueryCommentSegment
    {
        protected int? _index;
        protected int? _parentID;

        public QueryCommentSegment(int submissionID, int? parentID = null, int? index = null, CommentSortAlgorithm? sort = null)
        {
            _submissionID = submissionID;
            _parentID = parentID.HasValue && parentID.Value > 0 ? parentID : null;
            _index = index;
            _sort = sort.HasValue ? sort.Value : CommentSortAlgorithm.Top;
        }

        protected override IQueryable<usp_CommentTree_Result> FilterSegment(IQueryable<usp_CommentTree_Result> commentTree)
        {
            var filtered = commentTree.Where(x => x.ParentID == _parentID && !(x.IsDeleted && x.ChildCount == 0));

            //filtered = filtered.Skip(_index.HasValue ? _index.Value : 0).Take(_options.Count);
            return filtered;
        }

        protected override IQueryable<usp_CommentTree_Result> TakeSegment(IQueryable<usp_CommentTree_Result> commentTree)
        {
            return commentTree.Skip(_index.HasValue ? _index.Value : 0).Take(_count * 2);
        }

        public override async Task<CommentSegment> ExecuteAsync()
        {
            var segment = await base.GetSegment(true);
            segment.StartingIndex = _index.HasValue ? _index.Value : 0;
            return segment;
        }
        public int? ParentID
        {
            get {
                return _parentID;
            }
        }
    }
}
