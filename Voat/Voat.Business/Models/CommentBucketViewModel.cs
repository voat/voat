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
                Id = comment.Id,
                CommentContent = comment.CommentContent,
                Anonymized = comment.Anonymized,
                ChildCount = 0,
                Date = comment.Date,
                Dislikes = comment.Dislikes,
                Depth = 0,
                FormattedContent = comment.FormattedContent,
                IsDistinguished = comment.IsDistinguished,
                LastEditDate = comment.LastEditDate,
                Likes = comment.Likes,
                MessageId = comment.MessageId,
                Name = comment.Name,
                ParentId = comment.ParentId,
                Path = "",
                Subverse = "",
                Votes = comment.Votes
            };
            return singleComment;
        }
        public CommentBucketViewModel(Comment comment) : this() {
            //Convert object to known type so we can REUSE SOME CODE maybe - if God loves us.

            var singleComment = Map(comment);

            var commentTree = new System.Collections.Generic.List<usp_CommentTree_Result> { singleComment };
            DisplayTree = commentTree.AsQueryable();
            CommentTree = commentTree;
            Submission = DataCache.Submission.Retrieve(comment.MessageId);
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
        public Message Submission { get; set; }
        public IQueryable<usp_CommentTree_Result> DisplayTree { get; set; }
        public int TotalInDisplayBranch { get; set; }
        public List<usp_CommentTree_Result> CommentTree { get; set; }



    }
}