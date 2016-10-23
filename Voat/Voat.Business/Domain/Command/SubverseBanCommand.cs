using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voat.Data;

namespace Voat.Domain.Command
{
    public class SubverseBanCommand : CacheCommand<CommandResponse<bool?>, bool?>, IExcutableCommand<CommandResponse<bool?>>
    {
        private string _userToBan;
        private string _subverse;
        private string _reason;
        private bool? _force;

        public SubverseBanCommand(string userToBan, string subverse, string reason, bool? force = null)
        {
            _userToBan = userToBan;
            _subverse = subverse;
            _reason = reason;
            _force = force;
        }

        protected override async Task<Tuple<CommandResponse<bool?>, bool?>> CacheExecute()
        {
            using (var repo = new Repository())
            {
                var result = await repo.BanUserFromSubverse(_userToBan, _subverse, _reason, _force);
                return Tuple.Create(result, result.Response);
            }
        }

        protected override void UpdateCache(bool? result)
        {
            if (result.HasValue)
            {
                if (result.Value)
                {
                    //user has been banned
                }
                else
                {
                    //user has been unbanned
                }
            }
        }
    }
}
