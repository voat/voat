/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat, Inc.
All Rights Reserved.
*/

using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Web;
using Voat.Caching;
using Voat.Data;
//using Microsoft.AspNet.SignalR;
//using Voat.Data.Models;
using Voat.Data.Models;
using Voat.Domain;
using Voat.Utilities;

namespace Voat.Utilities.Components
{
    public static class NotificationManager
    {
        public static async Task SendUserMentionNotification(string user, Comment comment)
        {
            if (comment != null)
            {
                if (!UserHelper.UserExists(user))
                {
                    return;
                }
                try
                {
                    string recipient = UserHelper.OriginalUsername(user);

                    //BlockedUser Implementation - Comment User Mention
                    if (!MesssagingUtility.IsSenderBlocked(comment.UserName, recipient))
                    {

                        using (var _db = new voatEntities())
                        {
                            var submission = DataCache.Submission.Retrieve(comment.SubmissionID);
                            var subverse = DataCache.Subverse.Retrieve(submission.Subverse);

                            var message = new Domain.Models.Message();

                            message.IsAnonymized = comment.IsAnonymized;

                            message.Recipient = recipient;
                            message.RecipientType = Domain.Models.MessageIdentityType.User;

                            message.Sender = comment.UserName;
                            message.SenderType = Domain.Models.MessageIdentityType.User;

                            message.Subverse = subverse.Name;
                            message.SubmissionID = submission.ID;
                            message.CommentID = comment.ID;

                            message.Type = Domain.Models.MessageType.CommentMention;
                            message.Direction = Domain.Models.MessageDirection.InBound;

                            message.CreationDate = Repository.CurrentDate;

                            _db.Messages.Add(message.Map());

                            //OLD code
                            //var commentReplyNotification = new CommentReplyNotification();

                            //commentReplyNotification.CommentID = comment.ID;
                            //commentReplyNotification.SubmissionID = comment.SubmissionID.Value;
                            //commentReplyNotification.Recipient = recipient;

                            //if (submission.IsAnonymized || subverse.IsAnonymized)
                            //{
                            //    commentReplyNotification.Sender = (new Random()).Next(10000, 20000).ToString(CultureInfo.InvariantCulture);
                            //}
                            //else
                            //{
                            //    commentReplyNotification.Sender = comment.UserName;
                            //}
                            //commentReplyNotification.Body = comment.Content;
                            //commentReplyNotification.Subverse = subverse.Name;
                            //commentReplyNotification.IsUnread = true;
                            //commentReplyNotification.CreationDate = Repository.CurrentDate;

                            //commentReplyNotification.Subject = String.Format("@{0} mentioned you in a comment", comment.UserName, submission.Title);

                            //_db.CommentReplyNotifications.Add(commentReplyNotification);

                            await _db.SaveChangesAsync();
                        }

                        EventNotification.Instance.SendMentionNotice(recipient, comment.UserName, Domain.Models.ContentType.Comment, comment.ID, comment.Content);
                    }
                }
                catch (Exception ex) {
                    throw ex;
                }
            }
        }

        public static async Task SendUserMentionNotification(string user, Submission submission)
        {
            if (submission != null)
            {
                if (!UserHelper.UserExists(user))
                {
                    return;
                }
                try { 
                    string recipient = UserHelper.OriginalUsername(user);

                    //BlockedUser Implementation - Submission User Mention
                    if (!MesssagingUtility.IsSenderBlocked(submission.UserName, recipient))
                    {
                        using (var _db = new voatEntities())
                        {
                            var subverse = DataCache.Subverse.Retrieve(submission.Subverse);

                            var message = new Domain.Models.Message();

                            message.IsAnonymized = submission.IsAnonymized;

                            message.Recipient = recipient;
                            message.RecipientType = Domain.Models.MessageIdentityType.User;

                            message.Sender = submission.UserName;
                            message.SenderType = Domain.Models.MessageIdentityType.User;

                            message.Subverse = subverse.Name;
                            message.SubmissionID = submission.ID;

                            message.Type = Domain.Models.MessageType.SubmissionMention;
                            message.Direction = Domain.Models.MessageDirection.InBound;

                            message.CreationDate = Repository.CurrentDate;

                            _db.Messages.Add(message.Map());

                            //OLD CODE

                            //var commentReplyNotification = new CommentReplyNotification();
                            //commentReplyNotification.SubmissionID = submission.ID;
                            //commentReplyNotification.Recipient = recipient;
                            //if (submission.IsAnonymized || subverse.IsAnonymized)
                            //{
                            //    commentReplyNotification.Sender = (new Random()).Next(10000, 20000).ToString(CultureInfo.InvariantCulture);
                            //}
                            //else
                            //{
                            //    commentReplyNotification.Sender = submission.UserName;
                            //}
                            //commentReplyNotification.Body = submission.Content;
                            //commentReplyNotification.Subverse = subverse.Name;
                            //commentReplyNotification.IsUnread = true;
                            //commentReplyNotification.CreationDate = Repository.CurrentDate;

                            //commentReplyNotification.Subject = String.Format("@{0} mentioned you in post '{1}'", submission.UserName, submission.Title);

                            //_db.CommentReplyNotifications.Add(commentReplyNotification);
                            await _db.SaveChangesAsync();
                        }

                        EventNotification.Instance.SendMentionNotice(recipient, submission.UserName, Domain.Models.ContentType.Submission, submission.ID, submission.Content);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static async Task SendCommentReplyNotification(Comment comment)
        {
            try
            {
                using (var _db = new voatEntities())
                {
                    Random _rnd = new Random();

                    if (comment.ParentID != null && comment.Content != null)
                    {
                        // find the parent comment and its author
                        var parentComment = _db.Comments.Find(comment.ParentID);
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
                                        var submission = DataCache.Submission.Retrieve(comment.SubmissionID);
                                        if (submission != null)
                                        {
                                            var subverse = DataCache.Subverse.Retrieve(submission.Subverse);

                                            var message = new Domain.Models.Message();

                                            message.IsAnonymized = submission.IsAnonymized;

                                            message.Recipient = parentComment.UserName;
                                            message.RecipientType = Domain.Models.MessageIdentityType.User;

                                            message.Sender = submission.UserName;
                                            message.SenderType = Domain.Models.MessageIdentityType.User;

                                            message.Subverse = subverse.Name;
                                            message.SubmissionID = submission.ID;
                                            message.CommentID = comment.ID;

                                            message.Type = Domain.Models.MessageType.CommentReply;
                                            message.Direction = Domain.Models.MessageDirection.InBound;

                                            message.CreationDate = Repository.CurrentDate;

                                            _db.Messages.Add(message.Map());

                                            //OLD CODE
                                            //var commentReplyNotification = new CommentReplyNotification();
                                            //commentReplyNotification.CommentID = comment.ID;
                                            //commentReplyNotification.SubmissionID = submission.ID;
                                            //commentReplyNotification.Recipient = parentComment.UserName;
                                            //if (submission.IsAnonymized || subverse.IsAnonymized)
                                            //{
                                            //    commentReplyNotification.Sender = _rnd.Next(10000, 20000).ToString(CultureInfo.InvariantCulture);
                                            //}
                                            //else
                                            //{
                                            //    commentReplyNotification.Sender = comment.UserName;
                                            //}
                                            //commentReplyNotification.Body = comment.Content.SubstringMax(400);
                                            //commentReplyNotification.Subverse = subverse.Name;
                                            //commentReplyNotification.IsUnread = true;
                                            //commentReplyNotification.CreationDate = Repository.CurrentDate;

                                            //// self = type 1, url = type 2
                                            //commentReplyNotification.Subject = submission.Title;

                                            //_db.CommentReplyNotifications.Add(commentReplyNotification);

                                            await _db.SaveChangesAsync();

                                            EventNotification.Instance.SendMessageNotice(message.Recipient, message.Sender, Domain.Models.MessageTypeFlag.CommentReply, Domain.Models.ContentType.Comment, comment.ID);

                                            //OLD CODE
                                            //EventNotification.Instance.SendMessageNotice(commentReplyNotification.Recipient, commentReplyNotification.Sender, Domain.Models.MessageTypeFlag.Comment, Domain.Models.ContentType.Comment, comment.ID);

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
        public static async Task SendSubmissionReplyNotification(Comment comment)
        {
            try
            {
                using (var _db = new voatEntities())
                {
                    // comment reply is sent to a root comment which has no parent id, trigger post reply notification
                    var submission = DataCache.Submission.Retrieve(comment.SubmissionID);
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
                                    message.RecipientType = Domain.Models.MessageIdentityType.User;

                                    message.Sender = comment.UserName;
                                    message.SenderType = Domain.Models.MessageIdentityType.User;

                                    message.Subverse = submission.Subverse;
                                    message.SubmissionID = submission.ID;
                                    message.CommentID = comment.ID;

                                    message.Type = Domain.Models.MessageType.SubmissionReply;
                                    message.Direction = Domain.Models.MessageDirection.InBound;

                                    message.CreationDate = Repository.CurrentDate;

                                    _db.Messages.Add(message.Map());

                                    //OLD CODE
                                    //// send the message
                                    //var postReplyNotification = new SubmissionReplyNotification();

                                    //postReplyNotification.CommentID = comment.ID;
                                    //postReplyNotification.SubmissionID = submission.ID;
                                    //postReplyNotification.Recipient = submission.UserName;
                                    //var subverse = DataCache.Subverse.Retrieve(submission.Subverse);
                                    //if (submission.IsAnonymized || subverse.IsAnonymized)
                                    //{
                                    //    postReplyNotification.Sender = _rnd.Next(10000, 20000).ToString(CultureInfo.InvariantCulture);
                                    //}
                                    //else
                                    //{
                                    //    postReplyNotification.Sender = comment.UserName;
                                    //}

                                    //postReplyNotification.Body = comment.Content.SubstringMax(400);
                                    //postReplyNotification.Subverse = submission.Subverse;
                                    //postReplyNotification.IsUnread = true;
                                    //postReplyNotification.CreationDate = Repository.CurrentDate;

                                    //// self = type 1, url = type 2
                                    //postReplyNotification.Subject = submission.Title;

                                    //_db.SubmissionReplyNotifications.Add(postReplyNotification);

                                    await _db.SaveChangesAsync();

                                    EventNotification.Instance.SendMessageNotice(message.Recipient, message.Sender, Domain.Models.MessageTypeFlag.CommentReply, Domain.Models.ContentType.Comment, comment.ID);
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


        public static async Task SendCommentNotification(Comment comment)
        {
            if (comment.ParentID != null && comment.Content != null)
            {
                await SendCommentReplyNotification(comment);         
            }
            else
            {
                await SendSubmissionReplyNotification(comment);
            }
        }
    }
}