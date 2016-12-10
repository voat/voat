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
    public class UserData
    {
        protected string _userName;
        protected UserInformation _info;
        protected Data.Models.UserPreference _prefs;
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
                    EventLogger.Instance.Log(LogType.Debug, "ContextCache", $"Not found: {key}");
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
                var val = GetOrLoad(ref _votesInLast24Hours, userName =>
                {
                    Debug.Print("UserData[{0}].TotalVotesUsedIn24Hours(loading)", userName);
                    using (var repo = new Repository())
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
                    Debug.Print("UserData[{0}].TotalSubmissionsPostedIn24Hours(loading)", userName);
                    using (var repo = new Repository())
                    {
                        return repo.UserSubmissionCount(userName, TimeSpan.FromDays(1));
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
                    Debug.Print("UserData[{0}].BlockedSubverses(loading)", userName);
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
                var val = GetOrLoad(ref _blockedUsers, userName =>
                {
                    Debug.Print("UserData[{0}].BlockedUsers(loading)", userName);
                    var q = new QueryUserBlocks();
                    var r = q.Execute();
                    return r.Where(x => x.Type == DomainType.User).Select(x => x.Name);
                }, false);
                return val;
            }
        }
        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Data.Models.UserPreference Preferences
        {
            get
            {
                return GetOrLoad(ref _prefs, userName =>
                {
                    Debug.Print("UserData[{0}].Preferences(loading)", userName);
                    var q = new QueryUserPreferences(userName);
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
                    Debug.Print("UserData[{0}].Subscriptions(loading)", userName);
                    var q = new QueryUserSubscriptions(userName);
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
                    Debug.Print("UserData[{0}].Information(loading)", userName);
                    var q = new QueryUserInformation(userName);
                    var result = q.Execute();
                    return result;
                }, false);
            }
        }

        private T GetOrLoad<T>(ref T value, Func<string, T> loadFunc, bool recacheOnLoad = true)
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T)))
            {
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
            return IsSubscriber(DomainType.Subverse, subverse);
        }
        public bool IsSubscriber(DomainType type, string name)
        {
            var subs = GetSubscriptions(type);
            return subs.Any(x => x.IsEqual(name));
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
            return BlockedSubverses.Any(x => x.Equals(subverse, StringComparison.OrdinalIgnoreCase));
        }
        public bool HasSubscriptions(DomainType type = DomainType.Subverse)
        {
            var subs = GetSubscriptions(type);
            return subs.Any();
        }
        #endregion
    }
}
