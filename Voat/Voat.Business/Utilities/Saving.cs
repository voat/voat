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
using System.Linq;
using Voat.Data.Models;

namespace Voat.Utilities
{
    public static class Saving
    {
        // returns true if saved, false otherwise
        public static bool? CheckIfSaved(string userToCheck, int messageId)
        {

            using (voatEntities db = new voatEntities())
            {

                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM SubmissionSaveTracker WITH (NOLOCK) WHERE UserName = @UserName AND SubmissionID = @SubmissionID";

                var param = cmd.CreateParameter();
                param.ParameterName = "UserName";
                param.DbType = System.Data.DbType.String;
                param.Value = userToCheck;
                cmd.Parameters.Add(param);

                param = cmd.CreateParameter();
                param.ParameterName = "SubmissionID";
                param.DbType = System.Data.DbType.String;
                param.Value = messageId;
                cmd.Parameters.Add(param);

                if (cmd.Connection.State != System.Data.ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }
                
                int count = (int)cmd.ExecuteScalar();

                return count > 0;
            }

            //using (var db = new voatEntities())
            //{
            //    return db.Savingtrackers.Where(u => u.UserName == userToCheck && u.MessageId == messageId).AsNoTracking().Any();
            //}
        }

        // a user wishes to save a submission, save it
        public static void SaveSubmission(int submissionId, string userWhichSaved)
        {
            var result = CheckIfSaved(userWhichSaved, submissionId);

            using (var db = new voatEntities())
            {
                if (result == true)
                {
                    // Already saved, unsave
                    UnSaveSubmission(userWhichSaved, submissionId);
                }
                else
                {
                    // register save
                    var tmpSavingTracker = new SubmissionSaveTracker
                    {
                        SubmissionID = submissionId,
                        UserName = userWhichSaved,
                        CreationDate = DateTime.Now
                    };
                    db.SubmissionSaveTrackers.Add(tmpSavingTracker);
                    db.SaveChanges();
                }
            }

        }

        // a user has saved this submission earlier and wishes to unsave it, delete the record
        private static void UnSaveSubmission(string userWhichSaved, int submissionID)
        {
            using (var db = new voatEntities())
            {
                var saveTracker = db.SubmissionSaveTrackers.FirstOrDefault(b => b.SubmissionID == submissionID && b.UserName == userWhichSaved);

                if (saveTracker == null) return;
                //delete vote history
                db.SubmissionSaveTrackers.Remove(saveTracker);
                db.SaveChanges();
            }
        }
    }
}