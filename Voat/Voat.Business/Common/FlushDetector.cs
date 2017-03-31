using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Common
{
    public class FlushDetector
    {
        private int _flushCount = 1;
        private TimeSpan _flushSpan = TimeSpan.Zero;

        private int _currentCount = 0;
        private DateTime _lastActionDate = DateTime.UtcNow;

        public FlushDetector(int count, TimeSpan span)
        {
            this._flushCount = count;
            this._flushSpan = span;
        }

        public bool IsFlushable
        {
            get
            {
                return _currentCount >= _flushCount || (_flushSpan <= DateTime.UtcNow.Subtract(_lastActionDate) && _flushSpan != TimeSpan.Zero);
            }
        }

        public void Increment()
        {
            _currentCount += 1;
            _lastActionDate = DateTime.UtcNow;
        }
        public void Reset()
        {
            _currentCount = 0;
            _lastActionDate = DateTime.UtcNow;
        }

        public int FlushCount
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

        public TimeSpan FlushSpan
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
    }
}
