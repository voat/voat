using System;
using System.Collections.Generic;
using System.Text;
using Voat.Common;

namespace Voat.Logging
{
    public abstract class QueuedLogger : BaseLogger
    {
        private int _threshold = 5;
        private MemoryBatchOperation<ILogInformation> _batchProcessor = null;

        public QueuedLogger() : this(5, LogType.All)
        {

        }
        public QueuedLogger(int threshold, LogType logLevel) : base(logLevel)
        {
            _batchProcessor = new MemoryBatchOperation<ILogInformation>(threshold, TimeSpan.FromSeconds(30), ProcessBatch);

        }
        protected abstract void ProcessBatch(IEnumerable<ILogInformation> batch);

        protected override void ProtectedLog(ILogInformation info)
        {
            _batchProcessor.Add(info);
        }
    }
}
