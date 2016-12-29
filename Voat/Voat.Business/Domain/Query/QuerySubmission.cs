using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Data;
using Voat.Domain.Models;

namespace Voat.Domain.Query
{


    public class QuerySubmission : CachedQuery<Domain.Models.Submission>
    {
        protected int _submissionID;
        protected bool _hydrateUserData;

        public QuerySubmission(int submissionID, bool hydrateUserData = false) : this(submissionID, hydrateUserData, new CachePolicy(TimeSpan.FromMinutes(3)))
        {

        }

        public QuerySubmission(int submissionID, bool hydrateUserData, CachePolicy policy) : base(policy)
        {
            this._submissionID = submissionID;
            this._hydrateUserData = hydrateUserData;
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

        public override async Task<Submission> ExecuteAsync()
        {
            var submission = await base.ExecuteAsync();
            if (_hydrateUserData)
            {
                DomainMaps.HydrateUserData(submission);
            }
            return submission;
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
