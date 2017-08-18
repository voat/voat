#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;

namespace Voat.Domain.Command
{
    /// <summary>
    /// This command base will queue commands to a certain threshold then execute them in a batch
    /// </summary>
    /// <typeparam name="T"></typeparam>
    //[JsonObject(MemberSerialization=MemberSerialization.Fields)]
    public abstract class QueuedCommand<T> : CacheCommand<T>, IExcutableCommand<T> where T : CommandResponse, new()
    {
        private static BatchOperation<QueuedCommand<T>> _commands = null;
        static QueuedCommand()
        {
            if (Voat.Caching.CacheHandler.Instance.CacheEnabled)
            {
                _commands = new CacheBatchOperation<QueuedCommand<T>>("Command", Voat.Caching.CacheHandler.Instance, 10, TimeSpan.FromMinutes(5), ProcessBatch);
            }
            else
            {
                _commands = new MemoryBatchOperation<QueuedCommand<T>>(10, TimeSpan.FromMinutes(5), ProcessBatch);
            }
        }
        protected static async Task ProcessBatch(IEnumerable<QueuedCommand<T>> batch)
        {
            foreach (var cmd in batch)
            {
                await cmd.CacheExecute();
            }
        }
        public override Task<T> Execute()
        {
            _commands.Add(this);
            return Task.FromResult((T)CommandResponse.FromStatus(Status.Queued));
        }
       

        protected override void UpdateCache(T result)
        {
            throw new NotImplementedException();
        }
    }
}
