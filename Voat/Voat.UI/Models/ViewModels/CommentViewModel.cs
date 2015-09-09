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
using System.Web;
using Voat.Data.Models;

namespace Voat.Models
{
    public class CommentViewModel
    {
        public CommentViewModel()
        {

            ChildComments = new HashSet<Comment>();            

        }


        public int Id { get; set; }
        public Nullable<int> Votes { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public Nullable<int> MessageId { get; set; }
        public string CommentContent { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public Nullable<int> ParentId { get; set; }

        public virtual ICollection<Comment> ChildComments { get; set; }
        public virtual Submission Submission { get; set; }
    }

    
}