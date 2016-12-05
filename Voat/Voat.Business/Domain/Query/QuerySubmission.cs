using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;

namespace Voat.Domain.Query
{


    public class QuerySubmission : CachedQuery<Domain.Models.Submission>
    {
        protected int _submissionID;

        public QuerySubmission(int submissionID) : this(submissionID, new CachePolicy(TimeSpan.FromMinutes(3)))
        {
        }

        public QuerySubmission(int submissionID, CachePolicy policy) : base(policy)
        {
            this._submissionID = submissionID;
        }

        public override string CacheKey
        {
            get
            {
                return String.Format("{0}", _submissionID);
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.Submission(_submissionID);
            }
        }

        protected override async Task<Domain.Models.Submission> GetData()
        {
            using (var db = new Repository())
            {
                var result = db.GetSubmission(this._submissionID);

                //TODO: This returns submissions from disabled subs
                return result.Map();
            }
        }
    }
    
}
