/*
This source file is subject to version 3 of the GPL license,
that is bundled with this package in the file LICENSE, and is
available online at http://www.gnu.org/licenses/gpl.txt;
you may not use this file except in compliance with the License.

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat, Inc.
All Rights Reserved.
*/

using System;
using System.Data.Entity;
using System.Linq;
using Voat.Common;
using Voat.Data;

//using Microsoft.AspNet.SignalR;
using Voat.Data.Models;

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
            //            .AsNoTracking()
            //            .FirstOrDefault();

            //    int intCheckResult = checkResult != null ? checkResult.VoteStatus.Value : 0;
            //    return intCheckResult;
            //}
        }
    }
}
