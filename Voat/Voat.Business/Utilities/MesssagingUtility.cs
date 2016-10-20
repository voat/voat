/*
This source file is subject to version 3 of the GPL license,
that is bundled with this package in the file LICENSE, and is
available online at http://www.gnu.org/licenses/gpl.txt;
you may not use this file except in compliance with the License.

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat, Inc.
All Rights Reserved.
*/

using System;
using System.Linq;
using System.Threading.Tasks;
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
                return blocks.Any(x => x.Type == DomainType.User && x.Name.Equals(sender, StringComparison.OrdinalIgnoreCase));
            }
            return false;
        }
    }
}
