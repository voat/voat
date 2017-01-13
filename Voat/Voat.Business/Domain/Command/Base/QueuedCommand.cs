using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Command.Base
{
    /// <summary>
    /// This command base will queue commands to a certain threshold then execute them in a batch
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class QueuedCommand<T> : CacheCommand<T>, IExcutableCommand<T> where T : CommandResponse, new()
    {

        protected override Task<T> CacheExecute()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateCache(T result)
        {
            throw new NotImplementedException();
        }
    }
}
