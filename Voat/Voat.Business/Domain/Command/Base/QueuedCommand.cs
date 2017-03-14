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
        private static List<QueuedCommand<T>> _commands = new List<QueuedCommand<T>>();

        private static DateTime _lastActionDate = DateTime.UtcNow;
        private static int _flushCount = 1;
        private static TimeSpan _flushSpan = TimeSpan.Zero;

        protected static bool IsFlushable
        {
            get
            {
                return _commands.Count >= _flushCount || (_flushSpan <= DateTime.UtcNow.Subtract(_lastActionDate) && _flushSpan != TimeSpan.Zero);
            }
        }

        public static int FlushCount
        {
            get
            {
                return _flushCount;
            }

            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                _flushCount = value;
            }
        }

        public static TimeSpan FlushSpan
        {
            get
            {
                return _flushSpan;
            }

            set
            {
                _flushSpan = value;
            }
        }

        public void Append()
        {
            _commands.Add(this);

            if (IsFlushable)
            {
                Flush();
            }
            _lastActionDate = DateTime.UtcNow;
        }

        protected void Flush()
        {
            var commands = _commands;
            _commands = new List<QueuedCommand<T>>();

            foreach (var command in commands)
            {
                command.CacheExecute();
            }
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
