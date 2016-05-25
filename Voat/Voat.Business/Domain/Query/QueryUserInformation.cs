using System;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Utilities;
using System.Linq;
using Voat.Common;

namespace Voat.Domain.Query
{
    public class QueryUserInformation : CachedQuery<UserInformation>
    {
        private string _userToRetrieve;

        public QueryUserInformation(string userToRetrieve) : this(userToRetrieve, new CachePolicy(TimeSpan.FromMinutes(5)))
        {
        }

        public QueryUserInformation(string userToRetrieve, CachePolicy policy) : base(policy)
        {
            this._userToRetrieve = userToRetrieve;
        }

        public override string CacheKey
        {
            get
            {
                return _userToRetrieve;
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.UserInformation(_userToRetrieve);
            }
        }

        protected override UserInformation GetData()
        {
            using (var db = new Repository())
            {
                var data = db.GetUserInfo(_userToRetrieve);
                if (data != null)
                {
                    if (data.Badges != null)
                    {
                        data.Badges.ToList().ForEach(x => x.Graphic = VoatPathHelper.BadgePath(x.Graphic, true, true));
                    }
                    var moderates = db.GetSubversesUserModerates(_userToRetrieve);
                    if (moderates != null)
                    {
                        data.Moderates = moderates.Select(x => x.Subverse).ToList();
                    }
                }
                //TODO: Need to ensure this condition doesn't happen often, throwing exception to test.
                else
                {
                    throw new VoatNotFoundException(String.Format("Can not find UserInformation for {0}", _userToRetrieve));
                }
               
                return data;
            }
        }
    }
}
