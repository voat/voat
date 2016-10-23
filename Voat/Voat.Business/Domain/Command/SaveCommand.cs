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
            using (var repo = new Repository())
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
