using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Command
{
    public class SendMessageCommand : Command<CommandResponse>
    {
        private SendMessage _message;

        public SendMessageCommand(SendMessage message)
        {
            this._message = message;
        }

        protected override async Task<CommandResponse> ProtectedExecute()
        {
            using (var repo = new Repository())
            {
                return await Task.Run(() => repo.SendMessage(_message));
            }
        }
    }
}
