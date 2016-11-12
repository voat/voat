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

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Voat.Configuration;
using Voat.Data.Models;
using Voat.Models;
using Voat.Data;
using Voat.Domain.Query;
using System.Net.Http;
using Voat.Caching;
using System.Threading;
using Voat.Domain;

namespace Voat.Utilities
{
    public static class UserHelper
    {
        // check if user exists in database
        public static bool UserExists(string userName)
        {
            var q = new QueryUserRecord(userName);
            var d = q.Execute();
            return d != null;
        }

        // return original username
        public static string OriginalUsername(string userName)
        {
            if (!String.IsNullOrEmpty(userName))
            {
                var q = new QueryUserRecord(userName);
                var d = q.Execute();
                return d != null ? d.UserName : null;
            }
            return null;
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
                        foreach (Comment c in comments)
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

                        // delete user preferences
                        var userPrefs = db.UserPreferences.Find(userName);
                        if (userPrefs != null)
                        {
                            // delete short bio
                            userPrefs.Bio = null;

                            // delete avatar
                            if (userPrefs.Avatar != null)
                            {
                                var avatarFilename = userPrefs.Avatar;
                                if (Settings.UseContentDeliveryNetwork)
                                {
                                    // try to delete from CDN
                                    CloudStorageUtility.DeleteBlob(avatarFilename, "avatars");
                                }
                                else
                                {
                                    // try to remove from local FS
                                    string tempAvatarLocation = Settings.DestinationPathAvatars + '\\' + userName + ".jpg";

                                    // the avatar file was not found at expected path, abort
                                    if (!FileSystemUtility.FileExists(tempAvatarLocation, Settings.DestinationPathAvatars))
                                        return false;

                                    // exec delete
                                    File.Delete(tempAvatarLocation);
                                }
                            }
                        }

                        // UNDONE: keep this updated as new features are added (delete sets etc)
                        // username will stay permanently reserved to prevent someone else from registering it and impersonating

                        db.SaveChanges();

                        return true;
                    }

                    // user account could not be found
                    return false;
                }
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
        public static string UserStylePreference()
        {
            string theme = "light";

            var tc = System.Web.HttpContext.Current.Request.Cookies["theme"];
            if (tc != null && !String.IsNullOrEmpty(tc.Value))
            {
                theme = tc.Value;
            }
            else if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {

                var userData = UserData.GetContextUserData();
                if (userData != null)
                {
                    theme = userData.Preferences.NightMode ? "dark" : "light";
                }
                SetUserStylePreferenceCookie(theme);
            }
            return theme;
        }
       

        // return user statistics for user profile overview
        public static UserStatsModel UserStatsModel(string userName)
        {
            var loadFunc = new Func<UserStatsModel>(() =>
            {
                var userStatsModel = new UserStatsModel();

                using (var db = new voatEntities())
                {
                    db.EnableCacheableOutput();

                    // 5 subverses user submitted to most
                    var subverses = db.Submissions.Where(a => a.UserName == userName && !a.IsAnonymized && !a.IsDeleted)
                             .GroupBy(a => new { a.UserName, a.Subverse })
                             .Select(g => new SubverseStats { SubverseName = g.Key.Subverse, Count = g.Count() })
                             .OrderByDescending(s => s.Count)
                             .Take(5)
                             .ToList();

                    // total comment count
                    var comments = db.Comments.Count(a => a.UserName == userName && !a.IsDeleted);

                    // voting habits
                    var userData = new Domain.UserData(userName);

                    var commentUpvotes = userData.Information.CommentVoting.UpCount;
                    var commentDownvotes = userData.Information.CommentVoting.DownCount;
                    var submissionUpvotes = userData.Information.SubmissionVoting.UpCount;
                    var submissionDownvotes = userData.Information.SubmissionVoting.DownCount;

                    //var commentUpvotes = db.CommentVoteTrackers.Count(a => a.UserName == userName && a.VoteStatus == 1);
                    //var commentDownvotes = db.CommentVoteTrackers.Count(a => a.UserName == userName && a.VoteStatus == -1);
                    //var submissionUpvotes = db.SubmissionVoteTrackers.Count(a => a.UserName == userName && a.VoteStatus == 1);
                    //var submissionDownvotes = db.SubmissionVoteTrackers.Count(a => a.UserName == userName && a.VoteStatus == -1);

                    // get 3 highest rated comments
                    var highestRatedComments = db.Comments
                        .Include("Submission").AsNoTracking()
                        .Where(a => a.UserName == userName && !a.IsAnonymized && !a.IsDeleted)
                        .OrderByDescending(s => s.UpCount - s.DownCount)
                        .Take(3)
                        .ToList();

                    // get 3 lowest rated comments
                    var lowestRatedComments = db.Comments
                        .Include("Submission").AsNoTracking()
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

                    //HACK: EF causes JSON to StackOverflow on the highest/lowest comments because of the nested loading EF does with the include option, therefore null the refs here.
                    highestRatedComments.ForEach(x => x.Submission.Comments = null);
                    lowestRatedComments.ForEach(x => x.Submission.Comments = null);
                }

                return userStatsModel;
            });

            var cachedData = CacheHandler.Instance.Register(CachingKey.UserOverview(userName), loadFunc, TimeSpan.FromMinutes(30));
            return cachedData;
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
       
        // check if a given user is subscribed to a given set
        public static bool IsUserSubscribedToSet(string userName, string setName)
        {
            using (var db = new voatEntities())
            {
                var result = db.UserSetSubscriptions.FirstOrDefault(s => s.UserSet.Name == setName && s.UserName == userName);
                return result != null;
            }
        }
        //[Obsolete("Arg Matie, you shipwrecked upon t'is Dead Code", true)]
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


        // subscribe to a set
        public static void SubscribeToSet(string userName, int setId)
        {
            throw new NotImplementedException("Sets have not been implemented on new command/query structure");
            //// do nothing if user is already subscribed
            //if (IsUserSetSubscriber(userName, setId))
            //{
            //    return;
            //}

            //using (var db = new voatEntities())
            //{
            //    // add a new set subscription
            //    var newSubscription = new UserSetSubscription { UserName = userName, UserSetID = setId };
            //    db.UserSetSubscriptions.Add(newSubscription);

            //    // record new set subscription in sets table subscribers field
            //    var tmpUserSet = db.UserSets.Find(setId);

            //    if (tmpUserSet != null)
            //    {
            //        tmpUserSet.SubscriberCount++;
            //    }

            //    db.SaveChanges();
            //}
        }

        // unsubscribe from a set
        public static void UnSubscribeFromSet(string userName, int setId)
        {
            throw new NotImplementedException("Sets have not been implemented on new command/query structure");
            //// do nothing if user is not subscribed to given set
            //if (!IsUserSetSubscriber(userName, setId))
            //{
            //    return;
            //}

            //using (var db = new voatEntities())
            //{
            //    var subscription = db.UserSetSubscriptions.FirstOrDefault(b => b.UserName == userName && b.UserSetID == setId);

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
       
        public static bool SimilarCommentSubmittedRecently(string userName, string commentContent)
        {
            // set starting date to 59 minutes ago from now
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, 0, -59, 0, 0));
            var toDate = Repository.CurrentDate;

            using (var db = new voatEntities())
            {
                var previousComment = db.Comments.FirstOrDefault(m => m.Content.Equals(commentContent, StringComparison.OrdinalIgnoreCase)
                    && m.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                    && m.CreationDate >= fromDate && m.CreationDate <= toDate);

                return previousComment != null;
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
        public static bool? IsSaved(Domain.Models.ContentType type, int id)
        {
            var identity = System.Threading.Thread.CurrentPrincipal.Identity;
            if (identity.IsAuthenticated)
            {
                string userName = identity.Name;
                string cacheKey = CachingKey.UserSavedItems(type, userName);
                if (!CacheHandler.Instance.Exists(cacheKey))
                {
                    var q = new QueryUserSaves(type);
                    var d = q.Execute();
                    return d.Contains(id);
                }
                else
                {
                    return CacheHandler.Instance.SetExists(cacheKey, id);
                }
            }
            return null;
        }
    }
}
