using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Models;

namespace Voat.Utilities
{
    public static class DataCache
    {
        public static class CommentTree
        {
            private static int cacheTimeInSeconds = 90;
            
            //HACK ATTACK: This is short term hack. Sorry.
            public static void AddCommentToTree(Comment comment)
            {
                var t = new Task(() => {
                    try
                    {
                        string key = CacheHandler.Keys.CommentTree(comment.SubmissionID.Value);
                        
                        //not in any way thread safe, will be jacked in high concurrency situations
                        var c = CommentBucketViewModel.Map(comment);

                        CacheHandler.Replace<List<usp_CommentTree_Result>>(key, new Func<List<usp_CommentTree_Result>, List<usp_CommentTree_Result>>(currentData =>
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
                CacheHandler.Remove(CacheHandler.Keys.CommentTree(submissionID));
            }
            public static void Refresh(int submissionID)
            {
                CacheHandler.Refresh(CacheHandler.Keys.CommentTree(submissionID));
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
                List<usp_CommentTree_Result> commentTree = CacheHandler.Register<List<usp_CommentTree_Result>>(CacheHandler.Keys.CommentTree(submissionID),

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
                CacheHandler.Remove(CacheHandler.Keys.Submission(submissionID));
            }

            /// <summary>
            /// </summary>
            /// <param name="submissionID">Using Nullable because everything seems to be nullable in this entire project</param>
            /// <returns></returns>
            public static Voat.Data.Models.Submission Retrieve(int? submissionID)
            {
                if (submissionID.HasValue && submissionID.Value > 0)
                {
                    string cacheKey = CacheHandler.Keys.Submission(submissionID.Value);
                    Voat.Data.Models.Submission submission = CacheHandler.Register<Voat.Data.Models.Submission>(cacheKey, new Func<Voat.Data.Models.Submission>(() => {
                        using (voatEntities db = new voatEntities())
                        {
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
                CacheHandler.Remove(CacheHandler.Keys.SubverseInfo(subverse));
            }
            public static Voat.Data.Models.Subverse Retrieve(string subverse)
            {

                if (!String.IsNullOrEmpty(subverse))
                {
                    string cacheKey = CacheHandler.Keys.SubverseInfo(subverse);

                    var sub = CacheHandler.Register<Voat.Data.Models.Subverse>(cacheKey, new Func<Voat.Data.Models.Subverse>(() =>
                    {
                        using (voatEntities db = new voatEntities())
                        {
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