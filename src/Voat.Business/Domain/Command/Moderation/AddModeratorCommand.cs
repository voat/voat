using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;

namespace Voat.Domain.Command
{
    public class AddModeratorCommand : Domain.Command.Command<CommandResponse>
    {
        public SubverseModerator _model;
        public AddModeratorCommand(SubverseModerator model)
        {
            _model = model;
        }

        protected override Task<CommandResponse> ProtectedExecute()
        {
            throw new NotImplementedException();
        }
    }
}
