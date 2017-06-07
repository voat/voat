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

namespace Voat.Utilities
{
    public static class UserHelper
    {

        public static IEnumerable<Func<string, IEnumerable<string>>> DefaultSpoofList(IDictionary<string, string> charSwaps = null)
        {
           
            List<Func<string, IEnumerable<string>>> spoofs = new List<Func<string, IEnumerable<string>>>();

            spoofs.Add(new Func<string, IEnumerable<string>>((userName) => {

                List<string> l = new List<string>();

                if (charSwaps == null || !charSwaps.Any())
                {
                    charSwaps = new Dictionary<string, string>();
                    charSwaps.Add("i", "l");
                    charSwaps.Add("o", "0");
                    //charSwaps.Add("h", "hahaha"); //just to make sure offset swapping does not break
                    //charSwaps.Add("heart", "like"); //just to make sure offset swapping does not break
                }
                string allSwapped = userName;

                Action<string, string, List<string>> processSwap = new Action<string, string, List<string>>((string1, string2, list) => {

                    var userArray = list.ToArray();
                    foreach (var username in userArray)
                    {
                        var lusername = username.ToLower();
                        if (lusername.Contains(string1.ToLower()))
                        {
                            //Add straight swap (all)
                            list.Add(lusername.Replace(string1.ToLower(), string2.ToLower()));

                            //replace each individual occurance
                            var matches = Regex.Matches(lusername, string1, RegexOptions.IgnoreCase);
                            if (matches.Count > 1) //If it has 1 match the above line already swapped it
                            {
                                //rolling sub
                                string rollingUserName = lusername;
                                var offset = 0;
                                var substitution = string2;
                                var rollingSwap = lusername;

                                List<Match> reverseProcessing = new List<Match>();

                                foreach (Match m in matches)
                                {
                                    reverseProcessing.Add(m);
                                    //Concat method (fractions of milliseconds faster)
                                    rollingSwap = String.Concat(rollingSwap.Substring(0, m.Index + offset), substitution, rollingSwap.Substring(m.Index + m.Length + offset, rollingSwap.Length - (m.Length + m.Index + offset)));
                                    list.Add(rollingSwap);
                                    offset += substitution.Length - m.Length;

                                    var individualSwap = String.Concat(rollingSwap.Substring(0, m.Index), substitution, rollingSwap.Substring(m.Index + m.Length, rollingSwap.Length - (m.Length + m.Index)));
                                    list.Add(individualSwap);
                                }

                                //Reverse swaps
                                offset = 0;
                                substitution = string2;
                                rollingSwap = lusername;
                                reverseProcessing.Reverse();
                                foreach (Match m in reverseProcessing)
                                {
                                    //Concat method (fractions of milliseconds faster)
                                    rollingSwap = String.Concat(rollingSwap.Substring(0, m.Index + offset), substitution, rollingSwap.Substring(m.Index + m.Length + offset, rollingSwap.Length - (m.Length + m.Index + offset)));
                                    list.Add(rollingSwap);
                                    //offset += substitution.Length - m.Length;
                                }
                            }
                        }
                    }
                });

                l.Add(userName);
                foreach (var swap in charSwaps)
                {


                    //swap key for value
                    processSwap(swap.Key, swap.Value, l);
                    //var swapped = userName.ToLower().Replace(swap.Key.ToLower(), swap.Value.ToLower());
                    //allSwapped = allSwapped.ToLower().Replace(swap.Key.ToLower(), swap.Value.ToLower());
                    //l.Add(swapped);
                    //l.Add(allSwapped);




                    //swap value for key
                    processSwap(swap.Value, swap.Key, l);
                    //swapped = userName.ToLower().Replace(swap.Value.ToLower(), swap.Key.ToLower());
                    //allSwapped = allSwapped.ToLower().Replace(swap.Value.ToLower(), swap.Key.ToLower());
                    //l.Add(swapped);
                    //l.Add(allSwapped);

                }

                return l.Distinct().ToList();

            }));

            return spoofs;
        }

        public static bool CanUserNameBeRegistered(VoatUserManager userManager, string userName, IEnumerable<Func<string, IEnumerable<string>>> spoofSubstitutionFuncList = null)
        {
            
            List<string> spoofsToCheck = new List<string>();
            spoofsToCheck.Add(userName); //add original username

            //process deviations
            if (spoofSubstitutionFuncList != null && spoofSubstitutionFuncList.Any())
            {
                foreach (var spoofFunc in spoofSubstitutionFuncList)
                {
                    var l = spoofFunc(userName);
                    if (l != null && l.Any())
                    {
                        spoofsToCheck.AddRange(l.Where(x => !String.IsNullOrEmpty(x) && !userName.IsEqual(x)).ToList()); //only add valid items
                    }
                }
            }

            //TODO: Need to migrate to dapper and repo
            var accountExists = spoofsToCheck.Any(x => userManager.FindByName(x) != null);
            if (accountExists)
            {
                return false;
            }
            else
            {
                return true;
            }

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

            string theme = "light";

            var tc = context.Request.Cookies["theme"];
            if (!String.IsNullOrEmpty(tc))
            {
                theme = tc;
            }
            else if (UserIdentity.IsAuthenticated)
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

                using (var db = new VoatDataContext())
                {
                    db.EnableCacheableOutput();

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
                        //CORE_PORT: EF has changed
                        //.Include("Submission")
                        .Where(a => a.UserName == userName && !a.IsAnonymized && !a.IsDeleted)
                        .OrderByDescending(s => s.UpCount - s.DownCount)
                        .Take(3)
                        .ToList();

                    // get 3 lowest rated comments
                    var lowestRatedComments = db.Comment
                        //CORE_PORT: EF has changed
                        //.Include("Submission")
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
            using (var db = new VoatDataContext())
            {
                var bannedUser = db.BannedUser.FirstOrDefault(n => n.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
                return bannedUser != null;
            }
        }

        // check if a given user is banned from a subverse
        public static bool IsUserBannedFromSubverse(string userName, string subverseName)
        {
            using (var db = new VoatDataContext())
            {
                var bannedUser = db.SubverseBan.FirstOrDefault(n => n.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) && n.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase));
                return bannedUser != null;
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
            using (var db = new VoatDataContext())
            {
                // get voting habits
                var commentUpvotes = db.CommentVoteTracker.Count(a => a.UserName == userName && a.VoteStatus == 1);
                var commentDownvotes = db.CommentVoteTracker.Count(a => a.UserName == userName && a.VoteStatus == -1);

                return commentDownvotes > commentUpvotes;
            }
        }

        // check if a given user has downvoted more submissions than upvoted
        public static bool IsUserSubmissionVotingMeanie(string userName)
        {
            using (var db = new VoatDataContext())
            {
                // get voting habits
                var submissionUpvotes = db.CommentVoteTracker.Count(a => a.UserName == userName && a.VoteStatus == 1);
                var submissionDownvotes = db.CommentVoteTracker.Count(a => a.UserName == userName && a.VoteStatus == -1);

                return submissionDownvotes > submissionUpvotes;
            }
        }
       
        public static bool SimilarCommentSubmittedRecently(string userName, string commentContent)
        {
            // set starting date to 59 minutes ago from now
            var fromDate = Repository.CurrentDate.Add(new TimeSpan(0, 0, -59, 0, 0));
            var toDate = Repository.CurrentDate;

            using (var db = new VoatDataContext())
            {
                var previousComment = db.Comment.FirstOrDefault(m => m.Content.Equals(commentContent, StringComparison.OrdinalIgnoreCase)
                    && m.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)
                    && m.CreationDate >= fromDate && m.CreationDate <= toDate);

                return previousComment != null;
            }
        }

        //CORE_PORT: Don't know what request obj to use so commenting out temp
        /*
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
        */
        public static string UserIpAddress(HttpRequest request)
        {
            string IPAddressHeaderKeys = "CF-Connecting-IP, X-Original-For"; //TODO: Move to Settings

            var keys = IPAddressHeaderKeys.Split(',', ';').Select(x => x.TrimSafe());
            string clientIpAddress = String.Empty;

            foreach (var key in keys)
            {
                if (request.Headers.ContainsKey(key))
                {
                    clientIpAddress = request.Headers[key];
                    if (!String.IsNullOrEmpty(clientIpAddress))
                    {
                        break;
                    }
                }
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
            var identity = UserIdentity.Principal.Identity;
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
