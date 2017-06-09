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

using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Utilities;

namespace Voat.Domain.Command
{
    public class UpdateUserPreferencesCommand : CacheCommand<CommandResponse>, IExcutableCommand<CommandResponse>
    {
        private Domain.Models.UserPreferenceUpdate _preferences = null;

        public UpdateUserPreferencesCommand(Domain.Models.UserPreferenceUpdate preferences)
        {
            _preferences = preferences;
        }

        protected override async Task<CommandResponse> CacheExecute()
        {
            using (var repo = new Repository(User))
            {
                repo.SaveUserPrefernces(_preferences);
            }
            return CommandResponse.Successful();
        }

        protected override void UpdateCache(CommandResponse result)
        {
            CacheHandler.Instance.Remove(CachingKey.UserPreferences(this.UserName));
            CacheHandler.Instance.Remove(CachingKey.UserInformation(this.UserName));
        }
    }
}
