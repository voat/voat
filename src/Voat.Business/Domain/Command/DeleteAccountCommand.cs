#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

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
            using (var db = new Repository(User))
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
