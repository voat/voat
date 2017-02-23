using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{
    public class QueryUserSubscribedSets : Query<IEnumerable<DomainReferenceDetails>>
    {

        private SearchOptions _options;

        public QueryUserSubscribedSets(SearchOptions options) : base()
        {
            _options = options;
        }

        public override async Task<IEnumerable<DomainReferenceDetails>> ExecuteAsync()
        {
            using (var repo = new Repository())
            {
                var results = await repo.UserSubscribedSetDetails(UserName, _options);
                return results;
            }
        }
    }
}
