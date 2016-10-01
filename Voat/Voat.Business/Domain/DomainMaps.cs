﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
        public static IEnumerable<Domain.Models.Comment> Map(this IEnumerable<Domain.Models.Comment> list, bool populateUserState = false)
        {
            var mapped = list.Select(x => { ProcessComment(x); return x; }).ToList();
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

                    Title = submission.Title,
                    Url = submission.Url,
                    Content = (submission.Type == 1 ? submission.Content : (string)null),
                    //Support For Backwards compat, if FormattedContent is empty, do it here.
                    FormattedContent = (submission.Type == 1 && String.IsNullOrEmpty(submission.FormattedContent) ? Formatting.FormatMessage(submission.Content, true) : submission.FormattedContent),

                    LastEditDate = submission.LastEditDate,
                    ThumbnailUrl = VoatPathHelper.ThumbnailPath(submission.Thumbnail, true, true),
                    CommentCount = CommentCounter.CommentCount(submission.ID),
                    CreationDate = submission.CreationDate,
                    UpCount = (int)submission.UpCount,
                    Views = (int)submission.Views,
                    DownCount = (int)submission.DownCount,
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
                    ChildCount = 0,
                    Depth = 0,
                    Path = "",
                    Subverse = "",
                    ID = comment.ID,
                    ParentID = comment.ParentID,
                    Content = comment.Content,
                    FormattedContent = comment.FormattedContent,
                    CreationDate = comment.CreationDate,
                    LastEditDate = comment.LastEditDate,
                    SubmissionID = comment.SubmissionID,
                    UpCount = comment.UpCount,
                    DownCount = comment.DownCount,
                    IsDistinguished = comment.IsDistinguished,
                    IsDeleted = comment.IsDeleted,
                    IsAnonymized = comment.IsAnonymized,
                    UserName = comment.UserName,
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
                    ChildCount = 0,
                    Depth = 0,
                    Path = "",
                    Subverse = comment.Subverse,
                    ID = comment.ID,
                    ParentID = comment.ParentID,
                    Content = comment.Content,
                    FormattedContent = comment.FormattedContent,
                    CreationDate = comment.CreationDate,
                    LastEditDate = comment.LastEditDate,
                    SubmissionID = comment.SubmissionID,
                    UpCount = comment.UpCount,
                    DownCount = comment.DownCount,
                    IsDistinguished = comment.IsDistinguished,
                    IsDeleted = comment.IsDeleted,
                    IsAnonymized = comment.IsAnonymized,
                    UserName = comment.UserName,
                    Votes = 0 //don't think we use this
                };
            }
            return result;
        }
        public static NestedComment Map(this usp_CommentTree_Result treeComment, string submissionOwnerName, IEnumerable<CommentVoteTracker> commentVotes = null, IEnumerable<CommentSaveTracker> commentSaves = null)
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
                result.IsSubmitter = (treeComment.UserName == submissionOwnerName);
                //Set User State and secure comment
                ProcessComment(result, false, commentVotes, commentSaves);
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
                //Just a note, the entire Subverse in Data models for comments is a bit hacky as this info is needed in the app but data models don't contain it.
                if (String.IsNullOrEmpty(subverse))
                {
                    //TODO: need to convert pipeline to support this or pull this data out of the db
                    result.Subverse = "TODO";
                }
                else
                {
                    result.Subverse = subverse;
                }
                //Set User State and secure comment
                ProcessComment(result, populateUserState);
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
                //Set User State and secure comment
                ProcessComment(result);
            }
            return result;
        }
        public static void ProcessComment(Domain.Models.Comment comment, bool populateMissingUserState = false, IEnumerable<CommentVoteTracker> commentVotes = null, IEnumerable<CommentSaveTracker> commentSaves = null)
        {
            string userName = Thread.CurrentPrincipal.Identity.IsAuthenticated ? Thread.CurrentPrincipal.Identity.Name : null;
            if (!String.IsNullOrEmpty(userName))
            {
                comment.IsOwner = comment.UserName == userName;
                comment.Vote = 0;
                if (commentVotes != null)
                {
                    var vote = commentVotes.FirstOrDefault(x => x.CommentID == comment.ID);
                    if (vote != null)
                    {
                        comment.Vote = vote.VoteStatus;
                    }
                }
                else if (populateMissingUserState)
                {
                    comment.Vote = VotingComments.CheckIfVotedComment(userName, comment.ID);
                }

                comment.IsSaved = false;
                if (commentSaves != null)
                {
                    comment.IsSaved = commentSaves.Any(x => x.CommentID == comment.ID);
                }
                else if (populateMissingUserState)
                {
                    comment.IsSaved = SavingComments.CheckIfSavedComment(userName, comment.ID);
                }

            }
            comment.UserName = (comment.IsAnonymized ? comment.ID.ToString() : comment.UserName);
        }
    }
}
