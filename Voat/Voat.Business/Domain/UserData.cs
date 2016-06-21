using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Utilities;

namespace Voat.Domain
{

    /// <summary>
    /// The purpose of this class is to cache expensive user based queries that are repeatidly accessed.
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class UserData
    {
        protected string _userName;
        protected UserInformation _info;
        protected UserPreference _prefs;
        protected IEnumerable<string> _subverseSubscriptions;
        protected IEnumerable<string> _blockedSubverses;
        protected IEnumerable<string> _blockedUsers;

        protected int? _votesInLast24Hours;
        protected int? _submissionsInLast24Hours;

        public UserData(string userName)
        {
            this._userName = userName;
        }
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
        public bool IsSubscriber
        {
            get
            {
                var isSubscriber = false;
                var subBadge = Information.Badges.FirstOrDefault(x => x.Name == "Subscriber");
                if (subBadge != null)
                {
                    isSubscriber = Repository.CurrentDate.Subtract(subBadge.CreationDate) < TimeSpan.FromDays(180);
                }
                return isSubscriber;
            }

        }
        public UserPreference Preferences
        {
            get
            {
                return GetOrLoad(ref _prefs, username => {
                    var q = new QueryUserPreferences(username);
                    var result = q.Execute();
                    return result;
                });
            }
            set
            {
                _prefs = value;
                Recache();
            }
        }
        public IEnumerable<string> Subscriptions
        {
            get
            {
                return GetOrLoad(ref _subverseSubscriptions, username => {
                    var q = new QueryUserSubscriptions(username);
                    var result = q.Execute();
                    if (result != null && result.ContainsKey("Subverse"))
                    {
                        return result["Subverse"];
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
        public UserInformation Information
        {
            get
            {
                return GetOrLoad(ref _info, username => {
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
                Debug.Print("Calling Load Func");
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
    }
}
