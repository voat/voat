using System;
using System.Linq;
using Voat.Data.Models;
using Voat.Domain.Query;

namespace Voat.Caching
{
    //TODO: Remove this class and all it's little siblings once the query commands are finalized.
    [Obsolete("This class will be removed in future versions", false)]
    public static class DataCache
    {
        public static class Keys
        {
            public static string Submission(int submissionID)
            {
                return String.Format("legacy:submission:{0}", submissionID).ToLower();
            }

            public static string Search(string subverse, string query)
            {
                return String.Format("legacy:search:{0}:{1}", subverse, query).ToLower();
            }

            public static string Search(string query)
            {
                return Search("all", query);
            }
        }

        [Obsolete("Replace Submission logic with class Submission_New logic")]
        public static class Submission
        {
            public static void Remove(int submissionID)
            {
                CacheHandler.Instance.Remove(DataCache.Keys.Submission(submissionID));
            }

            /// <summary>
            /// </summary>
            /// <param name="submissionID">Using Nullable because everything seems to be nullable in this entire project</param>
            /// <returns></returns>
            public static Voat.Data.Models.Submission Retrieve(int? submissionID)
            {
                if (submissionID.HasValue && submissionID.Value > 0)
                {
                    string cacheKey = DataCache.Keys.Submission(submissionID.Value);
                    Voat.Data.Models.Submission submission = CacheHandler.Instance.Register<Voat.Data.Models.Submission>(cacheKey, new Func<Voat.Data.Models.Submission>(() =>
                    {
                        using (voatEntities db = new voatEntities())
                        {
                            db.Configuration.ProxyCreationEnabled = false;
                            return db.Submissions.Where(x => x.ID == submissionID).FirstOrDefault();
                        }
                    }), TimeSpan.FromMinutes(30), -1);
                    return submission;
                }
                return null;
            }
        }

        //TODO: Repleace Submission class with this code (Views need to be converted)
        public static class Submission_New
        {
            public static void Remove(int submissionID)
            {
                CacheHandler.Instance.Remove(CachingKey.Submission(submissionID));
            }

            /// <summary>
            /// </summary>
            /// <param name="submissionID">Using Nullable because everything seems to be nullable in this entire project</param>
            /// <returns></returns>
            public static Voat.Domain.Models.Submission Retrieve(int? submissionID)
            {
                if (submissionID.HasValue && submissionID.Value > 0)
                {
                    string cacheKey = CachingKey.Submission(submissionID.Value);
                    var q = new QuerySubmission(submissionID.Value);
                    var submission = q.Execute();
                    return submission;
                }
                return null;
            }
        }

        public static class Subverse
        {
            //Leaving in for backwards compatibility
            public static Voat.Data.Models.Subverse Retrieve(string subverse)
            {
                if (!String.IsNullOrEmpty(subverse))
                {
                    var q = new QuerySubverse(subverse);
                    var sub = q.Execute();
                    return sub;
                }

                return null;
            }
        }
    }
}
