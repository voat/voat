namespace Query.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using Voat.Models;
    using Voat.Queries.Karma;
    using Xunit;

    [Trait("Category", "Semi-integration"), Trait("Subcategory", "Queries")]
    public class KarmaQueries : IDisposable
    {
        private readonly DbContext dbContext;

        public KarmaQueries()
        {
            var effortConnection = Effort.EntityConnectionFactory.CreateTransient(EntityString.Value);
            dbContext = new whoaverseEntities(effortConnection);

            dbContext.Set<Subverse>().Add(new Subverse
            {
                name = "subverse1",
                title = "subverse1"
            });

            dbContext.Set<Subverse>().Add(new Subverse
            {
                name = "subverse2",
                title = "subverse2"
            });

            dbContext.Set<Message>().Add(new Message
            {
                Id = 1,
                Name = "testName",
                Subverse = "subverse1",
                Date = new DateTime(2015, 4, 20),
                Likes = 22,
                Dislikes = 10
            });

            dbContext.Set<Message>().Add(new Message
            {
                Id = 2,
                Name = "testName",
                Subverse = "subverse2",
                Date = new DateTime(2015, 4, 22),
                Likes = 32,
                Dislikes = 3
            });

            dbContext.Set<Message>().Add(new Message
            {
                Id = 3,
                Name = "testName2",
                Subverse = "subverse1",
                Date = new DateTime(2015, 5, 2),
                Likes = 4,
                Dislikes = 10
            });

            dbContext.Set<Comment>().Add(new Comment
            {
                Id = 1,
                Name = "testName",
                Likes = 23,
                Dislikes = 22,
                MessageId = 3,
                CommentContent = "Empty"
            });

            dbContext.Set<Comment>().Add(new Comment
            {
                Id = 2,
                Name = "testName",
                Likes = 40,
                Dislikes = 2,
                MessageId = 2,
                CommentContent = "Empty"
            });

            dbContext.Set<Comment>().Add(new Comment
            {
                Id = 3,
                Name = "testName2",
                Likes = 3,
                Dislikes = 15,
                MessageId = 1,
                CommentContent = "Empty"
            });

            dbContext.Set<Commentvotingtracker>().Add(new Commentvotingtracker
            {
                CommentId = 3,
                UserName = "testName2",
                VoteStatus = 1
            });

            dbContext.Set<Commentvotingtracker>().Add(new Commentvotingtracker
            {
                CommentId = 3,
                UserName = "testName2",
                VoteStatus = 0
            });

            dbContext.Set<Commentvotingtracker>().Add(new Commentvotingtracker
            {
                CommentId = 1,
                UserName = "testName",
                VoteStatus = 1
            });

            dbContext.Set<Commentvotingtracker>().Add(new Commentvotingtracker
            {
                CommentId = 2,
                UserName = "testName",
                VoteStatus = 1
            });

            dbContext.Set<Commentvotingtracker>().Add(new Commentvotingtracker
            {
                CommentId = 3,
                UserName = "testName",
                VoteStatus = 0
            });

            dbContext.Set<Votingtracker>().Add(new Votingtracker
            {
                MessageId = 1,
                UserName = "testName",
                VoteStatus = 1
            });

            dbContext.Set<Votingtracker>().Add(new Votingtracker
            {
                MessageId = 2,
                UserName = "testName",
                VoteStatus = 0
            });

            // Setting up the views
            dbContext.Set<TotalKarma>().Add(new TotalKarma
            {
                UserName = "testName",
                Subverse = "subverse1",
                LinkKarma = 22-10,
                CommentKarma = 23-22
            });

            dbContext.Set<TotalKarma>().Add(new TotalKarma
            {
                UserName = "testName",
                Subverse = "subverse2",
                LinkKarma = 32-3,
                CommentKarma = 40-2
            });

            dbContext.SaveChanges();
        }

        [Theory(DisplayName = "Link karma should be properly retrieved from all subverses for given user")]
        [InlineData("testName", (22 - 10) + (32 - 3))]
        [InlineData("testName2", 4 - 10)]
        public async Task LinkKarmaRetrievalForUser(string userName, int expectedKarma)
        {
            var result = await dbContext.Set<Message>().GetLinkKarmaAsync(userName);
            Assert.Equal(expectedKarma, result);
        }

        [Theory(DisplayName = "Link karma should be properly retrieved for selected subverse and given user")]
        [InlineData("testName", "subverse1", 22 - 10)]
        [InlineData("testName", "subverse2", 32 - 3)]
        [InlineData("testName2", "subverse2", 0)]
        [InlineData("testName2", "subverse1", 4 - 10)]
        public async Task LinkKarmaRetrievalForUserAndSubverse(string userName, string subverse, int expectedKarma)
        {
            var result = await dbContext.Set<Message>().GetLinkKarmaAsync(userName, subverse);
            Assert.Equal(expectedKarma, result);
        }

        [Theory(DisplayName = "Comment karma should be properly retrieved from all subverses for given user")]
        [InlineData("testName", (23 - 22) + (40 - 2))]
        [InlineData("testName2", (3 - 15))]
        public async Task CommentKarmaRetrievalForUser(string userName, int expectedKarma)
        {
            var result = await dbContext.Set<Comment>().GetCommentKarmaAsync(userName);
            Assert.Equal(expectedKarma, result);
        }

        [Theory(DisplayName = "Link karma should be properly retrieved for selected subverse and given user")]
        [InlineData("testName", "subverse1", 23 - 22)]
        [InlineData("testName", "subverse2", 40 - 2)]
        [InlineData("testName2", "subverse2", 0)]
        [InlineData("testName2", "subverse1", 3 - 15)]
        public async Task CommentKarmaRetrievalForUserAndSubverse(string userName, string subverse, int expectedKarma)
        {
            var result = await dbContext.Set<Comment>().GetCommentKarmaAsync(userName, subverse);
            Assert.Equal(expectedKarma, result);
        }

        [Theory(DisplayName = "Total upvotes given should be properly counted for a given user")]
        [InlineData("testName", 3)]
        [InlineData("testName2", 1)]
        public async Task TotalUpvotesGivenRetrieval(string userName, int expectedUpvotes)
        {
            var result = await dbContext.GetUpvotesGivenAsync(userName);
            Assert.Equal(expectedUpvotes, result);
        }

        public static IEnumerable<object[]> CombinedKarmaData
        {
            get
            {
                yield return new object[] {"testName", "subverse1", new CombinedKarma(22-10, 23-22) };
                yield return
                    new object[] {"testName", null, new CombinedKarma((22 - 10) + (32 - 3), (23 - 22) + (40 - 2))};
            }
        }

        [Theory(DisplayName = "Combined link & comment karma should be properly retrieved for a given user and (optionally) subverse.")]
        [MemberData("CombinedKarmaData")]
        public async Task CombinedKarmaRetrieval(string userName, string subverse, CombinedKarma expectedResult)
        {
            var result = await dbContext.Set<TotalKarma>().GetCombinedKarmaAsync(userName, subverse);

            Assert.Equal(expectedResult, result);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }
    }
}
