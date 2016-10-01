﻿using System;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    public class QuerySubverse : CachedQuery<Subverse>
    {
        private string _subverse;

        public QuerySubverse(string subverse) : base(new CachePolicy(TimeSpan.FromMinutes(10)))
        {
            _subverse = subverse;
        }

        public override string CacheKey
        {
            get
            {
                return _subverse;
            }
        }
        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.Subverse(_subverse);
            }
        }
        protected override Subverse GetData()
        {
            using (var db = new Repository())
            {
                return db.GetSubverseInfo(_subverse);
            }
        }
    }
}
