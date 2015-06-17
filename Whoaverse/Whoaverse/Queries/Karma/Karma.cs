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

namespace Voat.Queries.Karma
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;

    public static class KarmaQueries
    {
        private static Task<int> GetKarmaAsync(this IQueryable<IKarmaTracked> messages)
        {
            return messages.Select(c => c.Likes - c.Dislikes).DefaultIfEmpty(0).SumAsync();
        }

        /// <summary>
        /// Gets link contribution points for specified user asynchronously
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
        /// Gets comment contribution points for specified user asynchronously
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
}

