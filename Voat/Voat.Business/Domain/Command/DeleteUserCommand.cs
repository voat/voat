using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Command
{
    public class DeleteUserCommand : Command<CommandResponse>
    {
        public override Task<CommandResponse> Execute()
        {
            throw new NotImplementedException();
        }
    }
}
