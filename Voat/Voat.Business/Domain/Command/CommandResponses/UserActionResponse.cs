namespace Voat.Domain.Command
{
    public class UserActionResponse
    {
        /// <summary>
        /// User that initiated the action
        /// </summary>
        public string OriginUserName { get; set; }

        /// <summary>
        /// User that was the target or destination of action
        /// </summary>
        public string TargetUserName { get; set; }
    }
}
