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
            using (var repo = new Repository())
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
