using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Query;

namespace Voat.Domain.Command
{
    public class RecordUserVoteCommand : Command<CommandResponse<VoteTracker>>
    {
        private int _voteID;
        private int _optionID;
        private bool? _restrictionsPassed;

        public RecordUserVoteCommand(int voteID, int optionID)
        {
            _voteID = voteID;
            _optionID = optionID;
            CommandStageMask = CommandStage.OnExecuting | CommandStage.OnExecuted;
        }

        protected override async Task<CommandResponse<VoteTracker>> ExecuteStage(CommandStage stage, CommandResponse<VoteTracker> previous)
        {
            switch (stage)
            {
                case CommandStage.OnExecuting:

                    var q = new QueryVote(_voteID);
                    var vote = await q.ExecuteAsync();

                    var notPassed = vote.Restrictions.FirstOrDefault(x => {
                        var e = x.Evaluate(User);
                        return !e.Success;
                    });

                    _restrictionsPassed = notPassed == null;

                    break;

                case CommandStage.OnExecuted:

                    if (previous.Success)
                    {
                        CacheHandler.Instance.Remove(CachingKey.VoteStatistics(_voteID));
                    }

                    break;

            }
            return await base.ExecuteStage(stage, previous);
        }

        protected override async Task<CommandResponse<VoteTracker>> ProtectedExecute()
        {
            using (var repo = new Repository(User))
            {
                var result = await repo.RecordUserVote(_voteID, _optionID, _restrictionsPassed.Value);
                return result;
            }
        }
    }
}
