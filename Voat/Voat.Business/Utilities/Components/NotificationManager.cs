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
//using Microsoft.AspNet.SignalR;
//using Voat.Data.Models;
using Voat.Data.Models;

namespace Voat.Utilities.Components
{
    public static class NotificationManager
    {
        public static async Task SendUserMentionNotification(string user, Comment comment, Action<string> onSuccess)
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

                    var commentReplyNotification = new CommentReplyNotification();
                    using (var _db = new voatEntities())
                    {
                        var submission = DataCache.Submission.Retrieve(comment.SubmissionID);
                        var subverse = DataCache.Subverse.Retrieve(submission.Subverse);

                        commentReplyNotification.CommentID = comment.ID;
                        commentReplyNotification.SubmissionID = comment.SubmissionID.Value;
                        commentReplyNotification.Recipient = recipient;
                        if (submission.IsAnonymized || subverse.IsAnonymized)
                        {
                            commentReplyNotification.Sender = (new Random()).Next(10000, 20000).ToString(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            commentReplyNotification.Sender = comment.UserName;
                        }
                        commentReplyNotification.Body = comment.Content;
                        commentReplyNotification.Subverse = subverse.Name;
                        commentReplyNotification.IsUnread = true;
                        commentReplyNotification.CreationDate = DateTime.Now;

                        commentReplyNotification.Subject = String.Format("@{0} mentioned you in a comment", comment.UserName, submission.Title);

                        _db.CommentReplyNotifications.Add(commentReplyNotification);
                       
                        await _db.SaveChangesAsync();
                    }

                    if (onSuccess != null)
                    {
                        onSuccess(recipient);
                    }
                }
                catch (Exception ex) {
                    throw ex;
                }
            }
        }

        public static async Task SendUserMentionNotification(string user, Submission submission, Action<string> onSuccess)
        {
            if (submission != null)
            {
                if (!UserHelper.UserExists(user))
                {
                    return;
                }
                try { 
                    string recipient = UserHelper.OriginalUsername(user);

                    var commentReplyNotification = new CommentReplyNotification();
                    using (var _db = new voatEntities())
                    {
                    
                        var subverse = DataCache.Subverse.Retrieve(submission.Subverse);

                        commentReplyNotification.SubmissionID = submission.ID;
                        commentReplyNotification.Recipient = recipient;
                        if (submission.IsAnonymized || subverse.IsAnonymized)
                        {
                            commentReplyNotification.Sender = (new Random()).Next(10000, 20000).ToString(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            commentReplyNotification.Sender = submission.UserName;
                        }
                        commentReplyNotification.Body = submission.Content;
                        commentReplyNotification.Subverse = subverse.Name;
                        commentReplyNotification.IsUnread = true;
                        commentReplyNotification.CreationDate = DateTime.Now;

                        commentReplyNotification.Subject = String.Format("@{0} mentioned you in post '{1}'", submission.UserName, submission.Title);

                        _db.CommentReplyNotifications.Add(commentReplyNotification);
                        await _db.SaveChangesAsync();
                    }

                    if (onSuccess != null)
                    {
                        onSuccess(recipient);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static async Task SendCommentNotification(Comment comment, Action<string> onSuccess)
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
                                if (parentComment.UserName != HttpContext.Current.User.Identity.Name)
                                {
                                    // send the message

                                    var submission = DataCache.Submission.Retrieve(comment.SubmissionID);
                                    if (submission != null)
                                    {
                                        var subverse = DataCache.Subverse.Retrieve(submission.Subverse);

                                        var commentReplyNotification = new CommentReplyNotification();
                                        commentReplyNotification.CommentID = comment.ID;
                                        commentReplyNotification.SubmissionID = submission.ID;
                                        commentReplyNotification.Recipient = parentComment.UserName;
                                        if (submission.IsAnonymized || subverse.IsAnonymized)
                                        {
                                            commentReplyNotification.Sender = _rnd.Next(10000, 20000).ToString(CultureInfo.InvariantCulture);
                                        }
                                        else
                                        {
                                            commentReplyNotification.Sender = HttpContext.Current.User.Identity.Name;
                                        }
                                        commentReplyNotification.Body = comment.Content;
                                        commentReplyNotification.Subverse = subverse.Name;
                                        commentReplyNotification.IsUnread = true;
                                        commentReplyNotification.CreationDate = DateTime.Now;

                                        // self = type 1, url = type 2
                                        commentReplyNotification.Subject = submission.Type == 1 ? submission.Title : submission.LinkDescription;

                                        _db.CommentReplyNotifications.Add(commentReplyNotification);

                                        await _db.SaveChangesAsync();

                                        if (onSuccess != null)
                                        {
                                            onSuccess(commentReplyNotification.Recipient);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // comment reply is sent to a root comment which has no parent id, trigger post reply notification
                        var submission = DataCache.Submission.Retrieve(comment.SubmissionID);
                        if (submission != null)
                        {
                            // check if recipient exists
                            if (UserHelper.UserExists(submission.UserName))
                            {
                                // do not send notification if author is the same as comment author
                                if (submission.UserName != HttpContext.Current.User.Identity.Name)
                                {
                                    // send the message
                                    var postReplyNotification = new SubmissionReplyNotification();

                                    postReplyNotification.CommentID = comment.ID;
                                    postReplyNotification.SubmissionID = submission.ID;
                                    postReplyNotification.Recipient = submission.UserName;
                                    var subverse = DataCache.Subverse.Retrieve(submission.Subverse);
                                    if (submission.IsAnonymized || subverse.IsAnonymized)
                                    {
                                        postReplyNotification.Sender = _rnd.Next(10000, 20000).ToString(CultureInfo.InvariantCulture);
                                    }
                                    else
                                    {
                                        postReplyNotification.Sender = HttpContext.Current.User.Identity.Name;
                                    }

                                    postReplyNotification.Body = comment.Content;
                                    postReplyNotification.Subverse = submission.Subverse;
                                    postReplyNotification.IsUnread = true;
                                    postReplyNotification.CreationDate = DateTime.Now;

                                    // self = type 1, url = type 2
                                    postReplyNotification.Subject = submission.Type == 1 ? submission.Title : submission.LinkDescription;

                                    _db.SubmissionReplyNotifications.Add(postReplyNotification);

                                    await _db.SaveChangesAsync();

                                    if (onSuccess != null)
                                    {
                                        onSuccess(postReplyNotification.Recipient);
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
    }
}