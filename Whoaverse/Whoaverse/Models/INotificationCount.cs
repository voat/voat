namespace Voat.Models
{
    public interface INotificationCount
    {
        string UserName { get; }
        int? PrivateMessages { get; }
        int? CommentReplies { get; }
        int? PostReplies { get; }
    }

    public partial class AllNotificationCount : INotificationCount { }
    public partial class UnreadNotificationCount : INotificationCount { }
}
