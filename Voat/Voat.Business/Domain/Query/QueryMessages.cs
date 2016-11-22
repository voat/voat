using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public abstract class QueryMessageBase<T> : Query<T>
    {
        protected string _ownerName;
        protected IdentityType _ownerType;
        protected MessageState _state;
        protected bool _markAsRead;
        protected MessageTypeFlag _type;
        protected SearchOptions _options = SearchOptions.Default;
        //private int _pageNumber = 0;
        //private int _pageCount = 25;

        public int PageNumber
        {
            get
            {
                return _options.Page;
            }

            set
            {
                if (value < 0)
                {
                    throw new InvalidOperationException("Page number must be 0 or greater");
                }
                _options.Page = value;
            }
        }

        public int PageCount
        {
            get
            {
                return _options.Count;
            }

            set
            {
                if (value <= 0)
                {
                    throw new InvalidOperationException("Page count must be 0 or greater");
                }
                _options.Count = value;
            }
        }

        public QueryMessageBase(string ownerName, IdentityType ownerType, MessageTypeFlag type, MessageState state, bool markAsRead = true)
        {
            this._ownerName = ownerName;
            this._ownerType = ownerType;
            this._state = state;
            this._markAsRead = markAsRead;
            this._type = type;
        }

        public QueryMessageBase(MessageTypeFlag type, MessageState state, bool markAsRead = true)
            : this("", IdentityType.User, type, state, markAsRead)
        {
            this._ownerName = UserName;
            this._ownerType = IdentityType.User;
        }
    }

    public class QueryMessages : QueryMessageBase<IEnumerable<Domain.Models.Message>>
    {
        public QueryMessages(string ownerName, IdentityType ownerType, MessageTypeFlag type, MessageState state, bool markAsRead = true)
            : base(ownerName, ownerType, type, state, markAsRead)
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
                return await repo.GetMessages(_ownerName, _ownerType, _type, _state, _markAsRead, _options);
            }
        }
    }
}
