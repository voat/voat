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
                            c.CommentContent = "deleted";
                            db.SaveChangesAsync();
                        }

                        //remove all submissions
                        var submissions = db.Messages.Where(c => c.Name == userName);
                        foreach (Message s in submissions)
                        {
                            if (s.Type == 1)
                            {
                                s.Name = "deleted";
                                s.MessageContent = "deleted";
                                s.Title = "deleted";
                            }
                            else
                            {
                                s.Name = "deleted";
                                s.Linkdescription = "deleted";
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

        // check if given user is moderator for a given subverse
        public static bool IsUserSubverseAdmin(string userName, string subverse)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                var subverseOwner = db.SubverseAdmins.Where(n => n.SubverseName == subverse && n.Power == 1).FirstOrDefault();
                if (subverseOwner != null && subverseOwner.Username == userName) {
                    return true;
                } else {
                    return false;
                };
            }
        }
    }
}