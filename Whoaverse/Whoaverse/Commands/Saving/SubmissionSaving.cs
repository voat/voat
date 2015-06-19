namespace Voat.Commands.Saving
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;

    public static class SubmissionSaving
    {
        public static async Task ToggleSubmissionSaveAsync(this DbContext context, int submissionId, string userName)
        {
            var existingComment =
                await context.Set<Savingtracker>()
                    .Where(c => c.MessageId == submissionId && c.UserName == userName)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

            if (existingComment != null)
            {
                context.Set<Savingtracker>().Remove(existingComment);
            }
            else
            {
                context.Set<Savingtracker>().Add(new Savingtracker
                {
                    MessageId = submissionId,
                    UserName = userName,
                    Timestamp = DateTime.Now
                });
            }
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}