using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryUserSets : Query<IEnumerable<Set>>
    {
        protected string _userName;

        public QueryUserSets(string userName)
        {
            _userName = userName;
        }

        public override async Task<IEnumerable<Set>> ExecuteAsync()
        {
            //TODO: Move to query
            using (var repo = new Repository())
            {
                var setList = await repo.GetUserSets(_userName);
                return setList.Map();
            }
        }
    }
}
