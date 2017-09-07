using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;

namespace Voat.Domain.Command
{
    public class RemoveModeratorCommand : Domain.Command.Command<CommandResponse>
    {
        public SubverseModerator _model;

        public RemoveModeratorCommand(SubverseModerator model)
        {
            _model = model;
        }

        protected override Task<CommandResponse> ProtectedExecute()
        {
            throw new NotImplementedException();
        }
    }
}
