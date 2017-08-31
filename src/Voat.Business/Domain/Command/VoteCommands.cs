#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using System;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Models;
using Voat.Notifications;
using Voat.Utilities;

namespace Voat.Domain.Command
{
    public class CommentVoteCommand : VoteCommand
    {
        public CommentVoteCommand(int commentID, int voteStatus, string addressHash, bool revokeOnRevote = true)
            : base(voteStatus, addressHash)
        {
            CommentID = commentID;
            RevokeOnRevote = revokeOnRevote;
        }

        public int CommentID { get; private set; }

        protected override async Task<Tuple<VoteResponse, VoteResponse>> CacheExecute()
        {
            using (var repo = new Repository(User))
            {
                var outcome = await Task.Run(() => repo.VoteComment(CommentID, VoteStatus, AddressHash, RevokeOnRevote)).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

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
        public SubmissionVoteCommand(int submissionID, int voteStatus, string addressHash, bool revokeOnRevote = true)
            : base(voteStatus, addressHash)
        {
            SubmissionID = submissionID;
            RevokeOnRevote = revokeOnRevote;
        }

        public int SubmissionID { get; private set; }

        protected override async Task<Tuple<VoteResponse, VoteResponse>> CacheExecute()
        {
            using (var repo = new Repository(User))
            {
                var outcome = await Task.Run(() => repo.VoteSubmission(SubmissionID, VoteStatus, AddressHash, RevokeOnRevote)).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

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
        public VoteCommand(int voteStatus, string addressHash, bool revokeOnRevote = true)
        {
            if (voteStatus < -1 || voteStatus > 1)
            {
                throw new ArgumentOutOfRangeException("voteValue", voteStatus, "Invalid vote value");
            }
            this.VoteStatus = voteStatus;
            this.RevokeOnRevote = revokeOnRevote;
            this.AddressHash = addressHash;
        }

        public bool RevokeOnRevote { get; protected set; }

        public int VoteStatus { get; private set; }

        public string AddressHash { get; private set; }
    }
}
