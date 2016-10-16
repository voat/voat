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
    public class MessageCounts
    {
        public List<MessageCount> Counts { get; set; } = new List<MessageCount>();

        public int GetCount(MessageType type)
        {
            return Counts.Where(x => x.Type == type).Sum(x => x.Count);
        }

        public int Total
        {
            get
            {
                return Counts.Sum(x => x.Count);
            }
        }
    }
    public class QueryMessageCounts : QueryMessageBase<MessageCounts>
    {

        public QueryMessageCounts(string ownerName, IdentityType ownerType, MessageTypeFlag type, MessageState state)
            :base (ownerName, ownerType, type, state)
        {
        }

        public QueryMessageCounts(MessageTypeFlag type, MessageState state)
            : base(type, state)
        {
        }

        private MessageCounts Context
        {
            get
            {
                MessageCounts counts = null;

                if (System.Web.HttpContext.Current != null && _ownerType == IdentityType.User)
                {
                    const string key = "MessageCounts";
                    counts = (MessageCounts)System.Web.HttpContext.Current.Items[key];
                }
                return counts;
            }
            set
            {
                if (System.Web.HttpContext.Current != null && _ownerType == IdentityType.User)
                {
                    const string key = "MessageCounts";
                    if (value != null)
                    {
                        if (System.Web.HttpContext.Current != null)
                        {
                            System.Web.HttpContext.Current.Items[key] = value;
                        }
                    }
                }
            }
        }

        public override MessageCounts Execute()
        {
            Task<MessageCounts> t = Task.Run(ExecuteAsync);
            Task.WaitAll(t);
            return t.Result;
        }
        public override async Task<MessageCounts> ExecuteAsync()
        {
            var counts = Context;
            if (counts == null)
            {
                using (var repo = new Repository())
                {
                    counts = await repo.GetMessageCounts(_ownerName, _ownerType, _type, _state);
                    Context = counts;
                }
            }
            return counts;
        }
    }
}
