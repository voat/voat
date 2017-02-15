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

    [Serializable]
    public class DeleteAccountCommand : CacheCommand<CommandResponse>
    {
        private DeleteAccountOptions _options;
        public DeleteAccountCommand(DeleteAccountOptions options)
        {
            _options = options;
        }


        protected override async Task<CommandResponse> CacheExecute()
        {
            using (var db = new Repository())
            {
                var data = await db.DeleteAccount(_options);
                return data;
            }
        }

        protected override void UpdateCache(CommandResponse result)
        {
            if (result.Success)
            {
                //Cleare user cache
                CacheHandler.Instance.Remove(CachingKey.UserBlocks(_options.UserName));
                CacheHandler.Instance.Remove(CachingKey.UserInformation(_options.UserName));
                CacheHandler.Instance.Remove(CachingKey.UserOverview(_options.UserName));
                CacheHandler.Instance.Remove(CachingKey.UserPreferences(_options.UserName));
                CacheHandler.Instance.Remove(CachingKey.UserSubscriptions(_options.UserName));
                CacheHandler.Instance.Remove(CachingKey.UserData(_options.UserName));
                CacheHandler.Instance.Remove(CachingKey.UserBlocks(_options.UserName));
                CacheHandler.Instance.Remove(CachingKey.UserRecord(_options.UserName));
                //CacheHandler.Instance.Remove(CachingKey.UserSavedItems(_options.UserName));
            }
        }
    }
}
