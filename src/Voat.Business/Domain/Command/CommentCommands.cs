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
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Command
{
    [Serializable]
    public class CreateCommentCommand : CacheCommand<CommandResponse<Domain.Models.Comment>>
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
        protected override async Task<CommandResponse<Models.Comment>> ExecuteStage(CommandStage stage, CommandResponse<Models.Comment> previous)
        {
            switch (stage)
            {
                case CommandStage.OnValidation:
                    if (Content.Length > 10000)
                    {
                        return CommandResponse.FromStatus<Models.Comment>(null, Status.Denied, "Comment can not exceed 10,000 characters");
                    }
                    break;
            }
            return CommandResponse.FromStatus<Models.Comment>(null, Status.Success);
        }
        public string Content { get; set; }

        public int? ParentCommentID { get; set; }

        public int SubmissionID { get; set; }

        protected override async Task<CommandResponse<Domain.Models.Comment>> CacheExecute()
        {
            using (var db = new Repository(User))
            {
                var data = await db.PostComment(this.SubmissionID, this.ParentCommentID, this.Content);
                var mapped = CommandResponse.Map(data, data.Response.Map(User));
                return data;
            }
        }

        protected override void UpdateCache(CommandResponse<Domain.Models.Comment> result)
        {
            if (result.Success)
            {
                var c = result.Response;
                var key = CachingKey.CommentTree(result.Response.SubmissionID.Value);

                //Prevent key-ed entries if parent isn't in cache with expiration date
                if (CacheHandler.Instance.Exists(key))
                {
                    //Add new comment
                    var treeItem = c.MapToTree();
                    treeItem.UserName = UserName;
                    CacheHandler.Instance.DictionaryReplace(key, c.ID, treeItem);

                    if (c.ParentID.HasValue)
                    {
                        //Update parent's ChildCount in cache
                        CacheHandler.Instance.DictionaryReplace<int, usp_CommentTree_Result>(key, c.ParentID.Value, x => { x.ChildCount += 1; return x; });
                    }
                }
            }
        }
    }

    [Serializable]
    public class DeleteCommentCommand : CacheCommand<CommandResponse<Domain.Models.Comment>>
    {
        public DeleteCommentCommand(int commentID, string reason = null)
        {
            if (commentID <= 0)
            {
                throw new ArgumentOutOfRangeException("CommentID is not valid");
            }
            this.CommentID = commentID;
            this.Reason = reason;
        }

        public int CommentID { get; set; }

        public string Reason { get; set; }

        protected override async Task<CommandResponse<Domain.Models.Comment>> CacheExecute()
        {
            using (var db = new Repository(User))
            {
                var result = await db.DeleteComment(this.CommentID, this.Reason);
                return result;
            }
        }

        protected override void UpdateCache(CommandResponse<Domain.Models.Comment> result)
        {
            if (result != null && result.Success)
            {
                var key = CachingKey.CommentTree(result.Response.SubmissionID.Value);

                //Prevent key-ed entries if parent isn't in cache with expiration date
                if (CacheHandler.Instance.Exists(key))
                {
                    //var treeItem = result.Response.MapToTree();

                    CacheHandler.Instance.DictionaryReplace<int, usp_CommentTree_Result>(key, result.Response.ID, x =>
                    {
                        x.IsDeleted = result.Response.IsDeleted;
                        x.Content = result.Response.Content;
                        x.FormattedContent = result.Response.FormattedContent;
                        return x;
                    });
                }
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

        protected override async Task<CommandResponse<Domain.Models.Comment>> ExecuteStage(CommandStage stage, CommandResponse<Domain.Models.Comment> previous)
        {
            switch (stage)
            {
                case CommandStage.OnValidation:
                    if (Content.Length > 10000)
                    {
                        return CommandResponse.FromStatus<Models.Comment>(null, Status.Denied, "Comment can not exceed 10,000 characters");
                    }
                    break;
            }
            return CommandResponse.FromStatus<Models.Comment>(null, Status.Success);
        }

        protected override async Task<Tuple<CommandResponse<Domain.Models.Comment>, Comment>> CacheExecute()
        {
            using (var db = new Repository(User))
            {
                var result = await db.EditComment(this.CommentID, this.Content);
                return Tuple.Create(new CommandResponse<Domain.Models.Comment>(result.Response.Map(User, null), result.Status, result.Message), result.Response);
            }
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

                    //CacheHandler.Instance.Replace(key, result.ID, treeItem);
                    CacheHandler.Instance.DictionaryReplace<int, usp_CommentTree_Result>(key, result.ID, x =>
                    {
                        x.Content = result.Content;
                        x.FormattedContent = result.FormattedContent;
                        x.LastEditDate = result.LastEditDate;
                        return x;
                    });
                }
            }
        }
    }
}
