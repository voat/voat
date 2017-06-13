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

using Newtonsoft.Json;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    public class MarkMessagesCommand : Command<CommandResponse>
    {
        private MessageTypeFlag _type;
        private MessageState _action;
        private int? _id = null;
        protected string _ownerName;
        protected IdentityType _ownerType;

        [JsonConstructor()]
        public MarkMessagesCommand(string ownerName, IdentityType ownerType, MessageTypeFlag type, MessageState action, int? id = null)
        {
            this._ownerName = ownerName;
            this._ownerType = ownerType;
            this._type = type;
            this._action = action;
            this._id = id;
        }

        public MarkMessagesCommand(MessageTypeFlag type, MessageState action, int? id = null)
            : this(null, IdentityType.User, type, action, id)
        {
            this._ownerName = UserName;
            this._ownerType = IdentityType.User;
        }

        protected override async Task<CommandResponse> ProtectedExecute()
        {
            using (var repo = new Repository(User))
            {
                return await repo.MarkMessages(_ownerName, _ownerType, _type, _action, _id);
            }
        }
    }

    public class DeleteMessagesCommand : Command<CommandResponse>
    {
        private MessageTypeFlag _type;
        private int? _id = null;
        protected string _ownerName;
        protected IdentityType _ownerType;

        [JsonConstructor()]
        public DeleteMessagesCommand(string ownerName, IdentityType ownerType, MessageTypeFlag type, int? id = null)
        {
            this._ownerName = ownerName;
            this._ownerType = ownerType;
            this._type = type;
            this._id = id;
        }

        public DeleteMessagesCommand(MessageTypeFlag type, int? id = null)
            : this(null, IdentityType.User, type, id)
        {
            this._ownerName = UserName;
            this._ownerType = IdentityType.User;
        }

        protected override async Task<CommandResponse> ProtectedExecute()
        {
            using (var repo = new Repository(User))
            {
                return await repo.DeleteMessages(_ownerName, _ownerType, _type, _id);
            }
        }
    }
}
