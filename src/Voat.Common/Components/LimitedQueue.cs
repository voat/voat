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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Common
{
    
    public class LimitedQueue<T> : Queue<T>
    {
        private int _limit = 100;

        public LimitedQueue(IEnumerable<T> collection) : base(collection)
        {

        }
        public LimitedQueue() : this(100) { }

        public LimitedQueue(int limit) : base(limit)
        {
            Limit = limit;

        }
        public int Limit
        {
            get
            {
                return _limit;
            }
            set
            {
                _limit = value;
            }
        }

        public new void Enqueue(T item)
        {
            Add(item);
        }
        public virtual void Add(T item)
        {
            while (Count >= Limit && Count > 0)
            {
                base.Dequeue();
            }
            base.Enqueue(item);
        }
    }
    public class LimitedStack<T> : Stack<T>
    {
        private int _limit = 100;

        public LimitedStack(IEnumerable<T> collection) : base(collection)
        {

        }
        public LimitedStack() : this(100) { }

        public LimitedStack(int limit) : base(limit)
        {
            Limit = limit;

        }
        public int Limit
        {
            get
            {
                return _limit;
            }
            set
            {
                _limit = value;
            }
        }

        public new void Push(T item)
        {
            Add(item);
        }
        public virtual void Add(T item)
        {
            while (Count >= Limit && Count > 0)
            {
                base.Pop();
            }
            base.Push(item);
        }
    }
    public enum TrimEnd
    {
        Head,
        Tail
    }
    public class LimitedList<T> : List<T>
    {
        private int _limit = 100;
        /// <summary>
        /// Changes the behavior of items being added - Tail (Queue), Head (Stack)
        /// </summary>
        private TrimEnd TrimBehavior { get; set; } = TrimEnd.Tail;

        public LimitedList(IEnumerable<T> collection) : base(collection)
        {

        }

        public LimitedList() : this(100) { }

        public LimitedList(int limit) : base(limit)
        {
            Limit = limit;

        }
        public int Limit { get => _limit; set => _limit = value; }

        public virtual new void Add(T item)
        {
            while (Count >= Limit && Count > 0)
            {
                switch (TrimBehavior)
                {
                    case TrimEnd.Head:
                        base.RemoveAt(0);
                        break;
                    case TrimEnd.Tail:
                        base.RemoveAt(Count - 1);
                        break;
                }
            }

            switch (TrimBehavior)
            {
                case TrimEnd.Head:
                    base.Add(item);
                    break;
                case TrimEnd.Tail:
                    base.Insert(0, item);
                    break;
            }
            
        }
    }
}
