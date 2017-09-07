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

namespace Voat.Domain.Command
{
    public class RemoveModeratorByRecordIDCommand : CacheCommand<CommandResponse<RemoveModeratorResponse>>, IExcutableCommand<CommandResponse<RemoveModeratorResponse>>
    {
        private int _subversModeratorRecordID;
        private bool _allowSelfExecution;

        public RemoveModeratorByRecordIDCommand(int subverseModeratorRecordID, bool allowSelfExecution = false)
        {
            this._subversModeratorRecordID = subverseModeratorRecordID;
            this._allowSelfExecution = allowSelfExecution;
        }

        protected override async Task<CommandResponse<RemoveModeratorResponse>> CacheExecute()
        {
            using (var repo = new Repository(User))
            {
                var response = await repo.RemoveModerator(_subversModeratorRecordID, _allowSelfExecution);
                return response;
            }
        }

        protected override void UpdateCache(CommandResponse<RemoveModeratorResponse> result)
        {
            if (result.Success)
            {
                CacheHandler.Instance.Remove(CachingKey.SubverseModerators(result.Response.Subverse));
            }
        }
    }
}
