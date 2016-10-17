using System;

namespace Voat.Data
{
    //This class should be phased out and the DomainMapping should replace this logic. Leaving in until refactoring time (soon (tm))
    public static class Selectors
    {
        public static Func<Models.Submission, Models.Submission> SecureSubmission = new Func<Models.Submission, Models.Submission>(x =>
        {
            if (x != null)
            {
                if (x.IsAnonymized)
                {
                    x.UserName = x.ID.ToString();
                }
                if (x.IsDeleted)
                {
                    x.UserName = "deleted";
                }
            }
            return x;
        });

        public static Func<Models.Comment, Models.Comment> SecureComment = new Func<Models.Comment, Models.Comment>(x =>
        {
            if (x != null)
            {
                if (x.IsAnonymized)
                {
                    x.UserName = x.ID.ToString();
                }
                if (x.IsDeleted)
                {
                    x.UserName = "deleted";
                }
            }
            return x;
        });

        public static Func<Models.usp_CommentTree_Result, Models.usp_CommentTree_Result> SecureCommentTree = new Func<Models.usp_CommentTree_Result, Models.usp_CommentTree_Result>(x =>
        {
            if (x != null)
            {
                if (x.IsAnonymized)
                {
                    x.UserName = x.ID.ToString();
                }
                if (x.IsDeleted)
                {
                    x.UserName = "deleted";
                }
            }
            return x;
        });
    }
}
