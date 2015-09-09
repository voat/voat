using System.Collections.Generic;
using System.Linq;
using Voat.Utilities;
using Voat.Data.Models;

namespace Voat.Models
{
    public enum CommentSort { 
        New,
        Top
    }
    public class CommentBucketViewModel
    {
        //HACK: This doesn't belong here.
        public static usp_CommentTree_Result Map(Comment comment) {
            var singleComment = new usp_CommentTree_Result()
            {
                ID = comment.ID,
                Content = comment.Content,
                IsAnonymized = comment.IsAnonymized,
                ChildCount = 0,
                CreationDate = comment.CreationDate,
                DownCount = comment.DownCount,
                Depth = 0,
                FormattedContent = comment.FormattedContent,
                IsDistinguished = comment.IsDistinguished,
                LastEditDate = comment.LastEditDate,
                UpCount = comment.UpCount,
                SubmissionID = comment.SubmissionID,
                UserName = comment.UserName,
                ParentID = comment.ParentID,
                Path = "",
                Subverse = "",
                Votes = comment.Votes
            };
            return singleComment;
        }
        
        public CommentBucketViewModel(Comment comment) : this() {
            var singleComment = Map(comment);

            var commentTree = new List<usp_CommentTree_Result> { singleComment };
            DisplayTree = commentTree.AsQueryable();
            CommentTree = commentTree;
            Submission = DataCache.Submission.Retrieve(comment.SubmissionID);
            Subverse = DataCache.Subverse.Retrieve(Submission.Subverse);
        }

        //set defaults in comment display behavior
        public CommentBucketViewModel() {
            NestingThreshold = 2;
            CollapseSiblingThreshold = 2;
            Sort = CommentSort.Top;
            NegativeScoreThreshold = -4;
        }

        public int StartingIndex { get; set; }
        public int EndingIndex { get; set; }
        public int Count {
            get {
                return EndingIndex - StartingIndex;
            }
        }

        public int CollapseSiblingThreshold { get; set; }
        public int NestingThreshold { get; set; }
        public int NegativeScoreThreshold { get; set; }
        public CommentSort Sort { get; set; }
        public int? ParentID { get; set; }
        public Subverse Subverse { get; set; }
        public Submission Submission { get; set; }
        public IQueryable<usp_CommentTree_Result> DisplayTree { get; set; }
        public int TotalInDisplayBranch { get; set; }
        public List<usp_CommentTree_Result> CommentTree { get; set; }
    }
}