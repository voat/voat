using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.ExceptionHandling;
using Voat.Utilities.Components;

namespace Voat.Utils
{
    public class VoatExceptionLogger : IExceptionLogger
    {
        public async Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            EventLogger.Log(context.Exception);
        }
    }
}