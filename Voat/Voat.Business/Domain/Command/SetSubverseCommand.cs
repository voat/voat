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
            _subverse = subverse;
            _action = action;
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
                //string key = CachingKey.UserSavedItems(_type, UserName);
                //if (result.Response.HasValue && CacheHandler.Instance.Exists(key))
                //{
                //    if (result.Response.Value)
                //    {
                //        CacheHandler.Instance.SetAdd(key, _id);
                //    }
                //    else
                //    {
                //        CacheHandler.Instance.SetRemove(key, _id);
                //    }
                //}
            }
        }
    }
}
