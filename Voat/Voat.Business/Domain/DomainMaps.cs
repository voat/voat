using System;
using System.Collections.Generic;
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
            //Have to materialize this list
            var mapped = list.Select(x => x.Map()).ToList();
            return mapped;
            //return mapped.Count > 0 ? mapped : null;
        }

        public static IEnumerable<Domain.Models.Comment> Map(this IEnumerable<Data.Models.Comment> list)
        {
            //Have to materialize this list
            //Have to materialize this list
            var mapped = list.Select(x => x.Map()).ToList();
            return mapped;
            //return mapped.Count > 0 ? mapped : null;
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
                    IsDeleted = submission.IsDeleted
                    //Rank = submission.Rank
                };
            }
            return result;
        }

        public static Domain.Models.Comment Map(this Data.Models.Comment comment)
        {
            Domain.Models.Comment result = null;
            if (comment != null)
            {
                result = new Domain.Models.Comment()
                {
                    ID = comment.ID,
                    UserName = (comment.IsAnonymized ? comment.ID.ToString() : comment.UserName),
                    LastEditDate = comment.LastEditDate,
                    CreationDate = comment.CreationDate,
                    UpCount = (int)comment.UpCount,
                    DownCount = (int)comment.DownCount,
                    IsDeleted = comment.IsDeleted,
                    IsAnonymized = comment.IsAnonymized,
                    IsDistinguished = comment.IsDistinguished,
                    //TODO: Throws object disposed exception
                    //Subverse = comment.Submission.Subverse,
                    Subverse = "TODO",
                    Content = comment.Content,
                    FormattedContent = comment.FormattedContent,
                    ParentID = comment.ParentID,
                    SubmissionID = comment.SubmissionID
                };
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
    }
}
