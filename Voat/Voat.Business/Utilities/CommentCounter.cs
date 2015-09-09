using System;
using Voat.Data.Models;

namespace Voat.Utilities
{

    //THIS IS A TEMPORARY DEADLOCK WORKAROUND (AND NOW CACHEABLE CONTENT)
    public static class CommentCounter
    {
        public static int CommentCount(int submissionID)
        {
            int count = 0;

            string cacheKey = String.Format("comment.count.{0}", submissionID).ToString();
            object data = CacheHandler.Retrieve(cacheKey);
            if (data == null)
            {

                data = CacheHandler.Register(cacheKey, new Func<object>(() =>
                {
                    using (voatEntities db = new voatEntities())
                    {
                        var cmd = db.Database.Connection.CreateCommand();
                        cmd.CommandText = "SELECT COUNT(*) FROM Comment WITH (NOLOCK) WHERE SubmissionID = @SubmissionID AND IsDeleted != 1";
                        var param = cmd.CreateParameter();
                        param.ParameterName = "SubmissionID";
                        param.DbType = System.Data.DbType.Int32;
                        param.Value = submissionID;
                        cmd.Parameters.Add(param);

                        if (cmd.Connection.State != System.Data.ConnectionState.Open)
                        {
                            cmd.Connection.Open();
                        }
                        return (int)cmd.ExecuteScalar();
                    }

                }), TimeSpan.FromMinutes(2), 1);

                count = (int)data;
            }
            else {
                count = (int)data;
            }
            return count;
        }
    }


}