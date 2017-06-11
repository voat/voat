using System;
using System.Collections.Generic;
using System.Text;
using Voat.Caching;

namespace Voat.Common
{
    public class CacheBatchOperation<T> : BatchOperation<T>
    {
        private string _keyPrefix = null;
        private string _currentKey = null;
        private ICacheHandler _cacheHandler = null;
        private bool _clearPrevious = true;

        public CacheBatchOperation(string keySpace, ICacheHandler cacheHandler, int flushCount, TimeSpan flushSpan, Action<IEnumerable<T>> batchAction) : base(flushCount, flushSpan, batchAction)
        {
            _cacheHandler = cacheHandler;
            _keyPrefix = $"{keySpace}:{typeof(T).Name}";
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

        public override int Count => _cacheHandler.ListLength(_currentKey);

        public bool ClearPrevious { get => _clearPrevious; set => _clearPrevious = value; }

        protected override IEnumerable<T> BatchPending()
        {
            var keyToProcess = SetNewKey();
            var list = _cacheHandler.ListRetrieveAll<T>(keyToProcess);
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
