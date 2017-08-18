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
using Voat.Caching;
using Voat.Common;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Utilities;

namespace Voat.Domain.Query
{


    public class QueryAllMessageCounts : QueryMessageBase<IEnumerable<MessageCounts>>
    {

        public QueryAllMessageCounts(IPrincipal user, MessageTypeFlag type, MessageState state)
            : base(user.Identity.Name, IdentityType.User, type, state)
        {

        }

        public override async Task<IEnumerable<MessageCounts>> ExecuteAsync()
        {
            DemandAuthentication();

            List<MessageCounts> counts = new List<MessageCounts>();

            var userData = new UserData(_ownerName);
            List<Task<MessageCounts>> tasks = new List<Task<MessageCounts>>();

            tasks.Add(Task.Factory.StartNew(() => { var q = new QueryMessageCounts(_ownerName, _ownerType, this._type, this._state).SetUserContext(User); return q.Execute(); }));

            var modList = userData.Information.Moderates;
            foreach (var mod in modList)
            {
                if (Utilities.ModeratorPermission.HasPermission(mod.Level, ModeratorAction.ReadMail))
                {
                    tasks.Add(Task.Factory.StartNew(() => { var q = new QueryMessageCounts(mod.Subverse, IdentityType.Subverse, this._type, this._state).SetUserContext(User); return q.Execute(); }));
                }
            }

            var taskArray = tasks.ToArray();
            await Task.WhenAll(taskArray).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

            foreach (var task in taskArray)
            {
                counts.Add(task.Result);
            }

            return counts;
        }
    }

    public class QueryMessageCounts : QueryMessageBase<MessageCounts>
    {
        public QueryMessageCounts(string ownerName, IdentityType ownerType, MessageTypeFlag type, MessageState state)
            : base(ownerName, ownerType, type, state)
        {
        }

        public QueryMessageCounts(IPrincipal user, MessageTypeFlag type, MessageState state)
            : base(user.Identity.Name, IdentityType.User, type, state)
        {
        }

        private MessageCounts Context
        {
            get
            {
                MessageCounts counts = null;

                //CORE_PORT: HttpContext not available 

                //convert to this later
                //counts = ContextCache.Get<MessageCounts>(context, $"MessageCounts:{_ownerName}");
                //original code
                /*
                if (System.Web.HttpContext.Current != null && _ownerType == IdentityType.User)
                {
                    string key = _ownerName;
                    counts = (MessageCounts)System.Web.HttpContext.Current.Items[key];
                }
                */
                return counts;
            }

            set
            {
                //CORE_PORT: HttpContext not available 
                /*
                if (System.Web.HttpContext.Current != null && _ownerType == IdentityType.User)
                {
                    string key = _ownerName;
                    if (value != null)
                    {
                        if (System.Web.HttpContext.Current != null)
                        {
                            System.Web.HttpContext.Current.Items[key] = value;
                        }
                    }
                }
                */
            }
        }

        public override async Task<MessageCounts> ExecuteAsync()
        {
            MessageCounts counts = Context; //Used to check a context cache
            if (counts == null)
            {
                using (var repo = new Repository(User))
                {
                    counts = await repo.GetMessageCounts(_ownerName, _ownerType, _type, _state).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
                    //CORE_PORT: Trying to ditch this query cache
                    //Context = counts;
                }
            }
            return counts;
        }
    }
}
