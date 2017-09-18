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
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Utilities;
using System.Linq;
using Voat.Common;
using System.Threading.Tasks;
using Voat.IO;
using Voat.Common.Models;

namespace Voat.Domain.Query
{
    public class QueryUserInformation : CachedQuery<UserInformation>
    {
        private string _userToRetrieve;
        private static TimeSpan _totalCacheTime = TimeSpan.FromHours(24);
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
                    CacheHandler.Replace(FullCacheKey, await GetData(), QueryUserInformation._totalCacheTime);
                });
            }
            return data;
        }


        protected override async Task<UserInformation> GetData()
        {
            using (var db = new Repository(User))
            {
                var data = await db.GetUserInformation(_userToRetrieve);
                if (data != null)
                {
                    if (data.Badges != null)
                    {
                        data.Badges.ToList().ForEach(x => x.Graphic = FileManager.Instance.Uri(new FileKey(x.Graphic, FileType.Badge)));
                        //data.Badges.ToList().ForEach(x => x.Graphic = VoatUrlFormatter.BadgePath(null, x.Graphic, true, true));
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
