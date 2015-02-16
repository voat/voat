/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2014 Voat
All Rights Reserved.
*/

using System;
using Voat.Models;

namespace Voat.Utils
{
    public static class MesssagingUtility
    {
        // a method to send a private message to a user, invoked by other methods
        public static bool SendPrivateMessage(string sender, string recipient, string subject, string body)
        {
            using (var db = new whoaverseEntities())
            {
                try
                {
                    var privateMessage = new Privatemessage
                    {
                        Sender = sender,
                        Recipient = recipient,
                        Timestamp = DateTime.Now,
                        Subject = subject,
                        Body = body,
                        Status = true,
                        Markedasunread = true
                    };

                    db.Privatemessages.Add(privateMessage);
                    db.SaveChanges();

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}