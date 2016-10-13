using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{

    public class MessageCount
    {
        public MessageType Type { get; set; }
        public int Count { get; set; }
    }

    public class QueryMessageCounts : QueryMessageBase<IEnumerable<MessageCount>>
    {

        public QueryMessageCounts(string ownerName, MessageIdentityType ownerType, MessageTypeFlag type, MessageState state, bool markAsRead = true)
            :base (ownerName, ownerType, type, state, markAsRead)
        {
        }

        public QueryMessageCounts(MessageTypeFlag type, MessageState state, bool markAsRead = true)
            : base(type, state, markAsRead)
        {
        }


        public override IEnumerable<MessageCount> Execute()
        {
            Task<IEnumerable< MessageCount >> t = Task.Run(ExecuteAsync);
            Task.WaitAll(t);
            return t.Result;
        }
        public override async Task<IEnumerable<MessageCount>> ExecuteAsync()
        {
            using (var repo = new Repository())
            {
                return await repo.GetMessageCounts(_ownerName, _ownerType, _type, _state, _markAsRead);
            }
        }
    }
}
