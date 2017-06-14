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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;

namespace Voat.Domain.Command
{
    
    public class LogVisitCommand : QueuedCommand<CommandResponse>, IExcutableCommand<CommandResponse>
    {
        protected string _subverse;
        protected int? _submissionID;
        protected string _clientIPAddress;

        public LogVisitCommand(string subverse, int? submissionID, string clientIPAddress)
        {
            _subverse = subverse;
            _submissionID = submissionID;
            _clientIPAddress = clientIPAddress;
        }
        public string Subverse { get => _subverse; }
        public int? SubmissionID { get => _submissionID; }
        public string ClientIPAddress { get => _clientIPAddress; }

        protected override async Task<CommandResponse> CacheExecute()
        {
            using (var repo = new Repository(User))
            {
                //TODO: Convert to async repo method
                var response = await repo.LogVisit(_subverse, _submissionID, _clientIPAddress);
                return response;
            }
        }

        protected override void UpdateCache(CommandResponse result)
        {
            if (result.Success)
            {
               
            }
        }
    }
}
