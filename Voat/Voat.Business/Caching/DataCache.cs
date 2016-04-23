using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Models;

namespace Voat.Caching
{
    //TODO: Remove this class and all it's little siblings once the query commands are finalized.
    [Obsolete("This class will be removed in future versions", false)]
    public static class DataCache
    {

        public static class Keys
        {

            public static string CommentTree(int submissionID)
            {
                return String.Format("legacy:Comment:Tree:{0}", submissionID).ToLower();
            }
            public static string Submission(int submissionID)
            {
                return String.Format("legacy:submission:{0}", submissionID).ToLower();
            }
            public static string SubverseInfo(string subverse)
            {
                return String.Format("legacy:subverse:{0}:info", subverse).ToLower();
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


        public static class CommentTree
        {
            private static int cacheTimeInSeconds = 90;
            
            //HACK ATTACK: This is short term hack. Sorry.
            public static void AddCommentToTree(Comment comment)
            {
                var t = new Task(() => {
                    try
                    {
                        string key = DataCache.Keys.CommentTree(comment.SubmissionID.Value);
                        
                        //not in any way thread safe, will be jacked in high concurrency situations
                        var c = Domain.DomainMaps.MapToTree(comment);

                        CacheHandler.Instance.Replace<List<usp_CommentTree_Result>>(key, new Func<List<usp_CommentTree_Result>, List<usp_CommentTree_Result>>(currentData =>
                        {
                            usp_CommentTree_Result parent = null;
                            if (c.ParentID != null)
                            {
                                parent = currentData.FirstOrDefault(x => x.ID == c.ParentID);
                                parent.ChildCount += 1;
                            }
                            currentData.Add(c);

                            return currentData;
                        }));

                    }
                    catch (Exception ex)
                    {
                        /*no-op*/
                    }
                });
                t.Start();
            }

            public static void Remove(int submissionID)
            {
                CacheHandler.Instance.Remove(DataCache.Keys.CommentTree(submissionID));
            }
            public static void Refresh(int submissionID)
            {
                CacheHandler.Instance.Refresh(DataCache.Keys.CommentTree(submissionID));
            }

            //This is the new process to retrieve comments.
            public static List<Voat.Data.Models.usp_CommentTree_Result> Retrieve<usp_CommentTree_Result>(int submissionID, int? depth, int? parentID)
            {
                return Retrieve<Voat.Data.Models.usp_CommentTree_Result>(submissionID, depth, parentID,
                    new Func<Voat.Data.Models.usp_CommentTree_Result, Voat.Data.Models.usp_CommentTree_Result>((x) => { return x; }),
                    new Action<Voat.Data.Models.usp_CommentTree_Result>((x) => { }));
            }
            //This is the new process to retrieve comments.
            public static List<T> Retrieve<T>(int submissionID, int? depth, int? parentID, Func<usp_CommentTree_Result, T> selector, Action<T> processor)
            {

                if (depth.HasValue && depth < 0)
                {
                    depth = null;
                }
                

                //Get cached results
                List<usp_CommentTree_Result> commentTree = CacheHandler.Instance.Register<List<usp_CommentTree_Result>>(DataCache.Keys.CommentTree(submissionID),

                    new Func<List<usp_CommentTree_Result>>(() =>
                    {
                        using (voatEntities db = new voatEntities())
                        {
                            //currently only working on the full tree, so params are nulled out
                            var flatTree = db.usp_CommentTree(submissionID, null, null).ToList();
                            return flatTree;
                        }
                    }), TimeSpan.FromSeconds(cacheTimeInSeconds), 10);

                //execute query
                var results = commentTree.Select(selector).ToList();

                if (results != null && processor != null)
                {
                    results.ForEach(processor);
                }
                return results;
            }
        }

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
                    Voat.Data.Models.Submission submission = CacheHandler.Instance.Register<Voat.Data.Models.Submission>(cacheKey, new Func<Voat.Data.Models.Submission>(() => {
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
        public static class Subverse
        {
            public static void Remove(string subverse)
            {
                CacheHandler.Instance.Remove(DataCache.Keys.SubverseInfo(subverse));
            }
            public static Voat.Data.Models.Subverse Retrieve(string subverse)
            {

                if (!String.IsNullOrEmpty(subverse))
                {
                    string cacheKey = DataCache.Keys.SubverseInfo(subverse);

                    var sub = CacheHandler.Instance.Register<Voat.Data.Models.Subverse>(cacheKey, new Func<Voat.Data.Models.Subverse>(() =>
                    {
                        using (voatEntities db = new voatEntities())
                        {
                            db.Configuration.ProxyCreationEnabled = false;
                            return db.Subverses.Where(x => x.Name.Equals(subverse, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        }
                    }), TimeSpan.FromMinutes(5), 50);

                    return sub;
                }

                return null;
            }

        }
    }
}