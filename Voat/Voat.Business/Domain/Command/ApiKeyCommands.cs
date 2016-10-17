using System;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Command
{
    public class CreateApiKeyCommand : Command<CommandResponse>
    {
        private string _description = null;
        private string _name = null;
        private string _url = null;
        private string _redirectUrl = null;

        public CreateApiKeyCommand(string name, string description, string url, string redirectUrl)
        {
            this._name = name;
            this._description = description;
            this._url = url;
            this._redirectUrl = redirectUrl;
        }

        protected override async Task<CommandResponse> ProtectedExecute()
        {
            if (String.IsNullOrEmpty(_name))
            {
                throw new ArgumentException("Api key name must be provided.");
            }

            using (var repo = new Repository())
            {
                await Task.Run(() => repo.CreateApiKey(_name, _description, _url, _redirectUrl));
            }
            return CommandResponse.Successful();
        }
    }

    public class DeleteApiKeyCommand : CacheCommand<CommandResponse, ApiClient>
    {
        private int id = 0;

        public DeleteApiKeyCommand(int apiKeyID)
        {
            this.id = apiKeyID;
        }

        protected override async Task<Tuple<CommandResponse, ApiClient>> CacheExecute()
        {
            return await Task.Run(() =>
            {
                using (var repo = new Repository())
                {
                    var apiClient = repo.DeleteApiKey(id);
                    return new Tuple<CommandResponse, ApiClient>(CommandResponse.Successful(), apiClient);
                }
            });
        }

        protected override void UpdateCache(ApiClient result)
        {
            if (result != null)
            {
                CacheHandler.Instance.Remove(CachingKey.ApiClient(result.PublicKey));
            }
        }
    }
}
