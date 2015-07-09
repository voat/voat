﻿/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2014 Voat
All Rights Reserved.
*/

using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Voat.Models;

namespace Voat.Utils.Components
{
    public static class NotificationManager
    {
        public static async Task SendUserMentionNotification(string user, Comment comment)
        {
            if (comment != null)
            {
                if (!User.UserExists(user))
                {
                    return;
                }

                string recipient = User.OriginalUsername(user);

                var commentReplyNotification = new Commentreplynotification();
                using (var _db = new whoaverseEntities())
                {

                    commentReplyNotification.CommentId = comment.Id;
                    commentReplyNotification.SubmissionId = comment.Message.Id;
                    commentReplyNotification.Recipient = recipient;
                    if (comment.Message.Anonymized || comment.Message.Subverses.anonymized_mode)
                    {
                        commentReplyNotification.Sender = (new Random()).Next(10000, 20000).ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        commentReplyNotification.Sender = comment.Name;
                    }
                    commentReplyNotification.Body = comment.CommentContent;
                    commentReplyNotification.Subverse = comment.Message.Subverse;
                    commentReplyNotification.Status = true;
                    commentReplyNotification.Timestamp = DateTime.Now;

                    commentReplyNotification.Subject = String.Format("@{0} mentioned you in a comment", comment.Name, comment.Message.Title);

                    _db.Commentreplynotifications.Add(commentReplyNotification);
                    await _db.SaveChangesAsync();
                }

                // get count of unread notifications
                int unreadNotifications = User.UnreadTotalNotificationsCount(commentReplyNotification.Recipient);

                // send SignalR realtime notification to recipient
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();
                hubContext.Clients.User(commentReplyNotification.Recipient).setNotificationsPending(unreadNotifications);
            }
        }

        public static async Task SendUserMentionNotification(string user, Message message)
        {
            if (message != null)
            {
                if (!User.UserExists(user))
                {
                    return;
                }

                string recipient = User.OriginalUsername(user);

                var commentReplyNotification = new Commentreplynotification();
                using (var _db = new whoaverseEntities())
                {
                    //commentReplyNotification.CommentId = comment.Id;
                    commentReplyNotification.SubmissionId = message.Id;
                    commentReplyNotification.Recipient = recipient;
                    if (message.Anonymized || message.Subverses.anonymized_mode)
                    {
                        commentReplyNotification.Sender = (new Random()).Next(10000, 20000).ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        commentReplyNotification.Sender = message.Name;
                    }
                    commentReplyNotification.Body = message.MessageContent;
                    commentReplyNotification.Subverse = message.Subverse;
                    commentReplyNotification.Status = true;
                    commentReplyNotification.Timestamp = DateTime.Now;

                    commentReplyNotification.Subject = String.Format("@{0} mentioned you in post '{1}'", message.Name, message.Title);

                    _db.Commentreplynotifications.Add(commentReplyNotification);
                    await _db.SaveChangesAsync();
                }

                // get count of unread notifications
                int unreadNotifications = User.UnreadTotalNotificationsCount(commentReplyNotification.Recipient);

                // send SignalR realtime notification to recipient
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();
                hubContext.Clients.User(commentReplyNotification.Recipient).setNotificationsPending(unreadNotifications);
            }
        }

        public static async Task SendCommentNotification(Comment comment)
        {
            using (var _db = new whoaverseEntities())
            {
                Random _rnd = new Random();

                if (comment.ParentId != null && comment.CommentContent != null)
                {
                    // find the parent comment and its author
                    var parentComment = _db.Comments.Find(comment.ParentId);
                    if (parentComment != null)
                    {
                        // check if recipient exists
                        if (User.UserExists(parentComment.Name))
                        {
                            // do not send notification if author is the same as comment author
                            if (parentComment.Name != HttpContext.Current.User.Identity.Name)
                            {
                                // send the message

                                var commentMessage = _db.Messages.Find(comment.MessageId);
                                if (commentMessage != null)
                                {
                                    var commentReplyNotification = new Commentreplynotification();
                                    commentReplyNotification.CommentId = comment.Id;
                                    commentReplyNotification.SubmissionId = commentMessage.Id;
                                    commentReplyNotification.Recipient = parentComment.Name;
                                    if (parentComment.Message.Anonymized || parentComment.Message.Subverses.anonymized_mode)
                                    {
                                        commentReplyNotification.Sender = _rnd.Next(10000, 20000).ToString(CultureInfo.InvariantCulture);
                                    }
                                    else
                                    {
                                        commentReplyNotification.Sender = HttpContext.Current.User.Identity.Name;
                                    }
                                    commentReplyNotification.Body = comment.CommentContent;
                                    commentReplyNotification.Subverse = commentMessage.Subverse;
                                    commentReplyNotification.Status = true;
                                    commentReplyNotification.Timestamp = DateTime.Now;

                                    // self = type 1, url = type 2
                                    commentReplyNotification.Subject = parentComment.Message.Type == 1 ? parentComment.Message.Title : parentComment.Message.Linkdescription;

                                    _db.Commentreplynotifications.Add(commentReplyNotification);

                                    await _db.SaveChangesAsync();

                                    // get count of unread notifications
                                    int unreadNotifications = User.UnreadTotalNotificationsCount(commentReplyNotification.Recipient);

                                    // send SignalR realtime notification to recipient
                                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();
                                    hubContext.Clients.User(commentReplyNotification.Recipient).setNotificationsPending(unreadNotifications);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // comment reply is sent to a root comment which has no parent id, trigger post reply notification
                    var commentMessage = _db.Messages.Find(comment.MessageId);
                    if (commentMessage != null)
                    {
                        // check if recipient exists
                        if (User.UserExists(commentMessage.Name))
                        {
                            // do not send notification if author is the same as comment author
                            if (commentMessage.Name != HttpContext.Current.User.Identity.Name)
                            {
                                // send the message
                                var postReplyNotification = new Postreplynotification();

                                postReplyNotification.CommentId = comment.Id;
                                postReplyNotification.SubmissionId = commentMessage.Id;
                                postReplyNotification.Recipient = commentMessage.Name;

                                if (commentMessage.Anonymized || commentMessage.Subverses.anonymized_mode)
                                {
                                    postReplyNotification.Sender = _rnd.Next(10000, 20000).ToString(CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    postReplyNotification.Sender = HttpContext.Current.User.Identity.Name;
                                }

                                postReplyNotification.Body = comment.CommentContent;
                                postReplyNotification.Subverse = commentMessage.Subverse;
                                postReplyNotification.Status = true;
                                postReplyNotification.Timestamp = DateTime.Now;

                                // self = type 1, url = type 2
                                postReplyNotification.Subject = commentMessage.Type == 1 ? commentMessage.Title : commentMessage.Linkdescription;

                                _db.Postreplynotifications.Add(postReplyNotification);

                                await _db.SaveChangesAsync();

                                // get count of unread notifications
                                int unreadNotifications = User.UnreadTotalNotificationsCount(postReplyNotification.Recipient);

                                // send SignalR realtime notification to recipient
                                var hubContext = GlobalHost.ConnectionManager.GetHubContext<MessagingHub>();
                                hubContext.Clients.User(postReplyNotification.Recipient).setNotificationsPending(unreadNotifications);
                            }
                        }
                    }
                }
            }
        }
    }
}