using System;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    public class MarkMessagesCommand : Command<CommandResponse>
    {
        private MessageTypeFlag _type;
        private MessageState _action;
        private int? _id = null;
        protected string _ownerName;
        protected MessageIdentityType _ownerType;

        public MarkMessagesCommand(string ownerName, MessageIdentityType ownerType, MessageTypeFlag type, MessageState action, int? id = null)
        {
            this._ownerName = ownerName;
            this._ownerType = ownerType;
            this._type = type;
            this._action = action;
            this._id = id;
        }


        public MarkMessagesCommand(MessageTypeFlag type, MessageState action, int? id = null)
            : this(null, MessageIdentityType.User, type, action, id)
        {
            this._ownerName = UserName;
            this._ownerType = MessageIdentityType.User;                        
        }

        protected override async Task<CommandResponse> ProtectedExecute()
        {
            using (var repo = new Repository())
            {
                return await Task.Run(() => repo.MarkMessages(_ownerName, _ownerType, _type, _action, _id));
            }
        }
    }
}
