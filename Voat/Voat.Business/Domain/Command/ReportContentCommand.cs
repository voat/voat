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
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    public class ReportContentCommand : Command<CommandResponse>
    {
        private int _id;
        private Models.ContentType _contentType;
        private int _reasonID;

        public ReportContentCommand(ContentType contentType, int id, int reasonID)
        {
            this._id = id;
            this._contentType = contentType;
            this._reasonID = reasonID;
        }

        protected override async Task<CommandResponse> ProtectedExecute()
        {
            using (var repo = new Repository(User))
            {
                var result = await repo.SaveRuleReport(_contentType, _id, _reasonID);
                return result;
            }
        }
    }
}
