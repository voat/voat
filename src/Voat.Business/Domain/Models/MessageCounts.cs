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

namespace Voat.Domain.Models
{
    public class MessageCount
    {
        public MessageType Type { get; set; }

        public int Count { get; set; }
    }

    public class MessageCounts
    {
        public List<MessageCount> Counts { get; set; } = new List<MessageCount>();

        public MessageCounts(UserDefinition owner)
        {
            UserDefinition = owner;
        }

        public int GetCount(MessageType type)
        {
            return Counts.Where(x => x.Type == type).Sum(x => x.Count);
        }
        [JsonIgnore]
        public UserDefinition UserDefinition { get; private set; }

        public int Total
        {
            get
            {
                return Counts.Sum(x => x.Count);
            }
        }
    }
}
