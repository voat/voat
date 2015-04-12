/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat
All Rights Reserved.
*/

using System;
using System.Data.Entity;
using System.Linq;
using Voat.Models;

namespace Voat.Utils
{
    public class SavingComments
    {

        // returns true if saved, false otherwise
        public static bool? CheckIfSavedComment(string userToCheck, int commentId)
        {
            using (var db = new whoaverseEntities())
            {
                var checkResult = db.Commentsavingtrackers.Where(b => b.CommentId == commentId && b.UserName == userToCheck)
                        .AsNoTracking()
                        .FirstOrDefault();

                var boolCheckResult = checkResult != null ? checkResult.SaveStatus : false;

                return boolCheckResult;
            }

        }

        // a user wishes to save a comment, save it
        public static void SaveComment(int commentId, string userWhichSaved)
        {
            var result = CheckIfSavedComment(userWhichSaved, commentId);

            using (var db = new whoaverseEntities())
            {
                if (result == true)
                {
                    // Already saved, unsave
                    UnSaveComment(userWhichSaved, commentId);
                }
                else
                {
                    // register save
                    var tmpSavingTracker = new Commentsavingtracker
                    {
                        CommentId = commentId,
                        UserName = userWhichSaved,
                        SaveStatus = true,
                        Timestamp = DateTime.Now
                    };
                    db.Commentsavingtrackers.Add(tmpSavingTracker);
                    db.SaveChanges();
                }
            }

        }

        // a user has saved this comment earlier and wishes to unsave it, delete the record
        private static void UnSaveComment(string userWhichSaved, int commentId)
        {
            using (var db = new whoaverseEntities())
            {
                var votingTracker = db.Commentsavingtrackers.FirstOrDefault(b => b.CommentId == commentId && b.UserName == userWhichSaved);

                if (votingTracker == null) return;
                // delete vote history
                db.Commentsavingtrackers.Remove(votingTracker);
                db.SaveChanges();
            }
        }
    }
}