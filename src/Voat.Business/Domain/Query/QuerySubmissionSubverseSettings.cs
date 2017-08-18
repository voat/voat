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

namespace Voat.Domain.Query
{
    
    public class QuerySubmissionSubverseSettings : Query<IEnumerable<Models.SubverseSubmissionSetting>>
    {
        private string _subverseNameLike;
        private bool _exactMatch;

        public QuerySubmissionSubverseSettings(string subverseName, bool exactMatch)
        {
            this._subverseNameLike = subverseName;
            this._exactMatch = exactMatch;
        }

        public override async Task<IEnumerable<Models.SubverseSubmissionSetting>> ExecuteAsync()
        {
            using (var repo = new Data.Repository(User))
            {
                return await repo.SubverseSubmissionSettingsSearch(_subverseNameLike, _exactMatch);
            }
        }
    }
}
