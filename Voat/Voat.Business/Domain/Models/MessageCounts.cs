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
