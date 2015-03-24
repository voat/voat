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
using System.Collections.Generic;
using System.Linq;
using Voat.Models;
using Voat.Models.ViewModels;

namespace Voat.Utils
{
    public static class SessionTracker
    {
        // remove a session
        public static void Remove(string sessionIdToRemove)
        {
            try
            {
                using (var db = new whoaverseEntities())
                {
                    // remove all records for given session id
                    db.Sessiontrackers.RemoveRange(db.Sessiontrackers.Where(s => s.SessionId == sessionIdToRemove));
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
            try
            {
                using (var db = new whoaverseEntities())
                {
                    db.Database.ExecuteSqlCommand("TRUNCATE TABLE SESSIONTRACKER");
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                //
            }
        }

        // add a new session
        public static void Add(string subverseName, string sessionId)
        {
            try
            {
                if (SessionExists(sessionId, subverseName)) return;
                using (var db = new whoaverseEntities())
                {
                    var newSession = new Sessiontracker { SessionId = sessionId, Subverse = subverseName, Timestamp = DateTime.Now };

                    db.Sessiontrackers.Add(newSession);
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
            using (var db = new whoaverseEntities())
            {
                var result = from sessions in db.Sessiontrackers
                             where sessions.Subverse.Equals(subverseName) && sessions.SessionId.Equals(sessionId)
                             select sessions;

                return result.Any();
            }
        }

        // get session count for given subverse
        public static int ActiveSessionsForSubverse(string subverseName)
        {
            try
            {
                using (var db = new whoaverseEntities())
                {
                    var result = from sessions in db.Sessiontrackers
                                 where sessions.Subverse.Equals(subverseName)
                                 select sessions;

                    return result.Count();
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        // get top 10 subverses by number of online users
        public static List<ActiveSubverseViewModel> MostActiveSubverses()
        {
            try
            {
                using (var db = new whoaverseEntities())
                {
                    var groups = db.Sessiontrackers
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