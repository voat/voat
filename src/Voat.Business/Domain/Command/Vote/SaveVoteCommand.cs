using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    public class SaveVoteCommand : CacheCommand<CommandResponse<Domain.Models.Vote>>
    {
        private Vote _model = null;
        public SaveVoteCommand(Vote model)
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
        protected override Task<CommandResponse<Vote>> ExecuteStage(CommandStage stage)
        {
            switch (stage)
            {
                case CommandStage.OnExecuting:
                    //perform validationn return non-success if not success
                    break;
            }
            return base.ExecuteStage(stage);
        }
        protected override void UpdateCache(CommandResponse<Vote> result)
        {
            if (result.Success)
            {
                //Clear Cache
            }
        }
    }
}
