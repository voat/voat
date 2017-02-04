using System;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Utilities;
using System.Linq;
using Voat.Common;
using System.Threading.Tasks;

namespace Voat.Domain.Query
{
    public class QueryUserInformation : CachedQuery<UserInformation>
    {
        private string _userToRetrieve;
        private static TimeSpan _totalCacheTime = TimeSpan.FromHours(12);
        private TimeSpan _refreshTime = TimeSpan.FromMinutes(15);

        public QueryUserInformation(string userToRetrieve)
            : this(userToRetrieve, new CachePolicy(QueryUserInformation._totalCacheTime))
        {
        }

        public QueryUserInformation(string userToRetrieve, CachePolicy policy)
            : base(policy)
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

        public override async Task<UserInformation> ExecuteAsync()
        {
            var data = await base.ExecuteAsync();
            //See if data is static and Update in backgroud if old
            if (Repository.CurrentDate.Subtract(data.GenerationDate) > _refreshTime)
            {
                Task.Run(async () => {
                    CacheHandler.Instance.Replace(FullCacheKey, await GetData(), QueryUserInformation._totalCacheTime);
                });
            }
            return data;
        }


        protected override async Task<UserInformation> GetData()
        {
            using (var db = new Repository())
            {
                var data = await db.GetUserInformation(_userToRetrieve);
                if (data != null)
                {
                    if (data.Badges != null)
                    {
                        data.Badges.ToList().ForEach(x => x.Graphic = VoatPathHelper.BadgePath(x.Graphic, true, true));
                    }
                    var moderates = db.GetSubversesUserModerates(_userToRetrieve);
                    if (moderates != null)
                    {
                        data.Moderates = moderates.Select(x => new SubverseModerator() { Subverse = x.Subverse, Level = (ModeratorLevel)Enum.Parse(typeof(ModeratorLevel), x.Power.ToString()) }).ToList();
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
