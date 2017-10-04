using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    public class PersistVoteCommand : CacheCommand<CommandResponse<Domain.Models.Vote>>
    {
        private Vote _model = null;
        public PersistVoteCommand(Vote model)
        {
            _model = model;
        }
        protected override async Task<CommandResponse<Vote>> CacheExecute()
        {
            using (var repo = new Repository(User))
            {
                var response = await repo.SaveVote(_model);
                return response;
            }
        }
        protected override async Task<CommandResponse<Vote>> ExecuteStage(CommandStage stage, CommandResponse<Vote> previous)
        {
            switch (stage)
            {
                case CommandStage.OnExecuting:
                    //perform validationn return non-success if not success
                    break;
            }
            return await base.ExecuteStage(stage, previous);
        }
        protected override void UpdateCache(CommandResponse<Vote> result)
        {
            if (result.Success)
            {
                CacheHandler.Instance.Remove(CachingKey.Vote(result.Response.ID));
            }
        }
    }
}
