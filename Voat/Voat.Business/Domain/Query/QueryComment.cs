using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;

namespace Voat.Domain.Query
{
    public class QueryComment : CachedQuery<Domain.Models.Comment>
    {
        protected int _commentID;

        public QueryComment(int commentID, CachePolicy policy = null) : base(policy)
        {
            this._commentID = commentID;
        }

        public override string CacheKey
        {
            get
            {
                return String.Format("{0}", _commentID);
            }
        }

        protected override async Task<Domain.Models.Comment> GetData()
        {
            using (var db = new Repository())
            {
                var result = db.GetComment(_commentID);
                DomainMaps.ProcessComment(result, true);
                return result;
            }
        }
    }
}
