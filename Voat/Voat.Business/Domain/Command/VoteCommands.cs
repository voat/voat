using System;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Models;
using Voat.Utilities;

namespace Voat.Domain.Command
{
    public class CommentVoteCommand : VoteCommand
    {
        public CommentVoteCommand(int commentID, int voteValue, string addressHash, bool revokeOnRevote = true)
            : base(voteValue, addressHash)
        {
            CommentID = commentID;
            RevokeOnRevote = revokeOnRevote;
        }

        public int CommentID { get; private set; }

        protected override async Task<Tuple<VoteResponse, VoteResponse>> CacheExecute()
        {
            using (var db = new Repository())
            {
                var outcome = await Task.Run(() => db.VoteComment(CommentID, VoteValue, AddressHash, RevokeOnRevote)).ConfigureAwait(false);

                //Raise event
                if (outcome.Success)
                {
                    EventNotification.Instance.SendVoteNotice(outcome.OwnerUserName, this.UserName, Models.ContentType.Comment, CommentID, outcome.Difference);
                }
                return new Tuple<VoteResponse, VoteResponse>(outcome, outcome);
            }
        }

        protected override void UpdateCache(VoteResponse result)
        {
            if (result.Success)
            {
                //update cache somehow
            }
        }
    }

    public class SubmissionVoteCommand : VoteCommand
    {
        public SubmissionVoteCommand(int submissionID, int voteValue, string addressHash, bool revokeOnRevote = true)
            : base(voteValue, addressHash)
        {
            SubmissionID = submissionID;
            RevokeOnRevote = revokeOnRevote;
        }

        public int SubmissionID { get; private set; }

        protected override async Task<Tuple<VoteResponse, VoteResponse>> CacheExecute()
        {
            using (var gateway = new Repository())
            {
                var outcome = await Task.Run(() => gateway.VoteSubmission(SubmissionID, VoteValue, AddressHash, RevokeOnRevote)).ConfigureAwait(false);

                //Raise event
                if (outcome.Success)
                {
                    EventNotification.Instance.SendVoteNotice(outcome.OwnerUserName, this.UserName, Models.ContentType.Submission, SubmissionID, outcome.Difference);
                }
                return new Tuple<VoteResponse, VoteResponse>(outcome, outcome);
            }
        }

        protected override void UpdateCache(VoteResponse result)
        {
            if (result.Success)
            {
                //update cache somehow
            }
        }
    }

    public abstract class VoteCommand : CacheCommand<VoteResponse, VoteResponse>
    {
        public VoteCommand(int voteValue, string addressHash, bool revokeOnRevote = true)
        {
            if (voteValue < -1 || voteValue > 1)
            {
                throw new ArgumentOutOfRangeException("voteValue", voteValue, "Invalid vote value");
            }
            this.VoteValue = voteValue;
            this.RevokeOnRevote = revokeOnRevote;
            this.AddressHash = addressHash;
        }

        public bool RevokeOnRevote { get; protected set; }

        public int VoteValue { get; private set; }

        public string AddressHash { get; private set; }
    }
}
