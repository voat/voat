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
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Utilities;

namespace Voat.Domain.Command
{
    public class DeleteSubmissionCommand : CacheCommand<CommandResponse<Data.Models.Submission>>
    {
        private int _submissionID = 0;
        private string _reason = null;

        public DeleteSubmissionCommand(int submissionID, string reason = null)
        {
            _submissionID = submissionID;
            _reason = reason;
        }

        protected override async Task<CommandResponse<Data.Models.Submission>> CacheExecute()
        {
            using (var db = new Repository(User))
            {
                var result = await db.DeleteSubmission(_submissionID, _reason);
                return result;
            }
           
        }

        protected override void UpdateCache(CommandResponse<Data.Models.Submission> result)
        {
            if (result.Success)
            {
                CacheHandler.Instance.Remove(CachingKey.Submission(result.Response.ID));
            }

            //Legacy item removal
            //CacheHandler.Instance.Remove(DataCache.Keys.Submission(result.ID));
        }
    }
}
