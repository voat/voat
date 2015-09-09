/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2014 Voat
All Rights Reserved.
*/

using System;
using System.Web.Caching;
using Voat.Data.Models;

namespace Voat.Utilities
{

    public enum KarmaCacheType
    {
        Link,
        Comment,
        UpvoteTotal,
        SubverseLink,
        SubverseComment
    }

    //HACK: This class is now hacked to reduce DB calls and locking. This will be done properly at a later date. Apologies for what follows.
    public static class Karma
    {
        private const int cacheTimeInSeconds = 90;

        private static string CacheKey(string userName, KarmaCacheType type, string subverse = null)
        {
            return String.Format("{0}_{1}_{2}", userName, type.ToString(), subverse ?? "none");
        }

        private static Cache Cache
        {
            get
            {
                return System.Web.HttpContext.Current.Cache;
            }
        }

        // get link contribution points for a user
        public static int LinkKarma(string userName)
        {

            string cacheKey = CacheKey(userName, KarmaCacheType.Link);

            object cacheData = Cache[cacheKey];
            if (cacheData != null)
            {
                return (int)cacheData;
            }


            int count = 0;
            using (voatEntities db = new voatEntities())
            {

                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "SELECT ISNULL(SUM(UpCount - DownCount), 0) FROM Submission WITH (NOLOCK) WHERE UserName = @UserName";
                var param = cmd.CreateParameter();
                param.ParameterName = "UserName";
                param.DbType = System.Data.DbType.String;
                param.Value = userName;
                cmd.Parameters.Add(param);

                if (cmd.Connection.State != System.Data.ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }
                long l = (long)cmd.ExecuteScalar();
                count = (int)l;
                Cache.Insert(cacheKey, count, null, DateTime.Now.AddSeconds(cacheTimeInSeconds), System.Web.Caching.Cache.NoSlidingExpiration);

            }


            return count;


            //using (var db = new voatEntities())
            //{
            //    try
            //    {
            //        return db.Messages.Where(c => c.Name.Trim().Equals(userName, StringComparison.OrdinalIgnoreCase))
            //            .Select(c => c.Likes - c.Dislikes)
            //            .Sum();
            //    }
            //    catch (Exception)
            //    {
            //        return 0;
            //    }
            //}
        }

        // get link contribution points for a user from a given subverse
        public static int LinkKarmaForSubverse(string userName, string subverseName)
        {

            string cacheKey = CacheKey(userName, KarmaCacheType.SubverseLink, subverseName);

            object cacheData = Cache[cacheKey];
            if (cacheData != null)
            {
                return (int)cacheData;
            }


            int count = 0;
            using (voatEntities db = new voatEntities())
            {

                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "SELECT ISNULL(SUM(UpCount - DownCount), 0) FROM Submission WITH (NOLOCK) WHERE UserName = @UserName AND Subverse = @Subverse";

                var param = cmd.CreateParameter();
                param.ParameterName = "UserName";
                param.DbType = System.Data.DbType.String;
                param.Value = userName;
                cmd.Parameters.Add(param);

                param = cmd.CreateParameter();
                param.ParameterName = "Subverse";
                param.DbType = System.Data.DbType.String;
                param.Value = subverseName;
                cmd.Parameters.Add(param);

                if (cmd.Connection.State != System.Data.ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }
                long l = (long)cmd.ExecuteScalar();
                count = (int)l;
                Cache.Insert(cacheKey, count, null, DateTime.Now.AddSeconds(cacheTimeInSeconds), System.Web.Caching.Cache.NoSlidingExpiration);

            }


            return count;


            //using (var db = new voatEntities())
            //{
            //    try
            //    {
            //        return db.Messages.Where(c => c.Name.Trim().Equals(userName, StringComparison.OrdinalIgnoreCase) && c.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase))
            //            .Select(c => c.Likes - c.Dislikes)
            //            .Sum();
            //    }
            //    catch (Exception)
            //    {
            //        return 0;
            //    }
            //}
        }

        // get comment contribution points for a user
        public static int CommentKarma(string userName)
        {

            string cacheKey = CacheKey(userName, KarmaCacheType.Comment);

            object cacheData = Cache[cacheKey];
            if (cacheData != null)
            {
                return (int)cacheData;
            }


            int count = 0;
            using (voatEntities db = new voatEntities())
            {

                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "SELECT ISNULL(SUM(UpCount - DownCount), 0) FROM Comment WITH (NOLOCK) WHERE UserName = @UserName";

                var param = cmd.CreateParameter();
                param.ParameterName = "UserName";
                param.DbType = System.Data.DbType.String;
                param.Value = userName;
                cmd.Parameters.Add(param);

                if (cmd.Connection.State != System.Data.ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }
                long l = (long)cmd.ExecuteScalar();
                count = (int)l;
                Cache.Insert(cacheKey, count, null, DateTime.Now.AddSeconds(cacheTimeInSeconds), System.Web.Caching.Cache.NoSlidingExpiration);

            }


            return count;

            //using (var db = new voatEntities())
            //{
            //    try
            //    {
            //        return db.Comments.Where(c => c.Name.Trim().Equals(userName, StringComparison.OrdinalIgnoreCase))
            //            .Select(c => c.Likes - c.Dislikes)
            //            .Sum();
            //    }
            //    catch (Exception)
            //    {
            //        return 0;
            //    }
            //}
        }

        // get comment contribution points for a user from a given subverse
        public static int CommentKarmaForSubverse(string userName, string subverseName)
        {

            string cacheKey = CacheKey(userName, KarmaCacheType.SubverseComment, subverseName);

            object cacheData = Cache[cacheKey];
            if (cacheData != null)
            {
                return (int)cacheData;
            }


            int count = 0;
            using (voatEntities db = new voatEntities())
            {

                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = @"SELECT ISNULL(SUM(c.UpCount - c.DownCount), 0) FROM Comment c WITH (NOLOCK) 
                                    INNER JOIN Submission m WITH (NOLOCK) ON (m.ID = c.SubmissionID)
                                    WHERE c.UserName = @UserName AND m.Subverse = @Subverse";


                var param = cmd.CreateParameter();
                param.ParameterName = "UserName";
                param.DbType = System.Data.DbType.String;
                param.Value = userName;
                cmd.Parameters.Add(param);

                param = cmd.CreateParameter();
                param.ParameterName = "Subverse";
                param.DbType = System.Data.DbType.String;
                param.Value = subverseName;
                cmd.Parameters.Add(param);


                if (cmd.Connection.State != System.Data.ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }
                long l = (long)cmd.ExecuteScalar();
                count = (int)l;
                Cache.Insert(cacheKey, count, null, DateTime.Now.AddSeconds(cacheTimeInSeconds), System.Web.Caching.Cache.NoSlidingExpiration);

            }


            return count;

            //using (var db = new voatEntities())
            //{
            //    try
            //    {
            //        return db.Comments.Join(db.Messages, comment => comment.MessageId, message => message.Id, (comment, message) => new {comment, message})
            //            .Where(
            //                x =>
            //                    x.comment.Name != "deleted" && x.comment.Name == userName &&
            //                    x.message.Subverse == subverseName)
            //            .Select(x => x.comment.Likes - x.comment.Dislikes)
            //            .Sum();
            //    }
            //    catch (Exception)
            //    {
            //        return 0;
            //    }
            //}
        }

        // get total upvotes given by a user
        public static int UpvotesGiven(string userName)
        {


            string cacheKey = CacheKey(userName, KarmaCacheType.UpvoteTotal);

            object cacheData = Cache[cacheKey];
            if (cacheData != null)
            {
                return (int)cacheData;
            }


            int count = 0;
            using (voatEntities db = new voatEntities())
            {

                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = @"SELECT 
                                    (SELECT ISNULL(COUNT(*), 0) FROM CommentVoteTracker WITH (NOLOCK) WHERE UserName = @Name AND VoteStatus = 1) 
                                    +
                                    (SELECT ISNULL(COUNT(*), 0) FROM SubmissionVoteTracker WITH (NOLOCK) WHERE UserName = @Name AND VoteStatus = 1)";

                var param = cmd.CreateParameter();
                param.ParameterName = "Name";
                param.DbType = System.Data.DbType.String;
                param.Value = userName;
                cmd.Parameters.Add(param);

                if (cmd.Connection.State != System.Data.ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }
                count = (int)cmd.ExecuteScalar();
                //count = (int)l;
                Cache.Insert(cacheKey, count, null, DateTime.Now.AddSeconds(cacheTimeInSeconds), System.Web.Caching.Cache.NoSlidingExpiration);

            }


            return count;


            //using (var db = new voatEntities())
            //{
            //    try
            //    {
            //        var submissionUpvotes = db.Votingtrackers.Count(a => a.UserName == userName && a.VoteStatus == 1);
            //        var commentUpvotes = db.Commentvotingtrackers.Count(a => a.UserName == userName && a.VoteStatus == 1);
            //        return submissionUpvotes + commentUpvotes;
            //    }
            //    catch (Exception)
            //    {
            //        return 0;
            //    }
            //}
        }
    }

}

