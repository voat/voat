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

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using Whoaverse.Models;

namespace Whoaverse.Utils
{
    public static class User
    {
        // check if user exists in database
        public static bool UserExists(string userName)
        {
            using (UserManager<ApplicationUser> tmpUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
            {
                var tmpuser = tmpUserManager.FindByName(userName);
                if (tmpuser != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        // return user registration date
        public static DateTime GetUserRegistrationDateTime(string userName)
        {
            using (UserManager<ApplicationUser> tmpUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
            {
                var tmpuser = tmpUserManager.FindByName(userName);
                if (tmpuser != null)
                {
                    return tmpuser.RegistrationDateTime;
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }

        // delete a user account and all history: comments, posts and votes
        public static bool DeleteUser(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                using (UserManager<ApplicationUser> tmpUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
                {
                    var tmpuser = tmpUserManager.FindByName(userName);
                    if (tmpuser != null)
                    {
                        //remove voting history for submisions
                        db.Votingtrackers.RemoveRange(db.Votingtrackers.Where(x => x.UserName == userName));

                        //remove voting history for comments
                        db.Commentvotingtrackers.RemoveRange(db.Commentvotingtrackers.Where(x => x.UserName == userName));

                        //remove all comments
                        var comments = db.Comments.Where(c => c.Name == userName);
                        foreach (Comment c in comments)
                        {
                            c.Name = "deleted";
                            c.CommentContent = "deleted by user";
                            db.SaveChangesAsync();
                        }

                        //remove all submissions
                        var submissions = db.Messages.Where(c => c.Name == userName);
                        foreach (Message s in submissions)
                        {
                            if (s.Type == 1)
                            {
                                s.Name = "deleted";
                                s.MessageContent = "deleted by user";
                                s.Title = "deleted by user";
                            }
                            else
                            {
                                s.Name = "deleted";
                                s.Linkdescription = "deleted by user";
                                s.MessageContent = "http://whoaverse.com";
                            }
                        }
                        db.SaveChangesAsync();

                        var result = tmpUserManager.DeleteAsync(tmpuser);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
        }

        // check if given user is the owner for a given subverse
        public static bool IsUserSubverseAdmin(string userName, string subverse)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var subverseOwner = db.SubverseAdmins.Where(n => n.SubverseName.Equals(subverse, StringComparison.OrdinalIgnoreCase) && n.Power == 1).FirstOrDefault();
                if (subverseOwner != null && subverseOwner.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                };
            }
        }

        // check if given user is moderator for a given subverse
        public static bool IsUserSubverseModerator(string userName, string subverse)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var subverseModerator = db.SubverseAdmins.Where(n => n.SubverseName.Equals(subverse, StringComparison.OrdinalIgnoreCase) && n.Username.Equals(userName, StringComparison.OrdinalIgnoreCase) && n.Power == 2).FirstOrDefault();
                if (subverseModerator != null && subverseModerator.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                };
            }
        }

        // check if given user is subscribed to a given subverse
        public static bool IsUserSubverseSubscriber(string userName, string subverse)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var subverseSubscriber = db.Subscriptions.Where(n => n.SubverseName.ToLower() == subverse.ToLower() && n.Username == userName).FirstOrDefault();
                if (subverseSubscriber != null)
                {
                    return true;
                }
                else
                {
                    return false;
                };
            }
        }

        // subscribe to a subverse
        public static void SubscribeToSubverse(string userName, string subverse)
        {
            if (!IsUserSubverseSubscriber(userName, subverse))
            {
                using (whoaverseEntities db = new whoaverseEntities())
                {
                    // add a new subscription
                    Subscription newSubscription = new Subscription();
                    newSubscription.Username = userName;
                    newSubscription.SubverseName = subverse;
                    db.Subscriptions.Add(newSubscription);

                    // record new subscription in subverse table subscribers field
                    Subverse tmpSubverse = db.Subverses.Find(subverse);

                    if (tmpSubverse != null)
                    {
                        tmpSubverse.subscribers++;
                    }

                    db.SaveChanges();
                }
            }
        }

        // unsubscribe from a subverse
        public static void UnSubscribeFromSubverse(string userName, string subverse)
        {
            if (IsUserSubverseSubscriber(userName, subverse))
            {
                using (whoaverseEntities db = new whoaverseEntities())
                {
                    var subscription = db.Subscriptions
                                .Where(b => b.Username == userName && b.SubverseName == subverse)
                                .FirstOrDefault();

                    if (subverse != null)
                    {
                        // remove subscription record
                        db.Subscriptions.Remove(subscription);

                        // record new unsubscription in subverse table subscribers field
                        Subverse tmpSubverse = db.Subverses.Find(subverse);

                        if (tmpSubverse != null)
                        {
                            tmpSubverse.subscribers--;
                        }

                        db.SaveChanges();
                    }

                }
            }
        }

        // return subscription count for a given user
        public static int SubscriptionCount(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                return db.Subscriptions
                                    .Where(r => r.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
                                    .Count();
            }
        }

        // return a list of user badges
        public static List<Userbadge> UserBadges(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                return db.Userbadges.Include("Badge")
                    .Where(r => r.Username.Equals(userName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        // check if given user has unread private messages, not including messages manually marked as unread
        public static bool UserHasNewMessages(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var privateMessages = db.Privatemessages
                        .Where(s => s.Recipient.Equals(userName, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(s => s.Timestamp)
                        .ThenBy(s => s.Sender)
                        .ToList();

                if (privateMessages.Count() > 0)
                {
                    var unreadPrivateMessages = privateMessages
                        .Where(s => s.Status == true && s.Markedasunread == false).ToList();

                    if (unreadPrivateMessages.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        // save a submission
        // TODO
    }
}