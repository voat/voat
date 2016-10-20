namespace Voat.Domain.Command
{
    public class SubverseUserActionResponse : UserActionResponse
    {
        /// <summary>
        /// Subverse that the action involved
        /// </summary>
        public string Subverse { get; set; }
    }
}
