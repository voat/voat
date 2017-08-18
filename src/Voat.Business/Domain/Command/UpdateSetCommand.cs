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
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;

namespace Voat.Domain.Command
{
    public class UpdateSetCommand : CacheCommand<CommandResponse<Domain.Models.Set>>, IExcutableCommand<CommandResponse<Domain.Models.Set>>
    {
        private Domain.Models.Set _set = null;

        public UpdateSetCommand(Domain.Models.Set set)
        {
            _set = set;
        }

        protected override async Task<CommandResponse<Domain.Models.Set>> CacheExecute()
        {
            using (var repo = new Repository(User))
            {
                var response = await repo.CreateOrUpdateSet(_set);

                return response;

            }
        }

        protected override void UpdateCache(CommandResponse<Domain.Models.Set> result)
        {
            if (result.Success)
            {
                CacheHandler.Instance.Remove(CachingKey.UserSubscriptions(UserName));
                CacheHandler.Instance.Remove(CachingKey.Set(this._set.Name, this._set.UserName));
            }
        }
    }
}



