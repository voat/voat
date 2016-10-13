using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{

    public abstract class QueryMessageBase<T> : Query<T>
    {
        protected string _ownerName;
        protected MessageIdentityType _ownerType;
        protected MessageState _state;
        protected bool _markAsRead;
        protected MessageTypeFlag _type;

        public QueryMessageBase(string ownerName, MessageIdentityType ownerType, MessageTypeFlag type, MessageState state, bool markAsRead = true)
        {
            this._ownerName = ownerName;
            this._ownerType = ownerType;
            this._state = state;
            this._markAsRead = markAsRead;
            this._type = type;
        }
        public QueryMessageBase(MessageTypeFlag type, MessageState state, bool markAsRead = true)
            : this("", MessageIdentityType.User, type, state, markAsRead)
        {
            this._ownerName = UserName;
            this._ownerType = MessageIdentityType.User;
        }
    }

    public class QueryMessages : QueryMessageBase<IEnumerable<Domain.Models.Message>>
    {
       
        public QueryMessages(string ownerName, MessageIdentityType ownerType, MessageTypeFlag type, MessageState state, bool markAsRead = true)
            : base (ownerName, ownerType, type, state, markAsRead)
        {
        }

        public QueryMessages(MessageTypeFlag type, MessageState state, bool markAsRead = true)
            : base(type, state, markAsRead)
        {
        }

        public override async Task<IEnumerable<Message>> ExecuteAsync()
        {
            using (var repo = new Repository())
            {
                return await repo.GetMessages(_ownerName, _ownerType, _type, _state, _markAsRead);
            }
        }

        public override IEnumerable<Message> Execute()
        {
            Task<IEnumerable<Message>> t = Task.Run(ExecuteAsync);
            Task.WaitAll(t);
            return t.Result;
        }
    }

}
