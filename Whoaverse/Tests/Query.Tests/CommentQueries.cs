namespace Query.Tests
{
    using System;
    using System.Threading.Tasks;
    using Voat.Models;
    using Voat.Queries.Comments;
    using Xunit;

    [Trait("Category", "Semi-integration"), Trait("Subcategory", "Queries")]
    public class CommentQueries : IDisposable
    {
         private readonly whoaverseEntities dbContext;

        public CommentQueries()
        {
            var effortConnection = Effort.EntityConnectionFactory.CreateTransient(EntityString.Value);
            dbContext = new whoaverseEntities(effortConnection);

            dbContext.Set<Subverse>().Add(new Subverse
            {
                title = "test",
                name = "test"
            });

            var submissions = new[]
            {
                dbContext.Set<Message>().Add(new Message
                {
                    Name = "test message",
                    Subverse = "test",
                    Id = 1

                }),
                dbContext.Set<Message>().Add(new Message
                {
                    Name = "another test",
                    Subverse = "test",
                    Id = 2
                })
            };

            dbContext.SaveChanges();

            dbContext.Set<Comment>().Add(new Comment
            {
                Id = 1,
                Message = submissions[0],
                Name = "test2",
                CommentContent = "Empty"
            });

            dbContext.Set<Comment>().Add(new Comment
            {
                Id = 2,
                Message = submissions[0],
                Name = "johnny",
                CommentContent = "Empty"
            });

            dbContext.Set<Comment>().Add(new Comment
            {
                Id = 3,
                Message = submissions[1],
                Name = "test",
                CommentContent = "Empty"
            });
            dbContext.SaveChanges();
        }

        [Theory(DisplayName = "Comment count should be correctly retrieved for submission.")]
        [InlineData(1, 2)]
        [InlineData(2, 1)]
        [InlineData(4, 0)]
        public async Task CommentCountRetrieval(int submissionId, int expectedCount)
        {
            var result = await dbContext.Set<Comment>().GetCommentCountAsync(submissionId);

            Assert.Equal(expectedCount, result);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }
    }
}
