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

using Voat.Data;

//using Microsoft.AspNet.SignalR;

namespace Voat.Utilities
{
    public static class Voting
    {
        //TODO: Need to relocate this, still used in view _VotingIconsSubmission.cshtml
        // returns -1:downvoted, 1:upvoted, 0:not voted
        public static int CheckIfVoted(string userToCheck, int submissionID)
        {
            using (var repo = new Repository())
            {
                return repo.UserVoteStatus(userToCheck, Domain.Models.ContentType.Submission, submissionID);
            }
            //using (var db = new voatEntities())
            //{
            //    var checkResult = db.SubmissionVoteTrackers.Where(u => u.UserName == userToCheck && u.SubmissionID == submissionID)
            //            
            //            .FirstOrDefault();

            //    int intCheckResult = checkResult != null ? checkResult.VoteStatus.Value : 0;
            //    return intCheckResult;
            //}
        }
    }
}
