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
        private static IQueryable<int> GetRawKarmaQuery(this IQueryable<IKarmaTracked> messages)
        {
            return messages.Select(c => c.Likes - c.Dislikes).DefaultIfEmpty(0);
        }

        private static IQueryable<IKarmaTracked> PrepareLinkKarmaQuery(this IQueryable<Message> query, string userName,
            string subverse)
        {
            var baseQuery =
               query
                   .Where(c => c.Name.Trim().Equals(userName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(subverse))
            {
                baseQuery = baseQuery.Where(c => c.Subverse.Equals(subverse, StringComparison.OrdinalIgnoreCase));
            }

            return baseQuery;
        }

        private static IQueryable<IKarmaTracked> PrepareCommentKarmaQuery(this IQueryable<Comment> query,
            string userName, string subverse)
        {
            var baseQuery = query.Where(c => c.Name != "deleted" && c.Name == userName);

            if (!string.IsNullOrEmpty(subverse))
            {
                baseQuery = baseQuery.Where(c => c.Message.Subverse == subverse);
            }

            return baseQuery;
        }

        public static async Task<CombinedKarma> GetCombinedKarmaAsync(this IQueryable<TotalKarma> query, string userName,
            string subverse = null)
        {
            var karma = query.Where(x => x.UserName == userName);
            if (!string.IsNullOrEmpty(subverse))
            {
                karma = karma.Where(x => x.Subverse == subverse);
            }

            var result =
                await
                    karma.GroupBy(x => x.UserName)
                        .Select(
                            g => new
                            {
                                LinkKarma = g.Sum(x => x.LinkKarma),
                                CommentKarma = g.Sum(x => x.CommentKarma)
                            })
                        .FirstAsync()
                        .ConfigureAwait(false);
            return new CombinedKarma(result.LinkKarma ?? 0, result.CommentKarma ?? 0);
        }

        /// <summary>
        /// Gets link contribution points for specified user asynchronously
        /// </summary>
        /// <param name="query"></param>
        /// <param name="userName"></param>
        /// <param name="subverse">Subverse name for which the link karma should be retrieved</param>
        /// <returns></returns>
        public static Task<int> GetLinkKarmaAsync(this IQueryable<Message> query, string userName, string subverse = null)
        {
            return query.PrepareLinkKarmaQuery(userName, subverse).GetRawKarmaQuery().SumAsync();
        }

        /// <summary>
        /// Gets comment contribution points for specified user asynchronously
        /// </summary>
        /// <param name="query"></param>
        /// <param name="userName"></param>
        /// <param name="subverse"></param>
        /// <returns></returns>
        public static Task<int> GetCommentKarmaAsync(this IQueryable<Comment> query, string userName, string subverse = null)
        {
            return query.PrepareCommentKarmaQuery(userName, subverse).GetRawKarmaQuery().SumAsync();
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
            return
                comments.Select(x => new { x.UserName, x.VoteStatus })
                    .Concat(submissionUpvotes.Select(x => new { x.UserName, x.VoteStatus }))
                    .CountAsync();
        }
    }
}

