/*
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

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Voat.Models;
using Voat.Models.ViewModels;

namespace Voat.Utils
{
    public static class User
    {
        // check if user exists in database
        public static bool UserExists(string userName)
        {
            using (var tmpUserManager = new UserManager<WhoaVerseUser>(new UserStore<WhoaVerseUser>(new ApplicationDbContext())))
            {
                var tmpuser = tmpUserManager.FindByName(userName);
                return tmpuser != null;
            }
        }

        // return original username
        public static string OriginalUsername(string userName)
        {
            using (var tmpUserManager = new UserManager<WhoaVerseUser>(new UserStore<WhoaVerseUser>(new ApplicationDbContext())))
            {
                var tmpuser = tmpUserManager.FindByName(userName);
                return tmpuser != null ? tmpuser.UserName : null;
            }
        }

        // return user registration date
        public static DateTime GetUserRegistrationDateTime(string userName)
        {
            using (var tmpUserManager = new UserManager<WhoaVerseUser>(new UserStore<WhoaVerseUser>(new ApplicationDbContext())))
            {
                var tmpuser = tmpUserManager.FindByName(userName);
                return tmpuser != null ? tmpuser.RegistrationDateTime : DateTime.MinValue;
            }
        }

        // delete a user account and all history: comments, posts and votes
        public static bool DeleteUser(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                using (var tmpUserManager = new UserManager<WhoaVerseUser>(new UserStore<WhoaVerseUser>(new ApplicationDbContext())))
                {
                    var tmpuser = tmpUserManager.FindByName(userName);
                    if (tmpuser != null)
                    {
                        // remove voting history for submisions
                        db.Votingtrackers.RemoveRange(db.Votingtrackers.Where(x => x.UserName == userName));

                        // remove voting history for comments
                        db.Commentvotingtrackers.RemoveRange(db.Commentvotingtrackers.Where(x => x.UserName == userName));

                        // remove all comments
                        var comments = db.Comments.Where(c => c.Name == userName).ToList();
                        foreach (Comment c in comments)
                        {
                            c.Name = "deleted";
                            c.CommentContent = "deleted by user";
                        }
                        db.SaveChanges();

                        // remove all submissions
                        var submissions = db.Messages.Where(c => c.Name == userName).ToList();
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
                                s.MessageContent = "http://voat.co";
                            }
                        }

                        // resign from all moderating positions
                        db.SubverseAdmins.RemoveRange(db.SubverseAdmins.Where(m => m.Username.Equals(userName, StringComparison.OrdinalIgnoreCase)));

                        // delete comment reply notifications
                        db.Commentreplynotifications.RemoveRange(db.Commentreplynotifications.Where(crp => crp.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase)));

                        // delete post reply notifications
                        db.Postreplynotifications.RemoveRange(db.Postreplynotifications.Where(prp => prp.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase)));

                        // delete private messages
                        db.Privatemessages.RemoveRange(db.Privatemessages.Where(pm => pm.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase)));

                        // TODO:
                        // keep this updated as new features are added (delete sets etc)

                        // username will stay permanently reserved

                        db.SaveChanges();

                        return true;
                    }
                    // user account could not be found
                    return false;
                }

            }
        }

        // check if given user is the owner for a given subverse
        public static bool IsUserSubverseAdmin(string userName, string subverse)
        {
            using (var db = new whoaverseEntities())
            {
                var subverseOwner = db.SubverseAdmins.FirstOrDefault(n => n.SubverseName.Equals(subverse, StringComparison.OrdinalIgnoreCase) && n.Power == 1);
                return subverseOwner != null && subverseOwner.Username.Equals(userName, StringComparison.OrdinalIgnoreCase);
            }
        }

        // check if given user is moderator for a given subverse
        public static bool IsUserSubverseModerator(string userName, string subverse)
        {
            using (var db = new whoaverseEntities())
            {
                var subverseModerator = db.SubverseAdmins.FirstOrDefault(n => n.SubverseName.Equals(subverse, StringComparison.OrdinalIgnoreCase) && n.Username.Equals(userName, StringComparison.OrdinalIgnoreCase) && n.Power == 2);

                return subverseModerator != null && subverseModerator.Username.Equals(userName, StringComparison.OrdinalIgnoreCase); ;
            }
        }

        // check if given user is subscribed to a given subverse
        public static bool IsUserSubverseSubscriber(string userName, string subverse)
        {
            using (var db = new whoaverseEntities())
            {
                var subverseSubscriber = db.Subscriptions.FirstOrDefault(n => n.SubverseName.ToLower() == subverse.ToLower() && n.Username == userName);
                return subverseSubscriber != null;
            }
        }

        // check if given user blocks a given subverse
        public static bool IsUserBlockingSubverse(string userName, string subverse)
        {
            using (var db = new whoaverseEntities())
            {
                var subverseBlock = db.UserBlockedSubverses.FirstOrDefault(n => n.SubverseName.ToLower() == subverse.ToLower() && n.Username == userName);
                return subverseBlock != null;
            }
        }

        // check if given user is subscribed to a given set
        public static bool IsUserSetSubscriber(string userName, int setId)
        {
            using (var db = new whoaverseEntities())
            {
                var setSubscriber = db.Usersetsubscriptions.FirstOrDefault(n => n.Set_id == setId && n.Username == userName);
                return setSubscriber != null;
            }
        }

        // subscribe to a subverse
        public static void SubscribeToSubverse(string userName, string subverse)
        {
            if (IsUserSubverseSubscriber(userName, subverse)) return;
            using (var db = new whoaverseEntities())
            {
                // add a new subscription
                var newSubscription = new Subscription { Username = userName, SubverseName = subverse };
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

        // unsubscribe from a subverse
        public static void UnSubscribeFromSubverse(string userName, string subverse)
        {
            if (IsUserSubverseSubscriber(userName, subverse))
            {
                using (var db = new whoaverseEntities())
                {
                    var subscription = db.Subscriptions.FirstOrDefault(b => b.Username == userName && b.SubverseName == subverse);

                    if (subverse == null) return;
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

        // return subscription count for a given user
        public static int SubscriptionCount(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                return db.Subscriptions.Count(s => s.Username.Equals(userName, StringComparison.OrdinalIgnoreCase));
            }
        }

        // return a list of subverses user is subscribed to
        public static List<SubverseDetailsViewModel> UserSubscriptions(string userName)
        {
            // get a list of subcribed subverses with details and order by subverse names, ascending
            using (var db = new whoaverseEntities())
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
            using (var db = new whoaverseEntities())
            {
                return db.Userbadges.Include("Badge")
                    .Where(r => r.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        // check if given user has unread private messages, not including messages manually marked as unread
        public static bool UserHasNewMessages(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                var unreadPrivateMessagesCount = db.Privatemessages.Count(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase) && s.Status && s.Markedasunread == false);
                var unreadCommentRepliesCount = db.Commentreplynotifications.Count(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase) && s.Status && s.Markedasunread == false);
                var unreadPostRepliesCount = db.Postreplynotifications.Count(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase) && s.Status && s.Markedasunread == false);

                return unreadPrivateMessagesCount > 0 || unreadCommentRepliesCount > 0 || unreadPostRepliesCount > 0;
            }
        }

        // check if given user has unread comment replies and return the count
        public static int UnreadCommentRepliesCount(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                var commentReplies = db.Commentreplynotifications
                    .Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(s => s.Timestamp)
                    .ThenBy(s => s.Sender);

                if (!commentReplies.Any()) return 0;

                var unreadCommentReplies = commentReplies.Where(s => s.Status && s.Markedasunread == false);
                return unreadCommentReplies.Any() ? unreadCommentReplies.Count() : 0;
            }
        }

        // check if given user has unread post replies and return the count
        public static int UnreadPostRepliesCount(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                var postReplies = db.Postreplynotifications
                    .Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(s => s.Timestamp)
                    .ThenBy(s => s.Sender);

                if (!postReplies.Any()) return 0;
                var unreadPostReplies = postReplies.Where(s => s.Status && s.Markedasunread == false);

                return unreadPostReplies.Any() ? unreadPostReplies.Count() : 0;
            }
        }

        // check if given user has unread private messages and return the count
        public static int UnreadPrivateMessagesCount(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                var privateMessages = db.Privatemessages
                    .Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(s => s.Timestamp)
                    .ThenBy(s => s.Sender);

                if (!privateMessages.Any()) return 0;
                var unreadPrivateMessages = privateMessages.Where(s => s.Status && s.Markedasunread == false);

                return unreadPrivateMessages.Any() ? unreadPrivateMessages.Count() : 0;
            }
        }

        // get total unread notifications count for a given user
        public static int UnreadTotalNotificationsCount(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                int totalCount = 0;
                int unreadPrivateMessagesCount = db.Privatemessages.Count(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase) && s.Status && s.Markedasunread == false);
                int unreadPostRepliesCount = db.Postreplynotifications.Count(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase) && s.Status && s.Markedasunread == false);
                int unreadCommentRepliesCount = db.Commentreplynotifications.Count(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase) && s.Status && s.Markedasunread == false);

                totalCount = totalCount + unreadPrivateMessagesCount + unreadPostRepliesCount + unreadCommentRepliesCount;
                return totalCount;
            }
        }

        // get total number of comment replies for a given user
        public static int CommentRepliesCount(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                var commentReplies = db.Commentreplynotifications.Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase));
                if (!commentReplies.Any()) return 0;
                return commentReplies.Any() ? commentReplies.Count() : 0;
            }
        }

        // get total number of post replies for a given user
        public static int PostRepliesCount(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                var postReplies = db.Postreplynotifications.Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase));
                if (!postReplies.Any()) return 0;
                return postReplies.Any() ? postReplies.Count() : 0;
            }
        }

        // get total number of private messages for a given user
        public static int PrivateMessageCount(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                var privateMessages = db.Privatemessages.Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase));
                if (!privateMessages.Any()) return 0;
                return privateMessages.Any() ? privateMessages.Count() : 0;
            }
        }

        // check if a given user does not want to see custom CSS styles
        public static bool CustomCssDisabledForUser(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                var result = db.Userpreferences.Find(userName);
                return result != null && result.Disable_custom_css;
            }
        }

        // check which theme style user selected
        public static string UserStylePreference(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                var result = db.Userpreferences.Find(userName);
                if (result != null)
                {
                    return result.Night_mode ? "dark" : "light";
                }
                return "light";
            }
        }

        // check if a given user wants to see NSFW (adult) content
        public static bool AdultContentEnabled(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                var result = db.Userpreferences.Find(userName);
                return result != null && result.Enable_adult_content;
            }
        }

        // check if a given user wants to open links in new window
        public static bool LinksInNewWindow(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                var result = db.Userpreferences.Find(userName);
                return result != null && result.Clicking_mode;
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

            using (var db = new whoaverseEntities())
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

        // return user statistics for user profile overview
        public static UserStatsModel UserStatsModel(string userName)
        {
            var userStatsModel = new UserStatsModel();

            using (var db = new whoaverseEntities())
            {
                // 5 subverses user submitted to most
                var subverses = db.Messages.Where(a => a.Name == userName && !a.Anonymized)
                         .GroupBy(a => new { a.Name, a.Subverse })
                         .Select(g => new SubverseStats { SubverseName = g.Key.Subverse, Count = g.Count() })
                         .OrderByDescending(s => s.Count)
                         .Take(5)
                         .ToList();

                // total comment count
                var comments = db.Comments.Count(a => a.Name == userName);

                // voting habits
                var commentUpvotes = db.Commentvotingtrackers.Count(a => a.UserName == userName && a.VoteStatus == 1);
                var commentDownvotes = db.Commentvotingtrackers.Count(a => a.UserName == userName && a.VoteStatus == -1);
                var submissionUpvotes = db.Votingtrackers.Count(a => a.UserName == userName && a.VoteStatus == 1);
                var submissionDownvotes = db.Votingtrackers.Count(a => a.UserName == userName && a.VoteStatus == -1);

                // get 3 highest rated comments
                var highestRatedComments = db.Comments
                    .Include("Message")
                    .Where(a => a.Name == userName && !a.Anonymized)
                    .OrderByDescending(s => s.Likes - s.Dislikes)
                    .Take(3)
                    .ToList();

                // get 3 lowest rated comments
                var lowestRatedComments = db.Comments
                    .Include("Message")
                    .Where(a => a.Name == userName && !a.Anonymized)
                    .OrderBy(s => s.Likes - s.Dislikes)
                    .Take(3)
                    .ToList();

                var linkSubmissionsCount = db.Messages.Count(a => a.Name == userName && a.Type == 2);
                var messageSubmissionsCount = db.Messages.Count(a => a.Name == userName && a.Type == 1);

                // get 5 highest rated submissions
                var highestRatedSubmissions = db.Messages.Where(a => a.Name == userName && !a.Anonymized)
                    .OrderByDescending(s => s.Likes - s.Dislikes)
                    .Take(5)
                    .ToList();

                // get 5 lowest rated submissions
                var lowestRatedSubmissions = db.Messages.Where(a => a.Name == userName && !a.Anonymized)
                    .OrderBy(s => s.Likes - s.Dislikes)
                    .Take(5)
                    .ToList();

                userStatsModel.TopSubversesUserContributedTo = subverses;
                userStatsModel.LinkSubmissionsSubmitted = linkSubmissionsCount;
                userStatsModel.MessageSubmissionsSubmitted = messageSubmissionsCount;
                userStatsModel.LowestRatedSubmissions = lowestRatedSubmissions;
                userStatsModel.HighestRatedSubmissions = highestRatedSubmissions;
                userStatsModel.TotalCommentsSubmitted = comments;
                userStatsModel.HighestRatedComments = highestRatedComments;
                userStatsModel.LowestRatedComments = lowestRatedComments;
                userStatsModel.TotalCommentsUpvoted = commentUpvotes;
                userStatsModel.TotalCommentsDownvoted = commentDownvotes;
                userStatsModel.TotalSubmissionsUpvoted = submissionUpvotes;
                userStatsModel.TotalSubmissionsDownvoted = submissionDownvotes;
            }

            return userStatsModel;
        }

        // check if a given user is globally banned
        public static bool IsUserGloballyBanned(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                var bannedUser = db.Bannedusers.FirstOrDefault(n => n.Username.Equals(userName, StringComparison.OrdinalIgnoreCase));
                return bannedUser != null;
            }
        }

        // check if a given user is banned from a subverse
        public static bool IsUserBannedFromSubverse(string userName, string subverseName)
        {
            using (var db = new whoaverseEntities())
            {
                var bannedUser = db.SubverseBans.FirstOrDefault(n => n.Username.Equals(userName, StringComparison.OrdinalIgnoreCase) && n.SubverseName.Equals(subverseName, StringComparison.OrdinalIgnoreCase));
                return bannedUser != null;
            }
        }

        // check if a given user is registered as a partner
        public static bool IsUserPartner(string userName)
        {
            using (var tmpUserManager = new UserManager<WhoaVerseUser>(new UserStore<WhoaVerseUser>(new ApplicationDbContext())))
            {
                var tmpuser = tmpUserManager.FindByName(userName);
                return (tmpuser != null) && tmpuser.Partner;
            }
        }

        // check if a given user wants to publicly display his subscriptions
        public static bool PublicSubscriptionsEnabled(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                var result = db.Userpreferences.Find(userName);
                return result != null && result.Public_subscriptions;
            }
        }

        // check if a given user wants to replace default menu bar with subscriptions
        public static bool Topmenu_From_Subscriptions(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                var result = db.Userpreferences.Find(userName);
                return result != null && result.Topmenu_from_subscriptions;
            }
        }

        // get short bio for a given user
        public static string UserShortbio(string userName)
        {
            const string placeHolderMessage = "Aww snap, this user did not yet write their bio. If they did, it would show up here, you know.";

            using (var db = new whoaverseEntities())
            {
                var result = db.Userpreferences.Find(userName);
                if (result == null) return placeHolderMessage;

                return result.Shortbio ?? placeHolderMessage;
            }
        }

        // get avatar for a given user
        public static string HasAvatar(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                var result = db.Userpreferences.Find(userName);
                return result == null ? null : result.Avatar;
            }
        }

        // check if a given user is subscribed to a given set
        public static bool IsUserSubscribedToSet(string userName, string setName)
        {
            using (var db = new whoaverseEntities())
            {
                var result = db.Usersetsubscriptions.FirstOrDefault(s => s.Userset.Name == setName && s.Username == userName);
                return result != null;
            }
        }

        // check if a given user is owner of a given set
        public static bool IsUserSetOwner(string userName, int setId)
        {
            using (var db = new whoaverseEntities())
            {
                var result = db.Usersets.FirstOrDefault(s => s.Set_id == setId && s.Created_by == userName);
                return result != null;
            }
        }

        // return sets subscription count for a given user
        public static int SetsSubscriptionCount(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                return db.Usersetsubscriptions.Count(s => s.Username.Equals(userName, StringComparison.OrdinalIgnoreCase));
            }
        }

        // check if a given user has used his daily posting quota for a given subverse
        public static bool UserDailyPostingQuotaForSubUsed(string userName, string subverse)
        {
            // set starting date to 24 hours ago from now
            var startDate = DateTime.Now.Add(new TimeSpan(0, -24, 0, 0, 0));

            // read daily posting quota per sub configuration parameter from web.config
            int dpqps = MvcApplication.DailyPostingQuotaPerSub;

            using (var db = new whoaverseEntities())
            {
                // check how many submission user made today
                var userSubmissionsToTargetSub = db.Messages.Count(
                    m => m.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase)
                        && m.Name.Equals(userName, StringComparison.OrdinalIgnoreCase)
                        && m.Date >= startDate && m.Date <= DateTime.Now);

                if (dpqps <= userSubmissionsToTargetSub)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his daily posting quota
        public static bool UserDailyPostingQuotaForNegativeScoreUsed(string userName)
        {
            // set starting date to 24 hours ago from now
            var startDate = DateTime.Now.Add(new TimeSpan(0, -24, 0, 0, 0));

            // read daily posting quota per sub configuration parameter from web.config
            int dpqps = MvcApplication.DailyPostingQuotaForNegativeScore;

            using (var db = new whoaverseEntities())
            {
                // check how many submission user made today
                var userSubmissionsInPast24Hours = db.Messages.Count(
                    m => m.Name.Equals(userName, StringComparison.OrdinalIgnoreCase)
                        && m.Date >= startDate && m.Date <= DateTime.Now);

                if (dpqps <= userSubmissionsInPast24Hours)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his daily comment posting quota
        public static bool UserDailyCommentPostingQuotaForNegativeScoreUsed(string userName)
        {
            // set starting date to 24 hours ago from now
            var startDate = DateTime.Now.Add(new TimeSpan(0, -24, 0, 0, 0));

            // read daily posting quota per sub configuration parameter from web.config
            int dpqps = MvcApplication.DailyCommentPostingQuotaForNegativeScore;

            using (var db = new whoaverseEntities())
            {
                // check how many submission user made today
                var userCommentSubmissionsInPast24Hours = db.Comments.Count(
                    m => m.Name.Equals(userName, StringComparison.OrdinalIgnoreCase)
                        && m.Date >= startDate && m.Date <= DateTime.Now);

                if (dpqps <= userCommentSubmissionsInPast24Hours)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his hourly posting quota for a given subverse
        public static bool UserHourlyPostingQuotaForSubUsed(string userName, string subverse)
        {
            // set starting date to 1 hours ago from now
            var startDate = DateTime.Now.Add(new TimeSpan(0, -1, 0, 0, 0));

            // read daily posting quota per sub configuration parameter from web.config
            int dpqps = MvcApplication.HourlyPostingQuotaPerSub;

            using (var db = new whoaverseEntities())
            {
                // check how many submission user made in the last hour
                var userSubmissionsToTargetSub = db.Messages.Count(
                    m => m.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase)
                        && m.Name.Equals(userName, StringComparison.OrdinalIgnoreCase)
                        && m.Date >= startDate && m.Date <= DateTime.Now);

                if (dpqps <= userSubmissionsToTargetSub)
                {
                    return true;
                }
                return false;
            }
        }

        // check if given user has submitted the same url before
        public static bool DailyCrossPostingQuotaUsed(string userName, string url)
        {
            // read daily crosspost quota from web.config
            int dailyCrossPostQuota = MvcApplication.DailyCrossPostingQuota;

            // set starting date to 24 hours ago from now
            var startDate = DateTime.Now.Add(new TimeSpan(0, -24, 0, 0, 0));

            using (var db = new whoaverseEntities())
            {
                var numberOfTimesSubmitted = db.Messages.Count(
                    m => m.MessageContent.Equals(url, StringComparison.OrdinalIgnoreCase)
                        && m.Name.Equals(userName, StringComparison.OrdinalIgnoreCase)
                        && m.Date >= startDate && m.Date <= DateTime.Now);

                if (dailyCrossPostQuota <= numberOfTimesSubmitted)
                {
                    return true;
                }
                return false;
            }
        }

        // subscribe to a set
        public static void SubscribeToSet(string userName, int setId)
        {
            // do nothing if user is already subscribed
            if (IsUserSetSubscriber(userName, setId)) return;

            using (var db = new whoaverseEntities())
            {
                // add a new set subscription
                var newSubscription = new Usersetsubscription { Username = userName, Set_id = setId };
                db.Usersetsubscriptions.Add(newSubscription);

                // record new set subscription in sets table subscribers field
                var tmpUserSet = db.Usersets.Find(setId);

                if (tmpUserSet != null)
                {
                    tmpUserSet.Subscribers++;
                }

                db.SaveChanges();
            }
        }

        // unsubscribe from a set
        public static void UnSubscribeFromSet(string userName, int setId)
        {
            // do nothing if user is not subscribed to given set
            if (!IsUserSetSubscriber(userName, setId)) return;

            using (var db = new whoaverseEntities())
            {
                var subscription = db.Usersetsubscriptions.FirstOrDefault(b => b.Username == userName && b.Set_id == setId);

                // remove subscription record
                db.Usersetsubscriptions.Remove(subscription);

                // record new unsubscription in sets table subscribers field
                var tmpUserset = db.Usersets.Find(setId);

                if (tmpUserset != null)
                {
                    tmpUserset.Subscribers--;
                }

                db.SaveChanges();
            }
        }

        // check if a given user has downvoted more comments than upvoted
        public static bool IsUserCommentVotingMeanie(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                // get voting habits
                var commentUpvotes = db.Commentvotingtrackers.Count(a => a.UserName == userName && a.VoteStatus == 1);
                var commentDownvotes = db.Commentvotingtrackers.Count(a => a.UserName == userName && a.VoteStatus == -1);

                var totalCommentVotes = commentUpvotes + commentDownvotes;

                // downvote ratio
                var downvotePercentage = (double)commentDownvotes / totalCommentVotes * 100;

                // upvote ratio
                var upvotePercentage = (double)commentUpvotes / totalCommentVotes * 100;

                return downvotePercentage > upvotePercentage;
            }
        }

        // check if a given user has downvoted more submissions than upvoted
        public static bool IsUserSubmissionVotingMeanie(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                // get voting habits
                var submissionUpvotes = db.Votingtrackers.Count(a => a.UserName == userName && a.VoteStatus == 1);
                var submissionDownvotes = db.Votingtrackers.Count(a => a.UserName == userName && a.VoteStatus == -1);

                var totalSubmissionVotes = submissionUpvotes + submissionDownvotes;

                // downvote ratio
                var downvotePercentage = (double)submissionDownvotes / totalSubmissionVotes * 100;

                // upvote ratio
                var upvotePercentage = (double)submissionUpvotes / totalSubmissionVotes * 100;

                return downvotePercentage > upvotePercentage;
            }
        }

        // get user IP address from httprequestbase
        public static string UserIpAddress(HttpRequestBase request)
        {
            string clientIpAddress = String.Empty;
            if (request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            {
                clientIpAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            }
            else if (request.UserHostAddress.Length != 0)
            {
                clientIpAddress = request.UserHostAddress;
            }
            return clientIpAddress;
        }

        // block a subverse
        public static void BlockSubverse(string userName, string subverse)
        {
            using (var db = new whoaverseEntities())
            {
                // unblock if subverse is already blocked
                if (IsUserBlockingSubverse(userName, subverse))
                {
                    var subverseBlock = db.UserBlockedSubverses.FirstOrDefault(n => n.SubverseName.ToLower() == subverse.ToLower() && n.Username == userName);
                    if (subverseBlock != null) db.UserBlockedSubverses.Remove(subverseBlock);
                    db.SaveChanges();
                    return;
                }

                // add a new block
                var blockedSubverse = new UserBlockedSubverse { Username = userName, SubverseName = subverse };
                db.UserBlockedSubverses.Add(blockedSubverse);
                db.SaveChanges();
            }
        }
    }
}