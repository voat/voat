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

using System.Data.Entity;
using System.Linq;
using Voat.Models;

namespace Voat.Utils
{
    public static class SetsUtility
    {
        public static IQueryable<SetSubmission> TopRankedSubmissionsFromASub(string subverseName, DbSet<Message> messagesDbSet, string setName, int desiredResults)
        {
            var topRankedSubmissions = (from message in messagesDbSet
                                        where message.Name != "deleted" && message.Subverse == subverseName
                                        select new SetSubmission
                                        {
                                            Id = message.Id,
                                            Votes = message.Votes,
                                            Name = message.Name,
                                            Date = message.Date,
                                            Type = message.Type,
                                            Linkdescription = message.Linkdescription,
                                            Title = message.Title,
                                            Rank = message.Rank,
                                            MessageContent = message.MessageContent,
                                            Subverse = message.Subverse,
                                            Likes = message.Likes,
                                            Dislikes = message.Dislikes,
                                            Thumbnail = message.Thumbnail,
                                            LastEditDate = message.LastEditDate,
                                            FlairLabel = message.FlairLabel,
                                            FlairCss = message.FlairCss,
                                            Anonymized = message.Anonymized,
                                            Views = message.Views,
                                            Comments = message.Comments,
                                            Votingtrackers = message.Votingtrackers,
                                            Subverses = message.Subverses,
                                            Stickiedsubmission = message.Stickiedsubmission,
                                            Viewstatistics = message.Viewstatistics,
                                            ParentSet = setName
                                        }).OrderByDescending(s => s.Rank).Take(desiredResults);

            return topRankedSubmissions;
        }

        public static IQueryable<SetSubmission> NewestSubmissionsFromASub(string subverseName, DbSet<Message> messagesDbSet, string setName, int desiredResults)
        {
            var topRankedSubmissions = (from message in messagesDbSet
                                        where message.Name != "deleted" && message.Subverse == subverseName
                                        select new SetSubmission
                                        {
                                            Id = message.Id,
                                            Votes = message.Votes,
                                            Name = message.Name,
                                            Date = message.Date,
                                            Type = message.Type,
                                            Linkdescription = message.Linkdescription,
                                            Title = message.Title,
                                            Rank = message.Rank,
                                            MessageContent = message.MessageContent,
                                            Subverse = message.Subverse,
                                            Likes = message.Likes,
                                            Dislikes = message.Dislikes,
                                            Thumbnail = message.Thumbnail,
                                            LastEditDate = message.LastEditDate,
                                            FlairLabel = message.FlairLabel,
                                            FlairCss = message.FlairCss,
                                            Anonymized = message.Anonymized,
                                            Views = message.Views,
                                            Comments = message.Comments,
                                            Votingtrackers = message.Votingtrackers,
                                            Subverses = message.Subverses,
                                            Stickiedsubmission = message.Stickiedsubmission,
                                            Viewstatistics = message.Viewstatistics,
                                            ParentSet = setName
                                        }).OrderByDescending(s => s.Date).Take(desiredResults);

            return topRankedSubmissions;
        }
    }
}