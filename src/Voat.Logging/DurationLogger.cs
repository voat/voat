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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;

namespace Voat.Logging
{
    public sealed class DurationLogger : IDisposable
    {
        private Stopwatch _stopwatch;
        private DateTime _startDate;
        private TimeSpan _minDuration = TimeSpan.Zero;
        private ILogger _logger;
        private ILogInformation _logInformation;

        public DurationLogger(ILogger logger, ILogInformation logInformation, TimeSpan? minimumDuration = null)
        {
            _logger = logger;
            _logInformation = logInformation;
            _stopwatch = new Stopwatch();
            _startDate = DateTime.UtcNow;
            if (minimumDuration.HasValue && minimumDuration.Value > TimeSpan.Zero)
            {
                _minDuration = minimumDuration.Value;
            }
            _stopwatch.Start();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            if (_stopwatch.Elapsed > _minDuration)
            {
                _logInformation.Data = new { elapsed = _stopwatch.Elapsed, elapsedMilliseconds = _stopwatch.ElapsedMilliseconds, startDate = _startDate, data = _logInformation.Data };
                _logger.Log(_logInformation);
            }
        }
    }
}
