﻿/*
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
using System.Linq;
using Whoaverse.Models;

namespace Whoaverse.Utils
{
    public static class Karma
    {
        // get link contribution points for a user
        public static int LinkKarma(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {

                try
                {
                    int likes = db.Messages
                                                .Where(r => r.Name.Equals(userName, StringComparison.OrdinalIgnoreCase))
                                                .Sum(r => (int)r.Likes);

                    int dislikes = db.Messages
                                        .Where(r => r.Name.Equals(userName, StringComparison.OrdinalIgnoreCase))
                                        .Sum(r => (int)r.Dislikes);

                    return likes - dislikes;
                }
                catch (Exception)
                {

                    return 0;
                }

            }
        }

        // get comment contribution points for a user
        public static int CommentKarma(string userName)
        {
            using (whoaverseEntities db = new whoaverseEntities())
            {
                try
                {
                    int sumOfLikes = db.Comments
                                               .Where(r => r.Name.Trim().Equals(userName, StringComparison.OrdinalIgnoreCase))
                                               .Sum(r => (int)r.Likes);

                    int sumOfdislikes = db.Comments
                                        .Where(r => r.Name.Trim().Equals(userName, StringComparison.OrdinalIgnoreCase))
                                        .Sum(r => (int)r.Dislikes);

                    return sumOfLikes - sumOfdislikes;
                }
                catch (Exception)
                {

                    return 0;
                }

            }
        }

    }

}

