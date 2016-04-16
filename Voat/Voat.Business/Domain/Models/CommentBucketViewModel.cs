using System.Collections.Generic;
using System.Linq;
using Voat.Caching;
using Voat.Data.Models;

namespace Voat.Models
{
    public class CommentBucketViewModel
    {
        #region Constructors

        public CommentBucketViewModel(Comment comment) : this()
        {
            var singleComment = Domain.DomainMaps.MapToTree(comment);

            var commentTree = new List<usp_CommentTree_Result> { singleComment };
            DisplayTree = commentTree.AsQueryable();
            CommentTree = commentTree;
            Submission = DataCache.Submission.Retrieve(comment.SubmissionID);
            Subverse = DataCache.Subverse.Retrieve(Submission.Subverse);
        }

        //set defaults in comment display behavior
        public CommentBucketViewModel()
        {
            NestingThreshold = 2;
            CollapseSiblingThreshold = 2;
            Sort = Domain.Models.CommentSort.Top;
            NegativeScoreThreshold = -4;
        }

        #endregion Constructors

        public int CollapseSiblingThreshold { get; set; }
        public List<usp_CommentTree_Result> CommentTree { get; set; }

        public int Count
        {
            get
            {
                return EndingIndex - StartingIndex;
            }
        }

        public IQueryable<usp_CommentTree_Result> DisplayTree { get; set; }
        public int EndingIndex { get; set; }
        public int NegativeScoreThreshold { get; set; }
        public int NestingThreshold { get; set; }
        public int? ParentID { get; set; }
        public Domain.Models.CommentSort Sort { get; set; }
        public int StartingIndex { get; set; }
        public Submission Submission { get; set; }
        public Subverse Subverse { get; set; }
        public int TotalInDisplayBranch { get; set; }
    }
}
