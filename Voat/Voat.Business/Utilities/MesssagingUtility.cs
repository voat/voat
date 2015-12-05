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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voat.Data.Models;

namespace Voat.Utilities
{
    public static class MesssagingUtility
    {
        // a method to send a private message to a user, invoked by other methods
        public static bool SendPrivateMessage(string sender, string recipientList, string subject, string body)
        {
            if (Voat.Utilities.UserHelper.IsUserGloballyBanned(System.Web.HttpContext.Current.User.Identity.Name))
            {
                return false;
            }

            if (Voat.Utilities.Karma.CommentKarma(System.Web.HttpContext.Current.User.Identity.Name) < 10)
            {
                return false;
            }

            List<PrivateMessage> messages = new List<PrivateMessage>();
            MatchCollection col = Regex.Matches(recipientList, @"((?'prefix'@|u/|/u/|v/|/v/)?(?'recipient'[\w-.]+))", RegexOptions.IgnoreCase);
            if (col.Count <= 0)
            {
                return false;
            }

            //Have to filter distinct because of spamming. If you copy a user name 
            //1,000 times into the recipient list the previous 
            //logic would send that user 1,000 messages. These guys find everything.
            var filtered = (from x in col.Cast<Match>()
                            select new
                            {
                                recipient = x.Groups["recipient"].Value,
                                prefix = (x.Groups["prefix"].Value.ToLower().Contains("v") ? "v" : "") //stop users from sending multiple messages using diff prefixes @user, /u/user, and u/user 
                            }).Distinct();

            foreach (var m in filtered)
            {
                var recipient = m.recipient;
                var prefix = m.prefix;

                if (!String.IsNullOrEmpty(prefix) && prefix.ToLower().Contains("v"))
                {
                    //don't allow banned users to send to subverses
                    if (!UserHelper.IsUserBannedFromSubverse(System.Web.HttpContext.Current.User.Identity.Name, recipient))
                    {
                        //send to subverse mods
                        using (var db = new voatEntities())
                        {
                            //designed to limit abuse by taking the level 1 mod and the next four oldest
                            var mods = (from mod in db.SubverseModerators
                                        where mod.Subverse.Equals(recipient, StringComparison.OrdinalIgnoreCase) && mod.UserName != "system" && mod.UserName != "youcanclaimthissub"
                                        orderby mod.Power ascending, mod.CreationDate descending
                                        select mod).Take(5);

                            foreach (var moderator in mods)
                            {
                                messages.Add(new PrivateMessage
                                {
                                    Sender = sender,
                                    Recipient = moderator.UserName,
                                    CreationDate = DateTime.Now,
                                    Subject = String.Format("[v/{0}] {1}", recipient, subject),
                                    Body = body,
                                    IsUnread = true,
                                    MarkedAsUnread = false
                                });
                            }
                        }
                    }
                }
                else
                {
                    //ensure proper cased
                    recipient = UserHelper.OriginalUsername(recipient);

                    if (Voat.Utilities.UserHelper.UserExists(recipient))
                    {
                        messages.Add(new PrivateMessage
                        {
                            Sender = sender,
                            Recipient = recipient,
                            CreationDate = DateTime.Now,
                            Subject = subject,
                            Body = body,
                            IsUnread = true,
                            MarkedAsUnread = false
                        });
                    }
                }
            }

            if (messages.Count > 0)
            {
                using (var db = new voatEntities())
                {
                    try
                    {
                        db.PrivateMessages.AddRange(messages);
                        db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return true;
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