using System;

using Voat.Common;
using Voat.Domain.Command;

namespace Voat.Models
{
    /// <summary>
    /// The result of the vote request.
    /// </summary>
    public class VoteResponse : CommandResponse<Score>
    {
        private int? _recordedValue = null;

        public VoteResponse(Status result, int? voteRecordedValue, string description) : base(null, result, description)
        {
            this.RecordedValue = voteRecordedValue;
        }

        /// <summary>
        /// The users recorded vote value after the operation has completed. Use this value to verify vote operation is recorded correctly. Valid values are: -1 (down voted, 0 (revoked, unvoted), or 1 (up voted)
        /// </summary>
        public int? RecordedValue
        {
            get
            {
                return _recordedValue;
            }
            private set
            {
                if (value.HasValue && Math.Abs(value.Value) <= 1)
                {
                    _recordedValue = value;
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{0}: {1}", Status.ToString(), SystemDescription);
        }

        #region Helper Methods

        public static VoteResponse Denied()
        {
            return Denied("Vote denied");
        }

        public static VoteResponse Denied(string message)
        {
            return new VoteResponse(Status.Denied, null, message);
        }

        public static VoteResponse Ignored(int voteValue)
        {
            return Ignored(voteValue, "Vote ignored");
        }

        public static VoteResponse Ignored(int voteValue, string message)
        {
            return new VoteResponse(Status.Ignored, voteValue, message);
        }

        public static VoteResponse Success(int voteValue)
        {
            return Success(voteValue, "Vote registered");
        }

        public static VoteResponse Success(int voteValue, string message)
        {
            return new VoteResponse(Status.Success, voteValue, message);
        }

        #endregion Helper Methods
    }
}
