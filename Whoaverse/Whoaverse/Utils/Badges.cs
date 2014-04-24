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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Whoaverse.Utils
{
    public class Badges
    {

        //return domain from URI
        public static string getDomainFromUri(string completeUri)
        {
            Uri tmpUri = new Uri(completeUri);
            return tmpUri.Host;
        }


        #region old asp webforms code
        //public static string CreateUserProfileTag(string submissionAuthor)
        //{
        //    string authorUrl = "user/" + submissionAuthor;
        //    string authorBadge = "<a href=\"" + authorUrl + "\"" + " class=\"author may-blank id-t2_8bunp\">" + submissionAuthor + "</a>";
        //    return authorBadge;
        //}

        //create formatted message submission
        //public static string CreateFormattedMessageSubmission(int msgId, string messageContent, Nullable<System.DateTime> postingDateTime, string submissionAuthor, int numberOfComments)
        //{
        //    string lineBreak = "<br>";
        //    string ageBadge = "submitted " + Whoaverse.Utils.Submissions.CalcSubmissionAge(postingDateTime);
        //    string authorUrl = "User?Username=" + submissionAuthor;
        //    string authorBadge = " by <a href=\"" + authorUrl + "\">" + submissionAuthor + "</a>";
        //    string commentsUrl = "Comments?msgId=" + msgId;
        //    string commentsBadge = "<a href=\"" + commentsUrl + "\">" + numberOfComments + " comments</a>";
        //    string formattedValue = "<a href=\"" + commentsUrl + "\">" + messageContent + "</a>";
        //    return formattedValue + lineBreak + ageBadge + authorBadge + lineBreak + commentsBadge;
        //}
        #endregion

    }

    
}