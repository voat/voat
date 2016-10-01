using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Business.DataAccess.Commands;

namespace Voat.Business.DataAccess.Handlers
{

    public class SubmissionCommandHandler : CommandHandler,
        ICommandHandler<CreateLinkSubmissionCommand>,
        ICommandHandler<CreateDiscussionSubmissionCommand>,
        ICommandHandler<DeleteSubmissionCommand>
    {
        public CommandResponse Execute(DeleteSubmissionCommand commandType)
        {
            return CommandResponse.Success();
            
        }

        public CommandResponse Execute(CreateLinkSubmissionCommand commandType)
        {
            return CommandResponse.Success();
        }

        public CommandResponse Execute(CreateDiscussionSubmissionCommand commandType)
        {
            return CommandResponse.Success();
        }
    }
}
