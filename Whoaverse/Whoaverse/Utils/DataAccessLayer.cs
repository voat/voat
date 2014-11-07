/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
*/

using System.Linq;
using Whoaverse.Models;

namespace Whoaverse.Utils
{
    public class DataAccessLayer
    {
        private static readonly whoaverseEntities Db = new whoaverseEntities();

        #region sfw submissions from all subverses
        public static IQueryable<Message> SfwSubmissionsFromAllSubversesByDate()
        {
            IQueryable<Message> sfwSubmissionsFromAllSubversesByDate = (from message in Db.Messages
                                                                        join subverse in Db.Subverses on message.Subverse equals subverse.name
                                                                        where message.Name != "deleted" && subverse.private_subverse != true && subverse.rated_adult == false
                                                                        select message
                                                                        ).AsQueryable().OrderByDescending(s => s.Date);

            return sfwSubmissionsFromAllSubversesByDate;
        }

        public static IQueryable<Message> SfwSubmissionsFromAllSubversesByRank()
        {
            IQueryable<Message> sfwSubmissionsFromAllSubversesByRank = (from message in Db.Messages
                                                                        join subverse in Db.Subverses on message.Subverse equals subverse.name
                                                                        where message.Name != "deleted" && subverse.private_subverse != true && subverse.rated_adult == false && message.Rank > 0.00009
                                                                        select message).AsQueryable().OrderByDescending(s => s.Rank);

            return sfwSubmissionsFromAllSubversesByRank;
        }

        public static IQueryable<Message> SfwSubmissionsFromAllSubversesByTop()
        {
            IQueryable<Message> sfwSubmissionsFromAllSubversesByTop = (from message in Db.Messages
                                                                       join subverse in Db.Subverses on message.Subverse equals subverse.name
                                                                       where message.Name != "deleted" && subverse.private_subverse != true && subverse.rated_adult == false
                                                                       select message).AsQueryable().OrderByDescending(s => s.Likes - s.Dislikes);


            return sfwSubmissionsFromAllSubversesByTop;
        }
        #endregion

        #region unfiltered submissions from all subverses
        public static IQueryable<Message> SubmissionsFromAllSubversesByDate()
        {
            IQueryable<Message> submissionsFromAllSubversesByDate = (from message in Db.Messages
                                                                     join subverse in Db.Subverses on message.Subverse equals subverse.name
                                                                     where message.Name != "deleted" && subverse.private_subverse != true
                                                                     select message).AsQueryable().OrderByDescending(s => s.Date);

            return submissionsFromAllSubversesByDate;
        }

        public static IQueryable<Message> SubmissionsFromAllSubversesByRank()
        {
            IQueryable<Message> submissionsFromAllSubversesByRank = (from message in Db.Messages
                                                                     join subverse in Db.Subverses on message.Subverse equals subverse.name
                                                                     where message.Name != "deleted" && subverse.private_subverse != true && message.Rank > 0.00009
                                                                     select message).AsQueryable().OrderByDescending(s => s.Rank);

            return submissionsFromAllSubversesByRank;
        }

        public static IQueryable<Message> SubmissionsFromAllSubversesByTop()
        {
            IQueryable<Message> submissionsFromAllSubversesByTop = (from message in Db.Messages
                                                                    join subverse in Db.Subverses on message.Subverse equals subverse.name
                                                                    where message.Name != "deleted" && subverse.private_subverse != true
                                                                    select message).AsQueryable().OrderByDescending(s => s.Likes - s.Dislikes);

            return submissionsFromAllSubversesByTop;
        }
        #endregion

        #region submissions from a single subverse
        public static IQueryable<Message> SubmissionsFromASubverseByDate(string subverseName)
        {
            IQueryable<Message> submissionsFromASubverseByDate = (from message in Db.Messages
                                                                  join subverse in Db.Subverses on message.Subverse equals subverse.name
                                                                  where message.Name != "deleted" && message.Subverse == subverseName
                                                                  select message).AsQueryable().OrderByDescending(s => s.Date);

            return submissionsFromASubverseByDate;
        }

        public static IQueryable<Message> SubmissionsFromASubverseByRank(string subverseName)
        {
            IQueryable<Message> submissionsFromASubverseByRank = (from message in Db.Messages
                                                                  join subverse in Db.Subverses on message.Subverse equals subverse.name
                                                                  where message.Name != "deleted" && message.Subverse == subverseName && message.Rank > 0.00009
                                                                  select message).AsQueryable().OrderByDescending(s => s.Rank);

            return submissionsFromASubverseByRank;
        }

        public static IQueryable<Message> SubmissionsFromASubverseByTop(string subverseName)
        {
            IQueryable<Message> submissionsFromASubverseByTop = (from message in Db.Messages
                                                                 join subverse in Db.Subverses on message.Subverse equals subverse.name
                                                                 where message.Name != "deleted" && message.Subverse == subverseName
                                                                 select message).AsQueryable().OrderByDescending(s => s.Likes - s.Dislikes);

            return submissionsFromASubverseByTop;
        }
        #endregion
    }
}