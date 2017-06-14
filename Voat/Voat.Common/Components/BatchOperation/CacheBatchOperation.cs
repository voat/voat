using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Caching;

namespace Voat.Common
{
    public class CacheBatchOperation<T> : BatchOperation<T>
    {
        private string _keyPrefix = null;
        private string _currentKey = null;
        private ICacheHandler _cacheHandler = null;
        private bool _clearPrevious = true;

        public CacheBatchOperation(string keySpace, ICacheHandler cacheHandler, int flushCount, TimeSpan flushSpan, Func<IEnumerable<T>, Task> batchAction) : base(flushCount, flushSpan, batchAction)
        {
            _cacheHandler = cacheHandler;
            _keyPrefix = $"{keySpace}";
            SetNewKey();
        }

        /// <summary>
        /// Changes the current key and returns the previously used key
        /// </summary>
        /// <returns></returns>
        protected string SetNewKey()
        {
            var existingKey = _currentKey;
            //_currentKey = $"{_keyPrefix}:{Guid.NewGuid().ToString()}".ToLower();
            _currentKey = $"{_keyPrefix}:{DateTime.UtcNow.ToDateTimeStamp()}".ToLower();
            return existingKey;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override int Count => _cacheHandler.ListLength(_currentKey);

        public bool ClearPrevious { get => _clearPrevious; set => _clearPrevious = value; }

        protected override IEnumerable<T> BatchPending()
        {
            var keyToProcess = SetNewKey();
            var list = _cacheHandler.ListRetrieveAll<T>(keyToProcess).ToList(); //materialize
            if (ClearPrevious)
            {
                _cacheHandler.Remove(keyToProcess);
            }
            return list;
        }

        protected override void ProtectedAdd(T item)
        {
            _cacheHandler.ListAdd(_currentKey, item);
        }
    }
}
