using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Voat.Common;
using System.Threading;

namespace Voat.Common
{
    [DebuggerDisplay("Current = {_lastCount} (Flush = {FlushCount}, Span = {FlushSpan})")]
    public abstract class BatchOperation<T> : IDisposable
    {
        private int _flushCount = 1;
        private int _lastCount = 0;
        private TimeSpan _flushSpan = TimeSpan.Zero;
        private DateTime _lastActionDate = DateTime.UtcNow;
        private Func<IEnumerable<T>,Task> _batchAction;
        private Timer _timer = null;


        public BatchOperation(int flushCount, TimeSpan flushSpan, Func<IEnumerable<T>, Task> batchAction)
        {
            this._flushCount = flushCount.EnsureRange(1, Int32.MaxValue);
            this._flushSpan = flushSpan.EnsureRange(TimeSpan.Zero, TimeSpan.MaxValue);
            _batchAction = batchAction;
            if (_flushSpan > TimeSpan.Zero)
            {
                var duration = _flushSpan.Add(TimeSpan.FromMilliseconds(250));
                _timer = new Timer(new System.Threading.TimerCallback(OnTimer), this, duration, duration);
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
                Task.Run(async () => await _batchAction(batchToProcess));
                _lastActionDate = DateTime.UtcNow;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsFlushable
        {
            get
            {
                _lastCount = Count;
                return _lastCount > 0
                    && (_lastCount >= _flushCount
                    || (_flushSpan <= DateTime.UtcNow.Subtract(_lastActionDate) && _flushSpan > TimeSpan.Zero));
            }
        }

        public int FlushCount { get => _flushCount; set => _flushCount = value; }
        public TimeSpan FlushSpan { get => _flushSpan; set => _flushSpan = value; }

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
