namespace Command.Tests.Saving
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Query.Tests;
    using Voat.Commands.Saving;
    using Voat.Models;
    using Xunit;

    [Trait("Category", "Semi-integration"), Trait("Subcategory", "Commands")]
    public class SavingTests : IDisposable
    {
        private readonly DbContext dbContext;

        public SavingTests()
        {
            var effortConnection = Effort.EntityConnectionFactory.CreateTransient(EntityString.Value);
            dbContext = new whoaverseEntities(effortConnection);

            var submission = dbContext.Set<Message>().Add(new Message
            {
                Id = 1,
                Name = "jane",
                Title = "test1"
            });

            dbContext.Set<Message>().Add(new Message
            {
                Id = 2,
                Name = "john",
                Title = "test2"
            });

            var firstComment = dbContext.Set<Comment>().Add(new Comment
            {
                Id = 1,
                Name = "user1",
                CommentContent = "Empty",
                Message = submission
            });

            var secondComment = dbContext.Set<Comment>().Add(new Comment
            {
                Id = 2,
                Name = "jane",
                CommentContent = "Empty",
                Message = submission
            });

            dbContext.Set<Commentsavingtracker>().Add(new Commentsavingtracker
            {
                Id = 1,
                Comment = firstComment,
                UserName = "test"
            });

            dbContext.Set<Commentsavingtracker>().Add(new Commentsavingtracker
            {
                Id = 2,
                Comment = secondComment,
                UserName = "john"
            });

            dbContext.Set<Savingtracker>().Add(new Savingtracker
            {
                Id = 1,
                Message = submission,
                UserName = "john"
            });

            dbContext.SaveChanges();
        }

        [Theory(DisplayName = "Comment should be saved if it isn't saved already.")]
        [InlineData(1, "johnny")]
        [InlineData(1, "john")]
        [InlineData(1, "jane")]
        public async Task SavingComment(int commentId, string userName)
        {
            await dbContext.ToggleCommentSaveAsync(commentId, userName);

            var result =
                await
                    dbContext.Set<Commentsavingtracker>()
                        .Where(c => c.CommentId == commentId && c.UserName == userName)
                        .SingleOrDefaultAsync();

            Assert.NotNull(result);

            Assert.Equal(commentId, result.CommentId);
            Assert.Equal(userName, result.UserName);
        }

        [Fact(DisplayName = "If the comment has already been saved, toggling it will unsave it.")]
        public async Task ToggleUnsaveComment()
        {
            var data =
                await
                    dbContext.Set<Commentsavingtracker>()
                        .Where(c => c.CommentId == 2 && c.UserName == "john")
                        .SingleOrDefaultAsync();
            Assert.NotNull(data);

            await dbContext.ToggleCommentSaveAsync(2, "john");

            data =
                await
                    dbContext.Set<Commentsavingtracker>()
                        .Where(c => c.CommentId == 2 && c.UserName == "john")
                        .SingleOrDefaultAsync();
            Assert.Null(data);
        }

        [Theory(DisplayName = "Submission should be saved if it isn't saved already.")]
        [InlineData(1, "johnny")]
        [InlineData(2, "user")]
        [InlineData(2, "jane")]
        public async Task SavingSubmission(int submissionId, string userName)
        {
            await dbContext.ToggleSubmissionSaveAsync(submissionId, userName);

            var result =
                await
                    dbContext.Set<Savingtracker>()
                        .Where(c => c.MessageId == submissionId && c.UserName == userName)
                        .SingleOrDefaultAsync();

            Assert.NotNull(result);

            Assert.Equal(submissionId, result.MessageId);
            Assert.Equal(userName, result.UserName);
        }

        [Fact(DisplayName = "If the submission has already been saved, toggling it will unsave it.")]
        public async Task ToggleUnsaveSubmission()
        {
            var data =
                await
                    dbContext.Set<Savingtracker>()
                        .Where(c => c.MessageId == 1 && c.UserName == "john")
                        .SingleOrDefaultAsync();
            Assert.NotNull(data);

            await dbContext.ToggleSubmissionSaveAsync(1, "john");

            data =
                await
                    dbContext.Set<Savingtracker>()
                        .Where(c => c.MessageId == 1 && c.UserName == "john")
                        .SingleOrDefaultAsync();
            Assert.Null(data);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }
    }
}
