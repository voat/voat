using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Data.Models;

namespace Voat.Domain.Query
{
    //This class exposes anon user names as it needs to determine submitter state
    internal class QueryCommentTree : CachedQuery<IDictionary<int, usp_CommentTree_Result>>
    {
        protected int _submissionID;

        public QueryCommentTree(int submissionID) : this(submissionID, new CachePolicy(TimeSpan.FromMinutes(15)))
        {
            //default constructor with cache policy specified.
        }

        public QueryCommentTree(int submissionID, CachePolicy policy = null) : base(policy)
        {
            _submissionID = submissionID;
        }

        public override string CacheKey
        {
            get
            {
                return _submissionID.ToString();
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.CommentTree(_submissionID);
            }
        }

        protected override async Task<IDictionary<int, usp_CommentTree_Result>> GetData()
        {
            using (var db = new Repository())
            {
                return db.GetCommentTree(this._submissionID, null, null).ToDictionary(x => x.ID);
            }
        }
    }
}
