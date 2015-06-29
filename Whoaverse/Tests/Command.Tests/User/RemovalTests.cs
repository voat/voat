namespace Command.Tests.User
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Effort;
    using EntityFramework;
    using EntityFramework.Batch;
    using Voat.Commands.User;
    using Voat.Models;
    using Xunit;

    public class RemovalTests
    {
        public RemovalTests()
        {
            var container = new Container();
            Locator.RegisterDefaults(container);
            container.Register<IBatchRunner>(() => new EffortBatchRunner());
            Locator.SetContainer(container);
        }

        private static async Task<DbContext> CreateContextAsync()
        {
            var effortConnection = EntityConnectionFactory.CreateTransient(EntityString.Value);
            var context = new whoaverseEntities(effortConnection);
            context.Set<Subverse>().Add(new Subverse { title = "test subverse", name = "test" });
            context.Set<Message>().Add(new Message
            {
                Id = 1,
                Name = "john",
                Subverse = "test",
                MessageContent = "Test message",
                Title = "Test msg!"
            });
            var msg = context.Set<Message>().Add(new Message
            {
                Id = 2,
                Name = "jane",
                Subverse = "test"
            });
            context.Set<Message>().Add(new Message
            {
                Id = 3,
                Name = "john",
                Subverse = "test",
                Type = 1,
                Title = "Other test",
                MessageContent = "Other test message"
            });
            context.Set<Comment>().Add(new Comment
            {
                Id = 1,
                Message = msg,
                Name = "john",
                CommentContent = "Hey there!"
            });
            var comment = context.Set<Comment>().Add(new Comment
            {
                Id = 2,
                Message = msg,
                Name = "jane",
                CommentContent = "Hey there!"
            });
            context.Set<SubverseAdmin>().Add(new SubverseAdmin
            {
                Id = 1,
                SubverseName = "test",
                Username = "john"
            });
            context.Set<Commentreplynotification>().Add(new Commentreplynotification
            {
                Id = 1,
                Subverse = "test",
                Recipient = "john",
                Body = "No body",
                Sender = "jane",
                Subject = "empty"
            });
            context.Set<Postreplynotification>().Add(new Postreplynotification
            {
                Id = 1,
                Recipient = "john",
                Subverse = "test",
                Body = "No body",
                Sender = "jane",
                Subject = "empty"
            });
            context.Set<Privatemessage>().Add(new Privatemessage
            {
                Id = 1,
                Recipient = "john",
                Body = "Empty body",
                Sender = "jane",
                Subject = "No subject"
            });
            context.Set<Votingtracker>().Add(new Votingtracker
            {
                Id = 1,
                Message = msg,
                UserName = "john",
                VoteStatus = 1
            });
            context.Set<Commentvotingtracker>().Add(new Commentvotingtracker
            {
                Id = 1,
                Comment = comment,
                UserName = "john",
                VoteStatus = 1
            });

            await context.SaveChangesAsync();

            return context;
        }
            
        [Fact(DisplayName = "Removing user also removes tracked votes")]
        public async Task VotingTackingIsRemoved()
        {
            using (var dbContext = await CreateContextAsync())
            {
                var data = await dbContext.Set<Votingtracker>().Where(v => v.UserName == "john").ToListAsync();
                Assert.NotEmpty(data);
                Assert.Equal(1, data.Count);

                await dbContext.DeleteUserAsync(new WhoaVerseUser {UserName = "john"});

                data = await dbContext.Set<Votingtracker>().Where(v => v.UserName == "john").ToListAsync();
                Assert.Empty(data);
            }
        }

        [Fact(DisplayName = "Removing user also removes tracked comments")]
        public async Task CommentVotingTrackingIsRemoved()
        {
            using (var dbContext = await CreateContextAsync())
            {
                var data = await dbContext.Set<Commentvotingtracker>().Where(v => v.UserName == "john").ToListAsync();
                Assert.NotEmpty(data);
                Assert.Equal(1, data.Count);

                await dbContext.DeleteUserAsync(new WhoaVerseUser {UserName = "john"});

                data = await dbContext.Set<Commentvotingtracker>().Where(v => v.UserName == "john").ToListAsync();
                Assert.Empty(data);
            }
        }

        [Fact(DisplayName = "Removing user also 'soft deletes' comments, i.e. leaves the comment, but replaces user name and content.")]
        public async Task CommentsUndergoSoftDeletion()
        {
            using (var dbContext = await CreateContextAsync())
            {
                var data = await dbContext.Set<Comment>().Where(v => v.Name == "john").ToListAsync();
                Assert.NotEmpty(data);
                Assert.Equal(1, data.Count);
                Assert.Equal("john", data[0].Name);
                Assert.Equal("Hey there!", data[0].CommentContent);

                await dbContext.DeleteUserAsync(new WhoaVerseUser {UserName = "john"});

                data = await dbContext.Set<Comment>().Where(v => v.Name == "john").ToListAsync();
                Assert.Empty(data);
                var deleted = await dbContext.Set<Comment>().Where(v => v.Name == "deleted").ToListAsync();
                Assert.Equal(1, deleted.Count);
                Assert.Equal("deleted", deleted[0].Name);
                Assert.Equal("deleted by user", deleted[0].CommentContent);
            }
        }

        [Theory(
            DisplayName =
                "Removing user also 'soft deletes' messages, i.e. leaves the message, but replaces user name, content and title"
            )]
        [InlineData(1, "Test msg!", "Test message", "http://voat.co")]
        [InlineData(3, "Other test", "Other test message", "deleted by user")]
        public async Task MessagesUndergoSoftDeletion(int id, string preDeletionTitle, string preDeletionContent,
            string postDeletionTitle)
        {
            using (var dbContext = await CreateContextAsync())
            {
                var data = await dbContext.Set<Message>().Where(x => x.Id == id).ToListAsync();
                Assert.NotEmpty(data);
                Assert.Equal(1, data.Count);
                Assert.Equal("john", data[0].Name);
                Assert.Equal(preDeletionContent, data[0].MessageContent);
                Assert.Equal(preDeletionTitle, data[0].Title);


                await dbContext.DeleteUserAsync(new WhoaVerseUser {UserName = "john"});

                data = await dbContext.Set<Message>().Where(v => v.Name == "john").ToListAsync();
                Assert.Empty(data);
                var deleted = await dbContext.Set<Message>().Where(v => v.Id == id).ToListAsync();
                Assert.Equal(1, deleted.Count);
                Assert.Equal("deleted", deleted[0].Name);
                Assert.Equal("deleted by user", deleted[0].MessageContent);
                Assert.Equal(postDeletionTitle, deleted[0].Title);
            }
        }

        [Fact(DisplayName = "Removing user also removes all admin and moderator rights")]
        public async Task AdminRightsAreRemoved()
        {
            using (var dbContext = await CreateContextAsync())
            {
                var data = await dbContext.Set<SubverseAdmin>().Where(x => x.Username == "john").ToListAsync();
                Assert.NotEmpty(data);
                Assert.Equal(1, data.Count);

                await dbContext.DeleteUserAsync(new WhoaVerseUser { UserName = "john" });

                data = await dbContext.Set<SubverseAdmin>().Where(x => x.Username == "john").ToListAsync();
                Assert.Empty(data);
            }
        }

        [Fact(DisplayName = "Removing user also removes user's comment reply notifications")]
        public async Task CommentNotificationsAreRemoved()
        {
            using (var dbContext = await CreateContextAsync())
            {
                var data = await dbContext.Set<Commentreplynotification>().Where(x => x.Recipient == "john").ToListAsync();
                Assert.NotEmpty(data);
                Assert.Equal(1, data.Count);

                await dbContext.DeleteUserAsync(new WhoaVerseUser { UserName = "john" });

                data = await dbContext.Set<Commentreplynotification>().Where(x => x.Recipient == "john").ToListAsync();
                Assert.Empty(data);
            }
        }

        [Fact(DisplayName = "Removing user also removes user's post reply notifications")]
        public async Task PostNotificationsAreRemoved()
        {
            using (var dbContext = await CreateContextAsync())
            {
                var data = await dbContext.Set<Postreplynotification>().Where(x => x.Recipient == "john").ToListAsync();
                Assert.NotEmpty(data);
                Assert.Equal(1, data.Count);

                await dbContext.DeleteUserAsync(new WhoaVerseUser { UserName = "john" });

                data = await dbContext.Set<Postreplynotification>().Where(x => x.Recipient == "john").ToListAsync();
                Assert.Empty(data);
            }
        }

        [Fact(DisplayName = "Removing user also removes user's private messages")]
        public async Task PrivateMessagesAreRemoved()
        {
            using (var dbContext = await CreateContextAsync())
            {
                var data = await dbContext.Set<Privatemessage>().Where(x => x.Recipient == "john").ToListAsync();
                Assert.NotEmpty(data);
                Assert.Equal(1, data.Count);

                await dbContext.DeleteUserAsync(new WhoaVerseUser { UserName = "john" });

                data = await dbContext.Set<Privatemessage>().Where(x => x.Recipient == "john").ToListAsync();
                Assert.Empty(data);
            }
        }
    }
}
