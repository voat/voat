using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Voat.Common;
using System.Threading;

namespace Voat.Common
{
    
    public class MemoryBatchOperation<T> : BatchOperation<T>
    {
        private List<T> _batch = new List<T>();
        private ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public MemoryBatchOperation(int flushCount, TimeSpan flushTimeSpan, Action<IEnumerable<T>> batchAction) : base(flushCount, flushTimeSpan, batchAction)
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

    [DebuggerDisplay("Current = {_batch.Count} (Flush = {_flushCount}, Span = {_flushSpan})", Name = "{key}")]
    public abstract class BatchOperation<T> : IDisposable
    {
        private int _flushCount = 1;
        private TimeSpan _flushSpan = TimeSpan.Zero;
        private DateTime _lastActionDate = DateTime.UtcNow;
        private Action<IEnumerable<T>> _batchAction;
        private Timer _timer = null;


        public BatchOperation(int flushCount, TimeSpan flushTimeSpan, Action<IEnumerable<T>> batchAction)
        {
            this._flushCount = flushCount.EnsureRange(1, Int32.MaxValue);
            this._flushSpan = flushTimeSpan;
            _batchAction = batchAction;
            if (_flushSpan > TimeSpan.Zero)
            {
                var duration = _flushSpan.Add(TimeSpan.FromMilliseconds(250));
                _timer = new System.Threading.Timer(new System.Threading.TimerCallback(OnTimer), this, duration, duration);
            }
        }

        public void Add(T item)
        {
            ProtectedAdd(item);
            FlushIfReady();
        }
        protected abstract void ProtectedAdd(T item);

        protected abstract IEnumerable<T> BatchPending();

        public abstract int Count { get; }

        protected virtual void FlushIfReady(bool force = false)
        {
            if (IsFlushable || (force && Count > 0))
            {
                var batchToProcess = BatchPending();
                Task.Run(() => _batchAction(batchToProcess));
                _lastActionDate = DateTime.UtcNow;
            }
        }
        public bool IsFlushable
        {
            get
            {
                return Count > 0
                    && (Count >= _flushCount
                    || (_flushSpan <= DateTime.UtcNow.Subtract(_lastActionDate) && _flushSpan > TimeSpan.Zero));
            }
        }
        private void OnTimer(object state)
        {
            FlushIfReady();
        }

        ~BatchOperation()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            FlushIfReady(true);
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
