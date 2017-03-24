using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Common
{
    public class LimitedQueue<T> : Queue<T>
    {
        public int Limit { get; set; }

        public LimitedQueue(int limit) : base(limit)
        {
            Limit = limit;
        }
        public new void Enqueue(T item)
        {
            Add(item);
        }
        public void Add(T item)
        {
            while (Count >= Limit)
            {
                base.Dequeue();
            }
            base.Enqueue(item);
        }
    }
}
