using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Command
{
    public class RecordUserVoteCommand : CacheCommand<CommandResponse>
    {
        protected override Task<CommandResponse> CacheExecute()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateCache(CommandResponse result)
        {
            throw new NotImplementedException();
        }
    }
}
