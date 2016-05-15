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
        protected int? _votesInLast24Hours;

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
                    return UserGateway.TotalVotesUsedInPast24Hours(username);
                });
                return (val.HasValue ? val.Value : 0);
            }
            set
            {
                _votesInLast24Hours = value;
                Recache();
            }
        }

        public UserPreference Preferences
        {
            get
            {
                return GetOrLoad(ref _prefs, username => {
                    var q = new QueryUserPreferences(username);
                    var result = Task.Run(() => q.Execute()).Result;
                    return result;
                });
            }
            set
            {
                _prefs = value;
                Recache();
            }
        }
       
        public UserInformation Information
        {
            get
            {
                return GetOrLoad(ref _info, username => {
                    var q = new QueryUserInformation(username);
                    var result = Task.Run(() => q.Execute()).Result;
                    return result;
                });
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
