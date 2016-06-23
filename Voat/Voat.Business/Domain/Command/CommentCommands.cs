using System;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Command
{
    [Serializable]
    public class CreateCommentCommand : CacheCommand<CommandResponse<Domain.Models.Comment>, CommandResponse<Comment>>
    {
        public CreateCommentCommand(int submissionID, int? parentCommentID, string content)
        {
            if (submissionID <= 0)
            {
                throw new ArgumentOutOfRangeException("SubmissionID must be greater than 0");
            }
            this.Content = content;
            this.SubmissionID = submissionID;
            this.ParentCommentID = parentCommentID;
        }

        public string Content { get; set; }
        public int? ParentCommentID { get; set; }
        public int SubmissionID { get; set; }

        protected override async Task<Tuple<CommandResponse<Domain.Models.Comment>, CommandResponse<Comment>>> CacheExecute()
        {
            var data = await Task.Factory.StartNew(() =>
            {
                using (var db = new Repository())
                {
                    return db.PostComment(this.SubmissionID, this.ParentCommentID, this.Content);
                }
            });

            var mapped = CommandResponse.Map(data, data.Response.Map());

            return Tuple.Create(mapped, data);
        }

        protected override void UpdateCache(CommandResponse<Comment> result)
        {
            if (result.Success)
            {
                var c = result.Response;

                //HACK: Pretty sure this will fail if dictionary isn't already in cache.
                var key = CachingKey.CommentTree(result.Response.SubmissionID.Value);

                //Prevent key-ed entries if parent isn't in cache with expiration date
                if (CacheHandler.Instance.Exists(key))
                {
                    var treeItem = c.MapToTree();
                    CacheHandler.Instance.Replace(key, c.ID, treeItem);
                }
            }
        }
    }

    [Serializable]
    public class DeleteCommentCommand : CacheCommand<CommandResponse, Comment>
    {
        public DeleteCommentCommand(int commentID)
        {
            if (commentID <= 0)
            {
                throw new ArgumentOutOfRangeException("CommentID is not valid");
            }
            this.CommentID = commentID;
        }

        public int CommentID { get; set; }

        protected override async Task<Tuple<CommandResponse, Comment>> CacheExecute()
        {
            var result = await Task.Factory.StartNew(() =>
            {
                using (var db = new Repository())
                {
                    return db.DeleteComment(this.CommentID);
                }
            });
            return Tuple.Create(CommandResponse.Successful(), result);
        }

        protected override void UpdateCache(Comment result)
        {
            if (result != null)
            {
                var key = CachingKey.CommentTree(result.SubmissionID.Value);
                //Prevent key-ed entries if parent isn't in cache with expiration date
                if (CacheHandler.Instance.Exists(key))
                {
                    var treeItem = result.MapToTree();
                    CacheHandler.Instance.Replace(key, result.ID, treeItem);
                }


                // CacheHandler.Instance.Replace(key, CommentID,
                //     new Func<usp_CommentTree_Result, usp_CommentTree_Result>(x =>
                //     {
                //         x.Content = result.Content;
                //         x.FormattedContent = result.FormattedContent;
                //         x.IsDeleted = result.IsDeleted;
                //         return x;
                //     })
                //);
            }
        }
    }

    [Serializable]
    public class EditCommentCommand : CacheCommand<CommandResponse<Domain.Models.Comment>, Comment>
    {
        public EditCommentCommand(int commentID, string content)
        {
            this.Content = content;
            this.CommentID = commentID;
        }

        public int CommentID { get; set; }
        public string Content { get; set; }

        protected override async Task<Tuple<CommandResponse<Domain.Models.Comment>, Comment>> CacheExecute()
        {
            var result = await Task.Factory.StartNew(() =>
            {
                using (var db = new Repository())
                {
                    return db.EditComment(this.CommentID, this.Content);
                }
            });
            return Tuple.Create(CommandResponse.Successful(result.Map()), result);
        }

        protected override void UpdateCache(Comment result)
        {
            if (result != null)
            {
                var key = CachingKey.CommentTree(result.SubmissionID.Value);
                //Prevent key-ed entries if parent isn't in cache with expiration date
                if (CacheHandler.Instance.Exists(key))
                {
                    var treeItem = result.MapToTree();
                    CacheHandler.Instance.Replace(key, result.ID, treeItem);
                }
                // CacheHandler.Instance.Replace(key, CommentID,
                //     new Func<usp_CommentTree_Result, usp_CommentTree_Result>(x =>
                //     {
                //         //update cache content only
                //         x.Content = result.Content;
                //         x.FormattedContent = result.FormattedContent;
                //         x.UpCount = result.UpCount;
                //         x.DownCount = result.DownCount;
                //         x.LastEditDate = result.LastEditDate;
                //         x.IsDistinguished = result.IsDistinguished;
                //         return x;
                //     })
                //);
            }
        }
    }
}
