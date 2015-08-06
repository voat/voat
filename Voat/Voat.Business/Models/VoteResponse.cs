using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Voat.Models;

namespace Voat.Models
{

    /// <summary>
    /// The result of the vote request.
    /// </summary>
    [DataContract]
    public class VoteResponse
    {

        private int? _recordedValue = null;

        /// <summary>
        /// The result of the vote operation
        /// </summary>
        [DataMember(Name = "result", Order = 2)]
        [JsonProperty("result", Order = 2)]
        public ProcessResult Result { get; private set; }

        /// <summary>
        /// The friendly name of the vote operation result
        /// </summary>
        [DataMember(Name = "resultName", Order = 3)]
        [JsonProperty("resultName", Order = 3)]
        public string ResultName
        {
            get
            {
                return this.Result.ToString();
            }
        }

        /// <summary>
        /// A description with information concerning the vote result
        /// </summary>
        [DataMember(Name = "message", Order = 4)]
        [JsonProperty("message", Order = 4)]
        public string Message { get; private set; }

        public VoteResponse(ProcessResult result, int? voteRecordedValue, string Message)
        {
            this.Result = result;
            this.Message = Message;
            this.RecordedValue = voteRecordedValue;
        }

        /// <summary>
        /// A value indicating whether the operation was successfull
        /// </summary>
        [JsonProperty("success", Order = 1)]
        [DataMember(Name = "success", Order = 1)]
        public bool Successfull
        {
            get
            {
                return Result == ProcessResult.Success;
            }
        }

        /// <summary>
        /// The users recorded vote value after the operation has completed. Use this value to verify vote operation is recorded correctly. Valid values are: -1 (down voted, 0 (revoked, unvoted), or 1 (up voted) 
        /// </summary>
        [JsonProperty("recordedValue")]
        [DataMember(Name = "recordedValue")]
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
            return String.Format("{0}: {1}", Result.ToString(), Message);
        }

        #region Helper Methods
        public static VoteResponse Success(int voteValue)
        {
            return Success(voteValue, "Vote registered");
        }
        public static VoteResponse Success(int voteValue, string message)
        {
            return new VoteResponse(ProcessResult.Success, voteValue, message);
        }
        public static VoteResponse Denied()
        {
            return Denied("Vote denied");
        }
        public static VoteResponse Denied(string message)
        {
            return new VoteResponse(ProcessResult.Denied, 0, message);
        }
        public static VoteResponse Ignored(int voteValue)
        {
            return Ignored(voteValue, "Vote ignored");
        }
        public static VoteResponse Ignored(int voteValue, string message)
        {
            return new VoteResponse(ProcessResult.Ignored, voteValue, message);
        }
        #endregion

    }
}
