using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    
    public class SetSubverseCommand : CacheCommand<CommandResponse<bool?>>, IExcutableCommand<CommandResponse<bool?>>
    {

        protected DomainReference _setRef;
        protected string _subverse;
        protected SubscriptionAction? _action;

        public SetSubverseCommand(DomainReference setRef, string subverse, SubscriptionAction? action = null)
        {
            _setRef = setRef;
            _action = action;
            _subverse = subverse;
        }

        protected override async Task<CommandResponse<bool?>> CacheExecute()
        {
            using (var repo = new Repository())
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
