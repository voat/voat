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
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Utilities;

namespace Voat.Domain.Command
{
    public class BlockCommand : CacheCommand<CommandResponse<bool?>, bool?>, IExcutableCommand<CommandResponse<bool?>>
    {
        protected DomainType _domainType = DomainType.Subverse;
        protected string _name = null;
        protected bool _toggleSetting = false; //if true then this command functions as a toggle command

        public BlockCommand(DomainType domainType, string name, bool toggleSetting = false)
        {
            _domainType = domainType;
            _name = name;
            _toggleSetting = toggleSetting;
        }

        protected override async Task<Tuple<CommandResponse<bool?>, bool?>> CacheExecute()
        {
            DemandAuthentication();

            using (var db = new Repository(User))
            {
                var response = await db.Block(_domainType, _name, (_toggleSetting ? SubscriptionAction.Toggle : SubscriptionAction.Subscribe)).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                return Tuple.Create(response, response.Response);
            }
        }

        protected override void UpdateCache(bool? result)
        {
            if (result.HasValue)
            {
                string key = CachingKey.UserBlocks(UserName);
                if (result.HasValue && CacheHandler.Instance.Exists(key))
                {
                    if (result.Value)
                    {
                        //Added block
                        CacheHandler.Instance.Replace<IList<BlockedItem>>(key, new Func<IList<BlockedItem>, IList<BlockedItem>>(x =>
                        {
                            var entry = x.FirstOrDefault(b => b.Type == _domainType && b.Name.ToLower() == _name.ToLower());
                            if (entry == null)
                            {
                                x.Add(new BlockedItem() { Type = this._domainType, Name = this._name, CreationDate = Repository.CurrentDate });
                            }
                            return x;
                        }), TimeSpan.FromMinutes(10));
                    }
                    else
                    {
                        //Removed block
                        CacheHandler.Instance.Replace<IList<BlockedItem>>(key, new Func<IList<BlockedItem>, IList<BlockedItem>>(x =>
                        {
                            var entry = x.FirstOrDefault(b => b.Type == _domainType && b.Name.ToLower() == _name.ToLower());
                            if (entry != null)
                            {
                                x.Remove(entry);
                            }
                            return x;
                        }), TimeSpan.FromMinutes(10));
                    }
                }
            }
        }
    }

    public class UnblockCommand : BlockCommand
    {
        public UnblockCommand(DomainType domainType, string name) : base(domainType, name)
        {
        }

        protected override async Task<Tuple<CommandResponse<bool?>, bool?>> CacheExecute()
        {
            DemandAuthentication();

            using (var db = new Repository(User))
            {
                //TODO: Convert to async repo method
                var response = await db.Block(_domainType, _name, SubscriptionAction.Unsubscribe).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                return Tuple.Create(response, response.Response);
            }
        }
    }
}
