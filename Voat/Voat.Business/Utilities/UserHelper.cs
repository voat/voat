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
using Voat.Data.Models;
using Voat.Models;
using Voat.Data;
using Voat.Domain.Query;
using System.Net.Http;
using Voat.Caching;
using System.Threading;
using Voat.Domain;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Security.Principal;
using Voat.Domain.Command;
using Voat.Common;
using System.Threading.Tasks;
using Voat.Business.Utilities;
using Microsoft.EntityFrameworkCore;
using Voat.Configuration;

namespace Voat.Utilities
{
    public static class UserHelper
    {

        
        //TODO: IMPORTANT This needs to be ported correctly, not using VoatUserManager param any longer
        public static async Task<bool> CanUserNameBeRegistered(VoatUserManager userManager, string userName, IDictionary<string, string> charSwaps = null)
        {

            //List<string> spoofsToCheck = new List<string>();
            //spoofsToCheck.Add(userName); //add original username

            ////process deviations
            //if (spoofSubstitutionFuncList != null && spoofSubstitutionFuncList.Any())
            //{
            //    foreach (var spoofFunc in spoofSubstitutionFuncList)
            //    {
            //        var l = spoofFunc(userName);
            //        if (l != null && l.Any())
            //        {
            //            spoofsToCheck.AddRange(l.Where(x => !String.IsNullOrEmpty(x) && !userName.IsEqual(x)).ToList()); //only add valid items
            //        }
            //    }
            //}

            //TODO: Need to migrate to dapper and repo
            //var accountExists = spoofsToCheck.Any(x => userManager.FindByNameAsync(x).Result != null);
            var spoofsToCheck = SpooferProofer.CharacterSwapList(userName, charSwaps, true, Normalization.Lower);
            var accountExists = await VoatUserManager.UserNameExistsAsync(spoofsToCheck);
            return !accountExists;

        }

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

        // check which theme style user selected
        public static void SetUserStylePreferenceCookie(HttpContext context, string theme)
        {

            context.Response.Cookies.Append("theme", 
                theme, 
                new CookieOptions() {
                    Expires = Repository.CurrentDate.AddDays(14)
                });
        }

        // check which theme style user selected
        public static string UserStylePreference(HttpContext context)
        {

            string theme = VoatSettings.Instance.SiteThemeDefault;

            var tc = context.Request.Cookies["theme"];
            if (!String.IsNullOrEmpty(tc))
            {
                theme = tc;
            }
            else if (context.User.Identity.IsAuthenticated)
            {

                var userData = UserData.GetContextUserData(context);
                if (userData != null)
                {
                    theme = userData.Preferences.NightMode ? "dark" : "light";
                }
                SetUserStylePreferenceCookie(context, theme);
            }
            return theme;
        }
        // return user statistics for user profile overview
        public static UserStatsModel UserStatsModel(string userName)
        {
            var loadFunc = new Func<UserStatsModel>(() =>
            {
                var userStatsModel = new UserStatsModel();

                using (var db = new VoatOutOfRepositoryDataContextAccessor())
                {
                    // 5 subverses user submitted to most
                    var subverses = db.Submission.Where(a => a.UserName == userName && !a.IsAnonymized && !a.IsDeleted)
                             .GroupBy(a => new { a.UserName, a.Subverse })
                             .Select(g => new SubverseStats { SubverseName = g.Key.Subverse, Count = g.Count() })
                             .OrderByDescending(s => s.Count)
                             .Take(5)
                             .ToList();

                    // total comment count
                    var comments = db.Comment.Count(a => a.UserName == userName && !a.IsDeleted);

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
                    var highestRatedComments = db.Comment
                        .Include(x => x.Submission)
                        .Where(a => a.UserName == userName && !a.IsAnonymized && !a.IsDeleted)
                        .OrderByDescending(s => s.UpCount - s.DownCount)
                        .Take(3)
                        .ToList();

                    // get 3 lowest rated comments
                    var lowestRatedComments = db.Comment
                        .Include(x => x.Submission)
                        .Where(a => a.UserName == userName && !a.IsAnonymized && !a.IsDeleted)
                        .OrderBy(s => s.UpCount - s.DownCount)
                        .Take(3)
                        .ToList();

                    var linkSubmissionsCount = db.Submission.Count(a => a.UserName == userName && a.Type == 2 && !a.IsDeleted && !a.IsAnonymized);
                    var messageSubmissionsCount = db.Submission.Count(a => a.UserName == userName && a.Type == 1 && !a.IsDeleted && !a.IsAnonymized);

                    // get 5 highest rated submissions
                    var highestRatedSubmissions = db.Submission.Where(a => a.UserName == userName && !a.IsAnonymized && !a.IsDeleted)
                        .OrderByDescending(s => s.UpCount - s.DownCount)
                        .Take(5)
                        .ToList();

                    // get 5 lowest rated submissions
                    var lowestRatedSubmissions = db.Submission.Where(a => a.UserName == userName && !a.IsAnonymized && !a.IsDeleted)
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

                    ////HACK: EF causes JSON to StackOverflow on the highest/lowest comments because of the nested loading EF does with the include option, therefore null the refs here.
                    //highestRatedComments.ForEach(x => x.Submission.Comments = null);
                    //lowestRatedComments.ForEach(x => x.Submission.Comments = null);
                }

                return userStatsModel;
            });

            var cachedData = CacheHandler.Instance.Register(CachingKey.UserOverview(userName), loadFunc, TimeSpan.FromMinutes(30));
            return cachedData;
        }

        // check if a given user is globally banned
        public static bool IsUserGloballyBanned(string userName)
        {
            using (var repo = new Repository())
            {
                var ban = repo.UserBan(userName, null, true);
                return ban != null;
            }
        }

        // check if a given user is banned from a subverse
        public static bool IsUserBannedFromSubverse(string userName, string subverseName)
        {
            using (var repo = new Repository())
            {
                var ban = repo.UserBan(userName, subverseName, false); //backwards compat did not check global bans
                return ban != null;
            }
        }
       
        //// check if a given user is subscribed to a given set
        //public static bool IsUserSubscribedToSet(string userName, string setName)
        //{
        //    using (var db = new voatEntities())
        //    {
        //        var result = db.UserSetSubscriptions.FirstOrDefault(s => s.UserSet.Name == setName && s.UserName == userName);
        //        return result != null;
        //    }
        //}
        ////[Obsolete("Arg Matie, you shipwrecked upon t'is Dead Code", true)]
        //// check if a given user is owner of a given set
        //public static bool IsUserSetOwner(string userName, int setId)
        //{
        //    using (var db = new voatEntities())
        //    {
        //        var result = db.UserSets.FirstOrDefault(s => s.ID == setId && s.CreatedBy == userName);
        //        return result != null;
        //    }
        //}

        //// return sets subscription count for a given user
        //public static int SetsSubscriptionCount(string userName)
        //{
        //    using (var db = new voatEntities())
        //    {
        //        return db.UserSetSubscriptions.Count(s => s.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
        //    }
        //}


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
        public static bool? IsSaved(IPrincipal user, Domain.Models.ContentType type, int id)
        {
            var identity = user.Identity;
            if (identity.IsAuthenticated)
            {
                var q = new QueryUserSaves(type).SetUserContext(user);
                var d = q.Execute();
                return d.Contains(id);

                //string userName = identity.Name;
                //string cacheKey = CachingKey.UserSavedItems(type, userName);
                //if (!CacheHandler.Instance.Exists(cacheKey))
                //{
                //    var q = new QueryUserSaves(type).SetUserContext(user);
                //    var d = q.Execute();
                //    return d.Contains(id);
                //}
                //else
                //{
                //    return CacheHandler.Instance.SetExists(cacheKey, id);
                //}
            }
            return null;
        }
    }
}
