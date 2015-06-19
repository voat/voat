namespace Voat.Commands.Saving
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;

    public static class CommentSaving
    {
        public static async Task ToggleCommentSaveAsync(this DbContext context, int commentId, string userName)
        {
            var existingComment =
                await context.Set<Commentsavingtracker>()
                    .Where(c => c.CommentId == commentId && c.UserName == userName)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

            if (existingComment != null)
            {
                context.Set<Commentsavingtracker>().Remove(existingComment);
            }
            else
            {
                context.Set<Commentsavingtracker>().Add(new Commentsavingtracker
                {
                    CommentId = commentId,
                    UserName = userName,
                    Timestamp = DateTime.Now
                });
            }

            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}