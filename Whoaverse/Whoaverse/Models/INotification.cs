namespace Voat.Models
{
    public interface INotification
    {
        int Id { get; }
        bool Markedasunread { get; }
        bool Status { get; }
        string Recipient { get; }
        string Sender { get; }
    }

    public partial class Privatemessage : INotification { }
    public partial class Commentreplynotification : INotification { }
    public partial class Postreplynotification : INotification { }
}