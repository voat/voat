using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;

namespace Voat.Domain.Command
{
    public class RemoveModeratorCommand : CacheCommand<CommandResponse<RemoveModeratorResponse>>, IExcutableCommand<CommandResponse<RemoveModeratorResponse>>
    {
        private int _subversModeratorRecordID;
        private bool _allowSelfExecution;

        public RemoveModeratorCommand(int subverseModeratorRecordID, bool allowSelfExecution = false)
        {
            this._subversModeratorRecordID = subverseModeratorRecordID;
            this._allowSelfExecution = allowSelfExecution;
        }

        protected override async Task<CommandResponse<RemoveModeratorResponse>> CacheExecute()
        {
            using (var repo = new Repository())
            {
                var response = await repo.RemoveModerator(_subversModeratorRecordID, _allowSelfExecution);
                return response;
            }
        }

        protected override void UpdateCache(CommandResponse<RemoveModeratorResponse> result)
        {
            if (result.Success)
            {
                CacheHandler.Instance.Remove(CachingKey.SubverseModerators(result.Response.Subverse));
            }
        }
    }
}
