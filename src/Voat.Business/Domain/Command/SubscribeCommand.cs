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

using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    public class SubscribeCommand : CacheCommand<CommandResponse<bool?>>
    {
        protected SubscriptionAction _action;
        protected DomainReference _domainReference;

        public SubscribeCommand(DomainReference domainReference, SubscriptionAction action)
        {
            _action = action;
            _domainReference = domainReference;
        }

        protected override async Task<CommandResponse<bool?>> CacheExecute()
        {
            using (var repo = new Repository(User))
            {
                var result = await repo.SubscribeUser(_domainReference, _action);
                return result;
            }
        }

        protected override void UpdateCache(CommandResponse<bool?> result)
        {
            if (result.Success)
            {
                //purge subscriptions from cache because they just changed
                CacheHandler.Instance.Remove(CachingKey.UserSubscriptions(UserName));
            }
        }
    }
}
