using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Caching.Models
{
    /// <summary>
    /// Envelope for cached items
    /// </summary>
    /// <typeparam name="T">The Cached Item</typeparam>
    public class CacheEnvelope<T>
    {
        public DateTime CreationTime { get; set; }
        public TimeSpan ExpirationSpan { get; set; }

        public bool IsValid
        {
            get
            {
                return CreationTime.Add(ExpirationSpan) >= DateTime.UtcNow;
            }
        }

        //Ideally cached items *should* have enough data to recache themselves.... Hmmm... Need to think about this.
        //public Query RefreshQuery { get; set; }

        public T Item { get; set; }
    }
}
