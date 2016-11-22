using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{


    public class QueryAllMessageCounts : QueryMessageBase<IEnumerable<MessageCounts>>
    {

        public QueryAllMessageCounts(MessageTypeFlag type, MessageState state)
            : base(type, state)
        {

        }

        public override async Task<IEnumerable<MessageCounts>> ExecuteAsync()
        {
            List<MessageCounts> counts = new List<MessageCounts>();

            var userData = new UserData(this._ownerName);
            List<Task<MessageCounts>> tasks = new List<Task<MessageCounts>>();

            tasks.Add(Task.Factory.StartNew(() => { var q = new QueryMessageCounts(this._ownerName, this._ownerType, this._type, this._state); return q.Execute(); }));

            var modList = userData.Information.Moderates;
            foreach (var mod in modList)
            {
                if (Utilities.ModeratorPermission.HasPermission(mod.Level, ModeratorAction.ReadMail))
                {
                    tasks.Add(Task.Factory.StartNew(() => { var q = new QueryMessageCounts(mod.Subverse, IdentityType.Subverse, this._type, this._state); return q.Execute(); }));
                }
            }

            var taskArray = tasks.ToArray();
            await Task.WhenAll(taskArray).ConfigureAwait(false);

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
                    string key = _ownerName;
                    counts = (MessageCounts)System.Web.HttpContext.Current.Items[key];
                }
                return counts;
            }

            set
            {
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
            }
        }

        public override async Task<MessageCounts> ExecuteAsync()
        {
            var counts = Context;
            if (counts == null)
            {
                using (var repo = new Repository())
                {
                    counts = await repo.GetMessageCounts(_ownerName, _ownerType, _type, _state).ConfigureAwait(false);
                    Context = counts;
                }
            }
            return counts;
        }
    }
}
