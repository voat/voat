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
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Utilities;
using Voat.Validation;

namespace Voat.Domain.Command
{
    public class EditSubmissionCommand : CacheCommand<CommandResponse<Domain.Models.Submission>, Data.Models.Submission>
    {
        private UserSubmissionContent _userSubmission;
        private int _submissionID;

        public EditSubmissionCommand(int submissionID, UserSubmissionContent submission)
        {
            _submissionID = submissionID;
            _userSubmission = submission;
        }
        protected override async Task<CommandResponse<Submission>> ExecuteStage(CommandStage stage, CommandResponse<Submission> previous)
        {
            switch (stage)
            {
                case CommandStage.OnValidation:
                    if (_userSubmission.Content.Length > 10000)
                    {
                        return CommandResponse.FromStatus<Submission>(null, Status.Invalid, "Content can not exceed 10,000 characters");
                    }
                    break;
            }
            return await base.ExecuteStage(stage, previous);
        }
        protected override async Task<Tuple<CommandResponse<Domain.Models.Submission>, Data.Models.Submission>> CacheExecute()
        {
            using (var db = new Repository(User))
            {
                var result = await db.EditSubmission(_submissionID, _userSubmission);
                if (result.Success)
                {
                    return Tuple.Create(CommandResponse.FromStatus(result.Response.Map(), Status.Success, ""), result.Response);
                }
                else
                {
                    return Tuple.Create(CommandResponse.FromStatus((Submission)null, result.Status, result.Message), result.Response);
                }
            }
        }

        protected override void UpdateCache(Data.Models.Submission result)
        {
            var key = CachingKey.Submission(result.ID);
            if (CacheHandler.Instance.Exists(key))
            {
                CacheHandler.Instance.Replace<Submission>(key, x =>
                {
                    x.Title = result.Title;
                    x.Content = result.Content;
                    x.Url = result.Url;
                    x.LastEditDate = result.LastEditDate;
                    return x;
                });
            }

            //Legacy item removal
            //CacheHandler.Instance.Remove(DataCache.Keys.Submission(result.ID));
        }
    }
}
