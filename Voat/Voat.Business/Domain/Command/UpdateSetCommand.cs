using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;

namespace Voat.Domain.Command
{
    public class UpdateSetCommand : CacheCommand<CommandResponse<Domain.Models.Set>>, IExcutableCommand<CommandResponse<Domain.Models.Set>>
    {
        private Domain.Models.Set _set = null;

        public UpdateSetCommand(Domain.Models.Set set)
        {
            _set = set;
        }

        protected override async Task<CommandResponse<Domain.Models.Set>> CacheExecute()
        {
            using (var repo = new Repository())
            {
                var response = await repo.CreateOrUpdateSet(_set);

                return response;

            }
        }

        protected override void UpdateCache(CommandResponse<Domain.Models.Set> result)
        {
            if (result.Success)
            {
                CacheHandler.Instance.Remove(CachingKey.UserSubscriptions(UserName));
                CacheHandler.Instance.Remove(CachingKey.Set(this._set.Name, this._set.UserName));
            }
        }
    }
}



