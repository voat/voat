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
using System.Threading.Tasks;
using Voat.Data.Models;

namespace Voat.Utilities
{
    public static class MesssagingUtility
    {
        // a method to send a private message to a user, invoked by other methods
        public static bool SendPrivateMessage(string sender, string recipient, string subject, string body)
        {
            using (var db = new voatEntities())
            {
                try
                {
                    var privateMessage = new PrivateMessage
                    {
                        Sender = sender,
                        Recipient = recipient,
                        CreationDate = DateTime.Now,
                        Subject = subject,
                        Body = body,
                        IsUnread = true,
                        MarkedAsUnread = true
                    };

                    db.PrivateMessages.Add(privateMessage);
                    db.SaveChanges();

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        // a method to mark single or all private messages as read for a given user
        public static async Task<bool> MarkPrivateMessagesAsRead(bool? markAll, string userName, int? itemId)
        {
            using (var db = new voatEntities())
            {
                try
                {
                    // mark all items as read
                    if (markAll != null && (bool) markAll)
                    {
                        IQueryable<PrivateMessage> unreadPrivateMessages = db.PrivateMessages
                                                                            .Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase) && s.IsUnread)
                                                                            .OrderByDescending(s => s.CreationDate)
                                                                            .ThenBy(s => s.Sender);

                        if (!unreadPrivateMessages.Any()) return false;

                        foreach (var singleMessage in unreadPrivateMessages.ToList())
                        {
                            singleMessage.IsUnread = false;
                        }
                        await db.SaveChangesAsync();
                        return true;
                    }

                    // mark single item as read
                    if (itemId != null)
                    {
                        var privateMessageToMarkAsread = db.PrivateMessages.FirstOrDefault(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase) && s.IsUnread && s.ID == itemId);
                        if (privateMessageToMarkAsread == null) return false;

                        var item = db.PrivateMessages.Find(itemId);
                        item.IsUnread = false;
                        await db.SaveChangesAsync();
                        return true;
                    }
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }

        }
    }
}