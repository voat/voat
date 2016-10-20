using Voat.Data.Models;

namespace Voat.Domain.Command
{
    public class RemoveModeratorResponse : SubverseUserActionResponse
    {
        public SubverseModerator SubverseModerator { get; set; }
    }
}
