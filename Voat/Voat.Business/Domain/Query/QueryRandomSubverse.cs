using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;

namespace Voat.Domain.Query
{
    public class QueryRandomSubverse : Query<string>
    {
        protected bool _nsfw;

        public QueryRandomSubverse(bool NSFW)
        {
            this._nsfw = NSFW;
        }

        public override async Task<string> ExecuteAsync()
        {
            using (var repo = new Repository())
            {
                var name = await repo.GetRandomSubverse(_nsfw);
                return name;
            }
        }
    }
}
