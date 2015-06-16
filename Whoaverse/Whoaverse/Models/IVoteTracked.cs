namespace Voat.Models
{
    public interface IVoteTracked
    {
        string UserName { get; set; } // this should probably be in a separate interface, IAuthenticated or something...
        int? VoteStatus { get; set; }
    }

    public partial class Votingtracker : IVoteTracked
    {
    }

    public partial class Commentvotingtracker : IVoteTracked
    {
    }
}