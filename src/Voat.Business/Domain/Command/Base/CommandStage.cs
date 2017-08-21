using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Domain.Command
{
    [Flags]
    public enum CommandStage
    {
        None = 0,
        OnAuthorization = 1,
        OnValidation = 2,
        OnQueuing = 4,
        OnDequeued = 8,
        OnExecuting = 16,
        OnExecuted = 32,
        All = OnAuthorization | OnValidation | OnQueuing | OnDequeued | OnExecuting | OnExecuted
    }
}
