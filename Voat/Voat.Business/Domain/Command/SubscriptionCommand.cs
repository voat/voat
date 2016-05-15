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
    public class SubscriptionCommand : CacheCommand<CommandResponse>
    {
        protected DomainType _domainType;
        protected SubscriptionAction _action;
        protected string _name;

        public SubscriptionCommand(DomainType domainType, SubscriptionAction action, string subscriptionItemName)
        {
            _domainType = domainType;
            _action = action;
            _name = subscriptionItemName;
        }

        protected override async Task<CommandResponse> CacheExecute()
        {
            using (var repo = new Repository())
            {
                await Task.Run(() => repo.SubscribeUser(_domainType, _action, _name));
            }
            return CommandResponse.Success();
        }

        protected override void UpdateCache()
        {
            //purge subscriptions from cache because they just changed
            CacheHandler.Instance.Remove(CachingKey.UserSubscriptions(UserName));
        }
    }
}
