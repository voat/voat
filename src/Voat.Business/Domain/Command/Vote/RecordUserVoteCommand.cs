using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;

namespace Voat.Domain.Command
{
    public class RecordUserVoteCommand : CacheCommand<CommandResponse>
    {
        public int _voteID;
        public int _optionID;

        public RecordUserVoteCommand(int voteID, int optionID)
        {
            _voteID = voteID;
            _optionID = optionID;
        }
        protected override async Task<CommandResponse> CacheExecute()
        {
            using (var repo = new Repository(User))
            {
                var result = await repo.RecordUserVote(_voteID, _optionID);
                return result;
            }
        }

        protected override void UpdateCache(CommandResponse result)
        {
            throw new NotImplementedException();
        }
    }
}
