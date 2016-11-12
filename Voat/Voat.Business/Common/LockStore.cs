using System.Collections.Generic;

namespace Voat.Common
{
    public class LockStore
    {
        private Dictionary<string, object> _lockObjects = new Dictionary<string, object>();
        private readonly object _lock = new object();
        private bool _enabled = true;

        public LockStore(bool enabled = true)
        {
            _enabled = enabled;
        }

        public object GetLockObject(string key)
        {
            if (_enabled)
            {
                var keyLookup = string.IsNullOrEmpty(key) ? "" : key.ToLower();

                if (!_lockObjects.ContainsKey(keyLookup))
                {
                    lock (_lock)
                    {
                        object o = (_lockObjects.ContainsKey(keyLookup) ? _lockObjects[keyLookup] : null);
                        if (o == null)
                        {
                            o = new object();
                            _lockObjects[keyLookup] = o;
                        }
                    }
                }
                return _lockObjects[keyLookup];
            }
            else
            {
                return new object();
            }
        }

        public bool Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                _enabled = value;
            }
        }

        public void Purge()
        {
            //clear out all old lockables
            lock (this)
            {
                _lockObjects = new Dictionary<string, object>();
            }
        }
    }
}
