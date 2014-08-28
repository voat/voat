/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
*/

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using Whoaverse.Models;
using Whoaverse.Models.ViewModels;

namespace Whoaverse.Utils
{
    public static class User
    {
        // check if user exists in database
        public static bool UserExists(string userName)
        {
            using (UserManager<ApplicationUser> tmpUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
            {
                var tmpuser = tmpUserManager.FindByName(userName);
                if (tmpuser != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        // return user registration date
        public static DateTime GetUserRegistrationDateTime(string userName)
        {
            using (UserManager<ApplicationUser> tmpUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
            {
                var tmpuser = tmpUserManager.FindByName(userName);
                if (tmpuser != null)
                {
                    return tmpuser.RegistrationDateTime;
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }

        // delete a user account and all history: comments, posts and votes
        public static bool DeleteUser(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                using (UserManager<ApplicationUser> tmpUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
                {
                    var tmpuser = tmpUserManager.FindByName(userName);
                    if (tmpuser != null)
                    {
                        //remove voting history for submisions
                        db.Votingtrackers.RemoveRange(db.Votingtrackers.Where(x => x.UserName == userName));

                        //remove voting history for comments
                        db.Commentvotingtrackers.RemoveRange(db.Commentvotingtrackers.Where(x => x.UserName == userName));

                        //remove all comments
                        var comments = db.Comments.Where(c => c.Name == userName);
                        foreach (Comment c in comments)
                        {
                            c.Name = "deleted";
                            c.CommentContent = "deleted by user";
                            db.SaveChangesAsync();
                        }

                        //remove all submissions
                        var submissions = db.Messages.Where(c => c.Name == userName);
                        foreach (Message s in submissions)
                        {
                            if (s.Type == 1)
                            {
                                s.Name = "deleted";
                                s.MessageContent = "deleted by user";
                                s.Title = "deleted by user";
                            }
                            else
                            {
                                s.Name = "deleted";
                                s.Linkdescription = "deleted by user";
                                s.MessageContent = "http://whoaverse.com";
                            }
                        }
                        db.SaveChangesAsync();

                        var result = tmpUserManager.DeleteAsync(tmpuser);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
        }

        // check if given user is the owner for a given subverse
        public static bool IsUserSubverseAdmin(string userName, string subverse)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var subverseOwner = db.SubverseAdmins.Where(n => n.SubverseName.Equals(subverse, StringComparison.OrdinalIgnoreCase) && n.Power == 1).FirstOrDefault();
                if (subverseOwner != null && subverseOwner.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                };
            }
        }

        // check if given user is moderator for a given subverse
        public static bool IsUserSubverseModerator(string userName, string subverse)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var subverseModerator = db.SubverseAdmins.Where(n => n.SubverseName.Equals(subverse, StringComparison.OrdinalIgnoreCase) && n.Username.Equals(userName, StringComparison.OrdinalIgnoreCase) && n.Power == 2).FirstOrDefault();
                if (subverseModerator != null && subverseModerator.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                };
            }
        }

        // check if given user is subscribed to a given subverse
        public static bool IsUserSubverseSubscriber(string userName, string subverse)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var subverseSubscriber = db.Subscriptions.Where(n => n.SubverseName.ToLower() == subverse.ToLower() && n.Username == userName).FirstOrDefault();
                if (subverseSubscriber != null)
                {
                    return true;
                }
                else
                {
                    return false;
                };
            }
        }

        // subscribe to a subverse
        public static void SubscribeToSubverse(string userName, string subverse)
        {
            if (!IsUserSubverseSubscriber(userName, subverse))
            {
                using (whoaverseEntities db = new whoaverseEntities())
                {
                    // add a new subscription
                    Subscription newSubscription = new Subscription();
                    newSubscription.Username = userName;
                    newSubscription.SubverseName = subverse;
                    db.Subscriptions.Add(newSubscription);

                    // record new subscription in subverse table subscribers field
                    Subverse tmpSubverse = db.Subverses.Find(subverse);

                    if (tmpSubverse != null)
                    {
                        tmpSubverse.subscribers++;
                    }

                    db.SaveChanges();
                }
            }
        }

        // unsubscribe from a subverse
        public static void UnSubscribeFromSubverse(string userName, string subverse)
        {
            if (IsUserSubverseSubscriber(userName, subverse))
            {
                using (whoaverseEntities db = new whoaverseEntities())
                {
                    var subscription = db.Subscriptions
                                .Where(b => b.Username == userName && b.SubverseName == subverse)
                                .FirstOrDefault();

                    if (subverse != null)
                    {
                        // remove subscription record
                        db.Subscriptions.Remove(subscription);

                        // record new unsubscription in subverse table subscribers field
                        Subverse tmpSubverse = db.Subverses.Find(subverse);

                        if (tmpSubverse != null)
                        {
                            tmpSubverse.subscribers--;
                        }

                        db.SaveChanges();
                    }

                }
            }
        }

        // return subscription count for a given user
        public static int SubscriptionCount(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                return db.Subscriptions
                                    .Where(r => r.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
                                    .Count();
            }
        }

        // return a list of subverses user is subscribed to
        public static List<SubverseDetailsViewModel> UserSubscriptions(string userName)
        {
            // get a list of subcribed subverses with details and order by subverse names, ascending
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var subscribedSubverses = from c in db.Subverses
                                          join a in db.Subscriptions
                                          on c.name equals a.SubverseName
                                          where a.Username.Equals(userName)
                                          orderby a.SubverseName ascending
                                          select new SubverseDetailsViewModel
                                          {
                                              Name = c.name
                                          };

                return subscribedSubverses.ToList();
            }
        }

        // return a list of user badges
        public static List<Userbadge> UserBadges(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                return db.Userbadges.Include("Badge")
                    .Where(r => r.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        // check if given user has unread private messages, not including messages manually marked as unread
        public static bool UserHasNewMessages(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var privateMessages = db.Privatemessages
                        .Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(s => s.Timestamp)
                        .ThenBy(s => s.Sender)
                        .ToList();

                var commentReplies = db.Commentreplynotifications
                        .Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(s => s.Timestamp)
                        .ThenBy(s => s.Sender)
                        .ToList();

                var postReplies = db.Postreplynotifications
                        .Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(s => s.Timestamp)
                        .ThenBy(s => s.Sender)
                        .ToList();

                if (privateMessages.Count() > 0 || commentReplies.Count() > 0 || postReplies.Count() > 0)
                {
                    var unreadPrivateMessages = privateMessages
                        .Where(s => s.Status == true && s.Markedasunread == false).ToList();

                    var unreadCommentReplies = commentReplies
                        .Where(s => s.Status == true && s.Markedasunread == false).ToList();

                    var unreadPostReplies = postReplies
                        .Where(s => s.Status == true && s.Markedasunread == false).ToList();

                    if (unreadPrivateMessages.Count > 0 || unreadCommentReplies.Count > 0 || unreadPostReplies.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        // check if given user has unread comment replies and return the count
        public static int UnreadCommentRepliesCount(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var commentReplies = db.Commentreplynotifications
                        .Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(s => s.Timestamp)
                        .ThenBy(s => s.Sender)
                        .ToList();

                if (commentReplies.Count() > 0)
                {

                    var unreadCommentReplies = commentReplies
                        .Where(s => s.Status == true && s.Markedasunread == false).ToList();

                    if (unreadCommentReplies.Count > 0)
                    {
                        return unreadCommentReplies.Count;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        // check if given user has unread post replies and return the count
        public static int UnreadPostRepliesCount(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var postReplies = db.Postreplynotifications
                        .Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(s => s.Timestamp)
                        .ThenBy(s => s.Sender)
                        .ToList();

                if (postReplies.Count() > 0)
                {
                    var unreadPostReplies = postReplies
                        .Where(s => s.Status == true && s.Markedasunread == false).ToList();

                    if (unreadPostReplies.Count > 0)
                    {
                        return unreadPostReplies.Count;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        // check if given user has unread private messages and return the count
        public static int UnreadPrivateMessagesCount(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var privateMessages = db.Privatemessages
                        .Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(s => s.Timestamp)
                        .ThenBy(s => s.Sender)
                        .ToList();

                if (privateMessages.Count() > 0)
                {
                    var unreadPrivateMessages = privateMessages
                        .Where(s => s.Status == true && s.Markedasunread == false).ToList();

                    if (unreadPrivateMessages.Count > 0)
                    {
                        return unreadPrivateMessages.Count;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        // check if a given user does not want to see custom CSS styles
        public static bool CustomCSSDisabledForUser(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var result = db.Userpreferences.Find(userName);
                if (result != null)
                {
                    return result.Disable_custom_css;
                }
                else
                {
                    return false;
                }
            }
        }

        // check if a given user wants to open links in new window
        public static bool LinksInNewWindow(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var result = db.Userpreferences.Find(userName);
                if (result != null)
                {
                    return result.Clicking_mode;
                }
                else
                {
                    return false;
                }
            }
        }

        // check how many votes a user has used in the past 24 hours
        // TODO: this is executed 25 times for frontpage, needs to be redesigned as follows:
        // - only call this function if user is attempting to vote
        // - if voting fails, display modal dialog showing why voting failed (example: too many votes in past 24hrs)

        public static int TotalVotesUsedInPast24Hours(string userName)
        {
            int commentVotesUsedInPast24Hrs = 0;
            int submissionVotesUsedInPast24Hrs = 0;

            var startDate = DateTime.Now.Add(new TimeSpan(0, -24, 0, 0, 0));

            using (whoaverseEntities db = new whoaverseEntities())
            {
                // calculate how many comment votes user made in the past 24 hours
                var commentVotesUsedToday = db.Commentvotingtrackers
                    .Where(c => c.Timestamp >= startDate && c.Timestamp <= DateTime.Now && c.UserName == userName);

                commentVotesUsedInPast24Hrs = commentVotesUsedToday.Count();

                // calculate how many submission votes user made in the past 24 hours
                var submissionVotesUsedToday = db.Votingtrackers
                    .Where(c => c.Timestamp >= startDate && c.Timestamp <= DateTime.Now && c.UserName == userName);

                submissionVotesUsedInPast24Hrs = submissionVotesUsedToday.Count();
            }

            return (commentVotesUsedInPast24Hrs + submissionVotesUsedInPast24Hrs);
        }

        // 5 subverses user submitted to most
        public static UserStatsModel userStatsModel(string userName)
        {
            UserStatsModel userStatsModel = new UserStatsModel();

            using (whoaverseEntities db = new whoaverseEntities())
            {
                var subverses = db.Messages.Where(a => a.Name == userName)
                         .GroupBy(a => new { a.Name, a.Subverse })
                         .Select(g => new SubverseStats { SubverseName = g.Key.Subverse, Count = g.Count() })
                         .OrderByDescending (s => s.Count)
                         .Take(5)
                         .ToList();

                var linkSubmissionsCount = db.Messages.Where(a => a.Name == userName && a.Type == 2).Count();
                var messageSubmissionsCount = db.Messages.Where(a => a.Name == userName && a.Type == 1).Count();
                
                //todo: top rated submissions

                userStatsModel.TopSubversesUserContributedTo = subverses;
                userStatsModel.LinkSubmissionsSubmitted = linkSubmissionsCount;
                userStatsModel.MessageSubmissionsSubmitted = messageSubmissionsCount;
            }

            return userStatsModel;
        }

        // check if a given user has used his daily posting quota
        public static bool UserDailyPostingQuotaUsed(string userName)
        {
            // if user is a new user with low CCP threshold
            int dailyPostingQuota = 10;

            // if user is old user with high CCP threshold
            dailyPostingQuota = 100;

            // check how many submission user made today            

            throw new NotImplementedException();
        }
    }
}