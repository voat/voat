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
using System.Security.Principal;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Data.Models;
using Voat.Domain.Models;
using Voat.Domain.Query;

namespace Voat.Utilities
{
    public static class MesssagingUtility
    {
        public static bool IsSenderBlocked(string sender, string recipient)
        {
            var q = new QueryUserBlocks(recipient);
            var blocks = q.Execute();

            if (blocks != null)
            {
                return blocks.Any(x => x.Type == DomainType.User && x.Name.ToLower() == sender.ToLower());
            }
            return false;
        }
    }
}
