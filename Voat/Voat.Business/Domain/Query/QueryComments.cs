﻿using System.Collections.Generic;
using Voat.Caching;
using Voat.Data;

namespace Voat.Domain.Query
{
    public class QueryComments : CachedQuery<IEnumerable<Domain.Models.Comment>>
    {
        protected SearchOptions _options;

        public QueryComments(SearchOptions options, CachePolicy policy = null) : base(policy)
        {
            this._options = options;
        }

        public override string CacheKey
        {
            get
            {
                return _options.ToString();
            }
        }

        protected override IEnumerable<Domain.Models.Comment> GetData()
        {
            using (var db = new Repository())
            {
                var result = db.GetComments(null, this._options);
                return result.Map();
            }
        }
    }
}