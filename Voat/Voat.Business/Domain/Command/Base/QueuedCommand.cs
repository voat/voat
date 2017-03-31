using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;

namespace Voat.Domain.Command.Base
{





    /// <summary>
    /// This command base will queue commands to a certain threshold then execute them in a batch
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class QueuedCommand<T> : CacheCommand<T>, IExcutableCommand<T> where T : CommandResponse, new()
    {
        private static List<QueuedCommand<T>> _commands = new List<QueuedCommand<T>>();


        protected abstract FlushDetector Flusher { get; }   

        public void Append()
        {
            _commands.Add(this);
            Flusher.Increment();

            if (Flusher.IsFlushable)
            {
                Flush();
            }
            
        }

        protected virtual void Flush()
        {
            var commands = _commands;
            _commands = new List<QueuedCommand<T>>();

            foreach (var command in commands)
            {
                command.CacheExecute();
            }
            Flusher.Reset();
        }


        protected override Task<T> CacheExecute()
        {
            return this.Execute();
        }

        protected override void UpdateCache(T result)
        {
            throw new NotImplementedException();
        }
    }
}
