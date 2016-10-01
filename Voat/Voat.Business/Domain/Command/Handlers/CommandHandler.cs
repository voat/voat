using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Voat.Common;
using Voat.Rules;

namespace Voat.Business.DataAccess.Handlers
{
    public abstract class CommandHandler : RequestHttpContextHandler, ICommandHandler
    {
        public CommandHandler() { }
        public CommandHandler(VoatRequestContext context) 
        {
            //_context = context;
        }

    }
}
