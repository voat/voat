namespace Voat.Models.Messaging
{
    public class PrivateMessage
    {
        public PrivateMessage(string sender, string recipient, string subject, string body)
        {
            Sender = sender;
            Recipient = recipient;
            Subject = subject;
            Body = body;
        }

        public string Sender { get; private set; }
        public string Recipient { get; private set; }
        public string Subject { get; private set; }
        public string Body { get; private set; }
    }
}