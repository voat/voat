using System;
using System.Threading.Tasks;

namespace Voat.Domain.Command
{
    public class DeleteUserCommand : Command<CommandResponse>
    {
        protected override Task<CommandResponse> ProtectedExecute()
        {
            throw new NotImplementedException();
        }
    }
}
