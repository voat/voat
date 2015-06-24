namespace Command.Tests.Voting
{
    using System.Data.Entity;
    using System.Threading.Tasks;
    using Query.Tests;
    using Voat.Commands.Voting;
    using Voat.Models;
    using Xunit;

    [Trait("Category", "Semi-integration"), Trait("Subcategory", "Commands")]
    public class CommentVoteCommands
    {
        private readonly DbContext dbContext;

        public CommentVoteCommands()
        {
            var effortConnection = Effort.EntityConnectionFactory.CreateTransient(EntityString.Value);
            dbContext = new whoaverseEntities(effortConnection);

            dbContext.Set<Subverse>().Add(new Subverse
            {
                title = "test",
                name = "test",
                minimumdownvoteccp = 4
            });

            var firstMessage = dbContext.Set<Message>().Add(new Message
            {
                Id = 1,
                Name = "yetAnotherUser",
                Anonymized = true
            });
            var secondMessage = dbContext.Set<Message>().Add(new Message
            {
                Id = 2,
                Name = "testuser",
                Anonymized = false,
                Subverse = "test"
            });

            dbContext.SaveChanges();

            dbContext.Set<Comment>().Add(new Comment
            {
                Id = 1,
                CommentContent = "Empty",
                Name = "testuser",
                Message = secondMessage
            });

            var secondComment = dbContext.Set<Comment>().Add(new Comment
            {
                Id = 2,
                CommentContent = "Empty",
                Name = "testuser2",
                Likes = 7,
                Dislikes = 5,
                Message = secondMessage
            });

            dbContext.Set<Comment>().Add(new Comment
            {
                Id = 3,
                CommentContent = "Empty",
                Name = "abc",
                Message = firstMessage
            });

            dbContext.Set<Comment>().Add(new Comment
            {
                Id = 4,
                Message = secondMessage,
                Name = "abc",
                Likes = 5,
                CommentContent = "Empty"
            });

            var fifthComment = dbContext.Set<Comment>().Add(new Comment
            {
                Id = 5,
                Message = secondMessage,
                Name = "abc",
                Likes = 5,
                CommentContent = "Empty"
            });

            dbContext.Set<Comment>().Add(new Comment
            {
                Id = 6,
                Message = secondMessage,
                Name = "abc",
                Likes = 5,
                CommentContent = "Empty"
            });

            dbContext.SaveChanges();

            dbContext.Set<Commentvotingtracker>().Add(new Commentvotingtracker
            {
                Id = 1,
                Comment = fifthComment,
                ClientIpAddress = "hashedAddress",
                VoteStatus = 1,
                UserName = "yetAnotherUser"
            });
            
            dbContext.Set<Commentvotingtracker>().Add(new Commentvotingtracker
            {
                Id = 2,
                Comment = secondComment,
                ClientIpAddress = "hashedAddress",
                VoteStatus = -1,
                UserName = "yetAnotherUser"
            });

            dbContext.Set<Commentvotingtracker>().Add(new Commentvotingtracker
            {
                Id = 3,
                Comment = fifthComment,
                ClientIpAddress = "hashedAddress",
                VoteStatus = 1,
                UserName = "abc"
            });


            dbContext.SaveChanges();
        }

        [Fact(DisplayName = "Users cannot upvote their own comment")]
        public async Task UsersCannotUpvoteTheirOwnComments()
        {
            var result = await dbContext.UpvoteCommentAsync(1, "testuser", "hash");
            Assert.Equal(VoteDirection.None, result);
        }

        [Fact(DisplayName = "If the comment is anonymous, it should not be available for upvote.")]
        public async Task CannotUpvoteIfAnonymous()
        {
            var result = await dbContext.UpvoteCommentAsync(3, "testuser", "hash");
            Assert.Equal(VoteDirection.None, result);
        }

        [Fact(DisplayName = "Users cannot upvote the same comment from different accounts from the same IP.")]
        public async Task CannotUpvoteTwiceFromSameIpAddress()
        {
            var result = await dbContext.UpvoteCommentAsync(5, "testuser", "hashedAddress");
            Assert.Equal(VoteDirection.None, result);
        }

        [Fact(DisplayName = "Initial vote on someone else's non-anonymous comment should pass.")]
        public async Task RegularFirstUpvote()
        {
            var result = await dbContext.UpvoteCommentAsync(4, "testuser2", "otherAddress");
            Assert.Equal(VoteDirection.Upvote, result);
        }

        [Fact(DisplayName = "Initial vote should add comment tracking entry")]
        public async Task FirstUpvoteShouldAddTrackingEntry()
        {
            var trackingData =
                await
                    dbContext.Set<Commentvotingtracker>()
                        .FirstOrDefaultAsync(cvt => cvt.CommentId == 4 && cvt.UserName == "testuser2");

            Assert.Null(trackingData);

            var result = await dbContext.UpvoteCommentAsync(4, "testuser2", "otherAddress");
            Assert.Equal(VoteDirection.Upvote, result);
            trackingData = await dbContext.Set<Commentvotingtracker>().FirstOrDefaultAsync(cvt => cvt.CommentId == 4 && cvt.UserName == "testuser2");
            
            Assert.NotNull(trackingData);
            Assert.Equal(1, trackingData.VoteStatus);
            Assert.Equal(4, trackingData.CommentId);
            Assert.Equal("testuser2", trackingData.UserName);
            Assert.Equal("otherAddress", trackingData.ClientIpAddress);
        }

        [Fact(DisplayName = "Removing upvote should decrement likes.")]
        public async Task RemovingUpvote()
        {
            var result = await dbContext.UpvoteCommentAsync(5, "yetAnotherUser", "hashedAddress");
            Assert.Equal(VoteDirection.Downvote, result);

            var data = await dbContext.Set<Comment>().FindAsync(5);

            Assert.Equal(4, data.Likes);
        }

        [Fact(DisplayName = "Removing upvote should remove tracking entry.")]
        public async Task RemovingUpvoteShouldRemoveTrackingEntry()
        {
            var data = await dbContext.Set<Commentvotingtracker>().FindAsync(1);

            Assert.NotNull(data);

            var result = await dbContext.UpvoteCommentAsync(5, "yetAnotherUser", "hashedAddress");
            Assert.Equal(VoteDirection.Downvote, result);

            data = await dbContext.Set<Commentvotingtracker>().FindAsync(1);

            Assert.Null(data);
        }

        [Fact(DisplayName = "Switching to upvote from downvote should decrement dislikes and increment likes")]
        public async Task SwitchingFromDownvoteToUpvote()
        {
            var commentTrackingData = await dbContext.Set<Commentvotingtracker>().FindAsync(2);

            Assert.NotNull(commentTrackingData);

            var result = await dbContext.UpvoteCommentAsync(2, "yetAnotherUser", "hashedAddress");
            Assert.Equal(VoteDirection.DownvoteToUpvote, result);

            commentTrackingData = await dbContext.Set<Commentvotingtracker>().FindAsync(1);
            var data = await dbContext.Set<Comment>().FindAsync(2);
            Assert.Equal(1, commentTrackingData.VoteStatus);
            Assert.Equal(4, data.Dislikes);
            Assert.Equal(8, data.Likes);
        }

        [Fact(DisplayName = "If the comment is anonymous, it should not be available for downvote.")]
        public async Task CannotDownvoteIfAnonymous()
        {
            var result = await dbContext.DownvoteCommentAsync(3, "testuser", "hash");
            Assert.Equal(VoteDirection.None, result);
        }

        [Fact(DisplayName = "If user doesn't have karma over a minimum downvote threshold, downvote is discarded.")]
        public async Task CannotDownvoteIfBelowDownvoteThreshold()
        {
            var result = await dbContext.DownvoteCommentAsync(2, "testuser2", "hash");
            Assert.Equal(VoteDirection.None, result);
        }

        [Fact(DisplayName = "Initial downvote after passing validation rules should be accepted.")]
        public async Task RegularFirstDownvote()
        {
            var result = await dbContext.DownvoteCommentAsync(2, "abc", "hash");
            Assert.Equal(VoteDirection.Downvote, result);
        }

        [Fact(DisplayName = "Removing the downvote should remove the 'dislike'")]
        public async Task RemovingDownvoteShouldRemoveDislike()
        {
            var result = await dbContext.UpvoteCommentAsync(2, "yetAnotherUser", "hashedAddress");
            Assert.Equal(VoteDirection.DownvoteToUpvote, result);

            var data = await dbContext.Set<Comment>().FindAsync(2);

            Assert.Equal(4, data.Dislikes);
        }

        [Fact(DisplayName = "Changing the upvote to downvote should remove the 'like' and add 'dislike'")]
        public async Task SwitchingUpvoteToDownvote()
        {
            var result = await dbContext.DownvoteCommentAsync(5, "abc", "hashedAddress");
            Assert.Equal(VoteDirection.UpvoteToDownvote, result);

            var data = await dbContext.Set<Comment>().FindAsync(5);

            Assert.Equal(1, data.Dislikes);
            Assert.Equal(4, data.Likes);
        }
    }
}
