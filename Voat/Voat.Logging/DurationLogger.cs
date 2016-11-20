using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Logging
{
    public sealed class DurationLogger : IDisposable
    {
        private Stopwatch _stopwatch;
        private DateTime _startDate;
        private ILogger _logger;
        private ILogInformation _logInformation;

        public DurationLogger(ILogger logger, ILogInformation logInformation)
        {
            _logger = logger;
            _logInformation = logInformation;
            _stopwatch = new Stopwatch();
            _startDate = DateTime.UtcNow;
            _stopwatch.Start();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _logInformation.Data = new { elapsed = _stopwatch.Elapsed, elapsedMilliseconds = _stopwatch.ElapsedMilliseconds, startDate = _startDate };
            _logger.Log(_logInformation);
        }
    }
}
