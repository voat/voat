using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Command
{
    public class UpdateUserPreferencesCommand : CacheCommand<CommandResponse>, IExcutableCommand<CommandResponse>
    {
        private Domain.Models.UserPreference _preferences = null;

        public UpdateUserPreferencesCommand(Domain.Models.UserPreference preferences)
        {
            _preferences = preferences;
        }

        protected override async Task<CommandResponse> CacheExecute()
        {
            await Task.Run(() =>
            {
                using (var repo = new Repository())
                {
                    repo.SaveUserPrefernces(_preferences);
                }
            });
            return CommandResponse.Successful();
        }

        protected override void UpdateCache(CommandResponse result)
        {
            CacheHandler.Instance.Remove(CachingKey.UserPreferences(this.UserName));
        }
    }
}
