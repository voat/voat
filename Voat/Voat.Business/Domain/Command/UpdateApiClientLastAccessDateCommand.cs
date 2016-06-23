using System.Threading.Tasks;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Command
{
    public class UpdateApiClientLastAccessDateCommand : Command, IExcutableCommand<CommandResponse>
    {
        private ApiClient _client;

        public UpdateApiClientLastAccessDateCommand(ApiClient client)
        {
            _client = client;
        }

        public async Task<CommandResponse> Execute()
        {
            if (_client != null)
            {
                using (var repo = new Repository())
                {
                    await Task.Run(() => repo.UpdateApiClientLastAccessDate(_client.ID));
                }
            }
            return CommandResponse.Successful();
        }
    }
}
