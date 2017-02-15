using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    public class SendMessageCommand : Command<CommandResponse<Message>>
    {
        private SendMessage _message;
        private bool _forceSend;
        private bool _ensureUserExists;
        private bool _isAnonymized;

        public SendMessageCommand(SendMessage message, bool forceSend = false, bool ensureUserExists = false, bool isAnonymized = false)
        {
            this._message = message;
            this._forceSend = forceSend;
            this._ensureUserExists = ensureUserExists;
            this._isAnonymized = isAnonymized;
        }

        protected override async Task<CommandResponse<Message>> ProtectedExecute()
        {
            using (var repo = new Repository())
            {
                return await repo.SendMessage(_message, _forceSend, _ensureUserExists, _isAnonymized).ConfigureAwait(false);
            }
        }
    }

    public class SendMessageReplyCommand : Command<CommandResponse<Message>>
    {
        private string _message;
        private int _messageID;

        public SendMessageReplyCommand(int messageID, string message)
        {
            this._message = message;
            this._messageID = messageID;
        }

        protected override async Task<CommandResponse<Message>> ProtectedExecute()
        {
            using (var repo = new Repository())
            {
                return await repo.SendMessageReply(_messageID, _message).ConfigureAwait(false);
            }
        }
    }
}
