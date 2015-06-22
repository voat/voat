namespace Query.Tests
{
    using System;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using Voat.Models;
    using Voat.Queries.Voting;
    using Xunit;

    [Trait("Category", "Semi-integration"), Trait("Subcategory", "Queries")]
    public class VoteQueries : IDisposable
    {
        private readonly DbContext dbContext;

        public VoteQueries()
        {
            var effortConnection = Effort.EntityConnectionFactory.CreateTransient(EntityString.Value);
            dbContext = new whoaverseEntities(effortConnection);
            #region Comment vote set up
            dbContext.Set<Subverse>().Add(new Subverse
            {
                title = "test",
                name = "test"
            });

            dbContext.Set<Comment>().Add(new Comment
            {
                Id = 1,
                CommentContent = "Empty",
                Name = "abc"
            });

            dbContext.Set<Comment>().Add(new Comment
            {
                Id = 2,
                CommentContent = "Empty",
                Name = "abc"
            });

            dbContext.Set<Comment>().Add(new Comment
            {
                Id = 3,
                CommentContent = "Empty",
                Name = "abc"
            });

            dbContext.Set<Commentvotingtracker>().Add(new Commentvotingtracker
            {
                Id = 1,
                UserName = "testUser",
                CommentId = 1,
                VoteStatus = 1
            });

            dbContext.Set<Commentvotingtracker>().Add(new Commentvotingtracker
            {
                Id = 2,
                UserName = "testUser",
                CommentId = 2,
                VoteStatus = 0
            });

            dbContext.Set<Commentvotingtracker>().Add(new Commentvotingtracker
            {
                Id = 3,
                UserName = "testUser2",
                VoteStatus = 1,
                CommentId = 2
            });

            dbContext.Set<Commentvotingtracker>().Add(new Commentvotingtracker
            {
                Id = 4,
                UserName = "testUser2",
                VoteStatus = -1,
                CommentId = 1
            });
            #endregion

            #region Submission vote setup
            dbContext.Set<Message>().Add(new Message
            {
                Id = 1,
                Name = "abc",
                Title = "test",
                Subverse = "test"
            });

            dbContext.Set<Message>().Add(new Message
            {
                Id = 2,
                Name = "abc",
                Title = "test",
                Subverse = "test"
            });

            dbContext.Set<Message>().Add(new Message
            {
                Id = 3,
                Name = "abc",
                Title = "test",
                Subverse = "test"
            });

            dbContext.Set<Votingtracker>().Add(new Votingtracker
            {
                Id = 1,
                UserName = "testUser",
                MessageId = 1,
                VoteStatus = 1
            });

            dbContext.Set<Votingtracker>().Add(new Votingtracker
            {
                Id = 2,
                UserName = "testUser",
                MessageId = 2,
                VoteStatus = 0
            });

            dbContext.Set<Votingtracker>().Add(new Votingtracker
            {
                Id = 3,
                UserName = "testUser2",
                VoteStatus = 1,
                MessageId = 2
            });

            dbContext.Set<Votingtracker>().Add(new Votingtracker
            {
                Id = 4,
                UserName = "testUser2",
                VoteStatus = -1,
                MessageId = 1
            });
            #endregion

            dbContext.SaveChanges();
        }


        [Theory(DisplayName = "Query to check for current vote should return correct vote status.")]
        [InlineData("testUser", 1, VoteStatus.Upvoted)]
        [InlineData("testUser", 2, VoteStatus.None)]
        [InlineData("testUser2", 2, VoteStatus.Upvoted)]
        [InlineData("testUser2", 1, VoteStatus.Downvoted)]
        [InlineData("testUser", 3, VoteStatus.None)]
        [InlineData("testUser2", 3, VoteStatus.None)]
        public async Task CheckingSubmissionForUserVote(string userName, int commentId,
            VoteStatus expectedStatus)
        {
            var result = await dbContext.Set<Votingtracker>().CheckSubmissionForVoteAsync(userName, commentId);
            Assert.Equal(expectedStatus, result);
        }

        [Theory(DisplayName = "Query to check for current vote should return correct vote status.")]
        [InlineData("testUser", 1, VoteStatus.Upvoted)]
        [InlineData("testUser", 2, VoteStatus.None)]
        [InlineData("testUser2", 2, VoteStatus.Upvoted)]
        [InlineData("testUser2", 1, VoteStatus.Downvoted)]
        [InlineData("testUser", 3, VoteStatus.None)]
        [InlineData("testUser2", 3, VoteStatus.None)]
        public async Task CheckingCommentForUserVote(string userName, int commentId,
            VoteStatus expectedStatus)
        {
            var result = await dbContext.Set<Commentvotingtracker>().CheckCommentForVoteAsync(userName, commentId);
            Assert.Equal(expectedStatus, result);
        }


        public void Dispose()
        {
            dbContext.Dispose();
        }
    }
}
