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
    public class CreateSubmissionCommand : Command<CommandResponse<Domain.Models.Submission>>
    {
        private UserSubmission _userSubmission;

        public CreateSubmissionCommand(UserSubmission submission)
        {
            _userSubmission = submission;
            base.CommandStageMask = CommandStage.OnValidation;
        }
        protected override async Task<CommandResponse<Submission>> ExecuteStage(CommandStage stage, CommandResponse<Submission> previous)
        {
            switch (stage)
            {
                case CommandStage.OnValidation:
                    var results = ValidationHandler.Validate(_userSubmission);
                    if (!results.IsNullOrEmpty())
                    {
                        return CommandResponse.Invalid<Submission>(results);
                    }
                    break;
            }
            return await base.ExecuteStage(stage, previous);
        }
        public UserSubmission UserSubmission { get => _userSubmission; set => _userSubmission = value; }

        protected override async Task<CommandResponse<Domain.Models.Submission>> ProtectedExecute()
        {
            using (var db = new Repository(User))
            {
                var result = await db.PostSubmission(_userSubmission);
                return CommandResponse.Map(result, result.Response);
            }
        }
    }
}
