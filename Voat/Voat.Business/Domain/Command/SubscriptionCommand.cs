using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    public class SubscriptionCommand : CacheCommand<CommandResponse>
    {
        protected SubscriptionAction _action;
        protected DomainReference _domainReference;

        public SubscriptionCommand(DomainReference domainReference, SubscriptionAction action)
        {
            _action = action;
            _domainReference = domainReference;
        }

        protected override async Task<CommandResponse> CacheExecute()
        {
            using (var repo = new Repository())
            {
                await repo.SubscribeUser(_domainReference, _action);
            }
            return CommandResponse.Successful();
        }

        protected override void UpdateCache(CommandResponse result)
        {
            //purge subscriptions from cache because they just changed
            CacheHandler.Instance.Remove(CachingKey.UserSubscriptions(UserName));
        }
    }
}
