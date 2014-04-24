/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
 */

using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Whoaverse.Models;
using System.Data.SqlClient;

namespace Whoaverse.Utils
{
    public class Voting
    {
        //returns -1:downvoted, 1:upvoted, or 0:not voted
        public static int CheckIfVoted(string userToCheck, int messageId)
        {         
            int intCheckResult = 0;

            using (whoaverseEntities db = new whoaverseEntities())            
            {
                var checkResult = db.Votingtrackers
                                .Where(b => b.MessageId == messageId && b.UserName == userToCheck)
                                .FirstOrDefault(); 

                if (checkResult != null)
                {
                    intCheckResult = checkResult.VoteStatus.Value;
                }
                else
                {
                    intCheckResult = 0;
                }

                return intCheckResult;
            }

        }


        //returns -1:downvoted, 1:upvoted, or 0:not voted
        public static int CheckIfVotedComment(string userToCheck, int commentId)
        {
            int intCheckResult = 0;

            using (whoaverseEntities db = new whoaverseEntities())
            {
                var checkResult = db.Commentvotingtrackers
                                .Where(b => b.CommentId == commentId && b.UserName == userToCheck)
                                .FirstOrDefault();

                if (checkResult != null)
                {
                    intCheckResult = checkResult.VoteStatus.Value;
                }
                else
                {
                    intCheckResult = 0;
                }

                return intCheckResult;
            }

        }   
    }
}