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
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    public class SaveCommand : CacheCommand<CommandResponse<bool?>>, IExcutableCommand<CommandResponse<bool?>>
    {
        protected ContentType _type = ContentType.Submission;
        protected int _id;
        protected bool? _toggleSetting = false; //if true then this command functions as a toggle command

        public SaveCommand(ContentType type, int id, bool? toggleSetting = null)
        {
            _type = type;
            _id = id;
            _toggleSetting = toggleSetting;
        }

        protected override async Task<CommandResponse<bool?>> CacheExecute()
        {
            DemandAuthentication();

            using (var repo = new Repository(User))
            {
                //TODO: Convert to async repo method
                var response = await repo.Save(_type, _id, _toggleSetting);
                return response;
            }
        }

        protected override void UpdateCache(CommandResponse<bool?> result)
        {
            if (result.Success)
            {
                string key = CachingKey.UserSavedItems(_type, UserName);
                if (result.Response.HasValue && CacheHandler.Instance.Exists(key))
                {
                    if (result.Response.Value)
                    {
                        CacheHandler.Instance.SetAdd(key, _id);
                    }
                    else
                    {
                        CacheHandler.Instance.SetRemove(key, _id);
                    }
                }
            }
        }
    }
}
