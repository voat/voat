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
    using System.Data.Entity;
    using System.Threading.Tasks;

    public static class KarmaRevamped
    {
        private static Task<int> GetKarmaAsync(this IQueryable<IKarmaTracked> messages)
        {
            return messages.Select(c => c.Likes - c.Dislikes).DefaultIfEmpty(0).SumAsync();
        }

        /// <summary>
        /// Gets link contribution points for specified user
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userName"></param>
        /// <param name="subverse">Subverse name for which the link karma should be retrieved</param>
        /// <returns></returns>
        public static Task<int> GetLinkKarmaAsync(this DbContext context, string userName, string subverse = null)
        {
            var baseQuery =
                context.Set<Message>()
                    .Where(c => c.Name.Trim().Equals(userName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(subverse))
            {
                baseQuery = baseQuery.Where(c => c.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase));
            }

            return baseQuery.GetKarmaAsync();
        }

        /// <summary>
        /// Gets comment contribution points for specified user
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userName"></param>
        /// <param name="subverse"></param>
        /// <returns></returns>
        public static Task<int> GetCommentKarmaAsync(this DbContext context, string userName, string subverse = null)
        {
            var baseQuery = context.Set<Comment>().Where(c => c.Name != "deleted" && c.Name == userName);

            if (!string.IsNullOrEmpty(subverse))
            {
                baseQuery = baseQuery.Where(c => c.Message.Subverse == subverse);
            }

            return baseQuery.GetKarmaAsync();
        }

        private static IQueryable<IVoteTracked> CreateUpvotedEntriesQuery(this IQueryable<IVoteTracked> source, string userName)
        {
            return source.Where(a => a.UserName == userName && a.VoteStatus == 1);
        }

        /// <summary>
        /// Gets the total number of upvotes given by a user
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static Task<int> GetUpvotesGivenAsync(this DbContext context, string userName)
        {
            var comments = context.Set<Commentvotingtracker>().CreateUpvotedEntriesQuery(userName);
            var submissionUpvotes = context.Set<Votingtracker>().CreateUpvotedEntriesQuery(userName);

            // Concat is the equivalent of UNION ALL
            return comments.Concat(submissionUpvotes).CountAsync();
        }
    }

    public static class Karma
    {
        // get link contribution points for a user
        public static int LinkKarma(string userName)
        {
            using (var db = new whoaverseEntities())
            {

                try
                {
                    return db.Messages.Where(c => c.Name.Trim().Equals(userName, StringComparison.OrdinalIgnoreCase))
                        .Select(c => c.Likes - c.Dislikes)
                        .Sum();
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
                    return db.Messages.Where(c => c.Name.Trim().Equals(userName, StringComparison.OrdinalIgnoreCase) && c.Subverse.Equals(subverseName, StringComparison.OrdinalIgnoreCase))
                        .Select(c => c.Likes - c.Dislikes)
                        .Sum();
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
                    return db.Comments.Where(c => c.Name.Trim().Equals(userName, StringComparison.OrdinalIgnoreCase))
                        .Select(c => c.Likes - c.Dislikes)
                        .Sum();
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
                    return db.Comments.Join(db.Messages, comment => comment.MessageId, message => message.Id, (comment, message) => new {comment, message})
                        .Where(
                            x =>
                                x.comment.Name != "deleted" && x.comment.Name == userName &&
                                x.message.Subverse == subverseName)
                        .Select(x => x.comment.Likes - x.comment.Dislikes)
                        .Sum();
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

