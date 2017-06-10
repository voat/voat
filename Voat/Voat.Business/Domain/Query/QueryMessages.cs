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
using System.Security.Principal;
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
        protected SearchOptions _options = new SearchOptions(100);
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

        //public QueryMessageBase(MessageTypeFlag type, MessageState state, bool markAsRead = true)
        //    : this("", IdentityType.User, type, state, markAsRead)
        //{
        //    this._ownerName = UserName;
        //    this._ownerType = IdentityType.User;
        //}
    }

    public class QueryMessages : QueryMessageBase<IEnumerable<Domain.Models.Message>>
    {
        public QueryMessages(string ownerName, IdentityType ownerType, MessageTypeFlag type, MessageState state, bool markAsRead = true)
            : base(ownerName, ownerType, type, state, markAsRead)
        {
        }

        public QueryMessages(IPrincipal user, MessageTypeFlag type, MessageState state, bool markAsRead = true)
            : base(user.Identity.Name, IdentityType.User, type, state, markAsRead)
        {
        }
        public override async Task<IEnumerable<Message>> ExecuteAsync()
        {
            using (var repo = new Repository(User))
            {
                var result = await repo.GetMessages(_ownerName, _ownerType, _type, _state, _markAsRead, _options);

                //Hydrate user data
                var submissions = result.Where(x => x.Submission != null).Select(x => x.Submission);
                DomainMaps.HydrateUserData(User, submissions);

                var comments = result.Where(x => x.Comment != null).Select(x => x.Comment);
                DomainMaps.HydrateUserData(User, comments);

                return result;
            }
        }
    }
}
