#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

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

            using (var repo = new Repository(User))
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
                using (var repo = new Repository(User))
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
    public class EditApiKeyCommand : CacheCommand<CommandResponse, ApiClient>
    {
        private string _apiKey = null;
        private string _description = null;
        private string _name = null;
        private string _url = null;
        private string _redirectUrl = null;

        public EditApiKeyCommand(string apiKeyID, string name, string description, string url, string redirectUrl)
        {
            this._apiKey = apiKeyID;
            this._name = name;
            this._description = description;
            this._url = url;
            this._redirectUrl = redirectUrl;
        }

        protected override async Task<Tuple<CommandResponse, ApiClient>> CacheExecute()
        {
            using (var repo = new Repository(User))
            {
                var apiClient = await repo.EditApiKey(_apiKey, _name, _description, _url, _redirectUrl);
                return new Tuple<CommandResponse, ApiClient>(CommandResponse.Successful(), apiClient);
            }
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
