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
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;
using Voat.Data;

//using Microsoft.AspNet.SignalR;
//using Voat.Data.Models;
using Voat.Data.Models;
using Voat.Domain.Query;
using Voat.Utilities;

namespace Voat.Notifications
{
    public static class NotificationManager
    {
        private static async Task<bool> UserAllowsAnonMentions(string userName, bool isAnonymized)
        {
            var allowed = true;
            if (isAnonymized)
            {
                var q = new QueryUserPreferences(userName);
                var prefs = await q.ExecuteAsync();
                allowed = !prefs.BlockAnonymized;
            }
            return allowed;
        }

        internal static async Task SendUserMentionNotification(Comment comment, IEnumerable<string> users)
        {
            if (comment != null)
            {
                foreach (var user in users)
                {
                    if (UserHelper.UserExists(user))
                    {
                        try
                        {
                            string recipient = UserHelper.OriginalUsername(user);

                            //BlockedUser Implementation - Comment User Mention
                            if (!MesssagingUtility.IsSenderBlocked(comment.UserName, recipient))
                            {
                                if (await UserAllowsAnonMentions(user, comment.IsAnonymized))
                                {
                                    //var submission = DataCache.Submission.Retrieve(comment.SubmissionID);

                                    var q = new QuerySubmission(comment.SubmissionID.Value);
                                    var submission = await q.ExecuteAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                                    //var subverse = DataCache.Subverse.Retrieve(submission.Subverse);

                                    var message = new Domain.Models.Message();

                                    message.IsAnonymized = comment.IsAnonymized;
                                    message.Recipient = recipient;
                                    message.RecipientType = Domain.Models.IdentityType.User;
                                    message.Sender = comment.UserName;
                                    message.SenderType = Domain.Models.IdentityType.User;
                                    message.Subverse = submission.Subverse;
                                    message.SubmissionID = comment.SubmissionID;
                                    message.CommentID = comment.ID;
                                    message.Type = Domain.Models.MessageType.CommentMention;
                                    message.CreationDate = Repository.CurrentDate;

                                    using (var repo = new Repository())
                                    {
                                        var response = await repo.SendMessage(message);

                                        if (response.Success)
                                        {
                                            EventNotification.Instance.SendMentionNotice(recipient, comment.UserName, Domain.Models.ContentType.Comment, comment.ID, comment.Content);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
        }

        internal static async Task SendUserMentionNotification(Submission submission, IEnumerable<string> users)
        {
            if (submission != null)
            {
                foreach (var user in users)
                {
                    if (UserHelper.UserExists(user))
                    {
                        try
                        {
                            string recipient = UserHelper.OriginalUsername(user);

                            //BlockedUser Implementation - Submission User Mention
                            if (!MesssagingUtility.IsSenderBlocked(submission.UserName, recipient))
                            {
                                if (await UserAllowsAnonMentions(user, submission.IsAnonymized))
                                {
                                    //var subverse = DataCache.Subverse.Retrieve(submission.Subverse);
                                    var message = new Domain.Models.Message();

                                    message.IsAnonymized = submission.IsAnonymized;
                                    message.Recipient = recipient;
                                    message.RecipientType = Domain.Models.IdentityType.User;
                                    message.Sender = submission.UserName;
                                    message.SenderType = Domain.Models.IdentityType.User;
                                    message.Subverse = submission.Subverse;
                                    message.SubmissionID = submission.ID;
                                    message.Type = Domain.Models.MessageType.SubmissionMention;
                                    message.CreationDate = Repository.CurrentDate;

                                    using (var repo = new Repository())
                                    {
                                        var response = await repo.SendMessage(message);

                                        if (response.Success)
                                        {
                                            EventNotification.Instance.SendMentionNotice(recipient, submission.UserName, Domain.Models.ContentType.Submission, submission.ID, submission.Content);
                                        }
                                    }
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
        }

        internal static async Task SendCommentReplyNotification(IPrincipal user, Data.Models.Submission submission, Data.Models.Comment comment)
        {
            try
            {
                using (var _db = new VoatOutOfRepositoryDataContextAccessor())
                {
                    Random _rnd = new Random();

                    if (comment.ParentID != null && comment.Content != null)
                    {
                        // find the parent comment and its author
                        var parentComment = _db.Comment.Find(comment.ParentID);
                        if (parentComment != null)
                        {
                            // check if recipient exists
                            if (UserHelper.UserExists(parentComment.UserName))
                            {
                                // do not send notification if author is the same as comment author
                                if (parentComment.UserName != comment.UserName)
                                {
                                    // send the message
                                    //BlockedUser Implementation - Comment Reply
                                    if (!MesssagingUtility.IsSenderBlocked(comment.UserName, parentComment.UserName))
                                    {
                                        //var submission = DataCache.Submission.Retrieve(comment.SubmissionID);
                                        //var q = new QuerySubmission(comment.SubmissionID.Value);
                                        //var submission = await q.ExecuteAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                                        if (submission != null)
                                        {
                                            //var subverse = DataCache.Subverse.Retrieve(submission.Subverse);

                                            var message = new Domain.Models.Message();

                                            message.IsAnonymized = submission.IsAnonymized;
                                            message.Recipient = parentComment.UserName;
                                            message.RecipientType = Domain.Models.IdentityType.User;
                                            message.Sender = comment.UserName;
                                            message.SenderType = Domain.Models.IdentityType.User;
                                            message.Subverse = submission.Subverse;
                                            message.SubmissionID = submission.ID;
                                            message.CommentID = comment.ID;
                                            message.Type = Domain.Models.MessageType.CommentReply;
                                            message.CreationDate = Repository.CurrentDate;

                                            using (var repo = new Repository(user))
                                            {
                                                var response = await repo.SendMessage(message);
                                                if (response.Success)
                                                {
                                                    EventNotification.Instance.SendMessageNotice(message.Recipient, message.Sender, Domain.Models.MessageTypeFlag.CommentReply, Domain.Models.ContentType.Comment, comment.ID);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static async Task SendSubmissionReplyNotification(IPrincipal user, Data.Models.Submission submission, Data.Models.Comment comment)
        {
            try
            {
                // comment reply is sent to a root comment which has no parent id, trigger post reply notification
                //var submission = DataCache.Submission.Retrieve(comment.SubmissionID);
                //var q = new QuerySubmission(comment.SubmissionID.Value);
                //var submission = await q.ExecuteAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                
                if (submission != null)
                {
                    // check if recipient exists
                    if (UserHelper.UserExists(submission.UserName))
                    {
                        // do not send notification if author is the same as comment author
                        if (submission.UserName != comment.UserName)
                        {
                            //BlockedUser Implementation - Submission Reply
                            if (!MesssagingUtility.IsSenderBlocked(comment.UserName, submission.UserName))
                            {
                                var message = new Domain.Models.Message();

                                message.IsAnonymized = submission.IsAnonymized;
                                message.Recipient = submission.UserName;
                                message.RecipientType = Domain.Models.IdentityType.User;
                                message.Sender = comment.UserName;
                                message.SenderType = Domain.Models.IdentityType.User;
                                message.Subverse = submission.Subverse;
                                message.SubmissionID = submission.ID;
                                message.CommentID = comment.ID;
                                message.Type = Domain.Models.MessageType.SubmissionReply;
                                message.CreationDate = Repository.CurrentDate;

                                using (var repo = new Repository(user))
                                {
                                    var response = await repo.SendMessage(message);
                                    if (response.Success)
                                    {
                                        EventNotification.Instance.SendMessageNotice(message.Recipient, message.Sender, Domain.Models.MessageTypeFlag.CommentReply, Domain.Models.ContentType.Comment, comment.ID);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task OnCommentPosted(IPrincipal user, Data.Models.Submission submission, Data.Models.Comment comment)
        {
            if (comment.ParentID != null && comment.Content != null)
            {
                await SendCommentReplyNotification(user, submission, comment);
            }
            else
            {
                await SendSubmissionReplyNotification(user, submission, comment);
            }
            var users = FindMentions(comment.Content);
            if (users.Any())
            {
                await SendUserMentionNotification(comment, users);
            }
        }

        public static async Task OnSubmissionPosted(IPrincipal user, Data.Models.Submission submission)
        {
            string input = submission.Title + " " + submission.Content;
            var users = FindMentions(input);
            if (users.Any())
            {
                await SendUserMentionNotification(submission, users);
            }
        }
        private static IEnumerable<string> FindMentions(string input)
        {
            var matchmaker = new MatchMaker() { IgnoreDuplicateMatches = true, MatchThreshold = 5 };
            matchmaker.IsDuplicate = (match, matches) => { return matches.Any(x => String.Equals(x.Groups["user"].Value, match.Groups["user"].Value, StringComparison.OrdinalIgnoreCase)); };

            if (matchmaker.Process(input, CONSTANTS.ACCEPTABLE_LEADS + CONSTANTS.USER_HOT_LINK_REGEX))
            {
                var matches = matchmaker.FilteredMatches;
                var users = matches.Where(x => !x.Groups["notify"].Success).Select(x => x.Groups["user"].Value);
                return users.ToList();
            }

            return Enumerable.Empty<string>();
        }
    }
}
