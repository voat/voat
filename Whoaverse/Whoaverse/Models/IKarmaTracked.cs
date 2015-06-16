namespace Voat.Models
{
    /// <summary>
    /// Represents entities which have their karma tracked
    /// </summary>
    public interface IKarmaTracked
    {
        short Likes { get; set; }
        short Dislikes { get; set; }
    }

    public partial class Comment : IKarmaTracked
    {
    }

    public partial class Message : IKarmaTracked
    {
    }
}
