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
using Voat.Common;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    
    public class SetSubverseCommand : CacheCommand<CommandResponse<bool?>>, IExcutableCommand<CommandResponse<bool?>>
    {

        protected DomainReference _setRef;
        protected string _subverse;
        protected SubscriptionAction _action;

        public SetSubverseCommand(DomainReference setRef, string subverse, SubscriptionAction action = SubscriptionAction.Toggle)
        {
            _setRef = setRef;
            _action = action;
            _subverse = subverse;
        }

        protected override async Task<CommandResponse<bool?>> CacheExecute()
        {
            using (var repo = new Repository(User))
            {
                //TODO: Convert to async repo method
                var response = await repo.SetSubverseListChange(_setRef, _subverse, _action);
                return response;
            }
        }

        protected override void UpdateCache(CommandResponse<bool?> result)
        {
            if (result.Success)
            {
                if (_setRef.Name.IsEqual(SetType.Front.ToString()))
                {
                    CacheHandler.Instance.Remove(CachingKey.UserSubscriptions(UserName));
                }
                else if (_setRef.Name.IsEqual(SetType.Blocked.ToString()))
                {
                    CacheHandler.Instance.Remove(CachingKey.UserBlocks(UserName));
                }
            }
        }
    }
}
