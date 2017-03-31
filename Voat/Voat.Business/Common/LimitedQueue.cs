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
        public void Add(T item)
        {
            while (Count >= Limit && Count > 0)
            {
                base.Dequeue();
            }
            base.Enqueue(item);
        }
    }
}
