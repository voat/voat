using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;

namespace Voat.Logging
{
    public abstract class QueuedLogger : BaseLogger, IDisposable
    {
        private BatchOperation<ILogInformation> _batchProcessor = null;

        public QueuedLogger() : this(1, TimeSpan.Zero, LogType.All)
        {

        }
        public QueuedLogger(int flushCount, TimeSpan flushSpan, LogType logLevel) : base(logLevel)
        {
            _batchProcessor = new MemoryBatchOperation<ILogInformation>(flushCount, flushSpan, ProcessBatch);

        }

        public void Dispose()
        {
            _batchProcessor.Dispose();
        }

        protected abstract Task ProcessBatch(IEnumerable<ILogInformation> batch);

        protected override void ProtectedLog(ILogInformation info)
        {
            _batchProcessor.Add(info);
        }
    }
}
