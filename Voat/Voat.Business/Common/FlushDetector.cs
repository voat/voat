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
