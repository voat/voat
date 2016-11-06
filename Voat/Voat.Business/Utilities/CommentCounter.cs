using System;
using Voat.Caching;
using Voat.Data.Models;

namespace Voat.Utilities
{
    public static class CommentCounter
    {
        public static int CommentCount(int submissionID)
        {
            int count = 0;

            string cacheKey = CachingKey.CommentCount(submissionID);
            var data = CacheHandler.Instance.Retrieve<int?>(cacheKey);
            if (data == null)
            {
                data = CacheHandler.Instance.Register(cacheKey, new Func<int?>(() =>
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
                }), TimeSpan.FromMinutes(4), 1);

                count = (int)data;
            }
            else
            {
                count = (int)data;
            }
            return count;
        }
    }
}
