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
using System.Threading;

namespace Voat.Common
{
    public class LockStore : BaseLockStore<object>
    {
        public LockStore(bool enabled = true) : base(enabled)
        {
        }

        protected override object CreateNewLockObject()
        {
            return new object();
        }
    }
    public class SemaphoreSlimLockStore : BaseLockStore<SemaphoreSlim>
    {
        public SemaphoreSlimLockStore(bool enabled = true) : base(enabled)
        {
        }
        protected override SemaphoreSlim CreateNewLockObject()
        {
            return new SemaphoreSlim(1);
        }
    }
    public abstract class BaseLockStore<T> where T : class
    {
        private Dictionary<string, WeakReference<T>> _lockObjects = new Dictionary<string, WeakReference<T>>();
        private readonly object _lock = new object();
        private bool _enabled = true;

        public BaseLockStore(bool enabled = true)
        {
            _enabled = enabled;
        }
        protected abstract T CreateNewLockObject();

        public T GetLockObject(string key)
        {
            if (_enabled)
            {
                var keyLookup = string.IsNullOrEmpty(key) ? "" : key.ToLower();

                WeakReference<T> weakRef = (_lockObjects.ContainsKey(keyLookup) ? _lockObjects[keyLookup] : (WeakReference<T>)null);
                if (weakRef != null)
                {
                    T target = null;
                    if (weakRef.TryGetTarget(out target))
                    {
                        //everything is good
                        return target;
                    }
                }
                //create new lockable 
                lock (_lock)
                {
                    var newLockable = CreateNewLockObject();
                    _lockObjects[keyLookup] = new WeakReference<T>(newLockable);
                    return newLockable;
                }
            }
            else
            {
                return CreateNewLockObject();
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
                _lockObjects = new Dictionary<string, WeakReference<T>>();
            }
        }
    }
}
