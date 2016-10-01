﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Models;
using Voat.Utilities;

namespace Voat.Domain
{
    /// <summary>
    /// I'm rewriting UserHelper class here using Query and Command objects. The UserHelper is a performance hog. 
    /// If it was a person it would be the guy who ate all the pizza before you got back from washing your hands.
    /// </summary>
    public class UserGateway
    {
        // check if user exists in database
        public static bool UserExists(string userName)
        {
            using (var tmpUserManager = new UserManager<VoatUser>(new UserStore<VoatUser>(new ApplicationDbContext())))
            {
                var tmpuser = tmpUserManager.FindByName(userName);
                return tmpuser != null;
            }
        }

        // return original username
        public static string OriginalUsername(string userName)
        {
            using (var tmpUserManager = new UserManager<VoatUser>(new UserStore<VoatUser>(new ApplicationDbContext())))
            {
                var tmpuser = tmpUserManager.FindByName(userName);
                return tmpuser != null ? tmpuser.UserName : null;
            }
        }

        // return user registration date
        public static DateTime GetUserRegistrationDateTime(string userName)
        {
            using (var tmpUserManager = new UserManager<VoatUser>(new UserStore<VoatUser>(new ApplicationDbContext())))
            {
                var tmpuser = tmpUserManager.FindByName(userName);
                return tmpuser != null ? tmpuser.RegistrationDateTime : DateTime.MinValue;
            }
        }

        // delete a user account and all history: comments, posts and votes
        public static bool DeleteUser(string userName)
        {
            using (var db = new voatEntities())
            {
                using (var tmpUserManager = new UserManager<VoatUser>(new UserStore<VoatUser>(new ApplicationDbContext())))
                {
                    var tmpuser = tmpUserManager.FindByName(userName);
                    if (tmpuser != null)
                    {
                        // remove voting history for submisions
                        db.SubmissionVoteTrackers.RemoveRange(db.SubmissionVoteTrackers.Where(x => x.UserName == userName));

                        // remove voting history for comments
                        db.CommentVoteTrackers.RemoveRange(db.CommentVoteTrackers.Where(x => x.UserName == userName));

                        // remove all comments
                        var comments = db.Comments.Where(c => c.UserName == userName).ToList();
                        foreach (Data.Models.Comment c in comments)
                        {
                            c.IsDeleted = true;
                            c.Content = "deleted by user";
                        }
                        db.SaveChanges();

                        // remove all submissions
                        var submissions = db.Submissions.Where(c => c.UserName == userName).ToList();
                        foreach (var s in submissions)
                        {
                            s.Title = "deleted by user";
                            if (s.Type == 1)
                            {
                                s.IsDeleted = true;
                                s.Content = "deleted by user";
                            }
                            else
                            {
                                s.IsDeleted = true;
                                s.Url = "http://voat.co";
                            }
                        }

                        // resign from all moderating positions
                        db.SubverseModerators.RemoveRange(db.SubverseModerators.Where(m => m.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)));

                        // delete comment reply notifications
                        db.CommentReplyNotifications.RemoveRange(db.CommentReplyNotifications.Where(crp => crp.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase)));

                        // delete post reply notifications
                        db.SubmissionReplyNotifications.RemoveRange(db.SubmissionReplyNotifications.Where(prp => prp.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase)));

                        // delete private messages
                        db.PrivateMessages.RemoveRange(db.PrivateMessages.Where(pm => pm.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase)));

                        // delete short bio if userprefs record exists for this user
                        var userPrefs = db.UserPreferences.Find(userName);
                        if (userPrefs != null)
                        {
                            userPrefs.Bio = null;
                        }

                        // TODO: delete avatar
                        // userPrefs.Avatar = ""

                        // TODO:
                        // keep this updated as new features are added (delete sets etc)

                        // username will stay permanently reserved to prevent someone else from registering it and impersonating

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
            var q = new QuerySubverseModerators(subverse);
            var d = Task.Run(() => q.ExecuteAsync()).Result;
            return d.Any(x => x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) && x.Power == 1);

            //using (var db = new voatEntities())
            //{
            //    var subverseOwner = db.SubverseModerators.FirstOrDefault(n => n.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) && n.Power == 1);
            //    return subverseOwner != null && subverseOwner.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase);
            //}
        }

        // check if given user is moderator for a given subverse
        public static bool IsUserSubverseModerator(string userName, string subverse)
        {
            var q = new QuerySubverseModerators(subverse);
            var d = Task.Run(() => q.ExecuteAsync()).Result;
            return d.Any(x => x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));


            //using (var db = new voatEntities())
            //{
            //    var subverseModerator = db.SubverseModerators.FirstOrDefault(n => n.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) && n.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) && n.Power == 2);
            //    var subverseOwner = db.SubverseModerators.FirstOrDefault(n => n.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase) && n.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) && n.Power == 1);

            //    if (subverseModerator != null && subverseModerator.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
            //    {
            //        return true;
            //    }

            //    // subverse owners are by default also moderators
            //    if (subverseOwner != null && subverseOwner.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
            //    {
            //        return true;
            //    }

            //    return false;
            //}
        }

        // check if given user is subscribed to a given subverse
        public static bool IsUserSubverseSubscriber(string userName, string subverse)
        {
            var result = false;
            var q = new QueryUserSubscriptions(userName);
            var d = Task.Run(() => q.ExecuteAsync()).Result;
            if (d.ContainsKey(DomainType.Subverse.ToString()))
            {
                result = d[DomainType.Subverse.ToString()].Any(x => x.Equals(subverse, StringComparison.OrdinalIgnoreCase));
            }
            return result;

            //using (var db = new voatEntities())
            //{
            //    var subverseSubscriber = db.SubverseSubscriptions.FirstOrDefault(n => n.Subverse.ToLower() == subverse.ToLower() && n.UserName == userName);
            //    return subverseSubscriber != null;
            //}
        }

        // check if given user blocks a given subverse
        public static bool IsUserBlockingSubverse(string userName, string subverse)
        {

            var result = false;
            var q = new QueryUserBlocks();
            var d = Task.Run(() => q.ExecuteAsync()).Result;
            result = d.Any(x => x.Type == Models.DomainType.Subverse && x.Name.Equals(subverse, StringComparison.OrdinalIgnoreCase));
            return result;

            //using (var db = new voatEntities())
            //{
            //    var subverseBlock = db.UserBlockedSubverses.FirstOrDefault(n => n.Subverse.ToLower() == subverse.ToLower() && n.UserName == userName);
            //    return subverseBlock != null;
            //}
        }

        // check if given user is subscribed to a given set
        public static bool IsUserSetSubscriber(string userName, string setName)
        {
            var result = false;
            var q = new QueryUserBlocks();
            var d = Task.Run(() => q.ExecuteAsync()).Result;
            result = d.Any(x => x.Type == Models.DomainType.Set && x.Name.Equals(setName, StringComparison.OrdinalIgnoreCase));
            return result;

            //using (var db = new voatEntities())
            //{
            //    var setSubscriber = db.UserSetSubscriptions.FirstOrDefault(n => n.UserSetID == setId && n.UserName == userName);
            //    return setSubscriber != null;
            //}
        }

        // subscribe to a subverse
        public static void SubscribeToSubverse(string userName, string subverse)
        {
            if (!IsUserSubverseSubscriber(userName, subverse))
            {

                var cmd = new SubscriptionCommand(DomainType.Subverse, SubscriptionAction.Subscribe, subverse);
                var result = Task.Run(() => cmd.Execute()).Result;

                //using (var db = new voatEntities())
                //{
                //    // add a new subscription
                //    var newSubscription = new SubverseSubscription { UserName = userName, Subverse = subverse };
                //    db.SubverseSubscriptions.Add(newSubscription);

                //    // record new subscription in subverse table subscribers field
                //    Subverse tmpSubverse = db.Subverses.Find(subverse);

                //    if (tmpSubverse != null)
                //    {
                //        tmpSubverse.SubscriberCount++;
                //    }

                //    db.SaveChanges();
                //}

            }
        }

        // unsubscribe from a subverse
        public static void UnSubscribeFromSubverse(string userName, string subverse)
        {
            if (IsUserSubverseSubscriber(userName, subverse))
            {
                var cmd = new SubscriptionCommand(DomainType.Subverse, SubscriptionAction.Unsubscribe, subverse);
                var result = Task.Run(() => cmd.Execute()).Result;

                //using (var db = new voatEntities())
                //{
                //    var subscription = db.SubverseSubscriptions.FirstOrDefault(b => b.UserName == userName && b.Subverse == subverse);

                //    if (subverse == null)
                //        return;
                //    // remove subscription record
                //    db.SubverseSubscriptions.Remove(subscription);

                //    // record new unsubscription in subverse table subscribers field
                //    Subverse tmpSubverse = db.Subverses.Find(subverse);

                //    if (tmpSubverse != null)
                //    {
                //        tmpSubverse.SubscriberCount--;
                //    }

                //    db.SaveChanges();
                //}
            }
        }

        // return subscription count for a given user
        public static int SubscriptionCount(string userName)
        {
            var result = 0;
            var q = new QueryUserSubscriptions(userName);
            var d = Task.Run(() => q.ExecuteAsync()).Result;
            if (d.ContainsKey(DomainType.Subverse.ToString()))
            {
                result = d[DomainType.Subverse.ToString()].Count();
            }
            return result;

            //using (var db = new voatEntities())
            //{
            //    return db.SubverseSubscriptions.Count(s => s.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
            //}
        }

        // return a list of subverses user is subscribed to
        public static IEnumerable<string> UserSubscriptions(string userName)
        {

            IEnumerable<string> result = null;
            var q = new QueryUserSubscriptions(userName);
            var d = Task.Run(() => q.ExecuteAsync()).Result;
            if (d.ContainsKey(DomainType.Subverse.ToString()))
            {
                result = d[DomainType.Subverse.ToString()];
            }
            return result;


            //// get a list of subcribed subverses with details and order by subverse names, ascending
            //using (var db = new voatEntities())
            //{
            //    var subscribedSubverses = from c in db.Subverses
            //                              join a in db.SubverseSubscriptions
            //                              on c.Name equals a.Subverse
            //                              where a.UserName.Equals(userName)
            //                              orderby a.Subverse ascending
            //                              select c.Name;

            //    return subscribedSubverses.ToList();
            //}
        }

        // return a list of user badges
        //HAD TO CHANGE TO DOMAIN MODEL
        public static IEnumerable<Models.UserBadge> UserBadges(string userName)
        {
            var q = new QueryUserInformation(userName);
            var result = Task.Run(() => q.ExecuteAsync()).Result;
            return result.Badges;

            //using (var db = new voatEntities())
            //{
            //    return db.UserBadges.Include("Badge")
            //        .Where(r => r.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
            //        .ToList();
            //}
        }

        // check if given user has unread private messages, not including messages manually marked as unread
        public static bool UserHasNewMessages(string userName)
        {
            using (var db = new voatEntities())
            {
                var unreadPrivateMessagesCount = db.PrivateMessages.Count(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase) && s.IsUnread && s.MarkedAsUnread == false);
                var unreadCommentRepliesCount = db.CommentReplyNotifications.Count(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase) && s.IsUnread && s.MarkedAsUnread == false);
                var unreadPostRepliesCount = db.SubmissionReplyNotifications.Count(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase) && s.IsUnread && s.MarkedAsUnread == false);

                return unreadPrivateMessagesCount > 0 || unreadCommentRepliesCount > 0 || unreadPostRepliesCount > 0;
            }
        }

        // check if given user has unread comment replies and return the count
        public static int UnreadCommentRepliesCount(string userName)
        {
            using (var db = new voatEntities())
            {
                var commentReplies = db.CommentReplyNotifications
                    .Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(s => s.CreationDate)
                    .ThenBy(s => s.Sender);

                if (!commentReplies.Any())
                    return 0;

                var unreadCommentReplies = commentReplies.Where(s => s.IsUnread && s.MarkedAsUnread == false);
                return unreadCommentReplies.Any() ? unreadCommentReplies.Count() : 0;
            }
        }

        // check if given user has unread post replies and return the count
        public static int UnreadPostRepliesCount(string userName)
        {
            using (var db = new voatEntities())
            {
                var postReplies = db.SubmissionReplyNotifications
                    .Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(s => s.CreationDate)
                    .ThenBy(s => s.Sender);

                if (!postReplies.Any())
                    return 0;
                var unreadPostReplies = postReplies.Where(s => s.IsUnread && s.MarkedAsUnread == false);

                return unreadPostReplies.Any() ? unreadPostReplies.Count() : 0;
            }
        }

        // check if given user has unread private messages and return the count
        public static int UnreadPrivateMessagesCount(string userName)
        {
            using (var db = new voatEntities())
            {
                var privateMessages = db.PrivateMessages
                    .Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(s => s.CreationDate)
                    .ThenBy(s => s.Sender);

                if (!privateMessages.Any())
                    return 0;
                var unreadPrivateMessages = privateMessages.Where(s => s.IsUnread && s.MarkedAsUnread == false);

                return unreadPrivateMessages.Any() ? unreadPrivateMessages.Count() : 0;
            }
        }

        // get total unread notifications count for a given user
        public static int UnreadTotalNotificationsCount(string userName)
        {
            using (var db = new voatEntities())
            {
                int totalCount = 0;
                int unreadPrivateMessagesCount = db.PrivateMessages.Count(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase) && s.IsUnread && s.MarkedAsUnread == false);
                int unreadPostRepliesCount = db.SubmissionReplyNotifications.Count(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase) && s.IsUnread && s.MarkedAsUnread == false);
                int unreadCommentRepliesCount = db.CommentReplyNotifications.Count(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase) && s.IsUnread && s.MarkedAsUnread == false);

                totalCount = totalCount + unreadPrivateMessagesCount + unreadPostRepliesCount + unreadCommentRepliesCount;
                return totalCount;
            }
        }

        // get total number of comment replies for a given user
        public static int CommentRepliesCount(string userName)
        {
            using (var db = new voatEntities())
            {
                var commentReplies = db.CommentReplyNotifications.Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase));
                if (!commentReplies.Any())
                    return 0;
                return commentReplies.Any() ? commentReplies.Count() : 0;
            }
        }

        // get total number of post replies for a given user
        public static int PostRepliesCount(string userName)
        {
            using (var db = new voatEntities())
            {
                var postReplies = db.SubmissionReplyNotifications.Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase));
                if (!postReplies.Any())
                    return 0;
                return postReplies.Any() ? postReplies.Count() : 0;
            }
        }

        // get total number of private messages for a given user
        public static int PrivateMessageCount(string userName)
        {
            using (var db = new voatEntities())
            {
                var privateMessages = db.PrivateMessages.Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase));
                if (!privateMessages.Any())
                    return 0;
                return privateMessages.Any() ? privateMessages.Count() : 0;
            }
        }

        // check if a given user does not want to see custom CSS styles
        public static bool CustomCssDisabledForUser(string userName)
        {
            using (var db = new voatEntities())
            {
                var result = db.UserPreferences.Find(userName);
                return result != null && result.DisableCSS;
            }
        }

        // check which theme style user selected
        public static void SetUserStylePreferenceCookie(string theme)
        {
            var cookie = new HttpCookie("theme", theme);
            cookie.Expires = Repository.CurrentDate.AddDays(14);
            System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
        }

        // check which theme style user selected
        public static string UserStylePreference(string userName)
        {
            string theme = "light";

            var tc = System.Web.HttpContext.Current.Request.Cookies["theme"];
            if (tc != null && !String.IsNullOrEmpty(tc.Value))
            {
                theme = tc.Value;
            }
            else
            {
                if (!String.IsNullOrEmpty(userName))
                {
                    using (var db = new voatEntities())
                    {
                        var result = db.UserPreferences.Find(userName);
                        if (result != null)
                        {
                            theme = result.NightMode ? "dark" : "light";
                        }
                    }
                }
            }
            return theme;
        }

        // check if a given user wants to see NSFW (adult) content
        public static bool AdultContentEnabled(string userName)
        {
            using (var db = new voatEntities())
            {
                var result = db.UserPreferences.Find(userName);
                return result != null && result.EnableAdultContent;
            }
        }

        // check if a given user wants to open links in new window
        public static bool LinksInNewWindow(string userName)
        {
            using (var db = new voatEntities())
            {
                var result = db.UserPreferences.Find(userName);
                return result != null && result.OpenInNewWindow;
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

            var startDate = Repository.CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));

            using (var db = new voatEntities())
            {
                // calculate how many comment votes user made in the past 24 hours
                var commentVotesUsedToday = db.CommentVoteTrackers
                    .Where(c => c.CreationDate >= startDate && c.CreationDate <= Repository.CurrentDate && c.UserName == userName);

                commentVotesUsedInPast24Hrs = commentVotesUsedToday.Count();

                // calculate how many submission votes user made in the past 24 hours
                var submissionVotesUsedToday = db.SubmissionVoteTrackers
                    .Where(c => c.CreationDate >= startDate && c.CreationDate <= Repository.CurrentDate && c.UserName == userName);

                submissionVotesUsedInPast24Hrs = submissionVotesUsedToday.Count();
            }

            return (commentVotesUsedInPast24Hrs + submissionVotesUsedInPast24Hrs);
        }

        // return user statistics for user profile overview
        public static UserStatsModel UserStatsModel(string userName)
        {
            var userStatsModel = new UserStatsModel();

            using (var db = new voatEntities())
            {
                // 5 subverses user submitted to most
                var subverses = db.Submissions.Where(a => a.UserName == userName && !a.IsAnonymized && !a.IsDeleted)
                         .GroupBy(a => new { a.UserName, a.Subverse })
                         .Select(g => new SubverseStats { SubverseName = g.Key.Subverse, Count = g.Count() })
                         .OrderByDescending(s => s.Count)
                         .Take(5)
                         .ToList();

                // total comment count
                var comments = db.Comments.Count(a => a.UserName == userName);

                // voting habits
                var commentUpvotes = db.CommentVoteTrackers.Count(a => a.UserName == userName && a.VoteStatus == 1);
                var commentDownvotes = db.CommentVoteTrackers.Count(a => a.UserName == userName && a.VoteStatus == -1);
                var submissionUpvotes = db.SubmissionVoteTrackers.Count(a => a.UserName == userName && a.VoteStatus == 1);
                var submissionDownvotes = db.SubmissionVoteTrackers.Count(a => a.UserName == userName && a.VoteStatus == -1);

                // get 3 highest rated comments
                var highestRatedComments = db.Comments
                    .Include("Submission")
                    .Where(a => a.UserName == userName && !a.IsAnonymized && !a.IsDeleted)
                    .OrderByDescending(s => s.UpCount - s.DownCount)
                    .Take(3)
                    .ToList();

                // get 3 lowest rated comments
                var lowestRatedComments = db.Comments
                    .Include("Submission")
                    .Where(a => a.UserName == userName && !a.IsAnonymized && !a.IsDeleted)
                    .OrderBy(s => s.UpCount - s.DownCount)
                    .Take(3)
                    .ToList();

                var linkSubmissionsCount = db.Submissions.Count(a => a.UserName == userName && a.Type == 2 && !a.IsDeleted);
                var messageSubmissionsCount = db.Submissions.Count(a => a.UserName == userName && a.Type == 1 && !a.IsDeleted);

                // get 5 highest rated submissions
                var highestRatedSubmissions = db.Submissions.Where(a => a.UserName == userName && !a.IsAnonymized && !a.IsDeleted)
                    .OrderByDescending(s => s.UpCount - s.DownCount)
                    .Take(5)
                    .ToList();

                // get 5 lowest rated submissions
                var lowestRatedSubmissions = db.Submissions.Where(a => a.UserName == userName && !a.IsAnonymized && !a.IsDeleted)
                    .OrderBy(s => s.UpCount - s.DownCount)
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
            using (var db = new voatEntities())
            {
                var bannedUser = db.BannedUsers.FirstOrDefault(n => n.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
                return bannedUser != null;
            }
        }

        // check if a given user is banned from a subverse
        public static bool IsUserBannedFromSubverse(string userName, string subverseName)
        {
            using (var db = new voatEntities())
            {
                var bannedUser = db.SubverseBans.FirstOrDefault(n => n.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) && n.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase));
                return bannedUser != null;
            }
        }

        // check if a given user is registered as a partner
        public static bool IsUserPartner(string userName)
        {
            using (var tmpUserManager = new UserManager<VoatUser>(new UserStore<VoatUser>(new ApplicationDbContext())))
            {
                var tmpuser = tmpUserManager.FindByName(userName);
                return (tmpuser != null) && tmpuser.Partner;
            }
        }

        // check if a given user wants to publicly display his subscriptions
        public static bool PublicSubscriptionsEnabled(string userName)
        {
            using (var db = new voatEntities())
            {
                var result = db.UserPreferences.Find(userName);
                return result != null && result.DisplaySubscriptions;
            }
        }

        // check if a given user wants to replace default menu bar with subscriptions
        public static bool Topmenu_From_Subscriptions(string userName)
        {
            using (var db = new voatEntities())
            {
                var result = db.UserPreferences.Find(userName);
                return result != null && result.UseSubscriptionsMenu;
            }
        }

        // get short bio for a given user
        public static string UserShortbio(string userName)
        {
            const string placeHolderMessage = "Aww snap, this user did not yet write their bio. If they did, it would show up here, you know.";

            using (var db = new voatEntities())
            {
                var result = db.UserPreferences.Find(userName);
                if (result == null)
                    return placeHolderMessage;

                return result.Bio ?? placeHolderMessage;
            }
        }

        // get avatar for a given user
        public static string HasAvatar(string userName)
        {
            using (var db = new voatEntities())
            {
                var result = db.UserPreferences.Find(userName);
                return result == null ? null : result.Avatar;
            }
        }

        // check if a given user is subscribed to a given set
        public static bool IsUserSubscribedToSet(string userName, string setName)
        {
            using (var db = new voatEntities())
            {
                var result = db.UserSetSubscriptions.FirstOrDefault(s => s.UserSet.Name == setName && s.UserName == userName);
                return result != null;
            }
        }

        // check if a given user is owner of a given set
        public static bool IsUserSetOwner(string userName, int setId)
        {
            using (var db = new voatEntities())
            {
                var result = db.UserSets.FirstOrDefault(s => s.ID == setId && s.CreatedBy == userName);
                return result != null;
            }
        }

        // return sets subscription count for a given user
        public static int SetsSubscriptionCount(string userName)
        {
            using (var db = new voatEntities())
            {
                return db.UserSetSubscriptions.Count(s => s.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
            }
        }

        // check if a given user has used his daily posting quota for a given subverse
        public static bool UserDailyPostingQuotaForSubUsed(string userName, string subverse)
        {
            // set starting date to 24 hours ago from now
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));
            var toDate = Repository.CurrentDate;
            // read daily posting quota per sub configuration parameter from web.config
            int dpqps = Settings.DailyPostingQuotaPerSub;

            using (var db = new voatEntities())
            {
                // check how many submission user made today
                var userSubmissionsToTargetSub = db.Submissions.Count(
                    m => m.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase)
                        && m.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                        && m.CreationDate >= fromDate && m.CreationDate <= toDate);

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
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));
            var toDate = Repository.CurrentDate;

            // read daily posting quota per sub configuration parameter from web.config
            int dpqps = Settings.DailyPostingQuotaForNegativeScore;

            using (var db = new voatEntities())
            {
                // check how many submission user made today
                var userSubmissionsInPast24Hours = db.Submissions.Count(
                    m => m.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                        && m.CreationDate >= fromDate && m.CreationDate <= toDate);

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
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));
            var toDate = Repository.CurrentDate;

            // read daily posting quota per sub configuration parameter from web.config
            int dpqps = Settings.DailyCommentPostingQuotaForNegativeScore;

            using (var db = new voatEntities())
            {
                // check how many submission user made today
                var userCommentSubmissionsInPast24Hours = db.Comments.Count(
                    m => m.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                        && m.CreationDate >= fromDate && m.CreationDate <= toDate);

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
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, -1, 0, 0, 0));
            var toDate = Repository.CurrentDate;

            // read daily posting quota per sub configuration parameter from web.config
            int dpqps = Settings.HourlyPostingQuotaPerSub;

            using (var db = new voatEntities())
            {
                // check how many submission user made in the last hour
                var userSubmissionsToTargetSub = db.Submissions.Count(
                    m => m.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase)
                        && m.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                        && m.CreationDate >= fromDate && m.CreationDate <= toDate);

                if (dpqps <= userSubmissionsToTargetSub)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his global hourly posting quota
        public static bool UserHourlyGlobalPostingQuotaUsed(string userName)
        {
            // only execute this check if user account is less than a month old and user SCP is less than 50 and user is not posting to a sub they own/moderate
            DateTime userRegistrationDateTime = GetUserRegistrationDateTime(userName);
            int memberInDays = (Repository.CurrentDate - userRegistrationDateTime).Days;
            int userScp = Karma.LinkKarma(userName);
            if (memberInDays > 30 && userScp >= 50)
            {
                return false;
            }

            // set starting date to 1 hours ago from now
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, -1, 0, 0, 0));
            var toDate = Repository.CurrentDate;

            // read daily posting quota per sub configuration parameter from web.config
            int dpqps = Settings.HourlyGlobalPostingQuota;

            using (var db = new voatEntities())
            {
                // check how many submission user made in the last hour
                var totalUserSubmissionsForTimeSpam = db.Submissions.Count(m => m.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) && m.CreationDate >= fromDate && m.CreationDate <= toDate);

                if (dpqps <= totalUserSubmissionsForTimeSpam)
                {
                    return true;
                }
                return false;
            }
        }

        // check if a given user has used his global daily posting quota
        public static bool UserDailyGlobalPostingQuotaUsed(string userName)
        {
            // only execute this check if user account is less than a month old and user SCP is less than 50 and user is not posting to a sub they own/moderate
            DateTime userRegistrationDateTime = GetUserRegistrationDateTime(userName);
            int memberInDays = (Repository.CurrentDate - userRegistrationDateTime).Days;
            int userScp = Karma.LinkKarma(userName);
            if (memberInDays > 30 && userScp >= 50)
            {
                return false;
            }

            // set starting date to 24 hours ago from now
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));
            var toDate = Repository.CurrentDate;

            // read daily global posting quota configuration parameter from web.config
            int dpqps = Settings.DailyGlobalPostingQuota;

            using (var db = new voatEntities())
            {
                // check how many submission user made today
                var userSubmissionsToTargetSub = db.Submissions.Count(m => m.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) && m.CreationDate >= fromDate && m.CreationDate <= toDate);

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
            int dailyCrossPostQuota = Settings.DailyCrossPostingQuota;

            // set starting date to 24 hours ago from now
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, -24, 0, 0, 0));
            var toDate = Repository.CurrentDate;

            using (var db = new voatEntities())
            {
                var numberOfTimesSubmitted = db.Submissions
                    .Where(m => m.Content.Equals(url, StringComparison.OrdinalIgnoreCase)
                    && m.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                    && m.CreationDate >= fromDate && m.CreationDate <= toDate);

                int nrtimessubmitted = numberOfTimesSubmitted.Count();

                if (dailyCrossPostQuota <= nrtimessubmitted)
                {
                    return true;
                }
                return false;
            }
        }

        // subscribe to a set
        public static void SubscribeToSet(string userName, string setName)
        {
            //// do nothing if user is already subscribed
            //if (IsUserSetSubscriber(userName, setName))
            //    return;

            //using (var db = new voatEntities())
            //{
            //    // add a new set subscription
            //    var newSubscription = new UserSetSubscription { UserName = userName, UserSetID = setName };
            //    db.UserSetSubscriptions.Add(newSubscription);

            //    // record new set subscription in sets table subscribers field
            //    var tmpUserSet = db.UserSets.Find(setName);

            //    if (tmpUserSet != null)
            //    {
            //        tmpUserSet.SubscriberCount++;
            //    }

            //    db.SaveChanges();
            //}
        }

        // unsubscribe from a set
        public static void UnSubscribeFromSet(string userName, string setName)
        {
            // do nothing if user is not subscribed to given set
            //if (!IsUserSetSubscriber(userName, setName))
            //    return;

            //using (var db = new voatEntities())
            //{
            //    var subscription = db.UserSetSubscriptions.FirstOrDefault(b => b.UserName == userName && b.UserSetID == setName);

            //    // remove subscription record
            //    db.UserSetSubscriptions.Remove(subscription);

            //    // record new unsubscription in sets table subscribers field
            //    var tmpUserset = db.UserSets.Find(setId);

            //    if (tmpUserset != null)
            //    {
            //        tmpUserset.SubscriberCount--;
            //    }

            //    db.SaveChanges();
            //}
        }

        // check if a given user has downvoted more comments than upvoted
        public static bool IsUserCommentVotingMeanie(string userName)
        {
            using (var db = new voatEntities())
            {
                // get voting habits
                var commentUpvotes = db.CommentVoteTrackers.Count(a => a.UserName == userName && a.VoteStatus == 1);
                var commentDownvotes = db.CommentVoteTrackers.Count(a => a.UserName == userName && a.VoteStatus == -1);

                return commentDownvotes > commentUpvotes;
            }
        }

        // check if a given user has downvoted more submissions than upvoted
        public static bool IsUserSubmissionVotingMeanie(string userName)
        {
            using (var db = new voatEntities())
            {
                // get voting habits
                var submissionUpvotes = db.CommentVoteTrackers.Count(a => a.UserName == userName && a.VoteStatus == 1);
                var submissionDownvotes = db.CommentVoteTrackers.Count(a => a.UserName == userName && a.VoteStatus == -1);

                return submissionDownvotes > submissionUpvotes;
            }
        }

        // get user IP address from httprequestbase
        public static string UserIpAddress(HttpRequestBase request)
        {
            const string HTTP_CONTEXT_KEY = "CF-Connecting-IP"; // USE REMOTE_ADDR

            string clientIpAddress = String.Empty;
            if (request.Headers[HTTP_CONTEXT_KEY] != null)
            {
                clientIpAddress = request.Headers[HTTP_CONTEXT_KEY];
            }
            else if (request.UserHostAddress.Length != 0)
            {
                clientIpAddress = request.UserHostAddress;
            }
            return clientIpAddress;
        }
        //this is for the API
        public static string UserIpAddress(HttpRequestMessage request)
        {
            const string HTTP_CONTEXT_KEY = "MS_HttpContext";
            const string REMOTE_ENDPOINT_KEY = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";

            string clientIpAddress = String.Empty;
            if (request.Properties.ContainsKey(HTTP_CONTEXT_KEY))
            {
                dynamic ctx = request.Properties[HTTP_CONTEXT_KEY];
                if (ctx != null)
                {
                    return ctx.Request.UserHostAddress;
                }
            }

            if (request.Properties.ContainsKey(REMOTE_ENDPOINT_KEY))
            {
                dynamic remoteEndpoint = request.Properties[REMOTE_ENDPOINT_KEY];
                if (remoteEndpoint != null)
                {
                    return remoteEndpoint.Address;
                }
            }

            return clientIpAddress;
        }

        //// block a subverse
        //public static void BlockSubverse(string userName, string subverse)
        //{
        //    using (var db = new voatEntities())
        //    {
        //        // unblock if subverse is already blocked
        //        if (IsUserBlockingSubverse(userName, subverse))
        //        {
        //            var subverseBlock = db.UserBlockedSubverses.FirstOrDefault(n => n.Subverse.ToLower() == subverse.ToLower() && n.UserName == userName);
        //            if (subverseBlock != null)
        //                db.UserBlockedSubverses.Remove(subverseBlock);
        //            db.SaveChanges();
        //            return;
        //        }

        //        // add a new block
        //        var blockedSubverse = new UserBlockedSubverse { UserName = userName, Subverse = subverse };
        //        db.UserBlockedSubverses.Add(blockedSubverse);
        //        db.SaveChanges();
        //    }
        //}
    }
}
