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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Logging;
using Voat.Utilities.Components;

namespace Voat.Domain
{
    //[Flags]
    //public enum UserDataPreLoad
    //{
    //    Information = 1,
    //    Preferences = 2,
    //    Subscriptions = 4,
    //    Blocks = 8
    //}

    /// <summary>
    /// The purpose of this class is to cache expensive user based queries that are repeatidly accessed.
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class UserData : SecurityContext<IPrincipal>
    {
        protected string _userNameInit;
        protected UserInformation _info;
        protected Domain.Models.UserPreference _prefs;
        //protected IEnumerable<string> _subverseSubscriptions;
        protected IDictionary<DomainType, IEnumerable<string>> _subscriptions;
        protected IEnumerable<string> _blockedSubverses;
        protected IEnumerable<string> _blockedUsers;

        protected int? _votesInLast24Hours;
        protected int? _submissionsInLast24Hours;

        //public async Task PreLoad(UserDataPreLoad preload)
        //{
        //    List<Task> tasks = new List<Task>();
        //    if (preload
        //    //This is to pre-cache user data that takes a long time to calc
        //    Task.Run(() => {
        //        var p = this.Preferences;
        //        var i = this.Information;
        //    });
        //}

        //public string UserName
        //{
        //    get
        //    {
        //        return _userName;
        //    }
        //}

        public static UserData GetContextUserData(HttpContext context)
        {
            UserData userData = null;
            if (context.User.Identity.IsAuthenticated)
            {
                var identity = context.User.Identity;
                var key = $"UserData:{identity.Name}";
                userData = ContextCache.Get<UserData>(context, key);
                if (userData == null)
                {
                    EventLogger.Instance.Log(LogType.Debug, "ContextCache", $"Not found: {key}");
                    userData = new UserData(context.User);
                    ContextCache.Set(context, key, userData);
                }
            }

            return userData;
        }
        public UserData(IPrincipal user) : base(user)
        {
            _userNameInit = user.Identity.Name;
        }
        public UserData(string userName, bool validateUserExists = false) : base(new ActivityContext(new ActivityPrincipal(new ActivityIdentity(userName))))
        {
            System.Diagnostics.Debug.WriteLine("UserData({0}, {1})", userName, validateUserExists.ToString());
            var val = UserDefinition.Parse(userName);
            if (val == null)
            {
                throw new ArgumentException("UserName does not meet expectations");
            }

            if (validateUserExists)
            {
                VoatIdentityUser user = null;
                if (!String.IsNullOrWhiteSpace(userName))
                {
                    using (var repo = VoatUserManager.Create())
                    {
                        user = repo.FindByName(userName);
                    }
                }
                if (user == null)
                {
                    throw new VoatNotFoundException("User doesn't exist");
                }
            }
            _userNameInit = userName;
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int TotalVotesUsedIn24Hours
        {
            get
            {
                var val = GetOrLoad(ref _votesInLast24Hours, userName =>
                {
                    Debug.WriteLine("UserData[{0}].TotalVotesUsedIn24Hours(loading)", userName);
                    using (var repo = new Repository(User))
                    {
                        return repo.UserVotingBehavior(userName, ContentType.Comment | ContentType.Submission, TimeSpan.FromDays(1)).Total;
                    }

                    //return UserGateway.TotalVotesUsedInPast24Hours(userName);
                }, false);
                return (val.HasValue ? val.Value : 0);
            }

            set
            {
                _votesInLast24Hours = value;
                Recache();
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int TotalSubmissionsPostedIn24Hours
        {
            get
            {
                var val = GetOrLoad(ref _submissionsInLast24Hours, userName =>
                {
                    Debug.WriteLine("UserData[{0}].TotalSubmissionsPostedIn24Hours(loading)", userName);
                    using (var repo = new Repository(User))
                    {
                        return repo.UserContributionCount(userName, ContentType.Submission, null, new DateRange(TimeSpan.FromDays(1)));
                    }

                    //return UserGateway.TotalVotesUsedInPast24Hours(userName);
                }, false);
                return (val.HasValue ? val.Value : 0);
            }

            set
            {
                _votesInLast24Hours = value;
                Recache();
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IEnumerable<string> BlockedSubverses
        {
            get
            {
                var val = GetOrLoad(ref _blockedSubverses, userName =>
                {
                    Debug.WriteLine("UserData[{0}].BlockedSubverses(loading)", userName);
                    var q = new QueryUserBlocks().SetUserContext(User);
                    var r = q.Execute();
                    return r.Where(x => x.Type == DomainType.Subverse).Select(x => x.Name);
                }, false);
                return val;
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IEnumerable<string> BlockedUsers
        {
            get
            {
                var val = GetOrLoad(ref _blockedUsers, userName =>
                {
                    Debug.WriteLine("UserData[{0}].BlockedUsers(loading)", userName);
                    var q = new QueryUserBlocks().SetUserContext(User);
                    var r = q.Execute();
                    return r.Where(x => x.Type == DomainType.User).Select(x => x.Name);
                }, false);
                return val;
            }
        }
        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Domain.Models.UserPreference Preferences
        {
            get
            {
                return GetOrLoad(ref _prefs, userName =>
                {
                    Debug.WriteLine("UserData[{0}].Preferences(loading)", userName);
                    var q = new QueryUserPreferences(userName).SetUserContext(User);
                    var result = q.Execute();
                    return result;
                }, false);
            }

            set
            {
                _prefs = value;
                Recache();
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IDictionary<DomainType, IEnumerable<string>> Subscriptions
        {
            get
            {
                return GetOrLoad(ref _subscriptions, userName =>
                {
                    Debug.WriteLine("UserData[{0}].Subscriptions(loading)", userName);
                    var q = new QueryUserSubscriptions(userName).SetUserContext(User);
                    var result = q.Execute();
                    return result;
                }, false);

            }

        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IEnumerable<string> SubverseSubscriptions
        {
            get
            {
                var subs = Subscriptions;
                if (subs != null && subs.ContainsKey(DomainType.Subverse))
                {
                    return subs[DomainType.Subverse];
                }
                else
                {
                    return new List<string>();
                }
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public UserInformation Information
        {
            get
            {
                return GetOrLoad(ref _info, userName =>
                {
                    Debug.WriteLine("UserData[{0}].Information(loading)", userName);
                    var q = new QueryUserInformation(userName).SetUserContext(User);
                    var result = q.Execute();
                    return result;
                }, false);
            }
        }
        //TODO: THis code needs to be wrapped and cached in a command
        public int ContributionPointsForSubverse(string subverse)
        {
            using (var repo = new Repository(User))
            {
                return repo.UserContributionPoints(_userNameInit, ContentType.Comment, subverse).Sum;
            }
        }
        private T GetOrLoad<T>(ref T value, Func<string, T> loadFunc, bool recacheOnLoad = true)
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                value = loadFunc(_userNameInit);
                if (recacheOnLoad)
                {
                    Recache();
                }
            }
            return value;
        }

        private void Recache()
        {
            Task.Run(() => CacheHandler.Instance.Replace<UserData>(CachingKey.UserData(UserName), this, TimeSpan.FromMinutes(5)));
        }
        #region 

        public bool IsUserSubverseSubscriber(string subverse)
        {
            return IsSubscriber(new DomainReference(DomainType.Subverse, subverse));
        }
        public bool IsSubscriber(DomainReference domainReference)
        {
            var subs = GetSubscriptions(domainReference.Type);
            return subs.Any(x => x.IsEqual(domainReference.FullName));
        }
        private IEnumerable<string> GetSubscriptions(DomainType type)
        {
            var subs = Subscriptions;
            if (subs.ContainsKey(type) && subs[type].Any())
            {
                return subs[type];
            }
            else
            {
                return new List<string>();
            }
        }
        public bool IsUserBlockingSubverse(string subverse)
        {
            var result = BlockedSubverses.Any(x => x.ToLower() == subverse.ToLower());
            return result;
        }
        public bool HasSubscriptions(DomainType type = DomainType.Subverse)
        {
            var subs = GetSubscriptions(type);
            return subs.Any();
        }
        #endregion
    }
}
