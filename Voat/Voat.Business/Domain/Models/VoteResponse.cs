using Newtonsoft.Json;
using System;

using Voat.Common;
using Voat.Domain.Command;
using Voat.RulesEngine;

namespace Voat.Models
{
    /// <summary>
    /// The result of the vote request.
    /// </summary>
    public class VoteResponse : CommandResponse<Score>
    {
        private int? _recordedValue = null;
        private int _difference = 0;
        private string _ownerUserName = "";

        public VoteResponse(Status result, int? voteRecordedValue, string message) : base(null, result, message)
        {
            this.RecordedValue = voteRecordedValue;
        }

        public VoteResponse()
        {
        }

        [JsonIgnore()]
        public int Difference
        {
            get
            {
                return _difference;
            }

            set
            {
                _difference = value;
            }
        }

        [JsonIgnore()]
        public string OwnerUserName
        {
            get
            {
                return _ownerUserName;
            }

            set
            {
                _ownerUserName = value;
            }
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
            return String.Format("{0}: {1}", Status.ToString(), Message);
        }

        #region Helper Methods

        public static VoteResponse Create(RuleOutcome outcome, int? recordedValue = null)
        {
            var status = Status.NotProcessed;
            if (outcome.Result == RuleResult.Allowed)
            {
                status = Status.Success;
            }
            if (outcome.Result == RuleResult.Denied)
            {
                status = Status.Denied;
            }
            return new VoteResponse(status, recordedValue, outcome.ToString());
        }

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

        public static VoteResponse Successful(int voteValue)
        {
            return Successful(voteValue, "Vote registered");
        }

        public static VoteResponse Successful(int voteValue, string message)
        {
            return new VoteResponse(Status.Success, voteValue, message);
        }

        #endregion Helper Methods
    }
}
