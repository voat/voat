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
using System.Web;
using Whoaverse.Models;

namespace Whoaverse.Utils
{
    public static class Karma
    {
        public static int LinkKarma(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                int likes = db.Messages.AsEnumerable()
                                    .Where(r => r.Name == userName && r.Type == 2)
                                    .Sum(r => (int)r.Likes);

                int dislikes = db.Messages.AsEnumerable()
                                    .Where(r => r.Name == userName && r.Type == 2)
                                    .Sum(r => (int)r.Dislikes);

                return likes - dislikes;
            }
        }

        public static int CommentKarma(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                int sumOfLikes = db.Comments.AsEnumerable()
                                    .Where(r => r.Name == userName)
                                    .Sum(r => (int)r.Likes);

                int sumOfdislikes = db.Comments.AsEnumerable()
                                    .Where(r => r.Name == userName)
                                    .Sum(r => (int)r.Dislikes);

                return sumOfLikes - sumOfdislikes;
            }
        }

    }

}

