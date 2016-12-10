using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

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
            using (var db = new Repository())
            {
                //TODO: Convert to async repo method
                var response = await Task.Run(() => db.Block(_domainType, _name, (_toggleSetting ? (bool?)null : true))).ConfigureAwait(false);
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
                            var entry = x.FirstOrDefault(b => b.Type == _domainType && b.Name.Equals(_name, StringComparison.OrdinalIgnoreCase));
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
                            var entry = x.FirstOrDefault(b => b.Type == _domainType && b.Name.Equals(_name, StringComparison.OrdinalIgnoreCase));
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
            using (var db = new Repository())
            {
                //TODO: Convert to async repo method
                var response = await Task.Run(() => db.Block(_domainType, _name, false)).ConfigureAwait(false);
                return Tuple.Create(response, response.Response);
            }
        }
    }
}
