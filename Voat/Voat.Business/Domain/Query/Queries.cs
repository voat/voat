using System;
using System.Collections.Generic;
using System.Linq;
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

        protected override Domain.Models.Comment GetData()
        {
            using (var db = new Repository())
            {
                var result = db.GetComment(_commentID);
                DomainMaps.ProcessComment(result, true);
                return result;
            }
        }
    }

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

        protected override Domain.Models.Submission GetData()
        {
            using (var db = new Repository())
            {
                var result = db.GetSubmission(this._submissionID);

                //TODO: This returns submissions from disabled subs
                return result.Map();
            }
        }
    }

    public class QueryUserCommentVotesForSubmission : CachedQuery<IEnumerable<Data.Models.CommentVoteTracker>>
    {
        protected int _submissionID;

        public QueryUserCommentVotesForSubmission(int submissionID) : this(submissionID, new CachePolicy(TimeSpan.FromMinutes(5)))
        {
        }

        public QueryUserCommentVotesForSubmission(int submissionID, CachePolicy policy) : base(policy)
        {
            this._submissionID = submissionID;
        }

        public override string CacheKey
        {
            get
            {
                return String.Format("{0}:{1}", UserName, _submissionID);
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.UserCommentVotes(UserName, _submissionID);
            }
        }

        protected override IEnumerable<Data.Models.CommentVoteTracker> GetData()
        {
            using (var repo = new Repository())
            {
                var result = repo.UserCommentVotesBySubmission(_submissionID, UserName);
                return (result == null || !result.Any() ? null : result);
            }
        }
    }

    public class QueryUserSavedCommentsForSubmission : CachedQuery<IEnumerable<Data.Models.CommentSaveTracker>>
    {
        protected int _submissionID;

        public QueryUserSavedCommentsForSubmission(int submissionID) : this(submissionID, new CachePolicy(TimeSpan.FromMinutes(3)))
        {
        }

        public QueryUserSavedCommentsForSubmission(int submissionID, CachePolicy policy) : base(policy)
        {
            this._submissionID = submissionID;
        }

        public override string CacheKey
        {
            get
            {
                return String.Format("{0}:{1}", UserName, _submissionID);
            }
        }

        protected override string FullCacheKey
        {
            get
            {
                return CachingKey.UserSavedComments(UserName, _submissionID);
            }
        }

        protected override IEnumerable<Data.Models.CommentSaveTracker> GetData()
        {
            using (var repo = new Repository())
            {
                var result = repo.UserCommentSavedBySubmission(_submissionID, UserName);
                return (result == null || !result.Any() ? null : result);
            }
        }
    }
}
