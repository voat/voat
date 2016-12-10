using System;
using System.Diagnostics;

namespace Voat.Logging
{

    public class NullLogger : BaseLogger
    {
        protected override void ProtectedLog(ILogInformation info)
        {
        }
    }
}
