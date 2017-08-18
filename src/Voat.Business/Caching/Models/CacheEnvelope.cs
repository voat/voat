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
