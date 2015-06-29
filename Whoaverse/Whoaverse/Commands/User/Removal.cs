namespace Voat.Commands.User
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using EntityFramework.Extensions;
    using Models;

    public static class Removal
    {
        public static async Task DeleteUserAsync(this DbContext context, WhoaVerseUser user)
        {
            // TODO: Maybe all the calls below should be moved into one stored proc?
            using (var transaction = context.Database.BeginTransaction())
            {
                await context.Set<Votingtracker>()
                    .Where(c => c.UserName == user.UserName)
                    .DeleteAsync()
                    .ConfigureAwait(false);

                await context.Set<Commentvotingtracker>()
                    .Where(c => c.UserName == user.UserName)
                    .DeleteAsync()
                    .ConfigureAwait(false);

                await context.Set<Comment>()
                    .Where(c => c.Name == user.UserName)
                    .UpdateAsync(c => new Comment {Name = "deleted", CommentContent = "deleted by user"})
                    .ConfigureAwait(false);

                await context.Set<Message>()
                    .Where(c => c.Name == user.UserName)
                    .UpdateAsync(m => new Message
                    {
                        Name = "deleted",
                        MessageContent = "deleted by user",
                        Title = m.Type == 1 ? "deleted by user" : "http://voat.co"
                    })
                    .ConfigureAwait(false);

                await
                    context.Set<SubverseAdmin>()
                        .Where(a => a.Username == user.UserName)
                        .DeleteAsync()
                        .ConfigureAwait(false);

                await
                    context.Set<Commentreplynotification>()
                        .Where(c => c.Recipient == user.UserName)
                        .DeleteAsync()
                        .ConfigureAwait(false);

                await context.Set<Postreplynotification>()
                    .Where(p => p.Recipient == user.UserName)
                    .DeleteAsync()
                    .ConfigureAwait(false);

                await
                    context.Set<Privatemessage>()
                        .Where(pm => pm.Recipient == user.UserName)
                        .DeleteAsync()
                        .ConfigureAwait(false);

                await context.SaveChangesAsync().ConfigureAwait(false);

                transaction.Commit();
            }
        }
    }
}