namespace Voat.Commands
{
    using System;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using Models;
    using Models.Messaging;

    public static class Message
    {
        public static async Task SendPrivateMessageAsync(this DbContext context, PrivateMessage message)
        {
            context.Set<Privatemessage>().Add(new Privatemessage
            {
                Sender = message.Sender,
                Subject = message.Subject,
                Body = message.Body,
                Status = true,
                Markedasunread = true,
                Timestamp = DateTime.Now
            });
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}