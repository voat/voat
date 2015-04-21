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
    public static class Saving
    {
        // returns true if saved, false otherwise
        public static bool? CheckIfSaved(string userToCheck, int messageId)
        {
            using (var db = new whoaverseEntities())
            {
                return db.Savingtrackers.Where(u => u.UserName == userToCheck && u.MessageId == messageId).AsNoTracking().Any();
            }
        }

        // a user wishes to save a submission, save it
        public static void SaveSubmission(int submissionId, string userWhichSaved)
        {
            var result = CheckIfSaved(userWhichSaved, submissionId);

            using (var db = new whoaverseEntities())
            {
                if (result == true)
                {
                    // Already saved, unsave
                    UnSaveSubmission(userWhichSaved, submissionId);
                }
                else
                {
                    // register save
                    var tmpSavingTracker = new Savingtracker
                    {
                        MessageId = submissionId,
                        UserName = userWhichSaved,
                        Timestamp = DateTime.Now
                    };
                    db.Savingtrackers.Add(tmpSavingTracker);
                    db.SaveChanges();
                }
            }

        }

        // a user has saved this submission earlier and wishes to unsave it, delete the record
        private static void UnSaveSubmission(string userWhichSaved, int messageId)
        {
            using (var db = new whoaverseEntities())
            {
                var saveTracker = db.Savingtrackers.FirstOrDefault(b => b.MessageId == messageId && b.UserName == userWhichSaved);

                if (saveTracker == null) return;
                //delete vote history
                db.Savingtrackers.Remove(saveTracker);
                db.SaveChanges();
            }
        }
    }
}