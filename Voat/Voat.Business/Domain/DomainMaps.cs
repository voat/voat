using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Voat.Data.Models;
using Voat.Domain.Models;
using Voat.Utilities;

namespace Voat.Domain
{
    public static class DomainMaps
    {
        public static IEnumerable<Domain.Models.Submission> Map(this IEnumerable<Data.Models.Submission> list)
        {
            var mapped = list.Select(x => x.Map()).ToList();
            return mapped;
        }
        public static IEnumerable<Domain.Models.Comment> Map(this IEnumerable<Data.Models.Comment> list, string subverse)
        {
            var mapped = list.Select(x => x.Map(subverse)).ToList();
            return mapped;
        }
        public static IEnumerable<UserMessage> Map(this IEnumerable<UserMessage> list)
        {
            var mapped = list.Select(x => x.Map()).ToList();
            return mapped;
        }
        public static UserMessage Map(this UserMessage message)
        {
            UserMessage result = message;
            if (result != null)
            {
                if (String.IsNullOrEmpty(result.FormattedContent))
                {
                    result.FormattedContent = Formatting.FormatMessage(result.Content);
                }
            }
            return result;
        }
        public static Domain.Models.Submission Map(this Data.Models.Submission submission)
        {
            Domain.Models.Submission result = null;
            if (submission != null)
            {
                result = new Domain.Models.Submission()
                {
                    ID = submission.ID,
                    UserName = (submission.IsAnonymized ? submission.ID.ToString() : submission.UserName),
                    LastEditDate = submission.LastEditDate,
                    ThumbnailUrl = VoatPathHelper.ThumbnailPath(submission.Thumbnail, true, true),
                    CommentCount = CommentCounter.CommentCount(submission.ID),
                    CreationDate = submission.CreationDate,
                    UpCount = (int)submission.UpCount,
                    Views = (int)submission.Views,
                    DownCount = (int)submission.DownCount,
                    Content = (submission.Type == 1 ? submission.Content : (string)null),
                    FormattedContent = (submission.Type == 1 ? Formatting.FormatMessage(submission.Content, true) : (string)null), //Need to override when formatting occurs on submission because data is mismatched in base table
                    Title = (submission.Type == 1 ? submission.Title : submission.LinkDescription),
                    Url = (submission.Type == 2 ? submission.Content : (string)null),
                    Type = submission.Type == 1 ? SubmissionType.Text : SubmissionType.Link,
                    Subverse = submission.Subverse,
                    IsAnonymized = submission.IsAnonymized,
                    IsDeleted = submission.IsDeleted,
                    Rank = submission.Rank,
                    RelativeRank = submission.RelativeRank
                };
            }
            return result;
        }

        public static Domain.Models.Comment Map(this Data.Models.Comment comment, string subverse, bool populateUserState = false)
        {
            Domain.Models.Comment result = null;
            if (comment != null)
            {
                result = MapToNestedComment(comment, subverse, populateUserState);
            }
            return result;
        }

        public static Models.SubverseInformation Map(this Subverse subverse)
        {
            SubverseInformation result = null;
            if (subverse != null)
            {
                result = new Models.SubverseInformation()
                {
                    Name = subverse.Name,
                    Title = subverse.Title,
                    Description = subverse.Description,
                    CreationDate = subverse.CreationDate,
                    SubscriberCount = (subverse.SubscriberCount == null ? 0 : subverse.SubscriberCount.Value),
                    RatedAdult = subverse.IsAdult,
                    Sidebar = subverse.SideBar,
                    Type = subverse.Type,
                    IsAnonymized = subverse.IsAnonymized,
                    IsAdult = subverse.IsAdult,
                    //IsAdminDisabled = (subverse.IsAdminDisabled.HasValue ? subverse.IsAdminDisabled.Value : false),
                    CreatedBy = subverse.CreatedBy
                };
                result.FormattedSidebar = Formatting.FormatMessage(subverse.SideBar, true);

            }

            return result;
        }

        public static usp_CommentTree_Result MapToTree(this Data.Models.Comment comment)
        {
            usp_CommentTree_Result result = null;
            if (comment != null)
            {
                result = new usp_CommentTree_Result()
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
            }
            return result;
        }
        public static usp_CommentTree_Result MapToTree(this Domain.Models.Comment comment)
        {
            usp_CommentTree_Result result = null;
            if (comment != null)
            {
                result = new usp_CommentTree_Result()
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
                    Subverse = comment.Subverse,
                    Votes = 0 //don't think we use this.
                };
            }
            return result;
        }
        public static NestedComment Map(this usp_CommentTree_Result treeComment)
        {
            NestedComment result = null;
            if (treeComment != null)
            {
                result = new NestedComment();
                result.ID = treeComment.ID;
                result.ParentID = treeComment.ParentID;
                result.ChildCount = treeComment.ChildCount.Value;
                result.Content = treeComment.Content;
                result.FormattedContent = treeComment.FormattedContent;
                result.UserName = treeComment.UserName;
                result.UpCount = (int)treeComment.UpCount;
                result.DownCount = (int)treeComment.DownCount;
                result.CreationDate = treeComment.CreationDate;
                result.IsAnonymized = treeComment.IsAnonymized;
                result.IsDeleted = treeComment.IsDeleted;
                result.IsDistinguished = treeComment.IsDistinguished;
                result.LastEditDate = treeComment.LastEditDate;
                result.SubmissionID = treeComment.SubmissionID;
                result.Subverse = treeComment.Subverse;
            }
            return result;
        }
        public static NestedComment MapToNestedComment(this Data.Models.Comment comment, string subverse, bool populateUserState = false)
        {
            NestedComment result = null;
            if (comment != null)
            {
                result = new NestedComment();
                result.ID = comment.ID;
                result.ParentID = comment.ParentID;
                result.ChildCount = 0;
                result.Content = comment.Content;
                result.FormattedContent = comment.FormattedContent;
                result.UserName = comment.UserName;
                result.UpCount = (int)comment.UpCount;
                result.DownCount = (int)comment.DownCount;
                result.CreationDate = comment.CreationDate;
                result.IsAnonymized = comment.IsAnonymized;
                result.IsDeleted = comment.IsDeleted;
                result.IsDistinguished = comment.IsDistinguished;
                result.LastEditDate = comment.LastEditDate;
                result.SubmissionID = comment.SubmissionID;
                if (String.IsNullOrEmpty(subverse))
                {
                    //TODO: need to convert pipeline to support this or pull this data out of the db
                    result.Subverse = "TODO";
                }
                else
                {
                    result.Subverse = subverse;
                }
                SetUserRelatedProperties(result, populateUserState);
            }
            return result;
        }
        public static NestedComment Map(this Domain.Models.Comment comment)
        {
            NestedComment result = null;
            if (comment != null)
            {
                result = new NestedComment();
                result.ID = comment.ID;
                result.ParentID = comment.ParentID;
                result.ChildCount = 0;
                result.Content = comment.Content;
                result.FormattedContent = comment.FormattedContent;
                result.UserName = comment.UserName;
                result.UpCount = (int)comment.UpCount;
                result.DownCount = (int)comment.DownCount;
                result.CreationDate = comment.CreationDate;
                result.IsAnonymized = comment.IsAnonymized;
                result.IsDeleted = comment.IsDeleted;
                result.IsDistinguished = comment.IsDistinguished;
                result.LastEditDate = comment.LastEditDate;
                result.SubmissionID = comment.SubmissionID;
                result.Subverse = comment.Subverse;
                SetUserRelatedProperties(result);
            }
            return result;
        }

        private static void SetUserRelatedProperties(NestedComment comment, bool populateUserState = false)
        {
            if (System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                string userName = System.Threading.Thread.CurrentPrincipal.Identity.Name;

                comment.IsSubmitter = comment.UserName == userName;

                if (populateUserState)
                {
                    if (!comment.IsSaved.HasValue)
                    {
                        comment.IsSaved = SavingComments.CheckIfSavedComment(userName, comment.ID);
                    }
                    if (!comment.Vote.HasValue)
                    {
                        comment.Vote = VotingComments.CheckIfVotedComment(userName, comment.ID);
                    }
                }
            }
        }
    }
}
