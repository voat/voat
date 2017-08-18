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
using Voat.Caching;
using Voat.Data;

namespace Voat.Domain.Command
{
    public class CreateSubverseCommand : CacheCommand<CommandResponse>
    {
        private string _name;
        private string _title;
        private string _sidebar;
        private string _description;

        public CreateSubverseCommand(string name, string title, string description, string sidebar = null)
        {
            this._name = name;
            this._title = title;
            this._sidebar = sidebar;
            this._description = description;
        }

        //protected override async Task<CommandResponse> ProtectedExecute()
        //{
           
        //}

        protected override async Task<CommandResponse> CacheExecute()
        {
            using (var repo = new Repository(User))
            {
                return await repo.CreateSubverse(_name, _title, _description, _sidebar);
            }
        }

        protected override void UpdateCache(CommandResponse result)
        {
            if (result.Success)
            {
                CacheHandler.Instance.Remove(CachingKey.UserSubscriptions(UserName));
            }
        }
    }
}
