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
using System.Linq;
using Voat.Models;

namespace Voat.Utils
{
    public static class Karma
    {
        // get link contribution points for a user
        public static int LinkKarma(string userName)
        {
            using (var db = new whoaverseEntities())
            {

                try
                {
                    var likes = db.Messages
                                                .Where(r => r.Name.Equals(userName, StringComparison.OrdinalIgnoreCase))
                                                .Sum(r => (int)r.Likes);

                    var dislikes = db.Messages
                                        .Where(r => r.Name.Equals(userName, StringComparison.OrdinalIgnoreCase))
                                        .Sum(r => (int)r.Dislikes);

                    return likes - dislikes;
                }
                catch (Exception)
                {
                    return 0;
                }

            }
        }

        // get link contribution points for a user from a given subverse
        public static int LinkKarmaForSubverse(string userName, string subverseName)
        {
            using (var db = new whoaverseEntities())
            {
                try
                {
                    var likes = db.Messages
                                                .Where(r => r.Name.Equals(userName, StringComparison.OrdinalIgnoreCase) && r.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase))
                                                .Sum(r => (int)r.Likes);


                    var dislikes = db.Messages
                                        .Where(r => r.Name.Equals(userName, StringComparison.OrdinalIgnoreCase) && r.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase))
                                        .Sum(r => (int)r.Dislikes);

                    return likes - dislikes;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        // get comment contribution points for a user
        public static int CommentKarma(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                try
                {
                    var sumOfLikes = db.Comments
                                               .Where(r => r.Name.Trim().Equals(userName, StringComparison.OrdinalIgnoreCase))
                                               .Sum(r => (int)r.Likes);

                    var sumOfdislikes = db.Comments
                                        .Where(r => r.Name.Trim().Equals(userName, StringComparison.OrdinalIgnoreCase))
                                        .Sum(r => (int)r.Dislikes);

                    return sumOfLikes - sumOfdislikes;
                }
                catch (Exception)
                {

                    return 0;
                }

            }
        }

        // get comment contribution points for a user from a given subverse
        public static int CommentKarmaForSubverse(string userName, string subverseName)
        {
            using (var db = new whoaverseEntities())
            {
                try
                {
                    var sumOfLikes = (from comment in db.Comments
                                      join message in db.Messages on comment.MessageId equals message.Id
                                      where comment.Name != "deleted" && comment.Name == userName && message.Subverse == subverseName
                                      select comment)
                                       .Sum(r => r.Likes);

                    var sumOfDislikes = (from comment in db.Comments
                                         join message in db.Messages on comment.MessageId equals message.Id
                                         where comment.Name != "deleted" && comment.Name == userName && message.Subverse == subverseName
                                         select comment)
                                       .Sum(r => r.Dislikes);

                    return sumOfLikes - sumOfDislikes;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        // get total upvotes given by a user
        public static int UpvotesGiven(string userName)
        {
            using (var db = new whoaverseEntities())
            {
                try
                {
                    var submissionUpvotes = db.Votingtrackers.Count(a => a.UserName == userName && a.VoteStatus == 1);
                    var commentUpvotes = db.Commentvotingtrackers.Count(a => a.UserName == userName && a.VoteStatus == 1);

                    return submissionUpvotes + commentUpvotes;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
    }

}

