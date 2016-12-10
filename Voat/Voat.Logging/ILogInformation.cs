using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Logging
{
    public interface ILogInformation
    {
        string UserName { get; set; }
        string Origin { get; set; }
        LogType Type { get; set; }
        string Category { get; set; }
        string Message { get; set; }
        //string Source { get; set; }
        Exception Exception { get; set; }
        object Data { get; set; }
        Nullable<Guid> ActivityID { get; set; }
    }
}
