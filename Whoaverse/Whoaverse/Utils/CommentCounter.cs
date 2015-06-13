using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Voat.Models;

namespace Voat.Utils
{

    //THIS IS A TEMPORARY DEADLOCK WORKAROUND
    public static class CommentCounter
    {
        public static int CommentCount(int submissionID)
        {
            int count = 0;

            using (whoaverseEntities db = new whoaverseEntities())
            {

                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Comments WITH (NOLOCK) WHERE MessageID = @MessageID AND Name != 'deleted'";
                var param = cmd.CreateParameter();
                param.ParameterName = "MessageID";
                param.DbType = System.Data.DbType.Int32;
                param.Value = submissionID;
                cmd.Parameters.Add(param);

                if (cmd.Connection.State != System.Data.ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }
                count = (int)cmd.ExecuteScalar();
            }
            return count;
        }
    }


}