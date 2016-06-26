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

        public BlockCommand(DomainType domainType, string name)
        {
            _domainType = domainType;
            _name = name;
        }

        protected override async Task<Tuple<CommandResponse<bool?>, bool?>> CacheExecute()
        {
            using (var db = new Repository())
            {
                //TODO: Convert to async repo method
                var response = await Task.Run(() => db.Block(_domainType, _name, true));
                return Tuple.Create(response, response.Response);
            }
        }
        protected override void UpdateCache(bool? result)
        {
            if (result.HasValue)
            {
                if (result.Value)
                {
                    //Added block
                    CacheHandler.Instance.Replace(CachingKey.UserBlocks(UserName), new Func<IList<BlockedItem>, IList<BlockedItem>>(x => {
                        x.Add(new BlockedItem() { Type = this._domainType, Name = this._name, CreationDate = Repository.CurrentDate });
                        return x;
                    }));
                }
                else
                {
                    //Removed block
                    CacheHandler.Instance.Replace(CachingKey.UserBlocks(UserName), new Func<IList<BlockedItem>, IList<BlockedItem>>(x => {
                        var entry = x.FirstOrDefault(b => b.Type == _domainType && b.Name == _name);
                        if (entry != null)
                        {
                            x.Remove(entry);
                        }
                        return x;
                    }));
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
                var response = await Task.Run(() => db.Block(_domainType, _name, false));
                return Tuple.Create(response, response.Response);
            }
        }
    }
}
