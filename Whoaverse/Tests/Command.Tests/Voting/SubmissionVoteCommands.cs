namespace Command.Tests.Voting
{
    using System;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using Voat.Commands.Voting;
    using Voat.Models;
    using Xunit;

    [Trait("Category", "Semi-integration"), Trait("Subcategory", "Commands")]
    public class SubmissionVoteCommands : IDisposable
    {
        private readonly DbContext dbContext;

        public SubmissionVoteCommands()
        {
            var effortConnection = Effort.EntityConnectionFactory.CreateTransient(EntityString.Value);
            dbContext = new whoaverseEntities(effortConnection);

            dbContext.Set<Subverse>().Add(new Subverse
            {
                title = "test",
                name = "test"
            });

            dbContext.SaveChanges();

            dbContext.Set<Message>().Add(new Message
            {
                Id = 1,
                Title = "Empty",
                Name = "testuser",
            });

            var secondMessage = dbContext.Set<Message>().Add(new Message
            {
                Id = 2,
                Title = "Empty",
                Name = "testuser2",
                Likes = 10,
                Dislikes = 5,
            });

            dbContext.Set<Message>().Add(new Message
            {
                Id = 3,
                Title = "Empty",
                Name = "abc",
                Anonymized = true
            });

            dbContext.Set<Message>().Add(new Message
            {
                Id = 4,
                Name = "abc",
                Likes = 5,
                Title = "Empty"
            });

            var fifthMessage = dbContext.Set<Message>().Add(new Message
            {
                Id = 5,
                Name = "abc",
                Likes = 5,
                Title = "Empty"
            });

            dbContext.Set<Message>().Add(new Message
            {
                Id = 6,
                Name = "abc",
                Likes = 5,
                Title = "Empty"
            });

            dbContext.SaveChanges();

            dbContext.Set<Votingtracker>().Add(new Votingtracker
            {
                Id = 1,
                Message = fifthMessage,
                ClientIpAddress = "hashedAddress",
                VoteStatus = 1,
                UserName = "yetAnotherUser"
            });

            dbContext.Set<Votingtracker>().Add(new Votingtracker
            {
                Id = 2,
                Message = secondMessage,
                ClientIpAddress = "hashedAddress",
                VoteStatus = -1,
                UserName = "yetAnotherUser"
            });

            dbContext.SaveChanges();
        }

        [Fact(DisplayName = "Users cannot upvote their own submission")]
        public async Task UsersCannotUpvoteTheirOwnComments()
        {
            var result = await dbContext.UpvoteSubmissionAsync(1, "testuser", "hash");
            Assert.Equal(VoteDirection.None, result);
        }

        [Fact(DisplayName = "If the submission is anonymous, it should not be available for upvote.")]
        public async Task CannotUpvoteIfAnonymous()
        {
            var result = await dbContext.UpvoteSubmissionAsync(3, "testuser", "hash");
            Assert.Equal(VoteDirection.None, result);
        }

        [Fact(DisplayName = "Users cannot upvote the same submission from different accounts from the same IP.")]
        public async Task CannotUpvoteTwiceFromSameIpAddress()
        {
            var result = await dbContext.UpvoteSubmissionAsync(5, "testuser", "hashedAddress");
            Assert.Equal(VoteDirection.None, result);
        }

        [Fact(DisplayName = "Initial vote on someone else's non-anonymous submission should pass.")]
        public async Task RegularFirstUpvote()
        {
            var result = await dbContext.UpvoteSubmissionAsync(4, "testuser2", "otherAddress");
            Assert.Equal(VoteDirection.Upvote, result);
        }

        [Fact(DisplayName = "Initial vote should add message tracking entry")]
        public async Task FirstUpvoteShouldAddTrackingEntry()
        {
            var trackingData =
                await
                    dbContext.Set<Votingtracker>()
                        .FirstOrDefaultAsync(cvt => cvt.MessageId == 4 && cvt.UserName == "testuser2");

            Assert.Null(trackingData);

            var result = await dbContext.UpvoteSubmissionAsync(4, "testuser2", "otherAddress");
            Assert.Equal(VoteDirection.Upvote, result);
            trackingData = await dbContext.Set<Votingtracker>().FirstOrDefaultAsync(cvt => cvt.MessageId == 4 && cvt.UserName == "testuser2");

            Assert.NotNull(trackingData);
            Assert.Equal(1, trackingData.VoteStatus);
            Assert.Equal(4, trackingData.MessageId);
            Assert.Equal("testuser2", trackingData.UserName);
            Assert.Equal("otherAddress", trackingData.ClientIpAddress);
        }

        [Fact(DisplayName = "Removing upvote should decrement likes.")]
        public async Task RemovingUpvote()
        {
            var result = await dbContext.UpvoteSubmissionAsync(5, "yetAnotherUser", "hashedAddress");
            Assert.Equal(VoteDirection.Downvote, result);

            var data = await dbContext.Set<Message>().FindAsync(5);

            Assert.Equal(4, data.Likes);
        }

        [Fact(DisplayName = "Removing upvote should remove tracking entry.")]
        public async Task RemovingUpvoteShouldRemoveTrackingEntry()
        {
            var data = await dbContext.Set<Votingtracker>().FindAsync(1);

            Assert.NotNull(data);

            var result = await dbContext.UpvoteSubmissionAsync(5, "yetAnotherUser", "hashedAddress");
            Assert.Equal(VoteDirection.Downvote, result);

            data = await dbContext.Set<Votingtracker>().FindAsync(1);

            Assert.Null(data);
        }

        [Fact(DisplayName = "Switching to upvote from downvote should decrement dislikes and increment likes")]
        public async Task SwitchingFromDownvoteToUpvote()
        {
            var commentTrackingData = await dbContext.Set<Votingtracker>().FindAsync(2);

            Assert.NotNull(commentTrackingData);

            var result = await dbContext.UpvoteSubmissionAsync(2, "yetAnotherUser", "hashedAddress");
            Assert.Equal(VoteDirection.DownvoteToUpvote, result);

            commentTrackingData = await dbContext.Set<Votingtracker>().FindAsync(1);
            var data = await dbContext.Set<Message>().FindAsync(2);
            Assert.Equal(1, commentTrackingData.VoteStatus);
            Assert.Equal(4, data.Dislikes);
            Assert.Equal(11, data.Likes);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }
    }
}
