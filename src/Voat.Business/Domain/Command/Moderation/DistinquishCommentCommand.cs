using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Command
{
    public class DistinquishCommentCommand : CacheCommand<CommandResponse<Models.Comment>>
    {
        private int _commentID;

        public DistinquishCommentCommand(int commentID)
        {
            _commentID = commentID;
        }

        protected override async Task<CommandResponse<Models.Comment>> CacheExecute()
        {
            using (var repo = new Repository(User))
            {
                var result = await repo.DistinguishComment(_commentID);
                return result;
            }
        }

        protected override void UpdateCache(CommandResponse<Models.Comment> result)
        {
            if (result.Success)
            {
                var comment = result.Response;
                //purge subscriptions from cache because they just changed
                CacheHandler.Instance.DictionaryReplace<int, usp_CommentTree_Result>(CachingKey.CommentTree(comment.SubmissionID.Value), comment.ID, x => { x.IsDistinguished = comment.IsDistinguished; return x; }, true);
            }
        }
    }
}
