using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Domain.Command
{
    public enum CommandStage
    {
        BeforeExecute,
        AfterExecute
    }
}
