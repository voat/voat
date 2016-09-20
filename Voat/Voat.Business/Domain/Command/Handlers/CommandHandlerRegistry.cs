using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Business.DataAccess.Commands;
using Voat.Common;

namespace Voat.Business.DataAccess
{
   

    public interface ICommandHandler {
        //RequestContext Context { get; }
    }

    //For commands that return state - will be refactored/removed later. Primarily this needs to be auto-gen PK's and such.
    public interface ICommandHandler<T, R> : ICommandHandler
     where T : ICommand where R : CommandResponse
    {
        R Execute(T command);
    }

    public interface ICommandHandler<T> : ICommandHandler<T, CommandResponse>
        where T : ICommand 
    {
       
    }



    public class CommandHandlerRegistry : ICommandHandler<CreateLinkSubmissionCommand, CommandResponse>
    {
        //private static Dictionary<Type, ICommandHandler> _handlers = new Dictionary<Type, ICommandHandler>();

        //public static void Register(ICommandHandler handler) {
        //    //TODO: Evaulate interfaces and register in buckets
        //}

        //public static Task<CommandResponse> ExecuteCommand(ICommand cmd)
        //{
        //    //TODO: Find handler in bucket and execute
        //    return Task.FromResult(CommandResponse.Success());
        //}
        //public static Task<CommandResponse<R>> ExecuteCommand<R>(ICommand cmd)
        //{
        //    //TODO: Find handler in bucket and execute
        //    return Task.FromResult(CommandResponse<R>.Success(default(R)));
        //}
        public CommandResponse Execute(CreateLinkSubmissionCommand command)
        {
            throw new NotImplementedException();
        }
    }


}
