using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Models;
using Voat.Domain.Query;

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
    public class UserData
    {
        protected string _userName;
        protected UserInformation _info;
        protected Data.Models.UserPreference _prefs;
        protected IEnumerable<string> _subverseSubscriptions;
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

        public string UserName
        {
            get
            {
                return _userName;
            }
        }

        public static UserData GetContextUserData()
        {
            UserData userData = null;

            var identity = System.Threading.Thread.CurrentPrincipal.Identity;
            if (identity != null && identity.IsAuthenticated && !String.IsNullOrEmpty(identity.Name))
            {
                var key = $"UserData:{identity.Name}";
                userData = ContextCache.Get<UserData>(key);
                if (userData == null)
                {
                    identity = System.Threading.Thread.CurrentPrincipal.Identity;
                    userData = new UserData(identity.Name);
                    ContextCache.Set(key, userData);
                }
            }

            return userData;
        }

        public UserData(string userName, bool validateUserExists = false)
        {

            System.Diagnostics.Debug.Print("UserData({0}, {1})", userName, validateUserExists.ToString());

            var val = UserDefinition.Parse(userName);
            if (val == null)
            {
                throw new ArgumentException("UserName does not meet expectations");
            }

            if (validateUserExists)
            {
                VoatUser user = null;
                if (!String.IsNullOrWhiteSpace(userName))
                {
                    using (var repo = new UserManager<VoatUser>(new UserStore<VoatUser>(new ApplicationDbContext())))
                    {
                        user = repo.FindByName(userName);
                    }
                }
                if (user == null)
                {
                    throw new VoatNotFoundException("User doesn't exist");
                }
            }
            this._userName = userName;
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int TotalVotesUsedIn24Hours
        {
            get
            {
                var val = GetOrLoad(ref _votesInLast24Hours, username =>
                {
                    using (var repo = new Repository())
                    {
                        return repo.UserVotingBehavior(username, ContentType.Comment | ContentType.Submission, TimeSpan.FromDays(1)).Total;
                    }

                    //return UserGateway.TotalVotesUsedInPast24Hours(username);
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
                var val = GetOrLoad(ref _submissionsInLast24Hours, username =>
                {
                    using (var repo = new Repository())
                    {
                        return repo.UserSubmissionCount(username, TimeSpan.FromDays(1));
                    }

                    //return UserGateway.TotalVotesUsedInPast24Hours(username);
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
                var val = GetOrLoad(ref _blockedSubverses, username =>
                {
                    var q = new QueryUserBlocks();
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
                var val = GetOrLoad(ref _blockedUsers, username =>
                {
                    var q = new QueryUserBlocks();
                    var r = q.Execute();
                    return r.Where(x => x.Type == DomainType.User).Select(x => x.Name);
                }, false);
                return val;
            }
        }
        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        //public bool IsSubscriber
        //{
        //    get
        //    {
        //        var isSubscriber = false;
        //        var subBadge = Information.Badges.FirstOrDefault(x => x.Name == "Subscriber");
        //        if (subBadge != null)
        //        {
        //            isSubscriber = Repository.CurrentDate.Subtract(subBadge.CreationDate) < TimeSpan.FromDays(180);
        //        }
        //        return isSubscriber;
        //    }
        //}
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Data.Models.UserPreference Preferences
        {
            get
            {
                return GetOrLoad(ref _prefs, username =>
                {
                    var q = new QueryUserPreferences(username);
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
        public IEnumerable<string> Subscriptions
        {
            get
            {
                return GetOrLoad(ref _subverseSubscriptions, username =>
                {
                    var q = new QueryUserSubscriptions(username);
                    var result = q.Execute();
                    if (result != null && result.ContainsKey(DomainType.Subverse))
                    {
                        return result[DomainType.Subverse];
                    }
                    else
                    {
                        return new List<string>(); //No subs
                    }
                }, false);
            }

            set
            {
                _subverseSubscriptions = value;
                Recache();
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public UserInformation Information
        {
            get
            {
                return GetOrLoad(ref _info, username =>
                {
                    var q = new QueryUserInformation(username);
                    var result = q.Execute();
                    return result;
                }, false);
            }
        }

        private T GetOrLoad<T>(ref T value, Func<string, T> loadFunc, bool recacheOnLoad = true)
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                Debug.Print("UserData[{0}].GetOrLoad({1}...)", UserName, typeof(T).Name);
                value = loadFunc(this._userName);
                if (recacheOnLoad)
                {
                    Recache();
                }
            }
            return value;
        }

        private void Recache()
        {
            Task.Run(() => CacheHandler.Instance.Replace<UserData>(CachingKey.UserData(this._userName), this, TimeSpan.FromMinutes(5)));
        }
        #region 

        public bool IsUserSubverseSubscriber(string subverse)
        {
            return Subscriptions.Any(x => x.Equals(subverse, StringComparison.OrdinalIgnoreCase));
        }
        public bool IsUserBlockingSubverse(string subverse)
        {
            return BlockedSubverses.Any(x => x.Equals(subverse, StringComparison.OrdinalIgnoreCase));
        }
        public bool HasSubscriptions()
        {
            return Subscriptions.Any();
        }
        #endregion
    }
}
