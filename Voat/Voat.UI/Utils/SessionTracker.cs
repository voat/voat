#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using Voat.Data;
using Voat.Data.Models;
using Voat.Models;
using Voat.Models.ViewModels;

namespace Voat.UI.Utilities
{
    public static class SessionHelper
    {
        // remove a session
        public static void Remove(string sessionIdToRemove)
        {
            try
            {
                using (var db = new VoatUIDataContextAccessor())
                {
                    // remove all records for given session id
                    db.SessionTracker.RemoveRange(db.SessionTracker.Where(s => s.SessionID == sessionIdToRemove));
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                //
            }
        }

        // clear all sessions
        public static void RemoveAllSessions()
        {
            //CORE_PORT: Not ported
            throw new NotImplementedException("Core port");

            //try
            //{
            //    using (var db = new VoatUIDataContextAccessor())
            //    {
            //        db.Database.ExecuteSqlCommand("TRUNCATE TABLE SESSIONTRACKER");
            //        db.SaveChanges();
            //    }
            //}
            //catch (Exception)
            //{
            //    //
            //}
        }

        // add a new session
        public static void Add(string subverseName, string sessionId)
        {
            try
            {
                if (SessionExists(sessionId, subverseName)) return;
                using (var db = new VoatUIDataContextAccessor())
                {
                    var newSession = new SessionTracker { SessionID = sessionId, Subverse = subverseName, CreationDate = Repository.CurrentDate };

                    db.SessionTracker.Add(newSession);
                    db.SaveChanges();

                }
            }
            catch (Exception)
            {
                //
            }
        }

        // check if session exists
        public static bool SessionExists(string sessionId, string subverseName)
        {
            using (var db = new VoatUIDataContextAccessor())
            {
                var result = from sessions in db.SessionTracker
                             where sessions.Subverse.Equals(subverseName) && sessions.SessionID.Equals(sessionId)
                             select sessions;

                return result.Any();
            }
        }

        // get session count for given subverse
        //HACK: This query is expensive. Cache results.
        public static int ActiveSessionsForSubverse(string subverseName)
        {
            //CORE_PORT: Cache not available
            return -6666;

            /*
            try
            {
                string cacheKey = String.Format("legacy:activeSubSessions_{0}", subverseName);

                object cacheData = Context.Cache[cacheKey];
                if (cacheData != null)
                {
                    return (int)cacheData;
                }


                int count = 0;
                using (var db = new VoatUIDataContextAccessor())
                {

                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = "SELECT ISNULL(COUNT(*),0) FROM [dbo].[Sessiontracker] WITH (NOLOCK) WHERE [Subverse] = @Subverse";
                    var param = cmd.CreateParameter();
                    param.ParameterName = "Subverse";
                    param.DbType = System.Data.DbType.String;
                    param.Value = subverseName;
                    cmd.Parameters.Add(param);

                    if (cmd.Connection.State != System.Data.ConnectionState.Open)
                    {
                        cmd.Connection.Open();
                    }
                    count = (int)cmd.ExecuteScalar();
                    context.Cache.Insert(cacheKey, count, null, Repository.CurrentDate.AddSeconds(120), System.Web.Caching.Cache.NoSlidingExpiration);

                }


                return count;



                //using (var db = new VoatUIDataContextAccessor())
                //{
                //    var result = from sessions in db.Sessiontrackers
                //                 where sessions.Subverse.Equals(subverseName)
                //                 select sessions;

                //    return result.Count();
                //}
            }
            catch (Exception)
            {
                return -1;
            }
            */
        }

        // get top 10 subverses by number of online users
        public static List<ActiveSubverseViewModel> MostActiveSubverses()
        {
            try
            {
                using (var db = new VoatUIDataContextAccessor())
                {
                    var groups = db.SessionTracker
                                .GroupBy(n => n.Subverse)
                                .Select(n => new ActiveSubverseViewModel
                                {
                                    Name = n.Key,
                                    UsersOnline = n.Count()
                                }
                                )
                                .OrderByDescending(n => n.UsersOnline)
                                .Take(7);

                    return groups.ToList();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
