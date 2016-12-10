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
            using (var repo = new Repository())
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
            using (var repo = new Repository())
            {
                return await repo.DeleteMessages(_ownerName, _ownerType, _type, _id);
            }
        }
    }
}
