using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Voat.Common
{
    public class MemoryBatchOperation<T> : BatchOperation<T>
    {
        private List<T> _batch = new List<T>();
        private ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public MemoryBatchOperation(int flushCount, TimeSpan flushSpan, Func<IEnumerable<T>, Task> batchAction) : base(flushCount, flushSpan, batchAction)
        {

        }
        public override int Count => _batch.Count;

        protected override void ProtectedAdd(T item)
        {
            _readerWriterLockSlim.EnterReadLock();
            try
            {
                _batch.Add(item);
            }
            finally
            {
                _readerWriterLockSlim.ExitReadLock();
            }
        }
        protected override IEnumerable<T> BatchPending()
        {
            List<T> batchToProcess = null;
            _readerWriterLockSlim.EnterWriteLock();
            try
            {
                //copy current batch
                batchToProcess = _batch;
                //create new batch
                _batch = new List<T>();
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
            return batchToProcess;
        }
    }
}
